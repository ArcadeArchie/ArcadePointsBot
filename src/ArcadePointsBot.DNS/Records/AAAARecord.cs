using ArcadePointsBot.DNS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Records;

/// <summary>
///   Contains the IPv6 address of the named resource.
/// </summary>
public class AAAARecord : AddressRecord
{
    /// <summary>
    ///   Creates a new instance of the <see cref="AAAARecord"/> class.
    /// </summary>
    public AAAARecord() : base()
    {
        Type = DnsType.AAAA;
    }

}
