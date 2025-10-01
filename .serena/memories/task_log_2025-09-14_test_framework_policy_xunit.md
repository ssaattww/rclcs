2025-09-14: テストフレームワーク方針 — xUnit 採用
目的: TDDの標準実行環境をxUnitに統一し、コンソールRunnerを廃止。
方針:
- フレームワーク: xUnit.net
- ディレクトリ: tests/<Name>.Tests (例: tests/Interop.Tests)
- 依存: Microsoft.NET.Test.Sdk / xunit / xunit.runner.visualstudio / (任意) coverlet.collector
- 実行: `dotnet test cdr_cs.sln`
- 命名: *Tests.cs, Fact/Theoryで表明、外部依存は最小化
- 既存Runner: tests/Runner は削除して統一
メモ: ネットワーク制限下では実行しないが、CI/ローカルでの復元・実行を前提にcsprojへ依存を宣言する。