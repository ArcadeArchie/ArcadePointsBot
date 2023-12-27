﻿using ArcadePointsBot.DNS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS;

/// <summary>
///   A question about a domain name to resolve.
/// </summary>
public class Question : DnsObject
{
    /// <summary>
    ///    A domain name to query.
    /// </summary>
    public DomainName Name { get; set; }

    /// <summary>
    ///    A two octet code which specifies the type of the query.
    /// </summary>
    /// <value>
    ///    One of the <see cref="DnsType"/> values.
    /// </value>
    /// <remarks>
    ///    The values for this field include all codes valid for a
    ///    TYPE field, together with some more general codes which
    ///    can match more than one type of the resource record.
    /// </remarks>
    public DnsType Type { get; set; }

    /// <summary>
    ///   A two octet code that specifies the class of the query.
    /// </summary>
    /// <value>
    ///   Defaults to <see cref="DnsClass.IN"/>.
    /// </value>
    public DnsClass Class { get; set; } = DnsClass.IN;

    /// <inheritdoc />
    public override IWireSerialiser Read(WireReader reader)
    {
        Name = reader.ReadDomainName();
        Type = (DnsType)reader.ReadUInt16();
        Class = (DnsClass)reader.ReadUInt16();

        return this;
    }

    /// <inheritdoc />
    public override void Write(WireWriter writer)
    {
        writer.WriteDomainName(Name);
        writer.WriteUInt16((ushort)Type);
        writer.WriteUInt16((ushort)Class);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        using (var s = new StringWriter())
        {
            var writer = new PresentationWriter(s);
            writer.WriteDomainName(Name);
            writer.WriteDnsClass(Class);
            writer.WriteDnsType(Type, appendSpace: false);
            return s.ToString();
        }
    }
}
