using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class _ActivityRank2_UI : ActivityUI
{
    private Text _tittle;
    private Text _descText;
    private Text _time;
    private ListView _powerList;
    private ActInfo_ActivityRank2 _actInfo;
    private int _aid;
    private int ustate;
    private RectTransform _mainView;
    private GameObject _rankReward;
    private _Activity_Rank_Inf _reward;
    private RectTransform _rootRect;
    private float _moveDistance;
    //玩家信息
    private GameObject _objSelfRank;
    private RectTransform _listRect;
    private Text _textRank;
    private _StateFlag _iconUnion;
    private Text _textName;
    private Text _textPpt;
    private Text _textReward;
    private Button _showRewardsBtn;

    private ObjectGroup UI;

    public override void OnCreate()
    {
        UI = transform.GetComponent<ObjectGroup>();
        _tittle = transform.Find<JDText>("MainView/Text_title");
        _descText = transform.Find<JDText>("MainView/Text_desc");
        _time = transform.FindText("MainView/Text_time");
        _powerList = ListView.Create<_ActRank2Item>(transform.Find("MainView/Scrollview"));
        _mainView = transform.Find<RectTransform>("MainView");
        _rootRect = gameObject.GetComponent<RectTransform>();
        _moveDistance = _rootRect.rect.height - 5;
        _textRank = transform.Find<Text>("MainView/Me/Text_rank");
        _iconUnion = new _StateFlag(transform.Find<RectTransform>("MainView/Me/Img_state"));
        _textName = transform.Find<Text>("MainView/Me/Text_name");
        _textPpt = transform.Find<Text>("MainView/Me/Text_count");
        _textReward = transform.Find<Text>("MainView/Me/Text_reward");
        _showRewardsBtn = transform.FindButton("MainView/Btn_reward");
        _objSelfRank = transform.Find("MainView/Me").gameObject;
        InitData();
        InitEvent();
        //InitListener();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnShow()
    {
        _mainView.localPosition = Vector3.zero;
        _powerList.ScrollRect.verticalNormalizedPosition = 1;
        _objSelfRank.SetActive(true);
        if (_listRect == null)
            _listRect = _powerList.ScrollRect.GetComponent<RectTransform>();
        _listRect.sizeDelta = new Vector2(_listRect.sizeDelta.x, 456);
        _actInfo.RefreshAct();
        UpdateUI(_aid);
    }

    public override void SetAid(int aid)
    {
        _aid = aid;
    }

    private void InitData()
    {
        ustate = Uinfo.Instance.Player.Info.ustate;
        _actInfo = (ActInfo_ActivityRank2)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    private void InitEvent()
    {
        _showRewardsBtn.onClick.AddListener(CheckRankReward);
    }

    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
        //TimeManager.Instance.TimePassSecond += UpdateTime;
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

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == _aid)
        {
            //刷新对应列的标题
            UI.Get<Text>("Text_type").text = Cfg.Act.GetRankType(aid);
            UI.Get<Text>("Text_desc").text = Cfg.Act.GetData(aid).act_desc;

            _tittle.text = _actInfo._name;
            _descText.text = _actInfo._desc;

            P_ActRank2DataList powerList = _actInfo._data2029;
            //刷新个人排名
            var myRank = powerList.my_rank;
            _textRank.text = myRank.rank + "";
            _iconUnion.SetState(ustate);
            _textName.text = Uinfo.Instance.Player.Info.uname;
            _textPpt.text = GlobalUtils.NumFormatPower(myRank.upower_history);
            _textReward.text = _actInfo.GetRankLv(myRank.rank) == 0
                ? Lang.Get("暂无")
                : string.Format(Lang.Get("{0}档"), _actInfo.GetRankLv(myRank.rank));
            //刷新列表
            _powerList.Clear();
            for (int i = 0; i < powerList.rank_list.Count; i++)
            {
                int rank = _actInfo._data2029.rank_list[i].rank;
                _powerList.AddItem<_ActRank2Item>()
                    .Refresh(powerList.rank_list[i], _actInfo._data2029.rank_list[i].ustate, rank, _actInfo.GetRankLv(rank));
            }
            RefreshRankReward();
        }
    }

    private void RefreshRankReward()
    {
        string note = Lang.Get("排名结算后 可获取相应奖励");
        int rewardLv = _actInfo.GetRankLv(_actInfo._data2029.my_rank.rank);
        DateTime time = TimeManager.ToServerDateTime(_actInfo.freshts);
        string tip = string.Format(Lang.Get("{0}开启奖励"), time.ToString("MM/dd HH:mm\n"));
        if (_rankReward == null)
        {
            ResHelper.LoadGameObjectInstanceAsyn("Activity/_Activity_Rank_01_Inf", _mainView, false, OnActRankLoadCB);
        }
        else
        {
            _reward.OnShow(_actInfo._rewardData, note, rewardLv, tip, _actInfo.isOpen(), _actInfo.GetAct2029Reward, _actInfo._data2029.my_rank.hasGet);
        }
    }

    private void OnActRankLoadCB(GameObject obj)
    {
        _rankReward = obj;
        _rankReward.transform.localPosition = new Vector3(0, -_rootRect.rect.height, 0);
        _reward = _rankReward.AddBehaviour<_Activity_Rank_Inf>();
        _reward.SetCloseCb(CloseRankReward);
    }


    //查看排行榜奖励
    private void CheckRankReward()
    {
        RefreshRankReward();
        _mainView.DOLocalMoveY(_moveDistance, 0.5f);
    }

    //关闭排行榜奖励
    private void CloseRankReward()
    {
        _mainView.DOLocalMoveY(0, 0.5f);
    }
}

