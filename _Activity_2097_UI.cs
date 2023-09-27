using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2097_UI : ActivityUI
{

    private ActInfo_2097 _actInfo;
    private Text _timeText;
    private Text _title;
    private Text _essenceNum;//精粹数量
    private int _aid;
    private Button _showOverflowingItemsButton;

    //规则展示界面
    private Button _showRulesButton;
    private ListView _shopListView;

    public override void OnCreate()
    {
        InitRef();
        //InitListener();
    }

    private void InitRef()
    {
        _aid = 2097;
        Transform shopRoot = transform.Find("Scroll View");
        _shopListView = ListView.Create<Act2097ShopItem>(shopRoot);

        _showOverflowingItemsButton = transform.Find<Button>("Button/Button1");

        _timeText = transform.Find<Text>("TextCountDown");
        _title = transform.Find<Text>("_title");
        _title.text = Lang.Get("异星集市");
        _showRulesButton = transform.Find<Button>("Button/Button2");

        _essenceNum = transform.Find<Text>("Text/Num");

        Image essenceImage = transform.Find<Image>("T_Main/Icon/Icon/Image");
        Cfg.Item.SetItemIcon(essenceImage, ItemId.Essence);
        Image essenceImageQua = transform.Find<Image>("T_Main/Icon/Icon/ImageQua");
        essenceImageQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(ItemId.Essence));

        _showOverflowingItemsButton.onClick.AddListener(ConvertItems);
        _showRulesButton.onClick.AddListener(On_showRulesButtonClick);
    }
    private void On_showRulesButtonClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_showRulesButtonDialogShowAsynCB);
    }
    private void On_showRulesButtonDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2097, _showRulesButton.transform.position, Direction.Up, 350);
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshEssence);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(RefreshEssence);
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2097)ActivityManager.Instance.GetActivityInfo(_aid);
        UpdateTime(0);
        RefreshEssence();
        RefreshShop();
    }

    public override void OnClose()
    {
        base.OnClose();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void RefreshEssence()
    {

        //代替精粹id
        _essenceNum.text = GLobal.NumFormat(BagInfo.Instance.GetItemCount(ItemId.Essence));
    }
    //购买道具
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _aid)
        {
            return;
        }

        RefreshEssence();
    }

    private void RefreshShop()
    {
        var tempItems = _actInfo.GetShopItems();
        _shopListView.Clear();
        for (int i = 0; i < tempItems.Count; i++)
        {
            var t = tempItems[i];
            _shopListView.AddItem<Act2097ShopItem>().Refresh(t);
        }
    }
    //转换溢出道具
    private void ConvertItems()
    {
        _actInfo.GetExtraItemInfo(ShowOverflowingItems);
    }

    private void ConfirmConvert()
    {
        _actInfo.ConvertItems(OnConvertItemsCB);
    }
    private void OnConvertItemsCB()
    {
        DialogManager.CloseDialog<_D_ItemOverflow>();
        RefreshEssence();
    }

    private void ShowOverflowingItems(List<Act2097ExtraItem> convertItems)
    {


        if (convertItems == null || convertItems.Count <= 0)
        {
            MessageManager.Show(Lang.Get("没有溢出的道具！"));
            return;
        }
        int num = 0;
        List<P_Item> itemList = new List<P_Item>();
        //更新需要转化的道具
        for (int i = 0; i < convertItems.Count; i++)
        {
            var t = convertItems[i];
            P_Item item = new P_Item(t.item_id, t.extra_count);
            itemList.Add(item);
            P_Item tempItem = new P_Item(t.trans);
            num += t.extra_count * tempItem.Num;
        }
        P_Item costItem = new P_Item(convertItems[0].trans);
        P_Item goalItem = new P_Item(costItem.id, num);
        DialogManager.ShowAsyn<_D_ItemOverflow>(d => { d?.OnShow(itemList, goalItem, ConfirmConvert); });
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
}
public class Act2097ShopItem : ListItem
{
    private int _itemId;
    private int _id;
    private int _remainingNum;
    private Image _icon;
    private Text _name;
    private Text _num;
    private Image _qua;
    private Text _remainingNumText;
    private Text _costNum;
    private GameObject _tag;
    private Button _iconButton;
    private Button _buyButton;

    private ActInfo_2097 _actInfo;
    private int _aid;
    public override void OnCreate()
    {
        InitRef();
    }

    private void InitRef()
    {
        _remainingNumText = transform.Find<Text>("Content/ImageFree/Text");
        _name = transform.Find<Text>("Content/Text_Title");
        _icon = transform.Find<Image>("Content/Icon/Image");
        _qua = transform.Find<Image>("Content/Icon/ImageQua");
        _num = transform.Find<Text>("Content/TextCount");
        _costNum = transform.Find<Text>("Content/Price/Text/Text");
        _tag = transform.Find("Content/Image").gameObject;
        _iconButton = _icon.GetComponent<Button>();
        _buyButton = transform.Find<Button>("Content/Price");
        _iconButton.onClick.AddListener(On_iconButtonClick);
        _buyButton.onClick.AddListener(On_buyButtonClick);
        _aid = 2097;
    }
    private void On_iconButtonClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_iconButtonDialogShowAsynCB);
    }
    private void On_iconButtonDialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(_itemId, (int)BagInfo.Instance.GetItemCount(_itemId), _iconButton.transform.position);
    }
    private void On_buyButtonClick()
    {
        BuyItem(_id);
    }


    private void BuyItem(int itemId)
    {
        var item = Cfg.Activity2097.GetShopItemById(_id);
        P_Item cost = new P_Item(item.cost_item);
        var num = BagInfo.Instance.GetItemCount(ItemId.Essence);

        var itemInfo = Cfg.Activity2097.GetShopItemById(_id);
        if (_remainingNum <= 0 && itemInfo.limit_times != 0)
        {
            MessageManager.Show(Lang.Get("该道具已经售罄！"));
            return;
        }
        if (num < cost.Num)
        {
            MessageManager.Show(Lang.Get("精粹不足！"));
            return;
        }

        _buyButton.interactable = false;
        _actInfo.BuyItem(Refresh, itemId);

    }

    public void Refresh(Act2097ShopItemInfo shopItem)
    {
        if (!_buyButton.interactable)
        {
            _buyButton.interactable = true;
        }

        _id = shopItem.id;

        _actInfo = (ActInfo_2097)ActivityManager.Instance.GetActivityInfo(_aid);
        var itemInfo = Cfg.Activity2097.GetShopItemById(shopItem.id);
        P_Item getItem = new P_Item(itemInfo.get_item);//商品
        P_Item costItem = new P_Item(itemInfo.cost_item);//花费

        _remainingNum = Mathf.Max(itemInfo.limit_times - shopItem.exchange_times, 0);//剩余数量
        Cfg.Item.SetItemIcon(_icon, getItem.Id);
        _costNum.text = costItem.Num.ToString();//读表获取
        _itemId = getItem.Id;
        _name.text = Cfg.Item.GetItemName(_itemId);
        _num.text = "X" + getItem.Num;
        _qua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(_itemId));
        string str = "购买";
        _remainingNumText.text = itemInfo.limit_times != 0 ? Lang.Get("剩余{0}件", _remainingNum) : Lang.Get("无{0}限制", str);
        _tag.SetActive(_remainingNum <= 0 && itemInfo.limit_times != 0);
    }
}