using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class _ActivityRank_UI : ActivityUI
{
    private int _aid;
    private ObjectGroup _ui;
    private ActInfo_ActivityRank _actInfo;
    private ListView list;
    private RectTransform _mainView;
    private GameObject _rankReward;
    private _Activity_Rank_Inf _reward;
    private RectTransform _rootRect;
    private GameObject _objSelfRank;
    private RectTransform _listRect;
    private float _moveDistance;
    private void InitData()
    {
        _actInfo = (ActInfo_ActivityRank)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void SetAid(int aid)
    {
        _aid = aid;
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
        _ui = gameObject.GetComponent<ObjectGroup>();
        _rootRect = gameObject.GetComponent<RectTransform>();
        Transform rootView = transform.Find<Transform>("MainView/Scrollview");
        _mainView = transform.Find<RectTransform>("MainView");
        list = ListView.Create<_ActRankItem>(rootView);
        _objSelfRank = transform.Find("MainView/Me").gameObject;
        _moveDistance = _rootRect.rect.height - 5;
        InitData();
        //InitListener();
        InitEvent();
        InitUi();
    }

    private void InitEvent()
    {
        _ui.Get<Button>("Btn_reward").onClick.AddListener(CheckRankReward);
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    private void InitUi()
    {
        UpdateUI(_aid);
    }
    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;

        if (stamp - _actInfo._data.startts < 0)
        {
            _ui.Get<Text>("Text_time").text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _ui.Get<Text>("Text_time").text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _ui.Get<Text>("Text_time").text = Lang.Get("活动已经结束");
        }
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == _aid)
        {
            _ui.Get<Text>("Text_title").text = Cfg.Act.GetTitleName(_aid);
            _ui.Get<Text>("Text_type").text = Cfg.Act.GetRankType(aid);
            _ui.Get<Text>("Text_desc").text = Cfg.Act.GetData(aid).act_desc;
            list.Clear();
            for (int i = 0; i < _actInfo._userData.Count; i++)
            {
                int rank = _actInfo._userData[i].cur_rank;
                list.AddItem<_ActRankItem>().
                    Refresh(_actInfo._userData[i], rank, _actInfo.GetRankLv(rank));
            }
            RefreshRankReward();
        }
    }
    public override void OnShow()
    {
        list.ScrollRect.verticalNormalizedPosition = 1;
        UpdateTime(TimeManager.ServerTimestamp);
        ResetUi();
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
        RefreshRankReward();

    }
    //每次展示界面放在排行榜(而不是奖励)
    private void ResetUi()
    {
        _mainView.localPosition = Vector3.zero;
        _objSelfRank.SetActive(false);
        if (_listRect == null)
            _listRect = list.ScrollRect.GetComponent<RectTransform>();
        _listRect.sizeDelta = new Vector2(_listRect.sizeDelta.x, 504);
    }
    private void RefreshRankReward()
    {
        if (_rankReward == null)
        {
            ResHelper.LoadGameObjectInstanceAsyn("Activity/_Activity_Rank_01_Inf", _mainView, false, OnActRank01LoadCB);
        }
        else
        {
            string note1 = Lang.Get("只需冲到对应排名，即可激活领取该档奖励");
            DateTime time = TimeManager.ToServerDateTime(_actInfo.rewardTs);
            string tip = string.Format(Lang.Get("{0}开启"), time.ToString("HH:mm"));
            _reward.OnShow(_actInfo._rewardData, note1, _actInfo._rewardLv, tip, _actInfo.IsOpen(), _actInfo.GetRewardById, _actInfo._myInfo.rankItem.hasGet);
        }
    }
    private void OnActRank01LoadCB(GameObject obj)
    {
        string note1 = Lang.Get("只需冲到对应排名，即可激活领取该档奖励");
        DateTime time = TimeManager.ToServerDateTime(_actInfo.rewardTs);
        string tip = string.Format(Lang.Get("{0}开启"), time.ToString("HH:mm"));
        _rankReward = obj;
        _rankReward.transform.localPosition = new Vector3(0, -_rootRect.rect.height, 0);
        _reward = _rankReward.AddBehaviour<_Activity_Rank_Inf>();
        _reward.SetCloseCb(CloseRankReward);
        _reward.OnShow(_actInfo._rewardData, note1, _actInfo._rewardLv, tip, _actInfo.IsOpen(), _actInfo.GetRewardById, _actInfo._myInfo.rankItem.hasGet);
    }


    //查看排行榜奖励
    public void CheckRankReward()
    {
        RefreshRankReward();
        _mainView.DOLocalMoveY(_moveDistance, 0.5f);
    }
    //关闭排行榜奖励
    public void CloseRankReward()
    {
        _mainView.DOLocalMoveY(0, 0.5f);
    }
}
