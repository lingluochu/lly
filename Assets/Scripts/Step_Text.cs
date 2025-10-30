using System.Collections;
using UnityEngine;


public class Step_Text : StepBase
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
        yield return EyeAdaptation(10f);

        
    }
}

    

