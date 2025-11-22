using System.Collections;
using UnityEngine;


public class Step_Text0 : StepBase
{

    public override void Start()
    {
        base.Start();
    }
    public override void Update()
    {
        base.Update();

    }

    public override void Init()
    {
        base.Init();
    }
    public override void AddStep()
    {
        base.AddStep();
        steps.Add(Step1());

    }

    public IEnumerator Step1()
    {

       

        yield return null;
        yield return SetOldMan("客厅站立1", "Stand_Idle");

        UIManager.instance.ShowUI<UI_Text>();
        UIManager.instance.FreshUI<UI_Text>(string.Format("提示：与语音输入一致即可"));
        UIManager.instance.SetTransform<UI_Text>("文本框");

        UIManager.instance.ShowUI<UI_StartGame>();
        UIManager.instance.InitUI<UI_StartGame>();
        UIManager.instance.SetTransform<UI_StartGame>("客厅开始界面");
        while (UI_Game.isEnterFreeMode == false && UI_Game.isEnterOrderMode == false)
        {
            yield return null;
        }
        //yield return SetOldMan("客厅坐沙发", "Sit_Idle");
        yield return SetOldMan("卧室躺床上", "Lay_Idle");
    }
}



