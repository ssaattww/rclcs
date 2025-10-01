2025-10-01: プロジェクト分離 — rclcs
- 旧cdr_csライブラリをsrc/Rclcs.Ddsへ移動し、命名と名前空間をRclcs.*に刷新。
- src/Rclcs.Clientプロジェクトを新設してクライアント層のプレースホルダーを追加。
- 新しいrclcs.slnへ再構成し、テスト/RunnerのProjectReferenceとusingをRclcs.Ddsへ更新。
- Specification.mdとAGENTS.mdのレイヤー表記と運用手順を新名称に合わせて更新。