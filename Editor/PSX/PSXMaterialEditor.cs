using ColbyO.VNTG.ColorPalette;
using ColbyO.VNTG.Editor;
using ColbyO.VNTG.PSX;
using ColbyO.VNTG.PSX.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXMaterialEditor.cs
//-----------------------------------------------------------------------
#if UNITY_EDITOR
public class PSXMaterialEditor : ShaderGUI
{
    private static readonly string _terrianShaderName = "VNTG/PSXTerrian_URP";
    private static readonly string _decalShaderName = "VNTG/PSXDecal_URP";
    private static readonly string _skyboxShaderName = "VNTG/PSXSkybox_URP";
    private static readonly string _waterShaderName = "VNTG/PSXWater_URP";

    private static readonly string[] _cullOptions = new string[] { "Off", "Front", "Back" };
    private static readonly string[] _surfaceOptions = new string[] { "Opaque", "Transparent" };
    private static readonly string[] _textureSamplingOptions = new string[] { "Point", "Bilinear", "N64" };
    private static readonly string[] _jitterOptions = new string[] { "Disabled", "View", "Screen" };
    private static readonly string[] _lightingOptions = new string[] { "Unlit", "Lit", "Texel Lit", "Vertex Lit" };
    private static readonly string[] _shadingOptions = new string[] { "Default", "Flat" };
    private static readonly string[] _ditherMethodOptions = new string[] { "Full", "Texture Rel" };
    private static readonly string[] _skyboxModeOptions = new string[] { "Cubemap", "Panoramic", "Procedual" };
    private static readonly string[] _procedualSkyboxModeOptions = new string[] { "Flat Gradient", "Space" };

    private bool _showSurfaceInputs = true;
    private bool _showRetroEffects = true;
    private bool _showLighting = true;
    private bool _showWater = true;
    private bool _showSkybox = true;
    private bool _showFlatGradientSkybox = true;
    private bool _showSpaceSkybox = true;
    private bool _showAdvancedOptions = false;

    MaterialProperty _surfaceType;
    MaterialProperty _cullMode;

    MaterialProperty _mainTex;
    MaterialProperty _textureSampleMode;
    MaterialProperty _baseColor;
    MaterialProperty _tiling;
    MaterialProperty _offset;

    MaterialProperty _hueShift;
    MaterialProperty _lightnessMultiplier;
    MaterialProperty _saturationMultiplier;

    MaterialProperty _enableTextureDownsampling;
    MaterialProperty _textureResolution;

    MaterialProperty _enableDithering;
    MaterialProperty _ditheringMethod;
    MaterialProperty _ditherStrength;
    MaterialProperty _ditherScale;
    MaterialProperty _usePalette;
    MaterialProperty _paletteLUT;
    MaterialProperty _useColorQuantization;
    MaterialProperty _colorDepth;
    MaterialProperty _useVertexColor;
    MaterialProperty _enableAffineTextureMapping;
    MaterialProperty _affineWrapStrength;

    MaterialProperty _lightingMethod;
    MaterialProperty _shadingMode;
    MaterialProperty _metalness;
    MaterialProperty _useSpecular;
    MaterialProperty _smoothness;
    MaterialProperty _specularColor;
    MaterialProperty _enableShadows;
    MaterialProperty _shadowTint;
    MaterialProperty _shadowDistCutoff;
    MaterialProperty _shadowOffset;

    MaterialProperty _vertexJitterMode;
    MaterialProperty _vertexPrecision;
    MaterialProperty _alphaClipThreshold;

    MaterialProperty _waterColor;
    MaterialProperty _waterCrestColor;
    MaterialProperty _waterSharpness;
    MaterialProperty _waterScale;
    MaterialProperty _waterSpeed;
    MaterialProperty _maxDepth;
    MaterialProperty _depthDecay;

    MaterialProperty _skyboxMode;
    MaterialProperty _procedualSkyboxMode;
    MaterialProperty _skyboxCubemap;
    MaterialProperty _skyboxRotation;

    MaterialProperty _flatGradientTopColor;
    MaterialProperty _flatGradientBottomColor;
    MaterialProperty _flatGradientTopAngle;
    MaterialProperty _flatGradientBottomAngle;

