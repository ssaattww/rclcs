2025-09-14: xUnit移行と最小実装
変更:
- AGENTS.md 更新: xUnit採用方針を追記、コンソールRunner禁止。
- tests/Interop.Tests 新設: xUnit依存を宣言、StringInteropTestsを追加（正常/異常）。
- 実装: IStringInterop/Utf8StringInterop を追加（CDR準拠: uint32 len=NUL含む）。
- テスト統合: tests/Runner を削除しxUnitに一本化。
- sln更新: Interop.Tests を追加。
検証（保留）:
- ネットワーク制限のため `dotnet restore/test` は未実行。ローカル/CIでの実行を想定。
次アクション:
- Specification.md のA1(String)ワイヤ形式追記（前回パッチ失敗のため再適用）。
- 追加テスト: エッジケース（最大長/空配列/過大len）を拡充。