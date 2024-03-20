using System;
using ArcadePointsBot.Infrastructure.Interop.Unix.XdoTool;
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
            if (OperatingSystem.IsLinux())
            {
#pragma warning disable CA1416 // Validate platform compatibility
                _downDelegate = async key =>
                {
                    var res = await XDoTool.KeyDown(key);
                    if (res.IsFailure)
                        throw new Exception(res.Error.Message);
                };
                _upDelegate = async key =>
                {
                    var res = await XDoTool.KeyUp(key);
                    if (res.IsFailure)
                        throw new Exception(res.Error.Message);
                };
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else if (OperatingSystem.IsWindows())
            {
                _downDelegate = ArcadePointsBot.Interop.Windows.Keyboard.Press;
                _upDelegate = ArcadePointsBot.Interop.Windows.Keyboard.Release;
            }
            else
                throw new Exception("Unsupported OS");
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
