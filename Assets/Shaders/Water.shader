Shader "Unlit/Water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ColorDeep ("ColorDeep", Color) = (1,1,1,1)
        _Test1 ("Test 1", Float) = 1
        _Test2 ("Test 2", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"  "Queue"="Transparent"}
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD2;
                float2 depth : TEXCOORD3;
                float3 norm : NORM;
            };

            fixed4 _Color;
            fixed4 _ColorDeep;
            sampler2D _CameraDepthTexture;
            fixed _Test1;
            fixed _Test2;

            v2f vert (appdata v)
            {
                v2f o;
                //v.vertex.y += sin(v.vertex.x * 4 + _Time.y) * 1;
                //v.vertex.y += sin(v.vertex.z * 2 + 1 + _Time.y) * 1;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.norm = v.normal;
                COMPUTE_EYEDEPTH(o.screenPos.z);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                float depth = sceneZ - i.screenPos.z;

                depth = smoothstep(_Test1, _Test2, depth);
                fixed4 col = lerp(_Color, _ColorDeep, depth);

                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float refl = abs(dot(lightDirection, normalize(i.norm)));
                col += fixed4(1,1,1,1) * refl * 0.35;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
