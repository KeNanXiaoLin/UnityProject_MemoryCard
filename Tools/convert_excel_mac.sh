#!/bin/zsh

# ===================== 无需修改任何路径！=====================
# 前提：脚本必须放在项目的 Tools 目录下（和 main 同目录）
# 相对路径基准：强制切换到项目根目录（When-Sleeping/）
# =============================================================

# 1. 先获取脚本自身的绝对路径（不管怎么运行，都能定位到脚本位置）
SCRIPT_PATH=$(cd "$(dirname "${0}")" && pwd)
# 2. 切换到项目根目录（Tools 的上一级目录）
PROJECT_ROOT=$(dirname "$SCRIPT_PATH")
cd "$PROJECT_ROOT" || {
    echo "❌ 无法切换到项目根目录：$PROJECT_ROOT"
    echo -n "按回车键退出..."
    read
    exit 1
}

# 3. 定义相对路径（基于项目根目录，永远正确）
TOOL_PATH="./Tools/main"       # 打包程序在 Tools 目录下
INPUT_DIR="./Excel"            # 输入目录在项目根目录
OUTPUT_DIR="./Assets/StreamingAssets"  # 输出目录在 Assets 下

# ------------------------ 以下无需修改 ------------------------
# 检查打包程序是否存在
if [ ! -f "$TOOL_PATH" ]; then
    echo "❌ 错误：打包程序不存在！"
    echo "  预期路径：$PROJECT_ROOT/$TOOL_PATH"
    echo -n "按回车键退出..."
    read
    exit 1
fi

# 检查输入目录是否存在
if [ ! -d "$INPUT_DIR" ]; then
    echo "❌ 错误：输入目录不存在！"
    echo "  预期路径：$PROJECT_ROOT/$INPUT_DIR"
    echo -n "按回车键退出..."
    read
    exit 1
fi

# 确保输出目录存在（自动创建）
if [ ! -d "$OUTPUT_DIR" ]; then
    echo "⚠️  输出目录不存在，正在创建..."
    mkdir -p "$OUTPUT_DIR" || {
        echo "❌ 无法创建输出目录：$PROJECT_ROOT/$OUTPUT_DIR"
        echo -n "按回车键退出..."
        read
        exit 1
    }
fi

# 执行转换命令
echo "======================================"
echo "项目根目录：$PROJECT_ROOT"
echo "程序路径：$TOOL_PATH"
echo "输入目录：$INPUT_DIR"
echo "输出目录：$OUTPUT_DIR"
echo "======================================"

# 执行打包程序（相对路径基于项目根目录，100%正确）
"$TOOL_PATH" -i "$INPUT_DIR" -o "$OUTPUT_DIR"

# 显示结果
if [ $? -eq 0 ]; then
    echo "======================================"
    echo "✅ 转换成功！"
else
    echo "======================================"
    echo "❌ 转换失败！"
fi

# 暂停窗口
echo -n "按回车键退出..."
read