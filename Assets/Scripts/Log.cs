#if UNITY_EDITOR
  #define DEBUG
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// デバッグ用ログ機能（画面へのログ出力に使用します）
/// </summary>
public class Log : MonoBehaviour {
  /// <summary>
  /// シングルトン
  /// </summary>
  private static Log logInstance = null;
  public static Log Instance {
    get {
      if (logInstance == null) {
        logInstance = (Log)FindObjectOfType(typeof(Log));
        if (logInstance == null) {
          GameObject go = new GameObject("Log");
          logInstance = go.AddComponent<Log>();
        }
      }
      return logInstance;
    }
  }

  private bool isActive = false;
  private const int MaxRows = 20;
  private List<string> logMessages = new List<string>(MaxRows);

  #pragma warning disable 0414
  private string message;
  private Rect rectLog = new Rect(0, 0, 100, 100);

  private const float TimeLogStay = 5.0f;   //表示時間
  private float waitLog = 0;
  #pragma warning restore 0067


  private void SetLog() {
    string buf = "";
    foreach (string s in logMessages) {
      buf += s + "\n";
    }
    message = buf;
  }

  private enum LogType : int {
    Message = 0,
    Warning,
    Error,
  }

  private void SetMessage(string msg, LogType logType = LogType.Message) {
    if (!isActive) {
      return;
    }

    if (logMessages.Count >= MaxRows) {
      logMessages.RemoveAt(0);
    }
    switch (logType) {
    case LogType.Message:
      logMessages.Add(string.Copy(msg));
      break;
    case LogType.Warning:
      logMessages.Add("<color=yellow>" + string.Copy(msg) + "</color>");
      break;
    case LogType.Error:
      logMessages.Add("<color=red>" + string.Copy(msg) + "</color>");
      break;
    }
    SetLog();
    //WebPlayerはログ出力JavaScriptを呼び出す
#if UNITY_WEBPLAYER && !UNITY_EDITOR
    Application.ExternalCall("console.log", msg);
#endif

    gameObject.SetActive(true);
    waitLog = TimeLogStay;
  }

  /// <summary>
  /// ログ出力：通常
  /// </summary>
  /// <param name="log">出力するログ</param>
  public static void Output(string log) {
    //DEBUGフラグはPlayerSettingのOtherSettingで指定する
#if DEBUG
    Debug.Log(log);
    Instance.SetMessage(log);
#endif
  }

  /// <summary>
  /// ログ出力：警告
  /// </summary>
  /// <param name="log">出力するログ</param>
  public static void Warning(string log) {
#if DEBUG
    Debug.LogWarning(log);
    Instance.SetMessage(log, LogType.Warning);
#endif
  }

  /// <summary>
  /// ログ出力：エラー
  /// </summary>
  /// <param name="log">出力するログ</param>
  public static void Error(string log) {
#if DEBUG
    Debug.LogError(log);
    Instance.SetMessage(log, LogType.Error);
#endif
  }

  /// <summary>
  /// ログ出力：例外
  /// </summary>
  /// <param name="ex">出力する例外</param>
  public static void LogException(Exception ex) {
#if DEBUG
    Debug.LogException(ex);
    Instance.SetMessage(ex.ToString(), LogType.Error);
#endif
  }

  void Awake() {
    if (Instance != this) {
      Destroy(this.gameObject);
      return;
    }
    DontDestroyOnLoad(this.gameObject);
    rectLog = new Rect(0, 0, Screen.width, Screen.height);
    isActive = true;
  }

  void OnDisable() {
    isActive = false;
  }

#if DEBUG
  void Update() {
    if (isActive && waitLog > 0) {
      waitLog -= Time.deltaTime;
      if (waitLog <= 0) {
        logMessages.RemoveAt(0);
        if (logMessages.Count <= 0) {
          gameObject.SetActive(false);
        } else {
          SetLog();
          waitLog += 0.5f;
        }
      }
    }
  }

  void OnGUI() {
    if (isActive && (waitLog > 0) && (message != null)) {
      GUI.color = Color.white;
      GUI.Label(rectLog, message);
    }
  }
#endif
}
