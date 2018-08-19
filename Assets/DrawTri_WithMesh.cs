using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTri_WithMesh : MonoBehaviour
{
    [SerializeField]
    private Material mat;

    [SerializeField]
    private float width;

    [SerializeField]
    private float height;

    // Use this for initialization
    void Start ( )
    {
        transform.position = new Vector3 ( -width / 2, -height / 2, 0 );
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        // Verts
        Vector3[] verts = new Vector3[3];
        verts [ 0 ] = new Vector3 ( 0, 0, 0 );
        verts [ 1 ] = new Vector3 ( 1, 0, 0 );
        verts [ 2 ] = new Vector3 ( 0, 1, 0 );

        mesh.vertices = verts;

        // Tris
        int[] tris = new int[3];
        tris [ 0 ] = 0;
        tris [ 1 ] = 2;
        tris [ 2 ] = 1;

        mesh.triangles = tris;

        // Normals
        Vector3[] normals = new Vector3[3];
        normals [ 0 ] = -Vector3.forward;
        normals [ 1 ] = -Vector3.forward;
        normals [ 2 ] = -Vector3.forward;

        mesh.normals = normals;

        // UVs
        Vector2[] uvs = new Vector2[3];
        uvs [ 0 ] = new Vector2 ( 0, 0 );
        uvs [ 1 ] = new Vector2 ( 1, 0 );
        uvs [ 2 ] = new Vector2 ( 0, 1 );

        mesh.uv = uvs;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = mat;


    }

}
