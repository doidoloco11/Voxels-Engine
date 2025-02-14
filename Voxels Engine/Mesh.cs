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
    private int VerticesBuffer;
    private int NormalBuffer;
    private int BlockIdBuffer;
    private int FaceIdBuffer;

    public bool Ready = false;

    public ChunkMesh()
    {
        VerticesBuffer = GL.GenBuffer();
        Vao = GL.GenVertexArray();
        NormalBuffer = GL.GenBuffer();
        BlockIdBuffer = GL.GenBuffer();
        FaceIdBuffer = GL.GenBuffer();

        Triangles = new int[0];
    }

    Vector3[] OrganizeVertices()
    {
        Vector3[] v = new Vector3[Triangles.Length];

        for (int i = 0; i < Triangles.Length; i++)
        {
            v[i] = Vertices[Triangles[i]];
        }

        return v;
    }

    Vector3[] OrganizeNormals()
    {
        Vector3[] v = new Vector3[Triangles.Length];

        for (int i = 0; i < Triangles.Length; i+=3)
        {
            Vector3 v0 = Vertices[Triangles[i]];
            Vector3 v1 = Vertices[Triangles[i + 1]];
            Vector3 v2 = Vertices[Triangles[i + 2]];

            Vector3 edgeA = v1 - v0;
            Vector3 edgeB = v2 - v0;

            Vector3 normal = Vector3.Cross(edgeA, edgeB).Normalized();

            v[i] = normal;
            v[i + 1] = normal;
            v[i + 2] = normal;
        }

        return v;
    }

    float[] OrganizeBlockIds()
    {
        float[] v = new float[Triangles.Length];

        for (int i = 0; i < Triangles.Length; i++)
        {
            v[i] = BlockIds[Triangles[i]];
        }

        return v;
    }
    
    float[] OrganizeFaceIds()
    {
        float[] v = new float[Triangles.Length];

        for (int i = 0; i < Triangles.Length; i++)
        {
            v[i] = FaceIds[Triangles[i]];
        }

        return v;
    }

    public void DefineBuffers()
    {
        GL.BindVertexArray(Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VerticesBuffer);
        Vector3[] OVertices = OrganizeVertices();
        GL.BufferData(BufferTarget.ArrayBuffer, OVertices.Length * sizeof(float) * 3, OVertices, BufferUsage.DynamicDraw);
        Vector3[] ONormal = OrganizeNormals();
        GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, ONormal.Length * sizeof(float) * 3, ONormal, BufferUsage.DynamicDraw);
        
        float[] OBlockIds = OrganizeBlockIds();
        GL.BindBuffer(BufferTarget.ArrayBuffer, BlockIdBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, OBlockIds.Length * sizeof(float), OBlockIds, BufferUsage.DynamicDraw);
        float[] OFaceIds = OrganizeFaceIds();
        GL.BindBuffer(BufferTarget.ArrayBuffer, FaceIdBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, OFaceIds.Length * sizeof(float), OFaceIds, BufferUsage.DynamicDraw);

        Ready = true;
    }

    public void render()
    {

        if (Window.window.KeyboardState.IsKeyPressed(Keys.F2))
        {
            type = type == PrimitiveType.Triangles ? PrimitiveType.Lines : PrimitiveType.Triangles;
        }
        
        ShaderManager.ChunkShader.Use();
        
        GL.BindVertexArray(Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VerticesBuffer);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBuffer);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, BlockIdBuffer);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, FaceIdBuffer);
        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.DrawArrays(type, 0, Triangles.Length);
    }
}