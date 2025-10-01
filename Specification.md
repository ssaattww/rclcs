# 仕様書: C# から ROS 2 と相互通信するための 2 アプローチ（OpenDDS 利用）

最終更新: 2025-09-14（JST）

---

## 0. 目的・非目的

- **目的**  
  - C#（.NET）アプリが、ROS 2 システムと**トピックレベルで相互通信**できるようにする。  
  - 自作 P/Invoke を書かずに、**OpenDDS の .NET バインディング（OpenDDSharp）** を用いる。

- **非目的（当初スコープ外）**  
  - ROS 2 ノード/グラフAPI/パラメータ/サービス/アクションの完全再現。  
  - DDS 独自機能への依存（標準DDS範囲内で構築）。

---

## 1. アプローチ概要

### A. 純DDSインタープ（ROS層をバイパス）
- C# から **OpenDDSharp（OpenDDS を .NET から扱うラッパー）** を使用。  
- ROS 2 側（RMW が Fast DDS / Cyclone DDS / Connext / OpenDDS など）と、**DDS の相互運用**によりデータ平面で通信。  
- 成功条件は **(1) 同一の型定義 (IDL)**、**(2) トピック名のマッピング準拠**、**(3) QoS 整合**。

### B. “C#版クライアントライブラリ”を OpenDDS 上に実装
- OpenDDS の Publisher/Subscriber/Request-Reply を足場に、**C# 側で ROS 2 の語彙（ノード/パラメータ/サービス/アクション等）を再現**。  
- `rmw_opendds` の設計・挙動を参考にしつつ、A を実証・安定化後に段階的に B を整備（長期ロードマップ）。

---

### C. レイヤー構造（OSS DDS 前提）

以下は最上段をアプリケーション、最下段を OS とした階層構造。ROS 2 クライアント層では、上段に各言語のクライアントライブラリ、下段に C 言語の `rcl` が位置する。参考: <https://docs.ros.org/en/rolling/Concepts/Basic/About-Client-Libraries.html> / <https://docs.ros.org/en/rolling/Concepts/Advanced/About-Middleware-Implementations.html>

| 層（上→下） | 役割 | 代表的コンポーネント | 今回の位置づけ |
| --- | --- | --- | --- |
| アプリケーションレイヤ | 利用者が実装する ROS 2/C# アプリケーション | C# アプリケーション、ROS 2 ノード | 利用者アプリ |
| ROS 2 クライアントレイヤ | 上段: `rclcpp`/`rclpy` 等の言語別クライアント API。下段: `rcl` が共通のクライアントサポートを提供。 | `rclcpp`, `rclpy`, `rclc`, `rcl` | 将来的に `Rclcs.Client` が `rcl` 相当の C# 抽象を提供 |
| DDS 抽象レイヤ | RMW インターフェースで DDS 実装差異を吸収し、CDR シリアライゼーション/タイプサポートを提供 | `rmw_*` パッケージ、OpenDDSharp、`Rclcs.Dds` | 現タスク: OpenDDSharp + `Rclcs.Dds` で抽象層を構築 |
| DDS 実装レイヤ | DDS プロトコルの実装本体 | OpenDDS、Cyclone DDS、Fast DDS など | 本計画では OpenDDS を採用 |
| OS レイヤ | ネットワーク・プロセス基盤 | Linux、Windows など | 実行環境 |

- OpenDDSharp は LGPL-3.0 ライセンス: <https://libraries.io/nuget/OpenDDSharp>

### D. プロジェクト構成（2025-10-01 時点）

- `src/Rclcs.Client`: ROS 2 クライアントレイヤの土台（API を今後整備）。
- `src/Rclcs.Dds`: DDS 抽象レイヤの最小実装（文字列インタープ等）。
- `tests/Interop.Tests`: レイヤー越しの相互運用テスト群。

## 2. 参照標準と設計ドキュメント

