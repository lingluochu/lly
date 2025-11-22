using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Text : UIBase
{
    public Text _tip;
    public UI_Text()
    {
        prefabsPath = "UIPrefab/文本提示";
    }
    public override void FindComponents()
    {
        base.FindComponents();
        uiGameObject.SetActive(true);
        _tip = GetOrAddComponent<Text>("文本输入");
    }

    public override void Init(params object[] args)
    {
        base.Init(args);

    }

    public override void Show()
    {
        uiGameObject.SetActive(true);
        GetOrAddComponent<CanvasGroup>(uiGameObject.name).alpha = 0;
        GetOrAddComponent<CanvasGroup>(uiGameObject.name).DOFade(1, 1);
    }
    public override void Close()
    {
        uiGameObject.SetActive(false);
    }

    public override void Fresh(params object[] args)
    {
        //_tip = GetOrAddComponent<Text>("文本输入");
        _tip.text = args[0].ToString();
    }
}