    MaterialProperty _enableNebula;
    MaterialProperty _nebulaPeriod;
    MaterialProperty _nebulaPower;
    MaterialProperty _nebulaColor;
    MaterialProperty _starDensity;
    MaterialProperty _starSharpness;

    Material targetMat;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GetProperties(properties);

        targetMat = (materialEditor.target as Material);

        EditorGUI.BeginChangeCheck();

        if (targetMat.shader == null || targetMat.shader.name == _skyboxShaderName)
        {
            _showSkybox = VNTGEditorGUIUtils.DrawSectionHeader(_showSurfaceInputs, "Skybox Options", () => ResetSkybox(materialEditor));
            if (_showSkybox)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawSkybox(materialEditor);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(2);

            if (_skyboxMode != null && _skyboxMode.floatValue == 2.0f)
            {
                if (_procedualSkyboxMode != null && _procedualSkyboxMode.floatValue == 0.0f)
                {
                    _showFlatGradientSkybox = VNTGEditorGUIUtils.DrawSectionHeader(_showFlatGradientSkybox, "Flat Gradient Skybox", () => ResetFlatGradientSkybox(materialEditor));
                    if (_showFlatGradientSkybox)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        DrawFlatGradientSkybox(materialEditor);
                        EditorGUILayout.EndVertical();
                    }
                }
                else if (_procedualSkyboxMode != null && _procedualSkyboxMode.floatValue == 1.0f)
                {
                    _showSpaceSkybox = VNTGEditorGUIUtils.DrawSectionHeader(_showSpaceSkybox, "Space Skybox", () => ResetSpaceSkybox(materialEditor)); ;
                    if (_showSpaceSkybox)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        DrawSpaceSkybox(materialEditor);
                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }

        if (
            targetMat.shader != null &&
            targetMat.shader.name != _waterShaderName &&
            (
                targetMat.shader.name != _skyboxShaderName ||
                _skyboxMode == null ||
                _skyboxMode.floatValue != 2.0f
            )
        )
        {
            _showSurfaceInputs = VNTGEditorGUIUtils.DrawSectionHeader(_showSurfaceInputs, "Surface Options", () => ResetSurfaceOptions(materialEditor));
            if (_showSurfaceInputs)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (
                    targetMat.shader != null &&
                    targetMat.shader.name == _skyboxShaderName &&
                    _skyboxMode != null &&
                    _skyboxMode.floatValue == 0.0f
               ) DrawSurfaceOptions(materialEditor, _skyboxCubemap);
                else DrawSurfaceOptions(materialEditor, _mainTex);

                EditorGUILayout.EndVertical();
            }
        }

        if (targetMat.shader != null && targetMat.shader.name == _waterShaderName)
        {
            EditorGUILayout.Space(2);

            _showWater = VNTGEditorGUIUtils.DrawSectionHeader(_showWater, "Water", () => ResetWater(materialEditor));
            if (_showWater)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawWater(materialEditor);
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space(2);

        _showRetroEffects = VNTGEditorGUIUtils.DrawSectionHeader(_showRetroEffects, "Retro Effects", () => ResetRetroEffects(materialEditor));
        if (_showRetroEffects)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawPSXEffects(materialEditor);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space(2);

        if (targetMat.shader != null && targetMat.shader.name != _skyboxShaderName)
        {
            _showLighting = VNTGEditorGUIUtils.DrawSectionHeader(_showLighting, "Lighting", () => ResetLightingOptions(materialEditor));
            if (_showLighting)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawLightingOptions(materialEditor);
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space(10);

        _showAdvancedOptions = VNTGEditorGUIUtils.DrawSectionHeader(_showAdvancedOptions, "Advanced Options");
        if (_showAdvancedOptions)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawAdvancedOptions(materialEditor);
            EditorGUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            materialEditor.RegisterPropertyChangeUndo("Modify Material Properties");
            materialEditor.PropertiesChanged();

            foreach (Object target in materialEditor.targets)
            {
                if (target is Material mat)
                {
                    SetupMaterialBlendMode(mat);
                }
            }

            EditorUtility.SetDirty(materialEditor.target);
        }
    }

