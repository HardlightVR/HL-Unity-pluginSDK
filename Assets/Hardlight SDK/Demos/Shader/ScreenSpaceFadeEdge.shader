Shader "Custom/Screen Space Fade Edge" {
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_ColorB("ColorB", Color) = (1,1,1,1)
		_MainTex("Main texture (RGB)", 2D) = "white" {}
		_ScrollXSpeed("X Scroll Speed", Range(-10,10)) = -1
		_ScrollYSpeed("Y Scroll Speed", Range(-10,10)) = .25
		_MainTexB("Main textureB (RGB)", 2D) = "white" {}
		_ScrollXSpeedB("X Scroll SpeedB", Range(-10,10)) = .1
		_ScrollYSpeedB("Y Scroll SpeedB", Range(-10,10)) = -1
		_ScreenTex("Screen space texture (RGB)", 2D) = "white" {}
		_RimColor("Rim", Color) = (1,1,1,1)
		_DotProduct("Rim effect", Range(-1,1)) = 0.25
		_Transparency("Transparency", Range(0,10)) = 0.4
	}
		SubShader{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200

		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard fullforwardshadows alpha:blend

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

		float _DotProduct;
		fixed4 _Color;
		fixed4 _RimColor;
		float _Transparency;
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
			o.Metallic = 1;
			o.Smoothness = 0;
			o.Alpha = pow(_Transparency - border + tex2D(_MainTexB, scrolledUV * _Time.x * 1) / 4, 4);
		}


		ENDCG
		}
			FallBack "Diffuse"
}