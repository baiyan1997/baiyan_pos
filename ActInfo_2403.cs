using System;
using System.Collections.Generic;
public class ActInfo_2403 : ActivityInfo
{
    public int get_buy_reward = -1;
    public long last_daliy_reward = -1;
    public int buy_uid = -1;
    public override void InitUnique()
    {
        RequestData();
    }

    public override bool IsAvaliable()
    {
        return IsCanGet();
    }

    public bool IsCanGet()
    {
        if(get_buy_reward == 0 && IsBuyServer())
        {
            return true;
        }
        else if(get_buy_reward >= 0)
        {
            DateTime t1 = TimeManager.ToServerDateTime(last_daliy_reward);
            if(TimeManager.ServerDateTime.DayOfYear != t1.DayOfYear)
            {
                return true;
            }
        }
        return false;
    }

    //是否是购买的区服
    public bool IsBuyServer()
    {
        return buy_uid == User.Uid;
    }

    //是否已经购买
    public bool IsBuyAready()
    {
        return get_buy_reward < 0 && last_daliy_reward < 0;
    }

    public void RequestData()
    {
        Rpc.Send<Dictionary<string, object>>("getCrossTokenInfo", null, (data) =>
        {
            var a = data.GetValueOrDefault("get_buy_reward", null);
            if(a != null)
            {
                get_buy_reward = (int)a;
            }
            var b = data.GetValueOrDefault("last_get_ts", null);
            if(b != null)
            {
                last_daliy_reward = (int)b;
            }
            var c = data.GetValueOrDefault("buy_uid", -1);
            if(c != null)
            {
                buy_uid = (int)c;
            }
            RemindMark();
            EventCenter.Instance.UpdateActivityUI.Broadcast(ActivityID.SpanOrder);
        });
    }

    public void BuyAct()
    {
        var config = PayConfig.GetAct2043PayData();
        GiftManager.Instance.RequestRecharge(config);
    }

    public void GetRewrad(int type, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking("getCrossTokenReward", Json.ToJsonString(type), (data) =>
        {
            if(data != null && data.IsArray && data.Count > 0)
            {
                if(1 == (int)data[0]) //领取成功，刷新状态
                {
                    RequestData();
                    int nId = (int)data[1];
                    string rewardsStr = Cfg.Activity2403.GetRewardById(nId);
                    if(!string.IsNullOrEmpty(rewardsStr))
                    {
                        Uinfo.Instance.AddItem(rewardsStr, true);
                        MessageManager.ShowRewards(rewardsStr);
                    }
                    if(callBack != null)
                    {
                        callBack();
                    }
                }
            }
        });
    }
}