- OpenDDS マニュアル: <https://opendds.readthedocs.io/en/latest/>  
- OpenDDSharp リポジトリ: <https://github.com/objectcomputing/OpenDDSharp>  
- rmw_opendds: <https://github.com/ros2/rmw_opendds>  
- ROS 2: Topic/Service 名の DDS マッピング: <https://design.ros2.org/articles/topic_and_service_names.html>  
- RMW OpenDDS 実装（参考）: <https://docs.ros.org/en/rolling/p/rmw_opendds/>  
- DDS を ROS 2 下で使う背景: <https://design.ros2.org/articles/ros_on_dds.html>

---

## 3. 共通要件（A/B に共通）

- ROS 2 の `.msg/.idl` と**同一レイアウトの DDS IDL**を用意すること。  
- 生成方法（いずれか）  
  - OpenDDS の IDL コンパイラ `opendds_idl` による **IDL → C++ 型生成** を行い、OpenDDSharp 既定の C# ラッパーを利用  
  - **OpenDDSharp の DynamicType API** を用い、IDL 由来の TypeObject をランタイム登録  
- **互換性原則**  
  - フィールド順・型幅・可変長/配列指定を厳密に一致。  
  - `builtin_interfaces` 等の標準型は既存 IDL を再利用（改変禁止）。

### 3.2 トピック名マッピング（ROS 2 → DDS）
- ROS 2 トピック `/foo/bar` は、DDS では一般に **`rt/foo/bar`** にマップ（設計文書に従う）。  
- 名前空間・サブネームスペースは `/` を保持。  
- 例: ROS 2 `/robot/cmd_vel` → DDS `rt/robot/cmd_vel`。  
- サービス/アクションを扱う場合は、Request/Reply トピック命名規約に従う。

### 3.3 QoS 整合（推奨初期値）
| 項目 | ROS 2 既定の目安 | OpenDDS/OpenDDSharp 設定例 |
|---|---|---|
| Reliability | Reliable | `ReliabilityQosPolicyKind.RELIABLE_RELIABILITY_QOS` |
| Durability | Volatile | `DurabilityQosPolicyKind.VOLATILE_DURABILITY_QOS` |
| History | KeepLast(depth=10) | `HistoryQosPolicyKind.KEEP_LAST_HISTORY_QOS`, depth=10 |
| Deadline | 未指定 | `Duration.Infinite`（`TimeValue.Zero` を指定しない） |
| Lifespan | 未指定 | 必要に応じ `LifespanQosPolicy` を設定 |
| Liveliness | Automatic | `LivelinessQosPolicyKind.AUTOMATIC_LIVELINESS_QOS` |

> 注意: 実際の既定は RMW/型/ツール（`ros2 topic pub/echo` 等）で差異があるため、通信相手の QoS を確認して合わせること。

### 3.4 ドメイン/ディスカバリ
- **Domain ID** は通信相手と一致させる（既定: `0` が多い）。  
- マルチキャスト/ユニキャスト・Participant QoS はネットワーク環境に応じ調整。

### 3.5 セキュリティ（任意）
- DDS Security（Access Control/Authentication/Encryption）を使用可能。  
- ROS 2 側が SROS2 等でセキュア化されている場合、DDS Security 文書に従い証明書類を合わせる。

---

## 4. アプローチ A の仕様（純DDSインタープ）

### 4.1 依存関係
- NuGet: `OpenDDSharp`（OpenDDS 向け .NET バインディング）  
  - NuGet パッケージ: <https://www.nuget.org/packages/OpenDDSharp>  
- もしくは OpenDDS ネイティブツールチェーン（`opendds_idl`, `dcpsinfo_repo` など）

### 4.2 構成要素
- `DomainParticipant`、`Topic<T>`、`DataWriter<T>`、`DataReader<T>` で構成。  
- `T` は IDL から生成した C# 型、または Dynamic Data を使用。

