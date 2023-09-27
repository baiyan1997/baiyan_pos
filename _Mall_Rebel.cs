using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Mall_Rebel : TabViewBase
{

    private Transform _listViewRoot;
    // private ListView _listView;
    private RecycleView _recycleView;
    private Text _bodyCurrencyNum;
    private Button _helpButton;
    private Text _gameProgress;
    //private int current_mid;
    private List<P_RebelItem> _rebelShopInfo;




    public override void OnCreate()
    {
        // _transform = transform;
        _listViewRoot = transform.Find("Scroll View");
        // _listView = ListView.Create<PJShopItem>(_listViewRoot);
        _recycleView = _listViewRoot.GetComponent<RecycleView>();
        _recycleView.Init(onListRender);
        _bodyCurrencyNum = transform.Find<Text>("Inf/Text_count");
        _helpButton = transform.Find<Button>("Inf/Button");
        _gameProgress = transform.Find<Text>("Banner/TextProgram");
        transform.Find<JDText>("Inf/Text").text = Lang.Get("机体货币");
        _helpButton.onClick.AddListener(On_helpButtonClick);
        _rebelShopInfo = new List<P_RebelItem>();
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdateBodyCurrency);
    }

    private void On_helpButtonClick()
    {
        DialogManager.ShowAsyn<_D_Top_HelpDesc>(On_helpButtonDialogShowAsynCB);
    }
    private void On_helpButtonDialogShowAsynCB(_D_Top_HelpDesc d)
    {
        d?.OnShow(HelpType.FlightRebel);
    }

    public override void OnShow()
    {
        //current_mid = StageInfo.Instance.RebelInfo.cur_rebel;
        // _transform.gameObject.SetActive(true);


        _rebelShopInfo = RebelShopInfo.Instance.GetSellItems();
        int shopLv = RebelShopInfo.Instance.GetShopLv();

        var step = Cfg.FlightRebel.GetFlightRebelShopNextStep(shopLv);
        var mid = Cfg.FlightRebel.GetFlightRebelShopNextMid(shopLv);
        if (mid != 0)
        {

            var current_name = Cfg.Rebel.GetRebelDugeonName(mid);
            string condition = Step2StarField.GetMapStepString(step);

            if (string.IsNullOrEmpty(condition))
            {
                _gameProgress.text = Lang.Get("叛军达到{0}商店货物会更新", current_name);
            }
            else
            {
                _gameProgress.text = Lang.Get("叛军达到{0}并且{1}商店货物会更新", current_name, condition);
            }

        }
        else
        {
            _gameProgress.text = Lang.Get("商店已达到最高等级");
        }

        Refresh();
        //RebelShopInfo.Instance.InitRebelShopSellInfo(_rebelShopInfo, (int mid, int step) =>
        //{
        //    Debug.LogError("mid" + mid);


        //    if (mid != 0)
        //    {

        //        var current_name = Cfg.Rebel.GetStageFormatName(mid);
        //        string condition = Step2StarField.GetMapStepString(step);

        //        if (string.IsNullOrEmpty(condition))
        //        {
        //            _gameProgress.text = Lang.Get("叛军达到{0}商店货物会更新", current_name);
        //        }
        //        else
        //        {
        //            _gameProgress.text = Lang.Get("叛军达到{0}并且{1}商店货物会更新", current_name, condition);
        //        }

        //    }
        //    else
        //    {
        //        _gameProgress.text = Lang.Get("商店已达到最高等级");
        //    }

        //    Refresh();
        //});
    }

    protected void onListRender(GameObject obj, int index)
    {
        if(obj == null) {
            return;
        }

        var info = _rebelShopInfo[index];
        if(info == null) {
            return;
        }

        var shopItem = obj.GetComponent<PJShopItemComp>();
        if(shopItem == null) {
            shopItem = obj.AddComponent<PJShopItemComp>();
        }
        shopItem.CreateItem();
        shopItem.item.Refresh(info, CallBuyItem);
    }

    private void Refresh()
    {

        UpdateBodyCurrency();
        // //_rebelShopInfo = RebelShopInfo.Instance.GetRebelShopItems(_rebelShopInfo);
        // var length = _rebelShopInfo.Count;
        // //var current_name = Cfg.Rebel.GetStageShowName(current_mid);
        // _listView.Clear();
        // //int length = _actInfo.UniqueInfo.PJshop.Count;
        // for (int i = 0; i < length; i++)
        // {
        //     _listView.AddItem<PJShopItem>().Refresh(_rebelShopInfo[i], CallBuyItem);
        // }
        _recycleView.ShowList(_rebelShopInfo.Count);
    }
    public void CallBuyItem(P_RebelItem buyItem)
    {
        DialogManager.ShowAsyn<_D_BuyRebelItem>(d => { d?.OnShow(buyItem, UpdateBodyCurrency); });
    }


    private void UpdateBodyCurrency()
    {
        _bodyCurrencyNum.text = BagInfo.Instance.GetItemCount(ItemId.BodyCurrency).ToString();
    }

    // public override void OnClose()
    // {
        // _transform.gameObject.SetActive(false);
    // }

    public override void OnDestroy()
    {
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdateBodyCurrency);
        _rebelShopInfo = null;
    }
}

public class PJShopItemComp : MonoBehaviour
{
    public PJShopItem item = null;
    public void CreateItem()
    {
        if(item == null) {
            item = new PJShopItem();
            item.gameObject = gameObject;
            item.OnCreate();
            item.OnAddToList();
        }
    }

    void OnDestroy()
    {
        if(item != null) {
            item.OnRemoveFromList();
        }
    }
}

public class PJShopItem : ListItem
{
    private Image _icon;
    private Button _buyButton;
    private Text _costText;
    private P_RebelItem _itemInfo;
    private Image _imageQua;
    private Text _countText;
    private Text _txtName;

    private Action<P_RebelItem> _buyItem;
    public override void OnCreate()
    {
        InitRef();
        _buyButton.onClick.SetListener(On_buyButtonClick);
    }
    private void On_buyButtonClick()
    {
        //需要修改
        _buyItem(_itemInfo);
    }
    private void InitRef()
    {
        _icon = transform.FindImage("Content/Icon/Image");
        _buyButton = transform.FindButton("Content");
        _costText = transform.Find<Text>("Content/Price/TextKr/Text");
        _imageQua = transform.Find<Image>("Content/Icon/ImageQua");
        _countText = transform.Find<JDText>("Content/TextCount");
        _txtName = transform.Find<JDText>("Content/Text_Title");
    }


    public void Refresh(P_RebelItem rebelItem, Action<P_RebelItem> action)
    {
        _itemInfo = rebelItem;
        _buyItem = action;

        //修改为正确的数值

        _imageQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(_itemInfo.itemid));
        Cfg.Item.SetItemIcon(_icon, _itemInfo.itemid);
        _costText.text = _itemInfo.cost.ToString();

        _countText.text = GLobal.NumFormat(_itemInfo.item_count);
        _txtName.text = Cfg.Item.GetItemName(_itemInfo.itemid);


    }
}
//叛军商店



