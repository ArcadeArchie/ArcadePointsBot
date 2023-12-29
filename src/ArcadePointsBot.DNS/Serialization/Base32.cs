﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.DNS.Serialization;
/// <summary>
/// Base32 encoding/decoding functions.
/// </summary>
public sealed class Base32
{
    private const int bitsPerByte = 8;
    private const int bitsPerChar = 5;

    private static readonly Lazy<Base32> crockford = new(() => new Base32(Base32Alphabet.Crockford));
    private static readonly Lazy<Base32> rfc4648 = new(() => new Base32(Base32Alphabet.Rfc4648));
    private static readonly Lazy<Base32> extendedHex = new(() => new Base32(Base32Alphabet.ExtendedHex));
    private static readonly Lazy<Base32> zBase32 = new(() => new Base32(Base32Alphabet.ZBase32));
    private static readonly Lazy<Base32> geohash = new(() => new Base32(Base32Alphabet.Geohash));
    private static readonly Lazy<Base32> bech32 = new(() => new Base32(Base32Alphabet.Bech32));
    private static readonly Lazy<Base32> filecoin = new(() => new Base32(Base32Alphabet.FileCoin));

    /// <summary>
    /// Initializes a new instance of the <see cref="Base32"/> class with a
    /// custom alphabet.
    /// </summary>
    /// <param name="alphabet">Alphabet to use.</param>
    public Base32(Base32Alphabet alphabet)
    {
        if (alphabet.PaddingPosition != PaddingPosition.End)
        {
            throw new ArgumentException(
                "Only alphabets with paddings at the end are supported by this implementation",
                nameof(alphabet));
        }

        Alphabet = alphabet;
    }

    private enum DecodeResult
    {
        Success = 0,
        InvalidInput,
        OutputOverflow,
    }

    /// <summary>
    /// Gets Douglas Crockford's Base32 flavor with substitution characters.
    /// </summary>
    public static Base32 Crockford => crockford.Value;

    /// <summary>
    /// Gets RFC 4648 variant of Base32 coder.
    /// </summary>
    public static Base32 Rfc4648 => rfc4648.Value;

    /// <summary>
    /// Gets Extended Hex variant of Base32 coder.
    /// </summary>
    /// <remarks>Also from RFC 4648.</remarks>
    public static Base32 ExtendedHex => extendedHex.Value;

    /// <summary>
    /// Gets z-base-32 variant of Base32 coder.
    /// </summary>
    /// <remarks>This variant is used in Mnet, ZRTP and Tahoe-LAFS.</remarks>
    public static Base32 ZBase32 => zBase32.Value;

    /// <summary>
    /// Gets Geohash variant of Base32 coder.
    /// </summary>
    public static Base32 Geohash => geohash.Value;

    /// <summary>
    /// Gets Bech32 variant of Base32 coder.
    /// </summary>
    public static Base32 Bech32 => bech32.Value;

    /// <summary>
    /// Gets FileCoin variant of Base32 coder.
    /// </summary>
    public static Base32 FileCoin => filecoin.Value;

    /// <summary>
    /// Gets the encoding alphabet.
    /// </summary>
    public Base32Alphabet Alphabet { get; }

    /// <inheritdoc/>
    public int GetSafeByteCountForDecoding(ReadOnlySpan<char> text)
    {
        return getAllocationByteCountForDecoding(text.Length - getPaddingCharCount(text));
    }

    /// <inheritdoc/>
    public int GetSafeCharCountForEncoding(ReadOnlySpan<byte> buffer)
    {
        return (((buffer.Length - 1) / bitsPerChar) + 1) * bitsPerByte;
    }

    /// <inheritdoc/>
    public string Encode(long number)
    {
        return number >= 0
            ? Encode((ulong)number)
            : throw new ArgumentOutOfRangeException(nameof(number), "Number is negative");
    }

