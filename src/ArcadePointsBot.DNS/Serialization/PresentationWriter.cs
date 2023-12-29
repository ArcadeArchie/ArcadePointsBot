using System;
using System.Globalization;
using System.IO;
using System.Net;
using ArcadePointsBot.DNS.Records;

namespace ArcadePointsBot.DNS.Serialization;

/// <summary>
///   Methods to write DNS data items encoded in the presentation (text) format.
/// </summary>
public class PresentationWriter
{
    //static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    readonly TextWriter _text;

    /// <summary>
    ///   Creates a new instance of the <see cref="PresentationWriter"/> using the
    ///   specified <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="text">
    ///   The source for data items.
    /// </param>
    public PresentationWriter(TextWriter text)
    {
        _text = text;
    }

    /// <summary>
    ///   Writes a space.
    /// </summary>
    public void WriteSpace()
    {
        _text.Write(' ');
    }

    /// <summary>
    ///   Writes a CRLF.
    /// </summary>
    public void WriteEndOfLine()
    {
        _text.Write("\r\n");
    }

    /// <summary>
    ///   Write an byte.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteByte(byte value, bool appendSpace = true)
    {
        _text.Write(value);
        if (appendSpace)
            WriteSpace();
    }

    /// <summary>
    ///   Write an unsigned short.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteUInt16(ushort value, bool appendSpace = true)
    {
        _text.Write(value);
        if (appendSpace)
            WriteSpace();
    }

    /// <summary>
    ///   Write an unsigned int.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteUInt32(uint value, bool appendSpace = true)
    {
        _text.Write(value);
        if (appendSpace)
            WriteSpace();
    }

    /// <summary>
    ///   Write a string.
    /// </summary>
    /// <param name="value">
    ///   An ASCII string.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    /// <remarks>
    ///   Quotes and escapes are added as needned.
    /// </remarks>
    public void WriteString(string? value, bool appendSpace = true)
    {
        bool needQuote = false;

        value ??= string.Empty;
        if (string.IsNullOrEmpty(value))
            needQuote = true;
        value = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        if (value.Contains(' '))
            needQuote = true;

        if (needQuote)
            _text.Write('"');
        _text.Write(value);
        if (needQuote)
            _text.Write('"');
        if (appendSpace)
            WriteSpace();
    }

    /// <summary>
    ///   Write a string.
    /// </summary>
    /// <param name="value">
    ///   An ASCII string.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    /// <remarks>
    ///   Quotes and escapes are NOT added.
    /// </remarks>
    public void WriteStringUnencoded(string? value, bool appendSpace = true)
    {
        _text.Write(value);
        if (appendSpace)
            WriteSpace();
    }

    /// <summary>
    ///   Write a domain name.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteDomainName(DomainName? value, bool appendSpace = true)
    {
        WriteStringUnencoded(value?.ToString(), appendSpace);
    }

    /// <summary>
    ///   Write bytes encoded in base-16.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteBase16String(byte[]? value, bool appendSpace = true)
    {
        WriteString(Base16.LowerCase.Encode(value), appendSpace);
    }

    /// <summary>
    ///   Write bytes encoded in base-64.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteBase64String(byte[]? value, bool appendSpace = true)
    {
        WriteString(value is null ? null : Convert.ToBase64String(value), appendSpace);
    }

    /// <summary>
    ///   Write a time span (interval) in 16-bit seconds.
    /// </summary>
    /// <param name="value">
    ///   The number of seconds to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteTimeSpan16(TimeSpan value, bool appendSpace = true)
    {
        WriteUInt16((ushort)value.TotalSeconds, appendSpace);
    }

    /// <summary>
    ///   Write a time span (interval) in 32-bit seconds.
    /// </summary>
    /// <param name="value">
    ///   The number of seconds to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteTimeSpan32(TimeSpan value, bool appendSpace = true)
    {
        WriteUInt32((uint)value.TotalSeconds, appendSpace);
    }

