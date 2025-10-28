using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class ScenesManager : UnitySingleton<ScenesManager>
{

    public delegate void OnSceneLoad(string sceneName);//委托类型定义
    public static event OnSceneLoad onSceneLoad;//创建静态事件onSceneLoad，允许其他脚本订阅场景加载完成通知
    public delegate void BeforeSceneLoad(string sceneName);//委托类型定义
    public static event OnSceneLoad beforeSceneLoad;
    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="sceneName">指定场景名</param>
    /// <param name="loadProgress">加载进度回调</param>
    /// <param name="callback">加载完成回调</param>
    public void LoadScene(string sceneName, UnityAction<float> loadProgress = null, UnityAction callback = null)// = null表示参数可选，可不传
    {
        beforeSceneLoad?.Invoke(sceneName);
        StartCoroutine(LoadSceneAsync(sceneName, loadProgress, callback));
    }

    public IEnumerator LoadSceneAsync(string sceneName, UnityAction<float> loadProgress, UnityAction Callback = null)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);//创建异步加载操作，调用异步加载场景方法，模式为替换当前场景
        asyncOperation.allowSceneActivation = false;//禁止自动激活场景，等待加载完成后再激活
        while (asyncOperation.progress < 0.9f)//当进度小于0.9时，循环等待
        {
            loadProgress?.Invoke(asyncOperation.progress);//参数实时传递进度值
            yield return null;//在协程的循环中，必须加入yield return null，否则协程会卡死
        }
        loadProgress?.Invoke(1);//强制设置进度为1

        yield return new WaitForSeconds(1f);

        asyncOperation.allowSceneActivation = true;//允许激活场景
        yield return new WaitForSeconds(1f);//等待1秒，确保所有资源加载完成(等待场景内对象Awake/Start方法执行完毕)

        Callback?.Invoke();//调用回调函数
        onSceneLoad?.Invoke(sceneName);//触发静态事件onSceneLoad，通知其他脚本场景加载完成

        SceneManager.UnloadSceneAsync(sceneName);
    }
}
