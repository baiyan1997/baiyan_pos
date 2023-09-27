using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_3000_UI : ActivityUI
{
    private Text _txtTime;
    private Button _btnChangeState;
    private Text _txtActDes;
    private Text _txtActRuleDes;
    private Text _txtCondition;
    private ActInfo_3000 _actInfo;
    public override void OnCreate()
    {
        _txtTime = transform.FindText("Text_Time");
        _txtActDes = transform.Find<Text>("ActDesText");
        _txtActRuleDes = transform.Find<Text>("Scroll View/Viewport/DescText");
        _txtCondition = transform.Find<Text>("ConditionText");
        _btnChangeState = transform.FindButton("Btn");
        _actInfo = (ActInfo_3000)ActivityManager.Instance.GetActivityInfo(ActivityID.ServerChange);
        _btnChangeState.onClick.AddListener(On_btnChangeStateClick);
        _txtCondition.text = Lang.Get("传送说明");
        _txtActDes.text = Lang.Get(Cfg.Act.GetData(ActivityID.ServerChange).act_desc); ;
        _txtActRuleDes.text = Cfg.Help.GetDesc((int)HelpType.CrossState);
    }
    private void On_btnChangeStateClick()
    {
        DialogManager.ShowAsyn<_D_TS_ChangeServerState>(On_btnChangeStateDialogShowAsynCB);
    }
    private void On_btnChangeStateDialogShowAsynCB(_D_TS_ChangeServerState d)
    {
        d?.OnShow();
    }
    public override void UpdateTime(long serverTime)
    {
        base.UpdateTime(serverTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        _txtTime.text = string.Format(Lang.Get("活动倒计时 {0}"), WorldUtils.CountTime_DHMS((int)_actInfo.LeftTime));
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        if (!_actInfo._isShowRedPoint)
        {
            PlayerPrefs.SetString(_actInfo._data.endts.ToString(), "1");
            EventCenter.Instance.RemindActivity.Broadcast(ActivityID.ServerChange, _actInfo.IsAvaliable());
        }
        UpdateTime(TimeManager.ServerTimestamp);
    }

}

