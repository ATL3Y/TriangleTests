using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class DrawQuad_Procedural_FromCenter : MonoBehaviour
{
    private struct Quad
    {
        public Vector2 id;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
        public Vector3 nor;
        public Color col;

        // Add physics later.
    }

    private struct Point
    {
        public Vector3 vert;
        public Vector3 nor;
        public Vector2 uv;
        public Color col;

        // Add physics later.
    }

    [SerializeField]
    private Material mat;

    // Makes a square.
    [SerializeField]
    private float scale;

    [SerializeField]
    private int quadCount;
    private int quadRoot;

    private ComputeBuffer computeBuffer;

    private Quad[][] quads;
    private Point[] verts;
    private Point[] points;

    private void Start ( )
    {
        // Make sure this is a good square.
        quadRoot = ( int ) Mathf.Ceil ( Mathf.Sqrt ( quadCount ) );
        print ( "quadRoot " + quadRoot );

        quadCount = ( int ) Mathf.Pow ( quadRoot, 2.0f );

        InitQuads ( );
        InitVerts ( );
        UpdateVerts ( );
        InitPoints ( );
        UpdatePoints ( );
        SetComputeBuffer ( points );
    }

    private void Update ( )
    {

        UpdatePos ( );
        UpdateVerts ( );
        UpdatePoints ( );
        SetComputeBuffer ( points );
        
    }

    private void UpdatePos ( )
    {
        for ( int i = 0; i < quadRoot; i++ )
        {
            for ( int j = 0; j < quadRoot; j++ )
            {
                // Evenly space quads in a square.
                quads [ i ] [ j ].pos += 0.01f * Mathf.Sin ( 0.1f * i * Time.timeSinceLevelLoad ) * transform.up + 0.02f * Mathf.Sin( 0.2f * j * Time.timeSinceLevelLoad ) * transform.right;
            }
        }
    } 

    private void InitQuads ( )
    {
        // Make double array of quads to make a square.
        quads = new Quad [ quadRoot ] [ ];
        for ( int i = 0; i < quads.Length; i++ )
        {
            quads [ i ] = new Quad [ quadRoot ];
        }

        float offset = scale / quadRoot;

        // Assign position based on the scale.  Scale of 1 = 1 unit. 
        // Start from 0,0 = lower left, then center this from the transform. 
        for ( int i = 0; i < quadRoot; i++ )
        {
            for ( int j = 0; j < quadRoot; j++ )
            {
                // Give an id / uv.
                quads [ i ] [ j ].id = new Vector2 ( i, j );

                // Evenly space quads in a square.
                quads [ i ] [ j ].pos = transform.position + i * offset * transform.up + j * offset * transform.right;

                // Place this transform at the center of the square. 
                quads [ i ] [ j ].pos -= scale / 2.0f * transform.up + scale / 2.0f * transform.right;

                // Rotate to the camera.  Note this is from the center.
                Vector3 toCam = Camera.main.transform.position - transform.position;
                quads [ i ] [ j ].rot = transform.rotation * Quaternion.LookRotation ( toCam );

                // Give a random scale.
                float randScale = Random.Range(0.1f, 1.0f);
                quads [ i ] [ j ].scale = new Vector3 ( randScale, randScale, randScale );

                // For now, the normal matches the rotation. 
                quads [ i ] [ j ].nor = toCam;

                // Color is debug. Row down goes through the rainbow, colume over goes from light to dark. 
                float j01 = j / (quadRoot - 1.0f); // As j increases, the color gets more light.
                print ( j01 );
                float i01 = i / (quadRoot - 1.0f);
                float r = i01; // As i increases, r increases.
                float g = j01; 
                float b = 1.0f - i01; // As i increases, b decreases.
                Color col = new Color ( r, g, b, 1.0f ); // As j increases, color gets darker.
                print ( "col: " + col );
                quads [ i ] [ j ].col = col;
            }
        }

        // print ( "quad count = " + quads.Length * quads[0].Length );
    }

    private void InitVerts ( )
    {
        // Each quad gets four verts.
        verts = new Point [ 4 * quadCount ];

    }

    private void UpdateVerts ( )
    {
        int count = 0;

        for ( int i = 0; i < quadRoot; i++ )
        {
            for ( int j = 0; j < quadRoot; j++ )
            {
                UpdateVert ( count, i, j, new Vector2 ( 0.0f, 0.0f ) ); // 00
                UpdateVert ( count + 1, i, j, new Vector2 ( 1.0f, 0.0f ) ); // 10
                UpdateVert ( count + 2, i, j, new Vector2 ( 0.0f, 1.0f ) ); // 01
                UpdateVert ( count + 3, i, j, new Vector2 ( 1.0f, 1.0f ) ); // 11
                count += 4;
            }
        }

        // print ( "verts count = " + count + ", verts.length = " + verts.Length );
    }

    private void UpdateVert ( int vertIndex, int quadI, int quadJ, Vector2 uv )
    {
        Vector2 offset = 2.0f * (uv - Vector2.one / 2.0f);
        Vector3 posCenter = quads [ quadI ] [ quadJ ].pos;
        // print ( "posCenter: " + posCenter );
        Vector3 up = transform.up; // This should be per quad.
        Vector3 right = transform.right; // This should be per quad.

        verts [ vertIndex ].vert = posCenter + offset.x * right + offset.y * up;
        verts [ vertIndex ].nor = quads [ quadI ] [ quadJ ].nor;
        verts [ vertIndex ].uv = uv;
        verts [ vertIndex ].col = quads [ quadI ] [ quadJ ].col;
    }

    private void InitPoints ( )
    {
        points = new Point [ 6 * quadCount ];
    }

    private void UpdatePoints ( )
    {
        // Each quad has two triangles.  Each triangle has three points.
        // Align quad uvs 00, 10, 01, 11 with triangles ABC and CBD: A=00, B=10, C=01, D=11
        int pointCount = 0;
        int vertCount = 0;

        for ( int i = 0; i < quadRoot; i++ )
        {
            for ( int j = 0; j < quadRoot; j++ )
            {
                // Triangle ABC
                points [ pointCount ] = verts [ vertCount ]; // A=00
                points [ pointCount + 1 ] = verts [ vertCount + 1 ]; // B=10
                points [ pointCount + 2 ] = verts [ vertCount + 2 ]; // C=01
                // print ( "Every 6th point vert pos: " + points [ pointCount ].vert );

                // Triangle CBD
                points [ pointCount + 3 ] = verts [ vertCount + 2 ]; // C=01
                points [ pointCount + 4 ] = verts [ vertCount + 1 ]; // B=10
                points [ pointCount + 5 ] = verts [ vertCount + 3 ]; // D=11

                pointCount += 6;
                vertCount += 4;
            }
        }

        // print ( "point count = " + pointCount + ", points.length = " + points.Length + ", vert count = " + vertCount );
    }

    private void SetComputeBuffer ( Point [ ] points )
    {
        computeBuffer = new ComputeBuffer ( points.Length, Marshal.SizeOf ( typeof ( Point ) ), ComputeBufferType.Default );
        computeBuffer.SetData ( points );
        mat.SetBuffer ( "points", computeBuffer );
    }

    private void OnRenderObject ( )
    {
        mat.SetPass ( 0 );
        Graphics.DrawProcedural ( MeshTopology.Triangles, points.Length, 1 ); // Works for all but MeshTopology.Points
    }

    private void OnDestroy ( )
    {
        computeBuffer.Release ( );
    }
}
