using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DrawTri_Procedural : MonoBehaviour
{
    private struct Point
    {
        public Vector3 vert;
        public Vector3 nor;
        // public Vector4 tan;
        public Vector2 uv;
    }

    [SerializeField]
    private Material mat;

    [SerializeField]
    private float width;

    [SerializeField]
    private float height;


    private ComputeBuffer computeBuffer;

    private int n = 3;

    // Use this for initialization
    void Update ( )
    {
        // Verts
        Vector3[] verts = new Vector3[n];
        verts [ 0 ] = new Vector3 ( 0, 0, 0 );
        verts [ 1 ] = new Vector3 ( width, 0, 0 );
        verts [ 2 ] = new Vector3 ( 0, height, 0 );
        // verts [ 3 ] = new Vector3 ( width, height, 0 );

        // Normals
        Vector3[] normals = new Vector3[n];
        normals [ 0 ] = transform.forward;
        normals [ 1 ] = transform.forward;
        normals [ 2 ] = transform.forward;
        // normals [ 3 ] = -Vector3.forward;

        // Tan?
        Vector4[] tans = new Vector4[n];
        tans [ 0 ] = new Vector4 ( verts [ 1 ].x - verts [ 0 ].x, verts [ 1 ].y - verts [ 0 ].y, verts [ 1 ].z - verts [ 0 ].z, 1.0f );
        tans [ 1 ] = new Vector4 ( verts [ 2 ].x - verts [ 1 ].x, verts [ 2 ].y - verts [ 1 ].y, verts [ 2 ].z - verts [ 1 ].z, 1.0f );
        tans [ 2 ] = new Vector4 ( verts [ 0 ].x - verts [ 2 ].x, verts [ 0 ].y - verts [ 2 ].y, verts [ 0 ].z - verts [ 2 ].z, 1.0f );
        // tans [ 3 ] = new Vector4 ( verts [ 0 ].x - verts [ 3 ].x, verts [ 0 ].y - verts [ 3 ].y, verts [ 0 ].z - verts [ 3 ].z, 1.0f );

        // UVs
        Vector2[] uvs = new Vector2[n];
        uvs [ 0 ] = new Vector2 ( 0, 0 );
        uvs [ 1 ] = new Vector2 ( 1, 0 );
        uvs [ 2 ] = new Vector2 ( 0, 1 );
        // uvs [ 3 ] = new Vector2 ( 1, 1 );


        Point[] points = new Point[n];
        for ( int i = 0; i < n; i++ )
        {
            points [ i ].vert = verts [ i ];
            points [ i ].nor = normals [ i ];
            // points [ i ].tan = tans [ i ];
            points [ i ].uv = uvs [ i ];
        }

        computeBuffer = new ComputeBuffer ( n, Marshal.SizeOf ( typeof ( Point ) ), ComputeBufferType.Default );
        computeBuffer.SetData ( points );
        mat.SetBuffer ( "points", computeBuffer );
    }


    private void OnRenderObject ( )
    {
        mat.SetPass ( 0 );
        Graphics.DrawProcedural ( MeshTopology.Triangles, n, 1 );
    }

    private void OnDestroy ( )
    {
        computeBuffer.Release ( );
    }
}
