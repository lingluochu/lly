using cakeslice;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
public class ModelControl : MonoBehaviour//用于控制模型的交互逻辑
{

    /// <summary>
    /// model是否处于可触发状态
    /// </summary>
    public bool isTrigger = false;

    /// <summary>
    /// 回调函数,触发后通知外部
    /// </summary>
    private UnityAction<GameObject> m_Callback;
     // 新增XR交互组件
    private XRGrabInteractable xrInteractable;
     void Awake()
    {
        // 初始化XR交互组件
        xrInteractable = gameObject.AddComponent<XRGrabInteractable>();
        xrInteractable.selectEntered.AddListener(OnXRSelect); // 绑定选择事件
    }
    /// <summary>
    /// 进入可触发的状态
    /// </summary>
    /// <param name="callback">触发的回调</param>
    public void EnterInteractableState(UnityAction<GameObject> callback)
    {
        OutLineRender(transform, true);//显示轮廓

        gameObject.GetComponent<Collider>().enabled = true;//启用碰撞体

        isTrigger = true;//标记为可触发状态

        m_Callback = callback;

         xrInteractable.enabled = true;
    }
    /// <summary>
    /// 不可触发的状态
    /// </summary>
    public void EnterUnInteractableState()
    {
        OutLineRender(transform, false);

        gameObject.GetComponent<Collider>().enabled = false;

        isTrigger = false;

        m_Callback = null;

        xrInteractable.enabled = false;
    }

     private void OnXRSelect(SelectEnterEventArgs args)
    {
        if (!isTrigger) return;
        m_Callback?.Invoke(gameObject);
    }
    
    /// <summary>
    /// 控制轮廓
    /// </summary>
    /// <param name="outlineTransform">控制对象对象</param>
    /// <param name="isShow">是否显示轮廓</param>
    public void OutLineRender(Transform outlineTransform, bool isShow)
    {
        if (isShow)
        {
            // 为所有 MeshRenderer 子对象添加轮廓
            foreach (MeshRenderer item in outlineTransform.GetComponentsInChildren<MeshRenderer>())
            {
                if (item.GetComponent<Outline>() == null)
                {
                    item.transform.gameObject.AddComponent<Outline>();
                }
            }
            // 为所有 SkinnedMeshRenderer 子对象添加轮廓，用于对象为人体时
            foreach (SkinnedMeshRenderer item in outlineTransform.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (item.GetComponent<Outline>() == null)
                {
                    item.transform.gameObject.AddComponent<Outline>();
                }
            }
            //设置所有轮廓的是否渲染
            foreach (Outline outline in outlineTransform.GetComponentsInChildren<Outline>())
            {
                outline.eraseRenderer = !isShow;//是否擦除渲染器与是否显示相反
            }
        }
        else
        {
            foreach (Outline outline in outlineTransform.GetComponentsInChildren<Outline>())
            {
                Destroy(outline);
            }
        }
    }
}
