using UnityEngine;

/// <summary>
/// 不继承MonoBehaviour的单例模式
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> where T : new()// 确保T有默认构造函数
{
    private static T _instance;// 静态 T 类型的实例
    public static object mutex = new object();// 每个单例专用的线程锁

    public static T Instance //触发静态构造函数
    {
        get //当第一次访问 T.Instance 时,,(延迟加载)
        {
            if (_instance == null)//避免已初始化时的锁开销
            {
                lock (mutex)//加锁
                {
                    if (_instance == null)//防止其他线程已创建实例
                    {
                        _instance = new T();//实例化
                    }
                }
            }
            return _instance;//返回 T 类型的实例
        }
    }
}
/// <summary>
/// 继承MonoBehaviour的单例模式
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnitySingleton<T> : MonoBehaviour where T : Component //约束T是组件
{
    private static T _instance;

    public static T instance
    {
        get//其他脚本访问instance时,调用
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject();//创建GameObject
                _instance = (T)obj.AddComponent(typeof(T));//挂载组件
                obj.hideFlags = HideFlags.DontSave;//不保存到场景中,退出运行模式自动销毁
                obj.name = typeof(T).Name;//设置GameObject的名字
            }
            return _instance;
        }
        set { _instance = value; } //允许外部修改实例
    }
    public virtual void Awake()//场景中有挂载该脚本的对象时
    {
        
        DontDestroyOnLoad(this.gameObject);//跨场景时不销毁
        if (_instance == null)
        {
            _instance = this as T;//给_instance赋值为T类型
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}


//注 ：非继承MonoBehaviour的单例模式，可能存在多线程的模式，需要线程锁
//     继承MonoBehaviour的单例模式，适用unity的强制主线程模式，不会出现多线程问题