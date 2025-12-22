using UnityEngine;
using UnityEditor;

/// <summary>
/// UI生成工具全局配置（可在Project Settings中编辑）
/// </summary>
[CreateAssetMenu(fileName = "UIGeneratorSettings", menuName = "UI生成工具/创建配置文件")]
public class UIGeneratorSettings : ScriptableObject
{
    [Header("基础配置")]
    [Tooltip("生成脚本的根路径（相对于项目根目录）")]
    public string scriptGenerateRootPath = "Assets/Scripts/UI/AutoGenerate";

    [Tooltip("默认命名空间")]
    public string defaultNamespace = "MyFrameWork";

    [Header("备份配置")]
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
    public const string ObjDataListKey = "UI_ObjDataList_Key";

    /// <summary>
    /// 获取备份完整路径
    /// </summary>
    public string BackupFullPath => $"{scriptGenerateRootPath}/{backupSubPath}";

    /// <summary>
    /// 获取全局配置实例（单例）
    /// </summary>
    public static UIGeneratorSettings Instance
    {
        get
        {
            // 查找已有配置文件
            UIGeneratorSettings settings = AssetDatabase.LoadAssetAtPath<UIGeneratorSettings>("ProjectSettings/UIGeneratorSettings.asset");
            if (settings == null)
            {
                // 不存在则创建并保存到ProjectSettings目录
                settings = CreateInstance<UIGeneratorSettings>();
                if (!AssetDatabase.IsValidFolder("ProjectSettings/UIGenerator"))
                {
                    AssetDatabase.CreateFolder("ProjectSettings", "UIGenerator");
                }
                string path = "ProjectSettings/UIGenerator/UIGeneratorSettings.asset";
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return settings;
        }
    }
}