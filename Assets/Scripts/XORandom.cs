using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 乱数クラス
/// XorShiftによる乱数生成クラス
/// </summary>
public class XORandom {
  private const ulong DefSeedX = 123456789;
  private const ulong DefSeedY = 362436069;
  private const ulong DefSeedZ = 521288629;
  private const ulong DefSeedW = 88675123;

  private ulong seedX;
  private ulong seedY;
  private ulong seedZ;
  private ulong seedW;

  /// <summary>
  /// コンストラクタ
  /// </summary>
  /// <param name="seed1">シード値1</param>
  /// <param name="seed2">シード値2</param>
  /// <param name="seed3">シード値3</param>
  /// <param name="seed4">シード値4</param>
  public XORandom(int seed1, int seed2, int seed3, int seed4) {
    this.Srand(seed1, seed2, seed3, seed4);
  }
  public XORandom(int seed) : this((int)DefSeedX, (int)DefSeedY, (int)DefSeedZ, seed) { }
  public XORandom() : this(DateTime.Now.Millisecond) { }

  /// <summary>
  /// 乱数シード値のセット
  /// </summary>
  /// <param name="seed1">シード値1</param>
  /// <param name="seed2">シード値2</param>
  /// <param name="seed3">シード値3</param>
  /// <param name="seed4">シード値4</param>
  public void Srand(int seed1, int seed2, int seed3, int seed4) {
    seedX = (ulong)seed1;
    seedY = (ulong)seed2;
    seedZ = (ulong)seed3;
    seedW = (ulong)seed4;
  }

  /// <summary>
  /// 簡易版乱数シード値セット
  /// シード4以外にデフォルト値を利用します。
  /// </summary>
  /// <param name="seed">シード値</param>
  public void Srand(int seed) {
    this.Srand((int)DefSeedX, (int)DefSeedY, (int)DefSeedZ, seed);
  }

  /// <summary>
  /// 時間（ミリ秒）をシード値に利用します
  /// </summary>
  public void Srand() {
    this.Srand(DateTime.Now.Millisecond);
  }

  /// <summary>
  /// xorshiftによる乱数取得
  /// </summary>
  /// <returns>生成した乱数値</returns>
  public ulong NextULong() {
    ulong t = (seedX ^ (seedX << 11));
    seedX = seedY;
    seedY = seedZ;
    seedZ = seedW;
    return (seedW = (seedW ^ (seedW >> 19)) ^ (t ^ (t >> 8)));
  }

  /// <summary>
  /// longでの値取得
  /// </summary>
  /// <returns></returns>
  public long NextLong() {
    return (long)NextULong();
  }

  /// <summary>
  /// uintでの値取得
  /// </summary>
  /// <returns></returns>
  public uint NextUInt() {
    return (uint)NextULong();
  }

  /// <summary>
  /// intでの乱数値取得
  /// </summary>
  /// <returns></returns>
  public int NextInt() {
    return (int)NextULong();
  }

  /// <summary>
  /// 指定範囲での乱数返却
  /// </summary>
  /// <param name="min">最小値</param>
  /// <param name="max">最大値</param>
  /// <returns>指定範囲の乱数（Random.Range同様max値を含まない）</returns>
  public int Range(int min, int max) {
    int val = max - min;
    if (val <= 0) {
      return min;
    }
    return min + Mathf.Abs(this.NextInt()) % val;
  }

  //小数返却版（maxを含まない）
  public float Range(float min, float max) {
    return min + (max - min) * ((float)Range(0, 0x7fffffff) / 0x7fffffff);
  }

  #region ---- static use ----
  /// <summary>
  /// static用インスタンス
  /// </summary>
  private static XORandom rand = new XORandom();

  /// <summary>
  /// 簡易的に利用する乱数発生器
  /// </summary>
  /// <param name="min">最小値</param>
  /// <param name="max">最大値</param>
  /// <returns>指定範囲の乱数</returns>
  public static int RandRange(int min, int max) {
    return rand.Range(min, max);
  }

  //小数版（maxを含まない）
  public static float RandRange(float min, float max) {
    return rand.Range(min, max);
  }
  #endregion
}
