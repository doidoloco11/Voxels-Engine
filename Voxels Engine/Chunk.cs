using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Voxels_Engine;

public class Chunk
{
    public static int CHUNKSIZE = 24;
    public static float HalfSize = CHUNKSIZE / 2f;
    public static float SphereRadius = float.Sqrt(HalfSize * HalfSize + HalfSize * HalfSize);
    
    public static Vector3[] Directions = new[]
    {
        new Vector3(0, 1, 0), new Vector3(0, -1, 0),
        new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
        new Vector3(0, 0, 1), new Vector3(0, 0, -1)
    };
    
    private Dictionary<Vector3, byte> voxels;
    private Commons.NoiseFunctions TerrainNoise;
    private Commons.NoiseFunctions HeightNoise;
    
    private Matrix4 m_model;
    public ChunkMesh mesh;
    
    public Vector3 Position;
    public Vector3 Center;
    public int Lod;

    public Chunk(Vector3 position, int lod, ref Commons.NoiseFunctions terrainNoise, ref Commons.NoiseFunctions heightNoise, ref Dictionary<Vector3, byte> voxels)
    {
        this.voxels = voxels;
        TerrainNoise = terrainNoise;
        HeightNoise = heightNoise;
        Position = position;
        Lod = int.Min(lod, (int)Math.Log(CHUNKSIZE, 2));

        Center = (Position + new Vector3(0.5f)) * CHUNKSIZE;
        
        mesh = new ChunkMesh();
        m_model = Matrix4.Identity;
        m_model *= Matrix4.CreateTranslation(position * CHUNKSIZE);
    }

    public void Render()
    {
        if (mesh.Ready && Window.window.camera.IsOnFrustum(this))
        {
            GL.UniformMatrix4(ShaderManager.ChunkShader.GetUniformId("m_model"), false, ref m_model);
            mesh.Render();
        }
        else if (!mesh.Ready)
        {
            if (mesh.Triangles.Length != 0)
            {
                mesh.DefineBuffers();
                GL.UniformMatrix4(ShaderManager.ChunkShader.GetUniformId("m_model"), false, ref m_model);
                mesh.Render();
            }
        }
    }

    public void GenerateVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<int> BlockIds = new List<int>();
        List<int> FaceIds = new List<int>();

        int b = (int)float.Pow(2, Lod);
        
        for (int x = 0; x < CHUNKSIZE; x+=b)
        {
            for (int y = 0; y < CHUNKSIZE; y+=b)
            {
                for (int z = 0; z < CHUNKSIZE; z+=b)
                {
                    Vector3 pos = new Vector3(x, y, z) + Position * CHUNKSIZE;
                    for (int i = 0; i < Directions.Length; i++)
                    {
                        Vector3 dir = Directions[i];
                        for (int nx = 0; nx < b; nx++)
                        {
                            for (int ny = 0; ny < b; ny++)
                            {
                                for (int nz = 0; nz < b; nz++)
                                {
                                    if (voxels[pos + new Vector3(nx, ny, nz)] != 0)
                                    {
                                        if (voxels[pos + new Vector3(nx, ny, nz) + dir] == 0)
                                        {
                                            Builder.BuildBlockFace(new Vector3(x, y, z), Lod, dir, ref vertices,
                                                ref triangles);

                                            BlockIds.Add(voxels[pos + new Vector3(nx, ny, nz)]);
                                            BlockIds.Add(voxels[pos + new Vector3(nx, ny, nz)]);
                                            BlockIds.Add(voxels[pos + new Vector3(nx, ny, nz)]);
                                            BlockIds.Add(voxels[pos + new Vector3(nx, ny, nz)]);

                                            if (dir.Y == 1)
                                            {
                                                FaceIds.Add(0);
                                                FaceIds.Add(0);
                                                FaceIds.Add(0);
                                                FaceIds.Add(0);
                                            }
                                            else if (dir.Y == -1)
                                            {
                                                FaceIds.Add(2);
                                                FaceIds.Add(2);
                                                FaceIds.Add(2);
                                                FaceIds.Add(2);
                                            }
                                            else
                                            {
                                                FaceIds.Add(1);
                                                FaceIds.Add(1);
                                                FaceIds.Add(1);
                                                FaceIds.Add(1);
                                            }

                                            goto terminou;
                                        }
                                    }
                                }
                            }
                        }

                        terminou: ;
                    }
                }
            }
        }
        
        mesh.Vertices = vertices.ToArray();
        mesh.BlockIds = BlockIds.ToArray();
        mesh.FaceIds = FaceIds.ToArray();
        mesh.Triangles = triangles.ToArray();
        mesh.Ready = false;
    }

    public void Dispose()
    {
        mesh.Dispose();
    }
    
    public void GenerateChunk()
    {
        for (int x = -1; x < CHUNKSIZE+1; x++)
        {
            for (int y = -1; y < CHUNKSIZE+1; y++)
            {
                for (int z = -1; z < CHUNKSIZE+1; z++)
                {
                    Vector3 pos = new Vector3(x, y, z) + Position * CHUNKSIZE;
                    if (!voxels.ContainsKey(pos))
                    {
                        float g = TerrainNoise.LayeredNoise(pos, 8, 0.1f, 2, 0.5f);
                        //Console.WriteLine(g);
                        if (g > -0.5f)
                        {
                            float b = float.Max(HeightNoise.LayeredNoise(pos.Xz, 4, 0.01f, 2, 0.5f) -0.7f, -1);
                            float hm = TerrainNoise.LayeredNoise(pos.Xz, 8, 0.01f, 2, 0.5f);
                            
                            //Console.WriteLine(h);
                            float hM = TerrainNoise.RidgeLayeredNoise(pos.Xz, 8, 0.01f, 2, 0.5f) / 7f;

                            float h = float.Min(float.Max(float.Lerp(hm, hM, (b / 2 + 0.5f)), -1f), 1f);
                            //Console.WriteLine(h);*/
                            h = h / 2 + 0.5f;
                            h = h * h * h;
                            h = h * 2 - 1;
                            h = h * ((b + 2) * 16) + CHUNKSIZE / 2f;
                            if (pos.Y < h)
                            {
                                if (pos.Y > h - 1f)
                                {
                                    voxels.Add(pos, 1);
                                }
                                else
                                {
                                    if (pos.Y < h - 5)
                                    {
                                        voxels.Add(pos, 3);
                                    }
                                    else
                                    {
                                        voxels.Add(pos, 2);
                                    }
                                }
                            }
                            else
                            {
                                voxels.Add(pos, 0);
                            }
                        }
                        else
                        {
                            voxels.Add(pos, 0);
                        }
                    }
                }
            }
        }
    }
}