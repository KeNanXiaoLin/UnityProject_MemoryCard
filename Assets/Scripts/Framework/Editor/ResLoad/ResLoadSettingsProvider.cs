using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ResLoadSettingsProvider : SettingsProvider
{
    private SerializedObject settingsObj;
    private ResLoadSettings settings;
    public ResLoadSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
    {
        settings = ResLoadSettings.Instance;
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
    public static SettingsProvider CreateResLoadSettingsProvider()
    {
        var provider = new ResLoadSettingsProvider("Project/MyFramework/资源加载工具", SettingsScope.Project)
        {
            // 设置面板图标（可选）
            label = "资源加载工具",
            keywords = new[] { "资源", "加载", "工具", "配置" }
        };
        return provider;
    }
}
