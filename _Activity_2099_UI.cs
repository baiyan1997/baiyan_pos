using System;
using System.Collections.Generic;
using System.ComponentModel;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2099_UI : ActivityUI
{
    private ActInfo_2099 _actInfo;
    private Text _timeText;

    private Button _openPage1Button;//培养台页面
    private Button _openPage2Button;//礼包页面
    private Image _button1Image;
    private Image _button2Image;
    private Button _butttonHelp;

    private string _selectSpritePath;//选中颜色
    private string _unselectSpritePath;//未选中颜色
    public override void OnCreate()
    {
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        InitPages();
        InitRef();
        //InitListener();
    }

    private void InitRef()
    {
        _selectSpritePath = "Button/btn_801";
        _unselectSpritePath = "Button/btn_802";
        _butttonHelp = transform.Find<Button>("BtnHelp");
        _openPage1Button = transform.Find<Button>("Menu/Button1");
        _openPage2Button = transform.Find<Button>("Menu/Button2");
        _timeText = transform.Find<Text>("CountDown");
        _button1Image = _openPage1Button.transform.GetComponent<Image>();
        _button2Image = _openPage2Button.transform.GetComponent<Image>();
        _butttonHelp.onClick.AddListener(On_butttonHelpClick);
        _openPage1Button.onClick.AddListener(On_openPage1ButtonClick);
        _openPage2Button.onClick.AddListener(On_openPage2ButtonClick);
    }
    private void On_butttonHelpClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_butttonHelpDialogShowAsynCB);
    }
    private void On_butttonHelpDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2099, _butttonHelp.transform.position, Direction.LeftDown, 350);
    }
    private void On_openPage1ButtonClick()
    {
        SpaceTimeCapsuleTablePage.Instance.OnShow();
        UIHelper.SetImageSprite(_button1Image, _selectSpritePath);
        UIHelper.SetImageSprite(_button2Image, _unselectSpritePath);
        CapsuleGiftBagPage.Instance.OnClose();
        SelectSpaceTimeCapsulePage.Instance.OnClose();
        SpaceTimeCapsuleTasks.Instance.OnClose();
        SpaceTimeCapsuleSpaceExchangePanel.Instance.OnClose();
    }
    private void On_openPage2ButtonClick()
    {
        CapsuleGiftBagPage.Instance.OnShow();
        UIHelper.SetImageSprite(_button1Image, _unselectSpritePath);
        UIHelper.SetImageSprite(_button2Image, _selectSpritePath);
        SpaceTimeCapsuleTablePage.Instance.OnClose();
        SelectSpaceTimeCapsulePage.Instance.OnClose();
        SpaceTimeCapsuleTasks.Instance.OnClose();
        SpaceTimeCapsuleSpaceExchangePanel.Instance.OnClose();
    }

    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        EventCenter.Instance.Act2099OpenGiftPage.AddListener(ToGiftPage);
        EventCenter.Instance.AddPushListener(OpcodePush.ACT2099_BUYGIFT, UpdateOpcAct2099);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.Act2099OpenGiftPage.RemoveListener(ToGiftPage);
        EventCenter.Instance.RemovePushListener(OpcodePush.ACT2099_BUYGIFT, UpdateOpcAct2099);
    }

    private void InitPages()
    {
        SpaceTimeCapsuleTablePage.Instance.OnCreate(transform.Find("Main_01"));
        CapsuleGiftBagPage.Instance.OnCreate(transform.Find("Main_02"));
        SelectSpaceTimeCapsulePage.Instance.OnCreate(transform.Find("Main_Tips01"));
        SpaceTimeCapsuleSpaceExchangePanel.Instance.OnCreate(transform.Find("Main_Tips02"));
        SpaceTimeCapsuleTasks.Instance.OnCreate(transform.Find("Main_Tips03"));
    }
    public override void OnShow()
    {
        UpdateTime(0);
        UIHelper.SetImageSprite(_button1Image, _selectSpritePath);
        UIHelper.SetImageSprite(_button2Image, _unselectSpritePath);
        SpaceTimeCapsuleTablePage.Instance.OnShow();
    }

    private void UpdateOpcAct2099(int opcode, string data)
    {
        EventCenter.Instance.Act2099OpenGiftPage.RemoveListener(ToGiftPage);
    }

    public void ToGiftPage(int id)
    {

        CapsuleGiftBagPage.Instance.OnShow(id);
        UIHelper.SetImageSprite(_button1Image, _unselectSpritePath);
        UIHelper.SetImageSprite(_button2Image, _selectSpritePath);
        SpaceTimeCapsuleTablePage.Instance.OnClose();
        SelectSpaceTimeCapsulePage.Instance.OnClose();
        SpaceTimeCapsuleTasks.Instance.OnClose();
        SpaceTimeCapsuleSpaceExchangePanel.Instance.OnClose();

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
        CapsuleGiftBagPage.Instance.OnClose();
        SpaceTimeCapsuleTablePage.Instance.OnClose();
        SelectSpaceTimeCapsulePage.Instance.OnClose();
        SpaceTimeCapsuleTasks.Instance.OnClose();
        SpaceTimeCapsuleSpaceExchangePanel.Instance.OnClose();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CapsuleGiftBagPage.Instance.OnDestroy();
        SpaceTimeCapsuleTasks.Instance.OnDestroy();
        SpaceTimeCapsuleSpaceExchangePanel.Instance.OnDestroy();
        SpaceTimeCapsuleTablePage.Instance.OnDestroy();
        SelectSpaceTimeCapsulePage.Instance.OnDestroy();
        _actInfo = null;
    }
}
//培养台页面
public class SpaceTimeCapsuleTablePage : Singleton<SpaceTimeCapsuleTablePage>
{

