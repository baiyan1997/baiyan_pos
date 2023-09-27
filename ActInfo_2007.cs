using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2007 : ActivityInfo
{
    private const int TicketId = 30005;
    private Dictionary<string, string> _reward; 
    public P_Item3[] _rewards;
    public int RewardIndex;
    public int LeftTickets()
    {
        return (int)Uinfo.Instance.Bag.GetItemCount(TicketId);
    }
    public override void InitUnique()
    {
        _reward = JsonMapper.ToObject<Dictionary<string, string>>(_data.avalue["data"].ToString());
        //形如"1|123|41 的id|count|index 序列
        P_Item2[] rewards = GlobalUtils.ParseItem2(_reward["items"]);
        _rewards= new P_Item3[10];
        for (int i = 0; i < rewards.Length; i++)//排序
        {
            int index = rewards[i].rate-1;
            P_Item3 item = new P_Item3 { itemid = rewards[i].id, count = rewards[i].count };
            _rewards[index] = item;
        }
    }

    public override bool IsAvaliable()
    {
        return LeftTickets() > 0;
    }

    public void SendStartLottory(Action ac)
    {
        Rpc.SendWithTouchBlocking<List<Dictionary<string,int>>>("drawOneCard", null, data =>
        {
            Uinfo.Instance.AddItem(TicketId,-1);
            Debug.Log("===itemid=" + data[0]["itemid"] + " count=" + data[0]["count"]);
            for (int i = 0; i < _rewards.Length; i++)
            {
                if (data[0]["itemid"] == _rewards[i].itemid && data[0]["count"] == _rewards[i].count)
                    RewardIndex = i;
            }
            Debug.Log("====rewardIndex=" + RewardIndex);
            if (ac != null)
                ac();
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
}