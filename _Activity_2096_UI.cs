using System;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2096_UI : ActivityUI
{
    private Text _tittleText;
    private Text _timeText;
    private Button _helpBtn;
    private GameObject[] _rewardsList;
    private HorizontalLayoutGroup _layout;
    private Button _buyBtn;
    private Text _priceText;
    private Button _boostBtn;
    private Text _progressText;
    private Text _giftTimeText;
    private ListView _listView;
    private Text _boostText;

    private ActInfo_2096 _config = null;
    private const int _aid = 2096;

    public override void OnCreate()
    {
        _tittleText = transform.Find<JDText>("Title");
        _timeText = transform.Find<JDText>("TextTime");
        _helpBtn = transform.Find<Button>("BtnHelp");
        _buyBtn = transform.Find<Button>("BtnBuy");
        _priceText = transform.Find<JDText>("BtnBuy/Text");
        _boostBtn = transform.Find<Button>("Image/Button");
        _progressText = transform.Find<JDText>("Image/TextHelp");
        _listView = ListView.Create<Act2096Mission>(transform.Find("Scroll View"));
        _rewardsList = new GameObject[]
        {
             transform.Find<GameObject>("IconList/01"),
             transform.Find<GameObject>("IconList/02"),
             transform.Find<GameObject>("IconList/03"),
        };
        _layout = transform.Find<HorizontalLayoutGroup>("IconList");
        _giftTimeText = transform.Find<JDText>("TextTip");
        _boostText = _boostBtn.GetComponentInChildren<Text>();

        InitEvent();
        //InitListener();
    }

    public override void OnShow()
    {
        _config = (ActInfo_2096)ActivityManager.Instance.GetActivityInfo(_aid);
        if (_config == null)
        {
            Debug.LogError("配置丢失" + _aid);
            return;
        }

        _config.UpdateToday();

        _tittleText.text = _config._name;

        _progressText.text = Lang.Get("已助力{0}/{1}", _config.Progress, _config.Sum);


        if (_config.IsBuy)
        {
            _priceText.text = "已购买";
            _buyBtn.interactable = false;
            _boostText.text = Lang.Get("点击邀请好友助力");

            SetBoostButtonState();
        }
        else
        {
            if (null == _config.PayInfo)
            {
                Debug.LogError("配置PayInfo丢失");
                return;
            }
            _buyBtn.interactable = true;
            _priceText.text = _config.PayInfo._price;
            string str = "购买";
            _boostText.text = Lang.Get("{0}今日特选礼包解锁助力奖励", str);
        }

        RefreshRewards();

        RefreshMissionUi();
    }

    public void RefreshRewards()
    {
        int count = _config.SpecialItemList.Length;
        for (int i = 0; i < _rewardsList.Length; i++)
        {
            GameObject go = _rewardsList[i];
            if (i < count)
            {
                P_Item item = _config.SpecialItemList[i];
                ItemForShow itemForShow = new ItemForShow(item.id, item.count);
                go.SetActive(true);
                itemForShow.SetIcon(go.transform.Find<Image>("Icon"));
                go.transform.Find<Image>("Qua").color = _ColorConfig.GetQuaColor(itemForShow.GetQua());
                go.transform.Find<Text>("Text").text = "x" + GLobal.NumFormat(itemForShow.GetCount());
                go.GetComponent<Button>().onClick.SetListener(() =>
                {
                    AudioManager.Instace.PlaySoundOfNormalBtn();
                    DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(item.id, item.count, go.transform.position); });
                });
            }
            else
            {
                go.SetActive(false);
            }
        }

        if (count == 2)
        {
            _layout.spacing = -120;
        }
        else
        {
            _layout.spacing = 0;
        }
    }

    public void RefreshMissionUi()
    {
        _listView.Clear();
        for (int i = 0; i < _config.MissionList.Count; i++)
        {
            P_Act2096Mission item = _config.MissionList[i];
            _listView.AddItem<Act2096Mission>().Refresh(item);
        }
    }


    private void InitEvent()
    {
        _helpBtn.onClick.AddListener(On_helpBtnClick);
        _buyBtn.onClick.AddListener(On_buyBtnClick);
        _boostBtn.onClick.AddListener(On_boostBtnClick);
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Top_HelpDesc>(On_helpBtnDialogShowAsynCB);
    }
    private void On_helpBtnDialogShowAsynCB(_D_Top_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2096);
    }


    private void On_buyBtnClick()
    {
        if (_config == null)
            return;
        if (_config.IsBuy)
            return;
        _config.BuyGift();
    }
    private void On_boostBtnClick()
    {
        if (_config == null)
            return;
        if (!_config.IsBuy)
            return;
        if (_config.BoostTime - TimeManager.ServerTimestamp >= 0)
            return;
        if (_config.Progress >= _config.Sum)
        {
            MessageManager.Show("已完成今日助力任务");
            return;
        }
        _config.ShareBoost(On_shareBoostCB);
    }
    private void On_shareBoostCB()
    {
        _config = (ActInfo_2096)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void InitListener()
    {
        base.InitListener();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _rewardsList = null;
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _aid)
            return;

        if (!gameObject.activeSelf)
            return;

        OnShow();
    }

    public override void UpdateTime(long obj)
    {
        base.UpdateTime(obj);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!gameObject.activeSelf)
            return;

        if (_config == null)
            return;

        if (!_config.IsDuration())
        {
            var leftTime = _config._data.startts - TimeManager.ServerTimestamp;
            if (leftTime < 0)
                leftTime = 0;
            _timeText.text = Lang.Get("开启倒计时 {0}", GLobal.TimeFormat(leftTime, true));
            return;
        }

        if (_config.LeftTime >= 0)
        {
            _timeText.text = GlobalUtils.ActivityLeftTime(_config.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }


        var giftTime = _config.GiftTime - TimeManager.ServerTimestamp;
        if (giftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)giftTime);
            _giftTimeText.text = Lang.Get("今日特选礼包 {0}小时{1}分钟后过期", span.Hours, span.Minutes);
        }
        else
        {
            _giftTimeText.text = Lang.Get("今日特选礼包 已过期");
        }
        if (_config.IsBuy)
        {
            SetBoostButtonState();
        }
    }


    private void SetBoostButtonState()
    {
        var boostTime = _config.BoostTime - TimeManager.ServerTimestamp;
        if (boostTime >= 0)
        {
            _boostText.text = Lang.Get("点击邀请好友助力({0}s)", boostTime);
            _boostText.GetComponent<Outline>().effectColor = new Color(0.44f, 0.07f, 0.59f);
            _boostBtn.GetComponent<Image>().color = new Color(0, 1, 0.34f);
            if (_boostBtn.enabled)
                _boostBtn.enabled = false;
        }
        else
        {
            _boostText.text = Lang.Get("点击邀请好友助力");
            _boostText.GetComponent<Outline>().effectColor = new Color(0.07f, 0.59f, 0.22f);
            _boostBtn.GetComponent<Image>().color = new Color(1, 0, 0);

            if (!_boostBtn.enabled)
                _boostBtn.enabled = true;
        }
    }
}

