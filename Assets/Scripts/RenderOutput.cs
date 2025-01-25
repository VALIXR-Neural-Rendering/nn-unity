using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

// Script to save the rendered images to disk for debugging purposes
public class RenderOutput : MonoBehaviour
{
    // Start is called before the first frame update
    RenderTexture colorBuffer, depthBuffer;
    RenderTexture rpout;

    void Start()
    {
        colorBuffer = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        colorBuffer.Create();
        depthBuffer = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        depthBuffer.Create();

        RenderPipelineManager.endContextRendering += OnEndContextRendering;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        RenderPipelineManager.endContextRendering -= OnEndContextRendering; 
    }

    void OnEndContextRendering(ScriptableRenderContext context, List<Camera> list)
    {
        //rpout = (RenderTexture)Shader.GetGlobalTexture("_OvColorTexture");
        //Graphics.Blit(rpout, colorBuffer);
        //RenderTexture.active = colorBuffer;
        //SaveImage("/ov_color.png");

        ////rpout = (RenderTexture)Shader.GetGlobalTexture("_CameraDepthAttachment");     // For the unfiltered depth of the whole scene
        //rpout = (RenderTexture)Shader.GetGlobalTexture("_OvDepthTexture");
        //Graphics.Blit(rpout, depthBuffer);
        //RenderTexture.active = depthBuffer;
        //SaveImage("/ov_depth.png");

        //rpout = (RenderTexture)Shader.GetGlobalTexture("_NonOvColorTexture");
        //Graphics.Blit(rpout, colorBuffer);
        //RenderTexture.active = colorBuffer;
        //SaveImage("/color.png");

        //rpout = (RenderTexture)Shader.GetGlobalTexture("_NonOvDepthTexture");
        //Graphics.Blit(rpout, depthBuffer);
        //RenderTexture.active = depthBuffer;
        //SaveImage("/depth.png");

        //rpout = (RenderTexture)Shader.GetGlobalTexture("_NeuralPPTexture");
        //Graphics.Blit(rpout, colorBuffer);
        //RenderTexture.active = colorBuffer;
        //SaveImage("/out.png");

        //rpout = (RenderTexture)Shader.GetGlobalTexture("_OutputTexture");
        //Graphics.Blit(rpout, colorBuffer);
        //RenderTexture.active = colorBuffer;
        //SaveImage("/final.png");

        //rpout = (RenderTexture)Shader.GetGlobalTexture("_OvBufferTexture");
        //Graphics.Blit(rpout, colorBuffer);
        //RenderTexture.active = colorBuffer;
        //SaveImage("/vel.png");
    }

    private void SaveImage(String fpath)
    {
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        screenshot.ReadPixels(new Rect(0, 0, screenshot.width, screenshot.height), 0, 0);
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + fpath, bytes);
    }
}
