using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieMenuContoller : GaussianBlurDynamic
{
    public float mouseDistanceToEnable = 0.3f;
    public Button[] elements;
    public Material postProcessMaterial;
    public float timeToLerp = 5.0f;
    public float timeSlowScale = 0.001f;

    Vector2 upVector = new Vector2(0.0f, 1.0f);
    Vector2 mousePositionCapture;
    bool mousePressed = false;
    int actualIndex = 0;

    float lerp = 1.0f;
    float innerTimer = 0.0f;

    private void Start()
    {
        lerp = 0.0f;
        foreach(Button btn in elements)
                btn.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (innerTimer>0.0f)
        {
           lerp =  Mathf.SmoothStep(0.0f,1.0f, innerTimer/timeToLerp);
            Time.timeScale = Mathf.Lerp(1.0f, timeSlowScale,lerp);
            Debug.Log(lerp);
                
            postProcessMaterial.SetFloat("_effectStrength",lerp);
            if(!mousePressed)
            {
                innerTimer-=Time.deltaTime* 1.0f/Time.timeScale;
            }
        }
        if(innerTimer<0.0f)
        {
            lerp = 0.0f;
            postProcessMaterial.SetFloat("_effectStrength",lerp);
        }
           

        if (Input.GetKeyDown("space"))
        {
            foreach(Button btn in elements)
                btn.gameObject.SetActive(true);
            mousePressed = true;
            Cursor.lockState = CursorLockMode.None;
            innerTimer = 0.0f;
        }
        if (Input.GetKeyUp("space"))
        {
            foreach(Button btn in elements)
                btn.gameObject.SetActive(false);
            mousePressed = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameObject myEventSystem = GameObject.Find("EventSystem");
            myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        }
        if (mousePressed)
        {
            innerTimer = Mathf.Clamp(innerTimer + Time.deltaTime * 1.0f/Time.timeScale,0.0f, timeToLerp);

            mousePositionCapture = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (Vector2.Distance(mousePositionCapture, screenCenter) > mouseDistanceToEnable)
            {
                Vector2 mouseVector = mousePositionCapture - screenCenter;
                float angle = Mathf.Atan2(mouseVector.normalized.y, mouseVector.normalized.x) - Mathf.Atan2(upVector.y, upVector.x) - Mathf.PI / (elements.Length);
                if (angle < 0) { angle += 2 * Mathf.PI; }
                float normalizedRange = angle / (Mathf.PI * 2.0f);
                int actualIndex = Mathf.FloorToInt(normalizedRange * elements.Length);

                elements[actualIndex].Select();
            }
        }

 
           

    }
    protected override void OnBlurDestroy()
    {
        base.OnBlurDestroy();
        lerp = 1.0f;
        foreach(Button btn in elements)
        {
        if (btn)
             btn.gameObject.SetActive(true);
        }
               
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
     
        float _radius = radius * lerp;

        if (_radius < 0.5f || blurShader == null)
        {
            Graphics.Blit(source, destination,postProcessMaterial);
            return;
        }
            CheckForErrorsAndResolution();
        blurShader.SetInt("blurRadius", (int)(_radius));
        blurShader.SetFloat("sigma", GetSigma(_radius));

        blurShader.Dispatch(sumsResetID, 1, 1, 1);
        blurShader.Dispatch(weightsCalculatorID, (int)_radius + 1, 1, 1);
        blurShader.Dispatch(weightsNormalizerID, (int)_radius * 2 + 1, 1, 1);
        DisptachWithSource(ref source, ref destination, postProcessMaterial);
    }
}
