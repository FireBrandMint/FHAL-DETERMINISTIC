using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Raylib_cs;

namespace FHAL.GameLoops;
public class ScreenSystem : GameLoop
{
    public static ScreenSystem Singleton;

    ImageResolution WindowSize = new ImageResolution(800,400);

    ImageResolution WindowResolution = new ImageResolution(1280, 720);

    RenderTexture2D ScreenTexture;

    public ScalingType ScalingMode;

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
        Raylib.DrawRectangle(MousePos.x.ToInt(), MousePos.y.ToInt(), 7, 7, Color.GREEN);
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

        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        
        WindowSize.Width = sw;
        WindowSize.Height = sh;

        //scales the window according to scaling mode
        float scale = 1f;
        if(ScalingMode == ScalingType.BIGGEST_BORDER)
            scale = MathF.Max((float) WindowSize.Width / WindowResolution.Width, (float) WindowSize.Height / WindowResolution.Height);
        else if(ScalingMode == ScalingType.SMALLEST_BORDER)
            scale = MathF.Min((float) WindowSize.Width / WindowResolution.Width, (float) WindowSize.Height / WindowResolution.Height);
        else
            scale = MathF.Min((float) WindowSize.Width / WindowResolution.Width, (float) WindowSize.Height / WindowResolution.Height);

        float widthProportion = (float) WindowResolution.Width / WindowResolution.Height;
        float widthPropWin = (float) WindowSize.Width / WindowSize.Height;

        CacheMouseInfo(scale);

        Raylib.BeginTextureMode(ScreenTexture);
        Raylib.ClearBackground(Color.WHITE);

        Render();
        Raylib.EndTextureMode();

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.WHITE);
        Raylib.DrawTexturePro
        (
            ScreenTexture.texture,
            new Rectangle(0f, 0f, (float)ScreenTexture.texture.width, (float) -ScreenTexture.texture.height),
            new Rectangle((WindowSize.Width - ((float)WindowResolution.Width*scale))*0.5f, 
            (WindowSize.Height - ((float)WindowResolution.Height*scale))*0.5f,
            (float)WindowResolution.Width*scale, (float)WindowResolution.Height*scale),
            new Vector2(0f, 0f), 0f, Color.WHITE
        );

        Raylib.EndDrawing();
    }

    private void CacheMouseInfo(float scale)
    {
        var mouse = Raylib.GetMousePosition();

        Vector2 virtualMouse = new Vector2(
            (mouse.X - (WindowSize.Width - WindowResolution.Width * scale) * 0.5f) / scale,
            (mouse.Y - (WindowSize.Height - (WindowResolution.Height * scale)) * 0.5f) / scale
        );

        var final = Vector2.Clamp(virtualMouse, new Vector2(0f,0f), new Vector2(WindowResolution.Width, WindowResolution.Height));

        bool pressedLeft = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
        bool pressedRight = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT);

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

        ScalingMode = ScalingType.BIGGEST_BORDER;

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

    public enum ScalingType : int
    {
        BIGGEST_BORDER = 0,
        SMALLEST_BORDER = 1
    }
}