    private Button _exchangeCapsuleButton;

    private Button _showCapsuleCoinButton;

    private Transform _transform;
    private SpacetimeCapsuleItem[] _spacetimeCapsuleItems;

    private ActInfo_2099 _actInfo;
    public void OnCreate(Transform transform)
    {
        _transform = transform;
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        OnClose();
        _spacetimeCapsuleItems = new[]
        {
            _transform.Find("01").gameObject.AddBehaviour<SpacetimeCapsuleItem>(),
            _transform.Find("02").gameObject.AddBehaviour<SpacetimeCapsuleItem>(),
            _transform.Find("03").gameObject.AddBehaviour<SpacetimeCapsuleItem>(),
        };
        _exchangeCapsuleButton = _transform.Find<Button>("Button1");
        _showCapsuleCoinButton = _transform.Find<Button>("Button2");
        _exchangeCapsuleButton.onClick.AddListener(OpenExchangeCapsulePage);
        _showCapsuleCoinButton.onClick.AddListener(OpenGetCapsulePage);

        InitEvent();
    }

    private void InitEvent()
    {
        EventCenter.Instance.UpdateActivityUI.AddListener(Refresh);
    }

    private void UnInitEvent()
    {
        EventCenter.Instance.UpdateActivityUI.RemoveListener(Refresh);
    }

    public void OnShow()
    {
        _transform.gameObject.SetActive(true);
        Refresh(2099);
    }
    private void Refresh(int aid)
    {
        if (aid != 2099)
        {
            return;
        }
        //刷新培养台
        RefreshCapsuleTable();

    }
    //刷新培养台
    private void RefreshCapsuleTable()
    {
        var tableInfo = _actInfo.GetCultureTableInfo();
        for (int i = 0; i < tableInfo.Count; i++)
        {
            _spacetimeCapsuleItems[i].Refresh(tableInfo[i]);
        }
    }
    //打开胶囊购买页面
    private void OpenExchangeCapsulePage()
    {
        SpaceTimeCapsuleSpaceExchangePanel.Instance.OnShow();
    }
    //打开胶囊获取页面
    private void OpenGetCapsulePage()
    {
        SpaceTimeCapsuleTasks.Instance.OnShow();
    }
    public void OnClose()
    {
        _transform.gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        UnInitEvent();
        _spacetimeCapsuleItems = null;
        _actInfo = null;
    }
}
//胶囊礼包页面
public class CapsuleGiftBagPage : Singleton<CapsuleGiftBagPage>
{
    private ActInfo_2099 _actInfo;
    private Transform _transform;
    private ListView _listView;
    private RectTransform _content;
    public void OnCreate(Transform transform)
    {
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _transform = transform;
        _listView = ListView.Create<CapsuleGift>(_transform.Find("Scroll View"));
        _content = transform.Find<RectTransform>("Scroll View/Viewport/Content");
        InitEvent();
        OnClose();
    }
    private void InitEvent()
    {
        EventCenter.Instance.UpdateActivityUI.AddListener(RefreshUI);
    }

    private void UnInitEvent()
    {
        EventCenter.Instance.UpdateActivityUI.RemoveListener(RefreshUI);
    }
    public void OnShow(int id = 0)
    {
        _transform.gameObject.SetActive(true);
        Refresh(id);
    }

    private void RefreshUI(int aid)
    {
        if (aid != 2099)
        {
            return;
        }
        Refresh();
    }
    public void Refresh(int id = 0)
    {
        var giftInfo = _actInfo.GetGiftInfo();
        _listView.Clear();
        for (int i = 0; i < giftInfo.Count; i++)
        {
            var one = giftInfo[i];
            var data = Cfg.Activity2099.GetPackage(one.id, _actInfo.GetStep());
            if (data != null)
            {
                _listView.AddItem<CapsuleGift>().Refresh(one, id, ToGiftPosition);
            }
        }
    }
    public void OnClose()
    {
        _transform.gameObject.SetActive(false);
    }

