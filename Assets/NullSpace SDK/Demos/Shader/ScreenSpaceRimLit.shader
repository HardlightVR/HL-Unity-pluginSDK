Shader "Custom/Screen Space Rim Lit" {
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_ColorB("ColorB", Color) = (1,1,1,1)
		_RimColor("Rim", Color) = (1,1,1,1)
		_MainTex("Main texture (RGB)", 2D) = "white" {}
		_MainTexB("Main textureB (RGB)", 2D) = "white" {}
		_ScrollXSpeed("X Scroll Speed", Range(-10,10)) = 2
		_ScrollYSpeed("Y Scroll Speed", Range(-10,10)) = 2
		_ScrollXSpeedB("X Scroll SpeedB", Range(-10,10)) = 2
		_ScrollYSpeedB("Y Scroll SpeedB", Range(-10,10)) = 2
		_ScreenTex("Screen space texture (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_DotProduct("Rim effect", Range(-1,1)) = 0.25
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MainTexB;
		sampler2D _ScreenTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_MainTexB;
			float4 screenPos;
			float3 worldNormal;
			float3 viewDir;
		};

		half _Glossiness;
		half _Metallic;
		float _DotProduct;
		fixed4 _Color;
		fixed4 _RimColor;
		fixed4 _ColorB;
		fixed _ScrollXSpeed;
		fixed _ScrollYSpeed;
		fixed _ScrollXSpeedB;
		fixed _ScrollYSpeedB;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed2 scrolledUV = IN.uv_MainTex;
			fixed2 scrolledUVB = IN.uv_MainTexB;
			scrolledUV += fixed2(_Time.x * _ScrollXSpeed, _Time.x * _ScrollYSpeed);
			scrolledUVB += fixed2(_Time.x * _ScrollXSpeedB, _Time.x * _ScrollYSpeedB);

			float border = .5 - (abs(dot(IN.viewDir, IN.worldNormal))) * 2;

			fixed4 c = tex2D(_MainTex, scrolledUV) * _Color * (tex2D(_MainTexB, scrolledUVB) * _ColorB);
			//c = fixed4(lerp(c, _RimColor.rgb, pow(border, 4)), 1);

			half2 screenUV = (IN.screenPos.xy * (_Time.x * .2f + .5f)) / IN.screenPos.w;

			fixed4 sstc = tex2D(_ScreenTex, screenUV);
			sstc = sstc + (sstc.r * .3 + sstc.g * .59 + sstc.b * .11) * 3;
			sstc = fixed4(lerp(sstc, _RimColor.rgb,  1 - pow(border, 2)), 1);
			o.Albedo = c.rgb * sstc.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}


		ENDCG
		}
			FallBack "Diffuse"
}