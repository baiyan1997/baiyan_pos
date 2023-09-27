using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2046 : ActivityInfo
{
    public int lv { private set; get; }

    public int curExp { private set; get; }

    public int totalExp { private set; get; }

    public int step { private set; get; }

    public bool getAllBox
    {
        get
        {
            return lv > 4;
        }
    }

    private long missionEndTime;

    private List<Act2046Mission> missionList = new List<Act2046Mission>();

    private List<P_Item5> showItemList = new List<P_Item5>();

    public List<P_Item5> ShowItemList { get { return showItemList; } }

    public List<Act2046Mission> MissionList { get { return missionList; } }

    public long missionLeftTime
    {
        get
        {
            return missionEndTime - TimeManager.ServerTimestamp;
        }
    }

    public override void InitUnique()
    {
        missionList.Clear();

        base.InitUnique();

        missionEndTime = Convert.ToInt64(_data.avalue["mission_endts"]);

        UpdateBoxInfo();

        UpdateMissionInfo();
    }

    private void UpdateBoxInfo()
    {
        lv = Convert.ToInt32(_data.avalue["box_level"]);

        curExp = Convert.ToInt32(_data.avalue["box_show_exp"]);

        step = Convert.ToInt32(_data.avalue["box_step"]);

        totalExp = Cfg.Activity2046Box.GetNeedExp(lv, step);

        showItemList = Cfg.Activity2046Box.GetMissionAllReward(step);
    }

    private void UpdateMissionInfo()
    {
        List<P_ActMissionInfo> data = JsonMapper.ToObject<List<P_ActMissionInfo>>(_data.avalue["mission_info"].ToString());
        for (int i = 0; i < data.Count; i++)
        {
            P_ActMissionInfo info = data[i];
            Act2046Mission mission = new Act2046Mission(info);
            missionList.Add(mission);
        }
    }

    public Act2046Mission GetMissionById(int tid)
    {
        for (int i = 0; i < missionList.Count; ++i)
        {
            if (missionList[i].id == tid)
                return missionList[i];
        }
        return null;
    }

    public void GetBoxReward(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2046GetBoxInfo>("getAct2046BoxReward", Json.ToJsonString(0), data =>
         {
             //添加道具
             Uinfo.Instance.AddItem(data.get_items, true);
             MessageManager.ShowRewards(data.get_items);

             //更新进度
             lv = data.box_level;
             curExp = data.box_exp;
             totalExp = Cfg.Activity2046Box.GetNeedExp(lv, step);

             if (callback != null)
                 callback();

             EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
             EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
         });
    }

    public void GetTaskReward(int tid, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2046GetRewardInfo>("getAct2046Reward", Json.ToJsonString(tid), data =>
        {
            //任务道具
            Uinfo.Instance.AddItem(data.get_items, true);
            MessageManager.ShowRewards(data.get_items);

            //刷新宝箱进度
            curExp = data.box_exp;
            lv = data.box_level;
            totalExp = Cfg.Activity2046Box.GetNeedExp(lv, step);

            //刷新任务状态
            Act2046Mission mission = GetMissionById(tid);
            mission.isGet = true;

            if (callback != null)
                callback();

            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        });
    }

    //有宝箱可领，以及有任务奖励可领
    public override bool IsAvaliable()
    {
        if (curExp >= totalExp && totalExp != 0 && lv <= 4)
            return true;

        for (int i = 0; i < missionList.Count; i++)
        {
            Act2046Mission mission = missionList[i];
            if (mission.IsAvailable())
                return true;
        }
        return false;
    }
}

public class Act2046Mission
{
    public int id { private set; get; }
    public string desc
    {
        get
        {
            return Cfg.Activity2046.GetDesc(id);
        }
    }

    public int totalCount { private set; get; }

    public int getCount { private set; get; }

    public List<P_Item> itemList;

    public bool isGet { set; get; }

    public bool isFinished { set; get; }

    public Act2046Mission(P_ActMissionInfo data)
    {
        id = data.tid;
        isGet = data.get_reward;
        totalCount = data.total_count;
        getCount = data.do_number;
        isFinished = data.finished;
        itemList = Cfg.Activity2046.GetRewards(id);
    }

    public bool IsAvailable()
    {
        return isFinished && !isGet;
    }
}

public class P_ActMissionInfo
{
    public bool get_reward;

    public bool finished;

    public int do_number;

    public int tid;

    public int total_count;
}

public class P_Act2046GetBoxInfo
{
    public int box_level;

    public int box_exp;

    public string get_items;
}

public class P_Act2046GetRewardInfo
{
    public string get_items;

    public int box_exp;

    public int box_level;
}
