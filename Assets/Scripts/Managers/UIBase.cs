using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class UIBase //所有UI的基类，定义通用属性和方法
{

    /// <summary>
    /// UI预制体在Resource的资源路径，在子类中定义
    /// </summary>
    public string prefabsPath { get; set; }

    /// <summary>
    /// UI面板名称
    /// </summary>
    public string uiName { get; set; }

    /// <summary>
    /// 决定该UI的层级
    /// </summary>
    public int canvasSort;//canvas组件

    /// <summary>
    /// UI实例化的对象
    /// </summary>
    public GameObject uiGameObject = null;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="args"></param>
    public virtual void Init(params object[] args)//参数的类型与对应数量由子类决定
    {

    }
    /// <summary>
    /// 查找组件
    /// </summary>
    public virtual void FindComponents()
    {

    }

    /// <summary>
    /// 打开UI
    /// </summary>
    public virtual void Show()
    {

    }

    /// <summary>
    /// 关闭UI
    /// </summary>
    public virtual void Close()
    {

    }

    /// <summary>
    /// 刷新UI
    /// </summary>
    /// <param name="args"></param>
    public virtual void Fresh(params object[] args)
    {

    }

    /// <summary>
    /// 执行
    /// </summary>
    public virtual void Update()
    {

    }

    /// <summary>
    /// 销毁UI
    /// </summary>
    public virtual void Destroy()
    {

    }

    /// <summary>
    /// 在子对象中按名字查找子对象
    /// </summary>
    /// <param name="name">子对象的名字</param>
    /// <returns></returns>
    public GameObject GetObject(string name)//UI的命名不能重名
    {
        Transform[] trans = uiGameObject.GetComponentsInChildren<Transform>(true);//获取所有子对象的Transform组件

        foreach (var item in trans)//遍历查找name对应的Trans对象
        {
            if (item.name == name)
            {
                return item.gameObject;//返回gameObject类
            }
        }
        Debug.LogError(String.Format("您查找的{0}子对象不存在", name));
        return null;
    }

    /// <summary>
    /// 查组件，没有则添加
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="name">对象名</param>
    /// <returns></returns>
    public T GetOrAddComponent<T>(string name) where T : Component //给UI对象挂载对应的组件
    {
        GameObject child = GetObject(name);//查找对象

        if (child)//如果对象存在
        {
            if (child.GetComponent<T>() == null)//无组件
            {
                child.AddComponent<T>();//添加组件
            }

            if (typeof(T).Name == "Button")//为所有的Button类UI添加点击事件，通用的点击音效
            {
                child.GetComponent<Button>().onClick.AddListener(() =>
                {
                    //异步加载音效资源
                    LoadManager.instance.LoadSync<AudioClip>
                    (Application.streamingAssetsPath + "/Audios/按键音.mp3",//音效路径
                    (clip) =>//AudioClip类型的回调函数
                    {
                        AudioManager.instance.PlayAudioClip(
                        clip,
                        clip.name,
                        AudioType.UI,
                        callback: () =>
                        {
                            Debug.Log("播放成功");
                        });
                    });
                });
            }
            
            return child.GetComponent<T>();//返回获取组件
        }

        Debug.LogError(String.Format("您查找的{0}子对象不存在", name));
        return null;
    }
    

    protected UIBase()
    {

    }
}
