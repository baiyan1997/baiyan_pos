using System;
using UnityEngine;
using UnityEngine.UI;
using static ActivityInfo;

public class _Activity_2034_UI : ActivityUI
{
    private int _aid = 2034;
    private ActInfo_2034 _actInfo;

    private Button _claim;

    private Text _btnText;
    private Text _time;
    private RectTransform _content;
    private GameObject _item;
    private GameObject _notFinish;

    private ListView _list;

    private int rewardShipId;
    private void InitData()
    {
        _actInfo = (ActInfo_2034)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void Awake()
    {

    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void OnCreate()
    {
        InitData();
        //InitListener();

        _claim = transform.Find<Button>("Btn_claim");
        _time = transform.Find<Text>("Text_time");
        _notFinish = transform.Find("Obj_notFinish").gameObject;
        _btnText = transform.Find<Text>("Btn_claim/Text");
        _content = transform.Find<RectTransform>("List");
        _item = transform.Find("List/01").gameObject;

        _list = ListView.Create<Act2034Item>(_content, _item);

        rewardShipId = Cfg.Activity2034.GetRewardShipId();
        _claim.onClick.AddListener(On__claimClick);
    }
    private void On__claimClick()
    {
        _actInfo.GetReward(OnClaimGetRewardCB);
    }

    private void OnClaimGetRewardCB(P_ActCommonReward data)
    {
        var rewards = data.get_items;
        if (Cfg.Ship.IsPlayerShip(rewards[0].itemid))
        {
            DialogManager.ShowAsyn<_D_ShipDisplay>(d => { d?.OnShow(rewards[0].itemid); });
        }
        else
        {
            DialogManager.ShowAsyn<_D_ShipDisplay>(d => { d?.OnShowRepeat(rewardShipId, rewards[0].count); });
        }
    }
    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(aid =>
        //{
        //    if (aid == _aid)
        //        OnShow();
        //});

        //TimeManager.Instance.TimePassSecond += UpdateTime;
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid);
    }

    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (stamp - _actInfo._data.startts < 0)
        {
            _time.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _time.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _time.text = Lang.Get("活动已经结束");
        }

    }

    private void UpdateUi(int aid)
    {
        if (aid == _aid)
            OnShow();
    }

    private void SetBtnState()
    {
        var data = _actInfo._data;
        if (data.can_get_reward) //完成未领取
        {
            _notFinish.SetActive(false);
            _claim.gameObject.SetActive(true);
            _claim.interactable = true;
            _btnText.text = Lang.Get("领取战舰");
            _btnText.color = Color.white;
        }
        else if (data.get_all_reward) //完成已领取
        {
            _notFinish.SetActive(false);
            _claim.gameObject.SetActive(true);
            _claim.interactable = false;
            _btnText.text = Lang.Get("已领取");
            _btnText.color = new Color(0.8f, 0.8f, 0.8f, 1);
        }
        else //未完成
        {
            _notFinish.SetActive(true);
            _claim.gameObject.SetActive(false);
        }


    }



    public override void OnShow()
    {
        _list.Clear();
        var info = _actInfo._missionInfo;
        for (int i = 0; i < info.Count; i++)
        {
            _list.AddItem<Act2034Item>().Refresh(info[i]);
        }
        UpdateTime(TimeManager.ServerTimestamp);
        SetBtnState();

        _ShipDisplayControl.Instance.ShowShip(rewardShipId, _ShipDisplayControl.DisplayMode.AutoRotateOnly);
    }

    public override void OnClose()
    {
        base.OnClose();
        _ShipDisplayControl.Instance.CloseShipShow();
    }
}
public class Act2034Item : ListItem
{
    private Text _desc;
    private Text _process;
    private GameObject _finish;
    public override void OnCreate()
    {
        _desc = transform.Find<Text>("Text_desc");
        _process = transform.Find<Text>("Text_process");
        _finish = transform.Find("Img_finish").gameObject;
    }
    public void Refresh(P_Act2034Data info)
    {
        int needCount = Cfg.Activity2034.GetNeedCount(info.tid);
        _desc.text = Cfg.Activity2034.GetData(info.tid).mission_desc;

        if (info.finished == 0)//未完成
        {
            _process.gameObject.SetActive(true);
            _finish.SetActive(false);
            _process.text = string.Format("<Color=#00ff33ff>{0}</Color>/{1}", info.do_number, needCount);
        }
        else
        {
            _process.gameObject.SetActive(false);
            _finish.SetActive(true);
        }

    }
}
