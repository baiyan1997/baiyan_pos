using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class _Activity_2032_UI : ActivityUI
{
    private Text TimeText;
    private ListView _list;
    private Text Desc;
    private ActInfo_2032 _actInfo;
    private int _aid = 2032;
    public override void OnCreate()
    {
        TimeText = transform.Find<Text>("TimeText");
        _list = ListView.Create<_Act2032Item>(transform.Find("Scroll View"));
        Desc = transform.Find<Text>("Desc");
        InitData();
        //InitListener();
        Desc.text = _actInfo._desc;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    private void InitData()
    {
        _actInfo = (ActInfo_2032)ActivityManager.Instance.GetActivityInfo(_aid);
    }
    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
        ////同步服务器时间
        //TimeManager.Instance.TimePassSecond += UpdataTime;
    }
    public override void OnShow()
    {
        _list.ScrollRect.verticalNormalizedPosition = 1;
        UpdateUi(_aid);
    }

    public void RefreshInfo()
    {
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid);
    }
    private void UpdateUi(int aid)
    {
        if (aid == _aid)
        {
            _list.Clear();
            for (int i = 0; i < _actInfo._info.cfgData.Count; i++)
            {
                if (_actInfo.isPre)
                {
                    //存在前天未领取
                    SetButtonPre(i, _actInfo._info.previous_state_reward - 1);
                }
                else
                {
                    SetButton(i, _actInfo._info.which_state - 1);
                }
            }
            //添加排序方案：已领取排在最后
            var _sortlists = _actInfo._info.cfgData;
            for (int i = 0; i < _sortlists.Count; i++)
            {
                if (_sortlists[i].canGet != 3)
                    _list.AddItem<_Act2032Item>().Refresh(_sortlists[i], _actInfo.GetReward, _actInfo._info.user_startts, RefreshInfo);
            }
            for (int i = 0; i < _sortlists.Count; i++)
            {
                if (_sortlists[i].canGet == 3)
                    _list.AddItem<_Act2032Item>().Refresh(_sortlists[i], _actInfo.GetReward, _actInfo._info.user_startts, RefreshInfo);
            }
        }
    }
    private void SetButtonPre(int index, int tip)
    {
        if (index < tip)
        {
            _actInfo._info.cfgData[index].canGet = 3;
        }
        else if (index == tip)
        {
            _actInfo._info.cfgData[index].canGet = 2;
        }
        else
        {
            _actInfo._info.cfgData[index].canGet = 0;
        }
    }
    private void SetButton(int index, int tip)
    {
        if (tip > -1)
        {
            if (index < tip)
            {
                _actInfo._info.cfgData[index].canGet = 3;
            }
            else if (index == tip)
            {
                if (_actInfo.isCan)
                {
                    _actInfo._info.cfgData[index].canGet = 2;
                }
                else
                {
                    _actInfo._info.cfgData[index].canGet = 1;
                }
            }
            else
            {
                _actInfo._info.cfgData[index].canGet = 0;
            }
        }
        else
        {
            _actInfo._info.cfgData[index].canGet = 3;
        }
    }
    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (st - _actInfo._data.startts < 0)
        {
            TimeText.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            TimeText.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            TimeText.text = Lang.Get("活动已经结束");
        }
    }
}
public class _Act2032Item : ListItem
{
    //领取按钮
    private Button _getBtn;
    //未达成
    private GameObject haventBtn;
    //已领取
    private GameObject gotBtn;
    private GameObject _get;
    private ListView _rewardList;
    private GameObject _busyBg;
    private GameObject _freeBg;
    private Action<int, Action> _getRewardCd;
    private Text title;
    private Action _refresh;
    private P_Act2032Cfg _cfg;

    private int _id;
    private int isCan;
    private int _time;
    private long _startts;
    private string _timeNow;
    public override void OnCreate()
    {
        _getBtn = transform.Find<Button>("GetButton");
        _get = _getBtn.gameObject;
        haventBtn = transform.Find("HaventBtn").gameObject;
        gotBtn = transform.Find("GotBtn").gameObject;
        _rewardList = ListView.Create<_ActRewardItem>(transform.Find("ScrollView"));
        title = transform.Find<Text>("Title");
        _busyBg = transform.Find("BusyBg").gameObject;
        _freeBg = transform.Find("FreeBg").gameObject;
        _getBtn.onClick.AddListener(On_getBtnClick);
    }
    private void On_getBtnClick()
    {
        Uinfo.Instance.Bag.CheckBlueDrawAlert(_cfg.reward, OnBagCheckBlueDrawAlertCB);
    }
    private void OnBagCheckBlueDrawAlertCB()
    {
        if (_getRewardCd != null)
        {
            _getRewardCd(_id, OnBagGetRewardCB);
            _getRewardCd = null;
        }
        if (_refresh != null)
        {
            _refresh();
            _refresh = null;
        }
    }
    private void OnBagGetRewardCB()
    {
        isCan = 3;
        ResetBtnState();
        gotBtn.SetActive(true);
        _freeBg.SetActive(true);
    }




    public void Refresh(P_Act2032Cfg cfg, Action<int, Action> ac, long startts, Action refresh)
    {
        _cfg = cfg;
        _getRewardCd = ac;
        _refresh = refresh;
        _id = cfg.id;
        isCan = cfg.canGet;
        _time = cfg.time;
        _startts = startts;
        _rewardList.Clear();
        for (int i = 0; i < cfg.rewards.Length; i++)
        {
            _rewardList.AddItem<_ActRewardItem>().Refresh(cfg.rewards[i]);
        }
        _rewardList.ScrollRect.horizontalNormalizedPosition = 0;
        _rewardList.ScrollRect.enabled = cfg.rewards.Length >= 4;
        if (isCan != 1)
        {
            title.text = string.Format(Lang.Get("在线时间达到{0}分钟"), _time / 60);
        }
        ResetBtnState();
        RefreshButton();
    }

    private void RefreshButton()
    {
        switch (isCan)
        {
            case 0://未完成;
                _freeBg.SetActive(true);
                haventBtn.SetActive(true);
                break;
            case 1://领取计时
                _freeBg.SetActive(true);
                haventBtn.SetActive(true);
                break;
            case 2://领取
                _busyBg.SetActive(true);
                _get.SetActive(true);
                break;
            case 3://已领取
                _freeBg.SetActive(true);
                gotBtn.SetActive(true);
                break;
        }
    }

    private void RefreshTime(long st)
    {
        if (title && _startts != -1 && isCan == 1)
        {
            var time = _startts + _time - st;
            if (time > 0) {
                //UI时间显示
                _timeNow = string.Format("{0}:{1}", time / 60, time % 60);
                title.text = string.Format(Lang.Get("在线时间达到{0}分钟(<Color=#00ff00ff>{1}后领取奖励</Color>)"), _time / 60, _timeNow);
            }else {
                title.text = string.Format(Lang.Get("在线时间达到{0}分钟"), _time / 60);
            }
        }
    }
    private void ResetBtnState()
    {
        _get.SetActive(false);
        haventBtn.SetActive(false);
        gotBtn.SetActive(false);
        _busyBg.SetActive(false);
        _freeBg.SetActive(false);
    }

    public override void OnAddToList()
    {
        base.OnAddToList();
        TimeManager.Instance.TimePassSecond += RefreshTime;
    }

    public override void OnRemoveFromList()
    {
        base.OnRemoveFromList();
        TimeManager.Instance.TimePassSecond -= RefreshTime;
    }
}

