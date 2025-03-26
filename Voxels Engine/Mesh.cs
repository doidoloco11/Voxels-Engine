using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Voxels_Engine;

public class ChunkMesh
{
    public Vector3[] Vertices;
    public int[] Triangles;
    public int[] BlockIds;
    public int[] FaceIds;

    private PrimitiveType type = PrimitiveType.Triangles;

    private int Vao;
    private int ChunkBuffer;

    public bool Ready = false;

    public ChunkMesh()
    {
        ChunkBuffer = GL.GenBuffer();
        Vao = GL.GenVertexArray();
        

        Triangles = new int[0];
    }

    public void Dispose()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer,0);
    }

    uint[] packData()
    {
        Vector3[] nn = new Vector3[Triangles.Length];

        for (int i = 0; i < Triangles.Length; i+=3)
        {
            Vector3 v0 = Vertices[Triangles[i]];
            Vector3 v1 = Vertices[Triangles[i + 1]];
            Vector3 v2 = Vertices[Triangles[i + 2]];

            Vector3 edgeA = v1 - v0;
            Vector3 edgeB = v2 - v0;

            Vector3 normal = Vector3.Cross(edgeA, edgeB).Normalized();

            nn[i] = normal + new Vector3(1);
            nn[i + 1] = normal + new Vector3(1);
            nn[i + 2] = normal + new Vector3(1);
        }
        
        uint[] packedData = new uint[Triangles.Length];

        uint mask = 0b11111;
        uint mask2 = 0b11;
        uint mask3 = 0b11111111;
        uint mask4 = 0b111;
        
        //Console.WriteLine($"{mask}, {mask2}, {mask3}");

        for (int i = 0; i < Triangles.Length; i++)
        {
            uint data = 0;

            Vector3i v = (Vector3i)Vertices[Triangles[i]];
            Vector3i n = (Vector3i)nn[i];
            uint blockid = (uint)BlockIds[Triangles[i]];
            uint faceid = (uint)FaceIds[Triangles[i]];
            //Console.WriteLine($"V: {v}, N: {n}");
            
            data |= (uint)n.X << 0;
            data |= (uint)n.Y << 2;
            data |= (uint)n.Z << 4;
            data |= (uint)v.X << 6;
            data |= (uint)v.Y << 11;
            data |= (uint)v.Z << 16;
            data |= blockid << 21;
            data |= faceid << 29;
            
            //Console.WriteLine($"{(data>>29)&mask2} {faceid}, {(data>>21)&mask3} {blockid}");
            //Console.WriteLine($"pos: {v}, {(data >> 6)&mask}, {(data >> 11)&mask}, {(data >> 16)&mask}, norm: {n-new Vector3i(1)}, {(int)((data>>0)&mask2)-1}, {(int)((data>>2)&mask2)-1}, {(int)((data>>4)&mask2)-1}");
            //Console.WriteLine(data);

            packedData[i] = data;
        }

        return packedData;
    }

    public void DefineBuffers()
    {
        GL.BindVertexArray(Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, ChunkBuffer);
        uint[] packedData = packData();
        Console.WriteLine(sizeof(uint) * packedData.Length / 1024);
        GL.BufferData(BufferTarget.ArrayBuffer, packedData.Length * sizeof(uint), packedData, BufferUsageHint.StaticDraw);
        
        
        Ready = true;
    }

    public void Render()
    {
        if (Triangles.Length > 0)
        {
            if (Window.window.KeyboardState.IsKeyPressed(Keys.F2))
            {
                type = type == PrimitiveType.Triangles ? PrimitiveType.Lines : PrimitiveType.Triangles;
            }

            GL.BindVertexArray(Vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, ChunkBuffer);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribIPointer(0, 1, VertexAttribIntegerType.UnsignedInt, 0, 0);

            GL.DrawArrays(type, 0, Triangles.Length);
        }
    }
}

public class UIMesh
{
    private int Vao;
    private int VerticesBuffer;
    private int UvsBuffer;

    public UIMesh()
    {
        Vao = GL.GenVertexArray();
        VerticesBuffer = GL.GenBuffer();
        UvsBuffer = GL.GenBuffer();
        
        GL.BindVertexArray(Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VerticesBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, 4 * 8, new []
        {
            new Vector2(-1, -1),
            new Vector2(-1, 1),
            new Vector2(1, -1),
            new Vector2(1, 1)
        }, BufferUsageHint.StaticDraw);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, UvsBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, 4 * 8, new[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        }, BufferUsageHint.StaticDraw);
    }

    public void Render(Shader shader, Vector2 position, Vector2 size, float rotation = 0)
    {
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        
        Matrix4 m_model = Matrix4.Identity;
        m_model *= Matrix4.CreateScale(new Vector3(size));
        m_model *= Matrix4.CreateRotationZ(rotation);
        m_model *= Matrix4.CreateTranslation(new Vector3(position));
        
        GL.UniformMatrix4(shader.GetUniformId("m_model"), false, ref m_model);
        
        GL.BindVertexArray(Vao);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, VerticesBuffer);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, UvsBuffer);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
    }
    public void Render()
    {
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        
        GL.BindVertexArray(Vao);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, VerticesBuffer);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, UvsBuffer);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
    }
}