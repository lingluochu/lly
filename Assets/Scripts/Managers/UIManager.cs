using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : UnitySingleton<UIManager>
{
    /// <summary>
    /// UI对象池存储（类全名，UIBase类）
    /// </summary>
    private Dictionary<string, UIBase> UIDic = new Dictionary<string, UIBase>();//键对值，可通过key访问对应的value

    /// <summary>
    /// 获取当前UI对象池数量
    /// </summary>
    public int UICount
    {
        get { return UIDic.Count; }
    }
    
    private Transform canvasTransform;//UI根节点

    public override void Awake()
    {
        base.Awake();
    }
    /// <summary>
    /// ShowUI方法  
    /// </summary>
    /// <typeparam name="T">UIBase类</typeparam>
    /// <returns></returns>
    public UIBase ShowUI<T>() where T : UIBase
    {
        Type t = typeof(T);//获取T的类型

        if (UIDic.ContainsKey(t.FullName))//检查对象池中是否有该对象
        {
            UIDic[t.FullName].Show();//调用对应类的show方法
            return UIDic[t.FullName];
        }

        UIBase uiBase = Activator.CreateInstance(t) as UIBase;//基于反射系统运行中动态实例化，安全转化为UIBase类型，本质不变

        if (string.IsNullOrEmpty(uiBase.prefabsPath))//检查prefabs路径是否为空
        {
            Debug.LogError(String.Format("您查找的{0}路径不存在", uiBase.uiName));
            return null;
        }

        GameObject uiGameObject = LoadManager.instance.Load<GameObject>(uiBase.prefabsPath, canvasTransform);//获取预制体
        
        // 获取Canvas组件并设置排序
        Canvas uiCanvas = uiGameObject.GetComponent<Canvas>();
        if (uiCanvas != null)
        {
            // 假设myLayer现在用于表示sortingOrder
            uiCanvas.sortingOrder = uiBase.canvasSort; 
        }
        else
        {
            Debug.LogError($"{t.FullName}预制体缺少Canvas组件");
            return null;
        }

        uiBase.uiName = t.FullName;//设置UI名称
        uiBase.uiGameObject = uiGameObject;//设置UI预制体
        uiBase.FindComponents();
        uiBase.Show();//调用show方法
        UIDic.Add(t.FullName, uiBase);//加入对象池
        return uiBase;
    }
    /// <summary>
    /// 关闭UI方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void CloseUI<T>() where T : UIBase
    {
        UIBase uiBase = GetUIBaseFromPool<T>();
        if (uiBase != null)
        {
            uiBase.Close();
        }
    }
    /// <summary>
    /// 刷新UI方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    public void FreshUI<T>(params object[] args) where T : UIBase
    {
        UIBase uiBase = GetUIBaseFromPool<T>();
        if (uiBase != null)
        {
            uiBase.Fresh(args);
        }
    }


    /// <summary>
    /// 初始化UI方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    public void InitUI<T>(params object[] args) where T : UIBase
    {
        UIBase uiBase = GetUIBaseFromPool<T>();
        if (uiBase != null)
        {
            uiBase.Init(args);
        }
    }
    /// <summary>
    /// 移除UI（比较耗性能）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void RemoveUI<T>() where T : UIBase
    {
        Type t = typeof(T);
        UIBase uiBase = GetUIBaseFromPool<T>();

        if (uiBase != null)
        {
            uiBase.Destroy();
            Destroy(uiBase.uiGameObject);
            UIDic.Remove(t.FullName);
        }
        else
        {
            Debug.Log(String.Format("销毁{0}的UI对象不存在", t.FullName));
        }
    }
    /// <summary>
    /// 检索对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public UIBase GetUIBaseFromPool<T>()
    {
        Type t = typeof(T);
        UIBase uiBase = null;
        if (!UIDic.TryGetValue(t.FullName, out uiBase))//检查对象池中是否有该对象
        {
            return null;
        }
        return uiBase;
    }
    private void Update()//用于需要更新的UIBase对象
    {
        if (UIDic.Count > 0)//有UIBase对象时
        {
            foreach (var item in UIDic.Keys)
            {
                if (UIDic[item] != null)
                {
                    UIDic[item].Update();
                }
            }
        }
    }
}

