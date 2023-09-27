using System;
using LitJson;
using UnityEngine;
using static System.String;

public class ActInfo_2086 : ActivityInfo
{
    public P_2086UniqueInfo UniqueInfo = new P_2086UniqueInfo();
    public bool HasCanExchange;
    public bool ChallengeCompleted;
    public bool CanOpenFree;
    public bool DailyChallengeRefresh;
    public override void InitUnique()
    {
        UniqueInfo.ResistPower = int.Parse(_data.avalue["resist"].ToString());
        UniqueInfo.OfferCount = int.Parse(_data.avalue["offer_count"].ToString());
        UniqueInfo.LastCount = int.Parse(_data.avalue["last_count"].ToString());
        UniqueInfo.MissionInfo =
            JsonMapper.ToObject<P_2086MissionInfo[]>(_data.avalue["mission_info"].ToString());
        UniqueInfo.ExchangeInfo =
            JsonMapper.ToObject<P_2086ExchangeInfo[]>(_data.avalue["exchange_info"].ToString());
        UniqueInfo.SpecialRewards = GlobalUtils.ParseItem(_data.avalue["show_reward"].ToString());
        UniqueInfo.FirstFreeOpenUsed = int.Parse(_data.avalue["first_challenge_status"].ToString()) == 0;
        UniqueInfo.SecondFreeOpenUsed = int.Parse(_data.avalue["second_challenge_status"].ToString()) == 0;
        CheckRemind();
    }

    private void CheckRemind()
    {
        CheckHasCanExchange();
        CheckHasTaskCompletedAndCanOpenFree();
        CheckDailyChallengeRefresh();
        EventCenter.Instance.RemindActivity.Broadcast(ActivityID.OrderTreasure, IsAvaliable());
    }

    private void CheckDailyChallengeRefresh()
    {
        DailyChallengeRefresh = false;
        long localEndTs = long.Parse(PlayerPrefs.GetString("end_ts", "-1"));
        long serverEndTs = UniqueInfo.MissionInfo[0].end_ts;
        if (localEndTs != serverEndTs)
        {
            DailyChallengeRefresh = true;
        }
    }

    public void CheckHasTaskCompletedAndCanOpenFree(bool[] arr = null)
    {
        ChallengeCompleted = false;
        CanOpenFree = false;
        int len = UniqueInfo.MissionInfo.Length;
        int num1 = 0;
        int num2 = 0;
        for (int i = 0; i < len; i++)
        {
            P_2086MissionInfo info = UniqueInfo.MissionInfo[i];
            if (Cfg.Activity2086.IsChallenge1(info.tid))
            {
                num1++;
                if (info.finished == 1)
                {
                    num1--;
                }
            }
            else
            {
                num2++;
                if (info.finished == 1)
                {
                    num2--;
                }
            }
        }

        if (arr == null)
        {
            arr = new[] { false, false };
        }
        arr[0] = num1 == 0;
        arr[1] = num2 == 0;
        if (num1 == 0 || num2 == 0)
        {
            ChallengeCompleted = true;
        }

        CanOpenFree = !UniqueInfo.FirstFreeOpenUsed && num1 == 0 || !UniqueInfo.SecondFreeOpenUsed && num2 == 0;
    }

    public void OfferOpen(int openType, Action callBack)
    {
        float disCount = UniqueInfo.OfferCount > 0 ? 0.8f : 1;
        int costNum = (int)((openType == 0 ? 80 : 800) * disCount);
        if (!ItemHelper.IsCountEnough(ItemId.Gold, costNum))
        {
            return;
        }
        Rpc.SendWithTouchBlocking<P_2086OpenBox>("openTreasureChestCostGold", Json.ToJsonString(openType), data =>
        {
            UniqueInfo.LastCount = data.last_count;
            UniqueInfo.ResistPower = data.resist;
            CheckHasCanExchange();
            UniqueInfo.OfferCount = data.offer_count;
            Uinfo.Instance.AddItem(data.cost_item, false);
            Uinfo.Instance.AddItem(data.all_reward, true);
            UniqueInfo.ExtraReward = null;
            if (data.extra_reward != Empty)
            {
                UniqueInfo.ExtraReward = GlobalUtils.ParseItem(data.extra_reward)[0];
            }
            UniqueInfo.RareRewards = GlobalUtils.ParseItem(data.rare_reward);
            UniqueInfo.AllRewards = GlobalUtils.ParseItem(data.all_reward);
            callBack?.Invoke();
        });
    }

