2025-09-14: A1(String) 追加出典メモ
目的: 4.6節（ワイヤ形式）の根拠を一次情報で明示。
出典（短い原文引用を含む）:
- OMG CDR/CORBA（CDRの文字列表現）
  - 引用: "Strings are encoded as an unsigned long that indicates the length, including its terminating NUL byte."（Advanced CORBA Programming with C++）
  - 参照: https://ebin.pub/advanced-corba-programming-with-c-0201379279-9780201379273.html
  - 補足: unsigned long は CDRで4バイト（32-bit）。
- DDS-XTypes（NUL含有の根拠）
  - 引用: "including the terminating NUL character."（コミュニティ引用。原典は OMG DDS-XTypes）
  - 参照: https://github.com/ros2/rmw_cyclonedds/issues/43
  - 原典: https://www.omg.org/spec/DDS-XTypes/ （v1.x PDF内の string 型説明）
- eProsima Fast-CDR（NUL終端文字列の直列化）
  - 引用: "serializes a null-terminated string."（Cdr::operator<<(const char*)）
  - 参照: https://fast-dds.docs.eprosima.com/en/latest/fastcdr/fastcdr.html （または docs.ros.org からの鏡像）
- ROS 2 Fast RTPS TypeSupport 実装（4B整列と+1の扱い）
  - 例: `current_alignment += 4 + eprosima::fastcdr::Cdr::alignment(current_alignment, 4);`
  - 例: 上限チェックで `+1`（NUL）を含める前提（`string_upper_bound_ + 1`）。
  - 参照: https://docs.ros.org/en/ros2_packages/humble/api/rmw_fastrtps_dynamic_cpp/generated/program_listing_file_include_rmw_fastrtps_dynamic_cpp_TypeSupport_impl.hpp.html
要約:
- 長さヘッダ=uint32(4B)、値は(UTF-8バイト数 + 1[NUL])。
- 4バイト境界整列。A1はLittle Endianを既定（将来CDRカプセル化に追従）。