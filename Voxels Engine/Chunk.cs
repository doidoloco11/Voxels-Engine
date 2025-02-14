using OpenTK.Mathematics;

namespace Voxels_Engine;

public class Chunk
{
    public static int CHUNKSIZE = 16;
    public static Vector3[] Directions = new[]
    {
        new Vector3(0, 1, 0), new Vector3(0, -1, 0),
        new Vector3(1, 0, 0), new Vector3(-1, 0, 0),
        new Vector3(0, 0, 1), new Vector3(0, 0, -1)
    };
    
    private Dictionary<Vector3, byte> voxels;
    private Commons.NoiseFunctions TerrainNoise;

    private ChunkMesh mesh;
    private Vector3 Position;

    public Chunk(Vector3 position,ref Commons.NoiseFunctions terrainNoise ,ref Dictionary<Vector3, byte> voxels)
    {
        this.voxels = voxels;
        TerrainNoise = terrainNoise;
        Position = position;
        mesh = new ChunkMesh();
    }

    public void render()
    {
        if (mesh.Ready)
        {
            mesh.render();
        }
        else
        {
            if (mesh.Triangles.Length != 0)
            {
                mesh.DefineBuffers();
            }
        }
    }

    public void GenerateVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<int> BlockIds = new List<int>();
        List<int> FaceIds = new List<int>();

        List<FaceData> data = new List<FaceData>();
        
        for (int x = 0; x < CHUNKSIZE; x++)
        {
            for (int y = 0; y < CHUNKSIZE; y++)
            {
                for (int z = 0; z < CHUNKSIZE; z++)
                {
                    Vector3 pos = new Vector3(x, y, z) + Position * CHUNKSIZE;
                    if (voxels[pos] != 0)
                    {
                        for (int i = 0; i < Directions.Length; i++)
                        {
                            Vector3 dir = Directions[i];
                            FaceData da = new FaceData(pos, dir);
                            if (voxels[pos + dir] == 0 && !data.Contains(da))
                            {
                                int sx = 1;
                                for (int ssx = x+1; ssx < CHUNKSIZE; ssx++)
                                {
                                    FaceData d = new FaceData(pos + new Vector3(sx, 0, 0), dir);
                                    if (voxels[pos + new Vector3(sx, 0, 0) + dir] == 0 && voxels[pos + new Vector3(sx, 0, 0)] == voxels[pos] && !data.Contains(d))
                                    {
                                        sx++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                int sz = 1;
                                for (int ssz = z+1; ssz < CHUNKSIZE; ssz++)
                                {
                                    bool haveSize = true;
                                    for (int ssx = 0; ssx < sx; ssx++)
                                    {
                                        FaceData d = new FaceData(pos + new Vector3(ssx, 0, sz), dir);
                                        if (voxels[pos + new Vector3(ssx, 0, sz) + dir] == 0 && voxels[pos + new Vector3(ssx, 0, sz)] == voxels[pos] && !data.Contains(d))
                                        {
                                        }
                                        else
                                        {
                                            haveSize = false;
                                            break;
                                        }
                                    }

                                    if (haveSize)
                                    {
                                        sz++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                
                                int sy = 1;
                                for (int ssy = y+1; ssy < CHUNKSIZE; ssy++)
                                {
                                    bool haveSize = true;
                                    for (int ssz = 0; ssz < sz; ssz++)
                                    {
                                        for (int ssx = 0; ssx < sx; ssx++)
                                        {
                                            FaceData d = new FaceData(pos + new Vector3(ssx, sy, ssz), dir);
                                            if (voxels[pos + new Vector3(ssx, sy, ssz) + dir] == 0 &&
                                                voxels[pos + new Vector3(ssx, sy, ssz)] == voxels[pos] && !data.Contains(d))
                                            {
                                            }
                                            else
                                            {
                                                haveSize = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (haveSize)
                                    {
                                        sy++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                for (int ssy = 0; ssy < sy; ssy++)
                                {
                                    for (int ssx = 0; ssx < sx; ssx++)
                                    {
                                        for (int ssz = 0; ssz < sz; ssz++)
                                        {
                                            FaceData d = new FaceData(pos + new Vector3(ssx, ssy, ssz), dir);
                                            if (!data.Contains(d))
                                            {
                                                data.Add(d);
                                            }
                                        }
                                    }
                                }

                                if (sx > 0 && sy > 0 && sz > 0)
                                {
                                    Builder.BuildBlockFace(pos, dir, ref vertices, ref triangles, sx, sy, sz);
                                    
                                    BlockIds.Add(voxels[pos]);
                                    BlockIds.Add(voxels[pos]);
                                    BlockIds.Add(voxels[pos]);
                                    BlockIds.Add(voxels[pos]);

                                    if (dir.Y == 1)
                                    {
                                        FaceIds.Add(0);
                                        FaceIds.Add(0);
                                        FaceIds.Add(0);
                                        FaceIds.Add(0);
                                    }else if (dir.Y == -1)
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
                                }
                            }
                        }
                    }
                }
            }
        }

        mesh.Vertices = vertices.ToArray();
        mesh.Triangles = triangles.ToArray();
        mesh.BlockIds = BlockIds.ToArray();
        mesh.FaceIds = FaceIds.ToArray();
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
                        if (g > -0.4f)
                        {
                            float h = TerrainNoise.LayeredNoise(pos.Xz, 8, 0.01f, 2, 0.5f) * 8 + CHUNKSIZE / 2f;
                            if (pos.Y < h)
                            {
                                if (pos.Y > h - 0.99f)
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

struct FaceData
{
    public Vector3 Position;
    public Vector3 Direction;

    public FaceData(Vector3 position, Vector3 direction)
    {
        Position = position;
        Direction = direction;
    }
}