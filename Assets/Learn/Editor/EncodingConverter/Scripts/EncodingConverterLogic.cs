using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.EncodingConverter.Scripts
{
    public class EncodingConverterLogic
    {
        // UI 控件引用
        private Label _fileCountLabel;
        private VisualElement _fileListContainer;
        private Button _convertBtn;
        private Toggle _backupToggle;
        private Label _statusLabel;

        // 选中的文本文件路径列表（过滤非文本文件）
        private List<string> _selectedFilePaths = new List<string>();

        // 支持转换的文本文件后缀（可根据需求添加）
        private readonly List<string> _supportedExtensions = new List<string>
        {
            ".cs", ".js", ".ts", ".shader", ".cginc",
            ".json", ".xml", ".yml", ".yaml", ".txt",
            ".lua", ".php", ".html", ".css", ".md"
        };

        // GBK 编码（标准 Windows 中文编码，代码页 936）
        private readonly Encoding _gbkEncoding = Encoding.GetEncoding(936);
        // UTF-8 编码（不带 BOM，Unity/VSCode 默认兼容）
        private readonly Encoding _utf8Encoding = new UTF8Encoding(false);

        public EncodingConverterLogic(VisualElement root)
        {
            // 绑定 UI 控件
            _fileCountLabel = root.Q<Label>("FileCountLabel");
            _fileListContainer = root.Q<VisualElement>("FileListContainer");
            _convertBtn = root.Q<Button>("ConvertBtn");
            _backupToggle = root.Q<Toggle>("BackupToggle");
            _statusLabel = root.Q<Label>("StatusLabel");

            // 注册事件
            _convertBtn.clicked += OnConvertClicked;

            // 初始禁用转换按钮（无选中文件时）
            _convertBtn.SetEnabled(false);
        }

        /// <summary>
        /// 刷新选中文件：监听 Unity 选中变化，过滤出支持的文本文件
        /// </summary>
        public void RefreshSelectedFiles()
        {
            _selectedFilePaths.Clear();

            // 获取 Unity Project 窗口选中的所有资源
            var selectedAssets = Selection.objects;
            foreach (var asset in selectedAssets)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(assetPath)) continue;

                // 过滤：不是文件夹 + 是支持的文本后缀
                if (!AssetDatabase.IsValidFolder(assetPath) && IsSupportedExtension(assetPath))
                {
                    _selectedFilePaths.Add(assetPath);
                }
            }

            // 刷新 UI 显示
            RefreshFileListUI();
            UpdateFileCountLabel();
            UpdateConvertButtonState();
            UpdateStatus(_selectedFilePaths.Count == 0 ? "等待在 Project 窗口选中文件..." : "已就绪，点击转换按钮开始", Color.gray);
        }

        /// <summary>
        /// 检查文件后缀是否支持转换（文本类文件）
        /// </summary>
        private bool IsSupportedExtension(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return _supportedExtensions.Contains(extension);
        }

        /// <summary>
        /// 刷新文件列表 UI：显示选中的文件名称和路径（简化显示）
        /// </summary>
        private void RefreshFileListUI()
        {
            _fileListContainer.Clear();

            foreach (var path in _selectedFilePaths)
            {
                // 简化显示：只显示文件名 + 所在文件夹（相对 Assets 路径）
                var relativePath = Path.GetRelativePath(Application.dataPath, Path.GetDirectoryName(path));
                var displayText = $"{Path.GetFileName(path)} （Assets/{relativePath}）";

                var fileItem = new VisualElement();
                fileItem.AddToClassList("file-item");
                fileItem.Add(new Label(displayText));
                _fileListContainer.Add(fileItem);
            }

            // 无选中文件时显示提示
            if (_selectedFilePaths.Count == 0)
            {
                var tipItem = new VisualElement();
                var tipLabel = new Label("未选中支持的文本文件（支持 .cs/.json/.txt 等后缀）");
                tipLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                tipLabel.style.fontSize = 12;
                tipItem.Add(tipLabel);
                _fileListContainer.Add(tipItem);
            }
        }

        /// <summary>
        /// 更新选中文件数量标签
        /// </summary>
        private void UpdateFileCountLabel()
        {
            _fileCountLabel.text = $"已选中：{_selectedFilePaths.Count} 个文件";
        }

        /// <summary>
        /// 更新转换按钮状态（有选中文件才启用）
        /// </summary>
        private void UpdateConvertButtonState()
        {
            _convertBtn.SetEnabled(_selectedFilePaths.Count > 0);
        }

        /// <summary>
        /// 开始转换按钮：批量将选中的 GBK 文件转换为 UTF-8
        /// </summary>
        private void OnConvertClicked()
        {
            if (_selectedFilePaths.Count == 0)
            {
                UpdateStatus("无选中文件可转换！", Color.yellow);
                return;
            }

            _convertBtn.SetEnabled(false);
            UpdateStatus("正在转换...", Color.white);

            int successCount = 0;
            List<string> failedFiles = new List<string>();

            try
            {
                // 批量转换，提升效率
                foreach (var filePath in _selectedFilePaths)
                {
                    try
                    {
                        var fileName = Path.GetFileName(filePath);

                        // 1. 备份原文件（如果启用备份）
                        if (_backupToggle.value)
                        {
                            var backupPath = filePath + ".bak";
                            File.Copy(filePath, backupPath, overwrite: true);
                        }

                        // 2. 关键步骤：以 GBK 编码读取文件内容（解决中文乱码）
                        string content = File.ReadAllText(filePath, _gbkEncoding);

                        // 3. 以 UTF-8 编码写入文件（覆盖原文件）
                        File.WriteAllText(filePath, content, _utf8Encoding);

                        successCount++;
                        UpdateStatus($"正在转换：{fileName}（{successCount}/{_selectedFilePaths.Count}）", Color.white);
                    }
                    catch (Exception e)
                    {
                        // 记录失败文件和原因
                        failedFiles.Add($"{Path.GetFileName(filePath)}：{e.Message.Substring(0, Math.Min(e.Message.Length, 50))}...");
                    }
                }

                // 转换完成，显示结果
                string resultTitle = successCount == _selectedFilePaths.Count ? "转换成功" : "转换完成（部分失败）";
                string resultMsg = successCount == _selectedFilePaths.Count
                    ? $"全部转换成功！共 {successCount} 个文件"
                    : $"成功：{successCount} 个，失败：{failedFiles.Count} 个\n\n失败文件详情：\n{string.Join("\n", failedFiles)}";

                UpdateStatus(resultMsg, successCount == _selectedFilePaths.Count ? Color.green : Color.red);
                EditorUtility.DisplayDialog(resultTitle, resultMsg, "确定");
            }
            finally
            {
                _convertBtn.SetEnabled(true);
                AssetDatabase.Refresh(); // 刷新 Unity Project 窗口，立即显示文件编码变化
            }
        }

        /// <summary>
        /// 更新状态提示文本（带颜色）
        /// </summary>
        private void UpdateStatus(string msg, Color color)
        {
            _statusLabel.text = $"状态：{msg}";
            _statusLabel.style.color = color;
        }
    }
}