### 4.3 実行フロー（Publisher/Subscriber）
1. **Participant 作成**（Domain ID を ROS 2 と一致）  
2. **型登録**（IDL 生成型 or Dynamic Data）  
3. **Topic 作成**（名前は `rt/...` マッピングを適用）  
4. **QoS 設定**（Reliability/Durability/History 等を相手に合わせる）  
5. **Writer/Reader 作成**  
6. **送受信**（`writer.Write(data)` / `reader.OnDataAvailable` または Poll）

### 4.4 動作確認
- ROS 2 側で `ros2 topic echo /chatter`，C# 側から `rt/chatter` に Publish。  
- 逆方向は `ros2 topic pub` を使用し C# 側で受信確認。  
- 大規模データ（`sensor_msgs/PointCloud2` など）も、適切な QoS/Fragment 設定で実運用可。

### 4.5 制約
- ノード/パラメータ/グラフ情報は**自動では見えない**。  
- ツール（`ros2 node list` 等）への露出は限定的（データ平面のみ）。

### 4.6 A1: Minimal Interop (String) — ワイヤ形式
- エンコード: UTF-8（BOMなし）
- 長さヘッダ: 32ビット符号なし整数（4バイト）。値は「UTF-8バイト数 + 終端NUL(1バイト)」。
- 配置: `[len:4B][payload:len-1B][0x00]`。ヘッダは4B境界に整列。
- エンディアン: CDRカプセル化の指定に追従（A1の最小実装では Little Endian を既定）。
- 検証/エラー: デコード時に以下を検証し、不一致は例外とする。
  - バッファ長が `4 + len` 以上であること
  - `payload` 末尾に `0x00`（NUL）が存在すること
  - `payload`（`len-1`バイト）が厳格UTF-8として復元可能であること

例（Little Endian）
- "hello": 文字列長=5, バイト列=`68 65 6c 6c 6f` → len=6 →
  `06 00 00 00  68 65 6c 6c 6f 00`
- "ほげ": UTF-8=`E3 81 BB E3 81 92`（6バイト）→ len=7 →
  `07 00 00 00  E3 81 BB E3 81 92 00`

備考
- 本仕様は ROS 2 の CDR 文字列表現（長さに終端NULを含む）に整合する。将来的にエンディアンはカプセル化フラグで切替予定。

---

#### 参考と出典（短い原文引用）
- CDR（CORBA）文字列の長さとNUL含有  
  引用: "Strings are encoded as an unsigned long that indicates the length, including its terminating NUL byte."  
  出典: Advanced CORBA Programming with C++（CDR の定義解説）  
  参照: https://ebin.pub/advanced-corba-programming-with-c-0201379279-9780201379273.html

- DDS-XTypes（NUL含有の明示）  
  引用: "including the terminating NUL character."（コミュニティでの仕様引用）  
  参照: https://github.com/ros2/rmw_cyclonedds/issues/43  
  原典PDF: https://www.omg.org/spec/DDS-XTypes/1.3/PDF#page=28 （7.2.2.1.2.4 String<Char8> type）

- Fast-CDR の直列化（NUL終端文字列）  
  引用: "serializes a null-terminated string."（`Cdr::operator<<(const char*)` 説明）  
  参照: https://docs.ros.org/en/melodic/api/fastcdr/html/classeprosima_1_1fastcdr_1_1Cdr.html （ページ内の “operator<< (const char* string_t)” を参照）

- ROS 2 Fast RTPS TypeSupport 実装例（4B整列と +1[NUL]）  
  例: `current_alignment += 4 + eprosima::fastcdr::Cdr::alignment(current_alignment, 4);` → https://docs.ros.org/en/ros2_packages/humble/api/rmw_fastrtps_dynamic_cpp/generated/TypeSupport__impl_8hpp_source.html#l308  
  例: 上限チェックで `string_upper_bound_ + 1`（NULを含める） → https://docs.ros.org/en/ros2_packages/humble/api/rmw_fastrtps_dynamic_cpp/generated/TypeSupport__impl_8hpp_source.html#l136