    private void ToGiftPosition(int num)
    {
        float length = (num - 1) * 190.0f;
        if (length > _content.rect.height)
        {
            length = _content.rect.height;
        }
        _content.DOLocalMoveY(length, 0.5f);
    }

    public void OnDestroy()
    {
        UnInitEvent();
        _actInfo = null;
    }
}

//任务列表
public class SpaceTimeCapsuleTasks : Singleton<SpaceTimeCapsuleTasks>
{
    private Transform _transform;
    private GameObject _gameObject;
    private ActInfo_2099 _actInfo;
    private ListView _listView;
    private Button _closeButton;
    public void OnCreate(Transform transform)
    {
        _transform = transform;
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _listView = ListView.Create<SpaceTimeCapsuleTaskItem>(_transform.Find("Scroll View"));
        _closeButton = transform.Find<Button>("Title/Button");
        _closeButton.onClick.AddListener(OnClose);
        InitEvent();
    }
    private void InitEvent()
    {
        EventCenter.Instance.UpdateActivityUI.AddListener(Refresh);
    }

    private void UnInitEvent()
    {
        EventCenter.Instance.UpdateActivityUI.RemoveListener(Refresh);
    }
    public void OnShow()
    {
        _transform.gameObject.SetActive(true);
        Refresh(2099);
    }

    public void Refresh(int aid)
    {
        if (aid != 2099)
        {
            return;
        }
        var taskInfo = _actInfo.GetTaskInfo();
        _listView.Clear();
        for (int i = 0; i < taskInfo.Count; i++)
        {
            var one = taskInfo[i];
            _listView.AddItem<SpaceTimeCapsuleTaskItem>().Refresh(one);
        }
    }

    public void OnClose()
    {
        _transform.gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        UnInitEvent();
        _actInfo = null;
    }
}

//胶囊商店
public class SpaceTimeCapsuleSpaceExchangePanel : Singleton<SpaceTimeCapsuleSpaceExchangePanel>
{
    private Transform _transform;
    private Text _timeText;
    private ListView _listView;

    private ActInfo_2099 _actInfo;

    private Button _closeButton;
    private Text _coinNum;

    public void OnCreate(Transform transform)
    {
        _transform = transform;
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _listView = ListView.Create<ExchangeSpaceTimeCapsuleItem>(_transform.Find("Scroll View"));
        _timeText = transform.Find<Text>("Text/Text");
        _closeButton = transform.Find<Button>("Title/Button");
        _coinNum = transform.Find<Text>("Title/Text");
        _closeButton.onClick.AddListener(OnClose);

        InitEvent();
    }

    private void InitEvent()
    {
        TimeManager.Instance.TimePassSecond += UpdateTime;
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdateBoxCoin);
        EventCenter.Instance.UpdateActivityUI.AddListener(Refresh);
    }

    private void UnInitEvent()
    {
        TimeManager.Instance.TimePassSecond -= UpdateTime;
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdateBoxCoin);
        EventCenter.Instance.UpdateActivityUI.RemoveListener(Refresh);
    }
    private void UpdateBoxCoin()
    {
        _coinNum.text = BagInfo.Instance.GetItemCount(ItemId.Act2099Coin).ToString();
    }
    public void OnShow()
    {
        _transform.gameObject.SetActive(true);
        UpdateTime(TimeManager.ServerTimestamp);
        Refresh(2099);
    }

    public void Refresh(int aid)
    {
        if (aid == 2099)
        {
            UpdateBoxCoin();
            var capsules = Cfg.Activity2099.GetAllCapsule();
            List<Act2099ExchangeCapsule> info = _actInfo.GetCapsuleShopInfo();
            _listView.Clear();
            for (int i = 0; i < capsules.Count; i++)
            {
                var one = capsules[i];
                int num = 0;
                if (info != null)
                {
                    for (int k = 0; k < info.Count; k++)
                    {
                        var oneInfo = info[k];
                        if (oneInfo.id == one.id)
                        {
                            num = oneInfo.exchange_num;
                        }
                    }
                }
                if (one.capsule_coin > 0)
                {
                    _listView.AddItem<ExchangeSpaceTimeCapsuleItem>().Refresh(one, one.limit - num);
                }
            }
        }
    }

    private void UpdateTime(long currentTime)
    {
        if (!_transform.gameObject.activeSelf)
            return;

        if (_actInfo == null)
            return;
        var leftTime = _actInfo.GetRefreshTime() - currentTime;
        if (leftTime >= 0)
        {
            _timeText.text = Lang.Get("余量刷新倒计时:{0}", GLobal.TimeFormat(leftTime));
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }

    public void OnClose()
    {
        _transform.gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        UnInitEvent();
        _actInfo = null;
    }
}
//培养台
public class SpacetimeCapsuleItem : JDBehaviour
{


    private Button _selectCapsuleButton;
    private Image[] _icons;
    private Image _block;
    private Text _cost;
    private Text _time;

