using System;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2093_UI : ActivityUI
{
    //每日宝箱按钮
    private Button _blessingBagButton;
    //每日宝箱红点
    private GameObject _blessingBagRedPoint;

    //阶段奖励滑动框
    private Transform _listViewRoot;
    private ListView _listView;

    //时间
    private Text _timeText;
    //vip点数显示
    private Text _vipPointNum;


    private Button _helpButton;
    private ActInfo_2093 _actInfo;

    private int _aid = 2093;
    public override void OnCreate()
    {
        InitRef();
        InitButton();
        //InitListener();
    }

    public void InitRef()
    {

        _listViewRoot = transform.Find("Scroll View");
        _listView = ListView.Create<CumulativeRechargeItem>(_listViewRoot);
        _timeText = transform.Find<Text>("CountDown");
        _vipPointNum = transform.Find<Text>("VipPointText");
        Transform icon = transform.Find("Icon");
        _blessingBagButton = icon.Find<Button>("Icon/img_icon");
        _blessingBagRedPoint = icon.Find("Image").gameObject;
        _helpButton = transform.Find<Button>("Helpbtn");
        _actInfo = (ActInfo_2093)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public void InitButton()
    {
        _blessingBagButton.onClick.AddListener(GetBlessingBag);
        _helpButton.onClick.AddListener(On_helpButtonClick);
    }
    private void On_helpButtonClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(OnOn_helpButtonDialogShowAsynCB);
    }
    private void OnOn_helpButtonDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2093, _helpButton.transform.position, Direction.LeftDown, 350);
    }
    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(ActivityUpdate);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == _aid)
        {
            Refresh();

        }
    }

    public override void OnShow()
    {
        Refresh();
        UpdateTime(0);
    }


    private void Refresh()
    {

        _blessingBagRedPoint.SetActive(_actInfo.IsShowBlessingBagRedPoint);
        _blessingBagButton.interactable = _actInfo.IsShowBlessingBagRedPoint;

        _listView.Clear();

        int currentRechargeNum = _actInfo.GetCurrentRechargeNum();
        _vipPointNum.text = Lang.Get("活动期间大小充值均有好礼\n<Color=#ffcc00>充值获得的VIP点数：{0}</Color>", currentRechargeNum);

        var tempActInfo = _actInfo.GetAct2093DetailInfo();
        for (int i = 1; i < tempActInfo.Count; i++)
        {
            _listView.AddItem<CumulativeRechargeItem>().Show(tempActInfo[i], currentRechargeNum, _actInfo.GetReward);
        }


    }

    private void GetBlessingBag()
    {
        _actInfo.GetReward(Cfg.Activity2093.GetDailyTreasureTid(), OnActGetRewardCB);
    }
    private void OnActGetRewardCB()
    {
        _blessingBagRedPoint.SetActive(false);
        _blessingBagButton.interactable = false;
    }

    public override void UpdateTime(long currentTime)
    {
        base.UpdateTime(currentTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_actInfo == null)
            return;

        if (_actInfo.LeftTime >= 0)
        {
            _timeText.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }

    public override void OnClose()
    {
        base.OnClose();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _actInfo = null;
    }
}

public class CumulativeRechargeItem : ListItem
{

    struct Rewards
    {
        public int rewardId;
        public GameObject icon;
        public Image rewardIcon;
        public Image rewardQue;
        public Text rewardNum;
        public Button showButton;
    }
    private Rewards[] _rewards;

    //目标数量描述文本
    private Text _goalDesc;
    //当前进度
    private Text _progress;
    //领取按钮
    private Button _button;
    //未达成按钮
    private Image _haventBtn;
    //已领取按钮
    private Image _gotBtn;
    //点击领取回调
    private Action<int, Action> _callback;


    //tid
    private int _tid;


    private bool firstOpen;


    private int _aid = 2093;
    public override void OnCreate()
    {
        InitRef();
        firstOpen = true;
    }

    private void InitRef()
    {
        _goalDesc = transform.Find<Text>("Title");
        _progress = transform.Find<Text>("Text");
        _button = transform.Find<Button>("GetBtn");
        _haventBtn = transform.Find<Image>("HaventBtn");
        _gotBtn = transform.Find<Image>("GotBtn");


        _rewards = new Rewards[3];

        for (int i = 0; i < _rewards.Length; i++)
        {
            _rewards[i].icon = transform.Find<Image>("Icon_0" + (i + 1).ToString()).gameObject;
            _rewards[i].rewardIcon = _rewards[i].icon.transform.Find<Image>("img_icon");
            _rewards[i].rewardQue = _rewards[i].icon.transform.Find<Image>("Img_qua");
            _rewards[i].rewardNum = _rewards[i].icon.transform.Find<Text>("Text");
            _rewards[i].showButton = _rewards[i].rewardIcon.GetComponent<Button>();
            var i1 = i;
            _rewards[i].showButton.onClick.AddListener(() =>
            {
                DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(_rewards[i1].rewardId, 1, _rewards[i1].showButton.transform.position); });
            });
        }


        _button.onClick.AddListener(ReceiveReward);
    }

    private void ShowTaskRewards()
    {
        _goalDesc.text = Lang.Get(Cfg.Activity2093.GetDescriptionByTid(_tid));
        P_Item[] rewards = GlobalUtils.ParseItem(Cfg.Activity2093.GetRewardByTid(_tid));

        int num = 0;
        for (num = 0; num < rewards.Length; num++)
        {
            _rewards[num].rewardId = rewards[num].Id;
            _rewards[num].icon.SetActive(true);
            Cfg.Item.SetItemIcon(_rewards[num].rewardIcon, rewards[num].Id);
            _rewards[num].rewardQue.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(rewards[num].Id));
            _rewards[num].rewardNum.text = rewards[num].Num.ToString();
        }

        for (int i = num; i < _rewards.Length; i++)
        {
            _rewards[i].icon.SetActive(false);
        }
    }
    public void Show(Act2093Detail stageGoal, int currentStageProgress, Action<int, Action> callback)
    {
        _tid = stageGoal.tid;
        _callback = callback;

        if (firstOpen)
        {
            ShowTaskRewards();
            firstOpen = false;
        }
        _progress.text = string.Format("（<Color=#00ff00ff>{0}</Color>/{1}）", stageGoal.do_number, Cfg.Activity2093.GetStageGoalByTid(_tid));
        if (stageGoal.finished == 1)
        {
            _button.gameObject.SetActive(true);

            if (stageGoal.get_reward == 0)
            {
                _button.gameObject.SetActive(true);
                _gotBtn.gameObject.SetActive(false);
            }
            else
            {
                _button.gameObject.SetActive(false);
                _gotBtn.gameObject.SetActive(true);
            }
            _haventBtn.gameObject.SetActive(false);
        }
        else
        {
            _button.gameObject.SetActive(false);
            _haventBtn.gameObject.SetActive(true);
            _gotBtn.gameObject.SetActive(false);

        }

    }
    private void ReceiveReward()
    {
        _callback(_tid, OnReceiveRewardCB);
    }
    private void OnReceiveRewardCB()
    {
        EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
    }
}