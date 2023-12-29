﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Extended.Crypto;
/// <summary>
///   Identifies a key exchange algorithm.
/// </summary>
public enum KeyExchangeMode : ushort
{
    /// <summary>
    ///   Server assignment.
    /// </summary>
    ServerAssignment = 1,

    /// <summary>
    ///  Diffie-Hellman exchange (DH).
    /// </summary>
    DiffieHellman = 2,

    /// <summary>
    ///  GSS-API negotiation.
    /// </summary>
    GssApi = 3,

    /// <summary>
    ///   Resolver assignment.
    /// </summary>
    ResolverAssignment = 4,

    /// <summary>
    ///   Key deletion.
    /// </summary>
    KeyDeletion = 5,
}