using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 庆典红包
/// </summary>
public class ActInfo_2062 : ActivityInfo
{
    public enum ActivityStatus
    {
        /// <summary>
        /// 累计阶段
        /// </summary>
        Waitting,
        /// <summary>
        /// 领取奖励阶段
        /// </summary>
        Reward,
    }

    public const int Aid = 2062;
    /// <summary>
    /// 活动 累计持续天数
    /// </summary>
    public const int DayCount = 7;

    public static ActInfo_2062 GetData()
    {
        return (ActInfo_2062)ActivityManager.Instance.GetActivityInfo(Aid);
    }


    /// <summary>
    /// [获得的氪金]
    /// </summary>
    public JDEvent<int> GetAct2062Reward = new JDEvent<int>();


    List<P_Activity2062DayInfo> _daysInfo;

    /// <summary>
    /// 领奖时间开始时间，也是消费投资的截止时间
    /// </summary>
    public long RewardTs { get; private set; }

    /// <summary>
    /// 是否红点
    /// </summary>
    /// <returns></returns>
    public override bool IsAvaliable()
    {
        ActivityStatus status = GetStatus();
        if (status != ActivityStatus.Reward) { return false; }

        if (!HasGotReward())
        {
            if (_daysInfo == null) { return false; }
            if (_daysInfo.Exists((dayInfo) => { return dayInfo.CanGetReward(); })) { return true; }
        }

        return false;
    }

    public override void InitUnique()
    {
        if (_data.avalue == null)
            throw new ArgumentNullException("_data.avalue", "ActInfo_2062 _data.avalue cannot be null");

        RewardTs = long.Parse(_data.avalue["reward_ts"].ToString());
        object mission_info = _data.avalue["mission_info"];
        string mission_info_string = mission_info.ToString();
        _daysInfo = JsonMapper.ToObject<List<P_Activity2062DayInfo>>(mission_info_string);
        if (_daysInfo == null)
        {
            throw new ArgumentNullException("_daysInfo");
        }
        if (_daysInfo.Count != DayCount)
        {
            throw new ArgumentException(string.Format("_daysInfo.Count must be [{0}], but now is [{1}];\nmission_info_string=[{2}]", DayCount, _daysInfo.Count, mission_info_string), "_dasInfo.Count");
        }
        // _daysInfo.Sort((data1, data2) => data1.tid - data2.tid);
        _daysInfo.Sort(Sort_act2062);
    }
    private int Sort_act2062(P_Activity2062DayInfo data1, P_Activity2062DayInfo data2)
    {
        return data1.tid - data2.tid;
    }
    public ActivityStatus GetStatus()
    {
        return TimeManager.ServerTimestamp < RewardTs ? ActivityStatus.Waitting : ActivityStatus.Reward;
    }

    //是否已经领取过奖励
    public bool HasGotReward()
    {
        return _data.get_all_reward;
    }

    /// <summary>
    /// 获取某一天存了多少氪金
    /// </summary>
    /// <param name="day">第x天</param>
    /// <returns></returns>
    public int GetSavedGold(int day)
    {
        var dayInfo = FindDayInfo(day);
        return dayInfo.RewardGold();
    }

    /// <summary>
    /// 获取某天消耗了多少氪金
    /// </summary>
    /// <param name="day"></param>
    /// <returns></returns>
    public int GetConsumedGold(int day)
    {
        var dayInfo = FindDayInfo(day);
        return dayInfo.do_number;
    }

    /// <summary>
    /// 累计可以领取的氪金数
    /// </summary>
    /// <returns></returns>
    public int GetTotalRewardGold()
    {
        int total = 0;
        for (int i = 0; i < DayCount; i++)
        {
            total += _daysInfo[i].RewardGold();
        }
        return total;
    }

