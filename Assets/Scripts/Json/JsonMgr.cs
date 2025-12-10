using LitJson;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 序列化和反序列化Json时  使用的是哪种方案
/// </summary>
public enum JsonType
{
    /// <summary>
    /// Unity自带的JsonUtlity
    /// </summary>
    JsonUtlity,
    /// <summary>
    /// 第三方插件LitJson
    /// </summary>
    LitJson,
    /// <summary>
    /// 第三方插件NewtonsoftJson
    /// </summary>
    NewtonsoftJson
}

/// <summary>
/// Json数据管理类 主要用于进行 Json的序列化存储到硬盘 和 反序列化从硬盘中读取到内存中
/// </summary>
public class JsonMgr:BaseManager<JsonMgr>
{
    private JsonMgr(){}
    //存储Json数据 序列化
    public void SaveData(object data, string fileName, JsonType type = JsonType.NewtonsoftJson)
    {
        //确定存储路径
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        Debug.Log("存储位置"+path);
        //序列化 得到Json字符串
        string jsonStr = "";
        switch (type)
        {
            case JsonType.JsonUtlity:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.LitJson:
                jsonStr = JsonMapper.ToJson(data);
                break;
            case JsonType.NewtonsoftJson:
                jsonStr = JsonConvert.SerializeObject(data);
                break;
        }
        //把序列化的Json字符串 存储到指定路径的文件中
        File.WriteAllText(path, jsonStr);
    }

    //读取指定文件中的 Json数据 反序列化
    public T LoadData<T>(string fileName, JsonType type = JsonType.NewtonsoftJson) where T : new()
    {
        //确定从哪个路径读取
        //首先先判断 默认数据文件夹中是否有我们想要的数据 如果有 就从中获取
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //先判断 是否存在这个文件
        //如果不存在默认文件 就从 读写文件夹中去寻找
        if(!File.Exists(path))
            path = Application.persistentDataPath + "/" + fileName + ".json";
        //如果读写文件夹中都还没有 那就返回一个默认对象
        if (!File.Exists(path))
            return new T();

        //进行反序列化
        string jsonStr = File.ReadAllText(path);
        //数据对象
        T data = default(T);
        switch (type)
        {
            case JsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.LitJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
            case JsonType.NewtonsoftJson:
                data = JsonConvert.DeserializeObject<T>(jsonStr);
                break;
        }

        //把对象返回出去
        return data;
    }

    /// <summary>
    /// 把数据存储到指定路径的文件中
    /// </summary>
    /// <param name="data">要存储的数据</param>
    /// <param name="filePath">要存储的文件路径</param>
    /// <param name="type">使用哪种Json序列化方案</param>
    public void SaveDataToFilePath(object data,string filePath, JsonType type = JsonType.NewtonsoftJson)
    {
        //确定存储路径
        if(string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("文件路径不能为空");
            return;
        }
        //如果没有添加后缀，默认添加.json
        if(!filePath.Contains(".json"))
            filePath += ".json";
        Debug.Log("存储位置"+filePath);
        //序列化 得到Json字符串
        string jsonStr = "";
        switch (type)
        {
            case JsonType.JsonUtlity:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.LitJson:
                jsonStr = JsonMapper.ToJson(data);
                break;
            case JsonType.NewtonsoftJson:
                jsonStr = JsonConvert.SerializeObject(data);
                break;
        }
        //把序列化的Json字符串 存储到指定路径的文件中
        File.WriteAllText(filePath, jsonStr);
    }

    /// <summary>
    /// 从指定路径的文件中读取Json数据 反序列化
    /// </summary>
    /// <typeparam name="T">要反序列化的数据类型</typeparam>
    /// <param name="filePath">要读取的文件路径</param>
    /// <param name="type">使用哪种Json序列化方案</param>
    /// <returns>反序列化后的数据对象</returns>
    public T LoadDataFromFilePath<T>(string filePath, JsonType type = JsonType.NewtonsoftJson) where T : new()
    {
        //确定存储路径
        if(string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("文件路径不能为空");
            return default(T);
        }
        //如果没有添加后缀，默认添加.json
        if(!filePath.Contains(".json"))
            filePath += ".json";
        //如果不存在filePath
        if(!File.Exists(filePath))
        {
            Debug.LogError("文件路径不存在,请检查文件是否存在"+filePath);
            return default(T);
        }
        //进行反序列化
        string jsonStr = File.ReadAllText(filePath);
        //数据对象
        T data = default(T);
        switch (type)
        {
            case JsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.LitJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
            case JsonType.NewtonsoftJson:
                data = JsonConvert.DeserializeObject<T>(jsonStr);
                break;
        }
        //把对象返回出去
        return data;
    }
}
