using ArcadePointsBot.DNS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Records;
/// <summary>
///   Contains the IPv4 address of the named resource.
/// </summary>
public class ARecord : AddressRecord
{
    /// <summary>
    ///   Creates a new instance of the <see cref="ARecord"/> class.
    /// </summary>
    public ARecord() : base()
    {
        Type = DnsType.A;
    }

}