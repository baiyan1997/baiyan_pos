using System;
using UnityEngine;
using UnityEngine.UI;

public class _D_2092Exchange : Dialog
{
    private Text _num;
    private Button _btnClose;
    private ListView _listView;
    private ActInfo_2092 _info;
    private bool _isShowing;
    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }

    protected override void InitRef()
    {
        _info = ActivityManager.Instance.GetActivityInfo(2092) as ActInfo_2092;
        _num = transform.FindText("Main/Bg/Image/Num");
        _btnClose = transform.FindButton("CloseButton");
        _listView = ListView.Create<Item>(transform.Find("Main/Scroll View"));
    }

    public override bool IsFullScreen()
    {
        return false;
    }

    protected override void OnCreate()
    {
        _btnClose.onClick.AddListener(Close);
        AddBufferedEvent(EventCenter.Instance.UpdatePlayerItem, OnEvent_UpdatePlayerItem);
    }

    private void OnEvent_UpdatePlayerItem()
    {
        if (!_isShowing)
        {
            return;
        }
        _num.text = "x" + GLobal.NumFormat_2(Uinfo.Instance.Bag.GetItemCount(ItemId.Line));
        RefreshItems();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _info = null;
    }

    public void OnShow(Action call)
    {
        _isShowing = true;
        _num.text = "x" + GLobal.NumFormat_2(Uinfo.Instance.Bag.GetItemCount(ItemId.Line));
        RefreshItems();
        call?.Invoke();
    }

    public void RefreshItems()
    {
        _info.Tag = false;
        _listView.Clear();
        var exchangeInfo = _info.UniqueInfo.exchange_info;
        for (int i = 0; i < exchangeInfo.Count; i++)
        {
            _listView.AddItem<Item>().Refresh(exchangeInfo[i], _info);
        }
    }

    protected override void OnClose()
    {
        _isShowing = false;
    }

    class Item : ListItem
    {
        private Image _icon;
        private Text _name;
        private Button _btn;
        //private Image _btnImg;
        private Text _desc;
        private Text _costNum;
        private Text _btnText;
        private GameObject _redPoint;
        private ActInfo_2092 _actInfo;
        private P_2092ExchangeInfo _itemInfo;
        private int _id;

        public override void OnCreate()
        {
            _icon = transform.FindImage("_icon");
            _name = transform.FindText("_name");
            _btn = transform.FindButton("_btnBuy");
            //_btnImg = _btn.GetComponent<Image>();
            _costNum = transform.FindText("_costToken");
            _desc = transform.FindText("_desc");
            _redPoint = _btn.transform.Find("RedPoint").gameObject;
            _btnText = _btn.GetComponentInChildren<Text>();

            _btn.onClick.AddListener(On_btnClick);
        }
        private void On_btnClick()
        {
            if (_itemInfo.num >= 1)
            {
                MessageManager.Show(Lang.Get("已兑换"));
                return;
            }
            if (BagInfo.Instance.GetItemCount(ItemId.Line) < Cfg.Act2092.GetExchangeCostNum(_id))
            {
                MessageManager.Show(Lang.Get("雷达天线不足"));
                return;
            }
            _actInfo.Exchange(_itemInfo.id, OnExchange);
        }

        public void OnExchange()
        {
            DialogManager.GetInstanceOfDialog<_D_2092Exchange>().RefreshItems();
            MessageManager.Show(Lang.Get("成功兑换晶体管x{0}", Cfg.Act2092.GetExchangeGoodNum(_id)));
        }

        public void Refresh(P_2092ExchangeInfo info, ActInfo_2092 actInfo)
        {
            _id = info.id;
            _itemInfo = info;
            _actInfo = actInfo;
            Cfg.Item.SetItemIcon(_icon,70048);
            _desc.text = Cfg.Item.GetItemDesc(70048);
            _name.text = Cfg.Item.GetItemName(70048) + $"x{Cfg.Act2092.GetExchangeGoodNum(info.id)}";
            int needCost = Cfg.Act2092.GetExchangeCostNum(info.id);
            _costNum.text = needCost.ToString();
            //_btnImg.color = info.num < 1 ? _ColorConfig.ButtonGreen : _ColorConfig.ButtonGray;
            _btn.interactable = info.num < 1;
            var str = info.num < 1 ? Lang.Get("兑换") : Lang.Get("已兑换");
            _btnText.text = str;
            bool canExchange = needCost <= BagInfo.Instance.GetItemCount(ItemId.Line) && info.num < 1;
            _redPoint.SetActive(canExchange);
        }
    }
}