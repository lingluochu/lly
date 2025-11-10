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
        steps.Add(Step2());


    }

    public IEnumerator Step1()
    {

        UIManager.instance.ShowUI<UI_StartGame>();
        yield return EyeAdaptation(10f);
        yield return null;
    }

    public IEnumerator Step2()
    {
        yield return null;
        yield return EyeAdaptation(10f);


    }
    public IEnumerator Step3()
    {
        yield return null;
        yield return EyeAdaptation(10f);
    }
}

    

