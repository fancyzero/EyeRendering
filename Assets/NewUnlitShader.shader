Shader "Unlit/NewUnlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_IrisPos("iris pos", range(-2,2)) = 0
		_IrisSize("iris size", range(0.1,2)) = 0
		_IrisUVScale("iris uv scale", range(0.0,10)) = 0
		_IOR("ior", range(1.01,3))= 1
		_PuipleSize("puile size", range(0.1,3))= 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
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
				float3 normal:NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal:TEXCOORD2;
				float3 localPos: TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _IrisPos;
			float _IrisSize;
			float _IrisUVScale;
			float _IOR;
			float _PuipleSize;
			bool intersectPlane(float3 n, float3 p0, float3 l0, float3 l, out float t) 
            { 
                float denom = dot(-n, l); 
                if (denom > 1e-6) { 
                    float3 p0l0 = p0 - l0; 
                    t = dot(p0l0, -n) / denom; 
                    return (t >= 0); 
                } 
            
                return false; 
            } 
			v2f vert (appdata v)
			{
				v2f o;
				o.localPos = v.vertex;//mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.normal = v.normal;
				
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 localCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz,1));
				float3 viewDir = normalize( i.localPos - localCamPos);
				float t;
				float n1 = 1.0;
				float n2 = _IOR;
				float r = n1/n2;
				float c = dot(-i.normal, viewDir);
				float3 V_refraction = r*viewDir + (r*c - sqrt(1-pow(r,2)*(1-pow(c,2))))*i.normal;
				
				if (intersectPlane( float3(0,0,1), float3(0,0,_IrisPos), i.localPos, V_refraction, t ))
				{
					float3 hitpos = (i.localPos  + t*V_refraction);
					if (length(hitpos.xy) < _IrisSize)
					{	
						
						hitpos *=0.5;
						hitpos -=0.5;
						float2 dir = normalize(hitpos+0.5);
						float d = length(hitpos+0.5);
						d*=saturate(d/_PuipleSize);
						hitpos.xy = dir*d-0.5;
						hitpos = (hitpos + 0.5) *_PuipleSize -0.5;
						return tex2D(_MainTex, hitpos);
						return float4((hitpos).xy,0,1)*10;
					}
				}
				
					
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return (i.normal.xyzz+1) /2;
			}
			ENDCG
		}
	}
}