public class Act2096Mission : ListItem
{
    private Slider _slider;
    private Slider _slider2;
    private GameObject _unreachGo;
    private Button _getBtn;
    private Image _icon;
    private Image _qua;
    private Text _countText;
    private Text _lvText;
    private const int _aid = 2096;
    private P_Act2096Mission _data;

    public override void OnCreate()
    {
        _slider = transform.Find<Slider>("Slider01");
        _slider2 = transform.Find<Slider>("Slider02");
        _unreachGo = transform.Find<GameObject>("UnReach");
        _getBtn = transform.Find<Button>("ButtonGet");
        _icon = transform.Find<Image>("Item/Icon");
        _qua = transform.Find<Image>("Item/Qua");
        _countText = transform.Find<Text>("Item/Text");
        _lvText = transform.Find<JDText>("Icon02/Nub");

        _getBtn.onClick.SetListener(On_getBtnClick);
    }
    private void On_getBtnClick()
    {
        if (!gameObject.activeSelf)
            return;
        ActInfo_2096 config = (ActInfo_2096)ActivityManager.Instance.GetActivityInfo(_aid);
        if (config == null)
            return;
        AudioManager.Instace.PlaySoundOfNormalBtn();
        config.RequestMissionReward(_data.tid, On_RequestMissionRewardCB);
    }
    private void On_RequestMissionRewardCB(P_Act2096Mission data)
    {
        _data = data;
        SetButtonState();
    }

    internal void Refresh(P_Act2096Mission data)
    {
        ActInfo_2096 config = (ActInfo_2096)ActivityManager.Instance.GetActivityInfo(_aid);
        if (config == null)
            return;

        _data = data;

        cfg_act_2096_boost_task cfg = Cfg.Activity2096.GetTaskData(data.tid);
        cfg_act_2096_boost_task cfgnext = null;

        P_Item item = config.GetMissionRewards(data.tid)[0];

        ItemForShow itemForShow = new ItemForShow(item.id, item.count);
        itemForShow.SetIcon(_icon);
        _countText.text = "x" + GLobal.NumFormat(itemForShow.GetCount());
        _qua.color = _ColorConfig.GetQuaColor(itemForShow.GetQua());

        _lvText.text = cfg.need_boost_value.ToString();

        if (data.tid == 1)
        {
            _slider.gameObject.SetActive(true);

            _slider.value = (float)(config.Progress + cfg.need_boost_value) / (2 * cfg.need_boost_value);
        }
        else
        {
            _slider.gameObject.SetActive(false);
        }

        if (data.tid == Cfg.Activity2096.GetMaxTid())
        {
            _slider2.gameObject.SetActive(false);
        }
        else
        {
            _slider2.gameObject.SetActive(true);
            cfgnext = Cfg.Activity2096.GetTaskData(data.tid + 1);

            if (config.Progress >= cfgnext.need_boost_value)
            {
                _slider2.value = 1;
            }
            else if (config.Progress > cfg.need_boost_value)
            {
                _slider2.value = (float)(config.Progress - cfg.need_boost_value) / (cfgnext.need_boost_value - cfg.need_boost_value);
            }
            else
            {
                _slider2.value = 0;
            }
        }

        _icon.GetComponent<Button>().onClick.SetListener(() =>
        {
            AudioManager.Instace.PlaySoundOfNormalBtn();
            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(item.id, item.count, transform.position); });
        });

        SetButtonState();
    }


    public void SetButtonState()
    {
        if (_data.get_reward == 1)
        {
            _getBtn.gameObject.SetActive(false);
            _unreachGo.SetActive(true);
            _unreachGo.GetComponentInChildren<Text>().text = Lang.Get("已领取");
        }
        else if (_data.finished == 1)
        {
            _unreachGo.SetActive(false);
            _getBtn.gameObject.SetActive(true);
        }
        else
        {
            _unreachGo.SetActive(true);
            _unreachGo.GetComponentInChildren<Text>().text = Lang.Get("未达成");
            _getBtn.gameObject.SetActive(false);
        }
    }
}