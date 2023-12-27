﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Serialization;

/// <summary>
///   Wire format serialisation of a DNS object.
/// </summary>
public interface IWireSerialiser
{
    /// <summary>
    ///   Reads the DNS object that is encoded in the wire format.
    /// </summary>
    /// <param name="reader">
    ///   The source of the DNS object.
    /// </param>
    /// <returns>
    ///   The final DNS object.
    /// </returns>
    /// <remarks>
    ///   Reading a <see cref="ResourceRecord"/> will return a new instance that
    ///   is type specific unless the <see cref="ResourceRecord.GetDataLength">RDLENGTH</see>
    ///   is zero.
    /// </remarks>
    IWireSerialiser Read(WireReader reader);

    /// <summary>
    ///   Writes the DNS object encoded in the wire format.
    /// </summary>
    /// <param name="writer">
    ///   The destination of the DNS object.
    /// </param>
    void Write(WireWriter writer);
}
