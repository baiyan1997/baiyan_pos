using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2066 : ActivityInfo
{
    public int Step;//活动阶段
    public int TransportCount;//剩余运输次数
    public int PlunderCount;//剩余掠夺次数
    public List<P_Act2066Info> PersonTransportInfo;//个人运输数据
    public P_Act2066Info PersonPlunderInfo;//个人掠夺数据

    public P_Act2066DetailInfo ActInfo;

    /// <summary>
    /// 0-未开启 1-预告 2-正式 3-无法运输 4-结束
    /// </summary>
    public int State;

    public List<P_Act2066Info> HistoryInfo;//历史战报

    private static ActInfo_2066 _inst;

    public static ActInfo_2066 Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2066)ActivityManager.Instance.GetActivityInfo(ActivityID.DefenceShip);
            }
            return _inst;
        }
    }
    
    public override bool OnInited()
    {
        InitPush();
        return true;
    }
    public override void OnRemove()
    {
        UnInitPush();
    }

    public override void InitUnique()
    {
        base.InitUnique();
        //重置单例指向
        _inst = (ActInfo_2066)ActivityManager.Instance.GetActivityInfo(ActivityID.DefenceShip);
        //初始化运输船信息
        Refresh(null);
    }

    
    public override bool IsAvaliable()
    {
        if(IsPlundering())
        {
            //正在被人抢夺时提示
            return true;
        }
        else
            return base.IsAvaliable();
    }

    /// <summary>
    /// 初始化活动信息
    /// </summary>
    public void Refresh(Action callback)
    {
        //Rpc.SendWithTouchBlocking<P_Act2066DetailInfo>("getTransportBattleActivityDetail", null, data =>
        //{
        //    ActInfo = data;
        //    Step = ActInfo.step;
        //    TransportCount = ActInfo.transportCount;
        //    PlunderCount = ActInfo.plunderCount;
        //    State = ActInfo.activity_period;
        //    PersonTransportInfo = data.personInfo;
        //    PersonPlunderInfo = data.personPlunderInfo;
        //    EventCenter.Instance.UpdateActById.Broadcast(_aid);
        //});
        Rpc.SendWithTouchBlocking<P_Act2066DetailInfo>("getTransportBattleActivityDetail", null, On_getTransportBattleActivityDetail_SC);
    }
    private void On_getTransportBattleActivityDetail_SC(P_Act2066DetailInfo data)
    {
        ActInfo = data;
        Step = ActInfo.step;
        TransportCount = ActInfo.transportCount;
        PlunderCount = ActInfo.plunderCount;
        State = ActInfo.activity_period;
        PersonTransportInfo = data.personInfo;
        PersonPlunderInfo = data.personPlunderInfo;
        EventCenter.Instance.UpdateActById.Broadcast(_aid);
    }


    public void InitPush()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.ACT2066_STATE_CHANGE, _EventACT2066_STATE_CHANGE);

        EventCenter.Instance.AddPushListener(OpcodePush.DEFEND_TRANSPORT_CHANGE, _EventDEFEND_TRANSPORT_CHANGE);

        EventCenter.Instance.AddPushListener(OpcodePush.TPLUNDER_RANSPORT_CHANGE, _EventTPLUNDER_RANSPORT_CHANGE);

        EventCenter.Instance.AddPushListener(OpcodePush.ACT2066_TRANSPORT_STATE_CHANGE, _EventACT2066_TRANSPORT_STATE_CHANGE);
    }
    public void UnInitPush()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.ACT2066_STATE_CHANGE, _EventACT2066_STATE_CHANGE);

        EventCenter.Instance.RemovePushListener(OpcodePush.DEFEND_TRANSPORT_CHANGE, _EventDEFEND_TRANSPORT_CHANGE);

        EventCenter.Instance.RemovePushListener(OpcodePush.TPLUNDER_RANSPORT_CHANGE, _EventTPLUNDER_RANSPORT_CHANGE);

        EventCenter.Instance.RemovePushListener(OpcodePush.ACT2066_TRANSPORT_STATE_CHANGE, _EventACT2066_TRANSPORT_STATE_CHANGE);
    }

    private void _EventACT2066_STATE_CHANGE(int opcode, string data)
    {
        Refresh(null);
        //活动阶段变化触发banner刷新
        ActivityManager.Instance.RequestUpdateActivityById(ActivityID.DefenceShip);
    }
    private void _EventDEFEND_TRANSPORT_CHANGE(int opcode, string data)
    {
        PersonTransportInfo = JsonMapper.ToObject<List<P_Act2066Info>>(data);
        EventCenter.Instance.UpdateActById.Broadcast(_aid);
    }
    private void _EventTPLUNDER_RANSPORT_CHANGE(int opcode, string data)
    {
        PersonPlunderInfo = null;
        EventCenter.Instance.UpdateActById.Broadcast(_aid);
    }
    private void _EventACT2066_TRANSPORT_STATE_CHANGE(int opcode, string data)
    {
        ActInfo.transporting_state = data;
        EventCenter.Instance.UpdateActById.Broadcast(_aid);
    }

    //个人运输船和运输信息
    public void GetPersonalTransportInfo(Action callback)
    {
        Rpc.SendWithTouchBlocking<P_PersonalTransportInfo>("getPersonalTransportInfo", null, data =>
        {
            TransportCount = data.transportCount;
            PlunderCount = data.plunderCount;
            PersonTransportInfo = data.personInfo;
            EventCenter.Instance.UpdateActById.Broadcast(_aid);
            if (callback != null)
                callback();
        });
    }

    //个人运输和战斗的历史记录
    public void GetPersonalHistoryInfo(Action callback)
    {
        Rpc.SendWithTouchBlocking<List<P_Act2066Info>>("getPersonalTransportHistoryInfo", null, data =>
        {
            HistoryInfo = data;
            if (callback != null)
                callback();
        });

    }

    //获取目标势力随机3条运输信息
    public void GetPersonalPlunderInfo(int state, Action<P_ActPlunder2066> callback = null)
    {
        Rpc.SendWithTouchBlocking<P_ActPlunder2066>("getPersonalPlunderInfo", Json.ToJsonString(state), data =>
        {
            if (callback != null)
                callback(data);
        });
    }

    //获得邮件
    public void GetBattleReport(int gid, int mid, int aimpos, Action<P_MailDetail> callback)
    {
        Rpc.SendWithTouchBlocking<P_MailDetail>("getTransportReport", Json.ToJsonString(gid, mid, aimpos), data =>
        {
            P_Mail.ChangeMailDetail(data);
            if (callback != null)
                callback(data);
        });
    }

    //第一参数是否是国家 第二个 ship_id 第三个运输船的拥有者
    public void GetTransportWarDetail(int is_state, int ship_id, int def_uid, Action<P_ActDetail2066> callback)
    {
        Rpc.SendWithTouchBlocking<P_ActDetail2066>("getTransportWarDetail", Json.ToJsonString(is_state, ship_id, def_uid), data =>
        {
            if (callback != null)
                callback(data);
        });
    }

    public P_Act2066Info GetPersonalTransportingInfo()
    {
        if (PersonTransportInfo == null || PersonTransportInfo.Count <= 0)
            return null;
        for (int i = 0; i < PersonTransportInfo.Count; i++)
        {
            var info = PersonTransportInfo[i];
            if (info.status == 1 || info.status == 2)
                return info;
        }
        return null;
    }

    //是否正在被敌人掠夺
    public bool IsPlundering()
    {
        if (PersonTransportInfo == null || PersonTransportInfo.Count <= 0)
            return false;

        for (int i = 0; i < PersonTransportInfo.Count; i++)
        {
            var info = PersonTransportInfo[i];
            if (info.atk_uid > 0)
                return true;
        }

        return false;
    }
}

