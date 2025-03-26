using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Voxels_Engine;

public class PostProcessing
{
    private UIMesh _mesh;

    private int FrameBuffer;

    private int ScreenTexture;
    private int DepthTexture;

    private float[] Kernel;

    public PostProcessing()
    {
        _mesh = new UIMesh();

        FrameBuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer);
        
        ScreenTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, ScreenTexture);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 800, 600, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, ScreenTexture, 0);
        
        DepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, 800, 600, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, DepthTexture, 0);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        UpdateKernel(9);
    }

    public void UpdateKernel(int pointCount)
    {
        List<float> points = new List<float>();

        Random r = new Random();

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 point = new Vector2(r.NextSingle() * 2 - 1, r.NextSingle() * 2 - 1);
            
            points.Add(point.X);
            points.Add(point.Y);
        }

        Vector2 AveragePos = new Vector2(0);

        for (int i = 0; i < pointCount * 2; i += 2)
        {
            AveragePos.X += points[i];
            AveragePos.Y += points[i + 1];
        }

        AveragePos /= pointCount;

        for (int i = 0; i < pointCount * 2; i += 2)
        {
            Vector2 xy = new Vector2(points[i], points[i + 1]);
            xy -= AveragePos;
            xy = Vector2.ComponentMax(xy, -Vector2.One);
            xy = Vector2.ComponentMin(xy, Vector2.One);
            points[i] = xy.X;
            points[i + 1] = xy.Y;
        }

        Kernel = points.ToArray();

        for (int i = 0; i < pointCount * 2; i += 2)
        {
            Console.WriteLine($"{Kernel[i]}, {Kernel[i + 1]}");
        }
    }

    public void ActiveFrameBuffer()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer);
    }

    public void DisableFrameBuffer()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Render()
    {
        ShaderManager.PostProcessingShader.Use();
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, ScreenTexture);
        
        GL.Uniform1(ShaderManager.PostProcessingShader.GetUniformId("ScreenTex"), 0);
        
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
        
        
        GL.Uniform1(ShaderManager.PostProcessingShader.GetUniformId("DepthTex"), 1);
        
        GL.Uniform1(ShaderManager.PostProcessingShader.GetUniformId("KernelSize"), Kernel.Length / 2);
        GL.Uniform2(ShaderManager.PostProcessingShader.GetUniformId("Kernel"), Kernel.Length / 2, Kernel);
        
        _mesh.Render();
    }

    public void ResizeTextures(Vector2i Size)
    {
        GL.BindTexture(TextureTarget.Texture2D, ScreenTexture);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0);
        
        GL.BindTexture(TextureTarget.Texture2D, DepthTexture);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, Size.X, Size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, 0);
    }
}