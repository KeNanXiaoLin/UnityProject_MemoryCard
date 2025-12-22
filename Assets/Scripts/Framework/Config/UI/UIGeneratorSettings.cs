using UnityEngine;
using UnityEditor;
using System.IO;


/// <summary>
/// UI生成工具全局配置（可在Project Settings中编辑）
/// </summary>
[CreateAssetMenu(fileName = "UIGeneratorSettings", menuName = "UI生成工具/创建配置文件")]
public class UIGeneratorSettings : ScriptableObject
{
    #region 自动UI面板生成相关配置
    [Header("基础配置")]
    [Tooltip("生成脚本的根路径（相对于项目根目录）")]
    public string scriptGenerateRootPath = "Assets/Scripts/UI/AutoGenerate";

    [Tooltip("默认命名空间")]
    public string defaultNamespace = "MyFrameWork";

    [Header("备份配置")]
    [Tooltip("是否启用备份功能")]
    public bool enableBackup = true;
    [Tooltip("备份文件存储子目录（相对于生成脚本路径）")]
    public string backupSubPath = "Backup";

    [Tooltip("最大保留备份数量")]
    [Min(1)] public int maxBackupCount = 10;

    [Tooltip("备份文件后缀")]
    public string backupFileSuffix = "_Backup";

    [Tooltip("备份文件自定义扩展名")]
    public string backupFileExtension = ".uibak";

    [Header("标记常量")]
    [Tooltip("新增代码行标记")]
    public string newAddMarker = "// NEW_ADD";

    [Tooltip("待删除代码行标记")]
    public string removeMarker = "// REMOVE_MARK";

    [Header("高亮颜色配置")]
    [Tooltip("新增代码行颜色")]
    public Color newAddColor = new Color(0.2f, 0.8f, 0.2f);

    [Tooltip("待删除代码行颜色")]
    public Color removeColor = new Color(0.8f, 0.8f, 0.2f);

    [Tooltip("正常代码行颜色")]
    public Color normalColor = Color.white;

    // 锚点常量（固定值，无需配置）
    public const string FIELD_ADD_ANCHOR = "// 留给新增的字段";
    public const string EVENT_BIND_ADD_ANCHOR = "// 留给新增的组件事件绑定";
    public const string EVENT_METHOD_ADD_ANCHOR = "// 留给新增的组件事件绑定函数";

    /// <summary>
    /// 获取备份完整路径
    /// </summary>
    public string BackupFullPath => $"{scriptGenerateRootPath}/{backupSubPath}";
    /// <summary>
    /// 配置文件的加载路径，Resources目录下的UIGeneratorSettings.asset
    /// </summary>
    private static readonly string SettingsPath = "Config/UI/UIGeneratorSettings";
    #endregion
    #region UI管理器加载相关配置
    [Header("UI管理器基础配置")]
    [Tooltip("UI管理器基础Canvas加载路径")]
    public string uiSystemRootPath = "UI/System/Canvas";
    [Tooltip("UI管理器基础EventSystem加载路径")]
    public string uiSystemEventSystemPath = "UI/System/EventSystem";
    [Tooltip("UI管理器基础UICamera加载路径")]
    public string uiSystemUICameraPath = "UI/System/UICamera";
    

    #endregion

    // 静态实例（双重校验：编辑器/运行时）
    private static UIGeneratorSettings _instance;

        /// <summary>
    /// 获取全局配置实例（单例）
    /// </summary>
    public static UIGeneratorSettings Instance
    {
        get
        {
            // 运行时（打包/真机）：仅从Resources加载，不创建新文件
            if (_instance == null)
            {
                _instance = Resources.Load<UIGeneratorSettings>(SettingsPath);
                
                // 运行时如果加载不到，返回默认实例（不创建文件）
                if (_instance == null)
                {
#if UNITY_EDITOR
                    // 编辑器环境：创建配置文件
                    _instance = CreateSettingsInEditor();
#else
                    // 运行时/打包：创建内存级默认实例（不写入文件）
                    _instance = CreateInstance<UIGeneratorSettings>();
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
    private static UIGeneratorSettings CreateSettingsInEditor()
    {
        Debug.LogWarning($"未找到{SettingsPath}配置文件，已创建默认配置");
        UIGeneratorSettings settings = CreateInstance<UIGeneratorSettings>();
        
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
}