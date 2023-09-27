using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Random = UnityEngine.Random;

public class ActInfo_2090 : ActivityInfo
{

    //所有奖励信息
    private List<P_Item> _allRewardsInfo;

    //内圈倍数

    private List<int> _timesInfo;

    //所有记录信息
    private List<TurntableRewardRecordInfo> _recordList;

    private int _freeTimes;

    private int _aid = 2090;
    public int GetFreeTimes()
    {
        return _freeTimes;
    }

    public List<int> GetTimesInfo()
    {
        return _timesInfo;
    }
    public override void InitUnique()
    {
        //此处初始化所有记录信息和奖励信息

        _freeTimes = int.Parse(_data.avalue["free_times"].ToString());
        _recordList = JsonMapper.ToObject<List<TurntableRewardRecordInfo>>(_data.avalue["rew_msg"].ToString());
        List<Act2090Info> rewardList = JsonMapper.ToObject<List<Act2090Info>>(_data.avalue["reward_list"].ToString());

        _allRewardsInfo = new List<P_Item>();
        _timesInfo = new List<int>();

        for (int i = 0; i < rewardList.Count; i++)
        {
            var one = rewardList[i];
            P_Item tempReward = new P_Item(one.outer_reward);
            _allRewardsInfo.Add(tempReward);
            _timesInfo.Add(one.inner_mul);
        }
        EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
    }


    public override bool IsAvaliable()
    {
        return _freeTimes < 3;
    }


    public List<TurntableRewardRecordInfo> GetRecordList()
    {
        return _recordList;
    }

    public List<P_Item> GetAllRewardsInfo()
    {
        return _allRewardsInfo;
    }
    //转动转盘
    public void TurnTurntable(Action<int, int, P_Item> callback)
    {
        int type = 2;
        if (_freeTimes < 3)
        {
            type = 1;
        }

        int lastFreeTimes = _freeTimes;
        Rpc.SendWithTouchBlocking<TurnTurntableInfo>("getTurnCircleReward", Json.ToJsonString(type), data =>
        {
            P_Item reward = new P_Item(data.get_reward);

            Uinfo.Instance.AddItem(reward.Id, reward.count * reward.extra);
            if (!string.IsNullOrEmpty(data.cost))
            {

                Uinfo.Instance.AddItem(data.cost, false);
            }
            //此处需要更新记录信息
            _recordList = data.rew_msg;
            _freeTimes = data.free_times;


            int target1 = GetTarget(reward.id, reward.count);
            int target2 = GetTimes(reward.extra);

            if (lastFreeTimes < 3 && _freeTimes >= 3)
            {
                EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
            }
            callback?.Invoke(target1, target2, reward);
        });
    }

    private int GetTarget(int rewardId, int count)
    {
        for (int i = 0; i < _allRewardsInfo.Count; i++)
        {
            if (_allRewardsInfo[i].id == rewardId && _allRewardsInfo[i].count == count)
            {
                return i;
            }
        }
        return 0;
    }
    private int GetTimes(int times)
    {
        List<int> tempList = new List<int>();
        for (int i = 0; i < _timesInfo.Count; i++)
        {
            if (_timesInfo[i] == times)
            {
                tempList.Add(i);
            }
        }
        int num = Random.Range(0, tempList.Count - 1);
        return tempList[num];
    }
}
public class TurnTurntableInfo
{
    public string cost;
    public string get_reward;
    public int free_times;
    public List<TurntableRewardRecordInfo> rew_msg;

}

public class Act2090Info
{
    public int step;
    public int id;
    public int circle_pos;
    public string outer_reward;
    public int inner_mul;
    public int outer_weight;
}
public class TurntableRewardRecordInfo
{
    public int uid;
    public long time_stamp;
    public string msg;
}