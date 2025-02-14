using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Voxels_Engine;

public class Camera
{
    public static float sensibility = 0.01f;
    public static float Speed = 5;
    
    public Vector3 Position;
    public Vector3 Angle;

    public Vector3 Forward;
    public Vector3 Right;
    public Vector3 Up;

    public CursorState cursorState;

    public Camera(Vector3 position)
    {
        Position = position;
        Angle = new Vector3(0, 0, 0);
        
        Forward = new Vector3(0, 0, 1);
        Right = new Vector3(1, 0, 0);
        Up = new Vector3(0, 1, 0);

        cursorState = CursorState.Normal;
    }

    Vector3 rotate(Vector3 vector)
    {
        vector = (new Vector4(vector, 1) * Matrix4.CreateRotationX(Angle.X)).Xyz;
        vector = (new Vector4(vector, 1) * Matrix4.CreateRotationY(Angle.Y)).Xyz;
        vector = (new Vector4(vector, 1) * Matrix4.CreateRotationZ(Angle.Z)).Xyz;

        return vector;
    }

    public void update()
    {

        Forward = rotate(new Vector3(0, 0, 1));
        Right = rotate(new Vector3(-1, 0, 0));
        Up = rotate(new Vector3(0, 1, 0));

        MouseState mouseState = Window.window.MouseState;
        KeyboardState keyboardState = Window.window.KeyboardState;

        float deltatime = (float)Window.window.UpdateTime;

        Vector2 delta = mouseState.Delta;

        Angle += cursorState == CursorState.Grabbed ? new Vector3(delta.Y, -delta.X, 0) * sensibility : Vector3.Zero;

        Window.window.CursorState = cursorState;

        if (keyboardState.IsKeyDown(Keys.W))
        {
            Position += Forward * Speed * deltatime;
        }
        
        if (keyboardState.IsKeyDown(Keys.S))
        {
            Position -= Forward * Speed * deltatime;
        }
        
        if (keyboardState.IsKeyDown(Keys.D))
        {
            Position += Right * Speed * deltatime;
        }
        
        if (keyboardState.IsKeyDown(Keys.A))
        {
            Position -= Right * Speed * deltatime;
        }
        
        if (keyboardState.IsKeyDown(Keys.E))
        {
            Position += Up * Speed * deltatime;
        }
        
        if (keyboardState.IsKeyDown(Keys.Q))
        {
            Position -= Up * Speed * deltatime;
        }

        if (keyboardState.IsKeyPressed(Keys.F1))
        {
            cursorState = cursorState == CursorState.Normal ? CursorState.Grabbed : CursorState.Normal;
        }

        if (keyboardState.IsKeyPressed(Keys.Escape)) Window.window.Close();
        
        ShaderManager.ChunkShader.Use();

        Matrix4 m_proj = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3,
            (float)Window.window.Size.X / Window.window.Size.Y, 0.1f, 1000);
        
        GL.UniformMatrix4f(ShaderManager.ChunkShader.GetUniformId("m_proj"), 1, false, ref m_proj);

        Matrix4 m_look = Matrix4.LookAt(Position, Position + Forward, Up);
        
        GL.UniformMatrix4f(ShaderManager.ChunkShader.GetUniformId("m_look"), 1, false, ref m_look);
    }
}