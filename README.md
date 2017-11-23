L-System 2D
===============
L-System 2D plant generate for Unity

---
### L-System Setting

右クリック > Create > Custom > PlantConfig

* Img Nodes, Img Leaves  
  樹木に利用する Sprite テクスチャ参照  
  指定パターンからランダムに採用します。

* Rule Fs, Rule Gs
  + Rate : 各ルールの出現頻度（0.0～1.0）
  + Rule : L-System ルール設定文字列（以下参照）

L-Sytem Rule:
|キーワード|動作|
|-|-|
|F|Forward|
|G|Generate|
|+|Left|
|-|Right|
|[|Push|
|]|Pop|

---
### LICENSE
This software is released under the MIT License, see LICENSE.
