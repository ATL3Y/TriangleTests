using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[RequireComponent ( typeof ( Renderer ) )]
// [ExecuteInEditMode]
public class DrawSpace_Noise : MonoBehaviour
{

    // Movement
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
    private Material quadMaterial;

    // Makes a square.
    [SerializeField]
    private float swarmScale;

    [SerializeField]
    private int quadCount;
    private int quadRoot;

    private ComputeBuffer computeBuffer;

    private Quad[][] quads;
    private Point[] verts;
    private Point[] points;

    // Noise
    [Range(0, 8)]
    public int octaves = 4;

    [Range(0f,8f)]
    public float lacunarity = 1f;

    [Range(0f, 8f)]
    public float gain = 1f;

    [Range(1f, 20f)]
    public float timeMultiplier = 1f;

    public Gradient color;
    private Gradient oldGradient;

    // Permutation table for persistant noise calculations
    private static readonly int[] noisePermutation = { 151,160,137,91,90,15,
                    131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
                    190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
                    88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
                    77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
                    102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
                    135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
                    5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
                    223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
                    129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
                    251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
                    49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
                    138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
                    };

    // gradients for 4D noise
    private static readonly float[] g4 = {
        0, -1, -1, -1,
        0, -1, -1, 1,
        0, -1, 1, -1,
        0, -1, 1, 1,
        0, 1, -1, -1,
        0, 1, -1, 1,
        0, 1, 1, -1,
        0, 1, 1, 1,
        -1, -1, 0, -1,
        -1, 1, 0, -1,
        1, -1, 0, -1,
        1, 1, 0, -1,
        -1, -1, 0, 1,
        -1, 1, 0, 1,
        1, -1, 0, 1,
        1, 1, 0, 1,

        -1, 0, -1, -1,
        1, 0, -1, -1,
        -1, 0, -1, 1,
        1, 0, -1, 1,
        -1, 0, 1, -1,
        1, 0, 1, -1,
        -1, 0, 1, 1,
        1, 0, 1, 1,
        0, -1, -1, 0,
        0, -1, -1, 0,
        0, -1, 1, 0,
        0, -1, 1, 0,
        0, 1, -1, 0,
        0, 1, -1, 0,
        0, 1, 1, 0,
        0, 1, 1, 0,
    };

    private Renderer m_Renderer;
    private Renderer renderer_
    {
        get
        {
            if ( m_Renderer == null )
                m_Renderer = GetComponent<Renderer> ( );
            return m_Renderer;
        }
    }

    private Shader m_OriginalShader;
    private Shader m_Shader;
    private Shader shader
    {
        get
        {
            if ( m_Shader == null )
                m_Shader = Shader.Find ( "Noise/DrawSpace_Noise" );
            return m_Shader;
        }
    }

    private Material m_Material;
    private Material material
    {
        get
        {
            return renderer_.sharedMaterial;
        }
    }

    private Texture2D m_PermutationTexture;
    private Texture2D permutationTexture
    {
        get
        {
            if ( m_PermutationTexture == null )
            {
                m_PermutationTexture = new Texture2D ( 256, 1, TextureFormat.ARGB32, false )
                {
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Point,
                };
                Color[] permutedValues = new Color[noisePermutation.Length];
                for ( int i = 0; i < permutedValues.Length; ++i )
                {
                    permutedValues [ i ] = new Color ( noisePermutation [ i ] / 255f, 0, 0, 0 );
                }
                m_PermutationTexture.SetPixels ( permutedValues );
                m_PermutationTexture.Apply ( );
            }
            return m_PermutationTexture;
        }
    }

    private Texture2D m_Gradient4Table;
    private Texture2D gradient4Table
    {
        get
        {
            if ( m_Gradient4Table == null )
            {
                m_Gradient4Table = new Texture2D ( 32, 1, TextureFormat.ARGB32, false )
                {
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Point,
                };
                int entries = g4.Length / 4;
                Color[] gradientValues = new Color[entries];
                for ( int i = 0; i < entries; ++i )
                {
                    int j = i * 4;
                    gradientValues [ i ] = new Color ( g4 [ j ], g4 [ j + 1 ], g4 [ j + 2 ], g4 [ j + 3 ] );
                }
                m_Gradient4Table.SetPixels ( gradientValues );
                m_Gradient4Table.Apply ( );
            }
            return m_Gradient4Table;
        }
    }

    private Texture2D m_ColorTexture;
    private Texture2D colorTexture
    {
        get
        {
            if ( m_ColorTexture == null )
            {
                m_ColorTexture = new Texture2D ( 256, 1, TextureFormat.ARGB32, false, false )
                {
                    name = "Noise color texture.",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 0,
                };
            }
            return m_ColorTexture;
        }
    }

    void OnEnable ( )
    {
        m_OriginalShader = material.shader;
        material.shader = shader;
        // Update ( );
    }

    void OnDisable ( )
    {
        if ( m_PermutationTexture != null )
            DestroyImmediate ( m_PermutationTexture );

        if ( m_Gradient4Table != null )
            DestroyImmediate ( m_Gradient4Table );

        m_PermutationTexture = null;
        m_Gradient4Table = null;

        material.shader = m_OriginalShader;
    }

