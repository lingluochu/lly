using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_StartGame : UIBase
{
    public GameObject StarBG;

    public GameObject ChooseModePanel;

    public Button StartGameBtn;

    public Text _tip;
    public UI_StartGame()
    {
        prefabsPath = "UIPrefab/开始实训";

        mLayer = UILayer.BasicLayer;
    }

    public override void FindComponents()
    {
        base.FindComponents();
        StarBG = GetObject("开始实训");
        StartGameBtn = GetOrAddComponent<Button>("开始游戏");
    }

    public override void Init(params object[] args)
    {
        base.Init(args);

        string SceneName = args[0].ToString();

        StartGameBtn.onClick.AddListener(() =>
        {
            StarBG.SetActive(false);
            UIManager.instance.ShowUI<UI_Game>();
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