    /// <summary>
    ///   Write a date/time.
    /// </summary>
    /// <param name="value">
    ///   The UTC <see cref="DateTime"/>. Resolution is seconds.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteDateTime(DateTime value, bool appendSpace = true)
    {
        WriteString(value.ToUniversalTime().ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture), appendSpace);
    }

    /// <summary>
    ///   Write an Internet address.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    public void WriteIPAddress(IPAddress? value, bool appendSpace = true)
    {
        WriteString(value?.ToString(), appendSpace);
    }

    /// <summary>
    ///   Write a DNS Type.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    /// <remarks>
    ///   Either the name of a <see cref="DnsType"/> or
    ///   the string "TYPEx".
    /// </remarks>
    public void WriteDnsType(DnsType value, bool appendSpace = true)
    {
        if (!Enum.IsDefined(typeof(DnsType), value))
        {
            _text.Write("TYPE");
        }
        _text.Write(value);
        if (appendSpace)
            WriteSpace();
    }

    /// <summary>
    ///   Write a DNS Class.
    /// </summary>
    /// <param name="value">
    ///   The value to write.
    /// </param>
    /// <param name="appendSpace">
    ///   Write a space after the value.
    /// </param>
    /// <remarks>
    ///   Either the name of a <see cref="DnsClass"/> or
    ///   the string "CLASSx".
    /// </remarks>
    public void WriteDnsClass(DnsClass value, bool appendSpace = true)
    {
        if (!Enum.IsDefined(typeof(DnsClass), value))
        {
            _text.Write("CLASS");
        }
        _text.Write(value);
        if (appendSpace)
            WriteSpace();
    }

}

/// <summary>
/// A resource record or query type. 
/// </summary>
/// <seealso cref="Question.Type"/>
/// <seealso cref="ResourceRecord.Type"/>
public enum DnsType : ushort
{
    /// <summary>
    /// A host address.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035">RFC 1035</seealso>
    /// <seealso cref="ARecord"/>
    A = 1,

    /// <summary>
    /// An authoritative name server.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.11">RFC 1035</seealso>
    /// <seealso cref="NSRecord"/>
    NS = 2,

    /// <summary>
    /// A mail destination (OBSOLETE - use MX).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035">RFC 1035</seealso>
    [Obsolete("Use MX")]
    MD = 3,

    /// <summary>
    /// A mail forwarder (OBSOLETE - use MX).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035">RFC 1035</seealso>
    [Obsolete("Use MX")]
    MF = 4,

    /// <summary>
    /// The canonical name for an alias.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.1">RFC 1035</seealso>
    /// <seealso cref="CNAMERecord"/>
    CNAME = 5,

    /// <summary>
    /// Marks the start of a zone of authority.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.13">RFC 1035</seealso>
    /// <seealso cref="SOARecord"/>
    SOA = 6,

    /// <summary>
    /// A mailbox domain name (EXPERIMENTAL).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.3">RFC 1035</seealso>
    MB = 7,

    /// <summary>
    /// A mail group member (EXPERIMENTAL).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.6">RFC 1035</seealso>
    MG = 8,

    /// <summary>
    /// A mailbox rename domain name (EXPERIMENTAL).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.8">RFC 1035</seealso>
    MR = 9,

    /// <summary>
    /// A Null resource record (EXPERIMENTAL).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.8">RFC 1035</seealso>
    /// <seealso cref="NULLRecord"/>
    NULL = 10,

    /// <summary>
    /// A well known service description.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc3232">RFC 3232</seealso>
    WKS = 11,

    /// <summary>
    /// A domain name pointer.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.12">RFC 1035</seealso>
    /// <seealso cref="PTRRecord"/>
    PTR = 12,

    /// <summary>
    /// Host information.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.11">RFC 1035</seealso>
    /// <seealso href="https://tools.ietf.org/html/rfc1010">RFC 1010</seealso>
    /// <seealso cref="HINFORecord"/>
    HINFO = 13,

    /// <summary>
    /// Mailbox or mail list information.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.11">RFC 1035</seealso>
    MINFO = 14,

