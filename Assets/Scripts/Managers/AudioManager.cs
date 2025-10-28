using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static AudioInfo;


public enum AudioType//音频类型
{
    /// <summary>
    /// 背景音
    /// </summary>
    BGM,
    /// <summary>
    /// 界面
    /// </summary>
    UI,
    /// <summary>
    /// 音效
    /// </summary>
    SFX,
    /// <summary>   
    /// 角色对话
    /// </summary>
    Character
}

public class AudioInfo//用于存储音频信息，包括名称、类型、音频剪辑、是否循环和外部路径
{
    public string audioName;
    public AudioType audioType = AudioType.SFX;//默认类型
    public AudioClip audioClip;
    public bool isLoop = false;//是否循环
    public string audioPath = "";//外部加载的路径

    public AudioInfo() { }
    public AudioInfo(string _audioName) { audioName = _audioName; }
    public AudioInfo(string _audioName, string _audioPath) { audioName = _audioName; audioPath = _audioPath; }
    public AudioInfo(string _audioName, AudioType _audioType, bool _isLoop, string _audioPath)
    {
        audioName = _audioName;
        audioType = _audioType;
        isLoop = _isLoop;
        audioPath = _audioPath;
    }

    public class CHANNEL//音频通道，包含通道名称、类型、AudioSource组件和是否忙碌
    {
        public string channelName = " ";//通道名称
        public AudioType audioType = AudioType.SFX;
        public AudioSource audioSource;//音频源的组件
        public bool isBusy = false;//是否忙碌
    }
}
public class AudioManager : UnitySingleton<AudioManager>
{
    public List<CHANNEL> channelPool = new List<CHANNEL>();//音频通道池
    public List<AudioInfo> audioPool = new List<AudioInfo>();//音频信息池

    /// <summary>
    /// 播放外部音频
    /// </summary>
    /// <param name="_audio">音频信息结构体</param>
    /// <param name="callback">完成回调</param>
    public void PlayAudioSync(AudioInfo _audio, UnityAction callback = null)//通过AudioInfo播放的方法
    {
        StartCoroutine(PlayAudioSyncCoroutine(_audio, callback));
    }

    /// <summary>
    /// 播放外部音频
    /// </summary>
    /// <param name="_audioName">音频名</param>
    /// <param name="callback"></param>
    public void PlayAudioSync(string _audioName, UnityAction callback = null)
    {

    }

    private IEnumerator PlayAudioSyncCoroutine(AudioInfo _audio, UnityAction callback = null)
    {

        if (channelPool.Exists(T => T.channelName == _audio.audioName && T.isBusy))//如果已有同名且忙碌的通道
        {
            yield break;//则退出（避免重复播放）
        }
         
        AudioInfo audioInfo = GetAudioInfoByName(_audio.audioName);//从音频信息池中获取音频信息
        if (audioInfo == null)//如果音频信息不存在
        {
            bool loadOver = false;//定义bool，是否加载完毕

            LoadManager.instance.LoadSync<AudioClip>(audioInfo.audioPath, (clip) =>//调用异步加载外部音频
            {
                audioInfo.audioClip = clip;
                loadOver = true;//加载完毕

            });

            while (!loadOver)//等待加载完毕
            {
                yield return null;
            }

            audioPool.Add(audioInfo);//添加到音频信息池
            //下载完音频
        }
        
        CHANNEL channel = DistributeChannel(audioInfo);//分配音频频道
        channel.audioSource.Play();//播放音频

        if (!audioInfo.isLoop)//如果不是循环播放
        {
            while (channel.audioSource.isPlaying)//是否正在播放
            {
                yield return null;
            }

            channel.audioSource.clip = null;//播放结束，释放通道资源
            channel.isBusy = false;
            callback?.Invoke();
        }
    }

    /// <summary>
    /// 通过内部音频播放
    /// </summary>
    /// <param name="clip">音频片段</param>
    /// <param name="audioName">音频名</param>
    /// <param name="audioType">类型</param>
    /// <param name="isLoop"></param>
    /// <param name="callback"></param>
    public void PlayAudioClip(AudioClip clip, string audioName, AudioType audioType = AudioType.SFX, bool isLoop = false, UnityAction callback = null)//通过clip播放的方法
    {
        StartCoroutine(PlayAudioClipCoroutine(clip, audioName, audioType, isLoop, callback));
    }


    public IEnumerator PlayAudioClipCoroutine(AudioClip clip, string audioName, AudioType audioType = AudioType.SFX, bool isLoop = false, UnityAction callback = null)
    {
        if (channelPool.Exists(T => T.channelName == audioName && T.isBusy))//如果已有同名且忙碌的通道
        {
            //yield break;
        }

        AudioInfo audioInfo = GetAudioInfoByName(audioName);
        if (audioInfo == null)
        {
            audioInfo = new AudioInfo(audioName, audioType, isLoop, "");
            audioInfo.audioClip = clip;//分配音频片段
        }

        CHANNEL channel = DistributeChannel(audioInfo);//分配音频频道
        channel.audioSource.Play();

        if (!audioInfo.isLoop)
        {

            while (channel.audioSource.isPlaying)
            {
                yield return null;
            }

            channel.audioSource.clip = null;
            channel.isBusy = false;
            callback?.Invoke();
        }
    }


    /// <summary>
    /// 遍历对象池
    /// </summary>
    /// <param name="_audioName">查找的音频信息名</param>
    /// <returns></returns>
    public AudioInfo GetAudioInfoByName(string _audioName)
    {
        for (int i = 0; i < audioPool.Count; i++)
        {
            if (audioPool[i].audioName == _audioName)
            {
                return audioPool[i];
            }
        }
        return null;
    }



    /// <summary>
    /// 分配音频频道
    /// </summary>
    /// <param name="audioInfo">需要分配通关的音频信息</param>
    /// <returns></returns>
    public CHANNEL DistributeChannel(AudioInfo audioInfo)
    {
        CHANNEL channel = null;

        for (int i = 0; i < channelPool.Count; i++)//查找空闲的通道
        {
            if (!channelPool[i].isBusy)
            {
                channel = channelPool[i];
                break; // 找到第一个空闲通道后立即退出循环
            }
        }

        if (channel == null)//如果没有空闲的通道
        {
            GameObject channelObj = new GameObject("AudioChannel_" + channelPool.Count); // 创建独立 GameObject
            channel = new CHANNEL();//创建新的通道
            channel.audioSource = channelObj.AddComponent<AudioSource>();
            channelPool.Add(channel);
        }

        //设置通道参数
        channel.audioType = audioInfo.audioType;
        channel.channelName = audioInfo.audioName;
        channel.audioSource.clip = audioInfo.audioClip;
        channel.audioSource.loop = audioInfo.isLoop;
        channel.audioSource.volume = 0.5f;//后续对声音进行设置
        channel.isBusy = true;//设置忙碌状态

        return channel;
    }
}

