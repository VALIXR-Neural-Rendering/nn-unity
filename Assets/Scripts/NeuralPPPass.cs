using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine.Experimental.Rendering;

// This render pass performs neural post-processing on the overlay
class NeuralPPPass : ScriptableRenderPass
{
    private Settings settings;
    private ProfilingSampler _profilingSampler;
    private Material ppMaterial;
    private RTHandle ovColor, ovDepth, rtOut, dcDepth;
    private RTHandle[] rtInp = new RTHandle[2]; //Currently supports max 2*4 = 8 channels
    private readonly string ovColID = "_OvColorTexture", ovDepthID = "_OvDepthTexture", outID = "_NeuralPPTexture", dcDepthID = "_DCDepthTexture";
    private NeuralEngine nnEngine;
    private int channels;
    private bool reInitNNEngine;

    public NeuralPPPass(Settings settings, string name)
    {
        this.settings = settings;
        ppMaterial = settings.blitMaterial;
        _profilingSampler = new ProfilingSampler(name);

        ovColor = RTHandles.Alloc(ovColID, name: ovColID);
        ovDepth = RTHandles.Alloc(ovDepthID, name: ovDepthID);
        rtOut = RTHandles.Alloc(Screen.width, Screen.height, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, name: outID);
        dcDepth = RTHandles.Alloc(Screen.width, Screen.height, colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, name: dcDepthID);

        rtInp[0] = RTHandles.Alloc(Screen.width, Screen.height, colorFormat: GraphicsFormat.R32G32B32A32_SFloat);
        if (settings.enableCustomBuffer)
        {
            rtInp[1] = RTHandles.Alloc(Screen.width, Screen.height, colorFormat: GraphicsFormat.R32G32B32A32_SFloat);
        }

        reInitNNEngine = settings.enableCustomBuffer;
        channels = (settings.enableCustomBuffer) ? 8 : 4;
        nnEngine = new NeuralEngine(
                            settings.estimationModel, 
                            Screen.width, Screen.height,
                            channels,
                            settings.enableCustomBuffer);
    } 
    
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        //ConfigureTarget(rtInp);     // For MRT rendering
        ConfigureTarget(rtOut);
        ConfigureClear(ClearFlag.All, new Color(0, 0, 0, 0));
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (reInitNNEngine != settings.enableCustomBuffer)
        {
            nnEngine.Dispose();
            channels = (settings.enableCustomBuffer) ? 8 : 4;
            nnEngine = new NeuralEngine(
                        settings.estimationModel,
                        Screen.width, Screen.height,
                        channels,
                        settings.enableCustomBuffer);
            reInitNNEngine = settings.enableCustomBuffer;
        }

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            // Empty command buffer run for correct scope title in the profiler
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            if (ppMaterial == null)
            {
                Debug.LogError("Material uninitialized.");
                return;
            }

            // Post-process overlay with shader
            Blitter.BlitTexture(cmd, ovColor, rtInp[0], ppMaterial, 0);
            if (this.settings.enableCustomBuffer)
            {
                Blitter.BlitTexture(cmd, ovColor, rtInp[1], ppMaterial, 1);
            }
            context.Submit();
            // Post-process overlay with neural network
            nnEngine.RunInference(rtInp, ref rtOut, ref dcDepth);

            cmd.SetGlobalTexture(outID, rtOut);
            cmd.SetGlobalTexture(dcDepthID, dcDepth);
        }
        // Execute Command Buffer one last time and release it
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }

    public void Dispose()
    {
        ovColor?.Release();
        ovDepth?.Release();
        rtOut?.Release();
        for (int i = 0; i < rtInp.Length; i++)
            rtInp[i]?.Release();
        nnEngine?.Dispose();
    }
}