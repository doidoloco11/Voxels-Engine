using System.Collections;
using System.Diagnostics;
using OpenTK.Graphics.Egl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Voxels_Engine;

public class World
{
    public static int RenderDist = 4;
    public static float LodSize = 3;
    
    public Dictionary<Vector3, byte> voxels;
    public Commons.NoiseFunctions TerrainNoise;
    public Commons.NoiseFunctions HeightNoise;

    public Dictionary<Vector3,Chunk> chunks;
    public List<Vector3> CreateInChunks;
    public List<Vector3> DeleteInChunks;

    private Task GenChunkThread;
    private Task DelChunksThread;

    public bool IsRunning = false;

    private int texture;
    private int normalstexture;

    public World()
    {
        Window.window.camera.OnRayCastBreak += RaycastBreak;
        
        voxels = new Dictionary<Vector3, byte>();
        TerrainNoise = new Commons.NoiseFunctions();
        HeightNoise = new Commons.NoiseFunctions();
        
        chunks = new Dictionary<Vector3, Chunk>();
        CreateInChunks = new List<Vector3>();
        DeleteInChunks = new List<Vector3>();

        IsRunning = true;

        texture = Texture.GenerateTextureArray("../../../textureimage.png", 3, 4);
        normalstexture = Texture.GenerateTextureArray("../../../normalmaps.png", 3, 4);

        GenChunkThread = new Task(() => GenerateChunks());
        DelChunksThread = new Task(() => DeleteChunks());
        
        GenChunkThread.Start();
        DelChunksThread.Start();
    }

    public void DeleteChunks()
    {
        while (IsRunning)
        {
            if (!IsRunning)
            {
                break;
            }
            if (DeleteInChunks.Count > 0)
            {
                lock (chunks)
                {
                    //Console.WriteLine(DeleteInChunks.Count);
                    for (int i = 0; i < DeleteInChunks.Count; i++)
                    {
                        chunks.Remove(DeleteInChunks[0]);
                        DeleteInChunks.Remove(DeleteInChunks[0]);
                    }
                }
            }
        }
    }

    public void GenerateChunks()
    {
        while (IsRunning)
        {
            if (!IsRunning)
            {
                break;
            }
            if (CreateInChunks.Count > 0)
            {
                lock (CreateInChunks)
                {
                    lock (chunks)
                    {
                        if (chunks.ContainsKey(CreateInChunks[0]) &&
                            !DeleteInChunks.Contains(CreateInChunks[0]))
                        {
                            Stopwatch t = new Stopwatch();
                            t.Start();
                            chunks[CreateInChunks[0]].GenerateChunk();
                            t.Stop();
                            Stopwatch tt = new Stopwatch();
                            tt.Start();
                            chunks[CreateInChunks[0]].GenerateVertices();
                            tt.Stop();
                            Console.WriteLine(
                                $"{t.ElapsedMilliseconds / 1000f}, {tt.ElapsedMilliseconds / 1000f}");
                        }
                    }
                    
                    CreateInChunks.Remove(CreateInChunks[0]);
                    
                }
            }
        }
    }
    

    public void Render()
    {
        
        ShaderManager.ChunkShader.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, texture);
            
        GL.Uniform1(ShaderManager.ChunkShader.GetUniformId("main_texture"), 0);
        
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2DArray, normalstexture);
            
        GL.Uniform1(ShaderManager.ChunkShader.GetUniformId("normalmap_texture"), 1);
        foreach (var chunk in chunks)
        {
            chunk.Value.Render();
        }
    }

    public void Dispose()
    {
        IsRunning = false;
        GenChunkThread.Wait();
        DelChunksThread.Wait();
        foreach (var chunk in chunks)
        {
            chunk.Value.Dispose();
            chunks.Remove(chunk.Key);
        }
    }

    public void CreateChunks(Vector3 cameraPos)
    {
        List<Vector3> UnsortedList = new List<Vector3>();
        List<Vector3> u = new List<Vector3>();
        for (int x = -RenderDist; x <= RenderDist; x++)
        {
            for (int y = -RenderDist; y <= RenderDist; y++)
            {
                for (int z = -RenderDist; z <= RenderDist; z++)
                {
                    Vector3 c = new Vector3(x + cameraPos.X, y + cameraPos.Y, z + cameraPos.Z);
                    if (!chunks.ContainsKey(c))
                    {
                        float l = (cameraPos - c).Length;
                        Chunk chunk = new Chunk(c, (int)(l/LodSize), ref TerrainNoise, ref HeightNoise, ref voxels);
                        //Console.WriteLine(chunks.Count + " " + CreateInChunks.Count + " " + DeleteInChunks.Count);
                        chunks.Add(c, chunk);
                        UnsortedList.Add(c);
                    }
                    u.Add(c);
                }
            }
        }

        var cc = chunks.Where(pair => (!u.Contains(pair.Key) && !DeleteInChunks.Contains(pair.Key)));
        
        foreach (var chunk in chunks)
        {
            float l = (cameraPos - chunk.Key).Length;
            int lod = (int)(l / LodSize);
            if (chunk.Value.Lod != lod)
            {
                chunk.Value.Lod = lod;
                UnsortedList.Add(chunk.Key);
            }
        }
        
        
        foreach (var ccinf in cc)
        {
            DeleteInChunks.Add(ccinf.Key);
            /*if (CreateInChunks.Contains(ccinf.Key))
            {
                CreateInChunks.Remove(ccinf.Key);
            }*/
        }
        
        UnsortedList.Sort((x, y) =>
            (cameraPos - (x * Chunk.CHUNKSIZE)).LengthFast.CompareTo((cameraPos - (y * Chunk.CHUNKSIZE))
                .LengthFast));
        new Task(() =>
        {
            lock (CreateInChunks)
            {
                CreateInChunks.AddRange(UnsortedList);
                CreateInChunks.Sort((x, y) => ((x - cameraPos).LengthFast + float.Abs(x.Y - cameraPos.Y)).CompareTo((y - cameraPos).LengthFast + float.Abs(x.Y - cameraPos.Y)));
                //Camera cam = Window.window.camera;
                //CreateInChunks.Sort((x, y) => ((int)(Vector3.Dot((x - cam.Position).Normalized(), -cam.Forward) * 3)).CompareTo((int)(Vector3.Dot((y - cam.Position).Normalized(), -cam.Forward) * 3)));
            }
        }).Start();

        //Console.WriteLine($"{CreateInChunks.Count}, {DeleteInChunks.Count}");
    }

    void RaycastBreak(Vector3 origin, Vector3 direction, float maxLength)
    {
        for (float i = 0; i < (float)maxLength + 1; i+=1/3f)
        {
            Vector3 pos = origin + direction * i;
            Vector3 rpos = Commons.Round(pos);

            if (voxels.ContainsKey(rpos))
            {
                if (voxels[rpos] != 0)
                {
                    if ((pos - origin).Length - 1 < maxLength)
                    {
                        voxels[rpos] = 0;
                        Vector3 cpos = Commons.Round(pos / Chunk.CHUNKSIZE);
                        new Task(() =>
                        {
                            for (int x = -1; x < 2; x++)
                            {
                                for (int y = -1; y < 2; y++)
                                {
                                    for (int z = -1; z < 2; z++)
                                    {
                                        try
                                        {
                                            chunks[cpos + new Vector3(x, y, z)].GenerateVertices();
                                        }
                                        catch (Exception e)
                                        {
                                        }
                                    }
                                }
                            }
                        }).Start();
                        break;
                    }
                }
            }
        }
    }
}