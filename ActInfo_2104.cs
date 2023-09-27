using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;

//运营活动--超限夺宝
public class ActInfo_2104 : ActivityInfo
{
    public List<P_Act2104SumDrawReward> ListSumDrawReward { get; private set; }//累计次数奖励
    public List<P_Act2104DrawPoolReward> ListDrawPool { get; private set; }//抽奖奖池
    public P_Act2104Rank RankInfo { get; private set; }//排名

    public long DrawEndTs { get; private set; }//抽奖结束时间
    public int ActStep { get; private set; }//抽奖阶段
    public int DrawCount { get; private set; }//累计抽奖次数
    public int GotRankReward { get; private set; }//是否领取排行榜奖励
    public int RewardShipId { get; private set; }//稀有奖品战舰id

    //返回是否处于抽奖阶段
    public bool IsInDrawStep
    {
        get
        {
            return IsDuration() && DrawEndTs > TimeManager.ServerTimestamp;
        }
    }

    public List<int> DrawIdList { get; private set; }//抽出的奖励

    public List<P_Act2104Gift> GiftInfo { get; private set; }//礼包信息

    private const int ActId = 2104;


    public override bool OnInited()
    {
        //rmb礼包购买成功推送
        EventCenter.Instance.AddPushListener(OpcodePush.BUY_2104_GIFT_SUCC, (opcode, data) =>
        {
            Uinfo.Instance.AddItemAndShow(data);
            ActivityManager.Instance.RequestUpdateActivityById(ActId);
        });
        return true;
    }

    public override void InitUnique()
    {
        ListSumDrawReward = JsonMapper.ToObject<List<P_Act2104SumDrawReward>>(_data.avalue["return_reward"].ToString());
        DrawEndTs = (int)_data.avalue["snatch_end_ts"];
        ActStep = (int)_data.avalue["draw_step"];
        DrawCount = (int)_data.avalue["draw_sum"];
        GotRankReward = (int)_data.avalue["is_get_rank_reward"];
        RewardShipId = (int)_data.avalue["ship_id"];
        ListDrawPool = JsonMapper.ToObject<List<P_Act2104DrawPoolReward>>(_data.avalue["pool_list"].ToString());
        GiftInfo = JsonMapper.ToObject<List<P_Act2104Gift>>(_data.avalue["gift_info"].ToString());
    }

    public override bool IsAvaliable()
    {
        return false;
    }
    
