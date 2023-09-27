using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class _Activity_2203_UI : ActivityUI
{
    private Text _title;
    private Text _countDown;
    private Button _actDescBtn;
    private Image _bannerImg;
    private GameObject _defaultBanner;
    private Sprite _bannerSprite;
    private GameObject _highRewardPool;
    private GameObject _lowRewardPool;
    private Button _btnUnlockHigh;
    private ListReuse3<Act2203Item> _list;
    private Text _exp;
    private Button _btnGetExp;
    private GameObject _redPoint;
    private long _lastExp;
    private ActInfo_2203 _actInfo;

    private bool _isShowing;

    private List<cfg_act_2203_reward> _actData;

    //private 
    public override void OnCreate()
    {
        InitRef();
        InitEvent();
        //InitListener();
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdatePlayerItem);
        EventCenter.Instance.GotoUnlockTip.AddListener(On_btnUnlockHighClick);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdatePlayerItem);
        EventCenter.Instance.GotoUnlockTip.RemoveListener(On_btnUnlockHighClick);
    }

    private void InitEvent()
    {
        _btnUnlockHigh.onClick.AddListener(On_btnUnlockHighClick);
        _actDescBtn.onClick.AddListener(On_actDescBtnClick);
        _btnGetExp.onClick.AddListener(On_btnGetExpClick);
    }
    private void On_btnUnlockHighClick()
    {
        var alert = Alert.YesNo(GetUnlockTip());
        alert.SetYesCallback(() =>
        {
            if (_actInfo.UniqueInfo.type == 22 && !ItemHelper.IsCountEnoughWithFalseHandle(ItemId.Gold, _actInfo.UniqueInfo.price, null))
            {
                alert.Close();
                return;
            }
            _actInfo.UnlockHighLevel(() =>
            {
                alert.Close();
            });
        });
    }

    private void On_actDescBtnClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_actDescDialogShowAsynCB);
    }
    private void On_actDescDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2203, _actDescBtn.transform.position, Direction.LeftDown, 350);
    }
    private void On_btnGetExpClick()
    {
        DialogManager.ShowAsyn<_D_Act2203ExpGet>(On_btnGetExpDialogShowAsynCB);
    }
    private void On_btnGetExpDialogShowAsynCB(_D_Act2203ExpGet d)
    {
        d?.OnShow();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != 2203 || !_isShowing)
        {
            return;
        }

        CheckHighRewardPoolUnlock();
        CheckNewExpCanGet();
        EventCenter.Instance.RemindActivity.Broadcast(2203, _actInfo.IsAvaliable());
        //_list.RefreshVisibleItemInfo();
    }

    private void UpdatePlayerItem()
    {
        var nowExp = BagInfo.Instance.GetItemCount(14112);
        if (_isShowing && nowExp > _lastExp)
        {
            _exp.text = nowExp.ToString();
            _list.RefreshVisibleItemInfo();
            CheckNewExpCanGet();
        }
    }

    private void CheckNewExpCanGet()
    {
        _redPoint.SetActive(_actInfo.HasNewExpCanGet());
    }

    private void CheckHighRewardPoolUnlock()
    {
        SetRewardPoolUi(_actInfo.UniqueInfo.lv_price == 2);
        //所有的高级奖励解锁
        _list.RefreshVisibleItemInfo();
    }

    private string GetUnlockTip()
    {
        if (_actInfo.UniqueInfo.type == 22) {
            return Lang.Get("是否消耗{0}氪晶解锁高级奖池?", _actInfo.UniqueInfo.price);
        } else {
            var infos = Cfg.Activity2203.Get2203PackageInfo();
            if (infos == null) {
                return "";
            }
            return Lang.Get("是否前往充值￥{0}解锁高级奖池?\n（此商品仅解锁奖池，不包含氪晶）", infos.price);
        }
    }

    private void InitRef()
    {
        _title = transform.FindText("Title");
        _countDown = transform.FindText("CountDown");
        _actDescBtn = transform.FindButton("Btn");
        _bannerImg = transform.FindImage("Banner/Banner_img");
        _defaultBanner = transform.Find("Banner/Empty").gameObject;
        _highRewardPool = transform.Find("Banner/ImageHigh").gameObject;
        _lowRewardPool = transform.Find("Banner/ImageLow").gameObject;
        _btnUnlockHigh = transform.FindButton("Banner/BtnUnlockHigh");
        var root = transform.Find("Scroll View");
        _exp = transform.FindText("Exp/Exp/Nub");
        _btnGetExp = transform.FindButton("Exp/GetExpBtn");
        _redPoint = transform.Find("Exp/GetExpBtn/Point").gameObject;
        _list = ListReuse3<Act2203Item>.Create(root, RefreshIndex, ScrollDirectionEnum.Vertical, 0);
        _actInfo = ActivityManager.Instance.GetActivityInfo(2203) as ActInfo_2203;
    }

    private void RefreshIndex(ListItem item, int index)
    {
        ((Act2203Item)item).Refresh(index, _actData);
    }
    public override void OnShow()
    {
        _isShowing = true;
        ActivityManager.Instance.RequestUpdateActivityById(2203);//更新活动信息
        _title.text = Cfg.Help.GetData((int)HelpType.Act2203).title;
        _actData = Cfg.Activity2203.GetCfgDetailList(WorldUtils.Inst.GetMapStep());
        var nowExp = BagInfo.Instance.GetItemCount(14112);
        _lastExp = nowExp;
        _exp.text = nowExp.ToString();
        CheckNewExpCanGet();
        UpdateTime(TimeManager.ServerTimestamp);
        SetRewardPoolUi(_actInfo.UniqueInfo.lv_price == 2);
        _defaultBanner.SetActive(false);
        _bannerImg.gameObject.SetActive(true);
        // if (_bannerSprite != null)
        // {
        //     RefreshBannerImg(_bannerSprite);
        // }
        // else
        // {
        //     LoadSprite();
        // }

        _list.SetBeginAndRefresh(_actData.Count);

    }

    public override void OnClose()
    {
        base.OnClose();
        _isShowing = false;
    }

    private void LoadSprite()
    {
        var localFullPath = _actInfo.GetBannerFullPath();//本地路径
        var sprite = LoadHelper.LoadSpriteFromLocal(localFullPath);//本地加载
        if (sprite != null)
        {
            RefreshBannerImg(sprite);
        }
        else//网络加载
            LoadHelper.LoadImgFromUrl(gameObject, _actInfo._data.bg_url, localFullPath, RefreshBannerImg);
    }

    private void RefreshBannerImg(Sprite sprite)
    {
        _bannerSprite = sprite;
        if (sprite == null)
        {
            _defaultBanner.SetActive(true);
            _bannerImg.gameObject.SetActive(false);
        }
        else
        {
            _defaultBanner.SetActive(false);
            _bannerImg.gameObject.SetActive(true);
            _bannerImg.sprite = sprite;
        }
    }

    private void SetRewardPoolUi(bool isHighUnlock)
    {
        _highRewardPool.SetActive(isHighUnlock);
        _lowRewardPool.SetActive(!isHighUnlock);
        _btnUnlockHigh.gameObject.SetActive(!isHighUnlock);
    }

    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (nowTs - _actInfo._data.startts < 0)
        {
            _countDown.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            _countDown.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _countDown.text = Lang.Get("活动已经结束");
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_list != null)
        {
            _list.OnDestroy();
            _list = null;
        }
    }
}

