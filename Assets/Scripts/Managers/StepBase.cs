using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class StepBase : UnitySingleton<StepBase>
{
    /// <summary>
    /// 位置目标特效
    /// </summary>
    [HideInInspector]
    public GameObject targetP;

    /// <summary>
    /// 控制人物信息
    /// </summary>
    public GameObject player
    {
        get
        {
            // 添加空值检查防御
            var node = Generic.GetNodeInScene("XR Origin (XR Rig)");
            if (node == null)
            {
                Debug.LogError("未找到XR Origin (XR Rig)节点！请检查场景配置");
                return null; // 或创建兜底对象
            }
            return node.gameObject;
        }
    }
    /// <summary>
    /// 所有步骤的协程方法
    /// </summary>
    public List<IEnumerator> steps = new List<IEnumerator>();

    /// <summary>
    /// 添加所有步骤协程到列表里，并从第0个开始执行
    /// </summary>
    public virtual void Start()
    {
        AddStep();
        Init();
        StartCoroutine(RunStep());
    }
    public virtual void Update()
    {

    }
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {
        ScenesManager.beforeSceneLoad += Kill;
    }
    /// <summary>
    /// 添加步骤
    /// </summary>
    public virtual void AddStep()
    {

    }

    /// <summary>
    /// 执行步骤
    /// </summary>
    public virtual IEnumerator RunStep()
    {
        for (int i = 0; i < steps.Count; i++)
        {
            yield return steps[i];//通过yield return逐个执行，确保上一个完成
        }
    }
    public virtual void Kill(string sceneName)
    {

    }

    public void OnDestroy()
    {
        ScenesManager.beforeSceneLoad -= Kill;
    }
    #region 功能池

    /// <summary>
    /// 移动到目标点
    /// </summary>
    /// <param name="targetName">位置目录</param>
    /// <param name="tipName">位置名</param>
    /// <returns></returns>
    public IEnumerator Target(string targetName, string tipName = "")
    {

        if (tipName == "")
        {
            tipName = targetName;
        }
        ///显示并更新“提示UI”
        //UIManager.instance.ShowUI<UI_Tips>();


        Transform pos = Generic.GetNodeInScene(targetName);//找到对应位置的空对象

        if (targetP == null)
        {
            targetP = LoadManager.instance.Load<GameObject>("Target");
        }
        targetP.transform.position = pos.position;//将加载的物体设定在目标位置
        targetP.SetActive(true);
        //DonDestroyOnLoad(targetP);//保持存活
        if (player == null)
        {
            yield break;
        }
        while (Vector3.Distance(player.transform.position, targetP.transform.position) > 1f)//判断人物是否走到目标位置
        {
            yield return null;//没到则一直返回
        }
        //到了则关闭提示UI
        targetP.gameObject.SetActive(false);
        //UIManager.instance.CloseUI<UI_Tips>();
    }
    /// <summary>
    /// 触发模型
    /// </summary>
    /// <param name="modelName">模型对象名</param>
    public IEnumerator TriggerModel(string modelName)
    {

        //查询模型对象，并为其添加ModelControl组件，并显示模型
        Transform model = Generic.GetNodeInScene(modelName);
        ModelControl modelControl;
        if (model.GetComponent<ModelControl>() == null)
        {
            modelControl = model.AddComponent<ModelControl>();
        }
        else
        {
            modelControl = model.GetComponent<ModelControl>();
        }

        modelControl.EnterInteractableState((value) =>//交互完成后回调函数触发
        {
            modelControl.EnterUnInteractableState();//进入不可执行状态
        });

        while (modelControl.isTrigger == true)//等待模型交互完成
        {
            yield return null;
        }
    }




    /// <summary>
    /// 切换相机
    /// </summary>
    /// <param name="cameraName">相机名</param>
    /// <param name="callback">完成回调</param>
    /// <returns></returns>
    public IEnumerator CameraSwith(string cameraName, UnityAction callback = null)
    {
        Transform camera = Generic.GetNodeInScene(cameraName);
        camera.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        callback?.Invoke();
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="objName">动画对象名</param>
    /// <param name="animName">动画名</param>
    public IEnumerator PlayAnimator(string objName, string animName, UnityAction<float> callback = null)
    {
        Transform obj = Generic.GetNodeInScene(objName);
        Animator anim = obj.GetComponent<Animator>();
        obj.gameObject.SetActive(true);
        bool isOver = false;
        UnityAction action = () => { isOver = true; };//执行action时，isOver置为true
        AnimatorManager.instance.PlayAnimator(obj.gameObject, animName, action, playingProgress: callback =>
        {

        });
        while (!isOver)
        {
            yield return null;
        }
    }

    // <summary>
    // 播放音频
    // </summary>
    // <param name="audioName"></param>
    // <returns></returns>
    public IEnumerator PlayAudioOld(string audioName, bool isLoop = false)
    {

        bool isLoaded = false;
        AudioClip targetClip = null;
        string audioPath = $"{Application.streamingAssetsPath}/Audios/{audioName}.mp3";
        LoadManager.instance.LoadSync<AudioClip>(
            audioPath,
            (clip) =>
            {
                targetClip = clip;
                isLoaded = true;
            }
        );
        yield return new WaitUntil(() => isLoaded);
        // 处理加载失败
        if (targetClip == null)
        {
            Debug.LogError($"音频加载失败: {audioName}");
            yield break;
        }

        // 播放并等待完成
        yield return AudioManager.instance.PlayAudioClipCoroutine(
            targetClip,
            audioName,
            AudioType.UI,
            isLoop: isLoop,
            callback: () => { Debug.Log("播放成功"); }
        );

        // 卸载资源
        Resources.UnloadUnusedAssets();
        targetClip = null;
    }

    public IEnumerator PlayAudio(string name)
    {
        var node = Generic.GetNodeInScene(name);

        if (node == null)
        {
            Debug.LogError("未找到节点：" + name);
            yield break;
        }
        AudioSource audioSource = node.GetComponent<AudioSource>();

        node.gameObject.SetActive(true);
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
        node.gameObject.SetActive(false);

    }

    public IEnumerator EyeAdaptation(float adaptationSpeed)
    {
        Debug.Log("开始色彩校正");
        PostManager.instance.enabled = true;//启用后处理脚本
        PostManager.instance.StartAdaptation();
        yield return new WaitForSeconds(adaptationSpeed);//等待完成
        PostManager.instance.enabled = false;//禁用后处理脚本
    }

    /// <summary>
    /// 设置多个物体的激活状态（显示/隐藏）
    /// 支持通过父物体路径定位同名子物体
    /// </summary>
    /// <param name="targets">目标物体列表（每个元素可以是直接物体名或"父物体/子物体"路径）</param>
    /// <param name="setActive">是否激活（true=显示, false=隐藏）</param>
    /// <param name="delay">延迟执行时间（秒）</param>
    /// <param name="onComplete">完成后的回调</param>
    /// <returns></returns>
    public IEnumerator SetObjectsActive(
        List<string> targets,
        bool setActive,
        float delay = 0f,
        UnityAction onComplete = null)
    {
        // 延迟等待
        if (delay >= 0)
        {
            yield return new WaitForSeconds(delay);
        }

        foreach (var target in targets)
        {
            // 检查是否使用了路径格式（父物体/子物体）
            if (target.Contains("/"))
            {
                // 分割路径
                string[] pathParts = target.Split('/');
                if (pathParts.Length == 2)
                {
                    string parentName = pathParts[0];
                    string childName = pathParts[1];

                    // 查找父物体
                    Transform parent = Generic.GetNodeInScene(parentName);
                    if (parent != null)
                    {
                        // 在父物体下查找指定名称的子物体
                        Transform child = parent.Find(childName);
                        if (child != null)
                        {
                            child.gameObject.SetActive(setActive);
                            Debug.Log($"设置物体 [{parentName}/{childName}] 状态为: {setActive}");
                        }
                        else
                        {
                            Debug.LogWarning($"在父物体 '{parentName}' 下未找到子物体: {childName}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"未找到父物体: {parentName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"无效的目标路径格式: {target} (正确格式: 父物体名/子物体名)");
                }
            }
            else
            {
                // 直接查找物体
                Transform obj = Generic.GetNodeInScene(target);
                if (obj != null)
                {
                    Debug.Log($"设置物体 [{target}] 状态为: {setActive}");
                    obj.gameObject.SetActive(setActive);
                }
                else
                {
                    Debug.LogWarning($"未找到物体: {target}");
                }
            }
        }

        onComplete?.Invoke();
    }
    /// <summary>
    /// 设置老年人的位置旋转和状态动画（无需等待完成）
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="animName">动画片段名</param>
    /// <param name="onComplete">是否完成</param>
    /// <returns></returns>
    public IEnumerator SetOldMan(Vector3 position, Quaternion rotation,string animName, UnityAction onComplete = null)
    {
        GameObject oldMan = Generic.GetNodeInScene("OldMan").gameObject;
        if (oldMan == null)
        {
            Debug.LogError(" OldMan 对象未找到！");
            yield break;
        }
        oldMan.transform.position = position;
        oldMan.transform.rotation = rotation;
        Animator animator = oldMan.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError(" OldMan 缺少 Animator 组件！");
            yield break;
        }
        int stateHash = Animator.StringToHash(animName);
        if (!animator.HasState(0, stateHash))
        {
            Debug.LogError(" OldMan 没有动画片段：" + animName);
            yield break;
        }
        else
        {
            animator.Play(animName);
        }
        onComplete?.Invoke();
        yield return null;
    }

    #endregion
}
    