using System;
using ArcadePointsBot.Infrastructure.Interop.Unix.XdoTool;
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
        if (OperatingSystem.IsLinux())
        {
#pragma warning disable CA1416 // Validate platform compatibility
            _downDelegate = async key =>
            {
                var res = await XDoTool.MouseDown(key);
                if (res.IsFailure)
                    throw new Exception(res.Error.Message);
            };
            _upDelegate = async key =>
            {
                var res = await XDoTool.MouseUp(key);
                if (res.IsFailure)
                    throw new Exception(res.Error.Message);
            };
#pragma warning restore CA1416 // Validate platform compatibility
        }
        else if (OperatingSystem.IsWindows())
        {
            _downDelegate = ArcadePointsBot.Interop.Windows.Mouse.Down;
            _upDelegate = ArcadePointsBot.Interop.Windows.Mouse.Up;
        }
        else
            throw new Exception("Unsupported OS");
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
