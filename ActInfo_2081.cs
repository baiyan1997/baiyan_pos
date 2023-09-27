using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ActInfo_2081 : ActivityInfo
{
    public P_2081Info Info;

    private static ActInfo_2081 _inst;
    public static ActInfo_2081 Instance
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2081)ActivityManager.Instance.GetActivityInfo(2081);
            }
            return _inst;
        }
    }

    public override void InitUnique()
    {
        if (!_data.IsDuration())
            return;
        //重置单例指向
        _inst = (ActInfo_2081)ActivityManager.Instance.GetActivityInfo(2081);
        if (Info == null)
            Info = new P_2081Info();
        Info.act_tili = (int)_data.avalue["act_tili"];
        Info.castle_ids = _data.avalue["castle_ids"].ToString();
        Info.mission_info = JsonMapper.ToObject<List<P_2081MissionInfo>>(_data.avalue["mission_info"].ToString());
        Info.exchange_info = JsonMapper.ToObject<List<P_2081ExchangeInfo>>(_data.avalue["exchange_info"].ToString());
        Info.reward_info = JsonMapper.ToObject<List<P_2081RewardInfo>>(_data.avalue["reward_info"].ToString());
        Info.support_arm = JsonMapper.ToObject<List<P_2081SupportArm>>(_data.avalue["support_arm"].ToString());
        Info.end_turn_ts = int.Parse(_data.avalue["end_turn_ts"].ToString());
        //广播列表刷新事件
        EventCenter.Instance.Act2081ExchangeListUpdate.Broadcast();
        EventCenter.Instance.Act2081MissionListUpdate.Broadcast();
        EventCenter.Instance.Act2081SupportArmListUpdate.Broadcast();
        EventCenter.Instance.Act2081ActActionPointUpdate.Broadcast();
    }

    public override bool OnInited()
    {
        //活动体力刷新
        EventCenter.Instance.AddPushListener(OpcodePush.ACT_TILI_UPDATE, _EventActTiliUpdate);

        //活动过期
        EventCenter.Instance.ActivityOverdue.AddListener(_EventActivityOverdue);

        return true;
    }
    public override void OnRemove()
    {
        //活动体力刷新
        EventCenter.Instance.RemovePushListener(OpcodePush.ACT_TILI_UPDATE, _EventActTiliUpdate);

        //活动过期
        EventCenter.Instance.ActivityOverdue.RemoveListener(_EventActivityOverdue);
    }
    private void _EventActTiliUpdate(int opcode, string data)
    {
        Info.act_tili = int.Parse(data);
        EventCenter.Instance.Act2081ActActionPointUpdate.Broadcast();
    }
    private void _EventActivityOverdue(int aid)
    {
        //活动过期时关闭所有相关界面
        if (aid == 2081)
        {
            DialogManager.CloseDialog<_D_Top_2081Exchange>();
            DialogManager.CloseDialog<_D_Top_2081GeminiBoss>();
            DialogManager.CloseDialog<_D_Top_2081Mission>();
            DialogManager.CloseDialog<_D_Top_Select2081SupportArm>();
            DialogManager.CloseDialog<_D_Top_Upgrade2081SupportArm>();
        }
    }

    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }

    //获得对应id的辅助武装信息
    public P_2081SupportArm GetSupportArmInfo(int id)
    {
        if (Info == null || Info.support_arm == null)
        {
            var data = Cfg.Activity2081.GetSupportArmsData(id);
            if (data != null)
                //辅助武装未解锁时生成一个临时信息
                return new P_2081SupportArm
                {
                    id = id,
                    lv = 0,
                };
            else
                return null;
        }
        for(int i = 0; i < Info.support_arm.Count; i++)
        {
            var info = Info.support_arm[i];
            if (info.id == id)
                return info;
        }
        var baseData = Cfg.Activity2081.GetSupportArmsData(id);
        if (baseData != null)
            //辅助武装未解锁时生成一个临时信息
            return new P_2081SupportArm
            {
                id = id,
                lv = 0,
            };
        else
            return null;
    }

    //获得已解锁辅助武装数量
    public int GetUnlockSupportArmCount()
    {
        return Info.support_arm.Count;
    }

    //兑换奖励
    public void ExchangeReward(int exchangeId, Action<string> callback)
    {
        Rpc.SendWithTouchBlocking<P_2081ExchangeResult>("exchangeReward", Json.ToJsonString(exchangeId), data =>
        {
            //消耗 获得道具
            Uinfo.Instance.AddAndReduceItem(data.get_item, data.cost_item);
            //修改兑换次数
            var list = Info.exchange_info;
            for (int i = 0; i < list.Count; i++)
            {
                var info = list[i];
                if (info.exchange_id == exchangeId)
                {
                    info.exchange_num = data.exchange_num;
                    break;
                }
            }
            //广播列表刷新事件
            EventCenter.Instance.Act2081ExchangeListUpdate.Broadcast();

            if (callback != null)
                callback(data.get_item);
        });
    }

    //领取活动任务奖励
    public void GetTaskReward(int tid, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_2081TaskResult>("get2081TaskReward", Json.ToJsonString(tid), data =>
        {
            //获得奖励
            Uinfo.Instance.AddItemAndShow(data.reward);
            //替换任务列表
            Info.mission_info = data.mission_list;
            //广播列表刷新事件
            EventCenter.Instance.Act2081MissionListUpdate.Broadcast();

            if (callback != null)
                callback();
        });
    }

    //解锁辅助武装
    public void UnlockSupportArm(int supportId, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_UnlockSupportArm>("unlockBattle", Json.ToJsonString(supportId), data =>
        {
            //扣除消耗道具
            Uinfo.Instance.AddAndReduceItem("", data.cost);
            //替换辅助武装列表
            Info.support_arm = data.support_arm;
            //广播列表刷新事件
            EventCenter.Instance.Act2081SupportArmListUpdate.Broadcast();

            if (callback != null)
                callback();
        });
    }

    //升级辅助武装
    //参数sid为辅助武装对应的唯一skillId，不是表中的id
    public void UpgradeSupportArm(int sid, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_2081UpgradeArm>("upgradeSupportArm", Json.ToJsonString(sid), data =>
        {
            //处理道具消耗
            Uinfo.Instance.AddAndReduceItem("", data.cost);
            //修改辅助武装数据
            var list = Info.support_arm;
            for (int i = 0; i < list.Count; i++)
            {
                var info = list[i];
                if (info.sid == sid)
                {
                    list[i] = data.arm_info;
                    break;
                }
            }
            //广播列表刷新事件
            EventCenter.Instance.Act2081SupportArmListUpdate.Broadcast();

            if (callback != null)
                callback();
        });
    }

    //选择辅助武装
    //参数sid为辅助武装对应的唯一skillId，不是表中的id
    public void SelectSupportArm(int sid, Action callback)
    {
        Rpc.SendWithTouchBlocking<List<P_2081SupportArm>>("chooseSupportArm", Json.ToJsonString(sid), data =>
        {
            //修改辅助武装数据
            Info.support_arm = data;
            //广播列表刷新事件
            EventCenter.Instance.Act2081SupportArmListUpdate.Broadcast();

            if (callback != null)
                callback();
        });
    }

    //获得boss信息
    public void GetWildBossInfo(int planetId, Action<P_2081BossInfo> callback)
    {
        Rpc.SendWithTouchBlocking<P_2081BossInfo>("getSirteWildBossInfo", Json.ToJsonString(planetId), data =>
        {
            if (callback != null)
                callback(data);
        });
    }

    //攻打双子座boss
    public void AtkWildBoss(P_WorldUnitInfo planetInfo, string fleetList, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_2081AtkBoss>("atkSirteWildBoss", Json.ToJsonString(planetInfo.planet_id, fleetList, planetInfo.land_type), data =>
        {
            //扣除消耗的能源和体力
            Uinfo.Instance.AddAndReduceItem("", data.cost);

            if (callback != null)
                callback();
        });
    }

    //获得boss坐标文本
    public string GetBossCoordStr()
    {
        var sb = new StringBuilder();
        if (string.IsNullOrEmpty(Info.castle_ids))
            return sb.ToString();
        var ids = Info.castle_ids.Split(',').Select(int.Parse).ToArray();
        for(int i = 0; i < ids.Length; i++)
        {
            var castleId = ids[i];
            sb.Append(" ");
            sb.Append(GetPosStr(castleId));
        }
        return sb.ToString();
    }

    //将坐标id转为坐标文本
    private string GetPosStr(int cid)
    {
        return Vector2Extension.VectorForPlanetID(cid).CoordFormat();
    }
}

