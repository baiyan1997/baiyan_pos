using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2005_UI : ActivityUI
{
    private const int AID = 2005;
    private int PIECE_ID;

    private ObjectGroup UI;

    private Button _shipDisplayBtn;

    private List<ObjectGroup> items_ui = new List<ObjectGroup>(6);

    private ActInfo_2005 actInfo;

    private void InitData()
    {
        actInfo = (ActInfo_2005)ActivityManager.Instance.GetActivityInfo(AID);
        PIECE_ID = Cfg.Ship.GetPlayerShipData(actInfo.SHIP_ID).itemid;
    }

    private void SetShip()
    {
        _ShipDisplayControl.Instance.ShowShip(actInfo.SHIP_ID, _ShipDisplayControl.DisplayMode.AutoRotateOnly);
        var shipStr = String.Format(Lang.Get("[{0}]{1}"), Cfg.Ship.GetShipTypeNameByType(Cfg.Ship.GetShipType(actInfo.SHIP_ID)),
            Cfg.Ship.GetShipName(actInfo.SHIP_ID));
        string str = "购买";
        UI.Get<Text>("text_desc").text = string.Format(Lang.Get("{1}任意一个道具，其他道具售价<Color=#00ff00ff>下降10%</Color>，最高折扣<Color=#ff9900ff>50%</Color>！{1}<Color=#00ff00ff>7</Color>个部件即可组装获得<Color=#ffcc00ff>{0}</Color>！"), shipStr, str);
    }

    public override void Awake()
    {
    }

    public override void OnCreate()
    {
        _shipDisplayBtn = transform.Find<Button>("ShowShip/RawImage");

        UI = gameObject.GetComponent<ObjectGroup>();
        InitData();
        InitEvent();
        //InitListener();
        UpdateUI();
        UpdateTime(TimeManager.ServerTimestamp);
    }

    private void OnClickRefreshBtn()
    {
        if (actInfo.avalueData.free_count > 0)
        {
            actInfo.SendRefreshActItems(UpdateUI);
        }

        else
        {
            var cost_gold = actInfo.avalueData.refresh_gold;

            var alert = Alert.YesNo(String.Format(Lang.Get("是否花费{0}氪晶立即刷新？"), cost_gold));
            alert.SetYesCallback(() =>
            {
                alert.Close();
                var check = ItemHelper.IsCountEnough(ItemId.Gold, cost_gold);
                if (check)
                {
                    actInfo.SendRefreshActItems(UpdateUI);
                }
            });
        }
    }

    private void InitEvent()
    {
        _shipDisplayBtn.onClick.SetListener(On_shipDisplayBtnClick);      

        for (int i = 1; i <= 6; i++)
        {
            var item_ui = UI.Get<ObjectGroup>("item_" + i);
            items_ui.Add(item_ui);

            var index = i;
            item_ui.Get<Button>("btn_buy").onClick.AddListener(() =>
            {
                var items = GlobalUtils.ParseItem3(actInfo.avalueData.items);
                var prices = actInfo.avalueData.buy_gold.Split(',');
                var price = Int32.Parse(prices[index - 1]);
                string str = "购买";
                var alert = Alert.YesNo(String.Format(Lang.Get("是否花费{0}氪晶{2}{1}？"), price,
                        Cfg.Item.GetItemName(items[index - 1].itemid) + "x" + items[index - 1].count, str));
                alert.SetYesCallback(() =>
                {
                    alert.Close();
                    var check = ItemHelper.IsCountEnough(ItemId.Gold, price);
                    if (check)
                    {
                        actInfo.SendBuyOneItem(index, UpdateUI);
                    }
                });
            });

            item_ui.Get<Button>("img_icon").onClick.AddListener(() =>
            {
                var items = GlobalUtils.ParseItem3(actInfo.avalueData.items);
                ItemHelper.ShowTip(items[index - 1].itemid, items[index - 1].count, item_ui.transform);
            });
        }

        UI.Get<Button>("btn_refresh").onClick.AddListener(OnClickRefreshBtn);
        UI.Get<Button>("btn_free_refresh").onClick.AddListener(OnClickRefreshBtn);
        UI.Get<Button>("btn_assemble").onClick.AddListener(On_btn_assembleClick);    
    }
    private void On_shipDisplayBtnClick()
    {
        DialogManager.ShowAsyn<_D_ShareShipShow>(On_shipDisplayDialogShowAsynCB);
    }
    private void On_shipDisplayDialogShowAsynCB(_D_ShareShipShow d)
    {
        d?.Show(actInfo.SHIP_ID, _shipDisplayBtn.transform.position, Direction.RightDown);
    }
    private void On_btn_assembleClick()
    {
        actInfo.SendGetAct2005Reward(UpdateUI);
    }


    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(EventUpdateUI);

    }

    public override void OnShow()
    {
        SetShip();
    }

    public override void OnClose()
    {
        base.OnClose();
        _ShipDisplayControl.Instance.CloseShipShow();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != AID)
            return;

        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateBottomUI();

        Cfg.Item.SetItemIcon(UI.Image("img_piece_icon"), PIECE_ID);


        var do_num = actInfo.avalueData.do_number;
        UI["text_piece_count"].SetActive(do_num < 7);
        UI.Text("text_piece_count").text = String.Format("{0} <Color=#>{1}</Color>/7", Cfg.Item.GetItemName(PIECE_ID), do_num);
        UI["btn_assemble"].SetActive(do_num >= 7);
        var obtained = Convert.ToInt32(actInfo._data.avalue["get_reward"]) == 1;
        UI.Button("btn_assemble").interactable = !obtained;
        if (obtained)
            UI.Text("text_assemble").text = Lang.Get("已获得");
        else
            UI.Text("text_assemble").text = Lang.Get("组装");

        var prices = actInfo.avalueData.buy_gold.Split(',');
        var items = GlobalUtils.ParseItem3(actInfo.avalueData.items);
        var buy_index = GLobal.ParseWithMark(actInfo.avalueData.buy_index, ',');

        Func<int, bool> hasBought = (index) =>
        {
            bool bought = false;
            for (int i = 0; i < buy_index.Length; i++)
            {
                var v = buy_index[i];
                if (Int32.Parse(v) == index)
                {
                    bought = true;
                    break;
                }
            }
            return bought;
        };

        for (int i = 1; i <= 6; i++)
        {
            var item_info = items[i - 1];
            var item_ui = items_ui[i - 1];
            item_ui.Text("text_gold").text = prices[i - 1];
            var discount = Mathf.Max(50, 100 - actInfo.avalueData.buy_num * 10);
            item_ui["img_red"].SetActive(discount <= 50);
            item_ui["img_green"].SetActive(discount > 50);
            item_ui.Text("text_discount").text = discount + "%";
            item_ui.Text("text_count").text = "x" + GLobal.NumFormat(item_info.count);
            Cfg.Item.SetItemIcon(item_ui.Image("img_icon"), item_info.itemid);
            item_ui.Image("Img_qua").color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(item_info.itemid));
            var bought = hasBought(i);
            item_ui["text_bought"].SetActive(bought);
            item_ui["btn_buy"].SetActive(!bought);
            item_ui.Button("btn_buy").interactable = i != 6 || do_num < 7;
        }
    }

    private void UpdateBottomUI()
    {
        var free_count = actInfo.avalueData.free_count;
        UI["btn_refresh"].SetActive(free_count == 0);
        UI["btn_free_refresh"].SetActive(free_count > 0);
        if (free_count > 0)
            UI.Get<Text>("text_free_count").text = String.Format(Lang.Get("免费刷新次数 <Color=#00ff00ff>{0}</Color>/4"), free_count);
        else
            UI.Get<Text>("text_refresh_gold").text = actInfo.avalueData.refresh_gold.ToString();
    }

    public override void UpdateTime(long ts)
    {
        base.UpdateTime(ts);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (actInfo.LeftTime >= 0)
        {
            var span = new TimeSpan(0, 0, (int)actInfo.LeftTime);
            UI.Get<Text>("text_time").text = String.Format(Lang.Get("活动将于 {0}天{1}小时{2}分 后结束"), span.Days, span.Hours, span.Minutes);
        }
        else
            UI.Get<Text>("text_time").text = Lang.Get("活动已经结束");

        if (actInfo.avalueData.free_count > 0)
        {
            UI["btn_refresh"].SetActive(false);
            UI["btn_free_refresh"].SetActive(true);
        }
        else
        {
            UI["btn_refresh"].SetActive(true);
            UI["btn_free_refresh"].SetActive(false);
        }


        var cd = actInfo.avalueData.recover_ts + 6 * 3600 - TimeManager.ServerTimestamp;
        if (cd > 0)
        {
            UI["text_recover_ts"].SetActive(true);
            UI.Get<Text>("text_recover_ts").text = String.Format(Lang.Get("刷新次数恢复\n<Color=#00ff00ff>{0}</Color>"), GLobal.TimeFormat(cd));
        }
        else
        {
            UI["text_recover_ts"].SetActive(false);
            if (actInfo.avalueData.free_count == 0)
                ActivityManager.Instance.RequestUpdateActivityById(AID);
        }
    }
}
