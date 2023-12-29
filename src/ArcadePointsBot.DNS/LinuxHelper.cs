using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ArcadePointsBot.DNS;
internal static class LinuxHelper
{
    [SupportedOSPlatform("linux")]
    public static unsafe void ReuseAddresss(Socket socket)
    {
        int setval = 1;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            int rv = setsockopt(socket.Handle.ToInt32(), 1, 2, &setval, sizeof(int));
            if (rv != 0)
            {
                throw new Exception("Socet reuse addr failed.");
                //todo: throw new PlatformException();
            }
        }

        [DllImport("libc.so.6", SetLastError = true)]
        static extern int setsockopt(int socket, int level, int optname, void* optval, uint optlen);
    }
}
