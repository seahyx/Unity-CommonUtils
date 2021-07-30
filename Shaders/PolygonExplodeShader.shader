/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

Shader "Custom/PolygonExplodeShader"
{
	Properties
	{
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_ObjectHeight("Object Mesh Height", Float) = 1
		_AnimationProgress("Animation Progress", Range(0, 1)) = 0
		_ScaleFactor("Scale Factor", Range(0, 2)) = 0.5
		_ShrinkDuration("Shrink Duration", Range(0.001, 4)) = 0.2

		[Header(Wireframe Settings)]
		[Space(5)]
		[Toggle(_WIREFRAME)] _Enable_Wireframe("Enable Wireframe", Float) = 1
		_WireColor("Wireframe Color", Color) = (0, 0, 0, 0)
		_WireWidth("Wireframe Width", Range(0, 20)) = 1
		_WireSmoothing("Wireframe Smoothness", Range(0, 10)) = 1
		_WireFadeAnimDelay("Wireframe Fade Delay", Range(0, 4)) = 0.2
		_WireFadeMinThickness("Wireframe Minimum Fade Thickness", Range(0, 1)) = 0.6

		[Header(Iridescence Settings)]
		[Space(5)]
		[Toggle(_IRIDESCENCE)] _Iridescence("Iridescence", Float) = 0.0
		[NoScaleOffset] _IridescentSpectrumMap("Iridescent Spectrum Map", 2D) = "white" {}
		_IridescenceIntensity("Iridescence Intensity", Range(0.0, 1.0)) = 0.5
		_IridescenceThreshold("Iridescence Threshold", Range(0.0, 1.0)) = 0.05
		_IridescenceAngle("Iridescence Angle", Range(-0.78, 0.78)) = -0.78

		[Header(Environmental Colouring Settings)]
		[Space(5)]
		[Toggle(_ENVIRONMENT_COLORING)] _EnvironmentColoring("Environment Coloring", Float) = 0.0
		_EnvironmentColorThreshold("Environment Color Threshold", Range(0.0, 3.0)) = 1.5
		_EnvironmentColorIntensity("Environment Color Intensity", Range(0.0, 1.0)) = 0.5
		_EnvironmentColorX("Environment Color X (RGB)", Color) = (1.0, 0.0, 0.0, 1.0)
		_EnvironmentColorY("Environment Color Y (RGB)", Color) = (0.0, 1.0, 0.0, 1.0)
		_EnvironmentColorZ("Environment Color Z (RGB)", Color) = (0.0, 0.0, 1.0, 1.0)
		
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2
	}
	
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Overlay" }
		LOD 100
		ZTest Always
		ZWrite Off
		Cull[_CullMode]

		Pass
		{
			// Explode animation has 2 stages, or 3 if wireframe is enabled.
			// If wireframe is enabled, then the wireframe will first fade in, followed by the stages below.
			// Next stage is the detaching of triangles from their original positions.
			// Followed by the shrinking of the triangles as they float away.
			// Extra time must be given after the last triangles detach so they can shrink and disappear.

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#pragma shader_feature _WIREFRAME
			#pragma shader_feature _IRIDESCENCE
			#pragma shader_feature _ENVIRONMENT_COLORING

			#if defined(SHADER_API_D3D11)
			#pragma target 5.0
			#endif

			#include "UnityCG.cginc"
			#include "Easings.cginc"

			fixed4 _Color;
			float _ScaleFactor;
			float _AnimationProgress;
			float _ShrinkDuration;
			float _ObjectHeight;

#if defined(_WIREFRAME)
			fixed4 _WireColor;
			float _WireWidth;
			float _WireSmoothing;
			float _WireFadeAnimDelay;
			float _WireFadeMinThickness;

			static const float2 BarycentricCoords[3] =
			{
				float2(1, 0),
				float2(0, 1),
				float2(0, 0)
			};

#if defined(_IRIDESCENCE)
			sampler2D _IridescentSpectrumMap;
			fixed _IridescenceIntensity;
			fixed _IridescenceThreshold;
			fixed _IridescenceAngle;
#endif

#if defined(_ENVIRONMENT_COLORING)
			fixed _EnvironmentColorThreshold;
			fixed _EnvironmentColorIntensity;
			fixed3 _EnvironmentColorX;
			fixed3 _EnvironmentColorY;
			fixed3 _EnvironmentColorZ;
#endif
#endif

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed3 normal : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID // Single-pass Instanced
			};

			struct v2g
			{
				float4 vertex : POSITION;

#if defined(_WIREFRAME)
#if defined(_IRIDESCENCE)
				fixed3 iridescentColor : COLOR2;
#endif
#if defined(_ENVIRONMENT_COLORING)
				float3 worldPosition : TEXCOORD2;
				fixed3 worldNormal : COLOR3;
#endif
#endif

				UNITY_VERTEX_OUTPUT_STEREO // Single-pass Instanced
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
#if defined(_WIREFRAME)
				float2 barycentricCoords : TEXCOORD2;
				float wireAnimProgress : TEXCOORD3;
				float wireThicknessProgress :TEXCOORD4;
#if defined(_IRIDESCENCE)
				fixed3 iridescentColor : COLOR2;
#endif
#if defined(_ENVIRONMENT_COLORING)
				float3 worldPosition : TEXCOORD5;
				fixed3 worldNormal : COLOR3;
#endif
#endif

				UNITY_VERTEX_OUTPUT_STEREO // Single-pass Instanced
			};


#if defined(_IRIDESCENCE) && defined(_WIREFRAME)
			fixed3 Iridescence(float tangentDotIncident, sampler2D spectrumMap, float threshold, float2 uv, float angle, float intensity)
			{
				float k = tangentDotIncident * 0.5 + 0.5;
				float4 left = tex2D(spectrumMap, float2(lerp(0.0, 1.0 - threshold, k), 0.5), float2(0.0, 0.0), float2(0.0, 0.0));
				float4 right = tex2D(spectrumMap, float2(lerp(threshold, 1.0, k), 0.5), float2(0.0, 0.0), float2(0.0, 0.0));

				float2 XY = uv - float2(0.5, 0.5);
				float s = (cos(angle) * XY.x - sin(angle) * XY.y) / cos(angle);
				return (left.rgb + s * (right.rgb - left.rgb)) * intensity;
			}
#endif

			v2g vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);

				v2g o;
				UNITY_INITIALIZE_OUTPUT(v2g, o);
				o.vertex = v.vertex;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

