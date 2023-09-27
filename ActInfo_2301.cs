﻿using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2301 : ActivityInfo
{

    public P_2301UniqueInfo UniqueInfo { get; } = new P_2301UniqueInfo();
    public override void InitUnique()
    {
        UniqueInfo.type = int.Parse(_data.avalue["type"].ToString());
        //UniqueInfo.exp = int.Parse(_data.avalue["exp"].ToString());
        UniqueInfo.price = int.Parse(_data.avalue["price"].ToString());
        UniqueInfo.lv_price = int.Parse(_data.avalue["lv_price"].ToString());
        UniqueInfo.price_info = JsonMapper.ToObject<P_2301UnlockHighRewardPool>(_data.avalue["price_info"].ToString());
        UniqueInfo.reward_info = JsonMapper.ToObject<List<P_2301Item>>(_data.avalue["reward_info"].ToString());
        UniqueInfo.reward_info.Sort((a, b) => a.exp - b.exp);
        UniqueInfo.mission_info = JsonMapper.ToObject<List<P_2301Mission>>(_data.avalue["mission_info"].ToString());
    }

    public void UnlockHighLevel(Action callBack)
    {
        switch (UniqueInfo.type)
        {
            case 1:
                var platform = PlatformSdk.GetInstance();
                string platName = platform.GetChannel();
                if(platform.isPayNeedOrderId()) {
                    Rpc.SendWithTouchBlocking("payForAct2301PrizePro", Json.ToJsonString(platName, UniqueInfo.price_info.price * 100, User.Server.index, UniqueInfo.price), orderInfo =>
                    {
                        if ((int)orderInfo[0] == 10) //登录态失效，需要重新刷新
                        {
                            platform.Jscode2session();
                            return;
                        }
                        var data = SdkHelper.CreatePayData(orderInfo[1].ToString(), Convert.ToInt32(orderInfo[2].ToString()), UniqueInfo.price_info.id_price_level, UniqueInfo.price_info.name, UniqueInfo.price_info.price, 1);
                        platform.DoPay(data);
                        //UniqueInfo.lv_price = 2;
                        callBack?.Invoke();
                    });
                }else {
                    var data = SdkHelper.CreatePayData("", 0, UniqueInfo.price.ToString(), UniqueInfo.price_info.name, UniqueInfo.price_info.price, 1);
                    data.funcStr = "payForAct2301PrizePro";
                    data.paramStr = Json.ToJsonString(platName, UniqueInfo.price_info.price * 100, User.Server.index, UniqueInfo.price);
                    platform.DoPay(data);
                    callBack?.Invoke();
                }
                break;
            case 2:
                Rpc.SendWithTouchBlocking<string>("payForAct2301PrizeProByGold", Json.ToJsonString(UniqueInfo.price), data =>
                {
                    Uinfo.Instance.AddItem(data, false);
                    UniqueInfo.lv_price = 2;
                    callBack?.Invoke();
                });
                break;
            default:
                throw new Exception($"error type:{UniqueInfo.type}");
        }
    }
    private string GetBannerName()
    {
        //活动名时间戳 唯一确定banner名称
        var strs = _data.bg_url.Split('.');
        return _data.aid.ToString() + _data.startts + "." + strs[strs.Length - 1];
    }
    public string GetBannerFullPath()
    {
        return FileStrategy.WritableDir + "/ActivityImg/" + GetBannerName();
    }

    public void GetReward(int itemInfoId, Action callBack)
    {
        Rpc.SendWithTouchBlocking<List<P_2301Item>>("getAct2301Reward", Json.ToJsonString(itemInfoId), data =>
             {
                 UniqueInfo.reward_info = data;
                 UniqueInfo.reward_info.Sort((a, b) => a.exp - b.exp);
                 callBack?.Invoke();
             });
    }

    public bool HasNewExpCanGet()
    {
        var list = UniqueInfo.mission_info;
        int len = list.Count;
        for (int i = 0; i < len; i++)
        {
            if (list[i].CanGetReward())
            {
                return true;
            }
        }
        return false;
    }

    public void GetExp(int tid, Action callBack)
    {
        Rpc.SendWithTouchBlocking<string>("getAct2301TaskExp", Json.ToJsonString(tid), data =>
        {
            var list = UniqueInfo.mission_info;
            int len = list.Count;
            for (int i = 0; i < len; i++)
            {
                if (tid == list[i].tid)
                {
                    list[i].get_reward = 1;
                    break;
                }
            }
            Uinfo.Instance.AddItemAndShow(data);
            EventCenter.Instance.RemindActivity.Broadcast(2301, IsAvaliable());
            callBack?.Invoke();
        });
    }

    public override bool IsAvaliable()
    {
        return HasNewExpCanGet() || DoDailyRemind();
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }
}

public class P_2301UniqueInfo
{
    public int type;//活动类型 1充值活动  2氪金活动
    public int price;//若为消耗氪金活动 需要消耗氪金数量|若为充活动 充值内购码
    public int lv_price;//高级奖池是否解锁 1 当前为普通奖池 2 高级奖池
    public P_2301UnlockHighRewardPool price_info;
    public List<P_2301Item> reward_info;//经验奖励条目
    //public int exp;//当前经验值
    public List<P_2301Mission> mission_info;
}

public class P_2301Mission
{
    public string reward;
    public int finished;//任务是否完成
    public int get_reward;//奖励是否领取
    public int tid;
    public int need_count;
    public int do_number;
    public string name;

    public bool CanGetReward()
    {
        return finished == 1 && get_reward == 0;
    }
}

public class P_2301Item
{
    public int id;
    public int exp;
    public string prize_free;
    public int free_get;//普通奖励 1 已领取 0未领取
    public string prize_pro_1;
    public string prize_pro_2;
    public int pro_get; //高级奖励 1 已领取 0未领取

    public bool IsNormalGet()
    {
        return free_get == 1;
    }

    public bool IsProGet()
    {
        return pro_get == 1;
    }

    public string Prizepro1 => string.IsNullOrEmpty(prize_pro_1) ? prize_pro_2 : prize_pro_1;

    public bool OnlyOneProReward()
    {
        return string.IsNullOrEmpty(prize_pro_1) && !string.IsNullOrEmpty(prize_pro_2)
               || !string.IsNullOrEmpty(prize_pro_1) && string.IsNullOrEmpty(prize_pro_2);
    }
}

public class P_2301UnlockHighRewardPool
{
    public int id;
    public string id_price_level;
    public string name;
    public string price_cn;
    public float price => float.Parse(price_cn);
}

