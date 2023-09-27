using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ActInfo_2106 : ActivityInfo
{
    public List<P_Act2106Mission> MissionList { get; private set; }

    public ActMissionStatus Status { get; private set; }


    public override void InitUnique()
    {
        MissionList = JsonMapper.ToObject<List<P_Act2106Mission>>(_data.avalue["mission_info"].ToString());
        UpdateMissionStatus();
        EventCenter.Instance.Act2106MissionStatusUpdate.Clear();
        EventCenter.Instance.Act2106MissionStatusUpdate.AddListener(() =>
        {
            //任务状态改变时刷新活动
            ActivityManager.Instance.RequestUpdateActivityById(_aid);
        });
    }

    public void GetMissionReward(int tid, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_GetMissionReward>("getAct2106Reward", Json.ToJsonString(tid), data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_reward);
            MissionList = data.mission_info;

            EventCenter.Instance.Act2106MissionUpdate.Broadcast();
            UpdateMissionStatus();

            callback?.Invoke();
        });
    }

    //刷新任务状态
    private void UpdateMissionStatus()
    {
        var oldStatus = Status;
        Status = MissionList.Count > 0 ? ActMissionStatus.Todo : ActMissionStatus.Finished;
        for (int i = 0; i < MissionList.Count; i++)
        {
            var info = MissionList[i];
            if(info.finished > 0 && info.get_reward == 0)
            {
                Status = ActMissionStatus.ToGetReward;
                break;
            }
        }
        if (oldStatus != Status)
            EventCenter.Instance.Act2106MissionStatusUpdate.Broadcast();
    }

    public override bool IsAvaliable()
    {
        return Status == ActMissionStatus.ToGetReward;
    }
}

public class P_GetMissionReward
{
    public string get_reward;
    public List<P_Act2106Mission> mission_info;
}

public class P_Act2106Mission
{
    public int tid;
    public int finished;//任务是否已完成
    public int get_reward;//是否已领奖
    public int do_number;//任务进度
}

public enum ActMissionStatus
{
    Todo,
    ToGetReward,
    Finished
}
