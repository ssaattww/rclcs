using System;
using Rclcs.Dds.Interop;

// 最小TDD: まずは未実装APIを呼び出す失敗テストを用意する。
// 目的: A1(String) の往復（Encode→Decode）で等価性を確認する。

static class Assert
{
    // 簡易アサート（外部パッケージ不要）
    public static void True(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Equal(string expected, string actual, string message)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
            throw new Exception($"{message}\nexpected='{expected}', actual='{actual}'");
    }
}

class Program
{
    static int Main()
    {
        try
        {
            // DDS 抽象レイヤープロジェクト（Rclcs.Dds）から実装を取得する。
            var interop = new Utf8StringInterop();

            // 正常系1: ASCII
            RoundTrip(interop, "hello");

            // 正常系2: 多バイト（日本語）
            RoundTrip(interop, "ほげ");

            // 正常系3: 空文字
            RoundTrip(interop, "");

            Console.WriteLine("OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL: {ex.Message}");
            return 1;
        }
    }

    // 往復テスト（Encode→Decode）
    static void RoundTrip(IStringInterop interop, string input)
    {
        // 仕様に従ってエンコードし、復号結果を比較する。
        var encoded = interop.Encode(input);
        var decoded = interop.Decode(encoded);
        Assert.Equal(input, decoded, "RoundTrip mismatch");
    }
}