    //抽奖
    public void Draw(Action<P_Act2104Draw> callback)
    {
        Rpc.Send<P_Act2104Draw>("drawAct2104OverSnatch", Json.ToJsonString(1), data =>
        {
            DrawCount = data.draw_sum;
            ActStep = data.draw_step;
            ListDrawPool = data.pool_list;
            DrawIdList = data.draw_get_ids.Split(',').Select(int.Parse).ToList();

            Uinfo.Instance.AddAndReduceItem(data.get_items, data.cost_items);
            EventCenter.Instance.Act2104DrawUpdate.Broadcast();

            callback?.Invoke(data);
        });
    }
    public void Draw5Times(Action<P_Act2104Draw> callback)
    {
        Rpc.Send<P_Act2104Draw>("drawAct2104OverSnatch", Json.ToJsonString(5), data =>
        {
            DrawCount = data.draw_sum;
            ActStep = data.draw_step;
            ListDrawPool = data.pool_list;
            DrawIdList = data.draw_get_ids.Split(',').Select(int.Parse).ToList();

            Uinfo.Instance.AddAndReduceItem(data.get_items, data.cost_items);
            EventCenter.Instance.Act2104DrawUpdate.Broadcast();

            callback?.Invoke(data);
        });
    }
    /// <summary>
    /// 领取累计次数奖励
    /// </summary>
    /// <param name="count">抽奖次数</param>
    /// <param name="callback"></param>
    public void TakeSumReward(P_Act2104SumDrawReward info, Action<string> callback)
    {
        Rpc.Send<P_Act2104GetReward>("takeAct2104DrawNumReward", Json.ToJsonString(info.num), data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_item);
            info.is_get = 1;

            callback?.Invoke(data.get_item);
        });
    }
    //获取排行榜
    public void GetRankList(Action<P_Act2104Rank> callback)
    {
        Rpc.Send<P_Act2104Rank>("getAct2104RankList", null, data =>
        {
            RankInfo = data;

            callback?.Invoke(data);
        });
    }
    //领取排行奖励
    public void TakeRankReward(Action<string> callback)
    {
        Rpc.Send<P_Act2104GetReward>("takeAct2104RankReward", null, data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_item);

            callback?.Invoke(data.get_item);
        });
    }

    //购买氪晶礼包
    public void BuyGiftByGold(int id, Action<string> callback)
    {
        Rpc.SendWithTouchBlocking<P_Act2104GetAndCost>("buyAct2104GiftByGold", Json.ToJsonString(id), data =>
        {
            Uinfo.Instance.AddAndReduceItem(data.get_items, data.cost_item);

            callback?.Invoke(data.get_items);
        });
    }

    //购买rmb礼包
    public void BuyGiftByRMB(int giftId, int pid, Action callback)
    {
        P_Pay pay = PayConfig.GetAct2104PayData(pid);
        var platform = PlatformSdk.GetInstance();
        int priceLevel = Cfg.Payment.GetPriceLevel(pay._id);
        //platform.getPlatformName()
        var param = Json.ToJsonString(platform.GetChannel(), pay.GetPriceNum() * 100, User.Server.index, 12, giftId);
        if(platform.isPayNeedOrderId()) {
            Rpc.SendWithTouchBlocking("buyAct2104GiftByRMB", param,
            orderInfo =>
            {
                if ((int)orderInfo[0] == 10) //登录态失效，需要重新刷新
                {
                    platform.Jscode2session();
                    return;
                }
                var data = SdkHelper.CreatePayData(orderInfo[1].ToString(), Convert.ToInt32(orderInfo[2].ToString()), priceLevel.ToString(), pay.GetName(), pay.GetPriceNum(), 1);
                platform.DoPay(data);

                if (callback != null)
                    callback();
            });
        }
        else
        {
            var data = SdkHelper.CreatePayData("", 0, pay._id.ToString(), pay.GetName(), pay.GetPriceNum(), 1);
            data.funcStr = "buyAct2104GiftByRMB";
            data.paramStr = Json.ToJsonString(platform.GetChannel(), pay.GetPriceNum() * 100, User.Server.index, 12, giftId);
            platform.DoPay(data);
            if (callback != null)
                callback();
        }
    }

    public P_Act2104DrawPoolReward FindDrawRewardById(int id)
    {
        for(int i = 0; i < ListDrawPool.Count; i++)
        {
            if (id == ListDrawPool[i].id)
                return ListDrawPool[i];
        }
        return null;
    }

    //获得限量礼包剩余购买次数
    public int GetCurrentGiftBuyCount(int id)
    {
        for(int i = 0; i < GiftInfo.Count; i++)
        {
            var info = GiftInfo[i];
            if (id == info.id)
                return info.count;
        }
        return 0;
    }
}

public class P_Act2104Gift
{
    public int id;//礼包id
    public int count;//剩余可购买数
}

public class P_Act2104Rank
{
    public List<P_Act2104RankItem> rank_list;
    public P_Act2104RankItem my_info;
    public List<P_Act2104RankRewardCfg> cfg_rank_reward;
    public int top_rank_limit;//上榜要求抽取次数
}

public class P_Act2104RankItem
{
    public int index;//排名
    public string name;
    public int num;//抽取次数
    public int is_get_reward;//是否已领取奖励
    public int photo;//玩家头像
    public int photo_frame;//头像框
}

public class P_Act2104RankRewardCfg
{
    public int id;
    public int min_rank;//排名起始位
    public int max_rank;//排名终止位
    public string reward;//对应奖励
}

public class P_Act2104GetAndCost
{
    public string cost_item;
    public string get_items;
}

internal class P_Act2104GetReward
{
    public string get_item;
}

public class P_Act2104Draw
{
    public string cost_items;
    public string get_items;
    public int draw_sum;
    public int draw_step;
    public List<P_Act2104DrawPoolReward> pool_list;
    public string draw_get_ids;
}

//累计抽奖奖励
public class P_Act2104SumDrawReward
{
    public string reward;
    public int num;
    public int is_get;
}

public class P_Act2104DrawPoolReward
{
    public string item;
    public int count;
    public int is_win;
    public int id;
}