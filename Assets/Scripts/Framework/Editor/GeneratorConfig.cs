using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GeneratorConfig
{
    public static string FindComponentGeneratorPath = Application.dataPath + "/ZMUIFrameWork/Scripts/FindComponent";
    public static string BindComponentGeneratorPath = Application.dataPath + "/ZMUIFrameWork/Scripts/BindComponent";
    public static string WindowGeneratorPath = Application.dataPath + "/ZMUIFrameWork/Scripts/Window";
    public static string ObjDataListKey = "objDataList";
    // 标记常量
    public const string NEW_ADD_MARKER = "// NEW_ADD";      // 新增标记
    public const string REMOVE_MARKER = "// REMOVE_MARK";   // 移除标记


    // 备份配置
    public static string BackupPath => BindComponentGeneratorPath + "/Backup"; // 备份目录
    public const int MaxBackupCount = 10; // 最大保留备份数量
    public const string BackupFileSuffix = "_Backup"; // 备份文件后缀
    public const string BackupFileExtension = ".uibak"; // 自定义备份扩展名

    // 高亮颜色配置
    public static readonly Color NewAddColor = new Color(0.2f, 0.8f, 0.2f); // 浅绿色（新增）
    public static readonly Color RemoveColor = new Color(0.8f, 0.2f, 0.2f); // 浅红色（移除）
    public static readonly Color NormalColor = Color.white;
}
