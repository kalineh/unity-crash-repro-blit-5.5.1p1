Shader "CosmicTrip/BuildNormals"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 _MainTex_TexelSize;
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{

				float delta = _MainTex_TexelSize.x; 
				float left = tex2D(_MainTex, i.uv - float2(delta, 0)).x;
				float right = tex2D(_MainTex, i.uv + float2(delta, 0)).x;
				float bottom = tex2D(_MainTex, i.uv - float2(0, delta)).x;
				float top = tex2D(_MainTex, i.uv + float2(0, delta)).x;
				
				float x = (right - left) / (2.0 * delta); 
				float y = (top - bottom) / (2.0 * delta); 
				float z = (1.0 - (x * x) - (y * y)); 

				fixed4 packedNormals = fixed4(x, y, z, 0); 

				// normalize between 0 and 1 (range is -1 to 1) 
				packedNormals = (packedNormals + 1.0) / 2.0; 

				// might as well include copy of depth buffer 
				fixed4 col = tex2D(_MainTex, i.uv);
				packedNormals.w = col.x; 

				return packedNormals; 
			}
			ENDCG
		}
	}
}
