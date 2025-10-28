using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LoadManager : UnitySingleton<LoadManager>//加载管理器继承UnitySingleton
{
    /// <summary>
    /// 从Resource加载
    /// </summary>
    /// <typeparam name="T">调用方法的返回类型</typeparam>
    /// <param name="name">/Assets/Resources/下的文件名</param>
    /// <param name="parent">父对象</param>
    /// <returns></returns>
    public T Load<T>(string name,Transform parent = null) where T : Object//约束为Object——所有Unity引擎对象类型
    {
        T resource = Resources.Load<T>(name);//调用内置load方法，赋值给对应类型的变量
        if (resource is GameObject)//如果资源是GameObject
        {
            return Instantiate(resource,parent);//则实例化
        }
        else
        {
            return resource;
        }
    }
    /// <summary>
    /// 异步加载外部资源
    /// </summary>
    /// <typeparam name="T">调用的方法的返回类型</typeparam>
    /// <param name="Url">网络资源的地址</param>
    /// <param name="callback">加载完成后的回调</param>
    public void LoadSync<T>(string Url, UnityAction<T> callback) where T : Object
    {
        StartCoroutine(ReallyLoadsync(Url, callback));
    }

    public IEnumerator ReallyLoadsync<T>(string Url, UnityAction<T> callback) where T : Object
    {
        WWW www = new WWW(Url);//创建WWW下载器，下载指定Url的资源

        yield return www;//等待下载完成

        if (www.error == null)//如果没有错误
        {
            switch (typeof(T).Name)//根据资源类型，调用不同的回调函数 
            {
                case "TextAsset": TextAsset ta = new TextAsset(www.text);//将下载对象的文本内容赋值给TextAsset对象
                                  callback.Invoke(ta as T);//“as T”强制转换类型为T，确保与UnityAction<T>的类型一致
                                  break;
                case "Texture2D": callback(www.texture as T); break;//下载对象的纹理数据
                case "AudioClip": callback(www.GetAudioClip() as T); break;//通过GetAudioClip方法获取音频数据
            }
        }
        else
        {
            Debug.LogError("检查加载路径" + Url);
        }
    }
}

//注：as T 运算符** 是安全类型转换：成功：返回目标类型引用。失败：返回 null 而不报错