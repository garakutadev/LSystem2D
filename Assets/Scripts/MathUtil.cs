using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 数学関連ユーティリティ
/// </summary>
public class MathUtil {
  /// <summary>
  /// 座標を0～360度の範囲に収める
  /// </summary>
  /// <param name="angle">回転角</param>
  /// <returns>返還後の回転角</returns>
  public static Vector3 AngleRange360(Vector3 angle) {
    angle.x = Mathf.Repeat(angle.x, 360f); 
    angle.y = Mathf.Repeat(angle.y, 360f);
    angle.z = Mathf.Repeat(angle.z, 360f);
    return angle;
  }

  /// <summary>
  /// 座標を0～360度の範囲に収める
  /// </summary>
  /// <param name="angle">回転角</param>
  /// <returns>返還後の回転角</returns>
  public static Vector2 AngleRange360(Vector2 angle) {
    angle.x = Mathf.Repeat(angle.x, 360f); 
    angle.y = Mathf.Repeat(angle.y, 360f);
    return angle;
  }

  /// <summary>
  /// 線分の交差判定
  /// 参考：http://www5d.biglobe.ne.jp/~tomoya03/shtml/algorithm/Hougan.htm
  /// </summary>
  /// <param name="lp11">線1の始点</param>
  /// <param name="lp12">線1の終点</param>
  /// <param name="lp21">線2の始点</param>
  /// <param name="lp22">線2の終点</param>
  /// <returns>
  /// 0：線1上に線2が存在する（floatなのでほぼ無い）
  /// -：線1と線2は交差する
  /// +：線1と線2は交差しない
  /// </returns>
  public static int CheckCrossLine(Vector2 lp11, Vector2 lp12, Vector2 lp21, Vector2 lp22) {
    return (int)(((lp11.x - lp12.x) * (lp21.y - lp11.y) + (lp11.y - lp12.y) * (lp11.x - lp21.x)) *
                 ((lp11.x - lp12.x) * (lp22.y - lp11.y) + (lp11.y - lp12.y) * (lp11.x - lp22.x)));
  }

  /// <summary>
  /// 矩形内に点が存在するか判定する
  /// 矩形線上に点がある場合も含む
  /// </summary>
  /// <param name="pos">判定座標</param>
  /// <param name="rect">矩形</param>
  /// <returns>true：矩形内に点がある</returns>
  public static bool IsInsideRect(Vector2 pos, Rect rect) {
    return ((rect.xMin <= pos.x) && (rect.xMax >= pos.x) &&
            (rect.yMin <= pos.y) && (rect.yMax >= pos.y));
  }

  /// <summary>
  /// 三角形内に点が存在するか反転する
  /// </summary>
  /// <param name="pos">判定座標</param>
  /// <param name="tpos">三角形（3頂点配列）</param>
  /// <returns>true：三角形内に点がある</returns>
  public static bool IsInsideTriangle(Vector2 pos, Vector2[] tpos) {
    //三角形の３点が直線上にある場合は内包しない判定
    if ((tpos[0].x - tpos[2].x) * (tpos[0].y - tpos[1].y) ==
        (tpos[0].x - tpos[1].x) * (tpos[0].y - tpos[2].y)) {
      return false;
    }

    //各辺と点の交差判定を行う、1点をまたいで確認する点を結ぶ線が交差する場合は
    //三角形内に点は存在しない、尚、三角形の直線上に点がある場合は含むと判定する
    if (CheckCrossLine(tpos[0], tpos[1], tpos[2], pos) < 0) { return false; }
    if (CheckCrossLine(tpos[0], tpos[2], tpos[1], pos) < 0) { return false; }
    if (CheckCrossLine(tpos[1], tpos[2], tpos[0], pos) < 0) { return false; }
    return true;
  }

