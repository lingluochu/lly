using System.Collections;
using System.Collections.Generic;
// using Unity.VisualScripting; // 如无需要可移除
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostManager : UnitySingleton<PostManager>
{
    [Header("URP Volume Profile (可选)")]
    [Tooltip("可指定一个已有的 VolumeProfile 作为模板；留空则运行时动态创建")]
    public VolumeProfile presetProfile;

    [Header("适应参数")]
    public float adaptationSpeed = 0.5f;
    public float targetFocusDistance = 3.25f;
    public float initialFocusDistance = 0.1f;

    [Header("光圈参数")]
    public float initialAperture = 15f;    // 初始大光圈（更模糊）
    public float targetAperture = 1.4f;    // 目标小光圈（更清晰）

    [Header("焦距参数")]
    public float focalLength = 50f;

    // 运行时对象
    private Volume volume;                       // URP 的 Volume 组件
    private VolumeProfile runtimeProfile;        // 运行时使用的 Profile
    private DepthOfField dof;                    // URP 的 DoF（Volume 组件）

    public override void Awake()
    {
        base.Awake();
        enabled = false; // 初始禁用脚本，按需调用 StartAdaptation 开启
    }

    private void Start()
    {
        CreateGlobalVolumeAndProfile();
        EnsureCameraPostFXAndVolumeBinding();
    }

    private void Update()
    {
        // 平滑过渡到清晰状态
        if (dof != null)
        {
            if (dof.focusDistance.overrideState)
            {
                dof.focusDistance.value = Mathf.Lerp(
                    dof.focusDistance.value,
                    targetFocusDistance,
                    adaptationSpeed * Time.deltaTime
                );
            }

            if (dof.aperture.overrideState)
            {
                dof.aperture.value = Mathf.Lerp(
                    dof.aperture.value,
                    targetAperture,
                    adaptationSpeed * Time.deltaTime
                );
            }
        }
    }

    /// <summary>
    /// 创建全局 Volume，并赋予运行时的 VolumeProfile
    /// </summary>
    private void CreateGlobalVolumeAndProfile()
    {
        // 挂/取 Volume 组件
        volume = gameObject.GetComponent<Volume>();
        if (volume == null) volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;
        SetProcessingLayer(); // 将本物体放到 PostProcessing 层（若有）

        // 准备运行时 Profile：克隆或新建
        if (presetProfile != null)
        {
            runtimeProfile = ScriptableObject.Instantiate(presetProfile);
            runtimeProfile.name = presetProfile.name + " (Runtime)";
        }
        else
        {
            runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            runtimeProfile.name = "RuntimeProfile (Generated)";
        }

        volume.profile = runtimeProfile;

        // 尝试获取/添加 DoF 组件
        if (!runtimeProfile.TryGet(out dof))
        {
            dof = runtimeProfile.Add<DepthOfField>(true); // true: 覆盖状态默认启用
        }

        // 尝试设置为物理景深（若可用）
#if UNITY_6000_0_OR_NEWER || UNITY_2022_3_OR_NEWER
        // 新版 URP DepthOfField 支持不同模式（如 Gaussian / Bokeh / Physical）
        // 这里优先选 Physical（若该枚举存在），否则按当前 API 忽略
        try
        {
            // 反射安全设置（兼容部分老版本无该枚举项的情况）
            var modeField = typeof(DepthOfField).GetField("mode");
            if (modeField != null)
            {
                var enumType = modeField.FieldType;
                var physical = System.Enum.Parse(enumType, "Physical", ignoreCase: true);
                modeField.SetValue(dof, physical);
            }
        }
        catch { /* 忽略：旧版无 Physical 模式 */ }
#endif

        // 初始化参数并确保可覆盖
        dof.active = true;
        dof.focusDistance.overrideState = true;
        dof.aperture.overrideState = true;
        dof.focalLength.overrideState = true;

        dof.focusDistance.value = initialFocusDistance;
        dof.aperture.value = initialAperture;
        dof.focalLength.value = focalLength;
    }

    /// <summary>
    /// 确保主相机开启后处理，并正确绑定 Volume Layer 与 Trigger
    /// </summary>
    private void EnsureCameraPostFXAndVolumeBinding()
    {
        var cam = Camera.main;
        if (cam == null) return;

        var data = cam.GetComponent<UniversalAdditionalCameraData>();
        if (data == null) data = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

        data.renderPostProcessing = true;   // 开启 URP 后处理

        // 设置 Volume 影响的层（建议单独建层：PostProcessing）
        int postProcessingLayer = LayerMask.NameToLayer("PostProcessing");
        if (postProcessingLayer != -1)
        {
            data.volumeLayerMask = (1 << postProcessingLayer);
        }
        else
        {
            // 若没建层，就让相机能读到默认层
            data.volumeLayerMask = ~0; // 或者 LayerMask.GetMask("Default")
        }

        // 设置体积触发（体积系统以此 Transform 作为范围判定）
        data.volumeTrigger = cam.transform;
    }

    private void SetProcessingLayer()
    {
        int postProcessingLayer = LayerMask.NameToLayer("PostProcessing");
        if (postProcessingLayer != -1)
        {
            gameObject.layer = postProcessingLayer;
        }
        else
        {
            Debug.LogWarning("⚠ 未找到名为 \"PostProcessing\" 的层，已使用默认层。");
            gameObject.layer = 0; // Default
        }
    }

    // —— 对外控制接口（保持你原本的用法） ——

    public void StartAdaptation(float speed = -1, float targetFocus = -1, float targetAperture = -1)
    {
        if (speed > 0) adaptationSpeed = speed;
        if (targetFocus > 0) targetFocusDistance = targetFocus;
        if (targetAperture > 0) this.targetAperture = targetAperture;

        enabled = true;
    }

    public void ResetToBlur()
    {
        if (dof != null)
        {
            if (dof.focusDistance.overrideState) dof.focusDistance.value = initialFocusDistance;
            if (dof.aperture.overrideState) dof.aperture.value = initialAperture;
        }
        enabled = false;
    }

    public void SetToClear()
    {
        if (dof != null)
        {
            if (dof.focusDistance.overrideState) dof.focusDistance.value = targetFocusDistance;
            if (dof.aperture.overrideState) dof.aperture.value = targetAperture;
        }
        enabled = false;
    }
}