public class P_Act2066DetailInfo
{
    public int plunderCount;
    public int transportCount;
    public int activity_period;
    public int start_ts;
    public int end_ts;
    public int step;
    public string transporting_state;
    public List<P_Act2066Info> personInfo;//个人运输数据
    public P_Act2066Info personPlunderInfo;//个人掠夺信息
}


public class P_PersonalTransportInfo
{
    public List<P_Act2066Info> personInfo;//个人运输数据
    //public List<P_Act2066Info> personPlunderInfo;//个人掠夺信息
    public int plunderCount;  //剩余抢夺次数
    public int transportCount; //剩余运输次数
    //public P_NationInfo nationInfo;//国家保卫数据
}


//国家抢夺 个人抢夺
public class P_ActPlunder2066
{
    public int plunder_count;//条数
    public int next_refresh_ts;//刷新时间
    public List<P_Act2066Info> plunderingInfo;//抢夺数据
}

//Item数据
public class P_Act2066Info
{
    public int def_uid;
    public int atk_state;//进攻方国家
    public string def_name;//运输发起者名字
    public int is_state;//0 个人运输船 1国家运输船
    public int def_state;//防守国家
    public string def_state_name;//防御方国家名字
    public int end_ts;//已经运输在路上的才会有时间
    public int atk_uid;//进攻方uid
    public int ship_id;//战舰id
    public int people_num;//参与人数
    public int win; //防守方1-赢 0-输
    public int gtid; //战报id 0-没有战斗
    public int mid; //唯一id


    //客户端使用，对应读表
    public int shipid
    {
        get
        {
            return ship_id % 1000;
        }
    }
    public int status;//0 还在家中 1 运输中 2 被掠夺中 3运输完
}
//战斗数据
public class P_ActBattleInfo2066
{
    public DSType dsType;
    public int is_state;
    public int atk_state;
    public int atk_uid;
    public int def_uid;
    public int aimPos;
    public int status;
}
//战斗详情
public class P_ActDetail2066
{
    public string def_name;
    public int def_soldier;
    public string atk_name;
    public int atk_soldier;
    public int end_ts;
}
//发起抢夺
public class P_Plunder2066
{
    public int personal_plunder_count;//个人发起次数
    public P_Act2066Info plunderingInfo;
    //public int nation_plunder_count;//国家发起次数
}

public enum DSType
{
    Defence = 0,//保卫
    Seize = 1,//抢夺
}