using OpenTK.Graphics.OpenGL;

namespace Voxels_Engine;

public class ShaderManager
{
    public static Shader ChunkShader = new Shader("../../../Shaders/Chunk");
    public static Shader PostProcessingShader = new Shader("../../../Shaders/PostProcessing");
}

public struct Shader
{
    private int id;
    private Dictionary<string, int> UniformIds;

    public Shader(string path)
    {
        UniformIds = new Dictionary<string, int>();
        GetProgram(path);
    }

    public void Use()
    {
        GL.UseProgram(id);
    }

    public int GetUniformId(string UniformName)
    {
        if (!UniformIds.ContainsKey(UniformName))
        {
            UniformIds.Add(UniformName, GL.GetUniformLocation(id, UniformName));
        }

        return UniformIds[UniformName];
    }

    private int GetShader(string path, ShaderType type)
    {
        int id = GL.CreateShader(type);
        GL.ShaderSource(id, File.ReadAllText(path));
        GL.CompileShader(id);

        GL.GetShaderInfoLog(id, out string info);
        if (!String.IsNullOrEmpty(info))
        {
            throw new Exception(info);
        }

        return id;
    }

    public void GetProgram(string path)
    {
        int program = GL.CreateProgram();

        int vertId = GetShader(path + ".vert", ShaderType.VertexShader);
        int fragId = GetShader(path + ".frag", ShaderType.FragmentShader);
        
        GL.AttachShader(program, vertId);
        GL.AttachShader(program, fragId);
        
        GL.LinkProgram(program);
        
        GL.DetachShader(program, vertId);
        GL.DetachShader(program, fragId);

        GL.GetProgramInfoLog(program, out string info);
        if (!String.IsNullOrEmpty(info))
        {
            throw new Exception(info);
        }
        
        GL.DeleteShader(vertId);
        GL.DeleteShader(fragId);

        id = program;
    }
}