using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2088 : ActivityInfo
{

    public P_Act2088UniqueInfo UniqueInfo = new P_Act2088UniqueInfo();

    public int once_price = 1;
    //抽取十次消耗
    public int ten_times_price = 10;


    public override void InitUnique()
    {
        UniqueInfo.box_chip_num = int.Parse(_data.avalue["box_chip_num"].ToString());
        UniqueInfo.ExchangeList = JsonMapper.ToObject<List<P_Act2088Exchange>>(_data.avalue["shop_info"].ToString());
        UniqueInfo.pool_info = JsonMapper.ToObject<List<P_Act2088PoolInfoItem>>(_data.avalue["pool_info"].ToString());
        CalculateRemainingDrawNum();
    }

    public void StartRaffle(int type, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_Text0penBox>("blindBoxlukyDraw", Json.ToJsonString(type), data =>
        {
            Uinfo.Instance.AddAndReduceItem(data.get_item, data.cost_item);
            P_Item[] temp_items = GlobalUtils.ParseItem(data.get_item);
            UniqueInfo.DrawPrizes.Clear();
            for (int i = 0; i < temp_items.Length; i++)
            {
                var item = temp_items[i];
                UniqueInfo.DrawPrizes.Add(item);
            }
            Debug.LogError(UniqueInfo.DrawPrizes.Count);
            callback?.Invoke();
        });
    }

    public void ExchangeItems(int itemId, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_ExchangeShopItem>("buyInblindBoxShop", Json.ToJsonString(itemId), data =>
        {

            Debug.LogError(data.get_item);
            Uinfo.Instance.AddItemAndShow(data.get_item);
            callback?.Invoke();

        });
    }

    private void CalculateRemainingDrawNum()
    {
        UniqueInfo.DrawRemainingNum = 0;
        for (int i=0;i<  UniqueInfo.pool_info.Count;i++)
        {
            var reward = UniqueInfo.pool_info[i];
            UniqueInfo.DrawRemainingNum += Cfg.Activity2088.GetRewardData(reward.id).limit_num - reward.num;
        }
    }
}
public class P_Act2088UniqueInfo
{
    public int box_coin_num { get; set; }
    public int box_chip_num { get; set; }
    public List<P_Act2088Exchange> ExchangeList { get; set; }
    public List<P_Act2088PoolInfoItem> pool_info { get; set; }
    public List<P_Item> DrawPrizes { get; set; } = new List<P_Item>();
    public int DrawRemainingNum;
}

public class P_Act2088PoolInfoItem
{
    public int uid;
    public int num;
    public int id;
}
public class P_Act2088Exchange
{
    public int uid;
    public int num;
    public int id;
}
public class P_ExchangeShopItem
{
    public string get_item;
}
public class P_Text0penBox
{
    public string get_item;
    public string cost_item;
}