public class P_UnlockSupportArm
{
    public List<P_2081SupportArm> support_arm;
    public string cost;
}

public class P_2081TaskResult
{
    public string reward;// 获得任务的奖励
    public List<P_2081MissionInfo> mission_list;//任务列表
}

public class P_2081ExchangeResult
{
    public string cost_item;//兑换消耗的物品
    public int exchange_num;//已经兑换的次数
    public string get_item;//获得的物品
}

public class P_2081Info
{
    public int act_tili;//活动行动力
    public List<P_2081MissionInfo> mission_info;//任务信息 是一个列表
    public List<P_2081ExchangeInfo> exchange_info;//兑换奖励信息 是一个列表
    public List<P_2081RewardInfo> reward_info;//奖励的配置信息 是一个列表 对应的cfg_act_2081_reward里面的信息
    public List<P_2081SupportArm> support_arm;//辅助武装的信息， 是一个列表
    public string castle_ids;//boss的坐标
    public int end_turn_ts;//本轮boss截止时间
}

public class P_2081MissionInfo
{
    public int tid;//任务id
    public int finished;//是否完成
    public int end_ts;
    public int get_reward;//是否领取了奖励
    public int do_number;//（某些任务）完成次数
    public int mission_group;//任务类型
}

public class P_2081SupportArm
{
    public int sid;//辅助武装对应的唯一skillId
    public int id;//cfg_act_2081_support里面对应的id
    public int lv;//等级
    public int choose;//是否被选中 0-未选中 1-选中
}

public class P_2081UpgradeArm
{
    public string cost;
    public P_2081SupportArm arm_info;
}

public class P_2081ExchangeInfo
{
    public int uid;
    public int exchange_id;//奖励表的id
    public int exchange_num;//已领取的次数
    public int aid;//活动id
}

public class P_2081RewardInfo
{
    public int id;
    public string reward;  //奖励
    public int step;       //服务期阶段
    public string cost;    //兑换消耗的物品
    public int max_time;   //兑换最大次数
}

public class P_2081BossInfo
{
    public string ship_ids;
    public int hp;
    public int hp_max;
    //返回boss的ship_id
    public int boss_id
    {
        get
        {
            var ids = ship_ids.Split(',').Select(int.Parse).ToArray();
            for(int i = 0; i < ids.Length; i++)
            {
                var data = Cfg.WorldMonst.GetData(ids[i]);
                //只有boss有展示技能
                if (data != null && !string.IsNullOrEmpty(data.show_skills))
                    return ids[i];
            }
            return 0;
        }
    }
}

public class P_2081AtkBoss
{
    public string cost;
}

//战斗积分信息
public class P_2081BattleScoreInfo
{
    public int battle_coin;
    public float difficult_param;
    public int final_coin;
}