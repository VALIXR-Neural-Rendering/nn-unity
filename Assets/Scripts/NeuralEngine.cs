using UnityEngine;
using Unity.Sentis;
using UnityEngine.Rendering;
using System;

public class NeuralEngine
{
    public Worker m_renderWorker, m_resizeWorker, m_stackWorker;
    readonly Model runtimeModel, resizeModel, stackModel;
    public Tensor<float> inputTensor, resizedTensor;
    public Tensor<float>[] tensorArray;

    int width, height;
    readonly int resizedWidth = 512, resizedHeight = 512;
    int channels;
    bool enableExtraBuffers = false;

    public NeuralEngine(
        ModelAsset modelAsset,
        int width, int height,
        int channels,
        bool enableExtraBuffers)
    {
        this.width = width;
        this.height = height;
        this.channels = channels;
        this.enableExtraBuffers = enableExtraBuffers;

        // Creating resize graph for resizing input to correct size
        resizeModel = CreateResizeModel();
        m_resizeWorker = new Worker(resizeModel, BackendType.GPUCompute);

        // Creating Rendering Network graph
        runtimeModel = CreateRenderingNetwork(modelAsset);
        m_renderWorker = new Worker(runtimeModel, BackendType.GPUCompute);

        inputTensor = new Tensor<float>(new TensorShape(1, channels, height, width));
        resizedTensor = new Tensor<float>(new TensorShape(1, channels, resizedWidth, resizedHeight));

        // Creating stacking graph in case of extra buffers
        if (enableExtraBuffers)
        {
            stackModel = CreateStackModel();
            m_stackWorker = new Worker(stackModel, BackendType.GPUCompute);

            tensorArray = new Tensor<float>[(channels + 3) / 4];
            for (int i = 0; i < tensorArray.Length; i++)
            {
                tensorArray[i] = new Tensor<float>(new TensorShape(1, 4, height, width));
            }
        }
    }

    private Model CreateResizeModel()
    {
        var resizeGraph = new FunctionalGraph();
        var baseInput = resizeGraph.AddInput<float>(new TensorShape(1, channels, height, width));
        FunctionalTensor resizedInput = Functional.Interpolate(baseInput, new[] { 512, 512 });
        //FunctionalTensor resizedInput = Functional.Interpolate(baseInput, new[] { 2000, 1328 + 1 }); // Needs 1 extra height for some reason

        return resizeGraph.Compile(resizedInput);
    }

    private Model CreateRenderingNetwork(ModelAsset modelAsset)
    {
        var model = ModelLoader.Load(modelAsset);
        var modelGraph = new FunctionalGraph();
        var inputs = modelGraph.AddInputs(model);

        // Normalize and negate the depth channel
        FunctionalTensor depth = Functional.IndexSelect(inputs[0], 1, Functional.Constant(new[] { 3 }));
        FunctionalTensor dmin = Functional.ReduceMin(Functional.ReduceMin(Functional.ReduceMin(depth, new[] { 3 }), new[] { 2 }), new[] { 1 });
        FunctionalTensor dmax = Functional.ReduceMax(Functional.ReduceMax(Functional.ReduceMax(depth, new[] { 3 }), new[] { 2 }), new[] { 1 });
        depth = 1.0f - (Functional.Div(depth - dmin, dmax - dmin + 1e-6f));
        inputs[0] = Functional.SliceScatter(inputs[0], depth, 1, 3, 4);

        // Downsampling to 5 levels by fact or of 0.5 starting with (512x512)
        int[] imgDims = new[] { 512, 512 };
        for (int i = 0; i < 4; i++)
            inputs[i + 1] = Functional.Interpolate(inputs[i], new[] { imgDims[0] / (1 << (i + 1)), imgDims[1] / (1 << (i + 1)) });

        FunctionalTensor[] nnOut = Functional.Forward(model, inputs);
        nnOut[0] = Functional.Interpolate(nnOut[0], new[] { width, height });
        FunctionalTensor rgbOut = Functional.IndexSelect(nnOut[0], 1, Functional.Constant(new[] { 0, 1, 2 }));
        FunctionalTensor depthOut = Functional.IndexSelect(nnOut[0], 1, Functional.Constant(new[] { 3 }));
        FunctionalTensor odmin = Functional.ReduceMin(Functional.ReduceMin(Functional.ReduceMin(depthOut, new[] { 3 }), new[] { 2 }), new[] { 1 });
        FunctionalTensor odmax = Functional.ReduceMax(Functional.ReduceMax(Functional.ReduceMax(depthOut, new[] { 3 }), new[] { 2 }), new[] { 1 });
        depthOut = Functional.Div(depthOut - odmin, odmax - odmin + 1e-6f);
        FunctionalTensor dmask = Functional.Round(depthOut);
        depthOut = depthOut * dmask;

        return modelGraph.Compile(rgbOut, depthOut);
    }

    private Model CreateStackModel()
    {
        var stackGraph = new FunctionalGraph();
        FunctionalTensor[] inputs = new FunctionalTensor[(channels + 3) / 4];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = stackGraph.AddInput<float>(new TensorShape(1, 4, height, width));
        }
        FunctionalTensor stackedInput = Functional.Concat(inputs, 1);

        return stackGraph.Compile(stackedInput);
    }


    public void RunInference(RTHandle[] rtInp, ref RTHandle rtOut, ref RTHandle dcDepth)
    {
        PopulateInputTensor(rtInp);
        m_resizeWorker.Schedule(inputTensor);
        resizedTensor = m_resizeWorker.PeekOutput() as Tensor<float>;
        m_renderWorker.Schedule(resizedTensor);
        var rgbOut = m_renderWorker.PeekOutput("output_0") as Tensor<float>;
        var depthOut = m_renderWorker.PeekOutput("output_1") as Tensor<float>;
        TextureConverter.RenderToTexture(rgbOut, rtOut);
        TextureConverter.RenderToTexture(depthOut, dcDepth);
    }

    private void PopulateInputTensor(RTHandle[] rtInp)
    {
        if (enableExtraBuffers)
        {
            for (int i = 0; i < rtInp.Length; i++)
            {
                TextureConverter.ToTensor(rtInp[i], tensorArray[i], new TextureTransform());
            }
            m_stackWorker.Schedule(tensorArray);
            inputTensor = m_stackWorker.PeekOutput() as Tensor<float>;
        }
        else
        {
            TextureConverter.ToTensor(rtInp[0], inputTensor, new TextureTransform());
        }
    }

    public void Dispose()
    {
        // Neural Network Cleanup
        inputTensor?.Dispose();
        resizedTensor?.Dispose();
        m_resizeWorker?.Dispose();
        m_renderWorker?.Dispose();
    }
}