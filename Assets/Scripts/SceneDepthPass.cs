using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq.Expressions;

// This render pass renders the depth buffer of the non-overlay layer to a RenderTexture
class SceneDepthPass : ScriptableRenderPass
{
    private Settings settings;
    private FilteringSettings filteringSettings;
    private ProfilingSampler _profilingSampler;
    private List<ShaderTagId> shaderTagsList = new List<ShaderTagId>();
    private readonly string depID = "_NonOvDepthTexture";
    private RTHandle rtDepth, depthTex;

    public SceneDepthPass(Settings settings, string name)
    {
        this.settings = settings;
        LayerMask allExceptLayer = (LayerMask) (0xFFFFFFFF ^ (1 << settings.layerMask));
        filteringSettings = new FilteringSettings(RenderQueueRange.all, ~(settings.layerMask));

        shaderTagsList.Add(new ShaderTagId("DepthOnly"));

        _profilingSampler = new ProfilingSampler(name);
        depthTex = RTHandles.Alloc("_CameraDepthTexture", name: "_CameraDepthTexture");
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;
        RenderingUtils.ReAllocateIfNeeded(ref rtDepth, colorCopyDescriptor, name: depID);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            cmd.ClearRenderTarget(true, true, Color.black);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, sortingCriteria);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            Blitter.BlitCameraTexture(cmd, source, rtDepth);
            cmd.SetGlobalTexture(depID, rtDepth);

            CoreUtils.SetRenderTarget(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }

    public void Dispose()
    {
        rtDepth?.Release();
    }
}