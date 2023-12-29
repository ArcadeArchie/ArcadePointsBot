using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace ArcadePointsBot.DNS.Multicast.Events;
public class NetworkInterfaceEventArgs : EventArgs
{
    /// <summary>
    ///   The sequece of detected network interfaces.
    /// </summary>
    /// <value>
    ///   A sequence of network interfaces.
    /// </value>
    public IEnumerable<NetworkInterface>? NetworkInterfaces { get; set; }
}
