2025-09-14: A1(String) 仕様追記と承認ログ
承認: ユーザーによりサブIssue案（A1-1〜A1-8）が承認。
決定: ROS 2/DDS CDRに従い、文字列ワイヤ形式は以下。
- 長さヘッダ: uint32 (32bit, 4B)、値はUTF-8バイト数+終端NUL(1B)を含む。
- エンコード: UTF-8 (BOMなし)。
- レイアウト: [len(4B)][payload(len-1B)][0x00]。ヘッダは4B境界整列。
- エンディアン: CDRのカプセル化に追従。A1最小ではLittle Endianを既定とする。
- エラー: 不足長/終端欠落/不正UTF-8はデコード時に例外。
次アクション: Specification.mdへ追記→実装(IStringInterop/Utf8StringInterop)→ランナー緑化。