  /// <summary>
  /// 2点の交点を求める
  /// 参考：http://www.h4.dion.ne.jp/~zero1341/t/03.htm
  /// </summary>
  /// <param name="lp11">線1の始点</param>
  /// <param name="lp12">線1の終点</param>
  /// <param name="lp21">線2の始点</param>
  /// <param name="lp22">線2の終点</param>
  /// <param name="pcross">計算した交点</param>
  /// <returns>
  /// 0：線分は平行
  /// -：範囲内で交差する
  /// +：延長線上で交差する
  /// </returns>
  public static int GetCrossPoint(Vector2 lp11, Vector2 lp12, Vector2 lp21, Vector2 lp22, ref Vector2 pcross) {
    // パラメータ表記の値に変換する
    float x1 = lp11.x;
    float y1 = lp11.y;
    float f1 = lp12.x - lp11.x;
    float g1 = lp12.y - lp11.y;

    float x2 = lp21.x;
    float y2 = lp21.y;
    float f2 = lp22.x - lp21.x;
    float g2 = lp22.y - lp21.y;

    // detの計算
    float det = f2 * g1 - f1 * g2;
    if (det == 0) {
      //平行で交わらない
      return 0;
    }

    // 交点におけるパラメータ
    float dx = x2 - x1;
    float dy = y2 - y1;
    float t1 = (f2 * dy - g2 * dx) / det;
    float t2 = (f1 * dy - g1 * dx) / det;

    // 交点の座標
    pcross.x = x1 + f1 * t1;
    pcross.y = y1 + g1 * t1;

    //範囲内（2点を端とする線分）で交差するか確認
    if (0 <= t1 && t1 <= 1 && 0 <= t2 && t2 <= 1) {
      return -1;
    }
    return 1;
  }

  /// <summary>
  /// 2円の交点を求める
  /// </summary>
  /// <param name="pos1">円1の中心</param>
  /// <param name="radius1">円1の半径</param>
  /// <param name="pos2">円2の中心</param>
  /// <param name="radius2">円2の半径</param>
  /// <param name="poslist">求められた交点の格納先、2要素ある配列を用意</param>
  /// <returns>
  /// 交点の数を示す
  /// 0：交点が無い
  /// 1：2円は接する（交点が1）
  /// 2：2円は交わる（交点が2）
  /// </returns>
  public static int GetCrossPointRounds(Vector2 pos1, float radius1, Vector2 pos2, float radius2, Vector2[] poslist) {
    // 引数チェック
    if ((radius1 == 0) || (radius2 == 0)) { return 0; }

    // 2点間の距離を求める
    float dist = Vector2.Distance(pos1, pos2);

    // 距離のチェック
    if (dist > radius1 + radius2) { return 0; }			//円が届かない
    if (dist < Mathf.Abs(radius1 - radius2)) { return 0; }	//円を内包する

    // cosθを求める
    float cosval = (radius1 * radius1 + dist * dist - radius2 * radius2) / (2.0f * radius1 * dist);

    // 角度θを求める
    float rad = Mathf.Acos(cosval);

    // 2円の中心点のX座標、Y座標の差を求める
    float cx = pos2.x - pos1.x;
    float cy = pos2.y - pos1.y;

    // 2円の中心点とX軸のなす角度を求める
    float rad_origin = 0.0f;
    if (cx == 0) {
      if (cy > 0) {
        // 90度
        rad_origin = Mathf.PI / 2.0f;
      } else if (cy < 0) {
        // 270度
        rad_origin = Mathf.PI * 1.5f;
      } else {
        // 交点なし
        return 0;
      }
    } else {
      // Arctanを使用して角度を求める
      rad_origin = Mathf.Atan(cy / cx);

      //cxが負の場合、求める角度に180度加える
      if (cx < 0) { rad_origin += Mathf.PI; }
    }

    // 交点を取得する
    poslist[0].x = pos1.x + radius1 * Mathf.Cos(rad_origin + rad);
    poslist[0].y = pos1.y + radius1 * Mathf.Sin(rad_origin + rad);
    poslist[1].x = pos1.x + radius1 * Mathf.Cos(rad_origin - rad);
    poslist[1].y = pos1.y + radius1 * Mathf.Sin(rad_origin - rad);

    //2点は接する（ほぼ同じ）
    if (Mathf.Abs((radius1 + radius2) - dist) <= float.MinValue) {	//FLT_EPSILON
      return 1;
    }
    return 2;
  }

