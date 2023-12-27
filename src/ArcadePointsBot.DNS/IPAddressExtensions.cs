﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS;

/// <summary>
///   Extension methods for <see cref="IPAddress"/>.
/// </summary>
/// <remarks>
///   Original code copied from <see href="https://github.com/MichaCo/DnsClient.NET/blob/dev/src/DnsClient/IpAddressExtensions.cs"/> 
/// </remarks>
internal static class IPAddressExtensions
{
    /// <summary>
    /// Translates a IPv4 or IPv6 address into an arpa address.
    /// Used for reverse DNS lookup to get the domain name of the given address.
    /// </summary>
    /// <param name="ip">The address to translate.</param>
    /// <returns>The arpa representation of the address.</returns>
    /// <seealso href="https://en.wikipedia.org/wiki/.arpa"/>
    public static string GetArpaName(this IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        Array.Reverse(bytes);

        // check IP6
        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // reveresed bytes need to be split into 4 bit parts and separated by '.'
            var newBytes = bytes
                .SelectMany(b => new[] { (b >> 0) & 0xf, (b >> 4) & 0xf })
                .Aggregate(new StringBuilder(), (s, b) => s.Append(b.ToString("x")).Append(".")) + "ip6.arpa";

            return newBytes;
        }
        else if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            // else IP4
            return string.Join(".", bytes) + ".in-addr.arpa";
        }

        // never happens anyways!?
        throw new ArgumentException($"Unsupported address family '{ip.AddressFamily}'.", nameof(ip));
    }
}