public class Act2203Item : ListItem
{
    private Slider _sliderUp;
    private Slider _sliderDown;
    private Text _expText;
    private Reward _normalReward;
    private Reward _highReward1;
    private Reward _highReward2;
    private Button _btnGet;
    private Button _btnGet1;
    private Transform _unReach;
    private ActInfo_2203 _actInfo;
    private cfg_act_2203_reward _actData;
    private P_2203Item _itemInfo;
    private int _index;

    public override void OnCreate()
    {
        _sliderUp = transform.Find("Slider01").GetComponent<Slider>();
        _sliderDown = transform.Find("Slider02").GetComponent<Slider>();
        _expText = transform.FindText("Icon02/Nub");
        _normalReward = new Reward(transform.Find("Item1"));
        _highReward1 = new Reward(transform.Find("Item2"));
        _highReward2 = new Reward(transform.Find("Item3"));
        _btnGet = transform.FindButton("ButtonGet0");
        _btnGet1 = transform.FindButton("ButtonGet1");
        _unReach = transform.Find("UnReach");
        _actInfo = ActivityManager.Instance.GetActivityInfo(2203) as ActInfo_2203;
        _btnGet.onClick.AddListener(On_btnGetClick);
        _btnGet1.onClick.AddListener(On_btnGet1Click);
    }
    private void On_btnGetClick()
    {
        if (!_actInfo.IsDuration()) {
            MessageManager.Show(Lang.Get("活动时间已结束！"));
            return;
        }
        _actInfo.GetReward(_actData.id, On_btnGetRewardCB);
    }
    private void On_btnGetRewardCB()
    {
        _itemInfo = _actInfo.UniqueInfo.reward_info[_index];
        string rewards;
        if (_actData.OnlyOneProReward())
        {
            rewards = _actInfo.UniqueInfo.lv_price != 2 ? _actData.prize_free : $"{_actData.prize_free},{_actData.Prizepro1}";
        }
        else
        {
            rewards = _actInfo.UniqueInfo.lv_price != 2 ? _actData.prize_free : $"{_actData.prize_free},{_actData.prize_pro_1},{_actData.prize_pro_2}";
        }
        Uinfo.Instance.AddItemAndShow(rewards);
        SetButton(_actData.exp, _itemInfo, _actInfo.UniqueInfo.lv_price == 2);
        SetRwrdGetDisp(_itemInfo.free_get, _itemInfo.pro_get);
    }
    private void On_btnGet1Click()
    {
        if (!_actInfo.IsDuration()) {
            MessageManager.Show(Lang.Get("活动时间已结束！"));
            return;
        }
        if (_actInfo.UniqueInfo.lv_price != 2)
        {
            // MessageManager.Show(Lang.Get("解锁高级奖池后可领取更多超值奖励"));
            EventCenter.Instance.GotoUnlockTip.Broadcast();
            return;
        }
        _actInfo.GetReward(_actData.id, On_btn1GetRewardCB);
    }
    private void On_btn1GetRewardCB()
    {
        _itemInfo = _actInfo.UniqueInfo.reward_info[_index];
        string rewards;
        if (_actData.OnlyOneProReward())
        {
            rewards = $"{_actData.Prizepro1}";
        }
        else
        {
            rewards = $"{_actData.prize_pro_1},{_actData.prize_pro_2}";
        }
        Uinfo.Instance.AddItemAndShow(rewards);
        SetButton(_actData.exp, _itemInfo, _actInfo.UniqueInfo.lv_price == 2);
        SetRwrdGetDisp(_itemInfo.free_get, _itemInfo.pro_get);
    }