    public void FreeOpen(int type, Action callBack)
    {
        Rpc.SendWithTouchBlocking<P_2086OpenBox>("openTreasureChestFree", Json.ToJsonString(type), data =>
            {
                UniqueInfo.LastCount = data.last_count;
                UniqueInfo.ResistPower = data.resist;
                CheckHasCanExchange();
                UniqueInfo.ExtraReward = null;
                if (data.extra_reward != Empty)
                {
                    UniqueInfo.ExtraReward = GlobalUtils.ParseItem(data.extra_reward)[0];
                }
                UniqueInfo.AllRewards = GlobalUtils.ParseItem(data.all_reward);
                Uinfo.Instance.AddItem(data.all_reward, true);
                UniqueInfo.RareRewards = GlobalUtils.ParseItem(data.rare_reward);
                if (type == 1)
                {
                    UniqueInfo.FirstFreeOpenUsed = true;
                }
                else
                {
                    UniqueInfo.SecondFreeOpenUsed = true;
                }
                CheckHasTaskCompletedAndCanOpenFree();
                callBack?.Invoke();
            });
    }

    public void Exchange(int id, Action callBack)
    {
        Rpc.SendWithTouchBlocking<P_Exchange>("exchangeRewardByResit", Json.ToJsonString(id), data =>
         {
             RefreshExchangeInfo(id);
             CheckHasCanExchange();
             Uinfo.Instance.AddItemAndShow(data.get_reward);
             callBack?.Invoke();
         });
    }

    private void RefreshExchangeInfo(int id)
    {
        int len = UniqueInfo.ExchangeInfo.Length;
        for (int i = 0; i < len; i++)
        {
            var info = UniqueInfo.ExchangeInfo[i];
            if (info.id == id)
            {
                info.state = 1;
                return;
            }
        }
    }

    private void CheckHasCanExchange()
    {
        HasCanExchange = false;
        int len = UniqueInfo.ExchangeInfo.Length;
        for (int i = 0; i < len; i++)
        {
            P_2086ExchangeInfo info = UniqueInfo.ExchangeInfo[i];
            if (Cfg.Activity2086.GetExchangeRewardInfo(info.id).need_count <= UniqueInfo.ResistPower)
            {
                if (info.state == 1)
                {
                    continue;
                }
                info.state = 0;
                HasCanExchange = true;
            }
        }
    }

    public override bool IsAvaliable()
    {
        return HasCanExchange || ChallengeCompleted && CanOpenFree || DailyChallengeRefresh;
    }
    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }
}

public class P_2086UniqueInfo
{
    public int ResistPower;//当前抵抗之力
    public int OfferCount;//剩余优惠次数
    public int LastCount;//额外奖励剩余次数
    public P_Item[] SpecialRewards;//主页面三个稀有奖励
    public P_2086MissionInfo[] MissionInfo;//任务信息
    public bool FirstFreeOpenUsed;//挑战1的免费开箱机会是否使用
    public bool SecondFreeOpenUsed;//挑战2的免费开箱机会是否使用
    public P_2086ExchangeInfo[] ExchangeInfo;//正义联盟兑换信息
    //开箱子
    public P_Item ExtraReward;
    public P_Item[] RareRewards;
    public P_Item[] AllRewards;

}
public class P_2086MissionInfo
{
    public int tid;//任务id
    public int do_number;//已经完成的次数
    public int finished;//是否完成
    public long end_ts;//结束时间
}

public class P_2086ExchangeInfo
{
    public int id;//兑换id
    public int state;//兑换状态 已兑换|1，未兑换|0，未达到|-1
    public string reward;//可兑换奖励
    public int need_count;//需要的抵抗之力
}

public class P_Exchange
{
    public string get_reward;
}
public class P_2086OpenBox
{
    public int last_count;
    public string cost_item;
    public string extra_reward;
    public string rare_reward;
    public int resist;
    public int offer_count;
    public string all_reward;
}