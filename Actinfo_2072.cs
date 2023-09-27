using LitJson;
using System;
using System.Collections.Generic;

public class ActInfo_2072 : ActivityInfo
{
    private static ActInfo_2072 _inst;

    //世界boss
    private const int BossMissionId1 = 20120;
    private const int BossMissionId2 = 20220;
    private const int ActId = 2072;

    private int _worldBossId;
    private P_WorldBossInfo _bossInfo;
    public P_WorldBossInfo BossInfo
    {
        get
        {
            return _bossInfo;
        }
    }
    //任务累计经验
    public int MissionExp
    {
        get
        {
            if (_bossInfo == null)
                return 0;
            return _bossInfo.challenge_exp;
        }
    }

    //已增加的挑战次数
    public int AddExtraChallengeCount
    {
        get
        {
            if (_bossInfo == null)
                return 0;
            return _bossInfo.challenge_add_count;
        }
    }

    public static ActInfo_2072 Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2072)ActivityManager.Instance.GetActivityInfo(2072);
            }
            return _inst;
        }
    }

    public const int MAX_Declare_GRAND_SERVANT_WAR_COUNT = 3;

    public override bool OnInited()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.ACT_2072_PLANET_ROTTEN, _EventACT_2072_PLANET_ROTTEN);

        EventCenter.Instance.AddPushListener(OpcodePush.ACT_2072_ROTTEN_END, _EventACT_2072_ROTTEN_END);

        EventCenter.Instance.AddPushListener(OpcodePush.ACT_2072_ROTTEN_SUCCESS, _EventACT_2072_ROTTEN_SUCCESS);
        //在加载完成后发送星球腐蚀广播
        ActivityManager.Instance.DoOnLoadingFinish += _EventDoOnLoadingFinish;

        return true;
    }
    public override void OnRemove()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.ACT_2072_PLANET_ROTTEN, _EventACT_2072_PLANET_ROTTEN);

        EventCenter.Instance.RemovePushListener(OpcodePush.ACT_2072_ROTTEN_END, _EventACT_2072_ROTTEN_END);

        EventCenter.Instance.RemovePushListener(OpcodePush.ACT_2072_ROTTEN_SUCCESS, _EventACT_2072_ROTTEN_SUCCESS);
        //在加载完成后发送星球腐蚀广播
        ActivityManager.Instance.DoOnLoadingFinish -= _EventDoOnLoadingFinish;
    }
    private void _EventACT_2072_PLANET_ROTTEN(int opcode, string data)
    {
        //星球开始被腐蚀
        ActivityManager.Instance.RequestUpdateActivityById(ActId);
    }
    private void _EventACT_2072_ROTTEN_END(int opcode, string data)
    {
        // 星球腐蚀结束
        ActivityManager.Instance.RequestUpdateActivityById(ActId);
    }
    private void _EventACT_2072_ROTTEN_SUCCESS(int opcode, string data)
    {
        //星球完全被腐蚀
        ActivityManager.Instance.RequestUpdateActivityById(ActId);
    }
    private void _EventDoOnLoadingFinish()
    {
        EventCenter.Instance.Act2072PlanetRotten.Broadcast();
    }

    public override void InitUnique()
    {
        base.InitUnique();
        //重置单例指向
        _inst = (ActInfo_2072)ActivityManager.Instance.GetActivityInfo(2072);
        //刷新世界boss信息
        _bossInfo = JsonMapper.ToObject<P_WorldBossInfo>(_data.avalue["data"].ToString());
        //推送星球腐蚀事件
        EventCenter.Instance.Act2072PlanetRotten.Broadcast();
        //获得世界boss的任务id
        var worldBoss = MissionInfo.Instance.FindFirstWorldBoss();
        if (worldBoss != null)
            _worldBossId = worldBoss.tid;
    }

    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }

    public void AttackWorldBoss(Action<P_Battle> callback)
    {
        Rpc.Send<P_Battle>("atkWorldBoss", null, data =>
        {
            JDDebug.Dump(data);
            //打完boss进攻次数+1
            _bossInfo.atk_boss_count++;
            //打完boss后刷新一次活动时间
            ActivityManager.Instance.RequestUpdateActivityById(ActId);

            if (callback != null)
                callback(data);
        });
    }

    //领取活动任务奖励
    public void GetMissionReward(int tid, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_Get2072MissionReward>("getRewardOfAct2072", Json.ToJsonString(tid), data =>
        {
            //添加道具
            Uinfo.Instance.AddItemAndShow(data.reward);
            //刷新任务列表
            _bossInfo.mission_list = data.mission_list;
            if(Cfg.Activity2072.IsDailyMission(tid))
            {
                //每日任务 刷新经验和挑战次数
                _bossInfo.challenge_exp = data.challenge_exp;
                _bossInfo.challenge_add_count = data.challenge_add_count;
            }
            //广播任务刷新
            EventCenter.Instance.Act2072MissionUpdate.Broadcast();
            //回调
            if (callback != null)
                callback();
        });
    }

    //攻打世界Boss高级腐蚀卫兵(创建集结点)
    public void CreateAttackWorldBossGrandServent(int planetId, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_None>("createAtkMinions", Json.ToJsonString(planetId), data => {
            
            if (callback != null)
                callback();
        });
    }

    //获取高级腐蚀卫兵信息
    public void GetGrandServantInfo(P_WorldUnitInfo planetInfo, Action<P_2072GrandServantInfo> callback)
    {
        Rpc.Send<P_2072GrandServantInfo>("getHighMinionsInfo", Json.ToJsonString(planetInfo.land_type, planetInfo.land_lv), data =>
        {
            if (callback != null)
                callback(data);
        });
    }

    public override bool IsAvaliable()
    {
        return _bossInfo != null && (_bossInfo.atk_boss_count < _bossInfo.challenge_add_count + 1 || _bossInfo.use_buy_times < _bossInfo.buy_times);
    }
}

public class P_WorldBossInfo
{
    public int boss;
    public int status;
    public int hp;
    public int hp_max;
    public int atk_boss_count;
    public int challenge_add_count;
    public List<P_WorldBossMission> mission_list;//任务列表
    public int challenge_exp;//任务累计经验值
    public int max_challenge_exp;//任务最大经验值
    public int corrosion_status;//是否有被腐蚀的星球 1-有 0-没有
    public int buy_times;//购买挑战的次数
    public int use_buy_times;//使用购买挑战的次数
    public string GetBossName()
    {
        if (boss > 2) return Lang.Get("海盗军团");
        return Lang.Get("海盗联队");
    }
}

public class P_WorldBossMission
{
    public int tid;//任务id
    public int finished;//是否完成
    public int end_ts;
    public int get_reward;//是否领取了奖励
    public int do_number;//（某些任务）完成次数
    public int mission_group;//任务类型
    public int day_type;//任务类型 1-每日任务 0-挑战目标

    public bool IsDailyMission {
        get
        {
            return Cfg.Activity2072.IsDailyMission(tid);
        }
    }
}

public class P_Get2072MissionReward
{
    public int challenge_exp;
    public int challenge_add_count;
    public string reward;
    public List<P_WorldBossMission> mission_list;//任务列表
}
public class P_2072GrandServantInfo
{
    public int soldier_count;
    public string drop;
}