# Agent 運用ガイド（Serena 活用）

このリポジトリで作業するエージェントは、Serena のツールを積極的に活用して計画・記録・参照を行います。以下の指針に従ってください。

## 基本方針
- 日本語で簡潔に記録・報告する（コマンドは必要に応じて併記）。
- 必要最小限の探索・変更に留め、計画を小さく素早く回す。
- TDD を優先し、テストで挙動を確認する。
  - 仕様追加時は失敗する最小テスト→実装→リファクタの順で反復。
  - Writer などI/O中心の処理はユニット＋スモークの二層で検証。
- コードを編集するときには日本語のコメントを入れること

## Serena の使い方
- 計画管理（必須）: `update_plan`
  - ステップは短文（5～7語程度）で、状態は `pending`/`in_progress`/`completed` を維持。
  - フェーズ変更や追加があれば計画を更新し、理由を一行で付記。
- オンボーディング/メモ: `write_memory` / `read_memory`
  - 以下を最低限維持: `project_overview`/`suggested_commands`/`style_and_conventions`/`completion_checklist`。
  - 重要変更はサマリを新規メモ（例: `task_log_YYYY-MM-DD_topic`）に記録。
  - 作業サマリの参照を徹底: 直近の `work_summary_YYYY-MM-DD` を開始前に確認し、継続性を維持する（今回作成: `work_summary_2025-08-31`）。
- 参照/探索: `find_file`/`search_for_pattern`/`find_symbol`/`get_symbols_overview`
  - 広く読むのではなく、目的に応じてピンポイントに検索。
- 思考チェック: `think_about_task_adherence`/`think_about_collected_information`/`think_about_whether_you_are_done`
  - 編集前後で自己点検し、脱線や取りこぼしを避ける。

### メモ作成時のコミット順序（Memory-first）
- 目的: 未追跡メモが作業ツリーに残り続けることを防ぐ。
- 順序: `write_memory` → `git add` → `git commit`（直後にコミット）。
- 補足:
  - 複数メモをまとめて作成した場合も、同一トピックであれば1コミットにまとめて良い。
  - 以後のコード変更は通常のローカルコミット→push→PRフローに従う。
  - 一括ステージ例（コミット直前）: `git add -A .serena/memories`

### PR運用（Serenaメモ先行記録）
- 原則: PRを作成する前に、Serenaの `write_memory` で「PR意図」を必ず記録する。
  - 目的: 未追跡メモの取りこぼし防止と履歴一貫性の確保。
  - 推奨メモ名: `task_log_YYYY-MM-DD_pr_intent_<summary>`
  - 記載項目: 目的/変更ファイル/ブランチ名/ラベル/マージ方針（ユーザーがマージ）
- 手順:
  1) `write_memory` でPR意図を記録（上記テンプレートに従う）
  2) ブランチ作成 → 変更をコミット（必要なら未追跡メモも同PRに含める）
  3) PR作成（アシスタントはマージしない。マージはユーザーが実施）
- メモテンプレ（例）:
  ```
  2025-09-14: PR意図 — <要約>
  変更: <ファイル一覧>
  ブランチ: <branch-name>
  ラベル: type:docs|feat|...（必要に応じて）
  備考: マージはユーザーが実施
  ```

### Serena 必須ルール（Codex 連携）
- セッション開始時: このディレクトリを Serena プロジェクトとして必ず有効化する。
  - serena__activate_project を使い、プロジェクト名は プロジェクトフォルダ名（カレントディレクトリ）。
  - .serena/project.yml の language: csharp を確認。異なる場合は再アクティベート。
- 解析/探索は Serena ツールを優先: get_symbols_overview / find_symbol / find_referencing_symbols / search_for_pattern。
  - 結果が大きい場合は relative_path（例: csharp/…）や max_answer_chars を調整。
- LSP 同期ずれ時: C# シンボルが解決できない場合はプロジェクトを再アクティベート。可能なら restart_language_server を使用。
- OmniSharp（C# LSP）:
  - PATH に omnisharp を用意。優先度は「ネイティブ OmniSharp → OmniSharp.dll を dotnet + DOTNET_ROLL_FORWARD=Major → run/mono（最終手段）」。
  - 簡易検証は omnisharp --help / --version を 3–5 秒のタイムアウト付きで実行。
  - 連携確認の基準:クラスおよびメソッドが find_symbol で解決できること。
- プレアンブル: 複数の関連ツール呼び出し前に、意図と次ステップを 1–2 文で共有する。
- 承認: ネットワークアクセスや破壊的操作はユーザーの承認（既定: on-request）を得る。

## 記録粒度（既定）
- 既定は「タスク単位＋検証ログ」。
  - タスク単位: 主要変更点・影響・検証方法を短く列挙。
  - 検証ログ: 実行コマンド・結果要約・失敗時の原因/対処。
- 必要に応じて詳細化:
  - 変更単位（変更ファイル/関数/APIと理由・リスク）
  - 設計決定（選択肢・採用理由・影響・将来拡張）
  - 詳細トレース（入出力・境界条件・疑似コード/図）

