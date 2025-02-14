using OpenTK.Mathematics;

namespace Voxels_Engine;

public class Builder
{
    public static float s = (float)Math.Sqrt(2);
    public static void BuildBlockFace(Vector3 position, Vector3 dir, ref List<Vector3> vertices,
        ref List<int> triangles, int sizex, int sizey, int sizez)
    {
        
        Vector3 axisA = new Vector3(dir.Y, dir.Z, dir.X);
        Vector3 axisB = Vector3.Cross(axisA, dir);

        axisA = axisA * axisA;
        axisB = axisB * axisB;
        
        int i = vertices.Count;

        int g = (int)Math.Min(Math.Min(dir.X, dir.Y), dir.Z);

        if (g == 0)
        {
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + 3);
            triangles.Add(i);
            triangles.Add(i + 3);
            triangles.Add(i + 2);
        }
        else
        {
            triangles.Add(i + 3);
            triangles.Add(i + 1);
            triangles.Add(i);
            triangles.Add(i + 2);
            triangles.Add(i + 3);
            triangles.Add(i);
        }

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Vector2 percent = new Vector2(x, y) - new Vector2(0.5f, 0.5f);
                
                Vector3 point = (percent.X * axisA + percent.Y * axisB + dir/2);
                //Console.WriteLine(point.X);
                point.X += 0.5f;
                point.X *= sizex;

                point.Z += 0.5f;
                point.Z *= sizez;

                point.Y += 0.5f;
                point.Y *= sizey;
                
                vertices.Add(point + position);
            }
        }
    }
}