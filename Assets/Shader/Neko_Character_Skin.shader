Shader "Neko/Character/Skin"
{
    Properties
    {
        // 表面选项
        [Enum(Opaque,0,Transparent,1)] _Surface("Surface Type", Float) = 0
        [Enum(Alpha,1,Additive,0,Multiply,2)] _BlendMode("Blending Mode", Float) = 1
        [Toggle(_ALPHATEST_ON)]_ALPHATEST("Alpha Clipping", Float) = 1
        _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        
        // 投影控制
        [Toggle(_CAST_SHADOWS_ON)] _CastShadows("产生投影", Float) = 1
        [Toggle(_RECEIVE_SHADOWS_ON)] _ReceiveShadows("接受投影", Float) = 1
        
        // 渲染队列控制
        [Enum(Background,1000,Geometry,2000,AlphaTest,2450,Transparent,3000,Overlay,4000)] _QueueMode("Queue Mode", Float) = 2000
        [HideInInspector] _QueueOffset("队列偏移", Float) = 0
        [HideInInspector] _QueueControl("队列控制", Float) = -1
        [HideInInspector] _TransparentZWriteInitialized("透明ZWrite初始化标记", Float) = 0
        
        // 渲染状态
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("混合源", Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("混合目标", Int) = 10
        [Enum(Off,0,Front,1,Back,2)] _CullMode("剔除模式", Float) = 2
        [Enum(Off,0,On,1)] _ZWriteMode("ZWrite Mode", Float) = 1
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorMask("Color Mask", Float) = 15
        
        [HideInInspector][ToggleOff] _SpecularHighlights("高光", Float) = 1
        [HideInInspector][ToggleOff] _EnvironmentReflections("环境反射", Float) = 1
		
		[Header(Main__)][Space(10)] _MainColor( "主颜色值", Color ) = ( 1, 1, 1, 1 )
		_MainTex( "主帖图", 2D ) = "white" {}
		_LightScatter( "灯光散射强度", Range( 0, 2 ) ) = 1
		_BaseToneGlow( "基础色调发亮", Range( 0, 1 ) ) = 0.3
		[Header(Normals__)][Space(10)][Toggle] _NormalToggel( "法线开关", Float ) = 1
		_NormalMap( "法线贴图", 2D ) = "bump" {}
		_NormalScale( "法线强度", Range( 0, 2 ) ) = 1
		[Header(Metal_Roughness__)][Space(10)][Toggle] _MetalRoughnessToggel( "金属度粗糙度开关", Float ) = 1
		_MetalRoughness( "金属度-粗造度-", 2D ) = "black" {}
		_MetallicValue( "金属度强度", Range( 0, 1 ) ) = 0
		_RoughnessValue( "粗糙度强度", Range( 0, 1 ) ) = 0.45
		[Toggle] _OcclusionToggel( "AO：开关", Float ) = 0
		_Occlusion( "AO：强度", Range( 0, 1 ) ) = 1
		[Header(Mask(RGBA)__)][Space(10)] _Mask( "遮罩图(RGBA)", 2D ) = "black" {}
		[Header( __Mouth(R))][Toggle] _MouthToggel( "嘴mask开关", Float ) = 0
		_MouthColor( "嘴：Color", Color ) = ( 1, 1, 1, 0 )
		_Contrast( "嘴：对比度", Range( 0, 2 ) ) = 1
		_Saturation( "嘴：饱和度", Range( 0, 2 ) ) = 0
		[Header( __Emission(G))][Toggle] _EmissionToggel( "自发光开关", Float ) = 0
		_EmissionValue( "自发光强度(A)", Range( 0, 3 ) ) = 0
		[HDR] _EmissionColor( "自发光颜色", Color ) = ( 0, 0, 0, 0 )
		
		// 新增的流光属性
		[Header( __FlowMap(B))][Toggle(_LIUGUANG_ON)] _LiuguangToggle("流光开关", Float) = 0
		[HDR] _LiuguangColor("流光颜色", Color) = (1, 1, 1, 0)
		_LiuguangTex("流光纹理", 2D) = "black" {}
		[Enum(UV,0,ObjectPos,1,WorldPos,2,ViewPos,3)] _CoordinateSpace("坐标空间", Float) = 0
		_LiuguangXYZW("流光：XY(平铺) ZW(偏移)", Vector) = (1, 1, 1, 1)
		_LiuguangSpeed("流光速度", Float) = 0.1
		_LiuguangIntencity("流光强度", Float) = 10
		
		[HideInInspector] _SSSValue( "SSS强度(A)", Range( 0, 10 ) ) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector]_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 1
		[HideInInspector]_TransStrength( "Strength", Range( 0, 50 ) ) = 50
		[HideInInspector]_TransNormal( "Normal Distortion", Range( 0, 1 ) ) = 1
		[HideInInspector]_TransScattering( "Scattering", Range( 1, 50 ) ) = 1
		[HideInInspector]_TransDirect( "Direct", Range( 0, 1 ) ) = 0
		[HideInInspector]_TransAmbient( "Ambient", Range( 0, 1 ) ) = 1
		_TransShadow( "投影强度", Range( 0, 2 ) ) = 1
    }

    SubShader
    {
        LOD 0
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType" = "Lit" 
        }

        Cull[_CullMode]
        ZWrite On
        ZTest LEqual
        Offset 0 , 0
        AlphaToMask Off
        
        HLSLINCLUDE
        #pragma target 4.5
        #pragma prefer_hlslcc gles
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        
        ENDHLSL
        
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForwardOnly" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWriteMode]
            ZTest LEqual
            Offset 0, 0
            ColorMask [_ColorMask]
            
            HLSLPROGRAM
            
            #define _NORMAL_DROPOFF_TS 1
            #define ASE_TRANSMISSION 1
            #pragma multi_compile_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #define ASE_TRANSLUCENCY 1
            #define _ALPHATEST_SHADOW_ON 1
            #define _EMISSION
            #define _NORMALMAP 1

            // 投影控制关键字
            #pragma shader_feature _ _CAST_SHADOWS_ON
            #pragma shader_feature _ _RECEIVE_SHADOWS_ON
            
            // Alpha测试关键字
            #pragma shader_feature _ _ALPHATEST_ON
            
            // 表面类型关键字
            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            
            // 流光变体开关
            #pragma shader_feature _ _LIUGUANG_ON
            
            // 阴影相关编译指令
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #pragma vertex vert
            #pragma fragment frag

            #define SHADERPASS SHADERPASS_FORWARD
            
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
                #define ENABLE_TERRAIN_PERPIXEL_NORMAL
            #endif

            #define ASE_NEEDS_TEXTURE_COORDINATES0
            #define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
            #define ASE_NEEDS_WORLD_POSITION
            #define ASE_NEEDS_FRAG_WORLD_POSITION
            #define ASE_NEEDS_FRAG_SCREEN_POSITION_NORMALIZED

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float4 texcoord : TEXCOORD0;
                #if defined(LIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES1)
                    float4 texcoord1 : TEXCOORD1;
                #endif
                #if defined(DYNAMICLIGHTMAP_ON) || defined(ASE_NEEDS_TEXTURE_COORDINATES2)
                    float4 texcoord2 : TEXCOORD2;
                #endif
            };

            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 tangentWS : TEXCOORD2;
                float4 lightmapUVOrVertexSH : TEXCOORD3;
                float4 ase_texcoord6 : TEXCOORD6;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainColor;
            float4 _Mask_ST;
            float4 _MouthColor;
            float4 _EmissionColor;
            float4 _MetalRoughness_ST;
            float4 _NormalMap_ST;
            // 新增的流光相关变量
            float4 _LiuguangColor;
            float4 _LiuguangXYZW;
            float _LiuguangIntencity;
            float _LiuguangSpeed;
            float _LiuguangToggle;
            float _CoordinateSpace;
            
            float _MouthToggel;
            float _RoughnessValue;
            float _Occlusion;
            float _OcclusionToggel;
            float _MetallicValue;
            float _MetalRoughnessToggel;
            float _LightScatter;
            float _NormalToggel;
            float _BaseToneGlow;
            float _EmissionValue;
            float _EmissionToggel;
            float _Saturation;
            float _Contrast;
            float _NormalScale;
            float _SSSValue;
            float _AlphaCutoff;
            #ifdef ASE_TRANSMISSION
                float _TransmissionShadow;
            #endif
            #ifdef ASE_TRANSLUCENCY
                float _TransStrength;
                float _TransNormal;
                float _TransScattering;
                float _TransDirect;
                float _TransAmbient;
                float _TransShadow;
            #endif
            CBUFFER_END

            sampler2D _MainTex;
            sampler2D _Mask;
            sampler2D _NormalMap;
            sampler2D _MetalRoughness;
            // 新增的流光纹理
            sampler2D _LiuguangTex;

            float4 CalculateContrast( float contrastValue, float4 colorTarget )
            {
                float t = 0.5 * ( 1.0 - contrastValue );
                return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
            }
            
            PackedVaryings vert ( Attributes input )
            {
                PackedVaryings output = (PackedVaryings)0;
                output.ase_texcoord6.xy = input.texcoord.xy;
                
                //setting value to unused interpolator channels and avoid initialization warnings
                output.ase_texcoord6.zw = input.positionOS.xy;
                float3 vertexValue = float3(0, 0, 0);
                input.positionOS.xyz += vertexValue;
                input.normalOS = input.normalOS;
                input.tangentOS = input.tangentOS;
                VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );
                VertexNormalInputs normalInput = GetVertexNormalInputs( input.normalOS, input.tangentOS );

                #if defined(LIGHTMAP_ON)
                    OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
                #else
                    OUTPUT_SH(normalInput.normalWS.xyz, output.lightmapUVOrVertexSH.xyz);
                #endif
                #if defined(DYNAMICLIGHTMAP_ON)
                    output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif

                #if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
                    output.lightmapUVOrVertexSH.zw = input.texcoord.xy;
                    output.lightmapUVOrVertexSH.xy = input.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                #endif

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4( normalInput.tangentWS, ( input.tangentOS.w > 0.0 ? 1.0 : -1.0 ) * GetOddNegativeScale() );
                return output;
            }

            half4 frag ( PackedVaryings input
                        #if defined( ASE_DEPTH_WRITE_ON )
                        ,out float outputDepth : ASE_SV_DEPTH
                        #endif
                        #ifdef _WRITE_RENDERING_LAYERS
                        , out float4 outRenderingLayers : SV_Target1
                        #endif
                         ) : SV_Target
            {
                // 接受投影控制
				float4 shadowCoord = float4(0, 0, 0, 0);
				#if defined(_RECEIVE_SHADOWS_ON)
					#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						shadowCoord = TransformWorldToShadowCoord( input.positionWS );
					#endif
				#endif

                // @diogo: mikktspace compliant
                float renormFactor = 1.0 / max( FLT_MIN, length( input.normalWS ) );

                float3 PositionWS = input.positionWS;
                float3 PositionRWS = GetCameraRelativePositionWS( PositionWS );
                float3 ViewDirWS = GetWorldSpaceNormalizeViewDir( PositionWS );
                float4 ShadowCoord = shadowCoord;
                float4 ScreenPosNorm = float4( GetNormalizedScreenSpaceUV( input.positionCS ), input.positionCS.zw );
                float4 ClipPos = ComputeClipSpacePosition( ScreenPosNorm.xy, input.positionCS.z ) * input.positionCS.w;
                float4 ScreenPos = ComputeScreenPos( ClipPos );
                float3 TangentWS = input.tangentWS.xyz * renormFactor;
                float3 BitangentWS = cross( input.normalWS, input.tangentWS.xyz ) * input.tangentWS.w * renormFactor;
                float3 NormalWS = input.normalWS * renormFactor;

                #if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
                    float2 sampleCoords = (input.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
                    NormalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
                    TangentWS = -cross(GetObjectToWorldMatrix()._13_23_33, NormalWS);
                    BitangentWS = cross(NormalWS, -TangentWS);
                #endif

                float2 uv_MainTex = input.ase_texcoord6.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                float4 tex2DNode112 = tex2D( _MainTex, uv_MainTex );
                
                // 修复：使用MainColor的Alpha值控制全局透明度
                float4 temp_output_113_0 = ( tex2DNode112 * _MainColor );
                float globalAlpha = _MainColor.a; // 全局透明度控制
                
                float2 uv_Mask = input.ase_texcoord6.xy * _Mask_ST.xy + _Mask_ST.zw;
                float4 tex2DNode114 = tex2D( _Mask, uv_Mask );
                float Mouth314 = tex2DNode114.r;
                float3 temp_output_12_0_g2 = CalculateContrast(_Contrast,( temp_output_113_0 * Mouth314 * _MouthColor )).rgb;
                float dotResult28_g2 = dot( float3( 0.2126729, 0.7151522, 0.072175 ) , temp_output_12_0_g2 );
                float3 temp_cast_1 = (dotResult28_g2).xxx;
                float temp_output_21_0_g2 = _Saturation;
                float3 lerpResult31_g2 = lerp( temp_cast_1 , temp_output_12_0_g2 , temp_output_21_0_g2);
                float4 BaseColor306 = (( _MouthToggel )?( ( float4( lerpResult31_g2 , 0.0 ) + temp_output_113_0 ) ):( temp_output_113_0 ));
                float lerpResult133 = lerp( 0.0 , tex2DNode114.g , _EmissionValue);
                float4 lerpResult102 = lerp( BaseColor306 , (( _EmissionToggel )?( ( _EmissionColor * abs( ( BaseColor306 * lerpResult133 ) ) ) ):( float4( 0,0,0,0 ) )) , _LightScatter);
                float4 clampResult110 = clamp( ( lerpResult102 + (( _EmissionToggel )?( ( _EmissionColor * abs( ( BaseColor306 * lerpResult133 ) ) ) ):( float4( 0,0,0,0 ) )) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
                float4 clampResult246 = clamp( ( ( BaseColor306 + lerpResult102 ) + clampResult110 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
                
                // 应用全局透明度到基础颜色
                float4 BaseColorFuction303 = float4(clampResult246.rgb, clampResult246.a * globalAlpha);
                
                // 新增的流光计算 - 使用变体开关
                #ifdef _LIUGUANG_ON
                float mulTime303 = _TimeParameters.x * _LiuguangSpeed;
                float2 appendResult337 = (float2(_LiuguangXYZW.z , _LiuguangXYZW.w));
                float2 texCoord436 = input.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
                
                // 根据选择的坐标空间计算采样坐标
                float2 staticSwitch415 = float2(0,0);
                
                [branch]
                switch(_CoordinateSpace)
                {
                    case 0: // UV坐标
                        staticSwitch415 = (texCoord436).xy;
                        break;
                    case 1: // 物体空间位置
                        staticSwitch415 = input.ase_texcoord6.zw;
                        break;
                    case 2: // 世界空间位置
                        staticSwitch415 = PositionWS.xy;
                        break;
                    case 3: // 视图空间位置
                        staticSwitch415 = ScreenPosNorm.xy;
                        break;
                    default:
                        staticSwitch415 = texCoord436.xy;
                        break;
                }
                
                float2 appendResult333 = (float2(_LiuguangXYZW.x , _LiuguangXYZW.y));
                float2 panner302 = (mulTime303 * appendResult337 + (staticSwitch415 * appendResult333).xy);
                float4 tex2DNode307 = tex2D( _LiuguangTex, panner302 );
                
                // 修改：使用Mask的B通道（tex2DNode114.b）而不是R通道
                float3 temp_output_322_0 = ( tex2DNode114.b * tex2DNode307.rgb * BaseColorFuction303.rgb * _LiuguangColor.rgb * _LiuguangIntencity );
                #else
                float3 temp_output_322_0 = float3(0, 0, 0);
                #endif
                
                float2 uv_NormalMap = input.ase_texcoord6.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
                float3 unpack111 = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), _NormalScale );
                unpack111.z = lerp( 1, unpack111.z, saturate(_NormalScale) );
                float3 tex2DNode111 = unpack111;
                float3 normalizeResult188 = normalize( tex2DNode111 );
                float3 NormalFuction291 = (( _NormalToggel )?( normalizeResult188 ):( float3( 0, 0, 1 ) ));
                
                float2 uv_MetalRoughness = input.ase_texcoord6.xy * _MetalRoughness_ST.xy + _MetalRoughness_ST.zw;
                float4 tex2DNode118 = tex2D( _MetalRoughness, uv_MetalRoughness );
                float lerpResult330 = clamp( ( tex2DNode118.r + _MetallicValue ) , 0.0 , 1.0 );
                float lerpResult328 = clamp( ( tex2DNode118.g + _RoughnessValue ) , 0.0 , 1.0 );
                float2 appendResult342 = (float2(lerpResult330 , lerpResult328));
                float2 break343 = (( _MetalRoughnessToggel )?( appendResult342 ):( float2( 0,0 ) ));
                float MetalFuction293 = break343.x;
                
                float RoghnessFuction237 = break343.y;

                float Occlusion406 = (( _OcclusionToggel )?( pow( abs(tex2DNode118.b), _Occlusion ) ):( 1.0 ));
                
                float4 lerpResult254 = lerp( float4( 0,0,0,0 ) , BaseColorFuction303 , _BaseToneGlow);
                float4 FinalEmissive256 = clampResult110;
                // 修改发射光计算，包含流光效果
                float4 EmissionFuction301 = ( lerpResult254 + FinalEmissive256 + float4(temp_output_322_0, 0.0) );
                
                // 修复：Alpha值使用MainColor的Alpha控制全局透明度
                float Alpha396 = tex2DNode112.a * globalAlpha;
                
                // Alpha测试 - 关键修复：在所有通道中保持一致
                #ifdef _ALPHATEST_ON
                    clip(Alpha396 - _AlphaCutoff);
                #endif
                
                float lerpResult375 = lerp( 0.0 , tex2DNode114.a , _SSSValue);
                float SSSThick272 = lerpResult375;
                float3 temp_cast_5 = (SSSThick272).xxx;
                

                float3 BaseColor = (Occlusion406 * BaseColorFuction303).rgb;
                float3 Normal = NormalFuction291;
                float3 Specular = 0.5;
                float Metallic = MetalFuction293;
                float Smoothness = RoghnessFuction237;
                float Occlusion = 1;
                float3 Emission = EmissionFuction301.rgb;
                float Alpha = Alpha396;
                float AlphaClipThreshold = _AlphaCutoff;
                float AlphaClipThresholdShadow = _AlphaCutoff;
                float3 BakedGI = 0;
                float3 RefractionColor = 1;
                float RefractionIndex = 1;
                float3 Transmission = temp_cast_5;
                float3 Translucency = 1;

                #if defined( ASE_DEPTH_WRITE_ON )
                    float DeviceDepth = ClipPos.z;
                #endif

                #ifdef _CLEARCOAT
                    float CoatMask = 0;
                    float CoatSmoothness = 0;
                #endif

                InputData inputData = (InputData)0;
                inputData.positionWS = PositionWS;
                inputData.positionCS = float4( input.positionCS.xy, ClipPos.zw / ClipPos.w );
                inputData.normalizedScreenSpaceUV = ScreenPosNorm.xy;
                inputData.viewDirectionWS = ViewDirWS;
                
                // 修复：不接受投影时禁用阴影计算
                #if defined(_RECEIVE_SHADOWS_ON)
                    inputData.shadowCoord = ShadowCoord;
                #else
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif

                #ifdef _NORMALMAP
                        #if _NORMAL_DROPOFF_TS
                            inputData.normalWS = TransformTangentToWorld(Normal, half3x3(TangentWS, BitangentWS, NormalWS));
                        #elif _NORMAL_DROPOFF_OS
                            inputData.normalWS = TransformObjectToWorldNormal(Normal);
                        #elif _NORMAL_DROPOFF_WS
                            inputData.normalWS = Normal;
                        #endif
                    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                #else
                    inputData.normalWS = NormalWS;
                #endif

                #if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
                    float3 SH = SampleSH(inputData.normalWS.xyz);
                #else
                    float3 SH = input.lightmapUVOrVertexSH.xyz;
                #endif

                #if defined(DYNAMICLIGHTMAP_ON)
                    inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.dynamicLightmapUV.xy, SH, inputData.normalWS);
                #else
                    inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
                #endif

                #ifdef ASE_BAKEDGI
                    inputData.bakedGI = BakedGI;
                #endif

                inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUVOrVertexSH.xy);

                #if defined(DEBUG_DISPLAY)
                    #if defined(DYNAMICLIGHTMAP_ON)
                        inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
                    #endif
                    #if defined(LIGHTMAP_ON)
                        inputData.staticLightmapUV = input.lightmapUVOrVertexSH.xy;
                    #else
                        inputData.vertexSH = SH;
                    #endif
                #endif

                SurfaceData surfaceData;
                surfaceData.albedo              = BaseColor;
                surfaceData.metallic            = saturate(Metallic);
                surfaceData.specular            = Specular;
                surfaceData.smoothness          = saturate(Smoothness),
                surfaceData.occlusion           = Occlusion,
                surfaceData.emission            = Emission,
                surfaceData.alpha               = saturate(Alpha);
                surfaceData.normalTS            = Normal;
                surfaceData.clearCoatMask       = 0;
                surfaceData.clearCoatSmoothness = 1;

                #ifdef _CLEARCOAT
                    surfaceData.clearCoatMask       = saturate(CoatMask);
                    surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
                #endif

                #ifdef _DBUFFER
                    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
                #endif

                half4 color = UniversalFragmentPBR( inputData, surfaceData);

                #ifdef ASE_TRANSMISSION
                {
                    float shadow = _TransmissionShadow;

                    #define SUM_LIGHT_TRANSMISSION(Light)\
                        float3 atten = Light.color * Light.distanceAttenuation;\
                        atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
                        half3 transmission = max( 0, -dot( inputData.normalWS, Light.direction ) ) * atten * Transmission;\
                        color.rgb += BaseColor * transmission;

                    SUM_LIGHT_TRANSMISSION( GetMainLight( inputData.shadowCoord ) );

                    #if defined(_ADDITIONAL_LIGHTS)
                        uint meshRenderingLayers = GetMeshRenderingLayer();
                        uint pixelLightCount = GetAdditionalLightsCount();
                        #if USE_FORWARD_PLUS
                            for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                            {
                                FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
                    
                                Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
                                #ifdef _LIGHT_LAYERS
                                if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                                #endif
                                {
                                    SUM_LIGHT_TRANSMISSION( light );
                                }
                            }
                        #endif
                        LIGHT_LOOP_BEGIN( pixelLightCount )
                            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
                            #ifdef _LIGHT_LAYERS
                            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                            #endif
                            {
                                SUM_LIGHT_TRANSMISSION( light );
                            }
                        LIGHT_LOOP_END
                    #endif
                }
                #endif

                #ifdef ASE_TRANSLUCENCY
                {
                    float shadow = _TransShadow;
                    float normal = _TransNormal;
                    float scattering = _TransScattering;
                    float direct = _TransDirect;
                    float ambient = _TransAmbient;
                    float strength = _TransStrength;

                    #define SUM_LIGHT_TRANSLUCENCY(Light)\
                        float3 atten = Light.color * Light.distanceAttenuation;\
                        atten = lerp( atten, atten * Light.shadowAttenuation, shadow );\
                        half3 lightDir = Light.direction + inputData.normalWS * normal;\
                        half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );\
                        half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;\
                        color.rgb += BaseColor * translucency * strength;

                    SUM_LIGHT_TRANSLUCENCY( GetMainLight( inputData.shadowCoord ) );

                    #if defined(_ADDITIONAL_LIGHTS)
                        uint meshRenderingLayers = GetMeshRenderingLayer();
                        uint pixelLightCount = GetAdditionalLightsCount();
                        #if USE_FORWARD_PLUS
                            for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                            {
                                FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
                    
                                Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
                                #ifdef _LIGHT_LAYERS
                                if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                                #endif
                                {
                                    SUM_LIGHT_TRANSLUCENCY( light );
                                }
                            }
                        #endif
                        LIGHT_LOOP_BEGIN( pixelLightCount )
                            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
                            #ifdef _LIGHT_LAYERS
                            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                            #endif
                                SUM_LIGHT_TRANSLUCENCY( light );
                        LIGHT_LOOP_END
                    #endif
                }
                #endif

                #ifdef ASE_REFRACTION
                    float4 projScreenPos = ScreenPos / ScreenPos.w;
                    float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( NormalWS,0 ) ).xyz * ( 1.0 - dot( NormalWS, ViewDirWS ) );
                    projScreenPos.xy += refractionOffset.xy;
                    float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
                    color.rgb = lerp( refraction, color.rgb, color.a );
                    color.a = 1;
                #endif

                #ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
                    color.rgb *= color.a;
                #endif

                #if defined(ASE_DEPTH_WRITE_ON)
                    outputDepth = DeviceDepth;
                #endif

                #ifdef _WRITE_RENDERING_LAYERS
                    uint renderingLayers = GetMeshRenderingLayer();
                    outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
                #endif

                return color;
            }
            ENDHLSL
        }
        
        // 阴影投射通道 - 修复Alpha测试问题
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_CullMode]

            HLSLPROGRAM
            
            #define _NORMAL_DROPOFF_TS 1
            #define ASE_TRANSLUCENCY 1
            #define _EMISSION
            #define _NORMALMAP 1
            
            // 关键修复：添加Alpha测试支持
            #pragma shader_feature _ _ALPHATEST_ON
            #pragma shader_feature _ _CAST_SHADOWS_ON
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex vert
            #pragma fragment frag

            #define SHADERPASS SHADERPASS_SHADOWCASTER

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #define ASE_NEEDS_TEXTURE_COORDINATES0

            #if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
                #define ASE_SV_DEPTH SV_DepthLessEqual
                #define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
            #else
                #define ASE_SV_DEPTH SV_Depth
                #define ASE_SV_POSITION_QUALIFIERS
            #endif

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainColor;
            float _AlphaCutoff;
            float _CullMode;
            // ... 其他属性
            CBUFFER_END

            sampler2D _MainTex;
            float3 _LightDirection;
            float3 _LightPosition;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float4 texcoord : TEXCOORD0;
            };

            struct PackedVaryings
            {
                ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 texcoord : TEXCOORD1;
            };

            PackedVaryings vert(Attributes input)
            {
                PackedVaryings output;
                output.texcoord.xy = input.texcoord.xy;
                output.texcoord.zw = 0;
                
                // 产生投影控制
                #if defined(_CAST_SHADOWS_ON)
                    input.positionOS.xyz += float3(0, 0, 0);
                    input.normalOS = input.normalOS;
                    input.tangentOS = input.tangentOS;
                    
                    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                    float3 normalWS = TransformObjectToWorldDir(input.normalOS);

                    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                        float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                    #else
                        float3 lightDirectionWS = _LightDirection;
                    #endif

                    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                    #if UNITY_REVERSED_Z
                        positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                    #else
                        positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                    #endif

                    output.positionCS = positionCS;
                    output.positionWS = positionWS;
                #else
                    // 如果不产生投影，将顶点位置设置为无效位置
                    output.positionCS = float4(0, 0, 0, 0);
                    output.positionWS = float3(0, 0, 0);
                #endif

                return output;
            }

            half4 frag(PackedVaryings input
                #if defined(ASE_DEPTH_WRITE_ON)
                , out float outputDepth : ASE_SV_DEPTH
                #endif
            ) : SV_Target
            {
                // 关键修复：在阴影投射通道中也进行Alpha测试
                #if defined(_CAST_SHADOWS_ON)
                    float2 uv_MainTex = input.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                    float4 tex2DNode112 = tex2D(_MainTex, uv_MainTex);
                    
                    // 使用与主通道相同的Alpha计算逻辑
                    float alpha = tex2DNode112.a * _MainColor.a;
                    
                    // Alpha测试 - 与主通道保持一致
                    #ifdef _ALPHATEST_ON
                        clip(alpha - _AlphaCutoff);
                    #endif

                    #if defined(ASE_DEPTH_WRITE_ON)
                        float DeviceDepth = input.positionCS.z;
                        outputDepth = DeviceDepth;
                    #endif

                    return 0;
                #else
                    // 如果不产生投影，直接丢弃片段
                    discard;
                    return 0;
                #endif
            }
            ENDHLSL
        }

        // 深度Only通道 - 修复Alpha测试
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            AlphaToMask Off
            Cull [_CullMode]

            HLSLPROGRAM
            
            #define _NORMAL_DROPOFF_TS 1
            #define ASE_TRANSLUCENCY 1
            #define _EMISSION
            #define _NORMALMAP 1

            // 关键修复：添加Alpha测试支持
            #pragma shader_feature _ _ALPHATEST_ON

            #pragma vertex vert
            #pragma fragment frag

            #define SHADERPASS SHADERPASS_DEPTHONLY

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #define ASE_NEEDS_TEXTURE_COORDINATES0

            #if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
                #define ASE_SV_DEPTH SV_DepthLessEqual
                #define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
            #else
                #define ASE_SV_DEPTH SV_Depth
                #define ASE_SV_POSITION_QUALIFIERS
            #endif

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainColor;
            float _AlphaCutoff;
            float _CullMode;
            // ... 其他属性
            CBUFFER_END

            sampler2D _MainTex;

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float4 texcoord : TEXCOORD0;
            };

            struct PackedVaryings
            {
                ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 texcoord : TEXCOORD1;
            };

            PackedVaryings vert(Attributes input)
            {
                PackedVaryings output;
                output.texcoord.xy = input.texcoord.xy;
                output.texcoord.zw = 0;
                
                input.positionOS.xyz += float3(0, 0, 0);
                input.normalOS = input.normalOS;
                input.tangentOS = input.tangentOS;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                return output;
            }

            half4 frag(PackedVaryings input
                #if defined(ASE_DEPTH_WRITE_ON)
                , out float outputDepth : ASE_SV_DEPTH
                #endif
            ) : SV_Target
            {
                float2 uv_MainTex = input.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                float4 tex2DNode112 = tex2D(_MainTex, uv_MainTex);
                float alpha = tex2DNode112.a * _MainColor.a;

                // 关键修复：添加Alpha测试
                #ifdef _ALPHATEST_ON
                    clip(alpha - _AlphaCutoff);
                #endif

                float Alpha = alpha;

                #if defined(ASE_DEPTH_WRITE_ON)
                    float DeviceDepth = input.positionCS.z;
                #endif

                #if defined(ASE_DEPTH_WRITE_ON)
                    outputDepth = DeviceDepth;
                #endif

                return 0;
            }
            ENDHLSL
        }

        // 深度法线通道
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormalsOnly" }

            ZWrite On
            Blend One Zero
            ZTest LEqual
            Cull [_CullMode]

            HLSLPROGRAM

            #define _NORMAL_DROPOFF_TS 1
            #define ASE_TRANSLUCENCY 1
            #define _EMISSION
            #define _NORMALMAP 1

            // 关键修复：添加Alpha测试支持
            #pragma shader_feature _ _ALPHATEST_ON
            #pragma vertex vert
            #pragma fragment frag

            #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #define ASE_NEEDS_TEXTURE_COORDINATES0
            #define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
            
            #if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
                #define ASE_SV_DEPTH SV_DepthLessEqual
                #define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
            #else
                #define ASE_SV_DEPTH SV_Depth
                #define ASE_SV_POSITION_QUALIFIERS
            #endif

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainColor;
            float _AlphaCutoff;
            float _CullMode;
            float _NormalScale;
            float _NormalToggel;
            // ... 其他属性
            CBUFFER_END
            
            sampler2D _NormalMap;
            sampler2D _MainTex;

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 tangentOS : TANGENT;
                float4 texcoord : TEXCOORD0;
            };

            struct PackedVaryings
            {
                ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                half4 tangentWS : TEXCOORD2;
                float4 texcoord : TEXCOORD3;
            };

            PackedVaryings vert(Attributes input)
            {
                PackedVaryings output;
                output.texcoord.xy = input.texcoord.xy;
                output.texcoord.zw = 0;
                
                input.positionOS.xyz += float3(0, 0, 0);
                input.normalOS = input.normalOS;
                input.tangentOS = input.tangentOS;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS, (input.tangentOS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale());
                
                return output;
            }

            void frag(PackedVaryings input
                , out half4 outNormalWS : SV_Target0
                #if defined(ASE_DEPTH_WRITE_ON)
                , out float outputDepth : ASE_SV_DEPTH
                #endif
                #ifdef _WRITE_RENDERING_LAYERS
                , out float4 outRenderingLayers : SV_Target1
                #endif
            )
            {
                // 法线处理
                float2 uv_NormalMap = input.texcoord.xy;
                float3 normalSample = UnpackNormalScale(tex2D(_NormalMap, uv_NormalMap), _NormalScale);
                normalSample.z = lerp(1, normalSample.z, saturate(_NormalScale));
                float3 normalizedNormal = normalize(normalSample);
                float3 Normal = _NormalToggel ? normalizedNormal : float3(1, 1, 1);

                // 透明度处理
                float2 uv_MainTex = input.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                float4 tex2DNode112 = tex2D(_MainTex, uv_MainTex);
                float alpha = tex2DNode112.a * _MainColor.a;

                // 关键修复：添加Alpha测试
                #ifdef _ALPHATEST_ON
                    clip(alpha - _AlphaCutoff);
                #endif

                float Alpha = alpha;

                #if defined(ASE_DEPTH_WRITE_ON)
                    float DeviceDepth = input.positionCS.z;
                #endif

                #if defined(ASE_DEPTH_WRITE_ON)
                    outputDepth = DeviceDepth;
                #endif

                // 输出法线
                #if defined(_GBUFFER_NORMALS_OCT)
                    float2 octNormalWS = PackNormalOctQuadEncode(input.normalWS);
                    float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
                    half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
                    outNormalWS = half4(packedNormalWS, 0.0);
                #else
                    float3 normalWS = NormalizeNormalPerPixel(input.normalWS);
                    outNormalWS = half4(normalWS, 0.0);
                #endif

                #ifdef _WRITE_RENDERING_LAYERS
                    uint renderingLayers = GetMeshRenderingLayer();
                    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
                #endif
            }
            ENDHLSL
        }
    }
    CustomEditor "CharacterSkinShaderEditor"
    Fallback Off
}