    public void Refresh(int index, List<cfg_act_2203_reward> data)
    {
        _actInfo = ActivityManager.Instance.GetActivityInfo(2203) as ActInfo_2203;
        _index = index;
        _actData = data[index];
        var exp = BagInfo.Instance.GetItemCount(14112);
        _itemInfo = _actInfo.getSeverRewardInfo(_actData.id);
        if (_itemInfo == null) {
            _itemInfo = new P_2203Item()
            {
                free_get = 0,
                pro_get = 0,
                id = _actData.id
            };
        }
        _expText.text = _actData.exp.ToString();
        _sliderUp.gameObject.SetActive(false);
        if (index == 0)
        {
            _sliderUp.gameObject.SetActive(true);
            _sliderUp.value = (float)exp / _actData.exp;
        }

        if (index < data.Count - 1)
        {
            _sliderDown.gameObject.SetActive(true);
            _sliderDown.value = (float)(exp - _actData.exp) /
                                (data[index + 1].exp - _actData.exp);
        }
        else
        {
            _sliderDown.gameObject.SetActive(false);
        }

        _normalReward.RefreshNormal(_actData.prize_free, exp < _actData.exp);
        _highReward1.RefreshHigh(_actData.Prizepro1, _actInfo.UniqueInfo.lv_price, exp < _actData.exp);
        if (_actData.OnlyOneProReward())
        {
            _highReward2.Hide();
        }
        else
        {
            _highReward2.RefreshHigh(_actData.prize_pro_2, _actInfo.UniqueInfo.lv_price, exp < _actData.exp);
        }
        SetRwrdGetDisp(_itemInfo.free_get, _itemInfo.pro_get);
        SetButton(_actData.exp, _itemInfo, _actInfo.UniqueInfo.lv_price == 2);
    }

