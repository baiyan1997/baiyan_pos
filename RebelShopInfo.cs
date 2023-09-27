using System;
using System.Collections.Generic;
using UnityEngine;

public class RebelShopInfo : Singleton<RebelShopInfo>
{

    private P_FlightRebelShopInfo _shopInfo;
    private List<P_RebelItem> sellItems;

    public List<P_RebelItem> GetSellItems()
    {
        return sellItems;
    }

    public int GetShopLv()
    {
        return _shopInfo.shop_lv;
    }
    public void InitRebelShop(Action callback = null)
    {
        Rpc.Send<P_FlightRebelShopInfo>("getRebelShopInfo", null, data =>
        {
            _shopInfo = data;
            callback?.Invoke();
        });
    }
    public void InitRebelShopSellInfo(Action callback = null)
    {
        Rpc.Send<P_FlightRebelShopInfo>("getRebelShopInfo", null, data =>
        {
            if (sellItems == null)
            {
                sellItems = new List<P_RebelItem>();
            }
            else
            {
                sellItems.Clear();
            }
            _shopInfo = data;
            if (data.shop_lv == 0)
            {
                callback?.Invoke();
                return;
            }
            var shop = Cfg.FlightRebel.GetFlightRebelShopData(data.shop_lv);

            string[] shopItems = shop.goods.Split(',');
            for (var i = 0; i < shopItems.Length; i++)
            {
                var shopItem = shopItems[i];
                var item = Cfg.FlightRebel.GetFlightRebelShopGoodsData(Convert.ToInt32(shopItem));
                P_Item good = new P_Item(item.good);
                P_Item cost = new P_Item(item.cost);
                P_RebelItem temPRebelItem = new P_RebelItem();
                temPRebelItem.id = item.id;
                temPRebelItem.itemid = good.id;
                temPRebelItem.cost = cost.Num;
                temPRebelItem.item_count = good.Num;
                sellItems.Add(temPRebelItem);
            }
            callback?.Invoke();
            //var nextStep = Cfg.FlightRebel.GetFlightRebelShopNextStep(data.shop_lv);
            //var nextMid = Cfg.FlightRebel.GetFlightRebelShopNextMid(data.shop_lv);
            //callBack?.Invoke(nextMid, nextStep);
        });
    }
    public void BuyItem(int itemId, int buyCount, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_FlightRebelInfo>("buyGoodInRebelShop", Json.ToJsonString(itemId, buyCount), data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_item);
            Uinfo.Instance.AddItem(data.cost_item, false);
            callback?.Invoke();

        });
    }
    public bool CheckRebelShopUnlock()
    {
        return _shopInfo.shop_lv != 0;
    }
}

public class P_FlightRebelInfo
{
    public string get_item;
    public string cost_item;
}

public class P_FlightRebelShopInfo
{
    public int shop_lv;
    public int step;
}

public class P_RebelItem
{
    //good表中的id
    public int id;
    //good的id
    public int itemid;
    //good的数量
    public int item_count;

    //public int buy_count;
    //public int max_count;

    //需要消耗的机体货币
    public int cost;
    //public int type;
}