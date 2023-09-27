using System;

public class ActInfo_2083 : ActivityInfo
{
    public P_Act2083UniqueInfo UniqueInfo { get; } = new P_Act2083UniqueInfo();
    public bool IsGottenRewards { get; set; }
    public bool CanGetRewards { get; set; }
    public bool EndAct { get; set; }

    public override void InitUnique()
    {
        UniqueInfo.progress = Convert.ToInt32(_data.avalue["progress"].ToString());
        UniqueInfo.shield_value = Convert.ToInt32(_data.avalue["shield_value"].ToString());
        int exploreTimes = Convert.ToInt32(_data.avalue["explore_times"].ToString());
        UniqueInfo.explore_times = exploreTimes > 10 ? 10 : exploreTimes;
        UniqueInfo.all_reward = _data.avalue["all_reward"].ToString();
        if (UniqueInfo.progress / 100 == 100)
        {
            EndAct = true;
            CanGetRewards = true;
        }
        else if (UniqueInfo.shield_value == 0)
        {
            EndAct = true;
            CanGetRewards = GlobalUtils.ParseItem(UniqueInfo.all_reward).Length > 1;
        }
        IsGottenRewards = Convert.ToInt32(_data.avalue["received"].ToString()) == 1;

    }

    public void RefreshInfo(P_Act2083UniqueInfo info)
    {
        UniqueInfo.all_reward = info.all_reward;
        UniqueInfo.explore_times = info.explore_times > 10 ? 10 : info.explore_times;
        UniqueInfo.progress = info.progress;
        UniqueInfo.shield_value = info.shield_value;
        UniqueInfo.get_reward = info.get_reward;
        UniqueInfo.battle_report = info.battle_report;
        if (info.progress / 100 == 100)
        {
            EndAct = true;
            CanGetRewards = true;
        }
        else if (info.shield_value == 0)
        {
            EndAct = true;
            CanGetRewards = GlobalUtils.ParseItem(UniqueInfo.all_reward).Length > 1;
        }
        EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
        EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
    }

    public void RefreshShieldValue(int shieldValue)
    {
        UniqueInfo.shield_value = shieldValue;
        EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
    }

    public void RefreshEid(int eid)
    {
        UniqueInfo.eid = eid;
    }

    public override bool IsAvaliable()
    {
        return !IsGottenRewards && CanGetRewards;
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }

    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }
}



public class P_Act2083UniqueInfo
{
    //basic info
    public int progress;//航行进度
    public int shield_value;//防护值
    public int explore_times;//今日已经探索次数
    public string all_reward;//已经获得的全部奖励

    //event info
    public int eid;//某次探索事件id
    public string get_reward;//某次探索事件获得的奖励
    public P_Battle battle_report;//战斗事件信息

}

public class P_Act2083RepairInfo
{
    public string cost;
    public int shield_value;
}

public class P_Act2083GetRewardsActually
{
    public string get_rewards;
}
