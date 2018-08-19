Shader "Custom/DrawTri_Procedural"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			struct Point // appdata
			{
				float3 vert;
				float3 nor;
				// float4 tan;
				float2 uv;
			};

			StructuredBuffer<Point> points;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _LightColor0; // Light color.
			
			v2f vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f vs;
				float4 vert_pos = float4(points[id].vert, 1.0f);
				float4 vert_nor = float4(points[id].nor, 1.0f);
				
				vs.pos = mul(UNITY_MATRIX_VP, vert_pos);

				vs.uv = TRANSFORM_TEX(points[id].uv, _MainTex);

				float3 NorDir = normalize(vert_nor.xyz);
				float4 AmbLight = UNITY_LIGHTMODEL_AMBIENT; // Environmental ambient color
				float4 LightDir = normalize(_WorldSpaceLightPos0);
				// negative dot => blk, positive dot => col, Directional Light rotation matters.
				vs.col = saturate(dot(LightDir, NorDir)) * _LightColor0 * AmbLight; 

				return vs;
			}
			
			float4 frag (v2f ps) : SV_Target
			{
				return tex2D(_MainTex, ps.uv) * ps.col;
			}

			ENDCG
		}
	}
}
