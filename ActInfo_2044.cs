using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2044 : ActivityInfo
{
    private Dictionary<string, string> _reward;
    public int once_price;
    public int ten_times_price;
    public P_Item3[] _rewards;//转盘奖励数据
    public List<int> RewardIndex;//抽奖所获取奖励index用于转盘效果显示
    public P_Item3[] Rewards;// 包含购买的资源
    public bool isShow;//红点显示效果
    public override void InitUnique()
    {
        _reward = JsonMapper.ToObject<Dictionary<string, string>>(_data.avalue["data"].ToString());
        once_price = Convert.ToInt32(_data.avalue["once_price"]);
        ten_times_price = Convert.ToInt32(_data.avalue["ten_times_price"]);
        //形如"1|123|41 的id|count|index 序列
        P_Item2[] rewards = GlobalUtils.ParseItem2(_reward["items"]);
        _rewards = new P_Item3[11];
        RewardIndex = new List<int>();
        for (int i = 0; i < rewards.Length; i++)//排序
        {
            int index = rewards[i].rate - 1;
            P_Item3 item = new P_Item3 { itemid = rewards[i].id, count = rewards[i].count };
            _rewards[index] = item;
        }
    }
    public override bool IsAvaliable()
    {
        if (!isShow)
        {
            return true;
        }
        return false;
    }
    public void SendStartLottory(int type,Action ac)
    {
        Rpc.SendWithTouchBlocking<P_Item3[]>("buyResGetReward", Json.ToJsonString(type), data =>
        {
            Rewards = data;
            RewardIndex.Clear();
            //扣除抽奖所用氪晶
            if (type == 1)
            {
                Uinfo.Instance.Player.AddGold(-once_price);
            }
            else
            {
                Uinfo.Instance.Player.AddGold(-ten_times_price);
            }
            for (int i = 0; i < _rewards.Length; i++)
            {
                for (int j = 0; j < data.Length; j++)
                {
                    if (data[j].itemid == _rewards[i].itemid && data[j].count == _rewards[i].count)
                    {
                        RewardIndex.Add(i);
                    }
                }
            }
            if (ac != null)
                ac();
        });
    }
}