    /// <summary>
    /// Mail exchange.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3.9">RFC 1035</seealso>
    /// <seealso href="https://tools.ietf.org/html/rfc974">RFC 974</seealso>
    /// <seealso cref="MXRecord"/>
    MX = 15,

    /// <summary>
    /// Text resources.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035#section-3.3">RFC 1035</seealso>
    /// <seealso href="https://tools.ietf.org/html/rfc1464">RFC 1464</seealso>
    /// <seealso cref="TXTRecord"/>
    TXT = 16,

    /// <summary>
    /// Responsible Person.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1183">RFC 1183</seealso>
    /// <seealso cref="RPRecord"/>
    RP = 17,

    /// <summary>
    /// AFS Data Base location.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1183#section-1">RFC 1183</seealso>
    /// <seealso href="https://tools.ietf.org/html/rfc5864">RFC 5864</seealso>
    /// <seealso cref="AFSDBRecord"/>
    AFSDB = 18,

    /// <summary>
    /// An IPv6 host address.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc3596#section-2.2">RFC 3596</seealso>
    /// <seealso cref="AAAARecord"/>
    AAAA = 28,

    /// <summary>
    /// A resource record which specifies the location of the server(s) for a specific protocol and domain.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc2782">RFC 2782</seealso>
    /// <seealso cref="SRVRecord"/>
    SRV = 33,

    /// <summary>
    ///   Maps an entire domain name.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc6672">RFC 6672</seealso>
    /// <see cref="DNAMERecord"/>
    DNAME = 39,

    /// <summary>
    /// Option record.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc6891">RFC 6891</seealso>
    /// <see cref="OPTRecord"/>
    OPT = 41,

    /// <summary>
    ///   Delegation Signer.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc4034#section-5"/>
    /// <see cref="DSRecord"/>
    DS = 43,

    /// <summary>
    /// Signature for a RRSET with a particular name, class, and type.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc4034#section-3"/>
    /// <seealso cref="RRSIGRecord"/>
    RRSIG = 46,

    /// <summary>
    ///   Next secure owener.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc3845"/>
    /// <seealso cref="NSECRecord"/>
    NSEC = 47,

    /// <summary>
    ///   Public key cryptography to sign and authenticate resource records.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc4034#section-2.1"/>
    /// <seealso cref="DNSKEYRecord"/>
    DNSKEY = 48,

    /// <summary>
    ///   Authenticated next secure owner.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc5155"/>
    /// <seealso cref="NSEC3Record"/>
    NSEC3 = 50,

    /// <summary>
    ///   Parameters needed by authoritative servers to calculate hashed owner names.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc5155#section-4"/>
    /// <seealso cref="NSEC3PARAMRecord"/>
    NSEC3PARAM = 51,

    /// <summary>
    ///   Shared secret key.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc2930"/>
    /// <seealso cref="TKEYRecord"/>
    TKEY = 249,

    /// <summary>
    ///  Transactional Signature.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc2845"/>
    /// <seealso cref="TSIGRecord"/>
    TSIG = 250,

    /// <summary>
    /// A request for a transfer of an entire zone.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035">RFC 1035</seealso>
    AXFR = 252,

    /// <summary>
    ///  A request for mailbox-related records (MB, MG or MR).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035">RFC 1035</seealso>
    MAILB = 253,

    /// <summary>
    ///  A request for mail agent RRs (Obsolete - see MX).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035">RFC 1035</seealso>
    [Obsolete("Use MX")]
    MAILA = 254,

    /// <summary>
    ///  A request for any record(s).
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc1035">RFC 1035</seealso>
    ANY = 255,

    /// <summary>
    /// A Uniform Resource Identifier (URI) resource record.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc7553">RFC 7553</seealso>
    URI = 256,

    /// <summary>
    /// A certification authority authorization.
    /// </summary>
    /// <seealso href="https://tools.ietf.org/html/rfc6844">RFC 6844</seealso>
    CAA = 257,
}
