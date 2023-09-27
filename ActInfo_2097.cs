using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2097 : ActivityInfo
{
    private List<Act2097ShopItemInfo> _shopItems;


    public List<Act2097ShopItemInfo> GetShopItems()
    {
        return _shopItems;
    }

    public override void InitUnique()
    {
        //初始化当前商店可购买的道具信息
        _shopItems = JsonMapper.ToObject<List<Act2097ShopItemInfo>>(_data.avalue["shop_info"].ToString());
    }

    public void ConvertItems(Action callback)
    {
        Rpc.Send<Act2097ConvertItem>("exchangeAct2097ExtraItem", null, data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_reward);
            Uinfo.Instance.AddItem(data.cost, false);
            //Uinfo.Instance.AddAndReduceItem(data.get_item, data.cost);
            EventCenter.Instance.UpdateActivityUI.Broadcast(2097);
            callback?.Invoke();
        });
    }

    public void BuyItem(Action<Act2097ShopItemInfo> callback, int itemId)
    {
        Rpc.Send<Act2097BuyShopItem>("exchangeAct2097ShopItem", Json.ToJsonString(itemId), data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_item);
            Uinfo.Instance.AddItem(data.cost, false);
            for (int i = 0; i < _shopItems.Count; i++)
            {
                if (_shopItems[i].pos == data.shop_info.pos)
                {
                    _shopItems[i] = data.shop_info;
                }
            }
            callback?.Invoke(data.shop_info);
            EventCenter.Instance.UpdateActivityUI.Broadcast(2097);
        });

    }

    public void GetExtraItemInfo(Action<List<Act2097ExtraItem>> callback)
    {
        Rpc.Send<List<Act2097ExtraItem>>("getAct2097ExtraItemInfo", null, data =>
        {
            callback?.Invoke(data);
        });
    }
}
public class Act2097ConvertItem
{
    public string cost;
    public string get_reward;
}
public class Act2097ExtraItem
{
    public int item_id;
    public int extra_count;
    public string trans;
}
public class Act2097BuyShopItem
{
    public string get_item;
    public string cost;
    public Act2097ShopItemInfo shop_info;
}

public class Act2097ShopItemInfo
{
    public int id;
    public int pos;
    public int exchange_times;
}