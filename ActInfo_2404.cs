using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;

public class ActInfo_2404 : ActivityInfo
{
    // public _ActInfo_2404_CfgData actCfgData;

    private bool _IsAvaliable;

    public List<_ActInfo_2404_List> list;
    public int consume;
    public int step;

    public override void InitUnique()
    {
        _IsAvaliable = false;
        var _actData = JsonMapper.ToObject<_ActInfo_2404_Data>(_data.avalue["data"].ToString());
        
        consume = Convert.ToInt32(_data.avalue["consume"]);
        step = Convert.ToInt32(_data.avalue["step"]);

        for (int i = 0; i < _actData.list.Count; i++)
        {
            var data = _actData.list[i];
            var cfg = data.GetCfg(step);
            if (data.get_reward == 1)
            {
                data.order = 2;
            }
            else if (cfg.number > consume)
            {
                data.order = 1;
            }
            else
            {
                _IsAvaliable = true;
                data.order = 0;
            }
        }

        list = _actData.list.OrderBy(task => task.order)
            .ThenBy(task => task.tid)
            .ToList();
    }

    public void GetReward(int id, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking<int>("getAct2404Reward", Json.ToJsonString(id), data =>
        {
            if (id == data)
            {
                var _data = list.Where(v => v.tid == data).ToList()[0];
                var cfg = _data.GetCfg(step);
                var rewardsStr = GlobalUtils.ToItemStr3(GlobalUtils.ParseItem3(cfg.reward));
                Uinfo.Instance.AddItem(rewardsStr, true);
                MessageManager.ShowRewards(rewardsStr);
                ActivityManager.Instance.RequestUpdateActivityById(_aid); //更新活动信息
                if (callBack != null)
                    callBack();
            }
        });
    }

    public override bool IsAvaliable()
    {
        return IsDuration() && _IsAvaliable;
    }
}

public class _ActInfo_2404_Data
{
    public List<_ActInfo_2404_List> list;
}

public class _ActInfo_2404_List
{
    public int uid;
    public int get_reward;
    public int aid;
    public int tid;
    public int order;

    public cfg_act_2404 GetCfg(int step)
    {
        var list = Cfg.Activity2404.GetDataListForStep(step);
        return list?[tid];
    }
};

public class _ActInfo_2404_Reward : IProtocolPostprocess
{
    public List<P_Item3> get_items;
    public P_ShipInfo[] get_ships;
    public P_ShipEquip[] get_equips;

    public void OnToObject()
    {
    }
}
