2025-09-14: A1(String) 参考リンクへアンカー追加
内容:
- DDS-XTypes: 原典PDFにページアンカーを付与（1.3 / PDF#page=28, 7.2.2.1.2.4 String<Char8> type）。
- Fast-CDR: Cdr クラス参照ページへリンク（operator<< (const char*) 参照指示）。
- rmw_fastrtps TypeSupport_impl.hpp: doxygen source の行アンカーを追加（#l136: +1/NUL, #l308: 4B整列）。
- CORBA本: ebin参照はそのまま（書籍のため安定アンカーなし）。
備考:
- XTypes PDF内のNUL含有文言は原典内該当節にある（コミュニティissueの引用と整合）。
- 必要なら章節番号の併記と図版番号を追加予定。