    private void DrawSurfaceOptions(MaterialEditor materialEditor, MaterialProperty textureProp = null)
    {
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);

        if (_surfaceType != null && targetMat.shader != null && targetMat.shader.name != _decalShaderName && targetMat.shader.name != _skyboxShaderName) VNTGEditorGUIUtils.DrawEnumToolbar<SurfaceType>(materialEditor, _surfaceType, "Surface Type", _surfaceOptions);
        if (_cullMode != null && targetMat.shader != null && targetMat.shader.name != _decalShaderName && targetMat.shader.name != _skyboxShaderName) VNTGEditorGUIUtils.DrawEnumToolbar<CullMode>(materialEditor, _cullMode, "Face Culling", _cullOptions);
        if (_alphaClipThreshold != null) materialEditor.ShaderProperty(_alphaClipThreshold, "Alpha Clip Threshold");
        if (targetMat.shader != null && targetMat.shader.name != _decalShaderName && targetMat.shader.name != _skyboxShaderName && targetMat.shader.name != _terrianShaderName) EditorGUILayout.Space(16);

        if (textureProp != null)
        {
            GUIContent mainTexLabel = new GUIContent("Main Texture", "The primary color texture map.");
            materialEditor.TexturePropertySingleLine(mainTexLabel, textureProp, _baseColor);
        }
        else if (_baseColor != null)
        {
            materialEditor.ShaderProperty(_baseColor, "Base Color");
        }

        EditorGUI.indentLevel++;
        EditorGUI.indentLevel++;

        VNTGEditorGUIUtils.DrawEnumToolbar<PSXTextureSampleMode>(materialEditor, _textureSampleMode, "Texture Sampling Mode", _textureSamplingOptions);

        if (_textureSampleMode.floatValue == 2f && targetMat.shader != null && targetMat.shader.name == _terrianShaderName)
        {
            EditorGUILayout.HelpBox("Warning: N64 texture sampling is not supported on terrian shaders. Reverting to Bilinear.", MessageType.Warning);
        }

        if (_hueShift != null) materialEditor.ShaderProperty(_hueShift, "Hue Shift");
        if (_saturationMultiplier != null) materialEditor.ShaderProperty(_saturationMultiplier, "Saturation Multiplier");
        if (_lightnessMultiplier != null) materialEditor.ShaderProperty(_lightnessMultiplier, "Lightness Multiplier");

