using UnityEditor;
using UnityEngine;

/// <summary>
/// 注册UI生成工具配置到Project Settings
/// </summary>
public class UIGeneratorSettingsProvider : SettingsProvider
{
    private SerializedObject settingsObj;
    private UIGeneratorSettings settings;

    /// <summary>
    /// 构造函数
    /// </summary>
    public UIGeneratorSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
        : base(path, scope)
    {
        settings = UIGeneratorSettings.Instance;
        settingsObj = new SerializedObject(settings);
    }

    /// <summary>
    /// 绘制配置面板
    /// </summary>
    public override void OnGUI(string searchContext)
    {
        EditorGUI.BeginChangeCheck();

        // 绘制所有序列化属性
        SerializedProperty iterator = settingsObj.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            // 跳过m_Script（内置属性）
            if (iterator.propertyPath == "m_Script") continue;

            EditorGUILayout.PropertyField(iterator, true);
        }

        // 保存修改
        if (EditorGUI.EndChangeCheck())
        {
            settingsObj.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        // 额外提示
        EditorGUILayout.Space(20);
        EditorGUILayout.HelpBox(
            "注意：\n" +
            "1. 脚本路径为相对路径，基于项目根目录\n" +
            "2. 备份路径为相对于脚本生成路径的子目录\n" +
            "3. 修改配置后需重新生成脚本生效",
            MessageType.Info);
    }

    /// <summary>
    /// 注册配置面板到Project Settings
    /// </summary>
    [SettingsProvider]
    public static SettingsProvider CreateUIGeneratorSettingsProvider()
    {
        var provider = new UIGeneratorSettingsProvider("Project/UI生成工具", SettingsScope.Project)
        {
            // 设置面板图标（可选）
            label = "UI生成工具",
            keywords = new[] { "UI", "生成", "脚本", "配置", "备份" }
        };
        return provider;
    }
}