    private void SetRwrdGetDisp(int itemInfoFreeGet, int itemInfoProGet)
    {
        _normalReward.SetRwrdGetDisp(itemInfoFreeGet);
        _highReward1.SetRwrdGetDisp(itemInfoProGet);
        _highReward2.SetRwrdGetDisp(itemInfoProGet);
    }

    private void SetButton(int curexp, P_2203Item itemInfo, bool isUnlockHigh)
    {
        if (curexp > BagInfo.Instance.GetItemCount(14112))
        {
            _btnGet.gameObject.SetActive(false);
            _btnGet1.gameObject.SetActive(false);
            _unReach.gameObject.SetActive(true);
            _unReach.GetComponentInChildren<Text>().text = Lang.Get("未达成");
        }
        else if (itemInfo.IsNormalGet() && itemInfo.IsProGet())
        {
            _btnGet.gameObject.SetActive(false);
            _btnGet1.gameObject.SetActive(false);
            _unReach.gameObject.SetActive(true);
            _unReach.GetComponentInChildren<Text>().text = Lang.Get("已领取");
        }
        else if (itemInfo.IsNormalGet())
        {
            _btnGet.gameObject.SetActive(false);
            _btnGet1.gameObject.SetActive(true);
            _unReach.gameObject.SetActive(false);
        }
        else
        {
            _btnGet.gameObject.SetActive(true);
            _btnGet1.gameObject.SetActive(false);
            _unReach.gameObject.SetActive(false);
        }

    }

    class Reward
    {
        private Transform _trans;
        private Image _icon;
        private Image _imgQua;
        private Text _count;
        private GameObject _lock;
        private Button _btn;
        private int _itemId;
        private int _itemNum;
        private GameObject _hasGetTip;
        public Reward(Transform transform)
        {
            _trans = transform;
            OnCreate();
        }

        private void OnCreate()
        {
            _icon = _trans.FindImage("Icon");
            _imgQua = _trans.FindImage("Qua");
            _count = _trans.FindText("Text");
            _lock = _trans.Find("Lock").gameObject;
            _btn = _icon.GetComponent<Button>();
            _hasGetTip = _trans.Find("GetTip").gameObject;
            _btn.onClick.AddListener(On_btnClick);
        }
        private void On_btnClick()
        {
            DialogManager.ShowAsyn<_D_ItemTip>(On_btnDialogShowAsynCB);
        }
        private void On_btnDialogShowAsynCB(_D_ItemTip d)
        {
            d?.OnShow(_itemId, _itemNum, _trans.position);
        }

        public void RefreshNormal(string reward, bool isLock)
        {
            P_Item item = GlobalUtils.ParseItem(reward)[0];
            _itemId = item.Id;
            _itemNum = item.Num;
            Cfg.Item.SetItemIcon(_icon, item.Id);
            _imgQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(item.Id));
            _count.text = item.Num.ToString();
            _lock.SetActive(isLock);
            //_hasGetTip.SetActive(hasGet == 1);
        }

        public void RefreshHigh(string rewards, int isUnlock, bool isLock)
        {
            P_Item item = GlobalUtils.ParseItem(rewards)[0];
            _itemId = item.Id;
            _itemNum = item.Num;
            Cfg.Item.SetItemIcon(_icon, item.Id);
            _imgQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(item.Id));
            _count.text = item.Num.ToString();
            _lock.SetActive(isUnlock != 2 || isLock);
            //_hasGetTip.SetActive(hasGet == 1);
        }

        public void SetRwrdGetDisp(int hasGet)
        {
            _hasGetTip.SetActive(hasGet == 1);
        }

        public void Hide()
        {
            _trans.gameObject.SetActive(false);
        }
    }
}
