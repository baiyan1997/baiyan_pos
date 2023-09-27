using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;

public class ActInfo_ActivityRank : ActivityInfo
{

    public List<P_RankUserData> _userData;
    public List<P_RewardData> _rewardData;
    public P_RankUserData _myInfo;
    public int rewardTs;//领奖时间
    public int _rewardLv;//奖励档位
    public override void InitUnique()
    {
        if (_data.avalue == null)
        {
            throw new Exception("ActivityRank info avalue should not empty id= " + _aid);
        }

        object infoObj;
        _data.avalue.TryGetValue("data", out infoObj);
        if (infoObj == null)
        {
            throw new Exception("ActivityRank info avalue[data] should not empty id= " + _aid);
        }
        string info = infoObj.ToString();
        P_dataActRank actdata = JsonMapper.ToObject<P_dataActRank>(info);
        for (int i = 0; i < actdata.act_data.Count; i++)
        {
            var data = actdata.act_data[i];
            data.rankItem = JsonMapper.ToObject<P_ActRankItem>(data.data);
        }
        for (int i = 0; i < actdata.cfg_data.Count; i++)
        {
            var data = actdata.cfg_data[i];
            data.rewards = GlobalUtils.ParseItem3(data.reward);
        }
        actdata.my_info.rankItem = JsonMapper.ToObject<P_ActRankItem>(actdata.my_info.data);
        //设置已领
        string[] gets = actdata.my_info.rankItem.get_reward.Split(',');
        for (int i = 0; i < actdata.cfg_data.Count; i++)
        {
            int id = actdata.cfg_data[i].id;
            if (gets.Contains(id.ToString()))
                actdata.my_info.rankItem.hasGet[id] = true;
            else
                actdata.my_info.rankItem.hasGet[id] = false;
        }

        _userData = actdata.act_data;
        _rewardData = actdata.cfg_data;
        _myInfo = actdata.my_info;
        _rewardLv = GetRankLv(_myInfo.rankItem.history_rank);
        rewardTs = actdata.reward_ts;
        if (rewardTs != 0 && rewardTs > TimeManager.ServerTimestamp && !UpdateManager.Instance.ContainEvent(rewardTs, RefreshAct))
        {
            UpdateManager.Instance.AddEvent(rewardTs, RefreshAct);
        }
    }
    private void RefreshAct()
    {
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }
    public override bool IsAvaliable()
    {
        if (_myInfo == null || _myInfo.rankItem == null)
            return false;
        if (TimeManager.ServerTimestamp > rewardTs)
        {
            foreach (var kv in _myInfo.rankItem.hasGet)
            {
                //裂变粒子奖励和其他的排行榜不同 只有一个奖励档能领
                if (_data.aid != ActivityID.FissionParticleRank)
                {
                    if (!kv.Value && kv.Key >= _rewardLv && _rewardLv != 0)//没领 && 可领 && 有奖励
                        return true;
                }
                else
                {
                    if (!kv.Value && kv.Key == _rewardLv && _rewardLv != 0)//没领 && 可领 && 有奖励
                        return true;
                }
            }
            return base.IsAvaliable();
        }
        return base.IsAvaliable();
    }

    public bool IsOpen()
    {
        if (TimeManager.ServerTimestamp >= rewardTs)
        {
            return true;
        }
        return false;
    }
    public int GetRankLv(int rank)
    {
        for (int i = 0; i < _rewardData.Count; i++)
        {
            var reward = _rewardData[i];
            if (rank >= reward.min_rank && rank <= reward.max_rank)
                return reward.id;
        }
        return 0;
    }
    public void GetRewardById(int id, Action ac)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getActRankReward", Json.ToJsonString(_aid, id), data =>
        {
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(data.get_items);
            if (ac != null)//先ac 再广播
                ac();
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
}

public class P_dataActRank
{
    public List<P_RankUserData> act_data;
    public List<P_RewardData> cfg_data;
    public P_RankUserData my_info;
    public int reward_ts;
}

public class P_RankUserData
{
    public int uid;
    public string data;
    public long do_number;
    //    public int finish_ts;//服务器排名用
    public int cur_rank;
    public P_ActRankItem rankItem;
}

public class P_ActRankItem
{
    public int map_step;
    public string uname;
    public string get_reward;
    public int history_rank;
    public int ustate;
    public Dictionary<int, bool> hasGet = new Dictionary<int, bool>(8);
}

public class P_RewardData
{
    public string reward;
    public int max_rank;
    public int step;
    public int min_rank;
    public int id;
    public int aid;
    public P_Item3[] rewards;
    //客户端排序用
    public int state;
}