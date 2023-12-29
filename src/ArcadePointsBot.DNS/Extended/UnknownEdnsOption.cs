using ArcadePointsBot.DNS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Extended;

public class UnknownEdnsOption : EdnsOption
{
    /// <summary>
    ///   Specfic data for the option.
    /// </summary>
    public byte[]? Data { get; set; }

    /// <inheritdoc />
    public override void ReadData(WireReader reader, int length)
    {
        Data = reader.ReadBytes(length);
    }

    /// <inheritdoc />
    public override void WriteData(WireWriter writer)
    {
        writer.WriteBytes(Data);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $";   Type = {Type}; Data = {(Data is null ? "null" : Convert.ToBase64String(Data))}";
    }
}
