using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BlurCore : MonoBehaviour
{
#if UNITY_IOS || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    protected const int gpuMemoryBlockSizeBlur = 484;
    protected const int maxRadius = 64;
#elif UNITY_ANDROID
    protected const int gpuMemoryBlockSizeBlur = 64;
    protected const int maxRadius = 32;
#else
    protected const int gpuMemoryBlockSizeBlur = 1024;
    protected const int maxRadius = 92;
#endif

    protected private bool init = false;

    [Range(0.01f, 1.0f)]
    public float screenScaling = 1.0f;

    [Range(0.0f, maxRadius)]
    public float radius = 1;

    public ComputeShader blurShader;

    protected int texWidthVisibleSize, texHeightVisibleSize = 0;
    protected Camera thisCamera;

    protected RenderTexture verBlurOutput = null;
    protected RenderTexture horBlurOutput = null;
    protected RenderTexture tempSource = null;

    protected private int blurHorID;
    protected private int blurVerID;

    protected List<int> kernelsList = new List<int>();

    void ReportCompuetShaderError()
    {
        Debug.LogError("Error in compute shader, connect proper compute shader");
    }

    protected void CheckForErrorsAndResolution()
    {
        if (!blurShader)
        {
            init = false;
            ClearBuffers();
            ClearTextures();
        }

        texWidthVisibleSize = Mathf.RoundToInt(thisCamera.pixelWidth * screenScaling);
        texHeightVisibleSize = Mathf.RoundToInt(thisCamera.pixelHeight * screenScaling);
        checkForKernels();
        if (texWidthVisibleSize != thisCamera.pixelWidth || texHeightVisibleSize != thisCamera.pixelHeight)
        {
            warmUpTextures();
        }
    }

    public void ClearTexture(ref RenderTexture textureToClear)
    {
        if (null != textureToClear)
        {
            textureToClear.Release();
            textureToClear = null;
        }
    }

    protected virtual void ClearBuffers()
    {

    }

    protected void ClearTextures()
    {
        ClearTexture(ref verBlurOutput);
        ClearTexture(ref horBlurOutput);
        ClearTexture(ref tempSource);
    }

    public void CreateTextue(ref RenderTexture textureToMake)
    {
        textureToMake = new RenderTexture(texWidthVisibleSize, texHeightVisibleSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        textureToMake.enableRandomWrite = true;
        textureToMake.wrapMode = TextureWrapMode.Clamp;
        textureToMake.Create();
    }


    protected void warmUpTextures()
    {

        texWidthVisibleSize = Mathf.RoundToInt(thisCamera.pixelWidth * screenScaling);
        texHeightVisibleSize = Mathf.RoundToInt(thisCamera.pixelHeight * screenScaling);

        CreateTextue(ref verBlurOutput);
        CreateTextue(ref horBlurOutput);
        CreateTextue(ref tempSource);

        blurShader.SetTexture(blurHorID, "source", tempSource);
        blurShader.SetTexture(blurHorID, "horBlurOutput", horBlurOutput);

        blurShader.SetTexture(blurVerID, "horBlurOutput", horBlurOutput);
        blurShader.SetTexture(blurVerID, "verBlurOutput", verBlurOutput);
    }
    protected void SetRadius()
    {
        blurShader.SetInt("blurRadius", (int)radius);
    }

    protected virtual void Init()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError(" It seems your target Hardware does not support Compute Shaders.");
            return;
        }

        if (!blurShader)
        {
            Debug.LogError("No BlurShader");
            return;
        }

        blurHorID = blurShader.FindKernel("HorzBlurCs");
        blurVerID = blurShader.FindKernel("VertBlurCs");
        kernelsList.Add(blurHorID);
        kernelsList.Add(blurVerID);

        SetRadius();
        thisCamera = GetComponent<Camera>();

        if (!thisCamera)
        {
            Debug.LogError("Object has no Camera");
            return;
        }
        warmUpTextures();

    }

    protected void checkForKernels()
    {
        foreach (int kernel in kernelsList)
        {
            if (kernel < 0)
            {
                ReportCompuetShaderError();
            }
        }
    }



    private void OnEnable()
    {
        OnBlurEnable();
    }
    private void OnDisable()
    {
        OnBlurDisable();
    }

    private void OnDestroy()
    {
        OnBlurDestroy();
    }

    protected virtual void OnBlurEnable()
    {
        kernelsList = new List<int>();
        Init();
        warmUpTextures();
        init = true;
    }

    protected virtual void OnBlurDisable()
    {
        ClearTextures();
        ClearBuffers();
        init = false;
    }

    protected virtual void OnBlurDestroy()
    {
        ClearTextures();
        ClearBuffers();
        init = false;
    }

    protected void DisptachWithSource(ref RenderTexture source, ref RenderTexture destination)
    {
        if (!init)
            return;
        int horizontalBlurDisX = Mathf.CeilToInt(((float)texWidthVisibleSize / (float)gpuMemoryBlockSizeBlur)); // it is here becouse res of window can change
        int horizontalBlurDisY = Mathf.CeilToInt(((float)texHeightVisibleSize / (float)gpuMemoryBlockSizeBlur));

        Graphics.Blit(source, tempSource);
        blurShader.Dispatch(blurHorID, horizontalBlurDisX, texHeightVisibleSize, 1);
        blurShader.Dispatch(blurVerID, texWidthVisibleSize, horizontalBlurDisY, 1);

        Graphics.Blit(verBlurOutput, destination);
    }
    protected void DisptachWithSource(ref RenderTexture source, ref RenderTexture destination, Material postProcessMat)
    {
        if (!init)
            return;
        int horizontalBlurDisX = Mathf.CeilToInt(((float)texWidthVisibleSize / (float)gpuMemoryBlockSizeBlur)); // it is here becouse res of window can change
        int horizontalBlurDisY = Mathf.CeilToInt(((float)texHeightVisibleSize / (float)gpuMemoryBlockSizeBlur));

        Graphics.Blit(source, tempSource, postProcessMat);
        blurShader.Dispatch(blurHorID, horizontalBlurDisX, texHeightVisibleSize, 1);
        blurShader.Dispatch(blurVerID, texWidthVisibleSize, horizontalBlurDisY, 1);

        Graphics.Blit(verBlurOutput, destination, postProcessMat);
    }

}
