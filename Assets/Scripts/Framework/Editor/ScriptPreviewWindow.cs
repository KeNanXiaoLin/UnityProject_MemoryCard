using System;
using UnityEditor;
using UnityEngine;

public class ScriptPreviewWindow : EditorWindow
{
    private string _scriptContent;
    private string _scriptPath;
    private string _className;
    private bool _isIncremental;
    private Vector2 _scrollPos;

    /// <summary>
    /// 打开预览窗口
    /// </summary>
    public static void ShowWindow(string scriptContent, string scriptPath, string className, bool isIncremental)
    {
        ScriptPreviewWindow window = GetWindow<ScriptPreviewWindow>("脚本预览");
        window.minSize = new Vector2(800, 600);
        window._scriptContent = scriptContent;
        window._scriptPath = scriptPath;
        window._className = className;
        window._isIncremental = isIncremental;
        window.Show();
    }

    private void OnGUI()
    {
        // 标题
        GUILayout.Label("生成脚本预览", EditorStyles.boldLabel);

        // 增量提示
        if (_isIncremental)
        {
            GUILayout.Label("💡 浅绿色行=本次新增 | 浅红色行=本次移除", EditorStyles.miniBoldLabel);
            GUILayout.Label($"⚠️ 本次操作已自动备份原有脚本到：{GeneratorConfig.BackupPath}（扩展名：{GeneratorConfig.BackupFileExtension}）", EditorStyles.miniLabel);
        }

        GUILayout.Space(10);

        // 路径提示
        GUILayout.Label($"生成路径：{_scriptPath}", EditorStyles.miniLabel);
        GUILayout.Space(5);

        // 内容预览区域（逐行绘制，支持高亮）
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

        // 拆分内容为行
        string[] lines = _scriptContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            // 判断行类型
            bool isNewAdd = line.Contains(GeneratorConfig.NEW_ADD_MARKER);
            bool isRemove = line.Contains(GeneratorConfig.REMOVE_MARKER);

            // 设置颜色
            if (isRemove)
            {
                GUI.contentColor = GeneratorConfig.RemoveColor;
            }
            else if (isNewAdd)
            {
                GUI.contentColor = GeneratorConfig.NewAddColor;
            }
            else
            {
                GUI.contentColor = GeneratorConfig.NormalColor;
            }

            // 绘制行内容
            GUILayout.Label(line, EditorStyles.textArea);

            // 重置颜色
            GUI.contentColor = GeneratorConfig.NormalColor;
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        // 按钮区域
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // 取消按钮
        if (GUILayout.Button("取消", GUILayout.Width(100), GUILayout.Height(30)))
        {
            Close();
        }

        // 确认生成按钮
        if (GUILayout.Button("确认生成", GUILayout.Width(100), GUILayout.Height(30)))
        {
            // 调用生成逻辑（自动清理标记）
            GeneratorBindComponentTool.DoGenerateScript(_scriptContent, _scriptPath, _className);
            Close();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}