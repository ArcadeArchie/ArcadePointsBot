using ArcadePointsBot.DNS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Records;
/// <summary>
///   Alias for a name and all its subnames.
/// </summary>
/// <remarks>
///  Alias for a name and all its subnames, unlike <see cref="CNAMERecord"/>, which is an 
///  alias for only the exact name. Like a CNAME record, the DNS lookup will continue by 
///  retrying the lookup with the new name.
/// </remarks>
public class DNAMERecord : ResourceRecord
{
    /// <summary>
    ///   Creates a new instance of the <see cref="DNAMERecord"/> class.
    /// </summary>
    public DNAMERecord() : base()
    {
        Type = DnsType.DNAME;
    }

    /// <summary>
    ///  A domain-name which specifies the canonical or primary
    ///  name for the owner. The owner name is an alias.
    /// </summary>
    public DomainName? Target { get; set; }


    /// <inheritdoc />
    public override void ReadData(WireReader reader, int length)
    {
        Target = reader.ReadDomainName();
    }

    /// <inheritdoc />
    public override void ReadData(PresentationReader reader)
    {
        Target = reader.ReadDomainName();
    }

    /// <inheritdoc />
    public override void WriteData(WireWriter writer)
    {
        if (Target is null) throw new InvalidOperationException("Target cannot be null");
        writer.WriteDomainName(Target, uncompressed: true);
    }

    /// <inheritdoc />
    public override void WriteData(PresentationWriter writer)
    {
        if (Target is null) throw new InvalidOperationException("Target cannot be null");
        writer.WriteDomainName(Target, appendSpace: false);
    }

}
