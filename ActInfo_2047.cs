using System.Collections.Generic;
using LitJson;
using System;

public class ActInfo_2047 : ActivityInfo
{
    public const int CloseTime = 86400; //24小时的秒数

    public Dictionary<string, string> RankItems;//排名奖励数据，tips查看奖励
    private List<Act2047StateInfo> _stateInfo;//势力积分信息
    public List<Act2047BuyInfo> BuyInfo;//该活动兑换物品的信息数据
    public int ActivePoint;//玩家的活跃点数
    public int CanGetReward;//是否领取最终奖励 0:未领取|1:已领取
    private List<int> _bills;
    public override void InitUnique()
    {
        _bills = new List<int>();
        RankItems = JsonMapper.ToObject<Dictionary<string, string>>(_data.avalue["rank_items"].ToString());
        _stateInfo = JsonMapper.ToObject<List<Act2047StateInfo>>(_data.avalue["state_info"].ToString());
        BuyInfo = JsonMapper.ToObject<List<Act2047BuyInfo>>(_data.avalue["buy_info"].ToString());
        ActivePoint = Convert.ToInt32(_data.avalue["active_point"]);
        CanGetReward = Convert.ToInt32(_data.avalue["get_reward"]);
        SetRefreshTime();//定时刷新事件
    }
    //根据国家id获取国家排名
    public int GetStateRank(int stateCode)
    {
        //由StateInfo中数据按照point和finish_ts进行排名
        //_stateInfo.Sort((a, b) =>
        //{
        //    if (a.point != b.point)
        //    {
        //        return a.point < b.point ? 1 : -1;
        //    }
        //    if (a.finish_ts != b.finish_ts)
        //    {
        //        return a.finish_ts < b.finish_ts ? -1 : 1;
        //    }
        //    return 0;
        //});

        _stateInfo.Sort(Sort_act2047);

        for (int i = 0; i < _stateInfo.Count; i++)
        {
            if (_stateInfo[i].code == stateCode)
            {
                return i;
            }
        }
        return -1;
    }
    private int Sort_act2047(Act2047StateInfo a, Act2047StateInfo b)
    {
        if (a.point != b.point)
        {
            return a.point < b.point ? 1 : -1;
        }
        if (a.finish_ts != b.finish_ts)
        {
            return a.finish_ts < b.finish_ts ? -1 : 1;
        }
        return 0;
    }
    //获取国家势力值
    public int GetStatePoint(int stateCode)
    {
        for (int i = 0; i < _stateInfo.Count; i++)
        {
            var state = _stateInfo[i];
            if (state.code == stateCode)
            {
                return state.point;
            }
        }
        return 0;
    }

    private void SetRefreshTime()
    {
        var time = _data.endts - CloseTime;
        if (time != 0 && time > TimeManager.ServerTimestamp && !UpdateManager.Instance.ContainEvent(time, RefreshAct))
        {
            UpdateManager.Instance.AddEvent(time, RefreshAct);
        }
    }
    public bool IsClosing()
    {
        return LeftTime <= CloseTime;
    }
    //刷新数据
    public void RefreshAct()
    {
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }

    public override bool IsAvaliable()
    {
        if (IsClosing())
        {
            return CanGetReward == 0;
        }
        CanExchange();
        if (_bills.Count > 0)
        {
            return ActivePoint >= _bills[0];
        }
        return false;
    }

    private void CanExchange()
    {
        _bills.Clear();
        for (int i = 0; i < BuyInfo.Count; i++)
        {
            var info = BuyInfo[i];
            if (info.already_buy < info.buy_count)
            {
                _bills.Add(info.cost);
            }
        }
    }
    //最终领奖，无需传参
    public void GetRewardFromState(Action callback)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getRewardFromState", null, data =>
        {
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(data.get_items);
            if (callback != null)//先ac 再广播
                callback();
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
    //兑换请求，传给服务端兑换id
    public void ExchangeItem(int id)
    {
        //Rpc.SendWithTouchBlocking<P_ActCommonReward>("exchangeItem", Json.ToJsonString(id), data =>
        //{
        //    var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
        //    Uinfo.Instance.AddItem(rewardsStr, true);
        //    MessageManager.ShowRewards(data.get_items);
        //    RefreshAct();
        //});
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("exchangeItem", Json.ToJsonString(id), On_exchangeItem_SC);
    }
    private void On_exchangeItem_SC(P_ActCommonReward data)
    {
        var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
        Uinfo.Instance.AddItem(rewardsStr, true);
        MessageManager.ShowRewards(data.get_items);
        RefreshAct();
    }
}
public class Act2047BuyInfo
{
    public string item;
    public int cost;
    public int already_buy;//已兑换数量
    public int buy_count;//总计兑换数量
    public int id;
}
public class Act2047StateInfo
{
    public int code;//国家
    public long finish_ts;
    public int point;
}

