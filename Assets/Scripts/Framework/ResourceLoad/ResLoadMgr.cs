using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResLoadMgr : BaseManager<ResLoadMgr>
{
    // 获取全局配置
    private static ResLoadSettings Settings => ResLoadSettings.Instance;

    private ResLoadMgr() { }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resName">资源名称</param>
    /// <returns>加载到的资源</returns>
    public void LoadRes<T>(string resRootPath,string resName, UnityAction<T> callBack, bool isSync = false) where T : Object
    {
        T res = null;
        string resPath = "";
        if (isSync)
        {
            switch (Settings.resLoadType)
            {
                case E_ResLoadType.Editor:
                    resPath = resRootPath + "/" + resName;
                    res = EditorResMgr.Instance.LoadEditorRes<T>(resPath);
                    callBack?.Invoke(res);
                    break;
                // 因为AB包不支持同步加载，所以这里使用Resource的方式进行加载
                case E_ResLoadType.AB:
                    ABMgr.Instance.LoadResAsync<T>(resRootPath, resName, callBack,true);
                    break;
                case E_ResLoadType.Resources:
                    resPath = resRootPath + "/" + resName;
                    res = ResMgr.Instance.Load<T>(resPath);
                    callBack?.Invoke(res);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (Settings.resLoadType)
            {
                case E_ResLoadType.Editor:
                    Debug.LogError($"异步加载资源{resName}失败，因为编辑器模式下不支持异步加载");
                    break;
                // 因为AB包不支持同步加载，所以这里使用Resource的方式进行加载
                case E_ResLoadType.AB:
                    ABMgr.Instance.LoadResAsync<T>(resRootPath, resName, callBack,false);
                    break;
                case E_ResLoadType.Resources:
                    resPath = resRootPath + "/" + resName;
                    ResMgr.Instance.LoadAsync<T>(resPath, callBack);
                    break;
                default:
                    break;
            }
        }
    }


}
