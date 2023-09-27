using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
public class ActInfo_2401 : ActivityInfo
{
    // public _ActInfo_2401_CfgData actCfgData;
    public List<_ActInfo_2401_Data> actData;

    public string totalCharge;

    private bool _IsAvaliable;

    public override void InitUnique()
    {
        _IsAvaliable = false;
        totalCharge = _data.avalue["total_charge"].ToString();
        var _actData = JsonMapper.ToObject<List<_ActInfo_2401_Data>>(_data.avalue["data"].ToString());
        var _actCfgData = JsonMapper.ToObject<_ActInfo_2401_CfgData>(_data.avalue["cfg_data"].ToString());
        for (int i = 0; i < _actData.Count; i++)
        {
            var _data = _actData[i];
            var _dataCfg = _actCfgData.cfg_data[i];
            if (_dataCfg == null) continue;
            _data.dataCfg = _dataCfg;
            if (_data.get_reward == 1)
            {
                _data.order = 2;
            }
            else if (_data.finished == 0)
            {
                _data.order = 1;
            }
            else
            {
                _IsAvaliable = true;
                _data.order = 0;               
            }
        }

        actData = _actData.OrderBy(task => task.order)
                               .ThenBy(task => task.tid)
                               .ToList();
    }

    public void GetReward(int id, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking<_ActInfo_2401_Reward>("getAct2401Reward", Json.ToJsonString(id), data =>
        {
            string rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(rewardsStr);
            ActivityManager.Instance.RequestUpdateActivityById(_aid);//更新活动信息
            if (callBack != null)
                callBack();
        });
    }

    public override bool IsAvaliable()
    {
        return IsDuration() && _IsAvaliable;
    }
}

public class _ActInfo_2401_CfgData
{
    public List<cfg_act_2401> cfg_data;
}

public class _ActInfo_2401_Data
{
    public int start_ts;
    public int uid;
    public int finish_ts;
    public string data;
    public int end_ts;
    public int finished;
    public int get_reward;
    public int do_number;
    public int state;
    public int aid;
    public int tid;
    public cfg_act_2401 dataCfg;
    public int order;
}

public class _ActInfo_2401_Reward : IProtocolPostprocess
{
    public List<P_Item3> get_items;
    public P_ShipInfo[] get_ships;
    public P_ShipEquip[] get_equips;
    public void OnToObject()
    {
    }
}

