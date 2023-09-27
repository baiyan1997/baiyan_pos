using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2003 : ActivityInfo
{
    public int _today { private set; get; }
    private int _program;//任务进度
    private List<int> _record;//领取记录
    public static int ShipId
    {
        get { return 40104001; }  //奖励战舰id
    }

    public readonly int ProgressMissionId = 4800;

    public List<P_Act2003Mission> _missionInfo { private set; get; }


    public override void InitUnique()
    {
        _today = Math.Min(Convert.ToInt32(_data.avalue["today"]), 7);
        _program = Convert.ToInt32(_data.avalue["value"]);
        _missionInfo = JsonMapper.ToObject<List<P_Act2003Mission>>(_data.avalue["mission_info"].ToString());

        string tempString =  _data.avalue["record"].ToString();
        DealRecord(tempString);
    }

    private void DealRecord(string record)
    {
        if (_record == null)
        {
            _record = new List<int>();
        }

        if (!String.IsNullOrEmpty(record))
        {
            string[] tempRecord = record.Split(',');
            _record.Clear();
            for (int i=0; i<tempRecord.Length;i++)
            {
                string one=tempRecord[i];
                _record.Add(Convert.ToInt32(one));
            }
        }
    }
    // 该活动是否存在可领状态(红点提示状态) : 活动状态开启 ，在时间期限内，达到领取条件，并且还未领过
    public override bool IsAvaliable()
    {
        return IsDuration() && IsCanGet();
    }
    public int GetProgram()
    {
        return _program;
    }

    public List<int> GetRewardRecord()
    {
        return _record;
    }
    // 是否存在可领
    private bool IsCanGet()
    {
        bool canGet = false;
        for (int i = 0; i < _missionInfo.Count; i++)
        {
            var mission = _missionInfo[i];
            if (mission.finished == 1 && mission.get_reward == 0)
                canGet = true;
        }

        bool canGetBox = false;
        for (int i = 0; i < 5; i++)
        {
            int index = i + 1;
            var boxReward = Cfg.Activity2003.GetReward(index);
            if (_program >= boxReward.need_value && !_record.Contains(index))
            {
                canGetBox = true;
            }
        }
        //判断进度宝箱领取情况
        return canGet || canGetBox;
    }

    //统计今日任务完成情况
    public int GetProgressByDay(int day)
    {
        int count = 0;
        for (int i = 0; i < _missionInfo.Count; i++)
        {
            var mission = _missionInfo[i];
            cfg_act_2003 data = Cfg.Activity2003.GetData(mission.tid);
            if (mission.finished == 1 && data.day == day && data.mission_group != ProgressMissionId)
                count++;
        }
        return count;
    }

    public P_Act2003Mission FindMissionInfo(int id)
    {
        for (int i = 0; i < _missionInfo.Count; i++)
        {
            if (_missionInfo[i].tid == id)
                return _missionInfo[i];
        }
        return null;
    }


    public P_Act2003Mission FindProgressMissionInfo(int day)
    {
        cfg_act_2003 config = Cfg.Activity2003.GetProgressMissionByDay(day);

        return FindMissionInfo(config.tid);
    }

    public void GetBoxReward(int id, Action<int> callback = null)
    {
        //Rpc.SendWithTouchBlocking<P_Act2003BoxReward>("getAct2003BoxReward", Json.ToJsonString(id), data =>
        //{
        //    Uinfo.Instance.AddItemAndShow(data.get_item);
        //    DealRecord(data.record);
        //    _data.can_get_reward = IsCanGet();
        //    EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        //    EventCenter.Instance.RemindActivity.Broadcast(_data.aid, _data.can_get_reward);
        //});
        Rpc.SendWithTouchBlocking<P_Act2003BoxReward>("getAct2003BoxReward", Json.ToJsonString(id), On_getAct2003BoxReward_SC);
    }

    private void On_getAct2003BoxReward_SC(P_Act2003BoxReward data)
    {
        Uinfo.Instance.AddItemAndShow(data.get_item);
        DealRecord(data.record);
        _data.can_get_reward = IsCanGet();
        EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        EventCenter.Instance.RemindActivity.Broadcast(_data.aid, _data.can_get_reward);
    }

    // 领取奖励
    public void RequestGetReward(int tid)
    {
        PlatformWrap.Warn("发送RequestGetReward tid = " + tid, false);
        Rpc.SendWithTouchBlocking<P_Act2003TaskReward>("getAct2003Reward", Json.ToJsonString(tid), data =>
        {
            var mission = FindMissionInfo(tid);
            if (mission != null)
            {
                mission.get_reward = 1;
            }
            else
            {
                PlatformWrap.Warn("没有找到Act2003的mission", false);
            }

            _data.can_get_reward = IsCanGet();

            if (Uinfo.Instance != null)
            {
                string rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
                Uinfo.Instance.AddItem(rewardsStr, true);
                MessageManager.ShowRewards(rewardsStr);
            }
            var task = Cfg.Activity2003.GetData(tid);
            _program += task.reward_value;
            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, _data.can_get_reward);
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        });      
    }  
}

//根据传过来的json处理数据
public class P_Act2003Mission
{
    public int finished;
    public int get_reward;
    public int do_number;
    public int tid;
}

public class P_Act2003BoxReward
{
    public string get_item;
    public string record;
}

public class P_Act2003TaskReward
{
    public List<P_Item3> get_items;
    public int value;
    public int notify;
}