    //解锁按钮
    private Button _unlockButton;
    //解锁时回调
    private Action _unlockCallback;
    //是否解锁
    private bool _isFinish;

    private Button _finishNowButton;

    private Button _getRewardButton;

    private Act2099CultureTable _info;

    private ActInfo_2099 _actInfo;

    private GameObject _lockImage;

    private Text _textUnlockButton;
    public override void Awake()
    {
        base.Awake();
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _selectCapsuleButton = transform.Find<Button>("SelectButton");
        _finishNowButton = transform.Find<Button>("FinishButton");
        _unlockButton = transform.Find<Button>("UnlockButton");
        _getRewardButton = transform.Find<Button>("GetButton");
        _block = transform.Find<Image>("Image/Image");
        _time = transform.Find<Text>("Time");
        _lockImage = transform.Find("pfb_Chain").gameObject;
        _cost = transform.Find<Text>("FinishButton/Text");
        _textUnlockButton = _unlockButton.transform.Find<Text>("Text");
        _icons = new Image[3];
        for (int i = 1; i <= 3; i++)
        {
            _icons[i - 1] = transform.Find<Image>("Image/Icon" + i);
        }
        _selectCapsuleButton.onClick.AddListener(OpenSelectCapsulePage);
        _getRewardButton.onClick.AddListener(On_getRewardButtonClick);
        _finishNowButton.onClick.AddListener(Finish);
        _unlockButton.onClick.AddListener(On_unlockButtonClick);
        TimeManager.Instance.TimePassSecond += OnTimePass;
    }
    private void On_getRewardButtonClick()
    {
        _actInfo.GetCapsuleReward(_info.cultivar_id, OnCapsuleRewardCB);
    }
    private void OnCapsuleRewardCB(string rewards, Act2099CultureTable data)
    {
        GetRewards(rewards);
        Refresh(data);
    }
    private void On_unlockButtonClick()
    {
        var cultivator = Cfg.Activity2099.Get2099ByCultivarId(_info.cultivar_id);
        if (cultivator.unlock_type == 2)
        {
            //打开购买礼包界面
            EventCenter.Instance.Act2099OpenGiftPage.Broadcast(cultivator.need_gold);
        }
        else if (cultivator.unlock_type == 1)
        {
            if (BagInfo.Instance.GetItemCount(ItemId.Gold) < cultivator.need_gold)
            {
                DialogManager.ShowAsyn<_D_JumpConfirm>(d => { d?.OnShow(JumpType.Kr, cultivator.need_gold); });
            }
            else
            {
                var alertStr = Lang.Get("是否消耗{0}氪晶解锁高等培养台", cultivator.need_gold);
                var temp = Alert.YesNo(alertStr);
                temp.SetYesCallback(() =>
                {
                    _actInfo.UnlockCulture(_info.cultivar_id, (data) =>
                    {
                        temp.Close();
                        Refresh(data);
                    });
                });
                temp.SetNoCallback(temp.Close);
            }
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        TimeManager.Instance.TimePassSecond -= OnTimePass;
        _info = null;
        _actInfo = null;
    }
    public void Refresh(Act2099CultureTable info)
    {
        _info = info;
        var cultivator = Cfg.Activity2099.Get2099ByCultivarId(_info.cultivar_id);
        _time.gameObject.SetActive(false);
        _selectCapsuleButton.gameObject.SetActive(false);
        if (cultivator.unlock_type == 1)
        {
            _textUnlockButton.text = cultivator.need_gold.ToString();
        }
        else if (cultivator.unlock_type == 2)
        {
            string str = "购买";
            _textUnlockButton.text = Lang.Get("{0}培养台原匙解锁", str);
        }

        for (int i = 0; i < _icons.Length; i++)
        {
            var one = _icons[i];
            one.gameObject.SetActive(false);
        }
        if (_info.unlock_value == 0)//未解锁
        {
            _lockImage.gameObject.SetActive(true);
            _block.gameObject.SetActive(false);
            _selectCapsuleButton.gameObject.SetActive(false);
            _finishNowButton.gameObject.SetActive(false);
            _getRewardButton.gameObject.SetActive(false);
            _unlockButton.gameObject.SetActive(true);

        }
        else//已解锁
        {

            _lockImage.gameObject.SetActive(false);
            _unlockButton.gameObject.SetActive(false);
            if (_info.capsule_id > 0)
            {
                var capsule = Cfg.Activity2099.Get2099CapsuleById(_info.capsule_id);
                _cost.text = Lang.Get("{0} 立即完成", capsule.need_gold);
                _block.gameObject.SetActive(false);
                _icons[capsule.type - 1].gameObject.SetActive(true);
                _isFinish = (_info.end_ts - TimeManager.ServerTimestamp) <= 0;
                _finishNowButton.gameObject.SetActive(!_isFinish);
                _getRewardButton.gameObject.SetActive(_isFinish);
                if (_isFinish)
                {
                    _time.text = Lang.Get("培养完成");
                }
                _time.gameObject.SetActive(true);
            }
            else
            {
                _block.gameObject.SetActive(true);
                _selectCapsuleButton.gameObject.SetActive(true);
                _finishNowButton.gameObject.SetActive(false);
                _getRewardButton.gameObject.SetActive(false);
            }
        }
        OnTimePass(TimeManager.ServerTimestamp);
    }
    private void OnTimePass(long nowTs)
    {
        //
        var leftTs = _info.end_ts - nowTs;
        if (leftTs > 0 && _info.capsule_id > 0)
        {
            _time.text = Lang.Get("培养倒计时:{0}", GLobal.TimeFormat(leftTs));
        }
        else if (leftTs <= 0 && _info.capsule_id > 0)
        {
            if (!_getRewardButton.gameObject.activeSelf)
            {
                _finishNowButton.gameObject.SetActive(false);
                _getRewardButton.gameObject.SetActive(true);

            }
            _time.text = Lang.Get("培养完成");
        }
    }
    //打开选择胶囊页面
    private void OpenSelectCapsulePage()
    {
        SelectSpaceTimeCapsulePage.Instance.OnShow(_info.cultivar_id);
    }

    private void Finish()
    {
        var capsule = Cfg.Activity2099.Get2099CapsuleById(_info.capsule_id);
        var alertStr = Lang.Get("是否消耗{0}氪晶立即完成培养", capsule.need_gold);
        var temp = Alert.YesNo(alertStr);
        temp.SetYesCallback(() =>
        {
            if (BagInfo.Instance.GetItemCount(ItemId.Gold) < capsule.need_gold)
            {
                DialogManager.ShowAsyn<_D_JumpConfirm>(d => { d?.OnShow(JumpType.Kr, capsule.need_gold); });
            }
            else
            {
                _actInfo.FinishNow(_info.cultivar_id, (info) =>
                {
                    Refresh(info);
                });
            }
            temp.Close();
        });
        temp.SetNoCallback(() =>
        {
            temp.Close();
        });
    }

    private void GetRewards(string rewards)
    {
        var capsule = Cfg.Activity2099.Get2099CapsuleById(_info.capsule_id);
        List<P_Item> rewardItems = new List<P_Item>();
        P_Item[] tempItems = GlobalUtils.ParseItem(rewards);
        for (int i = 0; i < tempItems.Length; i++)
        {
            var one = tempItems[i];
            rewardItems.Add(one);
        }
        DialogManager.ShowAsyn<_D_ShowRewards>(d =>
        {
            d?.ShowCustonRewards(rewardItems,
capsule.name,
Lang.Get("恭喜您获得:"),
Lang.Get("确定"));
        });
    }
}
//选择胶囊页面
public class SelectSpaceTimeCapsulePage : Singleton<SelectSpaceTimeCapsulePage>
{

