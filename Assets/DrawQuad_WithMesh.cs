using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawQuad_WithMesh : MonoBehaviour
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

        Vector3[] verts = new Vector3 [ 4 ];
        verts [ 0 ] = new Vector3 ( 0, 0, 0 );
        verts [ 1 ] = new Vector3 ( width, 0, 0 );
        verts [ 2 ] = new Vector3 ( 0, height, 0 );
        verts [ 3 ] = new Vector3 ( width, height, 0 );

        mesh.vertices = verts;

        int[] tri = new int[6];
        tri [ 0 ] = 0;
        tri [ 1 ] = 2;
        tri [ 2 ] = 1;
        tri [ 3 ] = 2;
        tri [ 4 ] = 3;
        tri [ 5 ] = 1;

        mesh.triangles = tri;

        Vector3[] normals = new Vector3[4];
        normals [ 0 ] = new Vector3 ( 0, 0, -1 );
        normals [ 1 ] = new Vector3 ( 0, 0, -1 );
        normals [ 2 ] = new Vector3 ( 0, 0, -1 );
        normals [ 3 ] = new Vector3 ( 0, 0, -1 );

        mesh.normals = normals;

        Vector2[] uv = new Vector2[4];
        uv [ 0 ] = new Vector2 ( 0, 0 );
        uv [ 1 ] = new Vector2 ( 1, 0 );
        uv [ 2 ] = new Vector2 ( 0, 1 );
        uv [ 3 ] = new Vector2 ( 1, 1 );

        mesh.uv = uv;

        MeshRenderer rend = gameObject.AddComponent<MeshRenderer>();
        rend.material = mat;
    }
}
