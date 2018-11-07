using System;
using UnityEngine;
using UnityEditor;

internal class AFT_Effect_Simple_Shader_GUI : ShaderGUI
{
    public enum AFTBlendMode
    {
        Normal,
        Add,
        AddSmooth
    }

    public enum AFTOpenOrCloseMode
    {
        Close,
        Open
    }

    public static readonly string[] blendModeName = Enum.GetNames(typeof(AFTBlendMode));
    public static readonly string[] openOrClose = Enum.GetNames(typeof(AFTOpenOrCloseMode));

    MaterialEditor m_MaterialEditor;
    bool m_FirstTimeApply = true;
    
    MaterialProperty _Mode;
    MaterialProperty _TintColor;
    MaterialProperty _MainTex;
    MaterialProperty _InvFade;
    MaterialProperty _zwritee;
    MaterialProperty _culll;

    public void FindProperties(MaterialProperty[] props)
    {
        _Mode = FindProperty("_Mode", props);
        _TintColor = FindProperty("_TintColor", props);
        _MainTex = FindProperty("_MainTex", props);
        _InvFade = FindProperty("_InvFade", props);
        _zwritee = FindProperty("_zwritee", props);
        _culll = FindProperty("_culll", props);
    }

    public static void SetupMaterialWithFeatureMode(Material material, AFTBlendMode featureMode)
    {
        switch (featureMode)
        {
            case AFTBlendMode.Normal:
                material.EnableKeyword("_AFT_EFFECT_SIMPLE_NORMAL");
                material.DisableKeyword("_AFT_EFFECT_SIMPLE_ADD");
                material.DisableKeyword("_AFT_EFFECT_SIMPLE_ADD_SMOOTH");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case AFTBlendMode.Add:
                material.DisableKeyword("_AFT_EFFECT_SIMPLE_NORMAL");
                material.EnableKeyword("_AFT_EFFECT_SIMPLE_ADD");
                material.DisableKeyword("_AFT_EFFECT_SIMPLE_ADD_SMOOTH");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case AFTBlendMode.AddSmooth:
                material.DisableKeyword("_AFT_EFFECT_SIMPLE_NORMAL");
                material.DisableKeyword("_AFT_EFFECT_SIMPLE_ADD");
                material.EnableKeyword("_AFT_EFFECT_SIMPLE_ADD_SMOOTH");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                break;
        }
    }

    static void MaterialChanged(Material material)
    {
        SetupMaterialWithFeatureMode(material, (AFTBlendMode)material.GetFloat("_Mode"));
    }
    
    static void SetKeyword(Material m, string keyword, bool state)
    {
        if (state)
            m.EnableKeyword(keyword);
        else
            m.DisableKeyword(keyword);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        FindProperties(props);
        m_MaterialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        if (m_FirstTimeApply)
        {
            MaterialChanged(material);
            m_FirstTimeApply = false;
        }

        ShaderPropertiesGUI(material);
    }

    void ModePopup()
    {
        EditorGUI.showMixedValue = _Mode.hasMixedValue;
        var mode = (AFTBlendMode)_Mode.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (AFTBlendMode)EditorGUILayout.Popup("Rendering Mode", (int)mode, blendModeName);
        if (EditorGUI.EndChangeCheck())
        {
            m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
            _Mode.floatValue = (float)mode;
        }

        EditorGUI.showMixedValue = false;
    }

    void ZwritePopup()
    {
        EditorGUI.showMixedValue = _zwritee.hasMixedValue;
        var mode = (AFTOpenOrCloseMode)_zwritee.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (AFTOpenOrCloseMode)EditorGUILayout.Popup("ZWrite Mode", (int)mode, openOrClose);
        if (EditorGUI.EndChangeCheck())
        {
            m_MaterialEditor.RegisterPropertyChangeUndo("ZWrite Mode");
            _zwritee.floatValue = (float)mode;
        }

        EditorGUI.showMixedValue = false;
    }

    void CullPopup()
    {
        EditorGUI.showMixedValue = _culll.hasMixedValue;
        var mode = (AFTOpenOrCloseMode)_culll.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (AFTOpenOrCloseMode)EditorGUILayout.Popup("Cull Mode", (int)mode, openOrClose);
        if (EditorGUI.EndChangeCheck())
        {
            m_MaterialEditor.RegisterPropertyChangeUndo("Cull Mode");
            _culll.floatValue = (float)mode;
        }

        EditorGUI.showMixedValue = false;
    }

    public void ShaderPropertiesGUI(Material material)
    {
        EditorGUIUtility.labelWidth = 0f;

        EditorGUI.BeginChangeCheck();
        {
            ModePopup();
            ZwritePopup();
            CullPopup();
            m_MaterialEditor.ColorProperty(_TintColor, _TintColor.displayName);
            m_MaterialEditor.TextureProperty(_MainTex, _MainTex.displayName);
            m_MaterialEditor.FloatProperty(_InvFade, _InvFade.displayName);
        }
        if (EditorGUI.EndChangeCheck())
        {
            MaterialChanged(material);
        }
    }
}