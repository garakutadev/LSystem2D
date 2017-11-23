using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// L-Systemによる植物オブジェクト生成
/// </summary>
public class Plant : MonoBehaviour {
  [SerializeField]
  private PlantConfig refConfig = null;
  private PlantConfig config {
    get { return refConfig; }
  }

  [SerializeField]
  private int randomSeed = 0;

  private XORandom random = null;
  private Mesh plantMesh = null;

  /// <summary>
  /// ボーンの原点参照
  /// </summary>
  [SerializeField]
  private Transform refBoneOrigin = null;
  public Transform boneOrigin {
    get {
      if (refBoneOrigin == null) {
        var bone = (new GameObject("origin")).GetComponent<Transform>();
        bone.SetParent(this.transform, false);
        refBoneOrigin = bone;
      }
      return refBoneOrigin;
    }
  }
  private PlantNode nodeOrigin = null;

  private int maxDepth = 0;
  private float depthScale = 0f;

  private void ReleaseBones() {
    if (boneOrigin != null) {
      foreach (var t in boneOrigin.GetComponentsInChildren<Transform>()) {
        if (t != boneOrigin && t != null) {
#if UNITY_EDITOR
          DestroyImmediate(t.gameObject);
#else
          Destroy(t.gameObject);
#endif
        }
      }
    }
  }

  /// <summary>
  /// ボーン情報を持つSkinnedMesh描画用レンダラーの参照（SkinnedMeshRenderer）
  /// </summary>
  [SerializeField]
  private SkinnedMeshRenderer refMeshRenderer = null;
  public SkinnedMeshRenderer meshRenderer {
    get { return refMeshRenderer; }
  }

  /// <summary>
  /// 固定メッシュでの描画用レンダラーの参照（MeshRenderer用MeshFilter）
  /// </summary>
  [SerializeField]
  private MeshFilter refMeshFilter = null;
  public MeshFilter meshFilter {
    get { return refMeshFilter; }
  }

  void Start() {
    GeneratePlant();
  }

  void OnDestroy() {
    ReleaseMesh();
  }

  /// <summary>
  /// L-Systemによる植物生成実行
  /// </summary>
  public void GeneratePlant() {
    if (config == null) {
      return;
    }
    if (randomSeed != 0) {
      random = new XORandom(randomSeed);
    } else {
      random = new XORandom(XORandom.RandRange(0, 0x7fffffff));
    }

    maxDepth = 0;
    var dna = GenerateDNA();
    if (dna != null) {
      var nodes = CreateNodes(dna);

      ReleaseMesh();
      Transform[] bones = new Transform[nodes.Length];
      plantMesh = CreateMesh(nodes, ref bones);

      if (meshRenderer != null) {
        meshRenderer.rootBone = boneOrigin;
        meshRenderer.sharedMesh = plantMesh;
        meshRenderer.bones = bones;
      }
      if (meshFilter != null) {
        meshFilter.mesh = plantMesh;
      }
    }
  }

  private char[] GenerateDNA() {
    config.Normalize();
    System.Text.StringBuilder sb = null;
    var chs = config.initiator.ToCharArray();
    for (int gen = 0; gen < config.generations; gen++) {
      sb = new System.Text.StringBuilder();
      for (int i = 0; i < chs.Length; i++) {
        switch (chs[i]) {
        case 'F':   //Forward
          sb.Append(config.GetRuleF(random.Range(0f, 1f)));
          break;
        case 'G':   //Generate
          sb.Append(config.GetRuleG(random.Range(0f, 1f)));
          break;
        case '+':   //Left
        case '-':   //Right
        case '[':   //Push
        case ']':   //Pop
        default:
          sb.Append(chs[i]);
          break;
        }
      }
      chs = sb.ToString().ToCharArray();
    }
    if (sb != null) {
      return sb.ToString().ToCharArray();
    }
    return null;
  }

  private PlantNode[] CreateNodes(char[] dna) {
    if (nodeOrigin != null) {
      ReleaseBones();
      nodeOrigin = null;
    }
    var nodeList = new List<PlantNode>();
    nodeOrigin = new PlantNode(boneOrigin);
    nodeOrigin.SetNode(0, 0, config.thickness, PlantNode.NodeType.Origin);
    nodeOrigin.transform.name = "origin";
    nodeList.Add(nodeOrigin);

    DecodeNodes(nodeList, nodeOrigin, dna, 0, angleZero);
    depthScale = 0f;
    if (maxDepth > 0) {
      depthScale = (1.0f - config.tipScale) / maxDepth;
    }
    BuildNodes(nodeOrigin, boneOrigin.position);

    return nodeList.ToArray();
  }

  private static readonly Vector3 angleZero = Vector3.zero;

  private PlantNode AddNode(PlantNode parent, int index, Vector3 angle, PlantNode.NodeType nodeType) {
    var node = new PlantNode((new GameObject()).GetComponent<Transform>());
    node.SetNode(index, parent.Depth + 1, config.thickness, nodeType, parent);
    node.transform.localRotation = Quaternion.Euler(angle);
    return node;
  }