    /// <inheritdoc/>
    public string Encode(ulong number)
    {
        var buffer = BitConverter.GetBytes(number);
        bool little = BitConverter.IsLittleEndian;
        int numBytes = sizeof(ulong);
        int index = little ? 0 : (numBytes - 1);
        int increment = little ? -1 : 1;
        while (buffer[index] == 0)
        {
            index += increment;
        }

        var span = buffer.AsSpan();
        return Encode(little ? span[..index] : span[index..]);
    }

    /// <inheritdoc/>
    public ulong DecodeUInt64(string text)
    {
        var buffer = Decode(text);
        return buffer.Length <= sizeof(ulong)
            ? BitConverter.ToUInt64(buffer)
            : throw new InvalidOperationException("Decoded text is too long to fit in a buffer");
    }

    /// <inheritdoc/>
    public long DecodeInt64(string text)
    {
        return (long)DecodeUInt64(text);
    }

    /// <summary>
    /// Encode a memory span into a Base32 string without padding.
    /// </summary>
    /// <param name="bytes">Buffer to be encoded.</param>
    /// <returns>Encoded string.</returns>
    public string Encode(ReadOnlySpan<byte> bytes)
    {
        return Encode(bytes, padding: false);
    }

    /// <summary>
    /// Encode a memory span into a Base32 string.
    /// </summary>
    /// <param name="bytes">Buffer to be encoded.</param>
    /// <param name="padding">Append padding characters in the output.</param>
    /// <returns>Encoded string.</returns>
    public string Encode(ReadOnlySpan<byte> bytes, bool padding)
    {
        int bytesLen = bytes.Length;
        if (bytesLen == 0)
        {
            return string.Empty;
        }

        // we are ok with slightly larger buffer since the output string will always
        // have the exact length of the output produced.
        int outputLen = GetSafeCharCountForEncoding(bytes);
        Span<char> output = new char[outputLen];
        if (!internalEncode(
            bytes,
            output,
            padding,
            out int numCharsWritten))
        {
            throw new InvalidOperationException("Internal error: couldn't calculate proper output buffer size for input");
        }

        return new string(output[..numCharsWritten]);
    }

    /// <summary>
    /// Decode a Base32 encoded string into bytes.
    /// </summary>
    /// <param name="text">Encoded Base32 string.</param>
    /// <returns>Decoded bytes.</returns>
    public byte[] Decode(ReadOnlySpan<char> text)
    {
        int paddingLen = getPaddingCharCount(text);
        int textLen = text.Length - paddingLen;
        int outputLen = getAllocationByteCountForDecoding(textLen);
        if (outputLen == 0)
        {
            return [];
        }

        var outputBuffer = new byte[outputLen];
        var result = internalDecode(text[..textLen], outputBuffer, out int numBytesWritten);
        return result switch
        {
            DecodeResult.InvalidInput => throw new ArgumentException("Invalid character in input", nameof(text)),
            DecodeResult.OutputOverflow => throw new InvalidOperationException("Output buffer is too small"),
            DecodeResult.Success when numBytesWritten != outputLen => throw
                new InvalidOperationException("Actual written bytes are different"),
            DecodeResult.Success => outputBuffer,
            var x => throw new InvalidOperationException($"Unhandled decode result: {x}"),
        };
    }

    /// <summary>
    /// Encode a binary stream to a Base32 text stream without padding.
    /// </summary>
    /// <param name="input">Input bytes.</param>
    /// <param name="output">The writer the output is written to.</param>
    public void Encode(Stream input, TextWriter output)
    {
        Encode(input, output, padding: false);
    }

    /// <summary>
    /// Encode a binary stream to a Base32 text stream.
    /// </summary>
    /// <param name="input">Input bytes.</param>
    /// <param name="output">The writer the output is written to.</param>
    /// <param name="padding">Whether to use padding at the end of the output.</param>
    public void Encode(Stream input, TextWriter output, bool padding)
    {
        StreamHelper.Encode(input, output, (buffer, lastBlock) =>
        {
            bool usePadding = lastBlock && padding;
            return Encode(buffer.Span, usePadding);
        });
    }