    private Transform _transform;
    private GameObject _gameObject;
    private Text _timeText;
    private ListView _listView;
    private ActInfo_2099 _actInfo;
    private Button _closeButton;
    private GameObject _blockTag;
    private Text _titleName;
    public void OnCreate(Transform transform)
    {
        _transform = transform;
        _listView = ListView.Create<SelectCapsuleItem>(_transform.Find("Scroll View"));
        _closeButton = transform.Find<Button>("Title/Button");
        _titleName = transform.Find<Text>("Title/Text_Title");
        _blockTag = transform.Find("Bg/Text").gameObject;
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _closeButton.onClick.AddListener(OnClose);
    }
    public void OnShow(int tid)
    {
        _transform.gameObject.SetActive(true);
        var tempCul = Cfg.Activity2099.Get2099ByCultivarId(tid);
        _titleName.text = Lang.Get("{0}胶囊选择", tempCul.name);
        _listView.Clear();
        var capsules = Cfg.Activity2099.GetAllCapsule();
        for (int i = 0; i < capsules.Count; i++)
        {
            var one = capsules[i];
            cfg_act_2099_reward tempRewards = Cfg.Activity2099.GetRewardByCidAndTid(one.id, tid, _actInfo.GetStep());
            if (tempRewards == null)
            {
                continue;
            }
            var count = BagInfo.Instance.GetItemCount(one.item_id);
            if (count > 0)
            {
                _listView.AddItem<SelectCapsuleItem>().OnShow(one, tid);
            }
        }
        _blockTag.SetActive(_listView._listItems.Count <= 0);

    }

    public void OnClose()
    {
        _transform.gameObject.SetActive(false);
    }
    public void OnDestroy()
    {
        _actInfo = null;
    }

}
//商店 交换胶囊
public class ExchangeSpaceTimeCapsuleItem : ListItem
{
    private Button _buyButton;
    private Image _icon;
    private Image _qua;
    private Text _cost;
    private Text _num;
    private Text _name;
    private cfg_act_2099_capsule _item;
    private ActInfo_2099 _actInfo;
    private ListView _listView;

