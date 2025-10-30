using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostManager : UnitySingleton<PostManager>
{
    [Header("Volume Layer 名称（建议单独建层）")]
    public string volumeLayerName = "PostProcessing";

    [Header("过渡速度 (权重每秒变化量)")]
    [Tooltip("值越大过渡越快，推荐 0.5~2")]
    public float adaptationSpeed = 0.8f;

    [Header("Gaussian DoF 参数（若可用优先用）")]
    [Tooltip("模糊起始距离（近端）")]
    public float gaussianStart = 0.1f;
    [Tooltip("模糊结束距离（远端）")]
    public float gaussianEnd = 6.0f;
    [Tooltip("最大模糊半径（像素）")]
    public float gaussianMaxRadius = 1.5f;
    public bool gaussianHighQuality = true;

    [Header("Bokeh/Physical 回退参数（当 Gaussian 不可用时使用）")]
    public float initialFocusDistance = 0.1f;
    public float targetFocusDistance = 3.25f; // 仅备用展示用，不在权重混合中动态使用
    public float initialAperture = 16f;
    public float targetAperture = 1.8f;       // 仅备用展示用，不在权重混合中动态使用
    public float focalLength = 50f;

    // 运行时对象
    private Volume blurVolume;              // 有 DoF 的体积
    private Volume clearVolume;             // 清晰的体积（无 DoF）
    private VolumeProfile blurProfile;      // 模糊配置
    private DepthOfField blurDoF;           // 模糊用的 DoF 组件

    // 状态
    private bool adapting = false;
    private int volumeLayerMask = ~0;

    public override void Awake()
    {
        base.Awake();
        enabled = false;     // 默认不更新，由 StartAdaptation 控制
    }

    private void Start()
    {
        SetupCameraAndLayers();
        CreateVolumes();
    }

    private void Update()
    {
        if (!adapting || blurVolume == null) return;

        // 从模糊(1.0) -> 清晰(0.0)
        blurVolume.weight = Mathf.MoveTowards(blurVolume.weight, 0f, adaptationSpeed * Time.deltaTime);

        if (blurVolume.weight <= 0.0001f)
        {
            blurVolume.weight = 0f;
            adapting = false;
            enabled = false;
        }
    }

    // ========== 初始化与构建 ==========

    private void SetupCameraAndLayers()
    {
        var cam = Camera.main;
        if (!cam) return;

        // 确保相机有 URP 附加数据且启用后处理/深度纹理
        var data = cam.GetComponent<UniversalAdditionalCameraData>();
        if (!data) data = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        data.renderPostProcessing = true;
        data.requiresDepthTexture = true;

        // 体积层设置
        int postLayer = LayerMask.NameToLayer(volumeLayerName);
        if (postLayer == -1)
        {
            Debug.LogWarning($"[PostManager] 未找到体积层 \"{volumeLayerName}\"，将使用全部层(~0)。");
            volumeLayerMask = ~0;
        }
        else
        {
            volumeLayerMask = (1 << postLayer);
        }
        data.volumeLayerMask = volumeLayerMask;
    }

    private void CreateVolumes()
    {
        // 清理旧对象（避免重复创建）
        if (blurVolume) Destroy(blurVolume.gameObject);
        if (clearVolume) Destroy(clearVolume);

        // --- 清晰体积（无 DoF） ---
        clearVolume = gameObject.AddComponent<Volume>();
        clearVolume.isGlobal = true;
        clearVolume.priority = 0f;
        clearVolume.weight = 1f;
        clearVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        SetObjLayer(clearVolume.gameObject);

        // --- 模糊体积（有 DoF） ---
        blurVolume = new GameObject("URP_BlurVolume (Runtime)").AddComponent<Volume>();
        blurVolume.transform.SetParent(transform, false);
        blurVolume.isGlobal = true;
        blurVolume.priority = 1f;       // 真正混合靠 weight
        blurVolume.weight = 1f;         // 初始模糊
        blurProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        blurVolume.profile = blurProfile;
        SetObjLayer(blurVolume.gameObject);

        // 在 blurProfile 上添加 DoF（尽量用 Gaussian，不行则回退），并强制 override
        AddDepthOfFieldForBlurProfile();
    }

    private void SetObjLayer(GameObject go)
    {
        int postLayer = LayerMask.NameToLayer(volumeLayerName);
        go.layer = (postLayer == -1) ? 0 : postLayer; // 找不到就放 Default
    }

    // 兼容不同 URP 版本：优先写 Gaussian 字段，不存在则写 Bokeh/Physical 字段
    private void AddDepthOfFieldForBlurProfile()
    {
        // 获取或添加 DoF
        if (!blurProfile.TryGet(out blurDoF))
            blurDoF = blurProfile.Add<DepthOfField>(true);

        blurDoF.active = true;

        // 1) 先尝试设置 Gaussian 模式（URP 12/14+ 常见）
        bool appliedGaussian = TryApplyGaussian(blurDoF);

        if (!appliedGaussian)
        {
            // 2) 回退到 Bokeh/Physical（老版本或特定管线）
            ApplyBokehOrPhysical(blurDoF);
        }

        // —— 新增：强制开启该组件内所有 VolumeParameter 的 overrideState —— //
        ForceOverrideAllVolumeParameters(blurDoF);
    }

    private bool TryApplyGaussian(DepthOfField dof)
    {
        // 检查是否存在 "mode" 字段以及 "Gaussian" 枚举项
        var modeField = typeof(DepthOfField).GetField("mode");
        if (modeField == null) return false;

        try
        {
            var enumType = modeField.FieldType;
            var gaussianEnum = System.Enum.Parse(enumType, "Gaussian", true);
            modeField.SetValue(dof, gaussianEnum);
        }
        catch { return false; }

        // 反射写入 gaussianStart / gaussianEnd / gaussianMaxRadius / highQualitySampling
        var fStart = typeof(DepthOfField).GetField("gaussianStart");
        var fEnd = typeof(DepthOfField).GetField("gaussianEnd");
        var fMaxR = typeof(DepthOfField).GetField("gaussianMaxRadius");
        var fHQ   = typeof(DepthOfField).GetField("highQualitySampling");

        if (fStart == null || fEnd == null || fMaxR == null || fHQ == null) return false;

        SetVolumeParamFloat(fStart.GetValue(dof), gaussianStart);
        SetVolumeParamFloat(fEnd.GetValue(dof), gaussianEnd);
        SetVolumeParamFloat(fMaxR.GetValue(dof), gaussianMaxRadius);
        SetVolumeParamBool(fHQ.GetValue(dof), gaussianHighQuality);

        // 打开 overrideState（保险起见，下面还会统一再开一遍）
        SetOverrideStateTrue(fStart.GetValue(dof));
        SetOverrideStateTrue(fEnd.GetValue(dof));
        SetOverrideStateTrue(fMaxR.GetValue(dof));
        SetOverrideStateTrue(fHQ.GetValue(dof));

        return true;
    }

    private void ApplyBokehOrPhysical(DepthOfField dof)
    {
        // 优先尝试 Physical；失败则使用 Bokeh（名字不同但字段一致）
        var modeField = typeof(DepthOfField).GetField("mode");
        if (modeField != null)
        {
            object chosen = null;
            try { chosen = System.Enum.Parse(modeField.FieldType, "Physical", true); }
            catch
            {
                try { chosen = System.Enum.Parse(modeField.FieldType, "Bokeh", true); }
                catch { /* 旧版可能没有 mode，忽略 */ }
            }
            if (chosen != null) modeField.SetValue(dof, chosen);
        }

        // 设置 focusDistance / aperture / focalLength
        var fFocus = typeof(DepthOfField).GetField("focusDistance");
        var fApert = typeof(DepthOfField).GetField("aperture");
        var fFocal = typeof(DepthOfField).GetField("focalLength");

        if (fFocus != null) { SetVolumeParamFloat(fFocus.GetValue(dof), initialFocusDistance); SetOverrideStateTrue(fFocus.GetValue(dof)); }
        if (fApert != null) { SetVolumeParamFloat(fApert.GetValue(dof), initialAperture);    SetOverrideStateTrue(fApert.GetValue(dof)); }
        if (fFocal != null) { SetVolumeParamFloat(fFocal.GetValue(dof), focalLength);        SetOverrideStateTrue(fFocal.GetValue(dof)); }
    }

    // ========== 反射小工具（写 VolumeParameter<T>.value 与 overrideState） ==========

    private static void SetVolumeParamFloat(object volumeParam, float v)
    {
        if (volumeParam == null) return;
        var prop = volumeParam.GetType().GetProperty("value");
        if (prop != null && prop.PropertyType == typeof(float)) prop.SetValue(volumeParam, v);
    }

    private static void SetVolumeParamBool(object volumeParam, bool v)
    {
        if (volumeParam == null) return;
        var prop = volumeParam.GetType().GetProperty("value");
        if (prop != null && prop.PropertyType == typeof(bool)) prop.SetValue(volumeParam, v);
    }

    private static void SetOverrideStateTrue(object volumeParam)
    {
        if (volumeParam == null) return;
        var field = volumeParam.GetType().GetField("overrideState");
        if (field != null && field.FieldType == typeof(bool)) field.SetValue(volumeParam, true);
        var prop = volumeParam.GetType().GetProperty("overrideState");
        if (prop != null && prop.PropertyType == typeof(bool)) prop.SetValue(volumeParam, true);
    }

    /// <summary>
    /// 强制开启某个 VolumeComponent（如 DoF）里所有 VolumeParameter 字段的 overrideState
    /// </summary>
    private static void ForceOverrideAllVolumeParameters(VolumeComponent comp)
    {
        if (comp == null) return;
        var fields = comp.GetType().GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            var f = fields[i];
            // VolumeParameter<T> 的命名里通常包含 "VolumeParameter"
            if (!f.FieldType.Name.Contains("VolumeParameter")) continue;

            var param = f.GetValue(comp);
            if (param == null) continue;

            // 统一打开 overrideState
            var overrideProp = param.GetType().GetProperty("overrideState");
            if (overrideProp != null && overrideProp.PropertyType == typeof(bool))
                overrideProp.SetValue(param, true);
            else
            {
                var overrideField = param.GetType().GetField("overrideState");
                if (overrideField != null && overrideField.FieldType == typeof(bool))
                    overrideField.SetValue(param, true);
            }
        }
    }

    // ========== 对外接口（保持你的调用习惯） ==========

    /// <summary>
    /// 开始从模糊过渡到清晰（动画 blurVolume.weight 从 1 到 0）
    /// </summary>
    public void StartAdaptation(float speed = -1f)
    {
        if (speed > 0f) adaptationSpeed = speed;

        if (blurVolume == null) CreateVolumes();
        blurVolume.weight = 1f; // 确保从模糊开始

        adapting = true;
        enabled = true;
    }

    /// <summary>
    /// 立刻回到模糊状态
    /// </summary>
    public void ResetToBlur()
    {
        if (blurVolume == null) CreateVolumes();
        blurVolume.weight = 1f;
        adapting = false;
        enabled = false;
    }

    /// <summary>
    /// 立刻设置为清晰
    /// </summary>
    public void SetToClear()
    {
        if (blurVolume == null) CreateVolumes();
        blurVolume.weight = 0f;
        adapting = false;
        enabled = false;
    }
}