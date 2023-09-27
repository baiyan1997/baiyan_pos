using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2091 : ActivityInfo
{
    public int InviteType { private set; get; }
    public bool BandPhone { private set; get; }
    public bool IsBand { private set; get; }

    public int InviteProgress { private set; get; }
    public List<P_Act2091Mission> MissionList;

    private string _url = "https://www.huchihuchi.com/zhanjian";
    public string MyCode { private set; get; }

    public override void InitUnique()
    {
        base.InitUnique();

        if (!_data.IsDuration())
            return;

        InviteType = Convert.ToInt32(_data.avalue["type"].ToString());
        BandPhone = Convert.ToInt32(_data.avalue["bind"].ToString()) == 1;
        IsBand = Convert.ToInt32(_data.avalue["be_connect"].ToString()) == 1;
        MyCode = _data.avalue["act_code"].ToString();
        MissionList = JsonMapper.ToObject<List<P_Act2091Mission>>(_data.avalue["mission_info"].ToString());
    }

    public override bool IsAvaliable()
    {
        for (int i = 0; i < MissionList.Count; i++)
        {
            P_Act2091Mission mission = MissionList[i];
            if (mission.finished == 1 && mission.get_reward == 0)
                return true;
        }
        return false;
    }

    //分享检测
    public void SendCodeCheck()
    {
        //Rpc.SendWithTouchBlocking("reqGenerateInvitationCode", Json.ToJsonString(User.Uid, User.Server.index), data =>
        //{
        //    if ((int)data[0] != 1)
        //    {
        //        Alert.Ok(Lang.TranslateJsonString((string)data[1]));
        //        return;
        //    }
        //    MyCode = (string)data[1];

        //    PlatformSdk.GetInstance().WriteClipBoardString(MyCode);
        //    MessageManager.Show(Lang.Get("邀请码已复制,快去粘贴分享吧"));
        //});
        Rpc.SendWithTouchBlocking("reqGenerateInvitationCode", Json.ToJsonString(User.Uid, User.Server.index), On_reqGenerateInvitationCode_SC);
    }
    private void On_reqGenerateInvitationCode_SC(JsonData data)
    {
        if ((int)data[0] != 1)
        {
            Alert.Ok(Lang.TranslateJsonString((string)data[1]));
            return;
        }
        MyCode = (string)data[1];

        PlatformSdk.GetInstance().WriteClipBoardString(MyCode);
        MessageManager.Show(Lang.Get("邀请码已复制,快去粘贴分享吧"));
    }

    //确定关联 
    public void SendBand(string code)
    {
        //Rpc.SendWithTouchBlocking("connectInvitationCode", Json.ToJsonString(code), data =>
        //   {
        //       if ((int)data[0] != 1)
        //       {
        //           Alert.Ok(Lang.TranslateJsonString((string)data[1]));
        //           return;
        //       }

        //       PlatformSdk.GetInstance().WriteClipBoardString("");

        //       ActivityManager.Instance.RequestUpdateActivityById(_aid);

        //   });

        Rpc.SendWithTouchBlocking("connectInvitationCode", Json.ToJsonString(code), On_connectInvitationCode_SC);
    }
    private void On_connectInvitationCode_SC(JsonData data)
    {
        if ((int)data[0] != 1)
        {
            Alert.Ok(Lang.TranslateJsonString((string)data[1]));
            return;
        }

        PlatformSdk.GetInstance().WriteClipBoardString("");

        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }

    //领任务奖励
    public void GetMissionReward(int tid, Action<P_Act2091Mission> callback)
    {
        Rpc.SendWithTouchBlocking<P_Act2091GetRewardData>("get2091TaskReward", Json.ToJsonString(tid), data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_rewards);

            P_Act2091Mission mission = null;
            for (int i = 0; i < MissionList.Count; i++)
            {
                P_Act2091Mission m = MissionList[i];
                if (m.tid == tid)
                {
                    m.get_reward = 1;
                    mission = m;
                }
            }

            if (callback != null)
                callback(mission);

            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
}


public class P_Act2091Mission
{
    public int tid;
    public int do_number;
    public int get_reward;
    public int finished;
    public P_Item[] ItemList
    {
        get { return Cfg.Activity2091.GetTaskRewards(tid); }
    }
}

//领取任务奖励
public class P_Act2091GetRewardData
{
    public string get_rewards;
}

public class P_Act2091BoxData
{
    public int id;
    public int get_reward;
    public int count
    {
        get
        {
            return Cfg.Activity2091.GetBoxData(id).count;
        }
    }
}
