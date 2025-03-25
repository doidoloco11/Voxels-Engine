using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Voxels_Engine;

public class Texture
{
    public static int GenerateTextureArray(string path, int collums, int rows)
    {
        int texture = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2DArray, texture);

        Bitmap image = new Bitmap(path);

        int width = image.Width / collums;
        int height = image.Height / rows;
        
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, width, height, collums*width);
        
        
        
        for (int x = 0; x < collums; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Bitmap imagepart = image.Clone(new Rectangle(x * width, y * height, width, height), image.PixelFormat);

                int z = x + y * collums;

                BitmapData data = imagepart.LockBits(new Rectangle(0, 0, imagepart.Width, imagepart.Height), ImageLockMode.ReadOnly,
                    imagepart.PixelFormat);
                
                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, z, imagepart.Width, imagepart.Height, 1, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                
                imagepart.UnlockBits(data);
                
                imagepart.Dispose();
            }
        }
        
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        return texture;
    }
}