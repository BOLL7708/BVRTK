namespace BVRTK.Components.Graphics;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

public class ApplicationWindow
{
    private readonly IWindow _window;
    
    public ApplicationWindow()
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "This should be an OpenGL Window",
            IsVisible = true
        };
        
        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        
        // TODO: Set window icon
    }

    public void Run()
    {
        _window.Run();
    }

    public void SetWindowVisible(bool visible)
    {
        _window.IsVisible = visible;
    }
    
    private static void OnLoad() { }

    private static void OnUpdate(double deltaTime) { }

    private static void OnRender(double deltaTime) { }
}