using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2083_UI : ActivityUI
{
    #region params
    private Text _textCountDown;
    private Text _textSailingProgress;
    private Slider _sliderShieldValue;
    private Text _textShieldValue;
    private Button _btnRecoverShieldValue;
    private Button _btnSearch;
    private Text _textRemainSearchTimes;
    private Button _btnCargoHold;
    private Button _btnCommandRoom;
    private Button _btnManual;
    private GameObject _redPoint;
    private int _aid = ActivityID.StarTrek;
    private ActInfo_2083 _actInfo;
    private int _eid;
    private List<Dialog> _childDialogs;
    #endregion





    public override void OnCreate()
    {
        InitRef();
        InitButtonClick();
        //InitListener();
    }

    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += RefreshTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(RefreshUi);
    }

    private void InitButtonClick()
    {
        _btnManual.onClick.AddListener(On_btnManualClick);
        _btnCommandRoom.onClick.AddListener(On_btnCommandRoomClick);
        _btnCargoHold.onClick.AddListener(On_btnCargoHoldClick);
        _btnSearch.onClick.AddListener(OnSearch);
        _btnRecoverShieldValue.onClick.AddListener(On_btnRecoverShieldValueClick);
    }
    private void On_btnManualClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnManualDialogShowAsynClick);
    }
    private void On_btnManualDialogShowAsynClick(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2083, _btnManual.transform.position, Direction.LeftDown, 350);
        AddToChildDialogs(d);
    }
    private void On_btnCommandRoomClick()
    {
        DialogManager.ShowAsyn<_D_Act2083CommandRoom>(On_btnCommandRoomDialogShowAsyn);
    }
    private void On_btnCommandRoomDialogShowAsyn(_D_Act2083CommandRoom d)
    {
        d?.OnShow(_actInfo);
        AddToChildDialogs(d);
    }
    private void On_btnCargoHoldClick()
    {
        DialogManager.ShowAsyn<_D_Act2083CargoHold>(On_btnCargoHoldDialogShowAsynCB);
    }
    private void On_btnCargoHoldDialogShowAsynCB(_D_Act2083CargoHold d)
    {
        d?.OnShow(_actInfo);
        AddToChildDialogs(d);
    }
    private void On_btnRecoverShieldValueClick()
    {
        DialogManager.ShowAsyn<_D_Act2083CommandRoom>(On_btnRecoverShieldValueDialogShowAsynCB);
    }
    private void On_btnRecoverShieldValueDialogShowAsynCB(_D_Act2083CommandRoom d)
    {
        d?.OnShow(_actInfo);
        AddToChildDialogs(d);
    }
    private void AddToChildDialogs(Dialog dia)
    {
        if (dia == null)
        {
            return;
        }
        if (!_childDialogs.Contains(dia))
        {
            _childDialogs.Add(dia);
        }
    }
    private void OnSearch()
    {
        if (_actInfo.UniqueInfo.explore_times == Constans.TotalSearchTimesPerDay)
        {
            MessageManager.Show(Lang.Get("今日探索次数已经用完,明日再来吧!"));
            return;
        }
        Rpc.SendWithTouchBlocking<int>("startExplore", null, ShowSearchTip);
    }

    private void ShowSearchTip(int eid)
    {
        _eid = eid;
        _actInfo.RefreshEid(eid);
        string title = Cfg.Activity2083.GetTitle(eid);
        string desc = Cfg.Activity2083.GetDesc(eid);
        bool hasCancel = eid == 2 || eid == 3 || eid == 5;
        DialogManager.ShowAsyn<_D_Act2083SearchEvents>(d =>
        {
            d?.OnShow(title, desc, hasCancel);
            AddToChildDialogs(d);
            SetButtonPattern(d, eid);
            SetButtonCallBack(d, eid, hasCancel, title);
        });
    }

    private void SetButtonCallBack(_D_Act2083SearchEvents alert, int eid, bool hasCancel, string title)
    {
        switch (eid)
        {
            case 1://遇到星际风暴
                HandleStarStormEvent(alert, eid);
                break;
            case 2:
            case 3:
            case 5://战斗或者撤退
                HandleBattleOrRetreat(alert, eid, title);
                break;
            case 4://获得额外奖励的探索事件
                HandleProbeEvent(alert, eid);
                break;
            case 6://遇到虫洞
                HandleWormhole(alert, eid);
                break;
            default: throw new Exception("eid is not exist,please check");
        }

    }

    private void HandleWormhole(_D_Act2083SearchEvents alert, int eid)
    {
        alert.SetYesCallback(() =>
        {
            Rpc.SendWithTouchBlocking<P_Act2083UniqueInfo>("findWormhole", Json.ToJsonString(eid), data =>
            {
                _actInfo.RefreshInfo(data);
                //RefreshUi(_aid);
                MessageManager.Show(Lang.Get("航行进度+{0}%", Cfg.Activity2083.GetAddedProgress(eid)));
                alert.Close();
            });
        });
    }

    private void HandleProbeEvent(_D_Act2083SearchEvents alert, int eid)
    {
        alert.SetYesCallback(() =>
        {
            Rpc.SendWithTouchBlocking<P_Act2083UniqueInfo>("probeFromEvent", Json.ToJsonString(eid), data =>
            {
                _actInfo.RefreshInfo(data);
                //RefreshUi(_aid);
                alert.Close();
                DialogManager.ShowAsyn<_D_Act2083SearchPrizes>(d =>
                {
                    d?.OnShow(_actInfo.UniqueInfo);
                    AddToChildDialogs(d);
                });
            });
        });
    }

    private void HandleBattleOrRetreat(_D_Act2083SearchEvents alert, int eid, string title)
    {
        //战斗
        alert.SetYesCallback(
        () =>
        {
            DialogManager.ShowAsyn<_D_ArenaTeamDispose>(d =>
            {
                d?.OnShow(title, string.Empty, SelectTeamsCallBack, 1, isStarLost: true);
                AddToChildDialogs(d);
            });
            alert.Close();
        });
        //撤退
        alert.SetCancelCallback(() =>
        {
            Rpc.SendWithTouchBlocking<P_Act2083UniqueInfo>("evacuateFromEvent", Json.ToJsonString(eid), data =>
            {
                _actInfo.RefreshInfo(data);
                //RefreshUi(_aid);
                MessageManager.Show(Lang.Get("航行进度+{0}%", Cfg.Activity2083.GetAddedProgress(eid)));
                alert.Close();
            });

        });
    }
    private void HandleStarStormEvent(_D_Act2083SearchEvents alert, int eid)
    {
        alert.SetYesCallback(() =>
        {
            Rpc.SendWithTouchBlocking<P_Act2083UniqueInfo>("starStorm", Json.ToJsonString(eid), data =>
            {
                _actInfo.RefreshInfo(data);
                //RefreshUi(_aid);
                MessageManager.Show(Lang.Get("护盾值-{0}", Cfg.Activity2083.GetSubtractedShieldValue(eid)));
                MessageManager.Show(Lang.Get("航行进度+{0}%", Cfg.Activity2083.GetAddedProgress(eid)));
                alert.Close();
            });
        });
    }
    private void SelectTeamsCallBack(List<int> teams)
    {
        string ids = GlobalUtils.ToStringFormat(teams);
        Rpc.SendWithTouchBlocking<P_Act2083UniqueInfo>("atkFromEvent", Json.ToJsonString(_eid, ids), On_atkFromEvent_SC);
    }

    private void On_atkFromEvent_SC(P_Act2083UniqueInfo data)
    {
        _actInfo.RefreshInfo(data);
        _Battle.Instance.Show(data.battle_report, null);
    }


    private void SetButtonPattern(_D_Act2083SearchEvents alert, int eid)
    {
        if (alert == null)
        {
            return;
        }

        switch (eid)
        {
            case 1:
            case 6:
                alert.SetYesBtnText(Lang.Get("继续航行"));
                break;
            case 3:
            case 5:
                alert.SetYesBtnText(Lang.Get("掠夺"));
                alert.SetCancelBtnText(Lang.Get("撤离"));
                break;
            case 2:
                alert.SetYesBtnText(Lang.Get("战斗"));
                alert.SetCancelBtnText(Lang.Get("撤离"));
                break;
            case 4:
                alert.SetYesBtnText(Lang.Get("探查"));
                break;
            default: throw new Exception($"eid is wrong please check eid {eid}");
        }
    }

    private void InitRef()
    {
        _textCountDown = transform.FindText("TextCountDown");
        _textSailingProgress = transform.FindText("Img_SailingProgress/Text");
        _sliderShieldValue = transform.Find("Slider_ShieldValue").GetComponent<Slider>();
        _textShieldValue = transform.FindText("Text_ShieldValue");
        _btnRecoverShieldValue = transform.FindButton("Btn_RecoverShieldValue");
        _btnSearch = transform.FindButton("Btn_Search");
        _textRemainSearchTimes = transform.FindText("Text_RemainSearchTimes");
        _btnCargoHold = transform.FindButton("Btn_CargoHold");
        _btnCommandRoom = transform.FindButton("Btn_CommandRoom");
        _btnManual = transform.FindButton("_btnManual");
        _redPoint = _btnCargoHold.transform.Find("RedPoint").gameObject;
        _redPoint.SetActive(false);
        _childDialogs = new List<Dialog>();

    }

    public override void OnShow()
    {
        _ShipDisplayControl.Instance.ShowShip(40106001, _ShipDisplayControl.DisplayMode.AutoRotateOnly);
        _actInfo = ActivityManager.Instance.GetActivityInfo(_aid) as ActInfo_2083;
        UpdateTime(TimeManager.ServerTimestamp);
        RefreshUi(_aid);

    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        RefreshUi(aid);
    }

    private void RefreshUi(int aid)
    {
        if (_aid != aid)
        {
            return;
        }
        //刷新航行进度
        RefreshSailingProgress();
        //刷新防护值
        RefreshShieldValue();
        //刷新探索剩余次数
        _textRemainSearchTimes.text = Lang.Get("剩余次数:{0}/10", Constans.TotalSearchTimesPerDay - _actInfo.UniqueInfo.explore_times);

        if (_actInfo.EndAct)
        {
            StopActivity();
        }

    }

    private void StopActivity()
    {
        _redPoint.SetActive(!_actInfo.IsGottenRewards && _actInfo.CanGetRewards);
        _btnSearch.gameObject.SetActive(false);
        _textRemainSearchTimes.gameObject.SetActive(false);
    }

    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (nowTs - _actInfo._data.startts < 0)
        {
            _textCountDown.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            _textCountDown.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _textCountDown.text = Lang.Get("活动已经结束");
        }
    }

    private void RefreshSailingProgress()
    {
        //todo others

        _textSailingProgress.text = Lang.Get("{0}%", _actInfo.UniqueInfo.progress / 100);
    }

    private void RefreshShieldValue()
    {
        _sliderShieldValue.value = _actInfo.UniqueInfo.shield_value / 100f;
        _textShieldValue.text = Lang.Get("防护值:{0}/100", _actInfo.UniqueInfo.shield_value);
    }
    public override void OnDestroy()
    {
        base.OnClose();
        base.OnDestroy();
        //TimeManager.Instance.TimePassSecond -= RefreshTime;
        //EventCenter.Instance.UpdateActivityUI.RemoveListener(RefreshUi);
    }

    public override void OnClose()
    {
        base.OnClose();
        CloseAllChildDialog();
    }

    private void CloseAllChildDialog()
    {
        int count = _childDialogs.Count;
        if (count == 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var dia = _childDialogs[i];
            if (dia.IsShowing)
            {
                dia.Close();
            }
        }
    }
}