  /// <summary>
  /// 2Dポリゴン内に点が存在するか確認する
  /// </summary>
  /// <param name="pos">確認する点</param>
  /// <param name="poly">2Dポリゴン</param>
  /// <param name="start">開始インデックス</param>
  /// <param name="end">終了インデックス</param>
  /// <returns>true：ポリゴン内に点が存在する</returns>
  public static bool IsInsidePoly(Vector2 pos, Vector2[] poly, int start, int end) {
    int crossings = 0;
    Vector2 point0 = new Vector2();
    Vector2 point1 = new Vector2();
    bool checkX0, checkY0, checkY1;

    point0.x = poly[end].x;
    point0.y = poly[end].y;

    checkY0 = (point0.y >= pos.y);    //線分の先の点Yと指定座標Yの比較

    for (int i = start; i <= end; i++) {
      point1.x = poly[i].x;
      point1.y = poly[i].y;

      checkY1 = (point1.y >= pos.y);
      if (checkY0 != checkY1) {
        checkX0 = (point0.x >= pos.x);
        if (checkX0 == (point1.x >= pos.x)) {
          if (checkX0) {
            crossings += (checkY0 ? -1 : 1);
          }
        } else {
          if ((point1.x - (point1.y - pos.y)
                * (point0.x - point1.x) / (point0.y - point1.y)) >= pos.x) {
            crossings += (checkY0 ? -1 : 1);
          }
        }
      }
      checkY0 = checkY1;

      point0.x = point1.x;
      point0.y = point1.y;
    }

    //crossingsが0以外であれば多角形内に指定点は存在するとみなす
    return (crossings != 0);
  }
  //省略形：全体を対象とする
  public static bool IsInsidePoly(Vector2 pos, Vector2[] poly) {
    return IsInsidePoly(pos, poly, 0, poly.Length - 1);
  }

  /// <summary>
  /// 直線と点の距離が一番近い点と距離を取得
  /// </summary>
  /// <param name="linep1">線の点1</param>
  /// <param name="linep2">線の点2</param>
  /// <param name="pos">確認する点</param>
  /// <param name="calcpos">線上の点</param>
  /// <returns>線上の点との距離</returns>
  public static float GetNearPointOnLine(Vector3 linep1, Vector3 linep2, Vector3 pos, out Vector3 calcpos) {
    float dist1 = Vector3.Distance(linep1, pos);
    float dist2 = Vector3.Distance(linep2, pos);
    float dist = dist1 + dist2;
    if (dist <= 0) {
      //linep1 == linep2 == posが成立する
      calcpos = pos;
      return 0.0f;
    }
    //linep1を基準に距離スケールを出す
    float t = dist1 / dist;
    /*下記でも良いがLerpが楽
    Vector3 vec = linep2 - linep1;
    vec.x *= t;
    vec.y *= t;
    vec.z *= t;
    calcpos = linep1 + vec;
    */
    calcpos = Vector3.Lerp(linep1, linep2, t);
    return Vector3.Distance(pos, calcpos);
  }

  /// <summary>
  /// 直線と点の距離が一番近い点と距離を取得
  /// </summary>
  /// <param name="linep1">線の点1</param>
  /// <param name="linep2">線の点2</param>
  /// <param name="pos">確認する点</param>
  /// <param name="calcpos">線上の点</param>
  /// <returns>線上の点との距離</returns>
  public static float GetNearPointOnLine2D(Vector2 linep1, Vector2 linep2, Vector2 pos, out Vector2 calcpos) {
    float dist1 = Vector2.Distance(linep1, pos);
    float dist2 = Vector2.Distance(linep2, pos);
    float dist = dist1 + dist2;
    if (dist <= 0) {
      //linep1 == linep2 == posが成立する
      calcpos = pos;
      return 0.0f;
    }
    //linep1を基準に距離スケールを出す
    float t = dist1 / dist;
    calcpos = Vector2.Lerp(linep1, linep2, t);
    return Vector2.Distance(pos, calcpos);
  }

  //直線と点の距離が一番近い点を取得
  public static Vector3 GetPointOnLine(Vector3 linep1, Vector3 linep2, Vector3 pos) {
    float dist1 = Vector3.Distance(linep1, pos);
    float dist2 = Vector3.Distance(linep2, pos);
    float dist = dist1 + dist2;
    if (dist <= 0) {
      return pos;
    }
    float t = dist1 / dist;
    return Vector3.Lerp(linep1, linep2, t);
  }

  //直線と点の距離
  public static float GetDistancePointAndLine(Vector3 linep1, Vector3 linep2, Vector3 pos) {
    float dist1 = Vector3.Distance(linep1, pos);
    float dist2 = Vector3.Distance(linep2, pos);
    float dist = dist1 + dist2;
    if (dist <= 0) {
      return 0.0f;
    }
    float t = dist1 / dist;
    return Vector3.Distance(pos, Vector3.Lerp(linep1, linep2, t));
  }
}
