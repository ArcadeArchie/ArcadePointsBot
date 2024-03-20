using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using ArcadePointsBot.Common.Primitives;
using Avalonia.Input;
using CliWrap;
using CliWrap.Buffered;
using static ArcadePointsBot.Infrastructure.Interop.Unix.XdoTool.KeyCodes;

namespace ArcadePointsBot.Infrastructure.Interop.Unix.XdoTool
{
    [SupportedOSPlatform("linux")]
    public static class XDoTool
    {
        private const string EXE = "xdotool";

        private static Task<BufferedCommandResult> Execute(params string[] args) =>
            Cli.Wrap(EXE).WithArguments(args).ExecuteBufferedAsync();

        public static async Task<Result> KeyDown(Key key)
        {
            var res = await Execute("keydown", CLI_CODES[key]);
            if (!res.IsSuccess)
                return Result.Failure(new Error(res.ExitCode.ToString(), res.StandardError));

            return Result.Success();
        }

        public static async Task<Result> KeyUp(Key key)
        {
            var res = await Execute("keyup", CLI_CODES[key]);
            if (!res.IsSuccess)
                return Result.Failure(new Error(res.ExitCode.ToString(), res.StandardError));

            return Result.Success();
        }

        public static async Task<Result> MouseDown(MouseButton button)
        {
            var res = await Execute("keyup", TransformButton(button));
            if (!res.IsSuccess)
                return Result.Failure(new Error(res.ExitCode.ToString(), res.StandardError));

            return Result.Success();
        }

        public static async Task<Result> MouseUp(MouseButton button)
        {
            var res = await Execute("keyup", TransformButton(button));
            if (!res.IsSuccess)
                return Result.Failure(new Error(res.ExitCode.ToString(), res.StandardError));

            return Result.Success();
        }

        private static string TransformButton(MouseButton key) =>
            key switch
            {
                MouseButton.Left => "1",
                MouseButton.Right => "3",
                MouseButton.Middle => "2",
                _ => "not a button",
            };
    }
}
