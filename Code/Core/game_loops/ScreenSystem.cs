using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Raylib_cs;


public class ScreenSystem : GameLoop
{
    public static ScreenSystem Singleton;

    ImageResolution WindowSize = new ImageResolution(800,400);

    ImageResolution WindowResolution = new ImageResolution(1280, 720);

    RenderTexture2D ScreenTexture;

    public ImageResolution GetResolution() => WindowResolution;

    private Vector2Fi MousePos = new Vector2Fi();

    private bool MousePressedLeft = false;
    private bool MousePressedRight = false;


    //This function is called for drawing things on screen only.
    //The remaining logic to make this work is elsewhere.
    private void Render()
    {
        Raylib.DrawText("Hello, world!", 12, 12, 20, Color.BLACK);
        Raylib.DrawText("Mouse pos", MousePos.x.ToInt(), MousePos.y.ToInt(), 20, Color.BLACK);
    }

    protected override void OnDeltaUpdate()
    {
        if(Raylib.WindowShouldClose())
        {
            Program.RequestEndProgram();
            return;
        }

        if(Delta < 1.0) return;
        Delta = 0.0;
        
        WindowSize.Width = Raylib.GetScreenWidth();
        WindowSize.Height = Raylib.GetScreenHeight();

        float scale = MathF.Max(
            (float) WindowSize.Width / WindowResolution.Width,
        (float) WindowSize.Height / WindowResolution.Height
        );

        float widthProportion = (float) WindowResolution.Width / WindowResolution.Height;
        float widthPropWin = (float) WindowSize.Width / WindowSize.Height;

        CacheMouseInfo(scale);

        Raylib.BeginTextureMode(ScreenTexture);
        Raylib.ClearBackground(Color.WHITE);

        Render();
        Raylib.EndTextureMode();

        Raylib.BeginDrawing();
        Rectangle area;
        if(widthProportion > widthPropWin)
        {
            var real = ScreenTexture.texture.height * widthPropWin;
            area = new Rectangle(
                ScreenTexture.texture.width *0.5f - (real * 0.5f), 0f,
                real, (float)-ScreenTexture.texture.height);
        }
        else if(widthProportion < widthPropWin)
        {
            var real = ScreenTexture.texture.width * (1f / widthPropWin);
            area = new Rectangle(
                0f, ScreenTexture.texture.height *0.5f - (real * 0.5f),
                (float)-ScreenTexture.texture.width, real);
        }
        else
        {
            area = new Rectangle(0f, 0f, (float) ScreenTexture.texture.width, (float)-ScreenTexture.texture.height);
        }
        Rectangle screen = new Rectangle(
            0.0f,
            0.0f,
            WindowSize.Width,
            WindowSize.Height
        );
        Raylib.DrawTexturePro(ScreenTexture.texture, area, screen, new Vector2(0,0), 0.0f, Color.WHITE);

        Raylib.EndDrawing();
    }

    private void CacheMouseInfo(float scale)
    {
        var mouse = Raylib.GetMousePosition();
        bool pressedLeft = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
        bool pressedRight = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT);

        Vector2 virtualMouse = new Vector2(
            mouse.X * (1f / scale),
            mouse.Y * (1f / scale)
        );

        var final = Vector2.Clamp(virtualMouse, new Vector2(0f,0f), new Vector2(WindowResolution.Width, WindowResolution.Height));

        MousePos = new Vector2Fi((FInt)final.X, (FInt)final.Y);
        MousePressedLeft = pressedLeft;
        MousePressedRight = pressedRight;
    }


    protected override void OnInit()
    {
        Singleton = this;

        ExecutesEveryUpdate = true;
        
        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(WindowSize.Width, WindowSize.Height, "Hello World");

        ScreenTexture = Raylib.LoadRenderTexture(WindowResolution.Width, WindowResolution.Height);

        Raylib.SetTextureFilter(ScreenTexture.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
    }

    public override void OnEnd()
    {
        Raylib.UnloadRenderTexture(ScreenTexture);
        Raylib.CloseWindow();
    }

    public struct ImageResolution
    {
        public int Width, Height;

        public ImageResolution(int width, int height)
        {
            Width = width; Height = height;
        }
    }
}