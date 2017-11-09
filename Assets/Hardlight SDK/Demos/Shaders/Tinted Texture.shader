Shader "Unlit/Tinted Texture"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

		Tags { "RenderType"="Opaque" }
		LOD 100


		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			//This includes a bunch of useful default Unity code
			#include "UnityCG.cginc"

			
			struct appdata //Passed into the vert function
			{
				//Packed array : with a semantic binding (POSITION/TEXCOORD0)
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f //Passed from the vert function to the frag function
			{
				float2 uv : TEXCOORD0;
				//packed array: with the semantic binding screen position)
				float4 vertex : SV_POSITION;
			};

			//Steps of shader rendering
			//1. We start in local space for an object
			//2. Use a model matrix to convert it to world space
			//3. Use a view matrix to convert it to view space (eye space)
			//4. Use a projection matrix to convert it to clip space
			//5. Use a viewport transform to convert it to screen space

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			
			//Vertex function
			v2f vert (appdata v)
			{
				v2f o;
				//This is handling step 3.
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex); //Tiling and offset is performed in here
				return o;
			}
			
			//Fragment function
			fixed4 frag (v2f i) : SV_Target //Bound onto a render texture
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				//return col - _Color; //Black most of the time, fades to highlighted color. Black outline when highlighted
				//return col + _Color; // Oversaturates entire box to white. Only colors the outline with dark colors
				//return _Color - col; //Black box, white lines, highlights line in color
				return col * _Color; //Adequately gives the tinting of color to the core object
			}
			ENDCG
		}
	}
}
