using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Silk.NET.OpenGL;

namespace Siwa.Core.Assets.Kinds;

public class ShaderAsset : Asset
{
    [JsonInclude] public string VertexShaderPath;
    [JsonInclude] public string FragmentShaderPath;
    
    public override void OnRestore()
    {
        AssetPool<Components.Shader>.Registry.Restore(Handle.ToHandle<Components.Shader>());
    }

    public override void OnLoad()
    {
        var gl = AssetLoader.Instance.Gl;
        uint vertexShader = CreateVertexShader(gl, VertexShaderPath);
        uint fragmentShader = CreateFragmentShader(gl, FragmentShaderPath);
        
        // create the shader program and attach shaders
        var shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        
        // link program and delete the shaders
        gl.LinkProgram(shaderProgram);
        
        // register uniform index for camera data in the loaded shader
        uint blockIndex = gl.GetUniformBlockIndex(shaderProgram, "CameraData");
        if (blockIndex != uint.MaxValue)
        {
            gl.UniformBlockBinding(shaderProgram, blockIndex, 0);
        }
        
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        ref var shader = ref AssetPool<Components.Shader>.Registry.Get(Handle.ToHandle<Components.Shader>());
        shader.Handle = shaderProgram;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private uint CreateVertexShader(GL gl, string path)
    {
        // load shader source
        string shaderSource = File.ReadAllText(path);
        if(string.IsNullOrWhiteSpace(shaderSource)) throw new Exception($"Shader file at {path} not found");

        // create shader and compile the source
        var vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, shaderSource);
        gl.CompileShader(vertexShader);
        
        // validate shader compilation
        gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int result);
        if (result != (int)GLEnum.True)
            throw new Exception($"Vertex shader {path} failed to compile: {gl.GetShaderInfoLog(vertexShader)}");
        
        Console.WriteLine($"Vertex shader {path} compiled successfully");
        return  vertexShader;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private uint CreateFragmentShader(GL gl, string path)
    {
        // load shader source
        string shaderSource = File.ReadAllText(path);
        if(string.IsNullOrWhiteSpace(shaderSource)) throw new Exception($"Shader file at {path} not found");
        
        // create shader and compile the source
        var fragmentShader =  gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, shaderSource);
        gl.CompileShader(fragmentShader);
        
        // validate shader compilation
        gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int result);
        if (result != (int)GLEnum.True)
            throw new Exception($"Fragment shader {path} failed to compile: {gl.GetShaderInfoLog(fragmentShader)}");
        
        Console.WriteLine($"Fragment shader {path} compiled successfully");
        return fragmentShader;
    }
    
    protected override void OnUnload()
    {
        ref var shader = ref AssetPool<Components.Shader>.Registry.Get(Handle.ToHandle<Components.Shader>());
        AssetLoader.Instance.Gl.DeleteProgram(shader.Handle);
    }
}