using System;
using Avalonia.Input;

namespace ArcadePointsBot.Infrastructure.Interop;

public class Mouse
{
    private delegate void DownDelegate(MouseButton button);
    private delegate void UpDelegate(MouseButton button);
    private readonly DownDelegate _downDelegate;
    private readonly UpDelegate _upDelegate;
    public Mouse()
    {
        if (OperatingSystem.IsLinux()) { }
        else if (OperatingSystem.IsWindows())
        {
            _downDelegate = ArcadePointsBot.Interop.Windows.Mouse.Down;
            _upDelegate = ArcadePointsBot.Interop.Windows.Mouse.Up;
        }
        else throw new Exception("Unsupported OS");

    }
    public void Click(MouseButton mouseButton)
    {
        Down(mouseButton);
        Up(mouseButton);
    }
    public void DoubleClick(MouseButton mouseButton)
    {
        Click(mouseButton);
        Click(mouseButton);
    }
    public void Down(MouseButton mouseButton) => _downDelegate(mouseButton);

    public void Up(MouseButton mouseButton) => _upDelegate(mouseButton);
}
