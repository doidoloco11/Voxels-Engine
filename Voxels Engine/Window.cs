

using System.ComponentModel;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Voxels_Engine;

public class Window : GameWindow
{
    public static Window window;
    private World world;
    private Camera camera;

    private TextureArray texture;
    public Window() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        window = this;
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        
        GL.DepthFunc(DepthFunction.Less);
        GL.CullFace(TriangleFace.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);

        world = new World();
        world.CreateChunks(Vector3.Zero);

        texture = new TextureArray("../../../textureimage.png", 3, 4);

        camera = new Camera(new Vector3(8, 9, 0));
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        Title = (1 / UpdateTime).ToString();
        
        camera.update();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        GL.ClearColor(0.3f, 0.3f, 0.8f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        
        texture.use("main_texture", ShaderManager.ChunkShader, TextureUnit.Texture0, 0);
        world.render();
        
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        
        world.Remove();
    }
}