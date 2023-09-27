using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2067 : ActivityInfo
{
    public List<P_Slxf> Mission;

    public override void InitUnique()
    {
        Mission = JsonMapper.ToObject<List<P_Slxf>>(_data.avalue["data"].ToString());
    }

    public void GetSlxfReward(int tid, Action ac)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getActSlxfReward", Json.ToJsonString(tid), data =>
        {
            var info = GetMissionByTid(tid);
            if (info != null)
                info.get_reward = 1;
            string rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(rewardsStr);

            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
            if (ac != null)
                ac();
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }

    public P_Slxf GetFirstMission()
    {
        if (Mission.Count > 0)
            return Mission[0];
        return null;
    }

    public P_Slxf GetMissionByTid(int tid)
    {
        for (int i = 0; i < Mission.Count; i++)
        {
            if (Mission[i].tid == tid)
                return Mission[i];
        }
        return null;
    }

    public List<P_Slxf> GetAllMission()
    {
        return Mission;
    }

    public override bool IsAvaliable()
    {
        if (!IsDuration())
            return false;
        for (int i = 0; i < Mission.Count; i++)
        {
            var mis = Mission[i];
            if (mis.finished == 1 && mis.get_reward == 0)
            {
                return true;
            }
        }
        return false;
    }
}

public class P_Slxf
{
    public long start_ts;
    public long end_ts;
    public int finished;
    public int get_reward;
    public int do_number;
    public int tid;
}
public class SlxfMission
{
    public List<P_Slxf> slxf_mission;
}