2025-09-14: A1 Minimal Interop(String) 開始
目的: #21 の作業ブランチ作成、未追跡メモの取り込み、TDDの初期失敗テスト追加。
変更: 
- ブランチ作成: feature/interop-string-21（GitHub/ローカル）
- 追加コミット: .serena/memories/task_log_2025-09-14_post_merge_cleanup.md
- メモ: task_log_2025-09-14_a1_minimal_interop_string_plan（サブIssue再分割案）
- テストランナー追加: tests/Runner/（Program.cs, Runner.csproj）
検証: 
- `git status` クリーン、コミット作成済み。
- ビルド/実行は次ステップ（実装後に実行）。
次アクション:
- IStringInterop/Utf8StringInterop を実装（Encode/Decode, 最小仕様）
- tests/Runner を `dotnet run` でグリーン化
- 仕様メモ（A1-1）を詳細化（長さヘッダ/エンディアンの確定）