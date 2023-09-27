using System;
using System.Collections.Generic;

//委托商店和委托同步开启
public class EntrustShopInfo : Singleton<EntrustShopInfo>
{
    private List<P_EntrustItem> sellItems;
    private List<P_Item> sellItemsOver;

    public List<P_EntrustItem> GetSellItems()
    {
        return sellItems;
    }

    public List<P_Item> GetSellItemsOverflow()
    {
        return sellItemsOver;
    }

    public override void OnBegin()
    {
        base.OnBegin();
        EventCenter.Instance.AddPushListener(OpcodePush.ENTRUSTSHOP_UPDATE, _EventENTRUSTSHOP_UPDATE);
    }
    public override void OnEnd()
    {
        base.OnEnd();
        EventCenter.Instance.RemovePushListener(OpcodePush.ENTRUSTSHOP_UPDATE, _EventENTRUSTSHOP_UPDATE);
    }

    private void _EventENTRUSTSHOP_UPDATE(int opcode, string data)
    {
        Refresh(null);
    }

    public void Refresh(Action callback = null)
    {
        Rpc.Send<P_EntrustShopInfo>("getEntrustShopInfo", null, data =>
        {
            sellItems = data.entrust_shop_info;
            sellItemsOver = data.overflowing_battleship_parts;

            callback?.Invoke();

            EventCenter.Instance.EntrustShopUpdate.Broadcast();
        });
    }

    public void BuyItem(int id, int itemId, int itemCount, int buyCount, string cost, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_EntrustInfo>("buyGoodsInEntrustShop", Json.ToJsonString(itemId, itemCount, buyCount, cost, id), data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_item);
            Uinfo.Instance.AddItem(data.cost, false);
            callback?.Invoke();
        });
    }

    //溢出s舰零件转换
    public void OverflowPartConversion(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_PartConversion>("overflowPartConversion", null, data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_item);
            Uinfo.Instance.AddItem(data.cost_item, false);

            if (callback != null)
                callback();
        });
    }


    //得到转换目标道具
    public P_Item GetGoalItem()
    {
        long sum = 0;
        for(int i = 0; i < sellItemsOver.Count; i ++)
        {
            sum += sellItemsOver[i].count;
        }

       int id = Cfg.FuncAttr.GetIntAttrByName("entrust_exchange_itemId");
       float rate = Cfg.FuncAttr.GetIntAttrByName("entrust_exchange_rate");

        return new P_Item(id, (int)(sum * rate));
    }
}

public class P_EntrustInfo
{
    public string get_item;
    public string cost;
}

public class P_EntrustShopInfo
{
    public List<P_EntrustItem> entrust_shop_info;
    public List<P_Item> overflowing_battleship_parts;
}

public class P_EntrustItem
{
    public int id;
    public int item_id;
    public int count;
    //需要消耗的机体货币
    public string cost;
    public int type;//商品类型 1-原核，2-准备碎片，3-战舰零件
    public int limit;//购买次数限制 0-无限制
    public int ship_id;
}


public class P_PartConversion
{
    public string get_item;
    public string cost_item;
}