    public override void OnCreate()
    {
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _buyButton = transform.Find<Button>("Button");
        _icon = transform.Find<Image>("Bg1/Icon");
        _qua = transform.Find<Image>("Bg1/Img_qua");
        _name = transform.Find<Text>("Bg1/Title");
        _num = transform.Find<Text>("Bg1/Text");
        _cost = transform.Find<Text>("Button/Text");
        _buyButton.onClick.AddListener(BuyItem);
        _listView = ListView.Create<Act2099ExchangeCapsuleListItem>(transform.Find("Scroll View"));
    }

    public void Refresh(cfg_act_2099_capsule item, int lastNum)
    {
        _item = item;
        _name.text = item.name;
        _cost.text = item.capsule_coin.ToString();

        _qua.color = _ColorConfig.GetQuaColor(item.qua);
        Cfg.Item.SetItemIcon(_icon, item.item_id);
        if (lastNum <= 0)
        {
            _buyButton.interactable = false;
            _num.text = Lang.Get("今日售罄");

            if (item.limit == 0)
            {
                _buyButton.interactable = true;
                _num.text = Lang.Get("无限制");
            }
        }
        else
        {
            _buyButton.interactable = true;
            _num.text = Lang.Get("剩余:{0}", lastNum);
        }
        _listView.Clear();
        string tempReward = Cfg.Activity2099.GetRewardById(item.id, _actInfo.GetStep());
        P_Item[] capsuleReward = GlobalUtils.ParseItem(tempReward);

        for (int i = 0; i < capsuleReward.Length; i++)
        {
            var one = capsuleReward[i];
            _listView.AddItem<Act2099ExchangeCapsuleListItem>().Refresh(one);
        }
    }

    private void BuyItem()
    {
        var coinNum = BagInfo.Instance.GetItemCount(ItemId.Act2099Coin);
        var cfgCapsule = Cfg.Activity2099.Get2099CapsuleById(_item.id);
        if (coinNum < cfgCapsule.capsule_coin)
        {
            MessageManager.Show(Lang.Get("胶囊币不足!"));
        }
        else
        {
            //购买
            _actInfo.ExchangeCapsule(_item.id, OnExchangeCapsuleCB);
        }
    }
    private void OnExchangeCapsuleCB()
    {
        SpaceTimeCapsuleSpaceExchangePanel.Instance.Refresh(2099);
    }
}
//任务
public class SpaceTimeCapsuleTaskItem : ListItem
{
    private Button _goButton;
    private Button _getButton;
    private GameObject _blockButton;
    private GameObject _tag;
    private Text _name;

    private Text _num;
    private Image _icon;
    private Image _qua;
    private string _mission_click;

    private ActInfo_2099 _actInfo;

    private Act2099TaskInfo _info;
    public override void OnCreate()
    {

        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _goButton = transform.Find<Button>("Button1");
        _getButton = transform.Find<Button>("Button2");
        _blockButton = transform.Find("Button3").gameObject;
        _tag = transform.Find("Tag").gameObject;
        _name = transform.Find<Text>("Title");
        _num = transform.Find<Text>("Icon_01/Text");
        _icon = transform.Find<Image>("Icon_01/img_icon");
        _qua = transform.Find<Image>("Icon_01/Img_qua");


        _goButton.onClick.AddListener(On_goButtonClick);
        _getButton.onClick.AddListener(GetCoin);
    }
    private void On_goButtonClick()
    {
        MissionUtils.DoCustomFlow(_mission_click);
    }
    public void Refresh(Act2099TaskInfo info)
    {
        _info = info;
        var cfgInfo = Cfg.Activity2099.Get2099TaskById(info.tid);
        _mission_click = cfgInfo.click;
        _name.text = cfgInfo.name + string.Format("({0}/{1})", info.do_number, cfgInfo.need_count);
        Cfg.Item.SetItemIcon(_icon, ItemId.Act2099Coin);
        _qua.color = _ColorConfig.GetQuaColor(Cfg.Item.GetItemQua(ItemId.Act2099Coin));
        _num.text = "X" + cfgInfo.coin.ToString();
        if (info.finished == 1 && info.get_reward == 1)
        {
            _goButton.gameObject.SetActive(false);
            _getButton.gameObject.SetActive(false);
            _blockButton.gameObject.gameObject.SetActive(true);
            _tag.gameObject.SetActive(true);
        }
        else if (info.finished == 1 && info.get_reward == 0)
        {
            _goButton.gameObject.SetActive(false);
            _getButton.gameObject.SetActive(true);
            _blockButton.gameObject.gameObject.SetActive(false);
            _tag.gameObject.SetActive(false);
        }
        else
        {
            _goButton.gameObject.SetActive(true);
            _getButton.gameObject.SetActive(false);
            _blockButton.gameObject.gameObject.SetActive(false);
            _tag.gameObject.SetActive(false);
        }

    }