  /// <summary>
  /// 再帰：設定からノードを構成する
  /// </summary>
  private int DecodeNodes(List<PlantNode> nodeList, PlantNode parent, char[] dna, int readPos, Vector3 angle) {
    char prev = ' ', code;
    PlantNode node = parent;
    while (readPos < dna.Length) {
      switch (code = dna[readPos++]) {
      case 'F':
      case 'G':
        var len = Mathf.Lerp(config.distMin, config.distMax, random.Range(0f, 1f));
        node = AddNode(node, nodeList.Count, angle, PlantNode.NodeType.Node);
        nodeList.Add(node);
        node.SetGrowLength(len);
        angle = angleZero;
        break;

      case '[':
        readPos = DecodeNodes(nodeList, node, dna, readPos, angle);
        angle = angleZero;
        break;

      case ']':
        if (node.transform.childCount <= 0) {
          node = AddNode(node, nodeList.Count, angleZero, PlantNode.NodeType.Terminal);
          maxDepth = Mathf.Max(node.Depth, maxDepth);
          nodeList.Add(node);

          node = AddNode(node, nodeList.Count, angleZero, PlantNode.NodeType.Leaf);
          nodeList.Add(node);
        }
        return readPos;

      case '+':   //Left
        angle += Vector3.Lerp(config.angleMin, config.angleMax, random.Range(0f, 1f));
        angle = MathUtil.AngleRange360(angle);
        break;

      case '-':   //Right
        angle += -Vector3.Lerp(config.angleMin, config.angleMax, random.Range(0f, 1f));
        angle = MathUtil.AngleRange360(angle);
        break;

      default:
        //Log.Output("code:" + dna[readPos - 1]);
        break;
      }
      prev = code;
    }
    if ((prev != '[') && (node.transform.childCount <= 0)) {
      node = AddNode(node, nodeList.Count, angleZero, PlantNode.NodeType.Terminal);
      maxDepth = Mathf.Max(node.Depth, maxDepth);
      nodeList.Add(node);

      node = AddNode(node, nodeList.Count, angleZero, PlantNode.NodeType.Leaf);
      nodeList.Add(node);
    }

    return readPos;
  }

  /// <summary>
  /// 再帰：ノード構成から付属情報を生成する
  /// </summary>
  private void BuildNodes(PlantNode node, Vector3 posRoot) {
    float scale = 1.0f - depthScale * node.Depth;
    node.SetNodeJointPoints(posRoot, scale);
    var children = node.children;
    for (int i = children.Count; i-- > 0; ) {
      BuildNodes(children[i], posRoot);
    }
  }

  private void ReleaseMesh() {
    if (plantMesh != null) {
#if UNITY_EDITOR
      DestroyImmediate(plantMesh);
#else
      Destroy(plantMesh);
#endif
      plantMesh = null;
    }
  }

  private Mesh CreateMesh(PlantNode[] nodes, ref Transform[] bones) {
    List<Vector3> vertList = new List<Vector3>();
    List<int> triList = new List<int>();
    List<Vector2> uvList = new List<Vector2>();
    List<BoneWeight> bwList = new List<BoneWeight>();

    var nodeSprites = config.imgNodes;
    var leafSprites = config.imgLeaves;

    Matrix4x4[] matrix = new Matrix4x4[bones.Length];
    int len = nodes.Length;
    PlantNode node;
    Sprite sprite;
    int idx = 0;
    for (int i = 0; i < len; i++) {
      node = nodes[i];
      if (node.type == PlantNode.NodeType.Leaf) {
        sprite = leafSprites[random.Range(0, leafSprites.Length)];
        var spvertex = node.SpriteToVerteces(sprite, config.leafScale, config.leafZOffset);
        vertList.AddRange(spvertex);
        triList.AddRange(node.SpriteToTriangles(sprite, idx));
        uvList.AddRange(sprite.uv);
        bwList.AddRange(node.GetBoneWeights());
        idx += spvertex.Length;

        bones[node.Index] = node.transform;
        matrix[node.Index] = bones[node.Index].worldToLocalMatrix * transform.localToWorldMatrix;
        continue;
      }

      var vertex = node.GetVertices();
      if (vertex != null) {
        vertList.AddRange(vertex);
        triList.AddRange(node.GetTriangles(idx));
        uvList.AddRange(node.GetUV(nodeSprites[
            random.Range(0, nodeSprites.Length)].uv));
        bwList.AddRange(node.GetBoneWeights());
        idx += vertex.Length;
      }
      bones[node.Index] = node.transform;
      matrix[node.Index] = bones[node.Index].worldToLocalMatrix * transform.localToWorldMatrix;
    }

    Mesh mesh = new Mesh();
    mesh.vertices = vertList.ToArray();
    mesh.triangles = triList.ToArray();
    mesh.uv = uvList.ToArray();
    mesh.boneWeights = bwList.ToArray();
    mesh.bindposes = matrix;
    mesh.RecalculateNormals();

    return mesh;
  }

#if false
  void OnDrawGizmos() {
    DrawNodes(nodeOrigin);
  }
  private void DrawNodes(PlantNode node) {
    if (node == null) {
      return;
    }
    var nodes = node.children;
    if (nodes != null) {
      int len = nodes.Count;
      for (int i = nodes.Count; i-- > 0;) {
        DrawNodes(nodes[i]);
      }
    }
    node.DrawGizmos();
  }
#endif
}
