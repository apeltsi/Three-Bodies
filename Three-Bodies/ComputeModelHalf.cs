using System.Numerics;
using System.Runtime.InteropServices;
using SolidCode.Atlas;
using SolidCode.Atlas.AssetManagement;
using SolidCode.Atlas.Compute;
using SolidCode.Atlas.Rendering;
using Three_core;
using Veldrid;

// This is a simple implementation of compute shaders in Atlas.
// Though Atlas directly doesn't have an API for compute shaders, it's possible to use Veldrid's API directly as Atlas does expose all the necessary objects.
// Furthermore Atlas AssetManager does support loading & compilation of compute shaders
// Shader is located in data/shaders/3bpf.compute.hlsl and loaded from the "main" assetpack


namespace ThreeBodies;

public class ComputeModelHalf
{
    private Pipeline? _pipeline;
    private ResourceSet _resourceSet;

    struct BodyData
    {
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;
        public int simcount = Program.CThreadCount;
        public int framecount = Program.FrameCount;

        public BodyData(SimulationState state)
        {
            a = state.Bodies[0].Position;
            b = state.Bodies[1].Position;
            c = state.Bodies[2].Position;
        }

        public BodyData()
        {
            
        }
    }
    
    public ComputeModelHalf()
    {
        ComputeShader? shader = AssetManager.GetAsset<ComputeShader>("3bpf");
        
        if (shader == null || shader.Shader == null)
        {
            Debug.Error(LogCategory.Framework, "Could not create ComputeModel: Shader is null.");
            return;
        }
        
        CreateResources(shader);
    }

    private DeviceBuffer rwBuffer;
    private DeviceBuffer readBuffer;
    private DeviceBuffer uniformBuffer;
    private DeviceBuffer randomBuffer;
    private uint size = Program.CThreadCount * 3 * Program.FrameCount * 2; // One for positional data on for velocity data

    public float[] GetRandomData()
    {
        Random r = new Random();
        float[] data = new float[10000];
        for (int i = 0; i < 10000; i++)
        {
            data[i] = (float)Program.NormalDistribution.Sample(r);
        }

        return data;
    }
    private void CreateResources(ComputeShader shader)
    {
        ResourceFactory factory = Renderer.GraphicsDevice.ResourceFactory;
        ResourceLayout layout = factory.CreateResourceLayout(new ResourceLayoutDescription(new[]
        {
            new ResourceLayoutElementDescription("PrimaryBuffer", ResourceKind.StructuredBufferReadWrite,
                ShaderStages.Compute),
            new ResourceLayoutElementDescription("RandomBuffer", ResourceKind.StructuredBufferReadOnly,
                ShaderStages.Compute),
            new ResourceLayoutElementDescription("UniformBuffer", ResourceKind.UniformBuffer,
                ShaderStages.Compute)
        }));
        rwBuffer =
            factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<Vector2>() * size,
                BufferUsage.StructuredBufferReadWrite, (uint)Marshal.SizeOf<Vector2>()));
        
        readBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<Vector2>() * size,
            BufferUsage.Staging));
        
        randomBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<float>() * 10000,
            BufferUsage.StructuredBufferReadOnly,4));
        uniformBuffer =
            factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<BodyData>(), BufferUsage.UniformBuffer));
        Renderer.GraphicsDevice.UpdateBuffer(rwBuffer, 0, new Vector2[size]);
        Renderer.GraphicsDevice.UpdateBuffer(readBuffer, 0, new Vector2[size]);
        Renderer.GraphicsDevice.UpdateBuffer(uniformBuffer, 0, new BodyData(new SimulationState(Program.CThreadCount * Program.CThreadGroups)));
        _resourceSet = factory.CreateResourceSet(new ResourceSetDescription
        {
            Layout = layout,
            BoundResources = new BindableResource[]
            {
                rwBuffer,
                randomBuffer,
                uniformBuffer
            }
        });
        
        var pipelineDesc = new ComputePipelineDescription
        {
            ComputeShader = shader.Shader!,
            ResourceLayouts = new ResourceLayout[]
            {
                layout
            },
            ThreadGroupSizeX = 128,
            ThreadGroupSizeY = 1,
            ThreadGroupSizeZ = 1,
            
        };
        
        _pipeline = factory.CreateComputePipeline(pipelineDesc);
    }

    public void Dispatch(uint xGroups, uint yGroups, uint zGroups)
    {
        GraphicsDevice gd = Renderer.GraphicsDevice!;
        gd.WaitForIdle();
        gd.UpdateBuffer(randomBuffer, 0, GetRandomData());
        Renderer.CommandList.Begin();
        Renderer.CommandList.SetPipeline(_pipeline!);
        Renderer.CommandList.SetComputeResourceSet(0, _resourceSet);
        Renderer.CommandList.Dispatch(xGroups, yGroups, zGroups);
        
        Renderer.CommandList.CopyBuffer(rwBuffer, 0, readBuffer, 0,(uint)Marshal.SizeOf<Vector2>() * size );

        Renderer.CommandList.End();
        gd.SubmitCommands(Renderer.CommandList);
        gd.WaitForIdle();
    }

    public Vector2[] GetBuffer()
    {
        MappedResourceView<Vector2> res = Renderer.GraphicsDevice!.Map<Vector2>(readBuffer, MapMode.Read);
        Vector2[] data = new Vector2[res.Count];
        for (int i = 0; i < res.Count; i++)
        {
            data[i] = res[i];
        }
        Renderer.GraphicsDevice.Unmap(res.MappedResource.Resource);
        //Renderer.GraphicsDevice.UpdateBuffer(rwBuffer, 0, new Vector2[size]);
        //Renderer.GraphicsDevice.UpdateBuffer(readBuffer, 0, new Vector2[size]);
        return data;
    }
    
    public void UpdateUniformBuffer(SimulationState state)
    {
        Renderer.GraphicsDevice.WaitForIdle();
        Renderer.GraphicsDevice.UpdateBuffer(uniformBuffer, 0, new BodyData(state));
    }
}