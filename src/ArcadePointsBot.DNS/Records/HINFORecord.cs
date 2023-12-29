﻿using ArcadePointsBot.DNS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Records;
/// <summary>
///   Host information. 
/// </summary>
/// <remarks>
///   Standard values for CPU and OS can be found in [RFC-1010].
///
///   HINFO records are used to acquire general information about a host. The
///   main use is for protocols such as FTP that can use special procedures
///   when talking between machines or operating systems of the same type.
/// </remarks>
public class HINFORecord : ResourceRecord
{
    /// <summary>
    ///  CPU type.
    /// </summary>
    public string? Cpu { get; set; }

    /// <summary>
    ///  Operating system type.
    /// </summary>
    public string? OS { get; set; }
        
    /// <summary>
    ///   Creates a new instance of the <see cref="HINFORecord"/> class.
    /// </summary>
    public HINFORecord() : base()
    {
        Type = DnsType.HINFO;
        TTL = DefaultHostTTL;
    }

    /// <inheritdoc />
    public override void ReadData(WireReader reader, int length)
    {
        Cpu = reader.ReadString();
        OS = reader.ReadString();
    }

    /// <inheritdoc />
    public override void ReadData(PresentationReader reader)
    {
        Cpu = reader.ReadString();
        OS = reader.ReadString();
    }

    /// <inheritdoc />
    public override void WriteData(WireWriter writer)
    {
        writer.WriteString(Cpu);
        writer.WriteString(OS);
    }

    /// <inheritdoc />
    public override void WriteData(PresentationWriter writer)
    {
        writer.WriteString(Cpu);
        writer.WriteString(OS, appendSpace: false);
    }

}
