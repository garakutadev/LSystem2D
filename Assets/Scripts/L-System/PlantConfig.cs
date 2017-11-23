using System;
using UnityEngine;

/// <summary>
/// L-System generate setting
/// </summary>
[CreateAssetMenu(menuName = "Custom/Create Plant Config", fileName = "PlantConfig" )]
public class PlantConfig : ScriptableObject {
  //for render uv source
  public Sprite[] imgNodes;     //nodes(use random)
  public Sprite[] imgLeaves;    //leaves(use grow up)

  public string initiator = null;

  //LSystem rules
  public Rule[] ruleFs = null;
  public Rule[] ruleGs = null;

  //枝の広がり角範囲
  public Vector3 angleMin = new Vector3(0f, 0f, 20f);
  public Vector3 angleMax = new Vector3(0f, 0f, 20f);
  //枝の長さ範囲
  public float distMin = 0.5f;
  public float distMax = 0.5f;

  public int generations = 1;     //L-System生成世代数

  public float thickness = 0.05f; //枝の太さ
  public float tipScale = 0.5f;   //先端方向のスケール
  public float leafScale = 5f;    //葉のスケール
  public float leafZOffset = 0f;  //葉のZオフセット

  public void Normalize() {
    NormalizeRule(ruleFs);
    NormalizeRule(ruleGs);
  }

  private void NormalizeRule(Rule[] rules) {
    float total = 0f;
    for (int i = 0; i < rules.Length; i++) {
      total += rules[i].rate;
    }
    if (total <= 0) {
      total = 1f;
    }
    float rate = total;
    for (int i = rules.Length; i-- > 0;) {
      if (i > 0) {
        rules[i].rate = rules[i].rate / total;
        rate -= rules[i].rate;
      }
    }
    rules[0].rate = rate;
  }

  public char[] GetRuleF(float rate) {
    return GetRule(ruleFs, rate);
  }

  public char[] GetRuleG(float rate) {
    return GetRule(ruleGs, rate);
  }

  private char[] GetRule(Rule[] rules, float rate) {
    for (int i = 0; i < rules.Length; i++) {
      if (rules[i].rate >= rate) {
        return rules[i].ToCharArrayRule();
      }
      rate -= rules[i].rate;
    }
    return rules[rules.Length - 1].ToCharArrayRule();
  }

  /// <summary>
  /// L-System rules
  /// </summary>
  [Serializable]
  public class Rule {
    public float rate = 1f;
    public string rule = null;

    [NonSerialized]
    private char[] ruleCache = null;
    public char[] ToCharArrayRule() {
      if (rule != null && ruleCache == null) {
        ruleCache = rule.ToUpper().ToCharArray();
      }
      return ruleCache;
    }
  }
}
