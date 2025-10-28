using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering.PostProcessing;

public class PostManager : UnitySingleton<PostManager>
{
    [Header("后处理文件")]
    private PostProcessProfile postProcessProfile;
    [Header("适应参数")]
    public float adaptationSpeed = 0.5f;
    public float targetFocusDistance = 3.25f;
    public float initialFocusDistance = 0.1f;
    [Header("光圈参数")]
    public float initialAperture = 15f;    // 初始大光圈（更模糊）
    public float targetAperture = 1.4f;    // 目标小光圈（更清晰）
    [Header("焦距参数")]
    public float focalLength = 50f;
    private PostProcessVolume volume;
    private DepthOfField depthOfField;

    

    public override void Awake()
    {
        base.Awake();
        enabled = false;// 初始禁用脚本
    }
    private void Start()
    {
        CreatPostProcessAndSetting();
        EnsureCameraHasPostProcessLayer();
    }
    void Update()
    {
        // 平滑过渡到清晰状态
        if (depthOfField != null)
        {
            depthOfField.focusDistance.value = Mathf.Lerp(
                depthOfField.focusDistance.value,
                targetFocusDistance,
                adaptationSpeed * Time.deltaTime
            );
            
            depthOfField.aperture.value = Mathf.Lerp(
                depthOfField.aperture.value,
                targetAperture,
                adaptationSpeed * Time.deltaTime
            );
        }
    }
    void CreatPostProcessAndSetting()
    {
        // 添加PostProcessVolume组件并设置
        volume = gameObject.GetOrAddComponent<PostProcessVolume>();
        volume.isGlobal = true;
        volume.priority = 1;

        SetProcessingLayer();//设置处理层

        // 创建后处理配置文件
        postProcessProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
        volume.profile = postProcessProfile;

        // 创建并添加景深效果
        depthOfField = ScriptableObject.CreateInstance<DepthOfField>();
        depthOfField.enabled.Override(true);
        
        // 设置初始参数
        depthOfField.focusDistance.Override(initialFocusDistance);
        depthOfField.aperture.Override(initialAperture);
        depthOfField.focalLength.Override(focalLength);
        
        // 确保这些参数可以被覆盖
        depthOfField.focusDistance.overrideState = true;
        depthOfField.aperture.overrideState = true;
        depthOfField.focalLength.overrideState = true;
        
        // 添加到配置文件
        postProcessProfile.AddSettings(depthOfField);
    }
    void EnsureCameraHasPostProcessLayer()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // 添加PostProcessLayer组件如果不存在
            PostProcessLayer layer = mainCamera.GetComponent<PostProcessLayer>();
            if (layer == null)
            {
                layer = mainCamera.gameObject.AddComponent<PostProcessLayer>();
                layer.volumeTrigger = mainCamera.transform;

                // 设置层（通常使用默认的"PostProcessing"层）
                int postProcessingLayer = LayerMask.NameToLayer("PostProcessing");
                if (postProcessingLayer != -1)
                {
                    layer.volumeLayer = 1 << postProcessingLayer;
                }
            }
        }
    }

    void SetProcessingLayer()
    {
        int postProcessingLayer = LayerMask.NameToLayer("PostProcessing");
        if (postProcessingLayer != -1)
        {
            gameObject.layer = postProcessingLayer;
        }
        else
        {
            Debug.LogWarning("PostProcessing层不存在，使用默认层");
            gameObject.layer = 0; // 默认层
        }
    }

    public void StartAdaptation(float speed = -1, float targetFocus = -1, float targetAperture = -1)
    {
        if (speed > 0) adaptationSpeed = speed;
        if (targetFocus > 0) targetFocusDistance = targetFocus;
        if (targetAperture > 0) this.targetAperture = targetAperture;

        enabled = true;
    }
    public void ResetToBlur()
    {
        if (depthOfField != null)
        {
            depthOfField.focusDistance.value = initialFocusDistance;
            depthOfField.aperture.value = initialAperture;
        }
        enabled = false;
    }
    // 公共方法：立即设置为清晰状态
    public void SetToClear()
    {
        if (depthOfField != null)
        {
            depthOfField.focusDistance.value = targetFocusDistance;
            depthOfField.aperture.value = targetAperture;
        }
        enabled = false;
    }

}