    public P_Activity2062DayInfo FindDayInfo(int day)
    {
        if (day < 1 || day > DayCount)
        {
            throw new ArgumentOutOfRangeException("day", string.Format("day must be [1-7]:input is {0}", day));
        }
        int count = _daysInfo.Count;
        for (int i = 0; i < count; i++)
        {
            if (_daysInfo[i].tid == day)
            {
                return _daysInfo[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 从活动开始到现在的天数(第一天返回0,第二天返回1......)
    /// </summary>
    /// <returns></returns>
    public int DaysFromStart()
    {
        long ts = TimeManager.ServerTimestamp;
        if (ts >= RewardTs) { return DayCount; } //领奖阶段认为是第8天了
        for (int i = 0; i < DayCount; i++)
        {
            var dayInfo = _daysInfo[i];
            if (ts >= dayInfo.start_ts && ts < dayInfo.end_ts)
            {
                return i;
            }
        }
        //如果没有找到就认为是第8天
        return DayCount;
    }

    /// <summary>
    /// 下一次状态改变的时间戳
    /// </summary>
    /// <returns></returns>
    public long NextStatusChangeTs()
    {
        int day = DaysFromStart();
        if (day < DayCount)
        {
            return _daysInfo[day].end_ts;
        }
        else
        {
            //超过第7天就认为是活动截止时间
            return _data.endts;
        }
    }

    //----------请求-------------
    /// <summary>
    /// 请求获得奖励
    /// </summary>
    public void GetReward()
    {
        //Rpc.SendWithTouchBlocking("getAct2062Reward", null, data =>
        //{
        //    int retCode = (int)data[0];
        //    if (retCode != 1)
        //    {
        //        Alert.Ok(Lang.TranslateJsonString((string)data[1]));
        //        return;
        //    }
        //    int getGold = (int)data[1]["get_gold"];
        //    GetAct2062Reward.Broadcast(getGold);        //先广播GetAct2062Reward
        //    ItemHelper.AddItem(ItemId.Gold, getGold);       //添加 获得的氪金
        //    //RequestRefresh //获得氪金会刷新活动信息
        //});
        Rpc.SendWithTouchBlocking("getAct2062Reward", null, On_getAct2062Reward_SC);
    }
    private void On_getAct2062Reward_SC(JsonData data)
    {
        int retCode = (int)data[0];
        if (retCode != 1)
        {
            Alert.Ok(Lang.TranslateJsonString((string)data[1]));
            return;
        }
        int getGold = (int)data[1]["get_gold"];
        GetAct2062Reward.Broadcast(getGold);        //先广播GetAct2062Reward
        ItemHelper.AddItem(ItemId.Gold, getGold);       //添加 获得的氪金
                                                        //RequestRefresh //获得氪金会刷新活动信息
    }

    /// <summary>
    /// 刷新活动信息
    /// </summary>
    public void RequestRefresh()
    {
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }
}

/// <summary>
/// 活动一天的信息
/// </summary>
public class P_Activity2062DayInfo
{
    /// <summary>
    /// do_number是花了多少氪金
    /// </summary>
    public int do_number;

    /// <summary>
    /// data是算出来可以得到的多少奖励氪金
    /// </summary>
    public int data;
    /// <summary>
    /// tid是对应的day 1-7，
    /// </summary>
    public int tid;
    /// <summary>
    /// finished 是1是代表可以领取了，
    /// </summary>
    public int finished;
    /// <summary>
    /// get_reward代表领了没有
    /// </summary>
    public int get_reward;

    /// <summary>
    /// 这一天开始的时间戳
    /// </summary>
    public long start_ts;
    /// <summary>
    /// 这一天结束的时间戳
    /// </summary>
    public long end_ts;

    /// <summary>
    /// 可以得到的多少奖励氪金
    /// </summary>
    /// <returns></returns>
    public int RewardGold()
    {
        return data;
    }

    /// <summary>
    /// 判断这一天是否有奖励可以领取
    /// </summary>
    /// <returns></returns>
    public bool CanGetReward()
    {
        //只有可得到的奖励大于0才可以领取
        return RewardGold() > 0;
    }
}
