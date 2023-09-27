using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2302_UI : ActivityUI
{
    private Text _title;//标题

    private Text _timeText;//时间显示

    private Button _helpButton;//帮助按钮

    private Button _oneDrawButton;//抽奖一次
    private Text _oneDrawButtonText;//抽一次消耗显示

    private Button _tenDrawButton;//抽奖十次
    private Text _tenDrawButtonText;//抽十次消耗显示

    private Animator _animator_Senior;//滚动动画


    private Text _integral;//积分显示
    //奖励
    private ShowReward[] _rewardLists;
    //奖励展示
    private GameObject _rewardsPanel;
    private ListView _rewards;
    private Button _rewardCloseButton;

    private Coroutine _drawOnceFunc;
    private Coroutine _drawTenTimesFunc;
    private string _needShowRewards;//需要展示的道具 切换界面时展示用

    private bool _isDrawing;//是否正在抽卡

    private int _aid = 2302;
    private const string EffectName = "IsShowEffect";
    private const string SliderName = "_Slider";

    private Material _material;
    private class ShowReward//抽奖可获得的奖品
    {
        public Image Icon;
        public Image Qua;
        public Button Button;
        public Text Num;
        public int RewardId;//奖励id
        public GameObject SelectBorder;//选中框
    }
    private ActInfo_2302 _actInfo;
    public override void OnCreate()
    {
        InitRef();
        InitButton();
        //InitEvent();
        RefreshUI();
    }

    public override void OnShow()
    {
        UpdateTime(0);
        _isDrawing = false;
    }

    private void InitRef()
    {

        _rewardLists = new ShowReward[10];

        _title = transform.Find<Text>("Main/Title");
        _timeText = transform.Find<Text>("Main/TimeText");
        _helpButton = transform.Find<Button>("Main/BtnDetail");
        _oneDrawButton = transform.Find<Button>("Main/BtnBuyOne");
        _oneDrawButtonText = _oneDrawButton.transform.Find<Text>("costCount");
        _tenDrawButton = transform.Find<Button>("Main/BtnBuyTen");
        _tenDrawButtonText = _tenDrawButton.transform.Find<Text>("costCount");
        _integral = transform.Find<Text>("Main/Text");

        _animator_Senior = transform.Find<Animator>("Main/IconContent/Img_Senior/pfb_animation/ani_Senior_d");
        _material = transform.Find<Image>("Main/IconContent/Img_Senior/Img_SeniorLine").material;

        for (int i = 0; i < _rewardLists.Length; i++)
        {
            string path = "Main/IconContent/Icon" + (i + 1).ToString();
            _rewardLists[i] = new ShowReward
            {
                Icon = transform.Find<Image>(path + "/1/icon"),
                Qua = transform.Find<Image>(path + "/1/Img_qua"),
                Num = transform.Find<Text>(path + "/1/Text")
            };
            _rewardLists[i].Button = _rewardLists[i].Icon.GetComponent<Button>();
            _rewardLists[i].SelectBorder = transform.Find(path + "/Effect").gameObject;
            var i1 = i;
            _rewardLists[i].Button.onClick.AddListener(() =>
            {
                DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(_rewardLists[i1].RewardId, 1, _rewardLists[i1].Button.transform.position); });
            });
            //奖励展示部分
        }

        _rewardsPanel = transform.Find("Main/RewardPanel").gameObject;
        _rewardCloseButton = transform.Find<Button>("Main/RewardPanel/Img_bg/CloseBtn");

        Transform listViewRoot = _rewardsPanel.transform.Find("Scroll View");
        _rewards = ListView.Create<Act2302Reward>(listViewRoot);

    }
    private void InitButton()
    {
        _helpButton.onClick.AddListener(On_helpButtonClick);
        _oneDrawButton.onClick.AddListener(On_oneDrawButtonClick);
        _tenDrawButton.onClick.AddListener(On_tenDrawButtonClick);
        _rewardCloseButton.onClick.AddListener(On_rewardCloseButtonClick);
    }
    private void On_helpButtonClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpButtonDialogShowAsynCB);
    }
    private void On_helpButtonDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(_actInfo._name, _actInfo._desc, _helpButton.transform.position, Direction.LeftDown, 350, new Vector2(-25, -25));
    }
    private void On_oneDrawButtonClick()
    {
        StartLottery(1);
    }
    private void On_tenDrawButtonClick()
    {
        StartLottery(10);
    }
    private void On_rewardCloseButtonClick()
    {
        ChangeRewardPanel(false);
    }
    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(Refresh);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _aid)
            return;
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        RefreshUI();

    }
    private void RefreshUI()
    {

        _actInfo = (ActInfo_2302)ActivityManager.Instance.GetActivityInfo(_aid);
        _title.text = _actInfo._name;

        RefreshRewardsList();//刷新转盘奖励
        RefreshIntegral();// 刷新积分
        _oneDrawButtonText.text = Lang.Get("{0} 抽奖1次", GetCostGold(1));
        _tenDrawButtonText.text = Lang.Get("{0} 抽奖10次", GetCostGold(10));
    }
    //刷新积分
    private void RefreshIntegral()
    {
        _integral.text = Lang.Get("积分 : {0}", _actInfo.GetIntegral().ToString());
        float current = ((float)_actInfo.GetIntegral() / (float)_actInfo.GetMaxIntegral());
        _material.SetFloat(SliderName, current);
    }
    //刷新可以获得的抽奖道具
    private void RefreshRewardsList()
    {
        var rareReward = _actInfo.GetRareReward();
        var normalRewards = _actInfo.GetNormalRewards();
        //初始化稀有道具 稀有道具放在左上角
        _rewardLists[0].RewardId = rareReward.Id;
        Cfg.Item.SetItemIcon(_rewardLists[0].Icon, rareReward.Id);
        _rewardLists[0].Qua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(rareReward.Id));
        _rewardLists[0].Num.text = "X" + GLobal.NumFormat(rareReward.Num);
        //初始化普通道具
        for (int i = 0; i < normalRewards.Length; i++)
        {
            _rewardLists[i + 1].RewardId = normalRewards[i].Id;
            Cfg.Item.SetItemIcon(_rewardLists[i + 1].Icon, normalRewards[i].Id);
            _rewardLists[i + 1].Qua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(normalRewards[i].Id));
            _rewardLists[i + 1].Num.text = "X" + GLobal.NumFormat(normalRewards[i].Num);
        }
    }
    //开始抽奖
    private void StartLottery(int type)
    {
        if (_isDrawing)
        {
            MessageManager.Show(Lang.Get("正在抽奖中，请稍后操作！"));
            return;
        }

        var d = Alert.YesNo(Lang.Get("确定花费{0}氪晶进行{1}次抽奖？", GetCostGold(type).ToString(), type));
        d.SetYesCallback(() =>
        {
            if (ItemHelper.IsCountEnough(ItemId.Gold, GetCostGold(type)))
            {
                if (!_isDrawing)
                {
                    _isDrawing = true;
                    //调用抽奖接口
                    _actInfo.StartLottery(type, (rewards) =>
                   {
                       _needShowRewards = rewards;
                       ShowDrawResult(rewards);
                   });
                }
                else
                {
                    MessageManager.Show(Lang.Get("正在抽奖中，请稍后操作！"));
                }
            }
            d.Close();
        });
    }
    private int GetCostGold(int type)
    {
        if (type == 1)
        {
            return _actInfo.GetLotteryOnceConsumption();
        }
        else
        {
            return _actInfo.GetLotteryTenTimesConsumption();
        }
    }
    private void ShowDrawResult(string rewards)
    {
        if (_drawOnceFunc != null)
            _Scheduler.Instance.Stop(_drawOnceFunc);

        if (_drawTenTimesFunc != null)
            _Scheduler.Instance.Stop(_drawTenTimesFunc);

        MakeSelectBorderInvisible();
        _animator_Senior.SetBool(EffectName, true);
        var _indexList = _actInfo.GetIndexOfReward(rewards);

        if (_indexList.Count == 1)
        {
            _drawOnceFunc = _Scheduler.Instance.StartCoroutine(DoDrawSingle(_indexList[0], rewards));
        }
        else
        {
            _drawTenTimesFunc = _Scheduler.Instance.StartCoroutine(DoDraw(_indexList, rewards));
        }
    }
    //单抽
    private IEnumerator DoDrawSingle(int pos, string rewards)
    {
        float time = Time.realtimeSinceStartup;
        int index = 0;
        while (Time.realtimeSinceStartup <= time + 0.6f)
        {
            ShowDrawResultByPos2(index, 0.03f);
            yield return new WaitForSeconds(0.03f);
            index = (index + 1) % 10;
        }
        while (index != pos)
        {
            ShowDrawResultByPos2(index, 0.1f);
            yield return new WaitForSeconds(0.09f);
            index = (index + 1) % 10;
        }
        ShowDrawResultByPos2(pos, 1.0f);

        yield return new WaitForSeconds(1.0f);

        MakeSelectBorderInvisible();
        ShowRewards(rewards);
    }


    //连抽十次
    private IEnumerator DoDraw(List<int> posList, string rewards)
    {
        int i = 0;
        int pos = 0;

        bool extraDraw = posList[0] == pos ? true : false;

        //跑第一圈 第一个位置就是抽中目标 ，第一圈多跑
        if (extraDraw)
        {
            while (i < 1)
            {
                ShowDrawResultByPos2(pos, 0.09f);
                yield return new WaitForSeconds(0.08f);
                pos = (pos + 1) % 10;
                if (pos == 0)
                {
                    i++;
                }
            }
        }
        i = 0;
        while (true)
        {
            if (pos == posList[posList.Count - 1] && i >= posList.Count - 1)
            {
                ShowDrawResultByPos2(pos, 0.8f);
                break;
            }

            if (posList[i] == pos)
            {
                ShowDrawResultByPos2(pos, 0.8f);
                yield return new WaitForSeconds(0.6f);
            }
            else
            {
                ShowDrawResultByPos2(pos, 0.09f);
                yield return new WaitForSeconds(0.08f);
            }
            pos = (pos + 1) % 10;
            if (pos == 0)
            {
                i++;
            }
        }
        yield return new WaitForSeconds(1.0f);
        ShowRewards(rewards);
    }
    private void MakeSelectBorderInvisible()
    {

        for (int i = 0; i < _rewardLists.Length; i++)
        {
            var one = _rewardLists[i];
            one.SelectBorder.SetActive(false);
        }

    }

    private void ShowDrawResultByPos2(int index, float duration)
    {

        GameObject selectBorder = _rewardLists[index].SelectBorder;
        selectBorder.SetActive(true);
        _Scheduler.Instance.PerformWithDelay(duration, () =>
        {
            selectBorder.SetActive(false);
        });
    }
    //展示抽奖获得的奖励
    private void ShowRewards(string rewards)
    {

        _isDrawing = false;
        RefreshIntegral();
        ChangeRewardPanel(true);
        _animator_Senior.SetBool(EffectName, false);
        P_Item[] items = GlobalUtils.ParseItem(rewards);
        _rewards.Clear();
        for (int i = 0; i < items.Length; i++)
        {
            var one = items[i];
            _rewards.AddItem<Act2302Reward>().OnShow(one);
        }
        _needShowRewards = null;
    }
    public override void UpdateTime(long currentTime)
    {
        base.UpdateTime(currentTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!gameObject.activeSelf)
            return;
        if (_actInfo == null)
            return;
        _timeText.text = _actInfo.LeftTime >= 0 ? GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true) : Lang.Get("活动已经结束");
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _material = null;
    }
    public override void OnClose()
    {
        base.OnClose();

        if (_drawOnceFunc != null)
            _Scheduler.Instance.Stop(_drawOnceFunc);

        if (_drawTenTimesFunc != null)
            _Scheduler.Instance.Stop(_drawTenTimesFunc);
        if (_needShowRewards != null)
        {
            ShowRewards(_needShowRewards);
        }
        _isDrawing = false;
    }
    private void ChangeRewardPanel(bool change)
    {
        _rewardsPanel.gameObject.SetActive(change);
    }
}

public class Act2302Reward : ListItem
{
    public Text Name;
    public Image Icon;
    public Image Qua;
    public Button BtnTip;
    public Text TextNum;
    public int RewardId;
    public override void OnCreate()
    {

        Icon = transform.Find<Image>("ImageIcon");
        Qua = transform.Find<Image>("ImageQua");
        BtnTip = transform.GetComponent<Button>();
        TextNum = transform.Find<Text>("TextCount");
        Name = transform.Find<Text>("TextName");
        BtnTip.onClick.AddListener(OnBtnTipClick);
    }
    private void OnBtnTipClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(OnBtnTipDialogShowAsynCB);
    }
    private void OnBtnTipDialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(RewardId, 1, BtnTip.transform.position);
    }
    public void OnShow(P_Item item)
    {
        RewardId = item.Id;
        TextNum.text = "X" + GLobal.NumFormat(item.Num);
        Name.text = Cfg.Item.GetItemName(RewardId);
        Qua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(RewardId));
        Cfg.Item.SetItemIcon(Icon, RewardId);

    }
}