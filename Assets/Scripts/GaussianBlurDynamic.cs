using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GaussianBlurDynamic : BlurCore
{
    protected int sumsResetID;
    protected int weightsCalculatorID;
    protected int weightsNormalizerID;

    private ComputeBuffer weightsBuffer;
    private ComputeBuffer weightsSum;

    public float GetSigma(float r)
    {    float clampedRadius = Mathf.Min(r,gpuMemoryBlockSizeBlur/2);
         return r/3.0f;
    }   

    protected override void ClearBuffers()
    {
        if(weightsBuffer!=null)
            weightsBuffer.Dispose();
        if(weightsSum!=null)
            weightsSum.Dispose();
    }

    protected override void Init()
    {
        base.Init();
        weightsCalculatorID = blurShader.FindKernel("WeightCaculatorCs");
        weightsNormalizerID = blurShader.FindKernel("WeightNormalizerCs");
        sumsResetID = blurShader.FindKernel("SumsDeleteCS");

        kernelsList.Add(weightsCalculatorID);
        kernelsList.Add(weightsNormalizerID);
        kernelsList.Add(sumsResetID);

        base.checkForKernels();

        weightsSum = new ComputeBuffer(1, sizeof(float));
        weightsBuffer = new ComputeBuffer(Mathf.Min((int)maxRadius * 2 + 1,gpuMemoryBlockSizeBlur/2), sizeof(float));

        blurShader.SetBuffer(blurHorID, "gWeights", weightsBuffer);
        blurShader.SetBuffer(blurVerID, "gWeights", weightsBuffer);
        blurShader.SetBuffer(weightsCalculatorID, "gWeights", weightsBuffer);
        blurShader.SetBuffer(weightsNormalizerID, "gWeights", weightsBuffer);
        blurShader.SetFloat("sigma", GetSigma(radius));

        blurShader.SetBuffer(sumsResetID,"weightsSum",weightsSum);
        blurShader.SetBuffer(weightsCalculatorID,"weightsSum",weightsSum);
        blurShader.SetBuffer(weightsNormalizerID,"weightsSum",weightsSum);

        init = true;
    }
    private void OnValidate()
    {
        if(!init)
            Init();        
    }

    protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
         float pingPongRadius = Mathf.PingPong(Time.time*40.0f, radius);
        if (pingPongRadius < 0.5f || blurShader == null)
        { 
             Graphics.Blit(source, destination); // just copy
            return;
        }  
        CheckForErrorsAndResolution();
      
        blurShader.SetInt("blurRadius", (int)(pingPongRadius));
        blurShader.SetFloat("sigma", GetSigma(pingPongRadius));

        blurShader.Dispatch(sumsResetID,1,1,1);
        blurShader.Dispatch(weightsCalculatorID,(int)pingPongRadius + 1,1,1);
        blurShader.Dispatch(weightsNormalizerID,(int)pingPongRadius * 2 + 1,1,1);
        /// use with this block if you want to debug weights
        /*
        float[] blurWeights = new float[Mathf.Min((int)maxRadius * 2 + 1,gpuMemoryBlockSizeBlur/2)];
        weightsBuffer.GetData(blurWeights);
        for (int t = 0; t < radius * 2 + 1; t++)
        {
            Debug.Log(blurWeights[t]);
        }

        uint[] weightsSumData = new uint[1];
        weightsSum.GetData(weightsSumData);
        Debug.Log("sum data: " + weightsSumData[0] / uintToFloatConvert);
        */
        DisptachWithSource(ref source, ref destination);
    }
}
