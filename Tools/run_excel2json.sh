#!/bin/bash

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# 检查 excel2json 可执行文件是否存在
if [ ! -f "./excel2json" ]; then
    echo "错误: 找不到 excel2json 可执行文件"
    echo "请确保 excel2json 文件存在于当前目录"
    exit 1
fi

# 检查配置文件是否存在
if [ ! -f "./config.json" ]; then
    echo "错误: 找不到 config.json 配置文件"
    exit 1
fi

# 给可执行文件添加执行权限（如果需要）
chmod +x ./excel2json

echo "开始执行 Excel 转 JSON 转换..."
echo "当前目录: $SCRIPT_DIR"

# 执行转换
./excel2json -c config.json

# 检查执行结果
if [ $? -eq 0 ]; then
    echo "✅ Excel 转 JSON 转换完成！"
else
    echo "❌ 转换过程中出现错误"
fi

echo "按任意键继续..."
read -n 1 -s