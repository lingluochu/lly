using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 操作模式
/// </summary>
public enum OperationMode//枚举定义游戏的所有操作模式
{
    浏览模式,
    训练模式,
    考核模式
}
public class GlobalDefine : MonoBehaviour//全局定义
{
    /// <summary>
    /// 当前的操作模式
    /// </summary>
    public static OperationMode currentOperationMode = OperationMode.浏览模式;

    /// <summary>
    /// 背景音乐音量
    /// </summary>
    public static float BGMVolume//使用Unity的PlayerPrefs系统在本地存储音量值
    {
        get { return PlayerPrefs.GetFloat("BGMVolume", 0.5f); }//首次获取时返回0.5
        set { PlayerPrefs.SetFloat("BGMVolume", value); }//每次设置值后自动更新存储
    }
}
/// <summary>
/// 辅助进行MonoBehaviour的一些操作
/// </summary>
public static class MonoBehaviourHelper//在没有MonoBehaviour的情况下，可以用这个类来实现协程的开启和关闭
{
    public static Dictionary<string, GameObject> coroutines = new Dictionary<string, GameObject>();//存放协程的字典
    /// <summary>
    /// 开启协程
    /// </summary>
    /// <param name="routine"></param>
    /// <param name="isPresent"></param>
    /// <returns></returns>
    public static Coroutine StartCoroutine(IEnumerator routine, bool isPresent = false)
    {
        MonoBehaviourInstance monoHelper = new GameObject("Coroutine").AddComponent<MonoBehaviourInstance>();
        return monoHelper.DestroyWhenComplete(routine, isPresent);
    }
    /// <summary>
    /// 关闭协程
    /// </summary>
    /// <param name="routine"></param>
    public static void StopCoroutine(IEnumerator routine)
    {
        GameObject G;
        if (coroutines.TryGetValue(routine.GetType().FullName, out G))
        {
            MonoBehaviour.Destroy(G);
        }
    }

    public class MonoBehaviourInstance : MonoBehaviour
    {
        public Coroutine DestroyWhenComplete(IEnumerator routine, bool isPresent)
        {
            if (isPresent)
            {
                DontDestroyOnLoad(this.gameObject);
                coroutines.Add(routine.GetType().FullName, this.gameObject);
            }
            return StartCoroutine(DestroyObjectHandler(routine));
        }
        public IEnumerator DestroyObjectHandler(IEnumerator routine)
        {
            yield return StartCoroutine(routine);
            Destroy(this.gameObject);
        }
    }
}
