using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class GeneratorBindComponentTool : UnityEditor.Editor
{
    public static List<EditorObjectData> objDataList;
    public static Dictionary<string, string> controlNameMap = new()
    {
        {"Button","Button"},
        {"Text","Text"},
        {"TMP","TextMeshProUGUI"},
        {"Slider","Slider"},
        {"InputField","InputField"},
        {"Dropdown","Dropdown"},
        {"Image","Image"},
        {"RawImage","RawImage"},
        {"Toggle","Toggle"},
        {"ScrollRect","ScrollRect"},
        {"TMP_Dropdown","TMP_Dropdown"},
        {"TMP_InputField","TMP_InputField"},
    };

    // 获取全局配置
    private static UIGeneratorSettings Settings => UIGeneratorSettings.Instance;

    [MenuItem("GameObject/生成面板脚本")]
    static void CreateFindComponentScripts()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
        {
            Debug.LogError("需要选择GameObject");
            return;
        }
        Debug.Log($"生成面板脚本{obj.name}");
        if (!obj.name.EndsWith("Panel"))
        {
            Debug.LogError($" GameObject{obj.name} 不是面板,请将其命名为{obj.name}Panel");
            return;
        }

        // 1. 收集当前UI所有组件数据
        objDataList = new List<EditorObjectData>();
        PresWindowNodeData(obj.transform);
        string datalistJson = JsonConvert.SerializeObject(objDataList);
        PlayerPrefs.SetString(UIGeneratorSettings.ObjDataListKey, datalistJson);

        // 2. 设置脚本路径（从配置读取）
        string absoluteRootPath = Path.Combine(Application.dataPath, "..", Settings.scriptGenerateRootPath).Replace("\\", "/");
        if (!Directory.Exists(absoluteRootPath))
        {
            Directory.CreateDirectory(absoluteRootPath);
        }
        string csPath = Path.Combine(absoluteRootPath, $"{obj.name}.cs");

        // 3. 增量更新前先备份（首次生成无需备份）
        if (File.Exists(csPath))
        {
            BackupScript(csPath);
        }

        // 4. 核心：区分首次生成/增量更新（含新增+移除）
        string finalContent = string.Empty;
        bool isIncremental = File.Exists(csPath);
        if (!isIncremental)
        {
            // 首次生成：创建完整模板（无标记）
            finalContent = CreateFirstCS(obj.name);
        }
        else
        {
            // 增量更新：添加新增/移除标记
            finalContent = UpdateContentWithAddAndRemove(csPath, obj.name, true);
        }

        // 5. 打开预览窗口
        ScriptPreviewWindow.ShowWindow(finalContent, csPath, obj.name, isIncremental);
    }

    /// <summary>
    /// 备份脚本到指定目录（使用配置的扩展名）
    /// </summary>
    private static void BackupScript(string scriptPath)
    {
        try
        {
            // 1. 创建备份目录（从配置读取路径）
            string absoluteBackupPath = Path.Combine(Application.dataPath, "..", Settings.BackupFullPath).Replace("\\", "/");
            if (!Directory.Exists(absoluteBackupPath))
            {
                Directory.CreateDirectory(absoluteBackupPath);
            }

            // 2. 生成备份文件名（从配置读取后缀和扩展名）
            string fileName = Path.GetFileNameWithoutExtension(scriptPath);
            string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string backupFileName = $"{fileName}_{timeStamp}{Settings.backupFileSuffix}{Settings.backupFileExtension}";
            string backupPath = Path.Combine(absoluteBackupPath, backupFileName);

            // 3. 复制文件到备份目录
            File.Copy(scriptPath, backupPath, true);
            Debug.Log($"脚本备份成功：{backupPath}");

            // 4. 清理超出数量的旧备份（从配置读取最大数量）
            CleanOldBackups(absoluteBackupPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"脚本备份失败：{e.Message}");
        }
    }

    /// <summary>
    /// 清理超出最大数量的旧备份（适配配置的扩展名）
    /// </summary>
    private static void CleanOldBackups(string backupPath)
    {
        try
        {
            // 获取所有自定义扩展名的备份文件，按创建时间排序
            DirectoryInfo dirInfo = new DirectoryInfo(backupPath);
            FileInfo[] backupFiles = dirInfo.GetFiles($"*{Settings.backupFileSuffix}{Settings.backupFileExtension}")
                .OrderBy(f => f.CreationTime)
                .ToArray();

            // 超出最大数量则删除最旧的（从配置读取最大数量）
            if (backupFiles.Length > Settings.maxBackupCount)
            {
                int deleteCount = backupFiles.Length - Settings.maxBackupCount;
                for (int i = 0; i < deleteCount; i++)
                {
                    backupFiles[i].Delete();
                    Debug.Log($"清理旧备份：{backupFiles[i].FullName}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"清理旧备份失败：{e.Message}");
        }
    }

    /// <summary>
    /// 读取已有脚本，提取历史生成的字段列表
    /// </summary>
    private static List<string> GetHistoryFieldNames(string csPath)
    {
        List<string> historyFields = new List<string>();
        string content = File.ReadAllText(csPath, Encoding.UTF8);

        // 正则匹配所有自动生成的字段（public 类型 字段名;）
        Regex fieldRegex = new Regex(@"public\s+(\w+)\s+(\w+);");
        MatchCollection matches = fieldRegex.Matches(content);
        foreach (Match match in matches)
        {
            if (match.Success)
            {
                string fieldName = match.Groups[2].Value;
                historyFields.Add(fieldName);
            }
        }
        return historyFields;
    }

    /// <summary>
    /// 筛选新增/待删除字段
    /// </summary>
    private static void GetAddAndRemoveFields(string csPath, out List<EditorObjectData> addFields, out List<string> removeFields)
    {
        List<string> historyFields = GetHistoryFieldNames(csPath);
        List<string> currentFields = objDataList.Select(d => d.fieldName).ToList();

        // 新增字段：当前有，历史无
        addFields = objDataList.Where(d => !historyFields.Contains(d.fieldName)).ToList();
        // 待删除字段：历史有，当前无
        removeFields = historyFields.Where(f => !currentFields.Contains(f)).ToList();
    }

    /// <summary>
    /// 增量更新：处理新增+移除内容（带标记）
    /// </summary>
    private static string UpdateContentWithAddAndRemove(string csPath, string csName, bool withMarker)
    {
        // 1. 读取原有脚本内容，按行拆分（保留换行符）
        string[] oldLines = File.ReadAllLines(csPath, Encoding.UTF8);
        List<string> newLines = new List<string>(oldLines);

        // 2. 筛选新增/待删除字段
        GetAddAndRemoveFields(csPath, out List<EditorObjectData> addFields, out List<string> removeFields);

        // 3. 处理移除内容（标记待删除行，包括方法块）
        if (removeFields.Count > 0 && withMarker)
        {
            newLines = MarkRemoveContent(newLines, removeFields);
        }

        // 4. 处理新增内容（追加到锚点后）
        if (addFields.Count > 0)
        {
            string nameSpacePrefix = string.IsNullOrEmpty(Settings.defaultNamespace) ? "" : "\t";
            string newFieldContent = CreateNewFieldContent(addFields, nameSpacePrefix, withMarker);
            string newEventBindContent = CreateNewEventBindContent(addFields, nameSpacePrefix, withMarker);
            string newEventMethodContent = CreateNewEventMethodContent(addFields, nameSpacePrefix, withMarker);

            // 插入新增字段
            int fieldAnchorIndex = FindLineIndex(newLines, UIGeneratorSettings.FIELD_ADD_ANCHOR);
            if (fieldAnchorIndex != -1 && fieldAnchorIndex + 1 < newLines.Count)
            {
                newLines.Insert(fieldAnchorIndex + 1, newFieldContent);
            }

            // 插入新增事件绑定
            int eventBindAnchorIndex = FindLineIndex(newLines, UIGeneratorSettings.EVENT_BIND_ADD_ANCHOR);
            if (eventBindAnchorIndex != -1 && eventBindAnchorIndex + 1 < newLines.Count)
            {
                newLines.Insert(eventBindAnchorIndex + 1, newEventBindContent);
            }

            // 插入新增事件方法
            int eventMethodAnchorIndex = FindLineIndex(newLines, UIGeneratorSettings.EVENT_METHOD_ADD_ANCHOR);
            if (eventMethodAnchorIndex != -1 && eventMethodAnchorIndex + 1 < newLines.Count)
            {
                newLines.Insert(eventMethodAnchorIndex + 1, newEventMethodContent);
            }
        }

        // 合并行并返回
        return string.Join(Environment.NewLine, newLines);
    }

    /// <summary>
    /// 查找包含指定锚点的行索引
    /// </summary>
    private static int FindLineIndex(List<string> lines, string anchor)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains(anchor))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 标记待删除的内容（字段/Start内事件绑定/完整方法块）
    /// </summary>
    private static List<string> MarkRemoveContent(List<string> lines, List<string> removeFields)
    {
        List<string> markedLines = new List<string>(lines);
        string marker = $" {Settings.removeMarker}";

        // ========== 第一步：标记Start方法内的事件绑定行 ==========
        // 先定位Start方法的范围
        (int startMethodStart, int startMethodEnd) = FindMethodRange(markedLines, "Start");
        if (startMethodStart != -1 && startMethodEnd != -1)
        {
            // 遍历Start方法内的所有行，标记待删除的绑定行
            for (int i = startMethodStart; i <= startMethodEnd; i++)
            {
                string line = markedLines[i];
                foreach (string fieldName in removeFields)
                {
                    // 匹配所有类型的事件绑定
                    if (line.Contains($"{fieldName}.onClick.AddListener") ||
                        line.Contains($"{fieldName}.onValueChanged.AddListener") ||
                        line.Contains($"{fieldName}.onEndEdit.AddListener"))
                    {
                        markedLines[i] += marker;
                        break;
                    }
                }
            }
        }

        // ========== 第二步：标记字段行 ==========
        foreach (string fieldName in removeFields)
        {
            for (int i = 0; i < markedLines.Count; i++)
            {
                if (Regex.IsMatch(markedLines[i], $"public\\s+\\w+\\s+{fieldName};") &&
                    !markedLines[i].Contains(Settings.removeMarker))
                {
                    markedLines[i] += marker;
                    break;
                }
            }

            // ========== 第三步：标记完整事件方法块（含方法定义+函数体） ==========
            // 匹配所有相关方法名
            List<string> methodNames = new List<string>()
            {
                $"On{fieldName}ButtonClick",
                $"On{fieldName}InputChange",
                $"On{fieldName}InputEnd",
                $"On{fieldName}ToggleChange"
            };

            foreach (string methodName in methodNames)
            {
                MarkMethodBlock(markedLines, methodName, marker);
            }
        }

        return markedLines;
    }

    /// <summary>
    /// 查找指定方法的起止行（方法定义行 ~ 方法闭合}行）
    /// </summary>
    private static (int startIndex, int endIndex) FindMethodRange(List<string> lines, string methodName)
    {
        int methodStart = -1;
        int methodEnd = -1;
        int braceCount = 0;
        bool isInMethod = false;

        // 1. 找到方法定义行
        for (int i = 0; i < lines.Count; i++)
        {
            if (Regex.IsMatch(lines[i], $"protected override void {methodName}\\(\\)") ||
                Regex.IsMatch(lines[i], $"public override void {methodName}\\(\\)") ||
                Regex.IsMatch(lines[i], $"private void {methodName}\\(\\)"))
            {
                methodStart = i;
                isInMethod = true;
                break;
            }
        }

        if (methodStart == -1) return (-1, -1);

        // 2. 找到方法闭合}行
        for (int i = methodStart; i < lines.Count; i++)
        {
            string line = lines[i];
            foreach (char c in line)
            {
                if (c == '{')
                {
                    braceCount++;
                    isInMethod = true;
                }
                else if (c == '}')
                {
                    braceCount--;
                    if (braceCount == 0 && isInMethod)
                    {
                        methodEnd = i;
                        isInMethod = false;
                        break;
                    }
                }
            }
            if (!isInMethod && methodEnd != -1) break;
        }

        return (methodStart, methodEnd);
    }

    /// <summary>
    /// 标记完整的方法块（方法定义 + { } 内内容 + 后续空行）
    /// </summary>
    private static void MarkMethodBlock(List<string> lines, string methodName, string marker)
    {
        int methodStartIndex = -1;
        int braceCount = 0;
        bool isInMethodBlock = false;

        // 第一步：找到方法定义行
        for (int i = 0; i < lines.Count; i++)
        {
            if (Regex.IsMatch(lines[i], $"private void {methodName}\\(.*\\)") &&
                !lines[i].Contains(Settings.removeMarker))
            {
                methodStartIndex = i;
                isInMethodBlock = true;
                lines[i] += marker; // 标记方法定义行
                break;
            }
        }

        // 未找到方法，直接返回
        if (methodStartIndex == -1) return;

        // 第二步：遍历标记方法体（{ } 内所有内容）
        for (int i = methodStartIndex; i < lines.Count; i++)
        {
            string line = lines[i];

            // 统计大括号，确定方法块范围
            foreach (char c in line)
            {
                if (c == '{')
                {
                    braceCount++;
                    isInMethodBlock = true;
                }
                else if (c == '}')
                {
                    braceCount--;
                    // 遇到闭合大括号，标记当前行后退出方法块
                    if (braceCount == 0)
                    {
                        if (!lines[i].Contains(Settings.removeMarker))
                        {
                            lines[i] += marker;
                        }
                        isInMethodBlock = false;
                        // 标记方法块后的空行（优化格式）
                        if (i + 1 < lines.Count && string.IsNullOrWhiteSpace(lines[i + 1]) &&
                            !lines[i + 1].Contains(Settings.removeMarker))
                        {
                            lines[i + 1] += marker;
                        }
                        break;
                    }
                }
            }

            // 标记方法块内的行（除了方法定义行已标记）
            if (isInMethodBlock && i > methodStartIndex && !lines[i].Contains(Settings.removeMarker))
            {
                lines[i] += marker;
            }

            // 方法块结束，退出循环
            if (!isInMethodBlock && braceCount == 0)
            {
                break;
            }
        }
    }

    /// <summary>
    /// 清理移除标记和对应行（最终生成时调用）
    /// </summary>
    private static string CleanRemoveContent(string content)
    {
        List<string> cleanLines = new List<string>();
        string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (string line in lines)
        {
            // 跳过带移除标记的行
            if (!line.Contains(Settings.removeMarker))
            {
                // 移除新增标记，保留纯净代码
                string cleanLine = line.Replace(Settings.newAddMarker, "").TrimEnd();
                cleanLines.Add(cleanLine);
            }
        }

        // 合并行，移除多余空行（优化格式）
        return string.Join(Environment.NewLine, cleanLines).Trim();
    }

    /// <summary>
    /// 首次生成完整脚本模板（从配置读取命名空间）
    /// </summary>
    private static string CreateFirstCS(string csName)
    {
        StringBuilder sb = new();
        string nameSpaceStr = Settings.defaultNamespace;
        sb.AppendLine("/*----------------------------------");
        sb.AppendLine(" *Title:UI自动化组件生成代码生成工具");
        sb.AppendLine(" *Date:" + DateTime.Now);
        sb.AppendLine(" *Description:变量需要以[Text]括号加组件类型的格式进行声明,然后右键窗口物体——一键生成UI数据组件脚本即可");
        sb.AppendLine(" *注意:自动生成的内容仅追加/移除，不会覆盖手写逻辑；新增/删除组件后再次生成，会标记对应代码行");
        sb.AppendLine("----------------------------------*/");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using TMPro;");
        sb.AppendLine();

        string nameSpacePrefix = "";
        if (!string.IsNullOrEmpty(nameSpaceStr))
        {
            nameSpacePrefix = "\t";
            sb.AppendLine($"namespace {nameSpaceStr}");
            sb.AppendLine("{");
            sb.AppendLine();
        }

        sb.AppendLine($"{nameSpacePrefix}public class {csName}:BasePanel");
        sb.AppendLine($"{nameSpacePrefix}{{");

        // 字段声明区
        sb.AppendLine($"{nameSpacePrefix}\t//字段声明");
        foreach (var item in objDataList)
        {
            sb.AppendLine($"{nameSpacePrefix}\tpublic {item.fieldType} {item.fieldName};");
        }
        sb.AppendLine($"{nameSpacePrefix}\t{UIGeneratorSettings.FIELD_ADD_ANCHOR}");
        sb.AppendLine($"{nameSpacePrefix}\t//自己手写的字段声明，不会被覆盖");

        sb.AppendLine();

        // Awake方法
        sb.AppendLine($"{nameSpacePrefix}\t//重写Awake方法");
        sb.AppendLine($"{nameSpacePrefix}\tprotected override void Awake()");
        sb.AppendLine($"{nameSpacePrefix}\t{{");
        sb.AppendLine($"{nameSpacePrefix}\t\tbase.Awake();");
        sb.AppendLine($"{nameSpacePrefix}\t\t//自己手写的Awake逻辑，不会被覆盖");
        sb.AppendLine($"{nameSpacePrefix}\t}}");

        sb.AppendLine();

        // Start方法（事件绑定）
        sb.AppendLine($"{nameSpacePrefix}\t//重写Start方法,完成事件绑定");
        sb.AppendLine($"{nameSpacePrefix}\tprotected override void Start()");
        sb.AppendLine($"{nameSpacePrefix}\t{{");
        sb.AppendLine($"{nameSpacePrefix}\t\tbase.Start();");
        sb.AppendLine($"{nameSpacePrefix}\t\t//组件事件绑定");
        foreach (var item in objDataList)
        {
            string type = item.fieldType;
            string fieldName = item.fieldName;
            if (type.Contains("Button"))
            {
                sb.AppendLine($"{nameSpacePrefix}\t\t{fieldName}.onClick.AddListener(()=>On{fieldName}ButtonClick());");
            }
            if (type.Contains("InputField"))
            {
                sb.AppendLine($"{nameSpacePrefix}\t\t{fieldName}.onValueChanged.AddListener((value)=>On{fieldName}InputChange(value));");
                sb.AppendLine($"{nameSpacePrefix}\t\t{fieldName}.onEndEdit.AddListener((value)=>On{fieldName}InputEnd(value));");
            }
            if (type.Contains("Toggle"))
            {
                sb.AppendLine($"{nameSpacePrefix}\t\t{fieldName}.onValueChanged.AddListener((value)=>On{fieldName}ToggleChange(value));");
            }
        }
        sb.AppendLine($"{nameSpacePrefix}\t\t{UIGeneratorSettings.EVENT_BIND_ADD_ANCHOR}");
        sb.AppendLine($"{nameSpacePrefix}\t\t//自己手写的Start逻辑，不会被覆盖");
        sb.AppendLine($"{nameSpacePrefix}\t}}");

        sb.AppendLine();

        // 显示/隐藏方法
        sb.AppendLine($"{nameSpacePrefix}\tpublic override void ShowMe()");
        sb.AppendLine($"{nameSpacePrefix}\t{{");
        sb.AppendLine($"{nameSpacePrefix}\t\t//自己手写的ShowMe逻辑，不会被覆盖");
        sb.AppendLine($"{nameSpacePrefix}\t}}");
        sb.AppendLine();
        sb.AppendLine($"{nameSpacePrefix}\tpublic override void HideMe()");
        sb.AppendLine($"{nameSpacePrefix}\t{{");
        sb.AppendLine($"{nameSpacePrefix}\t\t//自己手写的HideMe逻辑，不会被覆盖");
        sb.AppendLine($"{nameSpacePrefix}\t}}");

        sb.AppendLine();

        // 事件方法区
        foreach (var item in objDataList)
        {
            string type = item.fieldType;
            string fieldName = item.fieldName;
            if (type.Contains("Button"))
            {
                sb.AppendLine($"{nameSpacePrefix}\tprivate void On{fieldName}ButtonClick()");
                sb.AppendLine($"{nameSpacePrefix}\t{{");
                sb.AppendLine($"{nameSpacePrefix}\t\t");
                sb.AppendLine($"{nameSpacePrefix}\t}}");
                sb.AppendLine();
            }
            if (type.Contains("InputField"))
            {
                sb.AppendLine($"{nameSpacePrefix}\tprivate void On{fieldName}InputChange(string value)");
                sb.AppendLine($"{nameSpacePrefix}\t{{");
                sb.AppendLine($"{nameSpacePrefix}\t\t");
                sb.AppendLine($"{nameSpacePrefix}\t}}");
                sb.AppendLine();
                sb.AppendLine($"{nameSpacePrefix}\tprivate void On{fieldName}InputEnd(string value)");
                sb.AppendLine($"{nameSpacePrefix}\t{{");
                sb.AppendLine($"{nameSpacePrefix}\t\t");
                sb.AppendLine($"{nameSpacePrefix}\t}}");
                sb.AppendLine();
            }
            if (type.Contains("Toggle"))
            {
                sb.AppendLine($"{nameSpacePrefix}\tprivate void On{fieldName}ToggleChange(bool value)");
                sb.AppendLine($"{nameSpacePrefix}\t{{");
                sb.AppendLine($"{nameSpacePrefix}\t\t");
                sb.AppendLine($"{nameSpacePrefix}\t}}");
                sb.AppendLine();
            }
        }
        sb.AppendLine($"{nameSpacePrefix}\t{UIGeneratorSettings.EVENT_METHOD_ADD_ANCHOR}");
        sb.AppendLine($"{nameSpacePrefix}\t//自己写的内容");

        sb.AppendLine($"{nameSpacePrefix}}}");
        if (!string.IsNullOrEmpty(nameSpaceStr))
        {
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 生成新增字段代码（从配置读取标记）
    /// </summary>
    private static string CreateNewFieldContent(List<EditorObjectData> addFields, string nameSpacePrefix, bool withMarker)
    {
        StringBuilder sb = new StringBuilder();
        string marker = withMarker ? $" {Settings.newAddMarker}" : "";
        foreach (var item in addFields)
        {
            sb.AppendLine($"{nameSpacePrefix}\tpublic {item.fieldType} {item.fieldName};{marker}");
        }
        return sb.ToString().Trim();
    }

    /// <summary>
    /// 生成新增事件绑定代码（从配置读取标记）
    /// </summary>
    private static string CreateNewEventBindContent(List<EditorObjectData> addFields, string nameSpacePrefix, bool withMarker)
    {
        StringBuilder sb = new StringBuilder();
        string marker = withMarker ? $" {Settings.newAddMarker}" : "";
        foreach (var item in addFields)
        {
            string type = item.fieldType;
            string fieldName = item.fieldName;
            if (type.Contains("Button"))
            {
                sb.AppendLine($"{nameSpacePrefix}\t\t{fieldName}.onClick.AddListener(()=>On{fieldName}ButtonClick());{marker}");
            }
            if (type.Contains("InputField"))
            {
                sb.AppendLine($"{nameSpacePrefix}\t\t{fieldName}.onValueChanged.AddListener((value)=>On{fieldName}InputChange(value));{marker}");
                sb.AppendLine($"{nameSpacePrefix}\t\t{fieldName}.onEndEdit.AddListener((value)=>On{fieldName}InputEnd(value));{marker}");
            }
            if (type.Contains("Toggle"))
            {
                sb.AppendLine($"{nameSpacePrefix}\t\t{fieldName}.onValueChanged.AddListener((value)=>On{fieldName}ToggleChange(value));{marker}");
            }
        }
        return sb.ToString().Trim();
    }

    /// <summary>
    /// 生成新增事件方法代码（从配置读取标记）
    /// </summary>
    private static string CreateNewEventMethodContent(List<EditorObjectData> addFields, string nameSpacePrefix, bool withMarker)
    {
        StringBuilder sb = new StringBuilder();
        string marker = withMarker ? $" {Settings.newAddMarker}" : "";
        foreach (var item in addFields)
        {
            string type = item.fieldType;
            string fieldName = item.fieldName;
            if (type.Contains("Button"))
            {
                sb.AppendLine($"{nameSpacePrefix}\tprivate void On{fieldName}ButtonClick(){marker}");
                sb.AppendLine($"{nameSpacePrefix}\t{{");
                sb.AppendLine($"{nameSpacePrefix}\t\t");
                sb.AppendLine($"{nameSpacePrefix}\t}}");
                sb.AppendLine();
            }
            if (type.Contains("InputField"))
            {
                sb.AppendLine($"{nameSpacePrefix}\tprivate void On{fieldName}InputChange(string value){marker}");
                sb.AppendLine($"{nameSpacePrefix}\t{{");
                sb.AppendLine($"{nameSpacePrefix}\t\t");
                sb.AppendLine($"{nameSpacePrefix}\t}}");
                sb.AppendLine();
                sb.AppendLine($"{nameSpacePrefix}\tprivate void On{fieldName}InputEnd(string value){marker}");
                sb.AppendLine($"{nameSpacePrefix}\t{{");
                sb.AppendLine($"{nameSpacePrefix}\t\t");
                sb.AppendLine($"{nameSpacePrefix}\t}}");
                sb.AppendLine();
            }
            if (type.Contains("Toggle"))
            {
                sb.AppendLine($"{nameSpacePrefix}\tprivate void On{fieldName}ToggleChange(bool value){marker}");
                sb.AppendLine($"{nameSpacePrefix}\t{{");
                sb.AppendLine($"{nameSpacePrefix}\t\t");
                sb.AppendLine($"{nameSpacePrefix}\t}}");
                sb.AppendLine();
            }
        }
        return sb.ToString().Trim();
    }

    /// <summary>
    /// 递归收集UI组件数据
    /// </summary>
    public static void PresWindowNodeData(Transform trans)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            GameObject obj = trans.GetChild(i).gameObject;
            string name = obj.name;
            if (name.Contains("[") && name.Contains("]"))
            {
                int index = name.IndexOf("]") + 1;
                string fieldName = name.Substring(index, name.Length - index);
                string fieldTypeKey = name.Substring(1, index - 2);
                if (!controlNameMap.TryGetValue(fieldTypeKey, out string fieldType))
                {
                    Debug.LogError($" GameObject{obj.name} 组件类型{fieldTypeKey} 未定义");
                    continue;
                }
                objDataList.Add(new EditorObjectData { fieldName = fieldName, fieldType = fieldType, insID = obj.GetInstanceID() });
            }
            PresWindowNodeData(trans.GetChild(i));
        }
    }

    /// <summary>
    /// 编译完成后自动挂载组件并赋值
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    public static void AddComponent2Window()
    {
        string className = EditorPrefs.GetString("GeneratorClassName");
        if (string.IsNullOrEmpty(className)) return;

        // 反射获取生成的类
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var cSharpAssembly = assemblies.First(assembly => assembly.GetName().Name == "Assembly-CSharp");
        string relClassName = $"{Settings.defaultNamespace}.{className}";
        Type type = cSharpAssembly.GetType(relClassName);
        if (type == null) return;

        // 找到目标物体并挂载组件
        GameObject windowObj = GameObject.Find(className);
        if (windowObj == null)
        {
            Debug.LogError($"未找到物体：{className}");
            return;
        }
        Component compt = windowObj.GetComponent(type);
        if (compt == null)
        {
            compt = windowObj.AddComponent(type);
        }

        // 赋值组件字段
        string datalistJson = PlayerPrefs.GetString(UIGeneratorSettings.ObjDataListKey);
        List<EditorObjectData> objDataList = JsonConvert.DeserializeObject<List<EditorObjectData>>(datalistJson);
        FieldInfo[] fieldInfoList = type.GetFields();

        foreach (var field in fieldInfoList)
        {
            var objData = objDataList.FirstOrDefault(d => d.fieldName == field.Name);
            if (objData == null) continue;

            GameObject uiObject = EditorUtility.InstanceIDToObject(objData.insID) as GameObject;
            if (uiObject == null) continue;

            if (field.FieldType == typeof(GameObject))
            {
                field.SetValue(compt, uiObject);
            }
            else
            {
                Component comp = uiObject.GetComponent(field.FieldType);
                if (comp != null)
                {
                    field.SetValue(compt, comp);
                }
            }
        }

        EditorPrefs.DeleteKey("GeneratorClassName");
    }

    /// <summary>
    /// 执行最终的脚本生成（由预览窗口调用）
    /// </summary>
    public static void DoGenerateScript(string previewContent, string path, string className)
    {
        // 清理移除标记和对应行，移除新增标记
        string cleanContent = CleanRemoveContent(previewContent);
        // 写入文件
        File.WriteAllText(path, cleanContent, Encoding.UTF8);
        // 刷新Unity资源
        AssetDatabase.Refresh();
        // 记录类名
        EditorPrefs.SetString("GeneratorClassName", className);

        // 提示备份信息（从配置读取参数）
        string backupTip = $"脚本生成成功！\n备份文件路径：{Settings.BackupFullPath}\n备份文件扩展名：{Settings.backupFileExtension}\n最多保留{Settings.maxBackupCount}个备份文件";
        Debug.Log(backupTip);
        EditorUtility.DisplayDialog("生成完成", backupTip, "确定");
    }
}