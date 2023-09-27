using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2057_UI : ActivityUI
{
    public override void OnShow()
    {
        _actInfo = (ActInfo_2057)ActivityManager.Instance.GetActivityInfo(2057);
        _txtDesc.text = _actInfo._desc;
        UpdateTime(TimeManager.ServerTimestamp);
        InitPlanetObj();
    }

    public override void OnClose()
    {
        base.OnClose();
        CleanPlanetObj();
    }

    private List<_WorldUnit> _planetObjList = new List<_WorldUnit>();

    private void InitPlanetObj()
    {
        P_WorldUnitInfo _planetInfo = new P_WorldUnitInfo();


        _planetInfo.land_type = 1001;
        _planetInfo.land_lv = 9;
        AddPlanetObj("ImageBg/_planetPos0", _planetInfo);
   

        _planetInfo.land_type = 247;
        _planetInfo.land_lv = 3;
        AddPlanetObj("ImageBg/_planetPos1", _planetInfo);


        _planetInfo.land_type = 226;
        _planetInfo.land_lv = 7;
        AddPlanetObj("ImageBg/_planetPos2", _planetInfo);


        _planetInfo.land_type = 238;
        _planetInfo.land_lv = 5;
        AddPlanetObj("ImageBg/_planetPos3", _planetInfo);
    }

    private async void AddPlanetObj(string root,P_WorldUnitInfo planetInfo )
    {
        _WorldUnit _planetUnit = await _WorldUnit.Get(planetInfo);
        _planetUnit.transform.SetParent(transform.Find(root), false);
        _planetUnit.SetPlanetInUI();
        _planetObjList.Add(_planetUnit);
        _planetUnit.HideUI();
    }

    private void CleanPlanetObj()
    {
        _planetObjList.ForEach((unit) => unit.SetPlanetInUI_Store());
        _planetObjList.Clear();
    }

    private Text _txtTime;
    private Text _txtDesc;
    private ActInfo_2057 _actInfo;

    public override void OnCreate()
    {

        _txtTime = transform.FindText("Text_time");
        _txtDesc = transform.FindText("Text_desc");
        //TimeManager.Instance.TimePassSecond += UpdateTime;
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void UpdateTime(long servserTime)
    {
        base.UpdateTime(servserTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_actInfo != null && transform.gameObject.activeSelf)
        {
            _txtTime.text = WorldUtils.CountTime_DHMS((int)_actInfo.LeftTime);
        }
    }
}
