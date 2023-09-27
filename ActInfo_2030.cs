using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2030 : ActivityInfo
{
    public P_Act2030UserData _info;
    public Dictionary<int, int> status = new Dictionary<int, int>(10);
    public override void InitUnique()
    {
        if (_info == null)
        {
            _info = new P_Act2030UserData();
        }
        _info.get_reward = Convert.ToInt32(_data.avalue["get_reward"]);
        _info.can_get_reward = Convert.ToInt32(_data.avalue["can_get_reward"]);
        _info.rewardData = JsonMapper.ToObject<List<P_Act2030RewardData>>(_data.avalue["cfg_data"].ToString());
        RefreshInfo(_info.rewardData);
    }

    private void RefreshInfo(List<P_Act2030RewardData> rewards)
    {
        for (int i = 0; i < rewards.Count; i++)
        {
            _info.rewardData[i].rewards = GlobalUtils.ParseItem3(_info.rewardData[i].reward);

            if (i < _info.get_reward)
            {
                if (i == _info.get_reward - 1 && _info.can_get_reward == 1)
                {
                    status[rewards[i].id] = 1;
                }
                else if (i == _info.get_reward - 1 && _info.can_get_reward == 0)
                {
                    status[rewards[i].id] = 0;
                }
                else
                {
                    status[rewards[i].id] = 2;
                }      
            }
            else
            {
                status[rewards[i].id] = 0;
            }
        }
    }
    public void RefreshAct()
    {
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }

    public override bool IsAvaliable()
    {
        foreach (var kv in status)
        {
            if (kv.Value == 1)
                return true;
        }
        return false;
    }

    public void GetRewardById(int id, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2030Reward", Json.ToJsonString(id, _aid), data =>
        {
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(data.get_items);
            if (callback != null)//先ac 再广播
                callback();
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
}

public class P_Act2030UserData
{
    public int can_get_reward;
    public int get_reward;
    public List<P_Act2030RewardData> rewardData;
}
public class P_Act2030RewardData
{
    public string reward;
    public int id;
    public P_Item3[] rewards;
}

