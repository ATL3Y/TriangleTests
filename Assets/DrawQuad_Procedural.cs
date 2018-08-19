using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DrawQuad_Procedural : MonoBehaviour
{
    private struct Point
    {
        public Vector3 vert;
        public Vector3 nor;
        public Vector2 uv;
    }

    [SerializeField]
    private Material mat;

    [SerializeField]
    private float width;

    [SerializeField]
    private float height;

    private int n = 6;

    private ComputeBuffer computeBuffer;

    void Update ( )
    {

        // Verts
        Vector3[] verts = new Vector3[n];
        verts [ 0 ] = new Vector3 ( 0, 0, 0 );
        verts [ 1 ] = new Vector3 ( width, 0, 0 );
        verts [ 2 ] = new Vector3 ( 0, height, 0 );
        verts [ 3 ] = new Vector3 ( 0, height, 0 );
        verts [ 4 ] = new Vector3 ( width, 0, 0 );
        verts [ 5 ] = new Vector3 ( width, height, 0 );

        // Normals
        Vector3[] normals = new Vector3[n];
        normals [ 0 ] = transform.forward;
        normals [ 1 ] = transform.forward;
        normals [ 2 ] = transform.forward;
        normals [ 3 ] = transform.forward;
        normals [ 4 ] = transform.forward;
        normals [ 5 ] = transform.forward;

        // UVs
        Vector2[] uvs = new Vector2[n];
        uvs [ 0 ] = new Vector2 ( 0, 0 );
        uvs [ 1 ] = new Vector2 ( 1, 0 );
        uvs [ 2 ] = new Vector2 ( 0, 1 );
        uvs [ 3 ] = new Vector2 ( 0, 1 );
        uvs [ 4 ] = new Vector2 ( 1, 0 );
        uvs [ 5 ] = new Vector2 ( 1, 1 );

        Point[] points = new Point[n];
        for ( int i = 0; i < n; i++ )
        {
            points [ i ].vert = verts [ i ];
            points [ i ].nor = normals [ i ];
            points [ i ].uv = uvs [ i ];
        }

        computeBuffer = new ComputeBuffer ( n, Marshal.SizeOf ( typeof ( Point ) ), ComputeBufferType.Default );
        computeBuffer.SetData ( points );
        mat.SetBuffer ( "points", computeBuffer );
    }


    private void OnRenderObject ( )
    {
        mat.SetPass ( 0 );
        Graphics.DrawProcedural ( MeshTopology.Triangles, n, 1 ); // Works for all but MeshTopology.Points
    }

    private void OnDestroy ( )
    {
        computeBuffer.Release ( );
    }
}
