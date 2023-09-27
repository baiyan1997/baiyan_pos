using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2038 : ActivityInfo
{
    private List<Act2038Mission> _missionList;

    public override void InitUnique()
    {
        base.InitUnique();
        _missionList = JsonMapper.ToObject<List<Act2038Mission>>(_data.avalue["mission_info"].ToString());
        for (int i = 0; i < _missionList.Count; i++)
        {
            Act2038Mission data = _missionList[i];
            data.detail = Cfg.Activity2038.GetData(data.tid);
        }
    }

    public List<Act2038Mission> GetMissionList()
    {
        return _missionList;
    }

    public void GetReward(int tid, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2038Reward", Json.ToJsonString(tid), result =>
        {
            Uinfo.Instance.AddItem(_missionList[tid - 1].detail.reward, true);
            MessageManager.ShowRewards(_missionList[tid - 1].detail.reward);
            for (int i = 0; i < _missionList.Count; i++)
            {
                Act2038Mission data = _missionList[i];
                if (data.tid == tid) data.get_reward = 1;
            }
            if (callback != null)
                callback();
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        });
    }

    public override bool IsAvaliable()
    {
        for (int i = 0; i < _missionList.Count; i++)
        {
            if (_missionList[i].finished > 0 && _missionList[i].get_reward == 0)
                return true;
        }
        return false;
    }
}

public class Act2038Mission
{
    public int mission_group;
    public string data;
    public int finished;
    public int get_reward;
    public int do_number;
    public int type;
    public int tid;
    public cfg_act_2038 detail;
}