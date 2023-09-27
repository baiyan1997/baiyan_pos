using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;

public class ActInfo_2405 : ActivityInfo
{
    private bool _IsAvaliable;

    public List<_ActInfo_2405_List> list;
    public int tier; // 抽奖所在层数
    public int count; // 目前已抽奖次数

    public long ticketCount
    {
        get
        {
            return BagInfo.Instance.GetItemCount(ItemId.JiaJieLiQuan);
        }
    }

    public List<int> boxStatus;

    public override void InitUnique()
    {
        _IsAvaliable = false;
        var _actData = JsonMapper.ToObject<_ActInfo_2405_Data>(_data.avalue["data"].ToString());
        
        SetList(_actData.status);
        
        tier = Convert.ToInt32(_data.avalue["tier"]);
        count = Convert.ToInt32(_data.avalue["count"]);
        boxStatus = JsonMapper.ToObject<List<int>>(_data.avalue["boxStatus"].ToString());
        
        // 层数可能是 0
        if (tier == 0) tier = 1;
        
        // 盒子可能是空的
        if (boxStatus == null)
        {
            boxStatus = new List<int>()
            {
               0,0,0,0,0,0,0,0,0,
            };
        }
    }

    public void SetList(List<_ActInfo_2405_List> list)
    {
        var timesCfg = Cfg.Activity2405._dictForStepRewards;
        // 次数奖励可能是空的
        if (list == null)
        {
            list = new List<_ActInfo_2405_List>();
        }
        if (list.Count != timesCfg.Count)
        {
            foreach (var cfg in timesCfg)
            {
                var has = false;
                foreach (var s in list)
                {
                    if (cfg.tid == s.pos)
                    {
                        has = true;
                        break;
                    }
                }

                if (!has)
                {
                    list.Add(new _ActInfo_2405_List() { pos = cfg.tid, receive_status = 0 });
                }
            }
        }

        list.Sort((a, b) =>
        {
            if (a.isGeted() != b.isGeted())
            {
                if (a.isGeted())
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }

            if (a.IsFinish() != b.IsFinish())
            {
                if (a.IsFinish())
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            return a.pos - b.pos;
        });
        this.list = list;
    }

    public Tuple<_ActInfo_2405_List, cfg_act_2405_reward,bool> GetNearlyTimesInfo()
    {
        foreach (var info in list)
        {
            if (count < info.pos)
            {
                return new Tuple<_ActInfo_2405_List, cfg_act_2405_reward, bool>(info, Cfg.Activity2405.GetTimesReward(info.pos), false);
            }
        }
        return new Tuple<_ActInfo_2405_List, cfg_act_2405_reward, bool>(list[^1], Cfg.Activity2405.GetTimesReward(list[^1].pos), true);
    }

    public _ActInfo_2405_List GetCanGetTimesReward()
    {
        return list.FirstOrDefault(info => info.IsFinish() && !info.isGeted());
    }

    public int GetNextOpenBox()
    {
        for (int i = 1; i <= 9; i++)
        {
            var value = boxStatus[i - 1];
            if (value == 0)
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsBoxOpened(int index)
    {
        return boxStatus[index - 1] == 1;
    }

    public bool isNeedRedPoint()
    {
        var info = GetCanGetTimesReward();
        if (info != null)
        {
            return true;
        }

        return ticketCount != 0;
    }

    public override bool IsAvaliable()
    {
        _IsAvaliable = isNeedRedPoint();
        return IsDuration() && _IsAvaliable;
    }

    public void GetReward(int id, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking<int>("getAct2404Reward", Json.ToJsonString(id), data =>
        {
            // if (id == data)
            // {
            //     var _data = list.Where(v => v.tid == data).ToList()[0];
            //     var cfg = _data.GetCfg(step);
            //     var rewardsStr = GlobalUtils.ToItemStr3(GlobalUtils.ParseItem3(cfg.reward));
            //     Uinfo.Instance.AddItem(rewardsStr, true);
            //     MessageManager.ShowRewards(rewardsStr);
            //     ActivityManager.Instance.RequestUpdateActivityById(_aid); //更新活动信息
            //     if (callBack != null)
            //         callBack();
            // }
        });
    }
}

public class _ActInfo_2405_Data
{
    public List<_ActInfo_2405_List> status;
}

public class _ActInfo_2405_List
{
    
    public int pos;
    public int receive_status;  // 0 未领取 1 可领取 2 已领取

    public bool IsFinish()
    {
        return receive_status is 1 or 2;
    }

    public bool isGeted()
    {
        return receive_status is 2;
    }
};

public class _ActInfo_2405_Box_Reward : IProtocolPostprocess
{
    public List<string> reward;

    public List<P_Item3> rewardList => reward.Select(v =>
    {
        var temp = GlobalUtils.ParseItem5(v)[0];
        return new P_Item3() { itemid = temp.id, count = temp.count };
    }).ToList();
    
    public int tier;
    public int drawCount;
    public string boxStatus;
    public List<int> boxStatusList => JsonMapper.ToObject<List<int>>(boxStatus);

    public string timesReward;
    public List<_ActInfo_2405_List> timesRewardList => timesReward == null ? null : JsonMapper.ToObject<List<_ActInfo_2405_List>>(timesReward);
    
    

    public void OnToObject()
    {
    }
}

public class _ActInfo_2405_Times_Reward : IProtocolPostprocess
{
    public string reward;

    public List<P_Item3> rewardList => GlobalUtils.ParseItem3(reward).ToList();

    public int pos;
    public int status;

    public void OnToObject()
    {
    }
}
