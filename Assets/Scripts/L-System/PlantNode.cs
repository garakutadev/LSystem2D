using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantNode {
  public enum NodeType : int {
    Node = 0,
    Origin,
    Terminal,

    Leaf,       //Terminalの後にSpriteサイズに合わせて設定
  }
  private NodeType nodeType = NodeType.Node;
  public NodeType type {
    get { return nodeType; }
  }

  private int index;
  public int Index {
    get { return index; }
  }

  private int depth;
  public int Depth {
    get { return depth; }
  }

  private PlantNode parent = null;
  private List<PlantNode> childNodes = null;
  public List<PlantNode> children {
    get {
      if (childNodes == null) {
        childNodes = new List<PlantNode>();
      }
      return childNodes;
    }
  }

  private float growLength = 0f;
  private float thickness;

  private static readonly Vector3 vecUp = Vector3.up;
  private static readonly Vector3 vecDown = Vector3.down;

  private static readonly Vector3 vecLeft = Vector3.left;
  private static readonly Vector3 vecRight = Vector3.right;

  private static readonly Vector3 posZero = Vector3.zero;
  private static readonly Vector3 scaleOne = Vector3.one;

  private Transform refTransform = null;
  public Transform transform {
    get {
      return refTransform;
    }
  }
  public PlantNode(Transform trans) {
    refTransform = trans;
  }
  
  public PlantNode SetNode(int _index, int _depth, float _thickness, NodeType ntype, PlantNode _parent = null) {
    parent = _parent;
    growLength = 0f;
    index = _index;
    depth = _depth;
    nodeType = ntype;
    thickness = _thickness;

    if (parent != null) {
      transform.SetParent(parent.transform, true);
      transform.localPosition = posZero;
      transform.localScale = scaleOne;
      parent.children.Add(this);
    }

    transform.name = ntype.ToString() + "_" + Index;
    return this;
  }

  /// <summary>
  /// 末端側の接合部情報
  /// </summary>
  private JointPoints jointP = JointPoints.zero;
  private Vector3 posCurrent = posZero;
  public void SetNodeJointPoints(Vector3 posRoot, float scale) {
    float jointThickness = thickness * scale;
    var pos = transform.position - posRoot;
    var quat = transform.rotation;
    var vecR = vecRight * jointThickness;
    var vecL = vecLeft * jointThickness;

    posCurrent = pos;
    jointP = new JointPoints(pos + quat * vecR, pos + quat * vecL);

    var nodes = children;
    int len = nodes.Count;
    if (len <= 0) {
      return;
    }
    for (int i = 0; i < len; i++) {
      nodes[i].SetChildJointPoints(jointP, posRoot, scale);
    }
  }

  /// <summary>
  /// 根側の接合部座標情報
  /// </summary>
  private JointPoints jointPUp = JointPoints.zero;      //親側接点を起点に上側
  private JointPoints jointPBase = JointPoints.zero;    //親側接点
  private JointPoints jointPDown = JointPoints.zero;    //親側接点を起点に下側
  public void SetChildJointPoints(JointPoints jp, Vector3 posRoot, float scale) {
    if (Depth <= 1) {
      jointPUp = jointPDown = jointPBase = jp;
      return;
    }

    float jointThickness = thickness * scale;
    var p = transform.parent.position - posRoot;
    var q = transform.rotation;
    var qp = transform.parent.rotation;

    var vecRU = (vecRight + vecUp * 0.5f) * jointThickness;
    var vecLU = (vecLeft + vecUp * 0.5f) * jointThickness;

    var vecRD = (vecRight + vecDown * 0.5f) * jointThickness;
    var vecLD = (vecLeft + vecDown * 0.5f) * jointThickness;

    //Up
    var jpu = new JointPoints(p + q * vecRU, p + q * vecLU);
    var jpup = new JointPoints(p + qp * vecRU, p + qp * vecLU);
    jointPUp = new JointPoints(
        Vector3.Lerp(jpu.right, jpup.right, RateUp),
        Vector3.Lerp(jpu.left, jpup.left, RateUp));
    
    //Down
    var jpd = new JointPoints(p + q * vecRD, p + q * vecLD);
    var jpdp = new JointPoints(p + qp * vecRD, p + qp * vecLD);
    jointPDown = new JointPoints(
        Vector3.Lerp(jpd.right, jpdp.right, RateDown),
        Vector3.Lerp(jpd.left, jpdp.left, RateDown));

    //Mid
    jointPBase = jp;
  }
  private const float RateUp = 0.25f;
  private const float RateDown = 1f - RateUp;

  /// <summary>
  /// Mesh vertices
  /// </summary>
  public Vector3[] GetVertices() {
    //lt, lb, rt, rb
    if (type == NodeType.Node) {
      if (parent.nodeType == NodeType.Origin) {
        return new Vector3[]{
          jointPUp.left, jointPUp.right,
          jointPBase.left, jointPBase.right,
          jointPBase.left, jointPBase.right,
          jointPDown.left, jointPDown.right,
          parent.jointP.left, parent.jointP.right,
        };
      }

      return new Vector3[]{
        jointPUp.left, jointPUp.right,
        jointPBase.left, jointPBase.right,
        jointPBase.left, jointPBase.right,
        jointPDown.left, jointPDown.right,
        parent.jointPUp.left, parent.jointPUp.right,
      };
    } else {
      if (type == NodeType.Terminal) {
        return new Vector3[]{
          jointP.left, jointP.right,
          parent.jointPUp.left, parent.jointPUp.right,
        };
      }
    }
    return null;
  }

  /// <summary>
  /// Mesh triangles
  /// </summary>
  public int[] GetTriangles(int idx) {
    if (type == NodeType.Node) {
      //Clock wise
      return new int[] {
        idx + 0, idx + 1, idx + 2,  idx + 1, idx + 3, idx + 2,  //0.B - 1.0 : bottom-base
        idx + 4, idx + 5, idx + 6,  idx + 5, idx + 7, idx + 6,  //0.0 - 0.T : base-top
        idx + 6, idx + 7, idx + 8,  idx + 7, idx + 9, idx + 8,  //0.T - 0.B : top-bottom
      };
    } else {
      if (type == NodeType.Terminal) {
        return new int[] {
          idx + 0, idx + 1, idx + 2,  idx + 1, idx + 3, idx + 2,  //0.0 - 0.B : head-bottom
        };
      }
    }
    return null;
  }

  private const float UVTop = 0.1f;//RateUp;       //0.1f;
  private const float UVBottom = 1.0f - UVTop;

  /// <summary>
  /// Mesh Uvs
  /// </summary>
  public Vector2[] GetUV(Vector2 ltUV, Vector2 rtUV, Vector2 lbUV, Vector2 rbUV) {
    float lx = ltUV.x;
    float rx = rtUV.x;
    float y = ltUV.y;
    float h = lbUV.y - y;

    if (nodeType == NodeType.Node) {
      if (parent.nodeType == NodeType.Origin) {
        return new Vector2[] {
          //for vertex uv
          new Vector2(lx, y + h * UVBottom), new Vector2(rx, y + h * UVBottom),   //0, 1
          new Vector2(lx, y + h), new Vector2(rx, y + h),                         //2, 3
          new Vector2(lx, y), new Vector2(rx, y),                                 //4, 5
          new Vector2(lx, y + h * UVTop), new Vector2(rx, y + h * UVTop),         //6, 7
          new Vector2(lx, y + h), new Vector2(rx, y + h),                         //8, 9
        };
      }
      return new Vector2[] {
        //for vertex uv
        new Vector2(lx, y + h * UVBottom), new Vector2(rx, y + h * UVBottom),   //0, 1
        new Vector2(lx, y + h), new Vector2(rx, y + h),                         //2, 3
        new Vector2(lx, y), new Vector2(rx, y),                                 //4, 5
        new Vector2(lx, y + h * UVTop), new Vector2(rx, y + h * UVTop),         //6, 7
        new Vector2(lx, y + h * UVBottom), new Vector2(rx, y + h * UVBottom),   //8, 9
      };
    } else {
      if (nodeType == NodeType.Terminal) {
        return new Vector2[] {
          new Vector2(lx, y), new Vector2(rx, y),                               //0, 1
          new Vector2(lx, y + h * UVBottom), new Vector2(rx, y + h * UVBottom), //2, 3
        };
      }
    }
    return new Vector2[] { ltUV, rtUV, lbUV, rbUV };
  }

  /// <summary>
  /// Mesh Uvs
  /// </summary>
  public Vector2[] GetUV(Vector2[] baseUV) {
    return GetUV(baseUV[0], baseUV[1], baseUV[2], baseUV[3]);
  }

  /// <summary>
  /// Convert sprite mesh vertex
  /// </summary>
  public Vector3[] SpriteToVerteces(Sprite sprite, float scale, float zOffset) {
    //lt, rt, lb, rb
    var v = sprite.vertices;
    return new Vector3[] {
      Vec2ToVec3(v[0], scale, zOffset) + posCurrent,
      Vec2ToVec3(v[1], scale, zOffset) + posCurrent,
      Vec2ToVec3(v[2], scale, zOffset) + posCurrent,
      Vec2ToVec3(v[3], scale, zOffset) + posCurrent,
    };
  }
  private static Vector3 Vec2ToVec3(Vector2 v, float scale, float zOffset) {
    return new Vector3(v.x * scale, v.y * scale, zOffset);
  }

  /// <summary>
  /// Convert sprite mesh triangles
  /// </summary>
  public int[] SpriteToTriangles(Sprite sprite, int idx) {
    return new int[] {
      idx + 0, idx + 1, idx + 2,  idx + 1, idx + 3, idx + 2,  //0.0 - 1.0
    };
  }

  /// <summary>
  /// BoneWeights 
  /// </summary>
  public BoneWeight[] FullBoneWeights() {
    var bw = new BoneWeight();
    bw.boneIndex0 = Index;
    bw.weight0 = 1.0f;
    return new BoneWeight[] { bw, bw, bw, bw};
  }

  /// <summary>
  /// BoneWeights combine node
  /// </summary>
  public BoneWeight[] GetBoneWeights() {
    //lt, lb, rt, rb
    if (type == NodeType.Node) {
      BoneWeight bwUp = new BoneWeight();
      bwUp.boneIndex0 = parent.Index;
      bwUp.weight0 = 1f;

      BoneWeight bwMid = new BoneWeight();
      bwMid.boneIndex0 = parent.Index;
      bwMid.weight0 = 1f;

      BoneWeight bwBottom = new BoneWeight();
      BoneWeight bwP = new BoneWeight();
      if (parent.nodeType == NodeType.Origin) {
        bwBottom.boneIndex0 = parent.Index;
        bwBottom.weight0 = 1f;

        bwP.boneIndex0 = parent.Index;
        bwP.weight0 = 1f;
      } else {
        bwBottom.boneIndex0 = parent.Index;
        bwBottom.boneIndex1 = parent.parent.Index;
        bwBottom.weight0 = RateUp;
        bwBottom.weight1 = RateDown;

        bwP.boneIndex0 = parent.parent.Index;
        bwP.weight0 = 1f;
      }

      return new BoneWeight[]{
        bwUp, bwUp,
        bwMid, bwMid,
        bwMid, bwMid,
        bwBottom, bwBottom,
        bwP, bwP,
      };
    } else {
      if (type == NodeType.Terminal) {
        BoneWeight bwHead = new BoneWeight();
        bwHead.boneIndex0 = parent.parent.Index;
        bwHead.weight0 = 1f;

        BoneWeight bwUp = new BoneWeight();
        bwUp.boneIndex0 = parent.parent.Index;
        bwUp.boneIndex1 = parent.parent.parent.Index;
        bwUp.weight0 = RateDown;
        bwUp.weight1 = RateUp;

        return new BoneWeight[]{
          bwHead, bwHead,
          bwUp, bwUp,
        };
      }
      if (type == NodeType.Leaf) {
        return FullBoneWeights();
      }
      if (type == NodeType.Origin) {

      }
    }
    return null;
  }

  /// <summary>
  /// node joint points(for mesh vertex source)
  /// </summary>
  public struct JointPoints {
    public Vector3 right;
    public Vector3 left;
    public JointPoints(Vector3 r, Vector3 l) {
      this.right = r;
      this.left = l;
    }
    public static JointPoints zero {
      get { return zeroRig; }
    }
    private static JointPoints zeroRig = new JointPoints(Vector3.zero, Vector3.zero);
  }

  public PlantNode SetGrowLength(float growLen) {
    growLength += growLen;
    Vector3 pos = transform.localRotation * vecUp * growLength;
    transform.localPosition = pos;
    return this;
  }

#if false
  public void DrawGizmos() {
    Color defColor = Gizmos.color;
    Gizmos.color = Color.yellow;
    var pos = transform.position;
    var posParent = Vector3.zero;
    if (transform.parent != null) {
      posParent = transform.parent.position;
    }
    Gizmos.DrawLine(pos, posParent);
    if (nodeType == NodeType.Terminal) {
      Gizmos.color = Color.white;
    } else {
      Gizmos.color = Color.red;
    }
    Gizmos.DrawWireSphere(pos, 0.02f);

    //current joints
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(jointP.right, RadiusJointPoint);
    Gizmos.DrawWireSphere(jointP.left, RadiusJointPoint);

    //current - parent joints
    if (parent != null) {
      if (nodeType == NodeType.Node) {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(jointPUp.right, RadiusJointPoint);
        Gizmos.DrawWireSphere(jointPUp.left, RadiusJointPoint);

        Gizmos.DrawWireSphere(jointPBase.right, RadiusJointPoint);
        Gizmos.DrawWireSphere(jointPBase.left, RadiusJointPoint);

        Gizmos.DrawWireSphere(jointPDown.right, RadiusJointPoint);
        Gizmos.DrawWireSphere(jointPDown.left, RadiusJointPoint);

        Gizmos.DrawLine(jointPUp.right, jointPBase.right);
        Gizmos.DrawLine(jointPUp.left, jointPBase.left);

        Gizmos.DrawLine(jointPBase.right, jointPDown.right);
        Gizmos.DrawLine(jointPBase.left, jointPDown.left);

        //Gizmos.color = Color.cyan;
        Gizmos.DrawLine(jointPDown.right, parent.jointPUp.right);
        Gizmos.DrawLine(jointPDown.left, parent.jointPUp.left);
      }
      if (nodeType == NodeType.Terminal) {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(jointP.right, parent.jointPUp.right);
        Gizmos.DrawLine(jointP.left, parent.jointPUp.left);
      }
      if (nodeType == NodeType.Leaf) {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos + Vector3.up * 0.01f, sizeLeafBox);
      }
    }
    Gizmos.color = defColor;
  }
  private const float RadiusJointPoint = 0.001f;
  private readonly static Vector2 sizeLeafBox = new Vector2(0.2f, 0.1f);
#endif
}
