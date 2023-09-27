using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2094 : ActivityInfo
{
    public P_Act2094InitInfo InitInfo { get; set; } = new P_Act2094InitInfo();
    public P_Act2094RankingInfo RankingInfo { get; set; }
    public List<P_Act2094BossInfo> BossList { get; set; }

    public override void InitUnique()
    {
        InitInfo.boss_id = int.Parse(_data.avalue["boss_id"].ToString());
        InitInfo.end_ts = long.Parse(_data.avalue["end_ts"].ToString());
        InitInfo.free = int.Parse(_data.avalue["free"].ToString());
        InitInfo.next_boss_id = int.Parse(_data.avalue["next_boss_id"].ToString());
        BossList = Cfg.Act2094.GetBossInfo(InitInfo.boss_id);
    }

    //开始演习
    public void StartExercise(List<int> teams, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking<P_2094BattleInfo>("atkAct2094Boss", Json.ToJsonString(teams[0]), data =>
        {
            if (data.cost_item != null)
            {
                Uinfo.Instance.AddItem(data.cost_item, false);
            }
            InitInfo.free = data.free;
            if (data.free == 0)
            {
                callBack?.Invoke();
            }
            _Battle.Instance.Show(data.battle_report, null);
        });
    }
    public void RefreshRankingInfo(Action callBack)
    {
        Rpc.SendWithTouchBlocking<P_Act2094RankingInfo>("getAct2094RankInfo", null, data =>
        {
            RankingInfo = data;
            RankingInfo.Refresh();
            RankingInfo.AllRankInfo.Sort((a, b) => b.score - a.score > 0 ? 1 : -1);
            callBack?.Invoke();
        });
    }

    public void GetShipLine(P_Act2094RankItemInfo info)
    {
        Rpc.SendWithTouchBlocking<P_ShipLineupInfo>("getRankShipDetail", Json.ToJsonString(info.uid), data =>
         {
             data.Refresh();
             DialogManager.ShowAsyn<_D_Act2094Lineup>(d=>{ d?.OnShow(2, info, data); });
         });
    }

    public override bool IsAvaliable()
    {
        return InitInfo.free > 0 || BagInfo.Instance.GetItemCount(ItemId.ExerciseToken) > 0 || DoDailyRemind();
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }
}

//阵容信息
public class P_ShipLineupInfo
{
    public string ship_info;
    public List<P_2094ShipInfo> ShipInfos { get; private set; }

    public void Refresh()
    {
        ShipInfos = JsonMapper.ToObject<List<P_2094ShipInfo>>(ship_info);
    }
}

public class P_2094BattleInfo
{
    public P_Battle battle_report;
    public string cost_item;
    public int free;
}
public class P_2094ShipInfo
{
    public int pos;
    public int ship_qua;
    public int ship_lv;
    public int star_lv;
    public int ship_id;
    public int captain_id;
    public int captain_qua;
    public int captain_lv;
    public int radar_id;
}

public class P_Act2094RankingInfo
{
    public string all_rank_info;
    public long u_score;
    public int u_rank;

    public List<P_Act2094RankItemInfo> AllRankInfo { get; private set; }
    public void Refresh()
    {
        AllRankInfo = JsonMapper.ToObject<List<P_Act2094RankItemInfo>>(all_rank_info);
    }
}

public class P_Act2094RankItemInfo
{
    public int rank;//排名
    public int uid;
    public int u_lv;
    public string uname;
    public long score;
}

public class P_Act2094InitInfo
{
    public int boss_id;//第几期
    public int free;//免费次数
    public long end_ts;//本期结束时间
    public int next_boss_id;
}

public class P_Act2094BossInfo
{
    public int pos;
    public int captain_id;
    public int radar_id;
    public int ship_id;
}