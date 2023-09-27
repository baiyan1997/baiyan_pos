using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2037 : ActivityInfo
{
    public List<P_Act2037Info> _missionInfo { private set; get; }//用于数据交互，任务刷新

    public int go_to_new_server;

    public override void InitUnique()
    {
        if (_data.avalue == null)
            throw new Exception("ActInfo_2037 info can not get availablely");
        _missionInfo = JsonMapper.ToObject<List<P_Act2037Info>>(_data.avalue["mission_info"].ToString());
        go_to_new_server = Convert.ToInt32(_data.avalue["go_to_new_server"]);
    }
    public override bool IsAvaliable()
    {
        return IsDuration() && IsCanGet();
    }
    private bool IsCanGet()
    {
        bool canGet = false;
        for (int i = 0; i < _missionInfo.Count; i++)
        {
            var mission = _missionInfo[i];
            if (mission.finished == 1 && mission.get_reward == 0)
                canGet = true;
        }
        return canGet;
    }

    public Dictionary<int, List<cfg_act_2037>> GetMissionsByType()
    {
        Dictionary<int, List<cfg_act_2037>> dict = new Dictionary<int, List<cfg_act_2037>>();
        List<cfg_act_2037> listInfo = new List<cfg_act_2037>();
        for (int i = 0; i < _missionInfo.Count; i++)
        {
            var info = _missionInfo[i];
            listInfo.Add(Cfg.Activity2037.GetData(info.tid));
        }
        for (int i = 0; i < listInfo.Count; i++)
        {
            var mission = listInfo[i];
            int page = mission.type;
            List<cfg_act_2037> temp = null;
            if (dict.TryGetValue(page, out temp))
            {
                // dict[page].Add(mission);
                temp.Add(mission);
            }
            else
            {
                dict.Add(page, new List<cfg_act_2037> { mission });
            }
        }
        return dict;
    }

    public P_Act2037Info FindMissionInfo(int id)
    {
        for (int i = 0; i < _missionInfo.Count; i++)
        {
            if (_missionInfo[i].tid == id)
                return _missionInfo[i];
        }
        return null;
    }

    // 领取奖励
    public void RequestGetReward(int tid)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2037Reward", Json.ToJsonString(tid), data =>
        {
            var mission = FindMissionInfo(tid);
            if (mission != null)
            {
                mission.get_reward = 1;
            }

            _data.can_get_reward = IsCanGet();

            if (Uinfo.Instance != null)
            {
                string rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
                Uinfo.Instance.AddItem(rewardsStr, true);
                MessageManager.ShowRewards(rewardsStr);
            }

            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, _data.can_get_reward);
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        });
    }
}

public class P_Act2037Info
{
    public int finished;//是否完成
    public int get_reward;//是否领奖
    public int do_number;//只用于显示任务进度
    public int tid;//id
}

