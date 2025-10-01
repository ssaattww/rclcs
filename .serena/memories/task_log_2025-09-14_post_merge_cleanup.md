2025-09-14: PR #41 マージ後のローカル整備。
- ローカル削除: docs/forbid-push-files-20250914（完了）
- リモート削除: ユーザー実施済み（報告）
- 同期: 環境の名前解決エラーで `git fetch` が失敗。現在 `master` は behind 2。
- 次回ネットワーク復旧後に実行: `git fetch --prune cdr_cs && git reset --hard cdr_cs/master`。