## 5. アプローチ B の仕様（OpenDDS 上で C# クライアント語彙を再現）

### 5.1 目的
- C# だけで ROS 2 に近い開発体験を提供（ノード、パラメータ、サービス/アクション、タイマー等）。

### 5.2 設計ガイド
- **Node 相当**: Participant + 名前空間 + ライフサイクル管理  
- **Publisher/Subscriber**: A と同様（Topic/QoS 整合）  
- **Service/Action**: **DDS Request-Reply** パターンを採用し、ROS 2 の命名規約に合わせる  
- **グラフ情報**: ROS 2 内部のメタトピックを購読し、C# 側でビューを構築（段階導入）

### 5.3 互換性・参照
- RMW OpenDDS 実装（`rmw_opendds`）の命名/QoS/型の扱いを参照。  
- 公式リリースに合わせ、ディストリごとの差分（型記述/ハッシュなど）が出た場合は追従。

### 5.4 品質/配布
- **品質レベル**: REP-2004 に準拠した Quality Declaration を整備。  
- **CI**: Linux/Windows の .NET（LTS）＋複数 DDS 実装との相互運用試験。  
- **サンプル**: `std_msgs/String`, `geometry_msgs/Twist`, `sensor_msgs/PointCloud2` から開始。

---

## 6. ビルド・デプロイ・運用

- **ビルド**:  
  - OpenDDSharp → 通常の `dotnet build` で利用可能。  
  - OpenDDS ネイティブ導入時は `opendds_idl` などの生成物をビルド後に配置。  
- **運用**:  
  - Domain/Discovery 設定を運用環境でプロファイル化（XML QoS プロファイル）。  
  - コンテナ/VM 配布では、ライセンスとネイティブ依存（ランタイム）配置に留意。

---

## 7. テスト方針

- **単体**: 型のシリアル化/デシリアル化、可変長/配列境界、NaN/Inf。  
- **相互運用**:  
  - RMW = Fast DDS / Cyclone DDS / OpenDDS / Connext の ROS 2 と往復通信。  
  - 複数 QoS 組合せ（信頼性/歴史/デッドライン等）。  
- **負荷**: 高頻度トピック、スループット、レイテンシ、ロス率。  
- **長期**: Discovery 再参加、ネットワーク切断/復旧、Participant 再生成。

---

## 8. 既知のリスクと回避策

- **名前/型/QoS の不一致** → 通信不可  
  - → 片側の QoS/トピック名/IDL を検証し整合させるチェックリストを運用。  
- **DDS 実装差異**（フラグメント化/リライアビリティ挙動）  
  - → 実装横断の試験ベンチを常設。  
- **セキュリティ/証明書配布**  
  - → SROS2/DDS Security の手順を文書化し、証明書ローテーションを手順化。

---

## 9. ロードマップ

1. **A の最小実装**（`std_msgs/String`）で往復通信  
2. `geometry_msgs/Twist`、`sensor_msgs/Image/PointCloud2` を追加  
3. QoS プロファイルのテンプレ整備  
4. **B の着手**: Request-Reply による Service、Action（Goal/Result/Feedback）の順で対応  
5. グラフ情報/ノード/パラメータの再現  
6. Quality Declaration/ドキュメント整備、サンプル群公開

---

## 10. 参考リンク（抜粋）

- OpenDDS マニュアル: <https://opendds.readthedocs.io/en/latest/>  
- OpenDDSharp サンプル: <https://github.com/objectcomputing/OpenDDSharp/tree/master/examples>  
- ROS 2 名称マッピング: <https://design.ros2.org/articles/topic_and_service_names.html>  
- RMW OpenDDS: <https://github.com/ros2/rmw_opendds> / <https://docs.ros.org/en/rolling/p/rmw_opendds/>  
- DDS 採用背景（ROS on DDS）: <https://design.ros2.org/articles/ros_on_dds.html>  
- OpenDDS セキュリティ ガイド: <https://opendds.readthedocs.io/en/latest/security/security.html>
