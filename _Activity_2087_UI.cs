using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2087_UI : ActivityUI
{
    private Text _timeText;
    private Button _helpBtn;
    private JDText _descText;
    private Slider _slider;
    private RectTransform _curTrans;
    private Text _curText;
    private Button[] _rewardsBtn;
    private Transform[] _rankGo;
    private Button _personBtn;
    private GameObject _rewardGo;
    private GameObject _fightGo;
    private GameObject _noticeGo;
    private GameObject _mainGo;
    //奖励
    private Button _stateRewardGo;
    private Button _personRewardGo;
    private Button _stateRewardBtn;
    private JDText _textStateReward;
    private Button _personRewardBtn;
    private Text _freeCount;

    //仓库子界面
    private ListView _listView;

    //预告
    private Text _noticeText;

    private ActInfo_2087 _actInfo;
    private ActivityInfo _actinfo;
    private int _aid = 2087;
    //50,200氪晶的价格各付费抽取一次
    private int[] _prices = new int[] { 50, 100, 200 };
    private const int _maxCount = 3;

    private Sprite[] _spriteHotIcon;

    private ObjectGroup UI;


    public override void OnCreate()
    {
        InitRef();
        InitEvent();
        //InitListener();
    }

    private void InitRef()
    {
        _timeText = transform.Find<JDText>("TextCountDown");

        _rewardsBtn = new Button[]
        {
            transform.Find<Button>("Mid_01/Box/Icon/Icon0"),
            transform.Find<Button>("Mid_01/Box/Icon/Icon1"),
            transform.Find<Button>("Mid_01/Box/Icon/Icon2"),
            transform.Find<Button>("Mid_01/Box/Icon/Icon3"),
            transform.Find<Button>("Mid_01/Box/Icon/Icon4"),
        };

        _helpBtn = transform.Find<Button>("_btnManual");

        _slider = transform.Find<Slider>("Mid_01/Box/Slider");

        _descText = transform.Find<JDText>("Mid_01/Text/TextDesc");

        _curTrans = transform.Find<RectTransform>("Mid_01/Box/CurPro");

        _curText = transform.Find<JDText>("Mid_01/Box/CurPro/Text");

        _rankGo = new Transform[]
        {
             transform.Find("Mid_01/ListItem1"),
             transform.Find("Mid_01/ListItem2"),
             transform.Find("Mid_01/ListItem3"),
        };

        _personBtn = transform.Find<Button>("Mid_01/Button");

        _rewardGo = transform.Find<GameObject>("RewardGo");

        _fightGo = transform.Find<GameObject>("BuildingGo");

        _stateRewardGo = transform.Find<Button>("RewardGo/Icon1");

        _personRewardGo = transform.Find<Button>("RewardGo/Icon2");

        _stateRewardBtn = transform.Find<Button>("RewardGo/Btn1");

        _personRewardBtn = transform.Find<Button>("RewardGo/Btn2");

        _textStateReward = transform.Find<JDText>("RewardGo/TextOpenBoxLimit");

        _freeCount = transform.Find<JDText>("RewardGo/Icon1/CountText");

        _mainGo = transform.Find<GameObject>("Mid_01");

        _noticeGo = transform.Find<GameObject>("Mid_02_text");

        _listView = ListView.Create<Act2087ModelItem>(transform.Find("BuildingGo/Scroll View"));

        _noticeText = transform.Find<JDText>("Mid_02_text/TextDesc");

        UI = transform.GetComponent<ObjectGroup>();

        _spriteHotIcon = new Sprite[]
        {
            UI.Sprite("ColdIcon"),
            UI.Sprite("NormalIcon"),
            UI.Sprite("HotIcon"),
        };
    }

    private void InitEvent()
    {
        _helpBtn.onClick.AddListener(On_helpBtnClick);
        _personBtn.onClick.SetListener(On_personBtnClick);
        _stateRewardBtn.onClick.SetListener(On_stateRewardBtnClick);
        _personRewardBtn.onClick.SetListener(On_personRewardBtnClick);
        for (int i = 0; i < _rewardsBtn.Length; i++)
        {
            int index = i;
            _rewardsBtn[i].onClick.SetListener(() =>
            {
                ShowBoxUI(index + 2);
            });
        }
        _stateRewardGo.onClick.SetListener(On_stateRewardGoClick);
        _personRewardGo.onClick.SetListener(On_personRewardGoClick);
        //});
        //点击跳转坐标
        _descText.SetHyperlinkCallback(_HyperLinkCallback.CoordCallback);
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Top_HelpDesc>(On_helpBtnDialogShowAsynCB);
    }
    private void On_helpBtnDialogShowAsynCB(_D_Top_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2087);
    }
    private void On_personBtnClick()
    {
        DialogManager.ShowAsyn<_D_Act2087Rank>(On_personBtnDialogShowAsynCB);
    }
    private void On_personBtnDialogShowAsynCB(_D_Act2087Rank d)
    {
        d?.OnShow();
    }
    private void On_stateRewardBtnClick()
    {
        _actInfo = (ActInfo_2087)ActivityManager.Instance.GetActivityInfo(_aid);
        if (_actInfo == null)
            return;
        if (_actInfo.FreeCount > 0)
        {
            _actInfo.Get2087Reward(1, () =>
            {
                if (gameObject.activeSelf)
                    ShowBottomUI();
            });
        }
        else if (_actInfo.CostCount > 0)
        {
            int gold = Uinfo.Instance.Player.Info.ugold;
            int price = _prices[_maxCount - _actInfo.CostCount];
            if (price <= gold)
            {
                var a = Alert.YesNo(Lang.Get("确定花费{0}氪晶开启势力宝箱?", price));
                a.SetYesCallback(() =>
                {
                    _actInfo.Get2087Reward(1, () =>
                    {
                        if (gameObject.activeSelf)
                            ShowBottomUI();
                    });

                    a.Close();
                });
            }
            else
            {
                DialogManager.ShowAsyn<_D_JumpConfirm>(d => { d?.OnShow(JumpType.Kr, price); });
            }
        }
    }
    private void On_personRewardBtnClick()
    {
        _actInfo = (ActInfo_2087)ActivityManager.Instance.GetActivityInfo(_aid);
        if (_actInfo == null)
            return;
        if (_actInfo.PersonRewards.Count == 0)
        {
            MessageManager.Show(Lang.Get("未参与战斗，个人奖励等级为0"));
            return;
        }
        _actInfo.Get2087Reward(2, () =>
        {
            if (gameObject.activeSelf)
                ShowBottomUI();
        });
    }
    private void On_stateRewardGoClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        if (_actInfo.StateRewards.Count == 0)
        {
            MessageManager.Show(Lang.Get("未参与战斗，势力宝箱等级为0"));
            return;
        }
        DialogManager.ShowAsyn<_D_ShowRewards>(On_stateRewardGoDialogShowAsynCB);
    }
    private void On_stateRewardGoDialogShowAsynCB(_D_ShowRewards d)
    {
        d?.PreviewBox(_actInfo.StateRewards, Lang.Get("势力宝箱"), Lang.Get("开启宝箱可获得下列道具之一"));
    }
    private void On_personRewardGoClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        if (_actInfo.PersonRewards.Count == 0)
        {
            MessageManager.Show(Lang.Get("未参与战斗，个人奖励等级为0"));
            return;
        }
        DialogManager.ShowAsyn<_D_ShowRewards>(On_personRewardGoDialogShowAsynCB);
    }
    private void On_personRewardGoDialogShowAsynCB(_D_ShowRewards d)
    {
        d?.PreviewBox(_actInfo.PersonRewards, Lang.Get("个人奖励"), Lang.Get("可获得下列所有道具"));
    }

    public override void InitListener()
    {
        base.InitListener();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _actInfo = null;
        _actinfo = null;
    }

    public override void OnShow()
    {
        _actinfo = ActivityManager.Instance.GetActivityInfo(_aid);
        if (_actinfo == null)
            _actinfo = ActivityManager.Instance.GetFutureActivityInfo(_aid);
        if (_actinfo == null)
            return;

        if (_actinfo.IsDuration())
        {
            _noticeGo.SetActive(false);
            _mainGo.SetActive(true);
            ActivityManager.Instance.RequestUpdateActivityById(_aid);
        }
        else
        {
            ShowNotice();
        }
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _aid)
            return;

        if (gameObject.activeSelf)
        {
            _OnShow();
        }
    }

    private void _OnShow()
    {
        _actinfo = ActivityManager.Instance.GetActivityInfo(_aid);
        if (_actinfo == null)
            _actinfo = ActivityManager.Instance.GetFutureActivityInfo(_aid);
        if (_actinfo == null)
            return;

        if (_actinfo.IsDuration())
        {
            _actInfo = (ActInfo_2087)_actinfo;

            PlayerPrefs.SetString("Act2087" + User.login_id, _actInfo._data.startts.ToString());
            EventCenter.Instance.RemindActivity.Broadcast(_aid, _actInfo.IsAvaliable());

            _noticeGo.SetActive(false);
            _mainGo.SetActive(true);

            int maxValue = Cfg.SupremacyReward.GetData(6).condition;

            float rate = (float)_actInfo.Score / maxValue;

            _slider.value = rate;

            float posx = Mathf.Lerp(-326, 325, rate);

            _curTrans.anchoredPosition = new Vector2(posx, -56);

            _curText.text = _actInfo.Score.ToString();

            for (int i = 0; i < 5; i++)
            {
                if (_actInfo.Score >= Cfg.SupremacyReward.GetData(i + +2).condition)
                {
                    UIHelper.SetImageSprite(_rewardsBtn[i].GetComponent<Image>(), "Icon/icon_361");
                }
                else
                {
                    UIHelper.SetImageSprite(_rewardsBtn[i].GetComponent<Image>(), "Icon/icon_362");
                }
            }

            ShowBottomUI();

            DefineRank(_actInfo.RankList);
        }
        else
        {
            ShowNotice();
        }
    }

    private void ShowNotice()
    {
        _noticeGo.SetActive(true);
        _mainGo.SetActive(false);
        _rewardGo.SetActive(false);
        _fightGo.SetActive(true);
        _noticeText.text = Cfg.Act.GetData(_aid).pre_act;
        ShowBuildingList();
    }

    private void ShowBuildingList()
    {
        _listView.Clear();

        var list = Cfg.Stellar.GetAllBuildingIdList();

        for (int i = 0; i < list.Count; i++)
        {
            _listView.AddItem<Act2087ModelItem>().RefreshNotice(list[i]);
        }
    }

    private void ShowBottomUI()
    {
        if (_actInfo.Step == 3)
        {
            _rewardGo.SetActive(true);
            _fightGo.SetActive(false);
            _textStateReward.text = Lang.Get("个人积分达到{0}后可开启", Cfg.SupremacyReward.MinPersonalScore);
            _descText.text = Lang.Get("黑洞争夺战已结束，占领积分如下");

            if (_actInfo.FreeCount > 0)
            {
                _freeCount.text = "x" + _actInfo.FreeCount;
                _freeCount.gameObject.SetActive(true);
                UIHelper.SetImageSprite(_stateRewardBtn.GetComponent<Image>(), "btn_332");
                _stateRewardBtn.GetComponentInChildren<Text>().text = Lang.Get("开启宝箱");
            }
            else
            {
                _freeCount.gameObject.SetActive(false);
                UIHelper.SetImageSprite(_stateRewardBtn.GetComponent<Image>(), "btn_331");

                if (_actInfo.CostCount > 0)
                {
                    _stateRewardBtn.GetComponentInChildren<Text>().text = Lang.Get("{0}氪晶开启宝箱", _prices[_maxCount - _actInfo.CostCount]);
                }
                else
                {
                    _stateRewardBtn.GetComponentInChildren<Text>().text = Lang.Get("已领取");
                }
            }

            if (_actInfo.IsGetPersonReward)
            {
                _personRewardBtn.GetComponentInChildren<Text>().text = Lang.Get("已领取");
            }
            else
            {
                _personRewardBtn.GetComponentInChildren<Text>().text = Lang.Get("领取奖励");

                if (_actInfo.PersonRewards.Count == 0 || _actInfo.PersonRewards.Count == 0)
                {
                    _personRewardBtn.GetComponent<Image>().color = new Color(255, 255, 255, 168);
                }
                else
                {
                    _personRewardBtn.GetComponent<Image>().color = new Color(255, 0, 0, 255);
                }
            }
        }
        else
        {
            _descText.text = Lang.Get("前往<color=#ffcc00>{0}</color>争夺星球，获取<color=#00FF00>占领积分</color>", WorldPositionCul.PlanetId_to_WorldPos_String(_actInfo.FocusPos));

            _rewardGo.SetActive(false);
            _fightGo.SetActive(true);

            ShowBuildingList();
        }
    }

    private void ShowBoxUI(int index)
    {
        int condition = Cfg.SupremacyReward.GetData(index).condition;
        cfg_supremacy_reward cfg = Cfg.SupremacyReward.GetStateData(condition, _actInfo.MapStep);
        if (!string.IsNullOrEmpty(cfg.reward))
        {
            P_Item[] rewards = GlobalUtils.ParseItem(cfg.reward);
            DialogManager.ShowAsyn<_D_ShowRewards>(d => { d?.PreviewBox(rewards.ToList(), Lang.Get("积分奖励"), Lang.Get("达到{0}积分后开启势力宝箱有概率获得下列道具", cfg.condition)); });
        }
    }

    private void DefineRank(List<P_Act2087Rank> data)
    {
        for (int i = 0; i < _rankGo.Length; i++)
        {
            Transform trans = _rankGo[i].transform;
            trans.Find<Text>("TextRank").text = data[i].rank.ToString();
            trans.Find<Text>("TextName").text = Lang.Get(data[i].state_name);
            trans.Find<Text>("TextScore").text = data[i].score.ToString();
            trans.Find<Image>("Icon").sprite = _spriteHotIcon[data[i].fire_status - 1];
            trans.Find<Text>("TextRewardCount").text = data[i].show_free_times.ToString();
            string battleStatus;
            switch (data[i].fire_status)
            {
                case 1:
                    battleStatus = Lang.Get("平静");
                    break;
                case 2:
                    battleStatus = Lang.Get("正常");
                    break;
                case 3:
                    battleStatus = Lang.Get("火热");
                    break;
                default:
                    battleStatus = Lang.Get("正常");
                    break;
            }
            var statusBtn = trans.Find<Button>("Icon");
            statusBtn.onClick.SetListener(() =>
            {
                DialogManager.ShowAsyn<_D_Tips_HelpDesc>(d => { d?.OnShow(Lang.Get("战斗状态"), battleStatus, statusBtn.transform.position, Direction.RightDown, 250, new Vector2(-25, -25)); });
            });
        }
    }

    public override void UpdateTime(long obj)
    {
        base.UpdateTime(obj);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!gameObject.activeSelf)
            return;

        if (_actinfo == null)
            return;

        if (!_actinfo.IsDuration())
        {
            var leftTime = _actinfo._data.startts - TimeManager.ServerTimestamp;
            if (leftTime < 0)
                leftTime = 0;
            _timeText.text = Lang.Get("开启倒计时 {0}", GLobal.TimeFormat(leftTime, true));
            return;
        }

        if (_actinfo.LeftTime >= 0)
        {
            _timeText.text = GlobalUtils.ActivityLeftTime(_actinfo.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }
}


public class Act2087ModelItem : ListItem
{
    private Text _nameText;
    private Image _icon;
    private int _id;

    public override void OnCreate()
    {
        _nameText = transform.Find<Text>("TextName");
        _icon = transform.Find<Image>("Icon");
        _icon.GetComponent<Button>().onClick.SetListener(On_iconClick);
    }
    private void On_iconClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        DialogManager.ShowAsyn<_D_StellarBDetail>(On_iconDialogShowAsynCB);
    }
    private void On_iconDialogShowAsynCB(_D_StellarBDetail d)
    {
        d?.OnShow(_id);
    }
    public void RefreshNotice(int id)
    {
        _id = id;
        UIHelper.SetImageSprite(_icon, Cfg.Stellar.GetBuildSpritePath(id, Uinfo.Instance.State.Info.state));
        _nameText.text = Cfg.Stellar.GetSBuildingData(id).name;
    }
}