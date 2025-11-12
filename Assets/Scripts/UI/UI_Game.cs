using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Game : UIBase
{
    public GameObject StarBG;

    public GameObject ChooseModePanel;

    public Button OneBtn,TwoBtn;

    public Text _tip;
    public UI_Game()
    {
        prefabsPath = "UIPrefab/学习模式";
    }

    public override void FindComponents()
    {
        base.FindComponents();
        StarBG = GetObject("学习模式");
        OneBtn = GetOrAddComponent<Button>("自主学习");
        TwoBtn = GetOrAddComponent<Button>("按顺序学习");
    }

    public override void Init(params object[] args)
    {
        base.Init(args);

        string SceneName = args[0].ToString();

        OneBtn.onClick.AddListener(() =>
        {
            StarBG.SetActive(false);
        });
        TwoBtn.onClick.AddListener(() =>
        {
            StarBG.SetActive(false);
        });

    }

    public override void Show()
    {
        //base.Show();
    }
    public override void Close()
    {
        //base.Close();

    }

    public override void Fresh(params object[] args)
    {
        base.Fresh(args);
    }

}
