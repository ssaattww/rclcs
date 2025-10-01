using System;
using System.Linq;
using System.Text;
using Rclcs.Dds.Interop;
using Xunit;

namespace Interop.Tests;

/// <summary>
/// A1(String) の最小往復とエラー系を xUnit で検証するテスト。
/// 仕様（最小）は CDR に準拠: [len: uint32 little-endian は既定][payload UTF-8][NUL]。
/// </summary>
public class StringInteropTests
{
    private static readonly Utf8StringInterop Interop = new();

    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("ほげ")]
    [InlineData("🧪テスト")] // サロゲートペアを含む
    public void RoundTrip_ok(string input)
    {
        var bytes = Interop.Encode(input);
        var output = Interop.Decode(bytes);
        Assert.Equal(input, output);
    }

    [Fact]
    public void Encode_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => Interop.Encode(null!));
    }

    [Fact]
    public void Header_includes_terminating_nul()
    {
        var bytes = Interop.Encode("A");
        // 期待: len=2 ("A"=1B + NUL=1B)
        Assert.Equal(2u, BitConverter.ToUInt32(bytes, 0));
        Assert.Equal((byte)'A', bytes[4]);
        Assert.Equal(0x00, bytes[5]);
    }

    [Fact]
    public void Decode_missing_nul_throws()
    {
        // "A" をエンコードして末尾NULを削る
        var bytes = Interop.Encode("A");
        var broken = bytes.Take(bytes.Length - 1).ToArray();
        Assert.Throws<FormatException>(() => Interop.Decode(broken));
    }

    [Fact]
    public void Decode_length_shorter_than_buffer_throws()
    {
        // len=3, payload=0x41 0x00（不足）
        var buf = new byte[] { 0x03, 0x00, 0x00, 0x00, 0x41, 0x00 };
        Assert.Throws<FormatException>(() => Interop.Decode(buf));
    }

    [Fact]
    public void Decode_invalid_utf8_throws()
    {
        // 無効な UTF-8 例: 0xC3 0x28 （NULを加えて len=3）
        var buf = new byte[] { 0x03, 0x00, 0x00, 0x00, 0xC3, 0x28, 0x00 };
        Assert.Throws<DecoderFallbackException>(() => Interop.Decode(buf));
    }
}