    /// <summary>
    /// Encode a binary stream to a Base32 text stream without padding.
    /// </summary>
    /// <param name="input">Input bytes.</param>
    /// <param name="output">The writer the output is written to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task EncodeAsync(Stream input, TextWriter output)
    {
        return EncodeAsync(input, output, padding: false);
    }

    /// <summary>
    /// Encode a binary stream to a Base32 text stream.
    /// </summary>
    /// <param name="input">Input bytes.</param>
    /// <param name="output">The writer the output is written to.</param>
    /// <param name="padding">Whether to use padding at the end of the output.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task EncodeAsync(Stream input, TextWriter output, bool padding)
    {
        await StreamHelper.EncodeAsync(input, output, (buffer, lastBlock) =>
        {
            bool usePadding = lastBlock && padding;
            return Encode(buffer.Span, usePadding);
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Decode a text stream into a binary stream.
    /// </summary>
    /// <param name="input">TextReader open on the stream.</param>
    /// <param name="output">Binary output stream.</param>
    public void Decode(TextReader input, Stream output)
    {
        StreamHelper.Decode(input, output, buffer => Decode(buffer.Span));
    }

    /// <summary>
    /// Decode a text stream into a binary stream.
    /// </summary>
    /// <param name="input">TextReader open on the stream.</param>
    /// <param name="output">Binary output stream.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DecodeAsync(TextReader input, Stream output)
    {
        await StreamHelper.DecodeAsync(input, output, buffer => Decode(buffer.Span))
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public bool TryEncode(ReadOnlySpan<byte> bytes, Span<char> output, out int numCharsWritten)
    {
        return TryEncode(bytes, output, padding: false, out numCharsWritten);
    }

    /// <summary>
    /// Encode to the given preallocated buffer.
    /// </summary>
    /// <param name="bytes">Input bytes.</param>
    /// <param name="output">Output buffer.</param>
    /// <param name="padding">Whether to use padding characters at the end.</param>
    /// <param name="numCharsWritten">Number of characters written to the output.</param>
    /// <returns>True if encoding is successful, false if the output is invalid.</returns>
    public bool TryEncode(
        ReadOnlySpan<byte> bytes,
        Span<char> output,
        bool padding,
        out int numCharsWritten)
    {
        int bytesLen = bytes.Length;
        if (bytesLen == 0)
        {
            numCharsWritten = 0;
            return true;
        }

        return internalEncode(bytes, output, padding, out numCharsWritten);
    }

    /// <inheritdoc/>
    public bool TryDecode(ReadOnlySpan<char> input, Span<byte> output, out int numBytesWritten)
    {
        int inputLen = input.Length - getPaddingCharCount(input);
        if (inputLen == 0)
        {
            numBytesWritten = 0;
            return true;
        }

        int outputLen = output.Length;
        if (outputLen == 0)
        {
            numBytesWritten = 0;
            return false;
        }

        return internalDecode(input[..inputLen], output, out numBytesWritten) == DecodeResult.Success;
    }

    private static int getAllocationByteCountForDecoding(int textLenWithoutPadding)
    {
        return textLenWithoutPadding * bitsPerChar / bitsPerByte;
    }

    private bool internalEncode(
       ReadOnlySpan<byte> input,
       Span<char> output,
       bool padding,
       out int numCharsWritten)
    {
        string table = Alphabet.Value;

        int bitsLeft = bitsPerByte;
        int outputPad;
        int o = 0;
        int value = input[0];
        for (int i = 0; i < input.Length;)
        {
            if (bitsLeft > bitsPerChar)
            {
                bitsLeft -= bitsPerChar;
                outputPad = value >> bitsLeft;
                if (o >= output.Length)
                {
                    goto Overflow;
                }

                output[o++] = table[outputPad];
                value &= (1 << bitsLeft) - 1;
            }

            int nextBits = bitsPerChar - bitsLeft;
            bitsLeft = bitsPerByte - nextBits;
            outputPad = value << nextBits;
            if (++i < input.Length)
            {
                value = input[i];
                outputPad |= value >> bitsLeft;
                value &= (1 << bitsLeft) - 1;
            }

            if (o >= output.Length)
            {
                goto Overflow;
            }

            output[o++] = table[outputPad];
        }

        if (padding)
        {
            char paddingChar = Alphabet.PaddingChar;
            while (o < output.Length)
            {
                output[o++] = paddingChar;
            }
        }

        numCharsWritten = o;
        return true;
    Overflow:
        numCharsWritten = o;
        return false;
    }

    private int getPaddingCharCount(ReadOnlySpan<char> text)
    {
        char paddingChar = Alphabet.PaddingChar;
        int result = 0;
        int textLen = text.Length;

        if (Alphabet.PaddingPosition == PaddingPosition.Start)
        {
            foreach (char c in text)
            {
                if (c != paddingChar)
                {
                    return result;
                }

                result++;
            }

            return result;
        }

        while (textLen > 0 && text[--textLen] == paddingChar)
        {
            result++;
        }

        return result;
    }

    private DecodeResult internalDecode(
        ReadOnlySpan<char> input,
        Span<byte> output,
        out int numBytesWritten)
    {
        var table = Alphabet.ReverseLookupTable;
        int outputPad = 0;
        int bitsLeft = bitsPerByte;

        numBytesWritten = 0;
        int o = 0;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            int b = table[c] - 1;
            if (b < 0)
            {
                numBytesWritten = o;
                return DecodeResult.InvalidInput;
            }

            if (bitsLeft > bitsPerChar)
            {
                bitsLeft -= bitsPerChar;
                outputPad |= b << bitsLeft;
                continue;
            }

            int shiftBits = bitsPerChar - bitsLeft;
            outputPad |= b >> shiftBits;
            if (o >= output.Length)
            {
                return DecodeResult.OutputOverflow;
            }

            output[o++] = (byte)outputPad;
            b &= (1 << shiftBits) - 1;
            bitsLeft = bitsPerByte - shiftBits;
            outputPad = b << bitsLeft;
        }

        numBytesWritten = o;
        return DecodeResult.Success;
    }
}

/// <summary>
/// Base32 alphabet flavors.
/// </summary>
public class Base32Alphabet : CodingAlphabet
{
    private static readonly Lazy<Base32Alphabet> rfc4648Alphabet = new
        (() => new Base32Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"));

    private static readonly Lazy<Base32Alphabet> extendedHexAlphabet = new
        (() => new Base32Alphabet("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

    private static readonly Lazy<Base32Alphabet> zBase32Alphabet = new
        (() => new Base32Alphabet("ybndrfg8ejkmcpqxot1uwisza345h769"));

    private static readonly Lazy<Base32Alphabet> geohashAlphabet = new
        (() => new Base32Alphabet("0123456789bcdefghjkmnpqrstuvwxyz"));

    private static readonly Lazy<Base32Alphabet> bech32Alphabet = new
        (() => new Base32Alphabet("qpzry9x8gf2tvdw0s3jn54khce6mua7l"));

    private static readonly Lazy<Base32Alphabet> fileCoinAlphabet = new
        (() => new Base32Alphabet("abcdefghijklmnopqrstuvwxyz234567"));

    private static readonly Lazy<AliasedBase32Alphabet> crockfordAlphabet = new
        (() => new AliasedBase32Alphabet(
            "0123456789ABCDEFGHJKMNPQRSTVWXYZ",
            new CharMap[]
            {
                new('O', '0'),
                new('I', '1'),
                new('L', '1'),
            }));

    private static readonly Lazy<AliasedBase32Alphabet> base32HAlphabet = new(
        () => new AliasedBase32Alphabet(
            "0123456789ABCDEFGHJKLMNPQRTVWXYZ",
            paddingChar: '0',
            PaddingPosition.Start,
            new CharMap[]
            {
                new('O', '0'),
                new('I', '1'),
                new('S', '5'),
                new('U', 'V'),
            }));

    /// <summary>
    /// Initializes a new instance of the <see cref="Base32Alphabet"/> class.
    /// </summary>
    /// <param name="alphabet">Characters.</param>
    public Base32Alphabet(string alphabet)
        : base(32, alphabet)
    {
        mapLowerCaseCounterparts(alphabet);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Base32Alphabet"/> class.
    /// </summary>
    /// <param name="alphabet">Encoding alphabet to use.</param>
    /// <param name="paddingChar">Padding character.</param>
    /// <param name="paddingPosition">Position of the padding characters in the encoder output.</param>
    public Base32Alphabet(string alphabet, char paddingChar, PaddingPosition paddingPosition)
        : this(alphabet)
    {
        PaddingChar = paddingChar;
        PaddingPosition = paddingPosition;
    }

    /// <summary>
    /// Gets Crockford alphabet.
    /// </summary>gpg
    public static Base32Alphabet Crockford => crockfordAlphabet.Value;

    /// <summary>
    /// Gets RFC4648 alphabet.
    /// </summary>
    public static Base32Alphabet Rfc4648 => rfc4648Alphabet.Value;

    /// <summary>
    /// Gets Extended Hex alphabet.
    /// </summary>
    public static Base32Alphabet ExtendedHex => extendedHexAlphabet.Value;

    /// <summary>
    /// Gets z-base-32 alphabet.
    /// </summary>
    public static Base32Alphabet ZBase32 => zBase32Alphabet.Value;

    /// <summary>
    /// Gets Geohash alphabet.
    /// </summary>
    public static Base32Alphabet Geohash => geohashAlphabet.Value;

    /// <summary>
    /// Gets FileCoin alphabet.
    /// </summary>
    public static Base32Alphabet FileCoin => fileCoinAlphabet.Value;

    /// <summary>
    /// Gets Base32H alphabet.
    /// </summary>
    public static Base32Alphabet Base32H => base32HAlphabet.Value;

    /// <summary>
    /// Gets Bech32 alphabet.
    /// </summary>
    public static Base32Alphabet Bech32 => bech32Alphabet.Value;

    /// <summary>
    /// Gets the padding character used in encoding.
    /// </summary>
    public char PaddingChar { get; } = '=';

    /// <summary>
    /// Gets the position of the padding characters in the encoder output.
    /// </summary>
    public PaddingPosition PaddingPosition { get; } = PaddingPosition.End;

    private void mapLowerCaseCounterparts(string alphabet)
    {
        foreach (char c in alphabet)
        {
            if (char.IsUpper(c))
            {
                Map(char.ToLowerInvariant(c), ReverseLookupTable[c] - 1);
            }
        }
    }
}

internal sealed class AliasedBase32Alphabet : Base32Alphabet
{
    public AliasedBase32Alphabet(string alphabet, IEnumerable<CharMap> map)
        : base(alphabet)
    {
        setupMap(map);
    }

    public AliasedBase32Alphabet(
        string alphabet,
        char paddingChar,
        PaddingPosition paddingPosition,
        IEnumerable<CharMap> map)
        : base(alphabet, paddingChar, paddingPosition)
    {
        setupMap(map);
    }

    private void setupMap(IEnumerable<CharMap> map)
    {
        foreach (var (from, to) in map)
        {
            mapAlternate(from, to);
        }
    }

    private void mapAlternate(char source, char destination)
    {
        int result = ReverseLookupTable[destination] - 1;
        Map(source, result);
        Map(char.ToLowerInvariant(source), result);
    }
}

/// <summary>
/// Position of the padding in an encoder output.
/// </summary>
public enum PaddingPosition
{
    /// <summary>
    /// Padding appears at the start of the encoded buffer.
    /// </summary>
    Start,

    /// <summary>
    /// Padding appears at the end of the buffer.
    /// </summary>
    End,
}