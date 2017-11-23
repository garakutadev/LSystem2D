using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Plant))]
public class PlantEditor : Editor {
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();
    Plant plant = target as Plant;
    if (GUILayout.Button("Generate L-System Plant")) {
      plant.GeneratePlant();
    }
  }
}
