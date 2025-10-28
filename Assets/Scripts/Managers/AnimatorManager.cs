using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorManager : UnitySingleton<AnimatorManager>
{
    
    /// <summary>
    /// 动画循环池
    /// </summary>
    public Dictionary<GameObject, string> AnimatorLoopPool = new Dictionary<GameObject, string>();//使用字典记录循环播放动画的对象及其动画名称
    private void Start()
    {
        ScenesManager.beforeSceneLoad += ClearAllLoopingPool;//将清楚对象池功能添加到代理事件
    }
    /// <summary>
    /// 播放动画方法
    /// </summary>
    /// <param name="obj">选中对象</param>
    /// <param name="animName">动画片段名</param>
    /// <param name="speed">播放速度</param>
    /// <param name="isLoop">是否循环</param>
    /// <param name="callback">动画完成回调</param>
    /// <param name="playingProgress">播放进度回调</param>
    public void PlayAnimator(GameObject obj, string animName, UnityAction callback = null,float speed = 1.0f, bool isLoop = false,  UnityAction<float> playingProgress = null)
    {
        StartCoroutine(PlayAnimatorCoroutine(obj, animName , callback , speed , isLoop , playingProgress));
    }

    public IEnumerator PlayAnimatorCoroutine(GameObject obj, string animName,UnityAction callback = null, float speed = 1.0f, bool isLoop = false,  UnityAction<float> playingCallback = null)
    {
        yield return new WaitForSeconds(Time.deltaTime);//一帧的等待时间
        if (obj.GetComponent<Animator>() != null)//如果对象有动画组件
        {
            Animator animator = obj.GetComponent<Animator>();//获取动画组件
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);//获取当前动画层的状态信息
            animator.Play(animName, 0, 0);//播放动画，第0层，从第0帧
            animator.speed = speed;//设置播放速度
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animName))//等待动画片段加载的过程中
            {
                yield return null;
            }
            bool isPlaying = true; //定义动画是否在播放的bool

            if (isLoop)//如果循环
            {
                AnimatorLoopPool.Add(obj, animName);//将对象及其动画名称添加到循环动画池
                while (isPlaying)
                {
                    yield return null;//加入返回空，防止卡死
                    stateInfo = animator.GetCurrentAnimatorStateInfo(0);//更新状态信息
                    if (stateInfo.normalizedTime >= 1.0f)//normalizedTime为动画播放进度，大于1.0说明动画播放完毕
                    {
                        animator.Play(animName, 0, 0);//重新播放动画

                    }
                }
            }
            else//如果不循环
            {
                while (isPlaying)
                {
                    yield return new WaitForSeconds(Time.deltaTime);

                    if(obj == null){yield break;}

                    stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    playingCallback?.Invoke(stateInfo.normalizedTime);
                    if (stateInfo.normalizedTime >= 1.0f)
                    {
                        isPlaying = false;
                    }
                }
                callback?.Invoke();
            }

        }
    }

    /// <summary>
    /// 清空全部对象池
    /// </summary>
    public void ClearAllLoopingPool(string sceneName)
    {
        AnimatorLoopPool.Clear();//清空全部循环动画池
    }

    /// <summary>
    /// 清空对象池中的指定对象
    /// </summary>
    /// <param name="obj"></param>
    public void ClearLoopingPool(GameObject obj)
    {
        for (int i = 0; i < AnimatorLoopPool.Count; i++)
        {
            (GameObject key, string value) = AnimatorLoopPool.ElementAt(i);//返回字典中第i个位置的键值对
            if (key == obj)
            {
                AnimatorLoopPool.Remove(obj);
            }
        }
    }

    void OnDestroy()
    {
        ScenesManager.beforeSceneLoad -= ClearAllLoopingPool;
    }
}
