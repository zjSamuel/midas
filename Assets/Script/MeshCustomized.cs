using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshCustomized : MonoBehaviour
{
    private Mesh myMesh;
    public Vector3[] myVertices = null;
    public int[] myTriangles = null;
    Color[] colors;

    public Color edgeColor = Color.blue;
    public float lineWidth = 0.02f;
    private Material lineMaterial;
    // Start is called before the first frame update
    void Start()
    {
        myMesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh = myMesh;
        colors = new Color[myMesh.vertexCount];
        myMesh.name = "111";
        myVertices = new Vector3[]
        {
            new Vector3 (-1, 0, 0),
            new Vector3 (0, 2, 0),
            new Vector3 (3, 0, 0),
            new Vector3 (0, 0, -1),
            new Vector3 (0, 0, 0),
        };
        myTriangles = new int[]
        {
            0, 2, 1,
            0, 1, 3,
            0, 3, 2,
            1, 2, 3,
            4, 2, 1,
        };

        CreateLineMaterial();
    }

    // Update is called once per frame
    void Update()
    {
        myMesh.Clear();
        myMesh.vertices = myVertices;
        myMesh.triangles = myTriangles;

    }

    //�߿����
    void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnRenderObject()
    {
        if (myMesh == null) return;

        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(edgeColor);

        // �������бߣ������е������ߣ�
        int[] triangles = myMesh.triangles;
        Vector3[] vertices = myMesh.vertices;

        // �������������α�
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            DrawThickLine(vertices[i1], vertices[i2]);
            DrawThickLine(vertices[i2], vertices[i3]);
            DrawThickLine(vertices[i3], vertices[i1]);
        }

        // �ر�����е������ߣ�ȷ�����ǲ������ԣ�
        DrawThickLine(vertices[4], vertices[0]);  // �е�-����0
        DrawThickLine(vertices[4], vertices[1]);  // �е�-����1
        DrawThickLine(vertices[4], vertices[2]);  // �е�-����2
        DrawThickLine(vertices[4], vertices[3]);  // �е�-����3

        GL.End();
        GL.PopMatrix();
    }

    void DrawThickLine(Vector3 start, Vector3 end)
    {
        GL.Vertex(start);
        GL.Vertex(end);
    }
}