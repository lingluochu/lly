using System.Collections;
using UnityEngine;


public class Step_Text3 : StepBase
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
        yield return null;
        UIManager.instance.FreshUI<UI_Mission>(1);
        yield return EyeAdaptation(10f);

        
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

    

