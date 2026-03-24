using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;

namespace Siwa.Core.Helper;

public struct Shader
{
    public readonly uint ShaderProgram;
    
    public Shader(GL gl, string fragmentShaderPath, string vertexShaderPath)
    {
        uint vertexShader = CreateVertexShader(gl, vertexShaderPath);
        uint fragmentShader = CreateFragmentShader(gl, fragmentShaderPath);
        
        // create the shader program and attach shaders
        ShaderProgram = gl.CreateProgram();
        gl.AttachShader(ShaderProgram, vertexShader);
        gl.AttachShader(ShaderProgram, fragmentShader);
        
        // link program and delete the shaders
        gl.LinkProgram(ShaderProgram);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private uint CreateVertexShader(GL gl, string path)
    {
        // load shader source
        string shaderSource = File.ReadAllText(path);
        if(string.IsNullOrWhiteSpace(shaderSource)) throw new Exception("Shader Not Found");

        // create shader and compile the source
        var vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, shaderSource);
        gl.CompileShader(vertexShader);
        
        // validate shader compilation
        gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int result);
        if (result != (int)GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(vertexShader));
        
        Console.WriteLine("Vertex shader compiled successfully");
        return  vertexShader;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private uint CreateFragmentShader(GL gl, string path)
    {
        // load shader source
        string shaderSource = File.ReadAllText(path);
        if(string.IsNullOrWhiteSpace(shaderSource)) throw new Exception("Shader Not Found");
        
        // create shader and compile the source
        var fragmentShader =  gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, shaderSource);
        gl.CompileShader(fragmentShader);
        
        // validate shader compilation
        gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int result);
        if (result != (int)GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + gl.GetShaderInfoLog(fragmentShader));
        
        Console.WriteLine("Fragment shader compiled successfully");
        return fragmentShader;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Activate(GL gl) => gl.UseProgram(ShaderProgram);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Delete(GL gl) => gl.DeleteProgram(ShaderProgram);
}