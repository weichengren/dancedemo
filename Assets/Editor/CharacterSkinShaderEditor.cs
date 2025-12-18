using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterSkinShaderEditor : ShaderGUI
{
    // 折叠面板状态
    private bool showSurfaceSettings = true;
    private bool showMainSettings = true;
    private bool showNormalSettings = false;
    private bool showMRSettings = false;
    private bool showMaskSettings = false;
    private bool showEmissionSettings = false;
    private bool showFlowSettings = false;
    private bool showAdvancedSettings = false;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;
        EditorGUI.BeginChangeCheck();

        // 1. 表面设置
        showSurfaceSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showSurfaceSettings, "🎭 表面设置");
        if (showSurfaceSettings)
        {
            DrawSurfaceProperties(materialEditor, properties);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 2. 主设置
        showMainSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMainSettings, "🌈 主设置");
        if (showMainSettings)
        {
            DrawMainProperties(materialEditor, properties);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 3. 法线设置
        showNormalSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showNormalSettings, "📐 法线设置");
        if (showNormalSettings)
        {
            DrawNormalProperties(materialEditor, properties);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 4. 金属粗糙度设置
        showMRSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMRSettings, "🔩 金属度粗糙度设置");
        if (showMRSettings)
        {
            DrawMRProperties(materialEditor, properties);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 5. 遮罩设置
        showMaskSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMaskSettings, "🎨 遮罩设置");
        if (showMaskSettings)
        {
            DrawMaskProperties(materialEditor, properties);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 6. 自发光设置
        showEmissionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showEmissionSettings, "💡 自发光设置");
        if (showEmissionSettings)
        {
            DrawEmissionProperties(materialEditor, properties);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 7. 流光设置
        showFlowSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showFlowSettings, "✨ 流光设置");
        if (showFlowSettings)
        {
            DrawFlowProperties(materialEditor, properties);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 8. 高级设置
        showAdvancedSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvancedSettings, "⚙️ 高级设置");
        if (showAdvancedSettings)
        {
            DrawAdvancedProperties(materialEditor, properties, targetMat);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 应用变更
        if (EditorGUI.EndChangeCheck())
        {
            UpdateRenderStates(targetMat);
            UpdateKeywords(targetMat);
        }

        // 添加调试信息
        DrawDebugInfo(targetMat);
    }

    #region 绘制属性区域
    private void DrawSurfaceProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty _Surface = FindProperty("_Surface", properties);
        MaterialProperty _BlendMode = FindProperty("_BlendMode", properties);
        MaterialProperty _ALPHATEST = FindProperty("_ALPHATEST", properties);
        MaterialProperty _AlphaCutoff = FindProperty("_AlphaCutoff", properties);
        MaterialProperty _QueueMode = FindProperty("_QueueMode", properties);
        MaterialProperty _CullMode = FindProperty("_CullMode", properties);
        MaterialProperty _ZWriteMode = FindProperty("_ZWriteMode", properties);
        MaterialProperty _ColorMask = FindProperty("_ColorMask", properties);
        MaterialProperty _SrcBlend = FindProperty("_SrcBlend", properties);
        MaterialProperty _DstBlend = FindProperty("_DstBlend", properties);

        // 表面类型
        materialEditor.ShaderProperty(_Surface, _Surface.displayName);

        // 透明模式特有属性
        if (_Surface.floatValue == 1)
        {
            materialEditor.ShaderProperty(_BlendMode, _BlendMode.displayName);
            materialEditor.ShaderProperty(_ALPHATEST, _ALPHATEST.displayName);

            if (_ALPHATEST.floatValue > 0)
                materialEditor.ShaderProperty(_AlphaCutoff, _AlphaCutoff.displayName);

            EditorGUILayout.HelpBox($"当前混合模式: {GetBlendModeName((int)_BlendMode.floatValue)}", MessageType.Info);
        }
        // 不透明模式特有属性
        else
        {
            materialEditor.ShaderProperty(_ALPHATEST, _ALPHATEST.displayName);
            if (_ALPHATEST.floatValue > 0)
                materialEditor.ShaderProperty(_AlphaCutoff, _AlphaCutoff.displayName);
        }

        // 渲染状态 - 部分属性在特定模式下灰显
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("渲染状态控制", EditorStyles.boldLabel);
        
        // 队列模式在特定情况下灰显（自动控制）
        using (new EditorGUI.DisabledScope(ShouldDisableQueueControl(_Surface.floatValue, _ALPHATEST.floatValue)))
        {
            materialEditor.ShaderProperty(_QueueMode, _QueueMode.displayName);
        }
        
        // 剔除模式始终可手动修改
        materialEditor.ShaderProperty(_CullMode, _CullMode.displayName);
        
        // ZWrite在不透明模式下灰显（强制开启），透明模式可手动修改
        using (new EditorGUI.DisabledScope(ShouldDisableZWriteControl(_Surface.floatValue)))
        {
            materialEditor.ShaderProperty(_ZWriteMode, _ZWriteMode.displayName);
        }
        
        materialEditor.ShaderProperty(_ColorMask, _ColorMask.displayName);

        // 高级混合设置（专家模式）
        EditorGUILayout.Space();
        bool showAdvancedBlend = EditorGUILayout.Toggle("显示高级混合设置", false);
        if (showAdvancedBlend)
        {
            EditorGUILayout.HelpBox("高级设置仅供专家使用，一般情况不需要修改", MessageType.Warning);
            materialEditor.ShaderProperty(_SrcBlend, _SrcBlend.displayName);
            materialEditor.ShaderProperty(_DstBlend, _DstBlend.displayName);
        }

        // 快速设置按钮
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("快速预设", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("标准不透明")) SetStandardOpaque(materialEditor.target as Material);
        if (GUILayout.Button("标准透明")) SetStandardTransparent(materialEditor.target as Material);
        if (GUILayout.Button("叠加发光")) SetAdditiveBlend(materialEditor.target as Material);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cutout镂空")) SetCutoutPreset(materialEditor.target as Material);
        if (GUILayout.Button("双面渲染")) SetDoubleSidedPreset(materialEditor.target as Material);
        GUILayout.EndHorizontal();
    }

    private void DrawMainProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty _MainColor = FindProperty("_MainColor", properties);
        MaterialProperty _MainTex = FindProperty("_MainTex", properties);
        MaterialProperty _LightScatter = FindProperty("_LightScatter", properties);
        MaterialProperty _BaseToneGlow = FindProperty("_BaseToneGlow", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("主纹理"), _MainTex, _MainColor);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("光照设置", EditorStyles.boldLabel);
        materialEditor.ShaderProperty(_LightScatter, _LightScatter.displayName);
        materialEditor.ShaderProperty(_BaseToneGlow, _BaseToneGlow.displayName);
    }

    private void DrawNormalProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty _NormalToggel = FindProperty("_NormalToggel", properties);
        MaterialProperty _NormalMap = FindProperty("_NormalMap", properties);
        MaterialProperty _NormalScale = FindProperty("_NormalScale", properties);

        materialEditor.ShaderProperty(_NormalToggel, _NormalToggel.displayName);
        if (_NormalToggel.floatValue > 0)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("法线贴图"), _NormalMap);
            materialEditor.ShaderProperty(_NormalScale, _NormalScale.displayName);
            
            if (_NormalMap.textureValue == null)
            {
                EditorGUILayout.HelpBox("请指定法线贴图纹理", MessageType.Warning);
            }
        }
    }

    private void DrawMRProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty _MetalRoughnessToggel = FindProperty("_MetalRoughnessToggel", properties);
        MaterialProperty _MetalRoughness = FindProperty("_MetalRoughness", properties);
        MaterialProperty _MetallicValue = FindProperty("_MetallicValue", properties);
        MaterialProperty _RoughnessValue = FindProperty("_RoughnessValue", properties);
        MaterialProperty _OcclusionToggel = FindProperty("_OcclusionToggel", properties);
        MaterialProperty _Occlusion = FindProperty("_Occlusion", properties);

        materialEditor.ShaderProperty(_MetalRoughnessToggel, _MetalRoughnessToggel.displayName);
        if (_MetalRoughnessToggel.floatValue > 0)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("金属粗糙度贴图"), _MetalRoughness);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PBR参数", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(_MetallicValue, _MetallicValue.displayName);
            materialEditor.ShaderProperty(_RoughnessValue, _RoughnessValue.displayName);
            
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(_OcclusionToggel, _OcclusionToggel.displayName);
            if (_OcclusionToggel.floatValue > 0)
                materialEditor.ShaderProperty(_Occlusion, _Occlusion.displayName);

            // 贴图通道说明
            if (_MetalRoughness.textureValue != null)
            {
                EditorGUILayout.HelpBox(
                    "贴图通道说明:\n" +
                    "R: 金属度 | G: 粗糙度 | B: 环境光遮蔽", 
                    MessageType.Info);
            }
        }
    }

    private void DrawMaskProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty _Mask = FindProperty("_Mask", properties);
        MaterialProperty _MouthToggel = FindProperty("_MouthToggel", properties);
        MaterialProperty _MouthColor = FindProperty("_MouthColor", properties);
        MaterialProperty _Contrast = FindProperty("_Contrast", properties);
        MaterialProperty _Saturation = FindProperty("_Saturation", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("遮罩贴图(RGBA)"), _Mask);
        
        if (_Mask.textureValue != null)
        {
            EditorGUILayout.HelpBox(
                "遮罩通道说明:\n" +
                "R: 嘴部区域 | G: 自发光区域 | B: 流光区域 | A: SSS厚度", 
                MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("嘴部设置 (R通道)", EditorStyles.boldLabel);
        materialEditor.ShaderProperty(_MouthToggel, _MouthToggel.displayName);
        if (_MouthToggel.floatValue > 0)
        {
            materialEditor.ShaderProperty(_MouthColor, _MouthColor.displayName);
            materialEditor.ShaderProperty(_Contrast, _Contrast.displayName);
            materialEditor.ShaderProperty(_Saturation, _Saturation.displayName);
        }
    }

    private void DrawEmissionProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty _EmissionToggel = FindProperty("_EmissionToggel", properties);
        MaterialProperty _EmissionColor = FindProperty("_EmissionColor", properties);
        MaterialProperty _EmissionValue = FindProperty("_EmissionValue", properties);

        materialEditor.ShaderProperty(_EmissionToggel, _EmissionToggel.displayName);
        if (_EmissionToggel.floatValue > 0)
        {
            materialEditor.ShaderProperty(_EmissionColor, _EmissionColor.displayName);
            materialEditor.ShaderProperty(_EmissionValue, _EmissionValue.displayName);
            
            // 显示强度预览
            float previewIntensity = _EmissionValue.floatValue;
            Color previewColor = _EmissionColor.colorValue * previewIntensity;
            EditorGUILayout.ColorField(new GUIContent("预览颜色"), previewColor, false, false, false);
        }
    }

    private void DrawFlowProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty _LiuguangToggle = FindProperty("_LiuguangToggle", properties);
        MaterialProperty _LiuguangColor = FindProperty("_LiuguangColor", properties);
        MaterialProperty _LiuguangTex = FindProperty("_LiuguangTex", properties);
        MaterialProperty _CoordinateSpace = FindProperty("_CoordinateSpace", properties);
        MaterialProperty _LiuguangXYZW = FindProperty("_LiuguangXYZW", properties);
        MaterialProperty _LiuguangIntencity = FindProperty("_LiuguangIntencity", properties);
        MaterialProperty _LiuguangSpeed = FindProperty("_LiuguangSpeed", properties);

        materialEditor.ShaderProperty(_LiuguangToggle, _LiuguangToggle.displayName);
        if (_LiuguangToggle.floatValue > 0)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent("流光纹理"), _LiuguangTex);
            materialEditor.ShaderProperty(_LiuguangColor, _LiuguangColor.displayName);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("动画参数", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(_CoordinateSpace, _CoordinateSpace.displayName);
            materialEditor.ShaderProperty(_LiuguangXYZW, "平铺(XY)与偏移(ZW)");
            materialEditor.ShaderProperty(_LiuguangSpeed, _LiuguangSpeed.displayName);
            materialEditor.ShaderProperty(_LiuguangIntencity, _LiuguangIntencity.displayName);

            // 坐标空间说明
            EditorGUILayout.HelpBox(
                $"坐标空间: {GetCoordinateSpaceName((int)_CoordinateSpace.floatValue)}\n" +
                "UV: 使用UV坐标\n" +
                "ObjectPos: 使用物体空间位置\n" +
                "WorldPos: 使用世界空间位置\n" +
                "ViewPos: 使用视图空间位置", 
                MessageType.Info);
        }
    }

    private void DrawAdvancedProperties(MaterialEditor materialEditor, MaterialProperty[] properties, Material targetMat)
    {
        MaterialProperty _CastShadows = FindProperty("_CastShadows", properties);
        MaterialProperty _ReceiveShadows = FindProperty("_ReceiveShadows", properties);

        // 投影控制（移动到高级设置）
        EditorGUILayout.LabelField("投影控制", EditorStyles.boldLabel);
        materialEditor.ShaderProperty(_CastShadows, _CastShadows.displayName);
        materialEditor.ShaderProperty(_ReceiveShadows, _ReceiveShadows.displayName);

        // 投影状态说明
        string castStatus = _CastShadows.floatValue > 0 ? "开启" : "关闭";
        string receiveStatus = _ReceiveShadows.floatValue > 0 ? "开启" : "关闭";
        
        EditorGUILayout.HelpBox(
            $"当前投影状态:\n" +
            $"• 产生投影: {castStatus}\n" +
            $"• 接受投影: {receiveStatus}",
            MessageType.Info);

        // 投影预设
        EditorGUILayout.LabelField("投影预设", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("全开", EditorStyles.miniButton))
        {
            _CastShadows.floatValue = 1;
            _ReceiveShadows.floatValue = 1;
        }
        if (GUILayout.Button("全关", EditorStyles.miniButton))
        {
            _CastShadows.floatValue = 0;
            _ReceiveShadows.floatValue = 0;
        }
        if (GUILayout.Button("仅产生", EditorStyles.miniButton))
        {
            _CastShadows.floatValue = 1;
            _ReceiveShadows.floatValue = 0;
        }
        if (GUILayout.Button("仅接受", EditorStyles.miniButton))
        {
            _CastShadows.floatValue = 0;
            _ReceiveShadows.floatValue = 1;
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 次表面散射设置
        EditorGUILayout.LabelField("次表面散射", EditorStyles.boldLabel);
        MaterialProperty _TransShadow = FindProperty("_TransShadow", properties);
        MaterialProperty _TransAmbient = FindProperty("_TransAmbient", properties);

        materialEditor.ShaderProperty(_TransShadow, _TransShadow.displayName);
        materialEditor.ShaderProperty(_TransAmbient, _TransAmbient.displayName);

        // 显示隐藏参数的当前值（只读信息）
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("内部参数（自动设置）", EditorStyles.boldLabel);
        
        if (targetMat.HasProperty("_TransStrength"))
        {
            float transStrength = targetMat.GetFloat("_TransStrength");
            EditorGUILayout.LabelField($"透射强度: {transStrength}", EditorStyles.miniLabel);
        }
        
        if (targetMat.HasProperty("_TransNormal"))
        {
            float transNormal = targetMat.GetFloat("_TransNormal");
            EditorGUILayout.LabelField($"法线影响度: {transNormal}", EditorStyles.miniLabel);
        }
        
        if (targetMat.HasProperty("_TransScattering"))
        {
            float transScattering = targetMat.GetFloat("_TransScattering");
            EditorGUILayout.LabelField($"散射强度: {transScattering}", EditorStyles.miniLabel);
        }
        
        if (targetMat.HasProperty("_TransDirect"))
        {
            float transDirect = targetMat.GetFloat("_TransDirect");
            EditorGUILayout.LabelField($"直接光透明度: {transDirect}", EditorStyles.miniLabel);
        }

        // 性能提示
        EditorGUILayout.Space();
        DrawPerformanceTips(targetMat);
    }

    private void DrawDebugInfo(Material targetMat)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🔍 调试信息", EditorStyles.boldLabel);
        
        // 当前状态显示
        string surfaceType = targetMat.GetFloat("_Surface") == 1 ? "透明" : "不透明";
        string blendMode = targetMat.GetFloat("_Surface") == 1 ? 
            GetBlendModeName((int)targetMat.GetFloat("_BlendMode")) : "无混合";
        string zWrite = targetMat.GetFloat("_ZWriteMode") == 1 ? "开启" : "关闭";
        string cullMode = GetCullModeName((int)targetMat.GetFloat("_CullMode"));
        string castShadows = targetMat.GetFloat("_CastShadows") > 0 ? "开启" : "关闭";
        string receiveShadows = targetMat.GetFloat("_ReceiveShadows") > 0 ? "开启" : "关闭";
        string alphaTest = targetMat.IsKeywordEnabled("_ALPHATEST_ON") ? "开启" : "关闭";

        EditorGUILayout.LabelField($"表面类型: {surfaceType}", EditorStyles.miniLabel);
        if (targetMat.GetFloat("_Surface") == 1)
            EditorGUILayout.LabelField($"混合模式: {blendMode}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"深度写入: {zWrite}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"剔除模式: {cullMode}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"产生投影: {castShadows}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"接受投影: {receiveShadows}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Alpha测试: {alphaTest}", EditorStyles.miniLabel);
        if (targetMat.IsKeywordEnabled("_ALPHATEST_ON"))
            EditorGUILayout.LabelField($"Alpha阈值: {targetMat.GetFloat("_AlphaCutoff")}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"渲染队列: {targetMat.renderQueue}", EditorStyles.miniLabel);

        // 关键字状态
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("关键字状态", EditorStyles.miniBoldLabel);
        DrawKeywordStatus(targetMat, "_CAST_SHADOWS_ON", "产生投影");
        DrawKeywordStatus(targetMat, "_RECEIVE_SHADOWS_ON", "接受投影");
        DrawKeywordStatus(targetMat, "_ALPHATEST_ON", "Alpha测试");
        DrawKeywordStatus(targetMat, "_NORMALMAP", "法线贴图");
        DrawKeywordStatus(targetMat, "_EMISSION", "自发光");
        DrawKeywordStatus(targetMat, "_LIUGUANG_ON", "流光效果");
    }

    private void DrawKeywordStatus(Material mat, string keyword, string displayName)
    {
        bool isEnabled = mat.IsKeywordEnabled(keyword);
        EditorGUILayout.LabelField($"{displayName}: {(isEnabled ? "开启" : "关闭")}", 
            isEnabled ? EditorStyles.whiteLabel : EditorStyles.miniLabel);
    }

    private void DrawPerformanceTips(Material material)
    {
        int complexityScore = CalculateComplexityScore(material);
        
        EditorGUILayout.LabelField("性能分析", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"复杂度评分: {complexityScore}/10", EditorStyles.miniLabel);
        
        if (complexityScore > 7)
        {
            EditorGUILayout.HelpBox("材质复杂度较高，建议优化", MessageType.Warning);
        }
        else if (complexityScore > 5)
        {
            EditorGUILayout.HelpBox("材质复杂度中等", MessageType.Info);
        }

        // Alpha测试性能提示
        if (material.IsKeywordEnabled("_ALPHATEST_ON"))
        {
            EditorGUILayout.HelpBox("Alpha测试已开启，阴影投射将正确显示镂空效果", MessageType.Info);
        }

        // 具体优化建议
        if (material.GetFloat("_LiuguangToggle") > 0 && material.GetFloat("_EmissionToggel") > 0)
        {
            EditorGUILayout.HelpBox("同时开启流光和自发光会增加性能开销", MessageType.Info);
        }

        if (material.GetFloat("_CastShadows") > 0)
        {
            EditorGUILayout.HelpBox("产生投影会增加阴影渲染开销", MessageType.Info);
        }

        if (material.GetFloat("_ReceiveShadows") > 0)
        {
            EditorGUILayout.HelpBox("接受投影会增加片元着色器计算", MessageType.Info);
        }
    }

    private int CalculateComplexityScore(Material material)
    {
        int score = 0;
        
        if (material.GetFloat("_NormalToggel") > 0) score += 1;
        if (material.GetFloat("_MetalRoughnessToggel") > 0) score += 2;
        if (material.GetFloat("_EmissionToggel") > 0) score += 1;
        if (material.GetFloat("_LiuguangToggle") > 0) score += 3;
        if (material.GetFloat("_Surface") == 1) score += 1;
        if (material.GetFloat("_CastShadows") > 0) score += 1;
        if (material.GetFloat("_ReceiveShadows") > 0) score += 1;
        if (material.IsKeywordEnabled("_ALPHATEST_ON")) score += 1;
        
        return Mathf.Min(score, 10);
    }
    #endregion

    #region 渲染状态更新
    private void UpdateRenderStates(Material material)
    {
        // 根据表面类型和Alpha测试自动设置队列
        UpdateQueueMode(material);
        
        // 根据表面类型自动设置ZWrite（核心逻辑）
        UpdateCullAndZWrite(material);

        // 设置混合模式
        if (material.GetFloat("_Surface") == 1) // 透明模式
        {
            material.SetOverrideTag("RenderType", "Transparent");
            switch ((int)material.GetFloat("_BlendMode"))
            {
                case 0: // Additive
                    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)BlendMode.One);
                    break;
                case 1: // Alpha
                    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    break;
                case 2: // Multiply
                    material.SetInt("_SrcBlend", (int)BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)BlendMode.Zero);
                    break;
            }
        }
        else // 不透明模式
        {
            material.SetOverrideTag("RenderType", material.GetFloat("_ALPHATEST") > 0 ? "TransparentCutout" : "Opaque");
            material.SetInt("_SrcBlend", (int)BlendMode.One);
            material.SetInt("_DstBlend", (int)BlendMode.Zero);
        }

        // 设置渲染队列
        material.renderQueue = (int)material.GetFloat("_QueueMode");
    }

    private void UpdateQueueMode(Material material)
    {
        float surfaceType = material.GetFloat("_Surface");
        float alphaTest = material.GetFloat("_ALPHATEST");

        if (surfaceType == 0) // 不透明
        {
            if (alphaTest > 0) // 启用Alpha测试
            {
                material.SetFloat("_QueueMode", 2450); // AlphaTest队列
            }
            else
            {
                material.SetFloat("_QueueMode", 2000); // Geometry队列
            }
        }
        else // 透明
        {
            material.SetFloat("_QueueMode", 3000); // Transparent队列
        }
    }

    private void UpdateCullAndZWrite(Material material)
    {
        float surfaceType = material.GetFloat("_Surface");
        float lastSurfaceType = material.HasProperty("_LastSurfaceType") ? material.GetFloat("_LastSurfaceType") : -1;

        // 记录当前表面类型用于下次判断
        material.SetFloat("_LastSurfaceType", surfaceType);

        if (surfaceType == 1) // 透明模式
        {
            // 关键逻辑：当从非透明模式切换到透明模式时，强制设置ZWrite为Off
            if (lastSurfaceType != 1)
            {
                material.SetFloat("_ZWriteMode", 0); // 强制Off
            }
            // 后续保持用户手动修改的值（不做强制修改）
        }
        else // 不透明模式
        {
            // 强制开启ZWrite，忽略用户设置
            material.SetFloat("_ZWriteMode", 1);
        }

        // 初始化剔除模式默认值（仅首次）
        if (material.GetFloat("_CullMode") < 0)
            material.SetFloat("_CullMode", 2); // 默认背面剔除
    }

    private bool ShouldDisableQueueControl(float surfaceType, float alphaTest)
    {
        // 透明模式或不透明+Alpha测试时自动控制队列，禁用手动修改
        return surfaceType == 1 || (surfaceType == 0 && alphaTest > 0);
    }

    private bool ShouldDisableZWriteControl(float surfaceType)
    {
        // 不透明模式下禁用ZWrite手动修改（强制开启）
        return surfaceType == 0;
    }

    private void UpdateKeywords(Material material)
    {
        // Alpha测试关键字 - 关键修复：在所有通道中保持一致
        if (material.GetFloat("_ALPHATEST") > 0)
        {
            material.EnableKeyword("_ALPHATEST_ON");
        }
        else
        {
            material.DisableKeyword("_ALPHATEST_ON");
        }

        // 表面类型关键字
        if (material.GetFloat("_Surface") == 1)
        {
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
        }
        else
        {
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_SURFACE_TYPE_OPAQUE");
        }

        // 功能开关关键字
        SetKeyword(material, "_NORMALMAP", material.GetFloat("_NormalToggel") > 0);
        SetKeyword(material, "_EMISSION", material.GetFloat("_EmissionToggel") > 0);
        SetKeyword(material, "_LIUGUANG_ON", material.GetFloat("_LiuguangToggle") > 0);
        
        // 投影控制关键字 - 关键修复：使用正确的关键字
        SetKeyword(material, "_CAST_SHADOWS_ON", material.GetFloat("_CastShadows") > 0);
        SetKeyword(material, "_RECEIVE_SHADOWS_ON", material.GetFloat("_ReceiveShadows") > 0);
    }

    private void SetKeyword(Material material, string keyword, bool enable)
    {
        if (enable)
            material.EnableKeyword(keyword);
        else
            material.DisableKeyword(keyword);
    }
    #endregion

    #region 快速预设
    private void SetStandardOpaque(Material mat)
    {
        mat.SetFloat("_Surface", 0);
        mat.SetFloat("_BlendMode", 1);
        mat.SetFloat("_ALPHATEST", 0);
        mat.SetFloat("_QueueMode", 2000);
        mat.SetFloat("_CullMode", 2);
        mat.SetFloat("_ZWriteMode", 1);
        mat.SetFloat("_ColorMask", 15);
        mat.SetFloat("_LastSurfaceType", 0);
        mat.SetFloat("_CastShadows", 1);
        mat.SetFloat("_ReceiveShadows", 1);
        UpdateKeywords(mat);
        UpdateRenderStates(mat);
    }

    private void SetStandardTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_BlendMode", 1);
        mat.SetFloat("_ALPHATEST", 0);
        mat.SetFloat("_QueueMode", 3000);
        mat.SetFloat("_ZWriteMode", 0);
        mat.SetFloat("_ColorMask", 15);
        mat.SetFloat("_LastSurfaceType", 1);
        mat.SetFloat("_CastShadows", 0);
        mat.SetFloat("_ReceiveShadows", 1);
        UpdateKeywords(mat);
        UpdateRenderStates(mat);
    }

    private void SetAdditiveBlend(Material mat)
    {
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_BlendMode", 0);
        mat.SetFloat("_ALPHATEST", 0);
        mat.SetFloat("_QueueMode", 3000);
        mat.SetFloat("_ZWriteMode", 0);
        mat.SetFloat("_ColorMask", 15);
        mat.SetFloat("_LastSurfaceType", 1);
        mat.SetFloat("_CastShadows", 0);
        mat.SetFloat("_ReceiveShadows", 1);
        UpdateKeywords(mat);
        UpdateRenderStates(mat);
    }

    private void SetCutoutPreset(Material mat)
    {
        mat.SetFloat("_Surface", 0);
        mat.SetFloat("_ALPHATEST", 1);
        mat.SetFloat("_AlphaCutoff", 0.5f);
        mat.SetFloat("_QueueMode", 2450);
        mat.SetFloat("_ZWriteMode", 1);
        mat.SetFloat("_LastSurfaceType", 0);
        mat.SetFloat("_CastShadows", 1);
        mat.SetFloat("_ReceiveShadows", 1);
        UpdateKeywords(mat);
        UpdateRenderStates(mat);
        EditorUtility.DisplayDialog("预设应用", "已设置为Cutout模式，Alpha测试将正确影响阴影投射", "确定");
    }

    private void SetDoubleSidedPreset(Material mat)
    {
        mat.SetFloat("_CullMode", 0); // 双面渲染
        UpdateRenderStates(mat);
        EditorUtility.DisplayDialog("预设应用", "已设置为双面渲染模式", "确定");
    }
    #endregion

    #region 辅助方法
    private string GetBlendModeName(int mode)
    {
        switch (mode)
        {
            case 0: return "Additive (叠加)";
            case 1: return "Alpha (透明混合)";
            case 2: return "Multiply (相乘)";
            default: return "未知";
        }
    }

    private string GetCullModeName(int mode)
    {
        switch (mode)
        {
            case 0: return "Off (双面)";
            case 1: return "Front (正面剔除)";
            case 2: return "Back (背面剔除)";
            default: return "未知";
        }
    }

    private string GetCoordinateSpaceName(int mode)
    {
        switch (mode)
        {
            case 0: return "UV坐标";
            case 1: return "物体空间";
            case 2: return "世界空间";
            case 3: return "视图空间";
            default: return "未知";
        }
    }
    #endregion
}