    private void GetCoin()
    {
        _actInfo.GetCapsuleCoin(_info.tid, OnGetCapsuleCoinCB);
    }
    private void OnGetCapsuleCoinCB(Act2099TaskInfo data)
    {
        Refresh(data);
    }
}
//备选胶囊
public class SelectCapsuleItem : ListItem
{
    private Image _icon;
    private Image _qua;
    private Text _name;

    private cfg_act_2099_capsule _item;

    private Button _selectButton;
    private Text _time;
    private Text _num;

    private ListView _listView;
    private ActInfo_2099 _actInfo;
    private int _tid;

    public override void OnCreate()
    {
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _listView = ListView.Create<Act2099CapsuleListItem>(transform.Find("Scroll View"));
        _icon = transform.Find<Image>("Bg1/Image");
        _qua = transform.Find<Image>("Bg1/Img_qua");
        _name = transform.Find<Text>("Bg2/Text");
        _num = transform.Find<Text>("Bg1/Text");
        _time = transform.Find<Text>("Bg2/TextCount");
        _selectButton = transform.Find<Button>("Button");

        _selectButton.onClick.AddListener(Select);

    }

    public void OnShow(cfg_act_2099_capsule item, int tid)
    {
        _item = item;
        _tid = tid;
        _num.text = Lang.Get("剩余{0}", BagInfo.Instance.GetItemCount(_item.item_id));
        Cfg.Item.SetItemIcon(_icon, _item.item_id);
        _qua.color = _ColorConfig.GetQuaColor(Cfg.Item.GetItemQua(_item.item_id));
        _name.text = item.name;
        _time.text = Lang.Get("培养需要时间: {0}", GLobal.TimeFormat(_item.ts));

        cfg_act_2099_reward tempRewards = Cfg.Activity2099.GetRewardByCidAndTid(_item.id, tid, _actInfo.GetStep());
        P_Item[] rewards = GlobalUtils.ParseItem(tempRewards.cultivar_info);

        _listView.Clear();
        for (int i = 0; i < rewards.Length; i++)
        {
            var one = rewards[i];
            _listView.AddItem<Act2099CapsuleListItem>().Refresh(one);
        }
    }

    public void Select()
    {
        _actInfo.StartCultureCapsule(_tid, _item.id, OnStartCultureCapsuleCB);
    }
    private void OnStartCultureCapsuleCB()
    {
        SelectSpaceTimeCapsulePage.Instance.OnClose();
        SpaceTimeCapsuleTablePage.Instance.OnShow();
    }
}

public class Act2099CapsuleListItem : ListItem
{
    private Image Icon;
    private Text Possibility;
    private Text Num;
    private Button Button;
    private Image Qua;
    private int id;
    public override void OnCreate()
    {
        Icon = transform.Find<Image>("Icon_01/img_icon");
        Qua = transform.Find<Image>("Icon_01/Img_qua");
        Num = transform.Find<Text>("Icon_01/Text");
        Button = transform.Find<Button>("Icon_01/img_icon");
        Possibility = transform.Find<Text>("Icon_01/text/Text");
        Button.onClick.AddListener(OnButtonClick);
    }
    private void OnButtonClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(OnButtonDialogShowAsynCB);
    }
    private void OnButtonDialogShowAsynCB(_D_ItemTip d)
    {
        var itemCount = BagInfo.Instance.GetItemCount(id);
        d?.OnShow(id, (int)itemCount, Button.transform.position);
    }
    public void Refresh(P_Item reward)
    {
        id = reward.Id;
        Cfg.Item.SetItemIcon(Icon, reward.Id);
        Qua.color = _ColorConfig.GetQuaColor(Cfg.Item.GetItemQua(reward.Id));
        Num.text = "X" + reward.Num.ToString();
        Possibility.text = Lang.Get("概率:{0}%", (float)reward.extra / 100.0f);
    }
}
public class Act2099ExchangeCapsuleListItem : ListItem
{
    private Image Icon;
    private Text Num;
    private Button Button;
    private Image Qua;
    private int id;
    public override void OnCreate()
    {
        Icon = transform.Find<Image>("Icon_01/img_icon");
        Qua = transform.Find<Image>("Icon_01/Img_qua");
        Num = transform.Find<Text>("Icon_01/Text");
        Button = transform.Find<Button>("Icon_01/img_icon");
        Button.onClick.AddListener(OnButtonClick);
    }
    private void OnButtonClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(OnButtonDialogShowAsynCB);
    }
    private void OnButtonDialogShowAsynCB(_D_ItemTip d)
    {
        var itemCount = BagInfo.Instance.GetItemCount(id);
        d?.OnShow(id, (int)itemCount, Button.transform.position);
    }

    public void Refresh(P_Item reward)
    {
        id = reward.Id;
        Cfg.Item.SetItemIcon(Icon, reward.Id);
        Qua.color = _ColorConfig.GetQuaColor(Cfg.Item.GetItemQua(reward.Id));
        Num.text = "X" + reward.Num.ToString();
    }
}
public class CapsuleGift : ListItem
{

