Shader "AFTShader/Effect/Simple"
{
	Properties
	{
		_TintColor ("Color", Color) = (0.5,0.5,0.5,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		[HideInInspector][Enum(Normal,0, Add,1, Add Smooth, 2)] _Mode("Mode", Float) = 0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector][Enum(Off, 0, On, 1)]_zwritee ("ZWrite", Float) = 0.0
		[HideInInspector][Enum(Off, 0, On, 1)]_culll ("Cull", Float) = 0.0
	}
	SubShader
	{
		Tags { "IgnoreProjector" = "True" "Queue" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Blend [_SrcBlend] [_DstBlend]
			ColorMask RGB
			Cull Off Lighting Off ZWrite[_ZWrite]

			CGPROGRAM
			#pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog
			#pragma shader_feature _ _AFT_EFFECT_SIMPLE_NORMAL _AFT_EFFECT_SIMPLE_ADD _AFT_EFFECT_SIMPLE_ADD_SMOOTH

            #include "UnityCG.cginc"

            sampler2D _MainTex; half4 _MainTex_ST;
            half4 _TintColor;
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			half _InvFade;

			struct appdata_t {
				half4 vertex : POSITION;
				half4 color : COLOR;
				half2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				half4 vertex : SV_POSITION;
				half4 color : COLOR;
				half2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
#ifdef SOFTPARTICLES_ON
				half4 projPos : TEXCOORD2;
#endif
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
#endif
				o.color = v.color;
#ifdef _AFT_EFFECT_SIMPLE_NORMAL
				o.color *= _TintColor;
#endif
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
#ifdef SOFTPARTICLES_ON
				half sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				half partZ = i.projPos.z;
				half fade = saturate(_InvFade * (sceneZ - partZ));
				i.color.a *= fade;
#endif
				half4 col = half4(0, 0, 0, 0);
#ifdef _AFT_EFFECT_SIMPLE_ADD
				col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, half4(0, 0, 0, 0));
#endif
#ifdef _AFT_EFFECT_SIMPLE_ADD_SMOOTH
				col = i.color * tex2D(_MainTex, i.texcoord);
				col.rgb *= col.a;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, half4(0, 0, 0, 0));
#endif
#ifdef _AFT_EFFECT_SIMPLE_NORMAL
				col = 2.0f * i.color * tex2D(_MainTex, i.texcoord);
				UNITY_APPLY_FOG(i.fogCoord, col);
#endif
				return col;
			}
            ENDCG
        }
    }
	CustomEditor "AFT_Effect_Simple_Shader_GUI"
}