2025-09-17: A1最小文字列相互運用計画をOSS DDS前提に刷新。
- Connext依存の仕様整理と削除対象を洗い出し。
- 無償DDS候補として OpenDDS + OpenDDSharp(LGPL) を採用方針に設定。C#バインディングとIDLコード生成の経路をOSSで統一。
- A1サブタスクをDDS非依存に再定義: 仕様更新(ライセンス記述/環境セットアップ)、OpenDDSharp最小サンプル検証、文字列CDR仕様ドキュメント、`IStringInterop`インタフェース設計、エンコード/デコード実装、xUnit単体テスト、OpenDDS連携スモークまでを段階化。