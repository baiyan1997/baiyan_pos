using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2406: ActivityInfo
{
    public List<P_Act2046_Mission_info> actData;

    public List<P_Act2046_Mission_info> dailyMissionList = new List<P_Act2046_Mission_info>();
    
    public List<P_Act2046_Mission_info> celeMissionList = new List<P_Act2046_Mission_info>();
    
    private bool _IsAvaliable;

    public bool IsShowDailyRed;
    public bool IsShowCeleRed;
    
    public override void InitUnique()
    {
        _IsAvaliable = false;
        IsShowDailyRed = false;
        IsShowCeleRed = false;
        actData = JsonMapper.ToObject<List<P_Act2046_Mission_info>>(_data.avalue["mission_info"].ToString());
        
        dailyMissionList.Clear();
        celeMissionList.Clear();
        for (int i = 0; i < actData.Count; i++)
        {
            var data = actData[i];
            if (data.type == 1)
            {
                if (data.finished == 1 && data.get_reward == 0)
                {
                    IsShowDailyRed = true;
                }
                dailyMissionList.Add(data);
            }
            else if(data.type == 2)
            {
                if (data.finished == 1 && data.get_reward == 0)
                {
                    IsShowCeleRed = true;
                }
                celeMissionList.Add(data);
            }
        }
        
        _IsAvaliable = isNeedRedPoint();
        
        EventCenter.Instance.MidAutumnSingleActRefresh.Broadcast(_data.aid);
    }

    public bool isNeedRedPoint()
    {
        for (int i = 0; i < actData.Count; i++)
        {
            var data = actData[i];
            if (data.finished == 1 && data.get_reward == 0)
            {
                return true;
            }
        }

        return false;
    }
    
    public override bool IsAvaliable()
    {
        _IsAvaliable = isNeedRedPoint();
        return IsDuration() && _IsAvaliable;
    }

    public void RefreshData()
    {
        ActivityManager.Instance.RequestUpdateActivityById(_aid);//更新活动信息
        // EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
        // EventCenter.Instance.RemindActivity.Broadcast(_aid, _data.can_get_reward);
    }
    
    // public override bool IfRefreshOnPush(int opcode)
    // {
    //     return opcode == OpcodePush.Recharge;
    // }
}

public class P_Act2046_Mission_info
{
    public int tid;
    public int mission_finish_client;         // 任务条件（前端读取计数用）
    public int data;                             // 1表示每日任务，2表示庆典任务
    public int finished;                         // 是否已完成
    public int get_reward;                       // 是否已领奖;  
    public int type;                             // 1表示每日任务，2表示庆典任务; 
    public string click;                             // 跳转id
    public int mission_group;                             // 跳转id
    public string mission_finish;                             // 任务完成条件
    public int do_number;                             // 任务已完成进度
    public int day;                             // // 出现天数（每日任务填对应天数，庆典任务填1）
    public string mission_desc;                             //// 任务描述
    public string reward;                           // 奖励
    public int end_ts;                              // 结束时间
}

public class P_Act2406Reward
{
    public List<P_Item3> get_items;
    public P_ShipInfo[] get_ships;
    public P_ShipEquip[] get_equips;
}

public class Act_2406_NetMgr : Singleton<Act_2406_NetMgr>
{
    public void GetReward(int id, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2406Reward>("getAct2406Reward", Json.ToJsonString(id), data =>
        {
            string rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(rewardsStr);
            // ActivityManager.Instance.RequestUpdateActivityById(_aid);//更新活动信息
            var _actInfo = (ActInfo_2406)ActivityManager.Instance.GetActivityInfo(ActivityID.MidAutumnMission);
            if (_actInfo != null)
            {
                _actInfo.RefreshData();
            }

            if (callBack != null)
                callBack();
        });
    }
}