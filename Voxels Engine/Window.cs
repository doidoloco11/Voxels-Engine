

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
    private PostProcessing _postProcessing;
    
    public Camera camera;
    public Window(NativeWindowSettings nws) : base(GameWindowSettings.Default, nws)
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
        
        camera = new Camera(new Vector3(0, 0, 0));

        _postProcessing = new PostProcessing();
        
        world = new World();
        world.CreateChunks(Vector3.Zero);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        Title = (1 / UpdateTime).ToString();
        
        camera.update();
        
        world.CreateChunks(Commons.Round(camera.Position/Chunk.CHUNKSIZE));
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        _postProcessing.ActiveFrameBuffer();
        
        GL.ClearColor(0.3f, 0.3f, 0.8f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        
        world.Render();
        _postProcessing.DisableFrameBuffer();
        
        _postProcessing.Render();
        
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        
        GL.Viewport(0, 0, Size.X, Size.Y);
        _postProcessing.ResizeTextures(Size);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        
        world.Dispose();
    }
}