        if (
            _mainTex != null &&
            _mainTex.textureValue != null &&
            _tiling != null &&
            _offset != null &&
            (targetMat.shader != null && targetMat.shader.name != _skyboxShaderName)
        )
        {
            EditorGUILayout.BeginVertical();
            materialEditor.ShaderProperty(_tiling, "Tiling");
            GUILayout.Space(-16);
            materialEditor.ShaderProperty(_offset, "Offset");
            EditorGUILayout.EndVertical();
        }

        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
    }

    private void DrawPSXEffects(MaterialEditor materialEditor)
    {
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);

        VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _enableTextureDownsampling, "Texture Downsampling");
        if (_enableTextureDownsampling != null && _enableTextureDownsampling.floatValue == 1.0f && _textureResolution != null)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(_textureResolution, "Target Resolution");
            EditorGUI.indentLevel--;
        }

        VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _enableAffineTextureMapping, "Affine Texture Wraping");
        if (_enableAffineTextureMapping != null && _enableAffineTextureMapping.floatValue == 1.0f && _affineWrapStrength != null)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(_affineWrapStrength, "Warp Strength");
            EditorGUI.indentLevel--;
        }

        VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _usePalette, "Palette");
        if (_usePalette != null && _usePalette.floatValue == 1.0f && _paletteLUT != null)
        {
            EditorGUI.indentLevel++;
            DrawPaletteMaterialProperty(materialEditor, _paletteLUT);
            EditorGUI.indentLevel--;
        }

        VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _useColorQuantization, "Color Quantization");
        if (_useColorQuantization != null && _useColorQuantization.floatValue == 1.0f && _colorDepth != null)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(_colorDepth, "Color Depth");
            EditorGUI.indentLevel--;
        }

        VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _enableDithering, "Dithering");
        if (_enableDithering != null && _enableDithering.floatValue == 1.0f)
        {
            EditorGUI.indentLevel++;
            if (_ditheringMethod != null && _enableTextureDownsampling != null && _enableTextureDownsampling.floatValue == 1) VNTGEditorGUIUtils.DrawEnumToolbar<DitherMode>(materialEditor, _ditheringMethod, "Dither Mode", _ditherMethodOptions);
            if (_ditherStrength != null) materialEditor.ShaderProperty(_ditherStrength, "Dither Strength");
            if (_ditherScale != null) materialEditor.ShaderProperty(_ditherScale, "Dither Scale");
            EditorGUI.indentLevel--;
        }

        VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _useVertexColor, "Use Vertex Colors");

        VNTGEditorGUIUtils.DrawEnumToolbar<PSXVertexJitterMode>(materialEditor, _vertexJitterMode, "Vertex Jitter Mode", _jitterOptions);

        if (_vertexJitterMode != null && Mathf.RoundToInt(_vertexJitterMode.floatValue) != 0)
        {
            EditorGUI.indentLevel++;
            if (_vertexPrecision != null) materialEditor.ShaderProperty(_vertexPrecision, "Vertex Precision");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
    }

    private void DrawLightingOptions(MaterialEditor materialEditor)
    {
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);

        VNTGEditorGUIUtils.DrawEnumToolbar<PSXLightingMethod>(materialEditor, _lightingMethod, "Lighting Method", _lightingOptions, (int newMode) =>
        {
            if (_shadingMode == null) return;

            if (newMode != 1)
            {
                VNTGEditorGUIUtils.UpdateShaderGraphEnumKeyword(materialEditor, _shadingMode, 0, _shadingOptions);
            }
            else
            {
                VNTGEditorGUIUtils.UpdateShaderGraphEnumKeyword(materialEditor, _shadingMode, Mathf.RoundToInt(_shadingMode.floatValue), _shadingOptions);
            }
        });

        bool isUnlit = _lightingMethod != null && Mathf.RoundToInt(_lightingMethod.floatValue) == 0;
        bool isLit = _lightingMethod != null && Mathf.RoundToInt(_lightingMethod.floatValue) == 1;

        if (!isUnlit)
        {
            if (isLit && targetMat.shader != null && targetMat.shader.name != _decalShaderName)
            {
#if !UNITY_6000_3_OR_NEWER
                if (_shadingMode != null && _shadingMode.floatValue == 1.0f) 
                {
                    EditorGUILayout.HelpBox("Warning: Flat Shading is not supported in Unity 6.2 or lower.", MessageType.Warning);
                }
#endif
                VNTGEditorGUIUtils.DrawEnumToolbar<PSXShadingMode>(materialEditor, _shadingMode, "Shading Mode", _shadingOptions);
            }

            if (_metalness != null && targetMat.shader != null && targetMat.shader.name != _decalShaderName) materialEditor.ShaderProperty(_metalness, "Metalness");

            VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _useSpecular, "Enable Specular");
            if (_useSpecular != null && _useSpecular.floatValue == 1.0f)
            {
                EditorGUI.indentLevel++;
                if (_specularColor != null) materialEditor.ShaderProperty(_specularColor, "Specular Color");
                if (_smoothness != null) materialEditor.ShaderProperty(_smoothness, "Smoothness");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);

            VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _enableShadows, "Enable Custom Shadows");
            if (_enableShadows != null && _enableShadows.floatValue == 1.0f)
            {
                EditorGUI.indentLevel++;
                if (_shadowTint != null) materialEditor.ShaderProperty(_shadowTint, "Shadow Tint");
                if (_shadowDistCutoff != null) materialEditor.ShaderProperty(_shadowDistCutoff, "Distance Cutoff");
                if (_shadowOffset != null) materialEditor.ShaderProperty(_shadowOffset, "Shadow Bias");
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
    }

    private void DrawWater(MaterialEditor materialEditor)
    {
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);

        if (_waterColor != null) materialEditor.ShaderProperty(_waterColor, "Water Color");
        if (_waterCrestColor != null) materialEditor.ShaderProperty(_waterCrestColor, "Crest Color");
        if (_waterSharpness != null) materialEditor.ShaderProperty(_waterSharpness, "Water Sharpness");
        if (_waterScale != null) materialEditor.ShaderProperty(_waterScale, "Water Scale");
        if (_waterSpeed != null) materialEditor.ShaderProperty(_waterSpeed, "Water Speed");

        EditorGUILayout.Space(4);

        if (_maxDepth != null) materialEditor.ShaderProperty(_maxDepth, "Max Depth");
        if (_depthDecay != null) materialEditor.ShaderProperty(_depthDecay, "Depth Decay");

        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
    }

    private void DrawSkybox(MaterialEditor materialEditor)
    {
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);

        if (_skyboxMode != null)
        {
            VNTGEditorGUIUtils.DrawEnumToolbar<SkyboxMode>(materialEditor, _skyboxMode, "Skybox Mode", _skyboxModeOptions);
            if (_procedualSkyboxMode != null && _skyboxMode.floatValue == 2.0f)
            {
                VNTGEditorGUIUtils.DrawEnumToolbar<ProcedualSkyboxMode>(materialEditor, _procedualSkyboxMode, "Procedual Mode", _procedualSkyboxModeOptions);
            }

            if (
                _skyboxRotation != null &&
                (
                    _skyboxMode.floatValue != 2.0f ||
                    (_procedualSkyboxMode != null && _procedualSkyboxMode.floatValue != 0.0f)
                )
           ) materialEditor.ShaderProperty(_skyboxRotation, "Rotation");
        }

        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
    }

    private void DrawFlatGradientSkybox(MaterialEditor materialEditor)
    {
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);

        if (_flatGradientTopColor != null) materialEditor.ShaderProperty(_flatGradientTopColor, "Top Color");
        if (_flatGradientBottomColor != null) materialEditor.ShaderProperty(_flatGradientBottomColor, "Bottom Color");
        if (_flatGradientTopAngle != null) materialEditor.ShaderProperty(_flatGradientTopAngle, "Top Angle (Degrees)");
        if (_flatGradientBottomAngle != null) materialEditor.ShaderProperty(_flatGradientBottomAngle, "Bottom Angle (Degrees)");

        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
    }

    private void DrawSpaceSkybox(MaterialEditor materialEditor)
    {
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);

        if (_starDensity != null) materialEditor.ShaderProperty(_starDensity, "Star Density");
        if (_starSharpness != null) materialEditor.ShaderProperty(_starSharpness, "Star Sharpness");

        if (_enableNebula != null) VNTGEditorGUIUtils.DrawPropertyToggle(materialEditor, _enableNebula, "Enable Nebula");

        if (_enableNebula != null && _enableNebula.floatValue > 0.0f)
        {
            EditorGUI.indentLevel++;
            if (_nebulaColor != null) materialEditor.ShaderProperty(_nebulaColor, "Nebula Color");
            if (_nebulaPeriod != null) materialEditor.ShaderProperty(_nebulaPeriod, "Nebula Period");
            if (_nebulaPower != null) materialEditor.ShaderProperty(_nebulaPower, "Nebula Power");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
    }

    private void DrawAdvancedOptions(MaterialEditor materialEditor)
    {
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);

        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();

        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
    }

    private void ResetSurfaceOptions(MaterialEditor materialEditor)
    {
        materialEditor.RegisterPropertyChangeUndo("Reset Surface Properties");

        if (_surfaceType != null) _surfaceType.floatValue = 0f;
        if (_cullMode != null) _cullMode.floatValue = 2f;
        if (_alphaClipThreshold != null) _alphaClipThreshold.floatValue = 0.5f;
        if (_textureSampleMode != null) _textureSampleMode.floatValue = 0f;
        if (_hueShift != null) _hueShift.floatValue = 0f;
        if (_saturationMultiplier != null) _saturationMultiplier.floatValue = 1f;
        if (_lightnessMultiplier != null) _lightnessMultiplier.floatValue = 1f;
        if (_tiling != null) _tiling.vectorValue = new Vector4(1, 1, 0, 0);
        if (_offset != null) _offset.vectorValue = Vector4.zero;

        RegisterReset(materialEditor);
    }

    private void ResetRetroEffects(MaterialEditor materialEditor)
    {
        materialEditor.RegisterPropertyChangeUndo("Reset Retro Properties");

        if (_enableTextureDownsampling != null) _enableTextureDownsampling.floatValue = 0f;
        if (_textureResolution != null) _textureResolution.floatValue = 240f;
        if (_enableAffineTextureMapping != null) _enableAffineTextureMapping.floatValue = 0f;
        if (_affineWrapStrength != null) _affineWrapStrength.floatValue = 1f;
        if (_usePalette != null) _usePalette.floatValue = 0f;
        if (_paletteLUT != null) _paletteLUT.textureValue = null;
        if (_useColorQuantization != null) _useColorQuantization.floatValue = 0f;
        if (_colorDepth != null) _colorDepth.floatValue = 32f;
        if (_enableDithering != null) _enableDithering.floatValue = 0f;
        if (_ditheringMethod != null) _ditheringMethod.floatValue = 1f;
        if (_ditherStrength != null) _ditherStrength.floatValue = 1f;
        if (_ditherScale != null) _ditherScale.floatValue = 1f;
        if (_useVertexColor != null) _useVertexColor.floatValue = 1f;
        if (_vertexJitterMode != null) _vertexJitterMode.floatValue = 0f;
        if (_vertexPrecision != null) _vertexPrecision.floatValue = 32f;

        RegisterReset(materialEditor);
    }

    private void ResetLightingOptions(MaterialEditor materialEditor)
    {
        materialEditor.RegisterPropertyChangeUndo("Reset Lighting Properties");

        if (_lightingMethod != null) _lightingMethod.floatValue = 1f;
        if (_shadingMode != null) _shadingMode.floatValue = 0f;
        if (_metalness != null) _metalness.floatValue = 0f;
        if (_useSpecular != null) _useSpecular.floatValue = 0f;
        if (_smoothness != null) _smoothness.floatValue = 0.5f;
        if (_specularColor != null) _specularColor.colorValue = Color.white;
        if (_enableShadows != null) _enableShadows.floatValue = 0f;
        if (_shadowTint != null) _shadowTint.colorValue = Color.black;
        if (_shadowDistCutoff != null) _shadowDistCutoff.floatValue = 50f;
        if (_shadowOffset != null) _shadowOffset.floatValue = 0.05f;

        RegisterReset(materialEditor);
    }

    private void ResetWater(MaterialEditor materialEditor)
    {
        materialEditor.RegisterPropertyChangeUndo("Reset Water Properties");

        if (_waterColor != null) _waterColor.colorValue = Color.blue;
        if (_waterCrestColor != null) _waterCrestColor.colorValue = Color.white;
        if (_waterSharpness != null) _waterSharpness.floatValue = 5.0f;
        if (_waterScale != null) _waterScale.floatValue = 256.0f;
        if (_waterSpeed != null) _waterSpeed.floatValue = 1.0f;
        if (_maxDepth != null) _maxDepth.floatValue = 0.0f;
        if (_depthDecay != null) _depthDecay.floatValue = 2.0f;

        RegisterReset(materialEditor);
    }

    private void ResetSkybox(MaterialEditor materialEditor)
    {
        materialEditor.RegisterPropertyChangeUndo("Skybox Properties");

        if (_skyboxMode != null) _skyboxMode.floatValue = 2.0f;
        if (_procedualSkyboxMode != null) _procedualSkyboxMode.floatValue = 0.0f;
        if (_skyboxRotation != null) _skyboxRotation.floatValue = 0.0f;

        RegisterReset(materialEditor);
    }

    private void ResetFlatGradientSkybox(MaterialEditor materialEditor)
    {
        materialEditor.RegisterPropertyChangeUndo("Flat Gradient Skybox Properties");

        if (_flatGradientTopColor != null) _flatGradientTopColor.colorValue = Color.lightSkyBlue;
        if (_flatGradientBottomColor != null) _flatGradientBottomColor.colorValue = Color.orangeRed;
        if (_flatGradientTopAngle != null) _flatGradientTopAngle.floatValue = 45.0f;
        if (_flatGradientBottomAngle != null) _flatGradientBottomAngle.floatValue = -15.0f;

        RegisterReset(materialEditor);
    }

    private void ResetSpaceSkybox(MaterialEditor materialEditor)
    {
        materialEditor.RegisterPropertyChangeUndo("Space Skybox Properties");

        if (_enableNebula != null) _enableNebula.floatValue = 0;
        if (_nebulaPeriod != null) _nebulaPeriod.floatValue = 30f;
        if (_nebulaPower != null) _nebulaPower.floatValue = 5.0f;
        if (_nebulaColor != null) _nebulaColor.colorValue = new Color(0.4471f, 0.2706f, 0.5098f, 1.0f);
        if (_starDensity != null) _starDensity.floatValue = 0.5f;
        if (_starSharpness != null) _starSharpness.floatValue = 0.5f;

        RegisterReset(materialEditor);
    }

    private void RegisterReset(MaterialEditor materialEditor)
    {
        materialEditor.PropertiesChanged();

        foreach (UnityEngine.Object target in materialEditor.targets)
        {
            if (target is Material material)
            {
                MaterialEditor.ApplyMaterialPropertyDrawers(material);
            }
        }
    }

    private void SetupMaterialBlendMode(Material material)
    {
        if (_surfaceType == null) return;

        int surfaceTypeValue = Mathf.RoundToInt(_surfaceType.floatValue);

        if (surfaceTypeValue == 0)
        {
            material.SetOverrideTag("RenderType", "Opaque");

            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            material.SetInt("_SrcBlend", (int)BlendMode.One);
            material.SetInt("_DstBlend", (int)BlendMode.Zero);
            material.SetInt("_ZWrite", 1);

            material.renderQueue = (int)RenderQueue.Geometry;
        }
        else
        {
            material.SetOverrideTag("RenderType", "Transparent");

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");

            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);

            material.renderQueue = (int)RenderQueue.Transparent;
        }
    }

    private void DrawPaletteMaterialProperty(MaterialEditor materialEditor, MaterialProperty lutProperty)
    {
        Texture3D currentLUT = lutProperty.textureValue as Texture3D;
        Palette currentPalette = FindPaletteByLUT(currentLUT);

        GUIContent label = new GUIContent("Palette Asset", "Select a Palette asset.");
        Rect lineRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

        Rect labelRect = new Rect(lineRect.x, lineRect.y, EditorGUIUtility.labelWidth, lineRect.height);
        Rect fieldRect = new Rect(lineRect.x + EditorGUIUtility.labelWidth, lineRect.y, lineRect.width - EditorGUIUtility.labelWidth, lineRect.height);

        EditorGUI.LabelField(labelRect, label);

        if (Event.current.type == EventType.MouseDown && fieldRect.Contains(Event.current.mousePosition))
        {
            PopupWindow.Show(fieldRect, new PaletteSelectorPopup(selectedPalette =>
            {
                materialEditor.RegisterPropertyChangeUndo("Select Palette Asset");
                lutProperty.textureValue = (selectedPalette != null) ? selectedPalette.LUT : null;
            }));

            Event.current.Use();
        }

        if (currentPalette != null && currentPalette.colors != null && currentPalette.colors.Count > 0)
        {
            PalettePropertyDrawer.DrawPaletteBar(fieldRect, currentPalette.colors);
            GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            EditorGUI.DropShadowLabel(fieldRect, currentPalette.name, textStyle);
        }
        else
        {
            GUI.Box(fieldRect, currentLUT != null ? currentLUT.name : "None (Click to select Palette)");
        }
    }

    private Palette FindPaletteByLUT(Texture3D lut)
    {
        if (lut == null) return null;

        string[] guids = AssetDatabase.FindAssets("t:Palette");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Palette p = AssetDatabase.LoadAssetAtPath<Palette>(path);
            if (p != null && p.LUT == lut)
            {
                return p;
            }
        }
        return null;
    }

    private void GetProperties(MaterialProperty[] properties)
    {
        _surfaceType = FindProperty("_SurfaceType", properties, false);
        _cullMode = FindProperty("_Cull", properties, false);

        _mainTex = FindProperty("_MainTex", properties, false);
        _textureSampleMode = FindProperty("_TEXTURESAMPLEMODE", properties, false);
        _baseColor = FindProperty("_BaseColor", properties, false);
        _tiling = FindProperty("_Tiling", properties, false);
        _offset = FindProperty("_Offset", properties, false);

        _hueShift = FindProperty("_HueShift", properties, false);
        _lightnessMultiplier = FindProperty("_LightnessMultiplier", properties, false);
        _saturationMultiplier = FindProperty("_SaturationMultiplier", properties, false);
        _enableTextureDownsampling = FindProperty("_EnableTextureDownsampling", properties, false);
        _textureResolution = FindProperty("_TextureResolution", properties, false);

        _enableDithering = FindProperty("_EnableDithering", properties, false);
        _ditheringMethod = FindProperty("_DITHERINGMETHOD", properties, false);
        _ditherStrength = FindProperty("_DitherStrength", properties, false);
        _ditherScale = FindProperty("_DitherScale", properties, false);
        _usePalette = FindProperty("_UsePalette", properties, false);
        _paletteLUT = FindProperty("_PaletteLUT", properties, false);
        _useColorQuantization = FindProperty("_UseColorQuantization", properties, false);
        _colorDepth = FindProperty("_ColorDepth", properties, false);
        _useVertexColor = FindProperty("_UseVertexColor", properties, false);
        _enableAffineTextureMapping = FindProperty("_EnableAffineTextureMapping", properties, false);
        _affineWrapStrength = FindProperty("_AffineWrapStrength", properties, false);

        _lightingMethod = FindProperty("_LIGHTINGMETHOD", properties, false);
        _shadingMode = FindProperty("_SHADINGMETHOD", properties, false);
        _metalness = FindProperty("_Metalness", properties, false);
        _useSpecular = FindProperty("_UseSpecular", properties, false);
        _smoothness = FindProperty("_Smoothness", properties, false);
        _specularColor = FindProperty("_SpecularColor", properties, false);
        _enableShadows = FindProperty("_EnableShadows", properties, false);
        _shadowTint = FindProperty("_ShadowTint", properties, false);
        _shadowDistCutoff = FindProperty("_ShadowDistCutoff", properties, false);
        _shadowOffset = FindProperty("_ShadowOffset", properties, false);

        _vertexJitterMode = FindProperty("_VERTEXJITTERMODE", properties, false);
        _vertexPrecision = FindProperty("_VertexPrecision", properties, false);
        _alphaClipThreshold = FindProperty("_AlphaClipThreshold", properties, false);

        _waterColor = FindProperty("_WaterColor", properties, false);
        _waterCrestColor = FindProperty("_WaterCrestColor", properties, false);
        _waterSharpness = FindProperty("_WaterSharpness", properties, false);
        _waterScale = FindProperty("_WaterScale", properties, false);
        _waterSpeed = FindProperty("_WaterSpeed", properties, false);
        _maxDepth = FindProperty("_MaxDepth", properties, false);
        _depthDecay = FindProperty("_DepthDecay", properties, false);

        _skyboxMode = FindProperty("_SKYBOXMODE", properties, false);
        _procedualSkyboxMode = FindProperty("_PROCEDUALSKYBOXMODE", properties, false);
        _skyboxCubemap = FindProperty("_SkyboxCubemap", properties, false);
        _skyboxRotation = FindProperty("_SkyboxRotation", properties, false);

        _flatGradientTopColor = FindProperty("_FlatGradientTopColor", properties, false);
        _flatGradientBottomColor = FindProperty("_FlatGradientBottomColor", properties, false);
        _flatGradientTopAngle = FindProperty("_FlatGradientTopAngle", properties, false);
        _flatGradientBottomAngle = FindProperty("_FlatGradientBottomAngle", properties, false);

        _enableNebula = FindProperty("_EnableNebula", properties, false);
        _nebulaPeriod = FindProperty("_NebulaPeriod", properties, false);
        _nebulaPower = FindProperty("_NebulaPower", properties, false);
        _nebulaColor = FindProperty("_NebulaColor", properties, false);
        _starDensity = FindProperty("_StarDensity", properties, false);
        _starSharpness = FindProperty("_StarSharpness", properties, false);
    }

    private enum SurfaceType
    {
        Opaque,
        Transparent
    }

    private enum CullOptions
    {
        Off,
        Front,
        Back
    }
}
#endif