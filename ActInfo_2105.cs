using System;

public class ActInfo_2105 : ActivityInfo
{
    public P_2105Info Info;

    private static ActInfo_2105 _inst;
    public static ActInfo_2105 Instance
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2105)ActivityManager.Instance.GetActivityInfo(2105);
            }
            return _inst;
        }
    }

    private readonly int _actId = 2105;

    public override void InitUnique()
    {
        if (!_data.IsDuration())
            return;
        //重置单例指向
        _inst = (ActInfo_2105)ActivityManager.Instance.GetActivityInfo(_actId);
        if (Info == null)
            Info = new P_2105Info();
        Info.last_refresh_ts = Convert.ToInt32(_data.avalue["last_refresh_ts"]);
        Info.get_reward_count = Convert.ToInt32(_data.avalue["get_reward_count"]);
        Info.max_reward_count = Convert.ToInt32(_data.avalue["max_reward_count"]);
        Info.castle_id = Convert.ToInt32(_data.avalue["castle_id"]);
    }

    //进攻限时据点
    public void AttackLimitTimeLocation(P_WorldUnitInfo planetInfo, string fleetList, Action callback, Action<string> errorAc)
    {
        Rpc.SendWithTouchBlocking<P_None>("attackNebula", Json.ToJsonString(planetInfo.planet_id, fleetList, planetInfo.land_type), data =>
        {
            if (callback != null)
                callback();
        }, error => errorAc(error));
    }

    //放弃占领限时据点
    public void GiveUpLimitTimeLocation(int planet_id, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_None>("giveUpNebula", Json.ToJsonString(planet_id), data =>
        {
            if (callback != null)
                callback();
        });
    }

    //查看星云地块详情
    public void GetNebulaCastleInfo(int planetId, Action<P_NebulaInfo> callback)
    {
        Rpc.SendWithTouchBlocking<P_NebulaInfo>("getNebulaCastleInfo", Json.ToJsonString(planetId), data =>
        {
            if (callback != null)
                callback(data);
        });
    }
}

public class P_NebulaInfo
{
    public int nebula_uid;
    public string nebula_name;
    public int nebula_state;
    public int end_ts;
    public int had_occupy;//当前正在占领数
}

public class P_2105Info
{
    public int castle_id;//临时据点坐标
    public int last_refresh_ts;//上次刷新时间
    public int get_reward_count;//已获取奖励次数
    public int max_reward_count;//最多可领取次数
}