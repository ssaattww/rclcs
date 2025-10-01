namespace Rclcs.Dds;

/// <summary>
/// DDS 抽象レイヤーがプロジェクトとして独立したことを示すマーカー。
/// 具体的な API は今後 OpenDDSharp 等と連携しながら拡張する予定。
/// </summary>
public static class DdsLayerMarker
{
    /// <summary>
    /// 現状のビルド確認用途のダミー定数。
    /// 実装開始時に適切なクラスへ差し替える計画。
    /// </summary>
    public const string Description = "DDS 抽象レイヤーの土台";
}
