import sys
import openpyxl
import json
import argparse
import os
from pathlib import Path


def excel_to_json(excel_path, json_path):
    """将单个Excel文件转换为JSON格式"""
    try:
        # 加载Excel工作簿
        workbook = openpyxl.load_workbook(excel_path)
        sheet = workbook.active  # 获取第一个工作表

        # 读取字段类型（第一行）和字段名（第三行）
        type_row = sheet[1]
        field_row = sheet[3]

        if not type_row or not field_row:
            raise ValueError("Excel文件格式不正确，缺少表头行")

        # 提取字段信息
        fields = []
        for type_cell, field_cell in zip(type_row, field_row):
            if field_cell.value is None:
                continue
            fields.append({"name": field_cell.value, "type": type_cell.value})

        # 读取并转换数据
        data = []
        for row in sheet.iter_rows(min_row=4, values_only=True):
            row_data = {}
            has_value = False  # 标记此行是否有非空值

            for i, field in enumerate(fields):
                value = row[i] if i < len(row) else None

                # 检查是否有非空值
                if value is not None:
                    has_value = True

                # 类型转换
                if field["type"] == "int":
                    try:
                        row_data[field["name"]] = (
                            int(value) if value is not None else None
                        )
                    except (ValueError, TypeError):
                        row_data[field["name"]] = value
                elif field["type"] == "float":
                    try:
                        row_data[field["name"]] = (
                            float(value) if value is not None else None
                        )
                    except (ValueError, TypeError):
                        row_data[field["name"]] = value
                elif field["type"] == "bool":
                    try:
                        row_data[field["name"]] = (
                            bool(value) if value is not None else None
                        )
                    except (ValueError, TypeError):
                        row_data[field["name"]] = value
                elif field["type"] == "str":
                    row_data[field["name"]] = str(value) if value is not None else None
                elif field["type"] == "list[int]":
                    try:
                        row_data[field["name"]] = (
                            [int(x.strip()) for x in value.split(",")]
                            if value is not None
                            else None
                        )
                    except (ValueError, TypeError):
                        row_data[field["name"]] = value
                elif field["type"] == "list[float]":
                    try:
                        row_data[field["name"]] = (
                            [float(x.strip()) for x in value.split(",")]
                            if value is not None
                            else None
                        )
                    except (ValueError, TypeError):
                        row_data[field["name"]] = value
                elif field["type"] == "list[bool]":
                    try:
                        row_data[field["name"]] = (
                            [bool(x.strip()) for x in value.split(",")]
                            if value is not None
                            else None
                        )
                    except (ValueError, TypeError):
                        row_data[field["name"]] = value
                elif field["type"] == "list[str]":
                    try:
                        row_data[field["name"]] = (
                            [x.strip() for x in value.split(",")]
                            if value is not None
                            else None
                        )
                    except (ValueError, TypeError):
                        row_data[field["name"]] = value
                else:
                    row_data[field["name"]] = str(value) if value is not None else None

            # 只添加有非空值的行
            if has_value:
                data.append(row_data)

        # 确保输出目录存在
        os.makedirs(os.path.dirname(json_path), exist_ok=True)

        # 写入JSON
        with open(json_path, "w", encoding="utf-8") as f:
            json.dump(data, f, ensure_ascii=False, indent=4)

        print(f"✅ 转换成功：{json_path}")

    except Exception as e:
        print(f"❌ 转换失败（{excel_path}）：{str(e)}")


def batch_convert(input_dir, output_dir):
    """批量转换输入目录下的所有.xlsx文件到输出目录"""
    # 处理输入输出路径为绝对路径（支持相对路径转换）
    input_dir_abs = os.path.abspath(input_dir)
    output_dir_abs = os.path.abspath(output_dir)
    print(f"输入目录：{input_dir_abs}")
    print(f"输出目录：{output_dir_abs}")

    # 遍历所有.xlsx文件（包括子目录）
    for excel_path in Path(input_dir_abs).rglob("*.xlsx"):
        excel_abs = str(excel_path)

        # 计算相对路径（保持目录结构）
        relative_path = os.path.relpath(excel_abs, input_dir_abs)

        # 生成输出JSON路径
        json_abs = os.path.join(
            output_dir_abs, os.path.splitext(relative_path)[0] + ".json"
        )

        # 执行转换
        excel_to_json(excel_abs, json_abs)


def main():
    parser = argparse.ArgumentParser(
        description="批量Excel转JSON工具（支持输入输出路径和相对路径）"
    )
    parser.add_argument(
        "-i", "--input", required=True, help="输入目录路径（支持相对路径）"
    )
    parser.add_argument(
        "-o", "--output", required=True, help="输出目录路径（支持相对路径）"
    )

    args = parser.parse_args()
    print(f"输入参数：{args}")
    # 验证输入目录是否存在
    if not os.path.isdir(args.input):
        print(f"错误：输入目录不存在 - {args.input}")
        return

    # 确保输出目录存在
    os.makedirs(args.output, exist_ok=True)

    # 执行批量转换
    print(f"开始处理：输入目录={args.input}，输出目录={args.output}")
    batch_convert(args.input, args.output)
    print("批量转换完成！")


if __name__ == "__main__":
    main()