## 実務ルール
- Gitブランチ運用: 各タスクは専用ブランチを切って作業する（例: `feature/…`, `fix/…`, `docs/…`）。
- コミット粒度: タスク内の区切りごとに小さくコミット（コンパイル可能/テスト通過を原則）。
- 影響範囲が広い変更は計画を分割し、段階的に適用。
- CI/テストは必要時のみ実行し、ログは簡潔に共有。
- セキュリティ: 秘密情報をコミットしない。大きなバイナリは Git LFS。

### Git操作の実行方法（重要）
- MCPサーバを用途で分離して利用する。
  - ローカルGit操作: 「mcp-server-git」（`git__*` ツール）を使用（ステータス/差分/ステージ/コミット/ブランチ操作）。
  - GitHub操作: 「github-mcp-server」（`github__*` ツール）を使用（PR/Issue/レビュー/ラベル/マージ/リリース等）。
  - ブランチ作成ポリシー（remote-first, github-mcp 統一）:
    - 1) `github__create_branch` で GitHub 上にブランチを作成
    - 2) ローカル同期: `git fetch rclcs` → `git checkout -t rclcs/<branch>`（以後、このブランチで作業開始）
    - 3) 以後の変更はローカルでコミットし、`git push` で同期（ドキュメントでも `github__push_files` は使用しない）
    - 4) PR作成（マージはユーザー）。マージ後は自動削除設定（Automatically delete head branches）を推奨。
- `push` について:
  - 原則はローカル`git`による `git push`（ネットワーク利用のため承認必須）。
  - `github__push_files` はドキュメントを含め全面禁止。
    - 理由: いずれもPRを経由するため、ローカル履歴と乖離する運用を採るメリットがない。
- 直接シェルの `git` コマンドは原則使用しない（障害時のみ、承認の上でフォールバック）。
- コミットメッセージは下記
[... omitted 0 of 182 lines ...]

  - `git fetch rclcs` → `git checkout <branch>` → `git reset --hard rclcs/<branch>`。
- 混在注意: `github__push_files` は使用しないため混在は発生しない。過去の履歴で混在がある場合は `git fetch` / `git reset --hard` で整合を取る。
- 乖離チェック: 次の作業前に `git status -sb` で upstream 乖離がないことを確認。

### コミットメッセージ規約（Conventional Commits 準拠）
- 重要: type 以外は日本語にすること。
```
<type>: <subject>

<body>
```
- type 一覧:
  - feat: A new feature
  - fix: A bug fix
  - docs: Documentation only changes
  - style: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
  - refactor: A code change that neither fixes a bug nor adds a feature
  - perf: A code change that improves performance
  - test: Adding missing or correcting existing tests
  - chore: Changes to the build process or auxiliary tools and libraries such as documentation generation

## テスト/検証方
- 基本: TDD を徹底（ユニット→結合→スモーク）。
- 相互検証: 公式実装で読み出し/書き出し検証を行う。
  - 目的: 相互運用性とレコード整合性の確認。
  - 手順例:
    - C#: `dotnet test McapCs.sln -c Release`
    - 生成ファイルを Python で検証:
      - インストール: `pip install mcap`（必要に応じて仮想環境）
      - 読み込みサンプル:
        - `python -c "from mcap.reader import make_reader; import sys; f=open(sys.argv[1],'rb'); r=make_reader(f); [(_ for _ in r.iter_messages())]; print('ok')" out.mcap`
      - 圧縮名検査（任意）: `mcap info out.mcap` で `Compression: lz4|zstd|none` を確認。

### MCAP Writer 圧縮の使い方（C#）
この内容はドキュメントを `McapCs/Writer/README.md` に移しました。最新情報はそちらを参照してください。

## 圧縮対応のTDD例（Writerのみ）
- テストを先行して追加:
  - 圧縮なし/あり（lz4, zstd）で小サイズ・しきい値前後・大サイズの各ケースを作成。
  - チャンクヘッダの `compression` と `compressedSize/uncompressedSize` の関係を検証。
  - CRC 有効/無効の動作を検証。
- 実装方針:
  - `ChunkWriter` 実装（`BufferWriter` に加え `Lz4ChunkWriter`/`ZstdChunkWriter`）を追加。
  - `McapWriter.Open/GetChunkWriter/WriteChunk` で切替と圧縮データ採用条件（最小サイズ/比率）を適用。
  - Python 公式実装で読み出し可能であることをスモーク確認。

## 例（ワークフロー）
1. `update_plan` で小さな計画を作成
2. `find_file`/`search_for_pattern` で関連箇所を特定
3. `apply_patch` で変更
4. ビルド/テスト（TDD）
5. Python 公式実装で相互検証（必要に応じて `mcap info`/読取スクリプト）
6. `write_memory` にタスク要約＋検証ログを記録
7. `think_about_whether_you_are_done` で完了確認

## テストフレームワーク（xUnit）
- 既定: xUnit.net を使用する。コンソールRunnerは使用しない。
- 配置: `tests/<Name>.Tests` ディレクトリ（例: `tests/Interop.Tests`）。
- 依存: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`（必要に応じ `coverlet.collector`）。
- 実行: `dotnet test rclcs.sln`（ネットワーク制限下では実行を保留し、CI/ローカルで復元・実行）。
- 設計: Fact/Theory を用いたTDD、外部依存の最小化、テスト名は日本語可（意図を明確に）。
