using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshCustomized : MonoBehaviour
{
    private Mesh myMesh;

    // 顶点 & 三角形
    public Vector3[] myVertices;
    public int[] myTriangles;

    // 线框材质
    public Color edgeColor = Color.blue;
    public Color selectedFaceColor = Color.red; // 选中面高亮色
    public float lineWidth = 1;                 // 若想用 GL.QUADS 自己画粗线可以用到
    private Material lineMaterial;

    // 交互控制
    public float rotateSpeed = 5f;
    public float zoomSpeed   = 5f;
    private int   pickedTriangle = -1;          // 记录被点中的三角形索引

    #region Unity 回调
    void Start ()
    {
        //--------------------------------------
        // 1. 构造 Mesh
        //--------------------------------------
        myMesh = new Mesh();
        myMesh.name = "RuntimeMesh";
        myVertices = new Vector3[]
        {
            new Vector3 (-1, 0, 0),   //0
            new Vector3 ( 0, 2, 0),   //1
            new Vector3 ( 3, 0, 0),   //2
            new Vector3 ( 0, 0,-1),   //3
            new Vector3 ( 0, 0, 0),   //4
        };
        myTriangles = new int[]
        {
            0, 2, 1,
            0, 1, 3,
            0, 3, 2,
            1, 2, 3,
            4, 2, 1
        };
        ApplyMeshData();             // 把数据塞进 Mesh

        // 挂到 MeshFilter & MeshCollider
        var mf = GetComponent<MeshFilter>();
        mf.mesh = myMesh;
        var mc = GetComponent<MeshCollider>();
        mc.sharedMesh = myMesh;

        //--------------------------------------
        // 2. GL 用材质
        //--------------------------------------
        CreateLineMaterial();
    }

    void Update ()
    {
        HandleMouseRotate();
        HandleMouseZoom();
        HandleMousePick();
    }
    #endregion

    #region Mesh & 绘制
    void ApplyMeshData ()
    {
        myMesh.Clear();
        myMesh.vertices  = myVertices;
        myMesh.triangles = myTriangles;
        myMesh.RecalculateNormals();
    }

    void CreateLineMaterial ()
    {
        if (lineMaterial != null) return;

        var shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;

        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull",     (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterial.SetInt("_ZWrite",   0);
    }

    // OnRenderObject 在所有摄像机完成渲染后调用
    void OnRenderObject ()
    {
        if (myMesh == null) return;

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        Vector3[] v  = myMesh.vertices;
        int[]     tri= myMesh.triangles;

        //--------- 1) 画线框 ----------
        GL.Begin(GL.LINES);
        GL.Color(edgeColor);
        for (int i = 0; i < tri.Length; i += 3)
        {
            DrawLine(v[tri[i]],   v[tri[i+1]]);
            DrawLine(v[tri[i+1]], v[tri[i+2]]);
            DrawLine(v[tri[i+2]], v[tri[i]]);
        }
        GL.End();

        //--------- 2) 高亮选中面 ----------
        if (pickedTriangle >= 0 && pickedTriangle*3 + 2 < tri.Length)
        {
            GL.Begin(GL.TRIANGLES);
            GL.Color(selectedFaceColor);

            GL.Vertex(v[tri[pickedTriangle*3]]);
            GL.Vertex(v[tri[pickedTriangle*3+1]]);
            GL.Vertex(v[tri[pickedTriangle*3+2]]);
            GL.End();
        }

        GL.PopMatrix();
    }

    void DrawLine (Vector3 a, Vector3 b)
    {
        GL.Vertex(a);
        GL.Vertex(b);
    }
    #endregion

    #region 鼠标交互
    void HandleMouseRotate ()
    {
        if (Input.GetMouseButton(1)) // 右键旋转
        {
            float dx = Input.GetAxis("Mouse X") * rotateSpeed;
            float dy = -Input.GetAxis("Mouse Y") * rotateSpeed;

            // 围绕世界 Y 轴水平旋转，围绕本地 X 轴竖直旋转
            transform.Rotate(Vector3.up,   dx, Space.World);
            transform.Rotate(Vector3.right,dy, Space.Self);
        }
    }

    void HandleMouseZoom ()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            Camera cam = Camera.main;
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Max(0.1f, cam.orthographicSize - scroll * zoomSpeed);
            }
            else
            {
                cam.transform.Translate(Vector3.forward * scroll * zoomSpeed, Space.Self);
            }
        }
    }

    void HandleMousePick ()
    {
        if (Input.GetMouseButtonDown(0))   // 左键点击
        {
            Camera cam = Camera.main;
            if (!cam) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 确保点到的就是当前物体
                if (hit.collider.gameObject == gameObject)
                {
                    pickedTriangle = hit.triangleIndex; // triangleIndex 就是第几个三角
                }
                else
                {
                    pickedTriangle = -1;
                }
            }
            else
            {
                pickedTriangle = -1;
            }
        }
    }
    #endregion
}