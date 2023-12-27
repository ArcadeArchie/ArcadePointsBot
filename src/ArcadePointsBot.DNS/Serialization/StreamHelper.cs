﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Serialization;

/// <summary>
/// Provides Stream functionality to any buffer-based encoding operation.
/// </summary>
internal static class StreamHelper
{
    private const int defaultBufferSize = 4096;

    public static void Encode(
        Stream input,
        TextWriter output,
        Func<ReadOnlyMemory<byte>, bool, string> bufferEncodeFunc,
        int bufferSize = defaultBufferSize)
    {
        var buffer = new byte[bufferSize];
        while (true)
        {
            int bytesRead = input.Read(buffer, 0, bufferSize);
            if (bytesRead < 1)
            {
                break;
            }

            var result = bufferEncodeFunc(buffer.AsMemory(0, bytesRead), bytesRead < bufferSize);
            output.Write(result);
        }
    }

    public static async Task EncodeAsync(
        Stream input,
        TextWriter output,
        Func<ReadOnlyMemory<byte>, bool, string> bufferEncodeFunc,
        int bufferSize = defaultBufferSize)
    {
        var buffer = new byte[bufferSize];
        while (true)
        {
            int bytesRead = await input.ReadAsync(buffer.AsMemory(0, bufferSize)).ConfigureAwait(false);
            if (bytesRead < 1)
            {
                break;
            }

            var result = bufferEncodeFunc(buffer.AsMemory(0, bytesRead), bytesRead < bufferSize);
            await output.WriteAsync(result).ConfigureAwait(false);
        }
    }

    public static void Decode(
        TextReader input,
        Stream output,
        Func<ReadOnlyMemory<char>, Memory<byte>> decodeBufferFunc,
        int bufferSize = defaultBufferSize)
    {
        var buffer = new char[bufferSize];
        while (true)
        {
            int bytesRead = input.Read(buffer, 0, bufferSize);
            if (bytesRead < 1)
            {
                break;
            }

            var result = decodeBufferFunc(buffer.AsMemory(0, bytesRead));
            output.Write(result.ToArray(), 0, result.Length);
        }
    }

    public static async Task DecodeAsync(
        TextReader input,
        Stream output,
        Func<ReadOnlyMemory<char>, Memory<byte>> decodeBufferFunc,
        int bufferSize = defaultBufferSize)
    {
        var buffer = new char[bufferSize];
        while (true)
        {
            int bytesRead = await input.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);
            if (bytesRead < 1)
            {
                break;
            }

            var result = decodeBufferFunc(buffer.AsMemory(0, bytesRead));
            await output.WriteAsync(result).ConfigureAwait(false);
        }
    }
}
