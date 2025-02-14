using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Vulkan.VulkanVideoCodecH264stdEncode;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Voxels_Engine;

public class TextureArray
{
    private int texture;

    public TextureArray(string path, int collums, int rows)
    {
        texture = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2dArray, texture);

        Bitmap image = new Bitmap(path);

        int width = image.Width / collums;
        int height = image.Height / rows;
        
        GL.TexStorage3D(TextureTarget.Texture2dArray, 1, SizedInternalFormat.Rgba8, width, height, collums*width);
        
        Console.WriteLine(width);
        Console.WriteLine(height);
        
        for (int x = 0; x < collums; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Bitmap imagepart = image.Clone(new Rectangle(x * width, y * height, width, height), image.PixelFormat);

                int z = x + y * collums;

                BitmapData data = imagepart.LockBits(new Rectangle(0, 0, imagepart.Width, imagepart.Height), ImageLockMode.ReadOnly,
                    imagepart.PixelFormat);
                
                GL.TexSubImage3D(TextureTarget.Texture2dArray, 0, 0, 0, z, imagepart.Width, imagepart.Height, 1, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                
                imagepart.UnlockBits(data);
                
                imagepart.Dispose();
            }
        }
        
        GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    }

    public void use(string uniformName, Shader shader, TextureUnit unit, int uni)
    {
        shader.Use();
        
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2dArray, texture);

        GL.Uniform1i(shader.GetUniformId(uniformName), uni);
    }
}