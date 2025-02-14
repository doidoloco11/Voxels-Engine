using System.Collections;
using OpenTK.Graphics.Egl;
using OpenTK.Mathematics;

namespace Voxels_Engine;

public class World
{
    public static int RenderDist = 5;
    
    public Dictionary<Vector3, byte> voxels;
    public Commons.NoiseFunctions TerrainNoise;

    public Dictionary<Vector3,Chunk> chunks;
    public List<Vector3> CreateInChunks;

    private Thread chunkThread;

    public bool IsRunning = false;

    public World()
    {
        voxels = new Dictionary<Vector3, byte>();
        TerrainNoise = new Commons.NoiseFunctions();
        
        chunks = new Dictionary<Vector3, Chunk>();
        CreateInChunks = new List<Vector3>();

        IsRunning = true;

        chunkThread = new Thread(() => GenerateChunks(ref CreateInChunks, ref chunks));
        
        chunkThread.Start();
    }

    public void GenerateChunks(ref List<Vector3> CreateInChunks, ref Dictionary<Vector3, Chunk> chunks)
    {
        while (IsRunning)
        {
            if (CreateInChunks.Count > 0)
            {
                if (chunks.ContainsKey(CreateInChunks[0]))
                {
                    chunks[CreateInChunks[0]].GenerateChunk();
                    chunks[CreateInChunks[0]].GenerateVertices();
                }

                CreateInChunks.Remove(CreateInChunks[0]);
            }
        }
    }
    

    public void render()
    {
        foreach (var chunk in chunks)
        {
            chunk.Value.render();
        }
    }

    public void Remove()
    {
        IsRunning = false;
        foreach (var chunk in chunks)
        {
            chunks.Remove(chunk.Key);
        }
    }

    public void CreateChunks(Vector3 cameraPos)
    {
        List<Vector3> UnsortedList = new List<Vector3>();
        for (int x = -RenderDist; x <= RenderDist; x++)
        {
            for (int y = -RenderDist; y <= RenderDist; y++)
            {
                for (int z = -RenderDist; z <= RenderDist; z++)
                {
                    Vector3 c = new Vector3(x, y, z);
                    if (!chunks.ContainsKey(c))
                    {
                        Chunk chunk = new Chunk(c, ref TerrainNoise, ref voxels);
                        
                        chunks.Add(c, chunk);
                        UnsortedList.Add(c);
                    }
                }
            }
        }

        for (int i = 0; i < UnsortedList.Count-1; i++)
        {
            for (int j = 0; j < UnsortedList.Count-1; j++)
            {
                Vector3 v0 = cameraPos - (UnsortedList[j] * Chunk.CHUNKSIZE);

                Vector3 v1 = cameraPos - (UnsortedList[j + 1] * Chunk.CHUNKSIZE);


                if (v0.Length >= v1.Length)
                {
                    Vector3 v3 = UnsortedList[j];
                    UnsortedList[j] = UnsortedList[j + 1];
                    UnsortedList[j + 1] = v3;
                }
                
            }
        }
        
        CreateInChunks.AddRange(UnsortedList);
    }
}