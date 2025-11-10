using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class UI_Mission : UIBase
{
    public Button FoldBtn;

    public Slider Slider;

    public List<GameObject> MissionObj = new List<GameObject>();

    //private RectTransform rectTransform;

    //private bool isFolded;

    public UI_Mission()
    {
        prefabsPath = "UIPrefab/UI_Mission";

    }

    public override void FindComponents()
    {
        base.FindComponents();
        FoldBtn = GetOrAddComponent<Button>("�۵�");
        //rectTransform = UIGameObject.GetComponent<RectTransform>();

        Slider = GetOrAddComponent<Slider>("������");

        GameObject Content = GetObject("Content");
        if (MissionObj.Count <= 0)
        {
            for (int i = 0; i < Content.transform.childCount; i++)
            {
                MissionObj.Add(Content.transform.GetChild(i).gameObject);
            }
        }

        //FoldBtn.onClick.AddListener(ToggleFold);
    }

    public override void Show()
    {
        uiGameObject.SetActive(true);
        //isFolded = true;
    }

    public override void Close()
    {
        uiGameObject.SetActive(false);
    }

    public override void Fresh(params object[] args)
    {
        base.Fresh(args);
        AdjustSlider((int)args[0]);
        MissionHightLight((int)args[0]);
    }

    /// <summary>
    /// ���ڽ����� slider
    /// </summary>
    /// <param name="value"></param>
    public void AdjustSlider(int value)
    {
        Slider.DOValue(Mathf.Clamp01(value / (float)MissionObj.Count), 0.5f);
    }

    /// <summary>
    /// ������ʾ����
    /// </summary>
    /// <param name="value"></param>
    public void MissionHightLight(int value)
    {
        for (int i = 0; i < MissionObj.Count; i++)
        {
            if (value == i + 1)
            {
                //MissionObj[i].transform.GetComponentInChildren<Image>().color = Color.yellow;
                MissionObj[i].transform.GetComponentInChildren<Text>().color = new Color(244f / 255f, 120f / 255f, 57f / 255f, 1);
            }
            else
            {
                //MissionObj[i].transform.GetComponentInChildren<Image>().color = Color.clear;
                MissionObj[i].transform.GetComponentInChildren<Text>().color = Color.black;
            }
        }
    }

}
