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
    public static bool isOver = false;
    public UI_StartGame()
    {
        prefabsPath = "UIPrefab/开始实训";

    }

    public override void FindComponents()
    {
        base.FindComponents();
        StarBG = GetObject("开始按钮");
        StartGameBtn = GetOrAddComponent<Button>("开始游戏");
    }

    public override void Init(params object[] args)
    {
        base.Init(args);
        StartGameBtn.onClick.AddListener(() =>
        {
            StarBG.SetActive(false);
            isOver = true;
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