#if defined(_WIREFRAME)
#if defined(_IRIDESCENCE)
				float3 rightTangent = normalize(mul((float3x3)unity_ObjectToWorld, float3(1.0, 0.0, 0.0)));
				float3 incidentWithCenter = normalize(mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)) - _WorldSpaceCameraPos);
				float tangentDotIncident = dot(rightTangent, incidentWithCenter);
				o.iridescentColor = Iridescence(tangentDotIncident, _IridescentSpectrumMap, _IridescenceThreshold, v.uv, _IridescenceAngle, _IridescenceIntensity);
#endif
#if defined(_ENVIRONMENT_COLORING)
				o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				
#endif
#endif

				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
			{
				// Calculate normal vector
				float3 vec1 = input[1].vertex - input[0].vertex;
				float3 vec2 = input[2].vertex - input[0].vertex;
				float3 normal = normalize(cross(vec1, vec2));

#if defined(_WIREFRAME)
				// The time taken from the first triangle until the last triangle begins to detach (and start shrinking)
				// Wireframe will fade in at the start, delaying the detachment animation
				// As the animation progress is always from 0 to 1, all durations are relative
				float detachAnimRelativeDur = 1 / (1 + _ShrinkDuration + _WireFadeAnimDelay);
				float shrinkAnimRelativeDur = _ShrinkDuration / (1 + _ShrinkDuration + _WireFadeAnimDelay);
				float wireFadeAnimRelativeDur = 1 - detachAnimRelativeDur - shrinkAnimRelativeDur;
#else
				// The time taken until the last triangle begins to detach (and start shrinking)
				// As the animation progress is always from 0 to 1, all durations are relative
				float detachAnimRelativeDur = 1 / (1 + _ShrinkDuration);
				float shrinkAnimRelativeDur = 1 - detachAnimRelativeDur;
#endif

				// Intialize variable to send to fragment shader
				g2f o;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				// Get and normalize the height of the triangle before manipulation, so we can determine its animation progress
				// Coordinates are in object-space
				// Get the center of the triangle which it will scale to, before detaching
				float3 center = (input[0].vertex + input[1].vertex + input[2].vertex) / 3;
				float height = (center.y + (_ObjectHeight / 2)) / _ObjectHeight;


				// Evaluate the detach and shrink animation progress on this triangle, with delay accounted
				float delay = height * detachAnimRelativeDur;
#if defined(_WIREFRAME)
				// Wireframe will fade before the detach/shrink animation starts
				float detachShrinkAnimProgress = clamp(_AnimationProgress - delay - wireFadeAnimRelativeDur, 0, shrinkAnimRelativeDur) / shrinkAnimRelativeDur;
				float wireFadeAnimProgress = clamp(_AnimationProgress - delay, 0, wireFadeAnimRelativeDur) / wireFadeAnimRelativeDur;
				wireFadeAnimProgress = easeInOutSmooth(wireFadeAnimProgress);
				float wireThicknessAnimProgress = clamp(_AnimationProgress - delay - shrinkAnimRelativeDur, 0, wireFadeAnimRelativeDur) / wireFadeAnimRelativeDur;
				wireThicknessAnimProgress = _WireFadeMinThickness + (easeInOutSin(1 - wireThicknessAnimProgress, 4) * (1 - _WireFadeMinThickness));
#else
				float detachShrinkAnimProgress = clamp(_AnimationProgress - delay, 0, shrinkAnimRelativeDur) / shrinkAnimRelativeDur;
#endif
				detachShrinkAnimProgress = easeInOutCos(detachShrinkAnimProgress, 0.4);

				// Scaling animation
				[unroll]
				for (int i = 0; i < 3; i++)
				{
					// Move vertices along normal vector
					input[i].vertex.xyz += normal * detachShrinkAnimProgress * _ScaleFactor;
				}

				// Get the center of the triangle which it will scale to, after scaling
				center = (input[0].vertex + input[1].vertex + input[2].vertex) / 3;


				// Shrinking animation
				[unroll]
				for (int i2 = 0; i2 < 3; i2++)
				{
					// Shrink vertex to center
					input[i2].vertex.xyz = input[i2].vertex.xyz + (detachShrinkAnimProgress * (center - input[i2].vertex.xyz));

					o.vertex = UnityObjectToClipPos(input[i2].vertex);

#if defined(_WIREFRAME)
					o.barycentricCoords = BarycentricCoords[i2];
					o.wireAnimProgress = wireFadeAnimProgress;
					o.wireThicknessProgress = wireThicknessAnimProgress;
#if defined(_IRIDESCENCE)
					o.iridescentColor = input[i2].iridescentColor;
#endif
#if defined(_ENVIRONMENT_COLORING)
					o.worldPosition = input[i2].worldPosition;
					o.worldNormal = input[i2].worldNormal;
#endif
#endif

					UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[i2], o);

					triStream.Append(o);
				}
				triStream.RestartStrip();
			}

			fixed4 frag(g2f i, fixed facing : VFACE) : SV_Target
			{
				fixed4 col = _Color;

#if defined(_WIREFRAME)
				// Wireframe effect using barycentric coordinates
				float3 barys;
				barys.xy = i.barycentricCoords;
				barys.z = 1 - barys.x - barys.y;
				float3 deltas = fwidth(barys);

				float3 smoothing = deltas * _WireSmoothing;
				float3 thickness = deltas * _WireWidth * i.wireThicknessProgress;

				barys = smoothstep(thickness, smoothing + thickness, barys);
				float minBary = min(barys.x, min(barys.y, barys.z));
				fixed4 wireCol = _WireColor;
#if defined(_IRIDESCENCE)
				wireCol.rgb += i.iridescentColor;
#endif
#if defined(_ENVIRONMENT_COLORING)
				fixed3 worldNormal = normalize(i.worldNormal) * facing;
				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPosition));
				fixed3 incident = -worldViewDir;
				fixed3 environmentColor = incident.x * incident.x * _EnvironmentColorX +
																	incident.y * incident.y * _EnvironmentColorY + 
																	incident.z * incident.z * _EnvironmentColorZ;
					wireCol.rgb += environmentColor * max(0.0, dot(incident, worldNormal) + _EnvironmentColorThreshold) * _EnvironmentColorIntensity;
#endif
				wireCol = lerp(col, wireCol, i.wireAnimProgress);
				col = lerp(wireCol, col, minBary);
#endif
				return col;
			}
			ENDCG
		}
	}
	FallBack "Unlit/Color"
}