    private Text _textName;

    private Button _buyButton;
    private Text _cost;
    private GameObject _blockTag;

    private Text _num;
    //private CapsuleRewardItem[] _rewardItems;

    private cfg_act_2099_package _data;

    private ActInfo_2099 _actInfo;
    private Act2099GiftInfo _item;

    private Action<int> _callback;


    private int _pid;
    //public GameObject Object;
    private Image _icon;
    private Button _button;
    private Text _pNum;
    private Image _qua;
    //private class CapsuleRewardItem
    //{
    //    public int Id;
    //    public GameObject Object;
    //    public Image Icon;
    //    public Button Button;
    //    public Text Num;
    //    public Image Qua;
    //}
    private GameObject _tag;
    private ListView _listView;
    public override void OnCreate()
    {

        _textName = transform.Find<Text>("Bg2/Text");
        _num = transform.Find<Text>("Bg2/TextCount");
        _buyButton = transform.Find<Button>("Button1");
        _cost = transform.Find<Text>("Button1/Text");
        _blockTag = transform.Find("Button2").gameObject;

        _icon = transform.Find<Image>("Bg1/Image");
        _qua = transform.Find<Image>("Bg1/Img_qua");
        _pNum = transform.Find<Text>("Bg1/Text");
        _tag = transform.Find("Text").gameObject;
        _button = _icon.transform.GetComponent<Button>();
        _listView = ListView.Create<Act2099CapsuleListItem>(transform.Find("Scroll View"));
        _button.onClick.AddListener(On_buttonClick);
        _actInfo = (ActInfo_2099)ActivityManager.Instance.GetActivityInfo(2099);
        _buyButton.onClick.AddListener(On_buyButtonClick);
    }
    private void On_buttonClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_buttonDialogShowAsynCB);
    }
    private void On_buttonDialogShowAsynCB(_D_ItemTip d)
    {
        var itemCount = BagInfo.Instance.GetItemCount(_pid);
        d?.OnShow(_pid, (int)itemCount, _button.transform.position);
    }
    private void On_buyButtonClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);
        var alertStr = Lang.Get("是否充值￥{0}购买礼包", _data.price_cn);
        var temp = Alert.YesNo(alertStr);
        temp.SetYesCallback(() =>
        {
            _actInfo.BuyCapsuleGift(_data, () =>
            {
                _buyButton.interactable = false;
            });
            temp.Close();
        });
        temp.SetNoCallback(() =>
        {
            temp.Close();
        });
    }

    public void Refresh(Act2099GiftInfo item, int selectId, Action<int> callback)
    {
        _callback = callback;
        _item = item;
        _data = Cfg.Activity2099.GetPackage(item.id, _actInfo.GetStep());
        _textName.text = _data.name;
        _cost.text = "￥" + _data.price_cn.ToString();
        _num.text = Lang.Get("购买次数(<Color=#00ff00ff>{0}</Color>/{1})", _item.exchange_num, _data.purchase_limit);


        _blockTag.gameObject.SetActive(item.exchange_num >= _data.purchase_limit);
        _buyButton.interactable = true;
        _buyButton.gameObject.SetActive(item.exchange_num < _data.purchase_limit);

        //P_Item[] rewards = GlobalUtils.ParseItem(_data.contents);

        P_Item reward = new P_Item(_data.contents);

        _pid = reward.Id;
        Cfg.Item.SetItemIcon(_icon, reward.Id);
        _qua.color = _ColorConfig.GetQuaColor(Cfg.Item.GetItemQua(reward.Id));
        _pNum.text = "X" + reward.Num.ToString();

        _listView.Clear();
        int cid = Cfg.Activity2099.GetCIdByItemId(reward.Id);
        _tag.gameObject.SetActive(_pid == ItemId.Act2099SpecialCultureTable);
        if (cid > 0)
        {
            string tempReward = Cfg.Activity2099.GetRewardById(cid, _actInfo.GetStep());
            P_Item[] capsuleReward = GlobalUtils.ParseItem(tempReward);

            for (int i = 0; i < capsuleReward.Length; i++)
            {
                var one = capsuleReward[i];
                _listView.AddItem<Act2099CapsuleListItem>().Refresh(one);
            }
        }

        if (_item.id == selectId)
        {
            //选中特效
            _callback.Invoke(selectId);
        }
    }
    public override void OnRemoveFromList()
    {
        base.OnRemoveFromList();
        if (_listView != null)
        {
            _listView.Clear();
        }
    }

}