using System;
using System.Buffers.Binary;
using System.Text;

namespace Rclcs.Dds.Interop;

// DDS レイヤーの最小文字列インタープ API を提供する。
/// <summary>
/// 文字列インタープの最小仕様インターフェース。
/// - ワイヤ形式: [len: uint32][payload: UTF-8][0x00]
/// - len は NUL を含む（CDR 準拠）。
/// - 既定のエンディアンは Little Endian（将来のCDRカプセル化により可変）。
/// </summary>
public interface IStringInterop
{
    /// <summary>
    /// 文字列をUTF-8 + NUL終端へ変換し、先頭に4バイトの長さヘッダを付加する。
    /// len は (UTF-8バイト数 + 1) を格納する。
    /// </summary>
    byte[] Encode(string value);

    /// <summary>
    /// ワイヤ形式（[len][payload][NUL]）から文字列を復元する。
    /// </summary>
    string Decode(byte[] buffer);
}

/// <summary>
/// UTF-8 + NUL 終端の最小実装。
/// </summary>
public sealed class Utf8StringInterop : IStringInterop
{
    // 例外フォールバックを有効にした UTF-8。無効なシーケンスは DecoderFallbackException を投げる。
    private static readonly Encoding Utf8Strict = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public byte[] Encode(string value)
    {
        // 入力検証: null は許容しない（用途に応じて将来Optional扱いを検討）。
        if (value is null) throw new ArgumentNullException(nameof(value));

        // UTF-8 ペイロード（NULは含めない）
        var payload = Utf8Strict.GetBytes(value);
        var len = checked((uint)payload.Length + 1u); // NUL 終端分を加算

        var buffer = new byte[4 + len];
        // Little Endian で長さを書き込む
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(0, 4), len);
        // payload コピー
        payload.AsSpan().CopyTo(buffer.AsSpan(4));
        // NUL 終端
        buffer[4 + payload.Length] = 0x00;
        return buffer;
    }

    public string Decode(byte[] buffer)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        if (buffer.Length < 4) throw new FormatException("ヘッダ不足: 4バイト未満");

        var len = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(0, 4));
        if (len == 0) throw new FormatException("長さ0は不正（NULを含むはず）");

        var total = 4 + (long)len;
        if (buffer.Length < total) throw new FormatException("バッファ長がヘッダの長さより短い");

        var payloadLen = checked((int)len - 1);
        var payloadSpan = buffer.AsSpan(4, payloadLen);
        var terminator = buffer[4 + payloadLen];
        if (terminator != 0x00) throw new FormatException("終端NULが存在しない");

        // 厳格UTF-8で復元（不正シーケンスは例外）
        return Utf8Strict.GetString(payloadSpan);
    }
}

