using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class _Activity_2030_UI : ActivityUI
{
    private Text _des;
    private ListView _rewardList;
    private ActInfo_2030 _actInfo;
    private int _aid = 2030;
    public override void OnCreate()
    {
        _des = transform.Find<Text>("des_text");
        _rewardList = ListView.Create<_Act2030Item>(transform.Find("Scroll View"));
        InitData();
        Init();
        //InitListener();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    private void InitData()
    {
        _actInfo = (ActInfo_2030)ActivityManager.Instance.GetActivityInfo(_aid);
    }
    private void Init()
    {
        _des.text = string.Format(Lang.Get("活动开启期间，每日登录游戏即可领取奖励") + "\n" + GlobalUtils.ActTimeFormat(_actInfo._data.startts, _actInfo._data.endts));
    }

    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
    }

    public override void OnShow()
    {
        _actInfo.RefreshAct();
        UpdateUi(_aid);
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
            _rewardList.Clear();
            for (int i = 0; i < _actInfo._info.rewardData.Count; i++)
            {
                _rewardList.AddItem<_Act2030Item>()
                    .Refresh(_actInfo._info.rewardData[i], _actInfo.GetRewardById, _actInfo.status, i);
            }
        }
    }

}

public class _Act2030Item : ListItem
{
    private int _id;
    private Text _textTime;
    private ListView _list;
    private Button _getBtn;
    private GameObject _notReach;
    private GameObject _claimed;
    private GameObject _get;
    private Action<int, Action> _getRewardCd;
    private Dictionary<int, int> _status;//0未达成 1未领奖 2已领奖
    private GameObject _busyBg;
    private GameObject _freeBg;

    public override void OnCreate()
    {
        _textTime = transform.Find<Text>("dateTime");
        _busyBg = transform.Find("BusyBg").gameObject;
        _freeBg = transform.Find("FreeBg").gameObject;
        _list = ListView.Create<_ActRewardItem>(transform.Find("ScrollView"));
        _getBtn = transform.FindButton("Btn_get");
        _get = _getBtn.gameObject;
        _notReach = transform.Find("Btn_notReach").gameObject;
        _claimed = transform.Find("Btn_claimed").gameObject;

        _getBtn.onClick.AddListener(On_getBtnClick);
    }
    private void On_getBtnClick()
    {
        if (_getRewardCd != null)
        {
            _getRewardCd(_id, OnGetBtnRewardCB);
            _getRewardCd = null;
        }
    }
    private void OnGetBtnRewardCB()
    {
        _status[_id] = 2;
        ResetBtnState();
        _freeBg.SetActive(true);
        _claimed.SetActive(true);
    }

    public void Refresh(P_Act2030RewardData rewards, Action<int, Action> ac, Dictionary<int, int> status, int day)
    {
        _textTime.text = string.Format(Lang.Get("第{0}天"), day + 1);
        _id = rewards.id;
        _getRewardCd = ac;
        _status = status;
        _list.Clear();
        for (int i = 0; i < rewards.rewards.Length; i++)
        {
            _list.AddItem<_ActRewardItem>().Refresh(rewards.rewards[i]);
        }
        _list.ScrollRect.horizontalNormalizedPosition = 0;
        _list.ScrollRect.enabled = rewards.rewards.Length >= 4;//大于等于4个可以滑动
        int type;
        status.TryGetValue(_id, out type);
        ResetBtnState();
        switch (type)//0未达成 1未领奖 2已领奖
        {
            case 0:
                _freeBg.SetActive(true);
                _notReach.SetActive(true);
                break;
            case 1:
                _busyBg.SetActive(true);
                _get.SetActive(true);
                break;
            case 2:
                _freeBg.SetActive(true);
                _claimed.SetActive(true);
                break;
            default:
                throw new Exception("can't find reward type " + type);
        }
    }

    private void ResetBtnState()
    {
        _notReach.SetActive(false);
        _get.SetActive(false);
        _claimed.SetActive(false);
        _busyBg.SetActive(false);
        _freeBg.SetActive(false);
    }
}


