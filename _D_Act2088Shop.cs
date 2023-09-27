using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class _D_Act2088Shop : Dialog
{

    private Transform _listViewRoot;
    private ListView _listViewShop;
    private ActInfo_2088 _actInfo;
    private Text _blindBoxFragmentsNum;
    private int _aid = 2088;
    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    protected override void InitRef()
    {
        _actInfo = (ActInfo_2088)ActivityManager.Instance.GetActivityInfo(_aid);
        _listViewRoot = transform.Find("Main/Scroll View");
        _listViewShop = ListView.Create<ShopItem>(_listViewRoot);
        _blindBoxFragmentsNum = transform.FindText("Main/ImageIcon/Text");

    }

    public override bool IsFullScreen()
    {
        return false;
    }

    protected override void OnCreate()
    {
        InitEvents();
    }

    private void InitEvents()
    {
        AddEvent(EventCenter.Instance.UpdateActivityUI, Refresh);
        //惊喜盲盒活动关闭时关闭界面
        AddEvent(EventCenter.Instance.UpdateAllActivity, _EventUpdateAllActivity);
        AddEvent(EventCenter.Instance.ActivityOverdue, _EventActivityOverdue);
    }
    private void _EventUpdateAllActivity()
    {
        if (!ActivityManager.Instance.IsActDuration(ActivityID.SupriseBox))
            Close();
    }
    private void _EventActivityOverdue(int aid)
    {
        if (aid == ActivityID.SupriseBox)
        {
            Close();
        }
    }

    private void Refresh(int aid)
    {

        if (aid != _aid) return;
        if (!IsShowing) return;
        _blindBoxFragmentsNum.text = _actInfo.UniqueInfo.box_chip_num.ToString();

    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _actInfo = null;
    }
    public void OnShow()
    {
        Debug.LogError("shop refresh");
        _actInfo = (ActInfo_2088)ActivityManager.Instance.GetActivityInfo(_aid);
        _blindBoxFragmentsNum.text = _actInfo.UniqueInfo.box_chip_num.ToString();
        var shopList =  _actInfo.UniqueInfo.ExchangeList;
        _listViewShop.Clear();
        int length = shopList.Count;
        for (int i = 0; i < length; i++)
        {
            _listViewShop.AddItem<ShopItem>().Refresh(shopList[i]);
        }
    }
}

public class ShopItem: ListItem
{
    private Image _icon;
    private Text _costText;
    private Button _buyButton;
    private GameObject _tag;
    private Text _progress;
    private Text _count;
    private int _aid = 2088;
    private P_Act2088Exchange _info;
    private ActInfo_2088 _actInfo;
    private int _max_time;
    private int _cost;
    private bool _firstOpen;
    private Image _imageQua;
    private Button _showItemButton;
    public override void OnCreate()
    {
        InitRef();
        InitButton();
    }

    private void InitButton()
    {
        _buyButton.onClick.AddListener(BuyItems);
    }

    private void InitRef()
    {

        //初始化UI
        _actInfo = (ActInfo_2088)ActivityManager.Instance.GetActivityInfo(_aid);

        _icon = transform.Find<Image>("Icon/ImageIcon");
        _buyButton = transform.FindButton("Button");
        _costText = transform.FindText("Button/Text");
        _progress = transform.FindText("Image/Text");
        _count = transform.FindText("Icon/Text");
        _tag = transform.Find("Tag").gameObject;
        _imageQua = transform.Find<Image>("Icon/ImageQua");
        _showItemButton = transform.gameObject.GetComponent<Button>();
    }

    private void BuyItems()
    {
        _actInfo.ExchangeItems(_info.id, OnExchangeItemsCB);
    }
    private void OnExchangeItemsCB()
    {
        _info.num++;
        RefreshAfterDraw();
        EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
    }


    public void Refresh(P_Act2088Exchange info)
    {
        _info = info;
        cfg_act_2088_shop data = Cfg.Activity2088.GetExchangeData(_info.id);



        P_Item reward = new P_Item(data.goods);

        var color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(reward.Id));
        _imageQua.color = color;


        _showItemButton.GetComponent<Button>().onClick.SetListener(() =>
        {
            DialogManager.ShowAsyn<_D_ItemTip>(d=>{ d?.OnShow(reward.Id, reward.count, _showItemButton.transform.position); });
        });

        _cost = data.cost;
        _costText.text = _cost.ToString();
        _count.text = reward.count.ToString();
        Cfg.Item.SetItemIcon(_icon,reward.id);
        _max_time = data.max_time;
        RefreshAfterDraw();
    }



    private void RefreshAfterDraw()
    {
        _actInfo = (ActInfo_2088)ActivityManager.Instance.GetActivityInfo(_aid);
        var chipNum = _actInfo.UniqueInfo.box_chip_num;
        if (_max_time == 0)//兑换不限次数
        {
            _progress.text = Lang.Get("兑换次数不限");
        }else
        {
            _progress.text = Lang.Get("限购:{0}/{1}", _info.num, _max_time);

        }
    }
}