    private void Start ( )
    {
        // Noise
        BakeColor ( );

        // Movement
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

        /*
        Color32[] pix = tex.GetPixels32 ();
        for ( int i = 0; i < pix.Length; i++ )
        {
            // print ( pix[i] );
        }
        */
    }
    void BakeColor ( )
    {
        float fWidth = colorTexture.width;
        Color[] pixels = new Color[colorTexture.width];

        for ( float i = 0f; i <= 1f; i += 1f / fWidth )
        {
            Color c = color.Evaluate(i);
            pixels [ ( int ) Mathf.Floor ( i * ( fWidth - 1f ) ) ] = c;
        }

        colorTexture.SetPixels ( pixels );
        colorTexture.Apply ( );
    }


    private void Update ( )
    {
        // Noise
        UpdateNoise ( );

        // Movement
        UpdatePos ( );
        UpdateVerts ( );
        UpdatePoints ( );
        SetComputeBuffer ( points );

    }

    private void UpdateNoise ( )
    {
        material.SetTexture ( "_ColorTexture", colorTexture );

        material.SetTexture ( "_PermutationTable", permutationTexture );
        material.SetTexture ( "_Gradient4Table", gradient4Table );

        material.SetInt ( "_Octaves", octaves );
        material.SetFloat ( "_Lacunarity", lacunarity );
        material.SetFloat ( "_Gain", gain );
        material.SetFloat ( "_TimeMultiplier", timeMultiplier );
    }

    private void UpdatePos ( )
    {
        // DO THIS IN THE SHADER AFTER THE NOISE PASS.
        for ( int i = 0; i < quadRoot; i++ )
        {
            for ( int j = 0; j < quadRoot; j++ )
            {
                // Evenly space quads in a square.
                quads [ i ] [ j ].pos += 0.01f * Mathf.Sin ( 0.1f * i * Time.timeSinceLevelLoad ) * transform.up + 0.02f * Mathf.Sin ( 0.2f * j * Time.timeSinceLevelLoad ) * transform.right;
            }
        }

        // Assume texture is mip 0, 256 x 256, and read/write enabled.
        // Pix is a flattened 2D array.
        // Assumes quadcount is 256, so one pixel per quad.
        /*
        Color32[] pix = tex.GetPixels32 ();
        int step = Mathf.FloorToInt(quadRoot / 256);
        for ( int i = 0; i < quadRoot; i++ )
        {
            for ( int j = 0; j < quadRoot; j++ )
            {
                // Evenly space quads in a square.
                // quads [ i ] [ j ].pos += 0.01f * Mathf.Sin ( 0.1f * i * Time.timeSinceLevelLoad ) * transform.up + 0.02f * Mathf.Sin ( 0.2f * j * Time.timeSinceLevelLoad ) * transform.right;
                // Make rgb vals that are from 0 to 255 (but mostly 60 to 180) be from 1 to -1.  So speed is 
                // print ( "Index " + i * quadRoot + j + " val " + pix [ i * quadRoot + j ] );


                float x = 2.0f * ((( float ) pix [ i * quadRoot + j ].r-50.0f) / 155.0f) - 1.0f;
                float y = 2.0f * ((( float ) pix [ i * quadRoot + j ].g-50.0f) / 155.0f) - 1.0f;
                float z = 2.0f * ((( float ) pix [ i * quadRoot + j ].b-50.0f) / 155.0f) - 1.0f;

                print ( "x: " + x + ", y: " + y + ", z: " + z );
                float speed = 0.5f;
                quads [ i ] [ j ].pos += new Vector3 ( x, y, z ) * Time.deltaTime * speed;
            }
        }
        */
    }

    private void InitQuads ( )
    {
        // Make double array of quads to make a square.
        quads = new Quad [ quadRoot ] [ ];
        for ( int i = 0; i < quads.Length; i++ )
        {
            quads [ i ] = new Quad [ quadRoot ];
        }

        float offset = swarmScale / quadRoot;

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
                quads [ i ] [ j ].pos -= swarmScale / 2.0f * transform.up + swarmScale / 2.0f * transform.right;

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
                // print ( j01 );
                float i01 = i / (quadRoot - 1.0f);
                float r = i01; // As i increases, r increases.
                float g = j01;
                float b = 1.0f - i01; // As i increases, b decreases.
                Color col = new Color ( r, g, b, 1.0f ); // As j increases, color gets darker.
                // print ( "col: " + col );
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

    public float quadSize = 0.3f;
    private void UpdateVert ( int vertIndex, int quadI, int quadJ, Vector2 uv )
    {
        Vector2 offset = quadSize * (uv - Vector2.one / 2.0f);
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
        quadMaterial.SetBuffer ( "points", computeBuffer );
    }

    private void OnRenderObject ( )
    {
        quadMaterial.SetPass ( 0 );
        Graphics.DrawProcedural ( MeshTopology.Triangles, points.Length, 1 ); // Works for all but MeshTopology.Points
    }

    private void OnDestroy ( )
    {
        computeBuffer.Release ( );
    }
}
