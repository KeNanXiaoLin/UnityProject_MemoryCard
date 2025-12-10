using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.EncodingConverter.Scripts
{
    [EditorWindowTitle(title = "GBK to UTF-8 编码转换工具")]
    public class EncodingConverterWindow : EditorWindow
    {
        private VisualElement _root;
        private EncodingConverterLogic _logic;

        [MenuItem("Tools/编码转换工具（GBK→UTF-8） %#e", false, 101)] // 快捷键 Ctrl+Shift+E
        public static void ShowWindow()
        {
            var window = GetWindow<EncodingConverterWindow>();
            window.titleContent = new GUIContent("GBK→UTF-8 转换工具");
            window.minSize = new Vector2(500, 400); // 最小窗口尺寸
        }

        private void CreateGUI()
        {
            // 加载 UXML（替换为你的实际路径）
            var uxmlPath = "Assets/Learn/Editor/EncodingConverter/UXML/EncodingConverterUI.uxml";
            _root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath).CloneTree();

            // 加载 USS（替换为你的实际路径）
            var ussPath = "Assets/Learn/Editor/EncodingConverter/USS/EncodingConverterStyle.uss";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            _root.styleSheets.Add(styleSheet);

            rootVisualElement.Add(_root);

            // 初始化逻辑
            _logic = new EncodingConverterLogic(_root);

            // 监听 Unity 选中资源变化，实时更新窗口
            Selection.selectionChanged += _logic.RefreshSelectedFiles;

            // 窗口关闭时移除监听（避免内存泄漏）
            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                Selection.selectionChanged -= _logic.RefreshSelectedFiles;
            };

            // 初始刷新一次选中文件
            _logic.RefreshSelectedFiles();
        }
    }
}