using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2086_UI : ActivityUI
{
    private Text _textCountDown;
    private Button _btnChallenge;
    private Button _btnJusticeAlliance;
    private Button _btnHelp;
    private Reward[] _rewards;
    private Text _textTimesOfGetExtraGift;
    private Text _textLeftDiscountTimes;
    private Button _btnOpenOne;
    private Button _btnOpenTen;
    private GameObject _oneDiscountKr;
    private GameObject _tenDiscountKr;
    private GameObject _openTenNotEnough;
    private GameObject _tenNeedKr;
    private ActInfo_2086 _actInfo;
    private bool _isShowing;
    private GameObject _redPointChallenge;
    private GameObject _redPointJusticeAlliance;
    private GameObject _boxEffect;
    private _D_ActCalendar _rootDialog;
    private int _mapStep;

    public override void OnCreate()
    {
        InitRef();
        InitBtn();
        //InitListener();
    }

    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += RefreshTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(RefreshUi);
    }

    private void InitBtn()
    {
        _btnHelp.onClick.AddListener(On_btnHelpClick);
        _btnChallenge.onClick.AddListener(On_btnChallengeClick);
        _btnJusticeAlliance.onClick.AddListener(On_btnJusticeAllianceClick);
        _btnOpenOne.onClick.AddListener(OpenOne);
        _btnOpenTen.onClick.AddListener(OpenTen);
    }
    private void On_btnHelpClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnHelpDialogShowAsynCB);
    }
    private void On_btnHelpDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        HelpType helpId = GetHelpId();
        d?.OnShow(helpId, _btnHelp.transform.position, Direction.LeftDown, 350);
    }
    private void On_btnChallengeClick()
    {
        DialogManager.ShowAsyn<_D_Act2086DailyChallenge>(On_btnChallengeDialogShowAsynCB);
    }
    private void On_btnChallengeDialogShowAsynCB(_D_Act2086DailyChallenge d)
    {
        d?.OnShow();
    }
    private void On_btnJusticeAllianceClick()
    {
        DialogManager.ShowAsyn<_D_Act2086JusticeAlliance>(On_btnJusticeAllianceClick);
    }
    private void On_btnJusticeAllianceClick(_D_Act2086JusticeAlliance d)
    {
        d?.OnShow();
    }
    private HelpType GetHelpId()
    {
        _mapStep = WorldInfo.Inst.GetMapStep();
        if (_mapStep >= 60 && _mapStep < 93)
        {
            return HelpType.Act2086MapStep60;
        }
        else if (_mapStep >= 93 && _mapStep < 110)
        {
            return HelpType.Act2086MapStep93;
        }
        else if (_mapStep >= 110 && _mapStep < 150)
        {
            return HelpType.Act2086MapStep110;
        }
        else if (_mapStep >= 150)
        {
            return HelpType.Act2086MapStep150;
        }
        else
        {
            throw new Exception("error map step");
        }
    }

    private void OpenTen()
    {
        _actInfo.OfferOpen(1, ShowGetRewards);
    }

    private void ShowGetRewards()
    {
        _rootDialog.SetBlock(true);
        StartCoroutine(ShowRewards());
    }

    private IEnumerator ShowRewards()
    {
        _boxEffect.SetActive(true);
        yield return new _WaitForSeconds(0.5f);
        _rootDialog.SetBlock(false);
        _boxEffect.SetActive(false);
        DialogManager.ShowAsyn<_D_Act2086GottenRewards>(OnShowRewardsDialogShowAsynCB);
        RefreshAfterDraw();
    }

    private void OnShowRewardsDialogShowAsynCB(_D_Act2086GottenRewards d)
    {
        d.OnShow(_actInfo.UniqueInfo.ExtraReward, _actInfo.UniqueInfo.RareRewards, _actInfo.UniqueInfo.AllRewards);
    }

    private void OpenOne()
    {
        _actInfo.OfferOpen(0, ShowGetRewards);
    }

    private void InitRef()
    {
        _textCountDown = transform.FindText("TextCountDown");
        _btnChallenge = transform.FindButton("Btn_DailyChallenge");
        _btnJusticeAlliance = transform.FindButton("Btn_JusticeAlliance");
        _btnHelp = transform.FindButton("_btnManual");
        _rewards = new[]{new Reward(transform.Find("IconList/RareReward_1")),
            new Reward(transform.Find("IconList/RareReward_2")),
            new Reward(transform.Find("IconList/RareReward_3")) };
        _textTimesOfGetExtraGift = transform.FindText("TimesOfGetExtraGift/Text");
        _textLeftDiscountTimes = transform.FindText("Text_LeftDiscountTimes");
        _btnOpenOne = transform.FindButton("Btn_OpenOne");
        _btnOpenTen = transform.FindButton("Btn_OpenTen");
        _oneDiscountKr = transform.Find("OneNeedKr/Mask").gameObject;
        _tenDiscountKr = transform.Find("TenNeedKr/Mask").gameObject;
        _openTenNotEnough = transform.Find("TenNeedKr/NotEnough").gameObject;
        _tenNeedKr = transform.Find("TenNeedKr/TextKr").gameObject;
        _redPointChallenge = _btnChallenge.transform.Find("RedPoint").gameObject;
        _redPointJusticeAlliance = _btnJusticeAlliance.transform.Find("RedPoint").gameObject;
        _boxEffect = transform.Find("RewardBox/PFB_D_MissionAccomplished_ani2").gameObject;
        _actInfo = ActivityManager.Instance.GetActivityInfo(ActivityID.OrderTreasure) as ActInfo_2086;
        _rootDialog = DialogManager.GetInstanceOfDialog<_D_ActCalendar>();
    }

    public override void OnShow()
    {
        _isShowing = true;
        RefreshAll();
    }

    private void RefreshAll()
    {
        UpdateTime(TimeManager.ServerTimestamp);
        Refresh3RareRewards();
        RefreshAfterDraw();
    }

    private void RefreshRemind()
    {
        _redPointJusticeAlliance.SetActive(_actInfo.HasCanExchange);
        _redPointChallenge.SetActive(_actInfo.ChallengeCompleted && _actInfo.CanOpenFree || _actInfo.DailyChallengeRefresh);
        EventCenter.Instance.RemindActivity.Broadcast(ActivityID.OrderTreasure, _actInfo.IsAvaliable());
    }

    private void RefreshAfterDraw()
    {
        RefreshLastCount();
        int count = _actInfo.UniqueInfo.OfferCount;
        _textLeftDiscountTimes.text =
            Lang.Get("剩余优惠次数<color=#00FF00>{0}</color>次", count);
        _textLeftDiscountTimes.gameObject.SetActive(count > 0);
        if (count > 0)
        {
            _oneDiscountKr.SetActive(true);
            if (count == 10)
            {
                _tenDiscountKr.SetActive(true);
                _openTenNotEnough.SetActive(false);
            }
            else
            {
                _tenDiscountKr.SetActive(false);
                _openTenNotEnough.SetActive(true);
                _tenNeedKr.SetActive(false);
                _btnOpenTen.interactable = false;
            }
        }
        else
        {
            _oneDiscountKr.SetActive(false);
            _tenDiscountKr.SetActive(false);
            _openTenNotEnough.SetActive(false);
            _tenNeedKr.SetActive(true);
            _btnOpenTen.interactable = true;
        }

        RefreshRemind();
    }

    private void Refresh3RareRewards()
    {
        int len = _rewards.Length;
        for (int i = 0; i < len; i++)
        {
            P_Item item = _actInfo.UniqueInfo.SpecialRewards[i];
            _rewards[i].Refresh(item, 0, 1);
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        _isShowing = false;
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != ActivityID.OrderTreasure)
        {
            return;
        }

        RefreshLastCount();
        RefreshRemind();
    }

    private void RefreshLastCount()
    {
        _textTimesOfGetExtraGift.text =
            Lang.Get("打开<color=#FF0000>{0}</color>次后获得<color=#00FF00>额外</color>奖励", _actInfo.UniqueInfo.LastCount);
    }

    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!_isShowing)
        {
            return;
        }
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
    public override void OnDestroy()
    {
        base.OnClose();
        //TimeManager.Instance.TimePassSecond -= RefreshTime;
        //EventCenter.Instance.UpdateActivityUI.RemoveListener(RefreshUi);
    }
}

