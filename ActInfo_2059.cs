using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2059 : ActivityInfo
{
    public P_ActInfo2059 Info;


    public override void InitUnique()
    {
        Info = JsonMapper.ToObject<P_ActInfo2059>(_data.avalue["data"].ToString());
    }

    public override bool OnInited()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.GAIN_ITEM, CheckRefresh);
        return true;
    }
    public override void OnRemove()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.GAIN_ITEM, CheckRefresh);
    }

    public override bool IsAvaliable()
    {
        var opcode = PromptOpcode.BlackMarket;
        bool prompt = PromptInfo.Instance.GetValue(opcode);
        return prompt;
    }

    public void CheckRefresh(int opcode, string data)
    {
        string itemList = data;
        var pitems = GlobalUtils.ParseItem(itemList);//击败海盗运输船掉落海盗金币
        for (int i = 0; i < pitems.Length; i++)
        {
            if (pitems[i].id == ItemId.PirateCoin && IsDuration())
            {
                ActivityManager.Instance.RequestUpdateActivityById(_aid);
            }
        }
    }

    public void ExchangeItemByPirateCoin(P_BlackmarketItem item, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_ExchangeItem>("exchangeItemByPirateCoin", Json.ToJsonString(item.id), data =>
        {
            item.exchange_times = data.already_get;
            ItemHelper.AddAndReduceItem(data.get_item, data.cost_item);
            MessageManager.ShowRewards(data.get_item);

            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);

            if (callback != null)
                callback();
        });
    }

    //index  [0-7]
    public void LotteryDrawByPirateCoin(int index,Action<string> callback)
    {
        Rpc.SendWithTouchBlocking<P_LotteryDraw>("lotteryDrawByPirateCoin", Json.ToJsonString(index), data =>
        {
            Info.lotteryed_info = data.lotteryed_info;
            ItemHelper.AddAndReduceItem(data.get_item, data.cost_item);
            MessageManager.ShowRewards(data.get_item);

            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
            if (Info.lotteryed_info.Count == 0)
            {
                //_Scheduler.Instance.PerformWithDelay(0.5f, () =>
                //{
                //    if(IsDuration())
                //        EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
                //});
                _Scheduler.Instance.PerformWithDelay(0.5f, PerformCallback);
            }
            if (callback != null)
                callback(data.get_item);
        });
    }

    private void PerformCallback()
    {
        if (IsDuration())
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
    }


    public void FreshLotteryDrawPool(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_LotteryDraw>("freshLotteryDrawPool", null, data =>
        {
            Info.lotteryed_info.Clear();
            ItemHelper.AddItem(data.cost_item,false);

            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);

            if (callback != null)
                callback();
        });
    }


    public void Showed()
    {
        var opcode = PromptOpcode.BlackMarket;
        PromptInfo.Instance.SetPrompt(opcode, false);
        EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
    }
}
public class P_ActInfo2059
{
    public List<P_BlackmarketItem> user_exchange_list;//兑换列表
    public Dictionary<string, int> lotteryed_info;
    public int pirate_drop_coin;//海盗掉落的金币
}

public class P_BlackmarketItem
{
    public int id;//唯一id
    public int exchange_times;//已兑换次数
}

public class P_ExchangeItem
{
    public int already_get;//已兑换次数
    public string cost_item;//消耗的物品
    public string get_item;//返回的物品
}

public class P_LotteryDraw
{
    public Dictionary<string, int> lotteryed_info;
    public string cost_item;//消耗的物品
    public string get_item;//返回的物品
}