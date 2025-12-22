using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum E_ResLoadType
{
    /// <summary>
    /// 编辑器模式下加载资源
    /// </summary>
    Editor,
    /// <summary>
    /// 发布模式下加载AB包资源
    /// </summary>
    AB,
    /// <summary>
    /// 发布模式下加载Resources资源
    /// </summary>
    Resources,
}

public class ResLoadSettings : ScriptableObject
{
    [Header("资源加载配置")]
    [Tooltip("资源加载的方式")]
    public E_ResLoadType resLoadType = E_ResLoadType.Editor;
    [Header("UI资源加载配置")]
    [Tooltip("使用Resources资源加载根路径")]
    public string uiResourceLoadRootPath = "UI/Panel";
    [Tooltip("使用AB包资源加载根路径")]
    public string uiABLoadRootPath = "ui";
    [Tooltip("使用Editor模式加载资源根路径")]
    public string uiEditorLoadRootPath = "Assets/ArtRes/UI/Panel";
    [Header("预制体资源加载配置")]
    [Tooltip("使用Resources资源加载预制体根路径")]
    public string prefabResourceLoadRootPath = "Prefabs";
    [Tooltip("使用AB包资源加载预制体根路径")]
    public string prefabABLoadRootPath = "prefabs";
    [Tooltip("使用Editor模式加载资源根路径")]
    public string prefabEditorLoadRootPath = "Assets/ArtRes/Prefabs";


    /// <summary>
    /// 配置文件的加载路径，Resources目录下的UIGeneratorSettings.asset
    /// </summary>
    private static readonly string SettingsPath = "Config/ResLoad/ResLoadSettings";

    // 静态实例（双重校验：编辑器/运行时）
    private static ResLoadSettings _instance;

    /// <summary>
    /// 获取全局配置实例（单例）
    /// </summary>
    public static ResLoadSettings Instance
    {
        get
        {
            // 运行时（打包/真机）：仅从Resources加载，不创建新文件
            if (_instance == null)
            {
                _instance = Resources.Load<ResLoadSettings>(SettingsPath);

                // 运行时如果加载不到，返回默认实例（不创建文件）
                if (_instance == null)
                {
#if UNITY_EDITOR
                    // 编辑器环境：创建配置文件
                    _instance = CreateSettingsInEditor();
#else
                    // 运行时/打包：创建内存级默认实例（不写入文件）
                    _instance = CreateInstance<ResLoadSettings>();
                    Debug.LogWarning($"运行时未找到配置文件 {SettingsPath}，使用内存默认配置");
#endif
                }
            }
            return _instance;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 仅在编辑器环境下创建配置文件（隔离编辑器代码）
    /// </summary>
    private static ResLoadSettings CreateSettingsInEditor()
    {
        Debug.LogWarning($"未找到{SettingsPath}配置文件，已创建默认配置");
        ResLoadSettings settings = CreateInstance<ResLoadSettings>();

        // 拼接Resources目录下的绝对路径
        string absoluteRootPath = Path.Combine(Application.dataPath, "Resources", SettingsPath);
        string dirName = Path.GetDirectoryName(absoluteRootPath);
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }

        // 转换为Unity资源路径（Assets/开头）
        string assetPath = Path.Combine("Assets/Resources", SettingsPath) + ".asset";

        // 编辑器专属操作：创建/保存资源
        AssetDatabase.CreateAsset(settings, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return settings;
    }
#endif

    public string GetUIPanelPath
    {
        get
        {
            switch (resLoadType)
            {
                case E_ResLoadType.Editor:
                    return uiEditorLoadRootPath;
                case E_ResLoadType.AB:
                    return uiABLoadRootPath;
                case E_ResLoadType.Resources:
                    return uiResourceLoadRootPath;
                default:
                    Debug.LogError($"未定义的资源加载类型 {resLoadType}");
                    return null;
            }
        }
    }

    public string GetPrefabPath
    {
        get
        {
            switch (resLoadType)
            {
                case E_ResLoadType.Editor:
                    return prefabEditorLoadRootPath;
                case E_ResLoadType.AB:
                    return prefabABLoadRootPath;
                case E_ResLoadType.Resources:
                    return prefabResourceLoadRootPath;
                default:
                    Debug.LogError($"未定义的资源加载类型 {resLoadType}");
                    return null;
            }
        }
    }
}
