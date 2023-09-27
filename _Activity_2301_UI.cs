using System;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2301_UI : ActivityUI
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
    private ListReuse3<Act2301Item> _list;
    private Text _exp;
    private Button _btnGetExp;
    private GameObject _redPoint;
    private long _lastExp;
    private ActInfo_2301 _actInfo;

    private bool _isShowing;

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
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdatePlayerItem);
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
            if (_actInfo.UniqueInfo.type == 2 && !ItemHelper.IsCountEnoughWithFalseHandle(ItemId.Gold, _actInfo.UniqueInfo.price, null))
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
        d.OnShow(_actInfo._name, _actInfo._desc,
         _actDescBtn.transform.position, Direction.LeftDown, 350, new Vector2(-25, -25));
    }
    private void On_btnGetExpClick()
    {
        DialogManager.ShowAsyn<_D_Act2301ExpGet>(On_btnGetExpDialogShowAsynCB);
    }
    private void On_btnGetExpDialogShowAsynCB(_D_Act2301ExpGet d)
    {
        d?.OnShow();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != 2301 || !_isShowing)
        {
            return;
        }

        CheckHighRewardPoolUnlock();
        CheckNewExpCanGet();
        EventCenter.Instance.RemindActivity.Broadcast(2301, _actInfo.IsAvaliable());
        //_list.RefreshVisibleItemInfo();
    }

    private void UpdatePlayerItem()
    {
        var nowExp = BagInfo.Instance.GetItemCount(14111);
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
        switch (_actInfo.UniqueInfo.type)
        {
            case 2:
                return Lang.Get("是否消耗{0}氪晶解锁高级奖池?", _actInfo.UniqueInfo.price);
            case 1:
                return Lang.Get("是否前往充值￥{0}解锁高级奖池?", _actInfo.UniqueInfo.price_info.price);
        }

        throw new Exception($"error type:{_actInfo.UniqueInfo.type}");
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
        _list = ListReuse3<Act2301Item>.Create(root, RefreshIndex, ScrollDirectionEnum.Vertical, 0);
        _actInfo = ActivityManager.Instance.GetActivityInfo(2301) as ActInfo_2301;
    }

    private void RefreshIndex(ListItem item, int index)
    {
        ((Act2301Item)item).Refresh(index, _actInfo);
    }

    public override void OnShow()
    {
        _isShowing = true;
        _title.text = _actInfo._data.name;
        var nowExp = BagInfo.Instance.GetItemCount(14111);
        _lastExp = nowExp;
        _exp.text = nowExp.ToString();
        CheckNewExpCanGet();
        UpdateTime(TimeManager.ServerTimestamp);
        SetRewardPoolUi(_actInfo.UniqueInfo.lv_price == 2);
        _defaultBanner.SetActive(false);
        _bannerImg.gameObject.SetActive(false);
        if (_bannerSprite != null)
        {
            RefreshBannerImg(_bannerSprite);
        }
        else
        {
            LoadSprite();
        }

        _list.SetBeginAndRefresh(_actInfo.UniqueInfo.reward_info.Count);

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

public class Act2301Item : ListItem
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
    private ActInfo_2301 _actInfo;
    private P_2301Item _itemInfo;
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
        _actInfo = ActivityManager.Instance.GetActivityInfo(2301) as ActInfo_2301;
        _btnGet.onClick.AddListener(On_btnGetClick);
        _btnGet1.onClick.AddListener(On_btnGet1Click);
    }
    private void On_btnGetClick()
    {
        _actInfo.GetReward(_itemInfo.id, On_btnGetRewardCB);
    }
    private void On_btnGetRewardCB()
    {
        _itemInfo = _actInfo.UniqueInfo.reward_info[_index];
        string rewards;
        if (_itemInfo.OnlyOneProReward())
        {
            rewards = _actInfo.UniqueInfo.lv_price == 1 ? _itemInfo.prize_free : $"{_itemInfo.prize_free},{_itemInfo.Prizepro1}";
        }
        else
        {
            rewards = _actInfo.UniqueInfo.lv_price == 1 ? _itemInfo.prize_free : $"{_itemInfo.prize_free},{_itemInfo.prize_pro_1},{_itemInfo.prize_pro_2}";
        }
        Uinfo.Instance.AddItemAndShow(rewards);
        SetButton(_itemInfo, _actInfo.UniqueInfo.lv_price == 2);
        SetRwrdGetDisp(_itemInfo.free_get, _itemInfo.pro_get);
    }
    private void On_btnGet1Click()
    {
        if (_actInfo.UniqueInfo.lv_price == 1)
        {
            MessageManager.Show(Lang.Get("解锁高级奖池后可领取更多超值奖励"));
            return;
        }
        _actInfo.GetReward(_itemInfo.id, On_btn1GetRewardCB);
    }
    private void On_btn1GetRewardCB()
    {
        _itemInfo = _actInfo.UniqueInfo.reward_info[_index];
        string rewards;
        if (_itemInfo.OnlyOneProReward())
        {
            rewards = $"{_itemInfo.Prizepro1}";
        }
        else
        {
            rewards = $"{_itemInfo.prize_pro_1},{_itemInfo.prize_pro_2}";
        }
        Uinfo.Instance.AddItemAndShow(rewards);
        SetButton(_itemInfo, _actInfo.UniqueInfo.lv_price == 2);
        SetRwrdGetDisp(_itemInfo.free_get, _itemInfo.pro_get);
    }


    public void Refresh(int index, ActInfo_2301 info)
    {
        _index = index;
        _actInfo = info;
        var exp = BagInfo.Instance.GetItemCount(14111);
        var list = info.UniqueInfo.reward_info;
        _itemInfo = list[index];
        _expText.text = _itemInfo.exp.ToString();
        _sliderUp.gameObject.SetActive(false);
        if (index == 0)
        {
            _sliderUp.gameObject.SetActive(true);
            _sliderUp.value = (float)exp / _itemInfo.exp;
        }

        if (index < list.Count - 1)
        {
            _sliderDown.value = (float)(exp - _itemInfo.exp) /
                                (list[index + 1].exp - _itemInfo.exp);
        }
        else
        {
            _sliderDown.gameObject.SetActive(false);
        }

        _normalReward.RefreshNormal(_itemInfo.prize_free);
        _highReward1.RefreshHigh(_itemInfo.Prizepro1, info.UniqueInfo.lv_price);
        if (_itemInfo.OnlyOneProReward())
        {
            _highReward2.Hide();
        }
        else
        {
            _highReward2.RefreshHigh(_itemInfo.prize_pro_2, info.UniqueInfo.lv_price);
        }
        SetRwrdGetDisp(_itemInfo.free_get, _itemInfo.pro_get);
        SetButton(_itemInfo, info.UniqueInfo.lv_price == 2);
    }

    private void SetRwrdGetDisp(int itemInfoFreeGet, int itemInfoProGet)
    {
        _normalReward.SetRwrdGetDisp(itemInfoFreeGet);
        _highReward1.SetRwrdGetDisp(itemInfoProGet);
        _highReward2.SetRwrdGetDisp(itemInfoProGet);
    }

    private void SetButton(P_2301Item itemInfo, bool isUnlockHigh)
    {
        if (itemInfo.exp > BagInfo.Instance.GetItemCount(14111))
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

        public void RefreshNormal(string reward)
        {
            P_Item item = GlobalUtils.ParseItem(reward)[0];
            _itemId = item.Id;
            _itemNum = item.Num;
            Cfg.Item.SetItemIcon(_icon, item.Id);
            _imgQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(item.Id));
            _count.text = item.Num.ToString();
            _lock.SetActive(false);
            //_hasGetTip.SetActive(hasGet == 1);
        }

        public void RefreshHigh(string rewards, int isUnlock)
        {
            P_Item item = GlobalUtils.ParseItem(rewards)[0];
            _itemId = item.Id;
            _itemNum = item.Num;
            Cfg.Item.SetItemIcon(_icon, item.Id);
            _imgQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(item.Id));
            _count.text = item.Num.ToString();
            _lock.SetActive(isUnlock == 1);
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
