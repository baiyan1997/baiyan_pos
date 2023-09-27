using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2412: ActivityInfo
{
    public List<P_ActInfo2410RankPoint> RankPoints;
    public List<P_ActInfo2410RankReward> RankRewards;
    public List<P_ActInfo2410RankInfo> RankInfos;
    public P_ActInfo2410RankInfo MyRankInfo;
    public int RewardTs;
    public int MyRankGear;

    private bool _IsAvaliable;
    
    public override void InitUnique()
    {
        _IsAvaliable = false;
        RankPoints = JsonMapper.ToObject<List<P_ActInfo2410RankPoint>>(_data.avalue["cfg_rank_point"].ToString());
        RankRewards = JsonMapper.ToObject<List<P_ActInfo2410RankReward>>(_data.avalue["cfg_rank_reward"].ToString());
        RankInfos = JsonMapper.ToObject<List<P_ActInfo2410RankInfo>>(_data.avalue["rank_list"].ToString());
        MyRankInfo = JsonMapper.ToObject<P_ActInfo2410RankInfo>(_data.avalue["my_info"].ToString());
        RewardTs = int.Parse(_data.avalue["reward_ts"].ToString());
        if (MyRankInfo == null) MyRankInfo = new P_ActInfo2410RankInfo();
    }
    
    public override bool IsAvaliable()
    {
        return IsDuration() && _IsAvaliable;
    }

    public int GetRankGear(int rank)
    {
        
        for (int i=0;i< RankRewards.Count;i++)
        {
            var rankSegment = RankRewards[i].rank_segment.Split("|");
            if (rank >= int.Parse(rankSegment[0]) && rank <= int.Parse(rankSegment[1]))
                return RankRewards[i].rank_gear;
        }
        return 0;
    }

    public cfg_act_2410_rank GetCfgActRank()
    {
        return Cfg.Activity2410.GetCfgActRankForType(3);
    }
}

// public class P_ActInfo2412RankPoint
// {
//     public int rank_point ;
//     public int rank_gear ;
// }

// public class P_ActInfo2412RankReward
// {
//     public string rank_segment ;
//     public string rank_reward ;
//     public int rank_gear ;
// }

// public class P_ActInfo2412RankInfo
// {
//     public int uid ;
//     public string uname ;
//     public int do_number ;
//     public int ustate ;
//     public int cur_rank ;
// }