public class Reward : ListItem
{
    private int _itemId;
    private int _itemNum;
    private Image _icon;
    private Image _qua;
    private Text _countText;
    private GameObject _tag;
    private Button _btn;
    private GameObject _extraGotten;
    private Text _nameText;

    public Reward(Transform transform)
    {
        gameObject = transform.gameObject;
        OnCreate();
    }

    public Reward()
    {

    }
    public override void OnCreate()
    {
        InitRef();
        InitBtn();
    }

    private void InitBtn()
    {
        _btn.onClick.AddListener(OnInitBtnClick);
    }
    private void OnInitBtnClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(OnInitBtnDialogShowAsynCB);
    }
    private void OnInitBtnDialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(_itemId, _itemNum, transform.position);
    }
    private void InitRef()
    {
        _icon = transform.FindImage("Icon");
        _qua = transform.FindImage("Qua");
        _countText = transform.FindText("CountText");
        _tag = transform.Find("RareTag").gameObject;
        _btn = transform.GetComponent<Button>();
        _extraGotten = transform.Find("ExtraGotten").gameObject;
        _nameText = transform.FindText("NameText");
    }
    /// <summary>
    /// type 0:普通礼物 1:稀有 2:额外获得
    /// </summary>
    /// <param name="item"></param>
    /// <param name="type"></param>
    public void Refresh(P_Item item, int dialogType, int type = 1)
    {
        _itemId = item.Id;
        _itemNum = item.Num;
        _countText.text = $"x{item.Num}";
        Cfg.Item.SetItemIcon(_icon, _itemId);
        _qua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(_itemId));
        _tag.SetActive(type == 1);
        _extraGotten.SetActive(type == 2);
        _nameText.gameObject.SetActive(false);
        if (dialogType == 1)
        {
            _nameText.text = Cfg.Item.GetItemName(_itemId);
            _nameText.gameObject.SetActive(true);
        }
    }
}
