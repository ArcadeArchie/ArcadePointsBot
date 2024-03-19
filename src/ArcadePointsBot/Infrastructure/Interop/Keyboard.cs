using System;
using Avalonia.Input;

namespace ArcadePointsBot.Infrastructure.Interop
{
    public class Keyboard
    {

    private delegate void DownDelegate(Key button);
    private delegate void UpDelegate(Key button);
    private readonly DownDelegate _downDelegate;
    private readonly UpDelegate _upDelegate;
    public Keyboard()
    {
        if (OperatingSystem.IsLinux()) { }
        else if (OperatingSystem.IsWindows())
        {
            _downDelegate = ArcadePointsBot.Interop.Windows.Keyboard.Press;
            _upDelegate = ArcadePointsBot.Interop.Windows.Keyboard.Release;
        }
        else throw new Exception("Unsupported OS");

    }
        public void Type(Key key)
        {
            Press(key);
            Release(key);
        }
        public void Press(Key key) => _downDelegate(key);
        public void Release(Key key) => _upDelegate(key);
    }
}