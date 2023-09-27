using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2101_UI : ActivityUI
{
    private Text _des;
    private Text _number;
    private Image _imgCode;
    private ActInfo_2101 _actInfo;
    private int _aid = 2101;
    private Image _btnImages;

    public override void OnCreate()
    {
        _des = transform.Find<Text>("Text_desc");
        _number = transform.Find<Text>("Text_number");
        _imgCode = transform.Find<Image>("Image_code");
        InitData();
        Init();
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2101)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    private void Init()
    {
        var cfgData = Cfg.VipAmbassador.GetData(2101);
        _des.text = cfgData.text;
        _number.text = cfgData.number.ToString();
        UIHelper.SetImageSprite(_imgCode, cfgData.code);
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        UpdateUi(_aid);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid);
    }

    private void UpdateUi(int aid)
    {
        
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _btnImages = null;
        _actInfo = null;

    }
}
