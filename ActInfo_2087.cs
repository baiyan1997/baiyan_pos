using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActInfo_2087 : ActivityInfo
{
    public int Score { private set; get; }

    public List<P_Act2087Rank> RankList;

    public List<P_Item> StateRewards = new List<P_Item>();

    public List<P_Item> PersonRewards = new List<P_Item>();

    public int FreeCount { private set; get; }

    public int CostCount { private set; get; }

    //阶段'step' 1-保护阶段，2-战斗阶段，3-领奖阶段
    public int Step { private set; get; }

    public bool IsGetPersonReward { private set; get; }

    //个人排名
    public Act2087RankData RankData { private set; get; }

    public int FocusPos{private set;get;}

    public List<int> PointPosList = new List<int>();

    public int MapStep { private set; get; }

    //个人累计捐献兵力
    public long DonateSoldierCount;

    public override void InitUnique()
    {
        base.InitUnique();

        if (!_data.IsDuration())
            return;

        Score = int.Parse(_data.avalue["score"].ToString());

        FreeCount = int.Parse(_data.avalue["free_time"].ToString());

        CostCount = int.Parse(_data.avalue["cost_time"].ToString());

        Step = int.Parse(_data.avalue["step"].ToString());

        MapStep = int.Parse(_data.avalue["map_step"].ToString());

        DonateSoldierCount = int.Parse(_data.avalue["assistant_army_add_solider"].ToString());

        string str1 = _data.avalue["state_reward"].ToString();

        string str2 = _data.avalue["person_reward"].ToString();

        IsGetPersonReward = int.Parse(_data.avalue["get_person_reward"].ToString()) == 1 ? true : false;

        StateRewards.Clear();

        if (!string.IsNullOrEmpty(str1))
        {
            var list = GlobalUtils.ParseItem(str1);

            for (int i = 0; i < list.Length; i++)
            {
                P_Item item = list[i];
                bool add = true;
                for (int j = i + 1; j < list.Length; j++)
                {
                    if (item.id == list[j].id && item.count == list[j].count)
                    {
                        add = false;
                    }
                }
                if (add)
                    StateRewards.Add(item);
            }
        }

        PersonRewards.Clear();

        if (!string.IsNullOrEmpty(str2))
        {
            PersonRewards = GlobalUtils.ParseItem(str2).ToList();
        }

        RankList = JsonMapper.ToObject<List<P_Act2087Rank>>(_data.avalue["state_rank_info"].ToString());

        string pos = _data.avalue["focus_pos"].ToString();
        FocusPos = int.Parse(pos);

        string pos2 = _data.avalue["stellar_point_pos"].ToString();

        PointPosList.Clear();
        if (!string.IsNullOrEmpty(pos2))
        {
            string[] strs = pos2.Split(',');
            for (int i=0;i< strs.Length;i++)
            {
                var s = strs[i];
                PointPosList.Add(int.Parse(s));
            }
        }
    }

    //活动开始时，活动入口出现一次红点，点击进入界面就消失,有免费奖励可以领时

    public override bool IsAvaliable()
    {
        string time = PlayerPrefs.GetString("Act2087" + User.login_id, "");
        if (!time.Equals(_data.startts.ToString()))
        {
            return true;
        }
        if(Step == 3)
        {
            return FreeCount > 0 || (PersonRewards.Count > 0 && !IsGetPersonReward);
        }
        return false;
    }


    //领取奖励    领奖类型 1-领取势力奖励，2-领取个人奖励
    public void Get2087Reward(int type, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2087Reward>("get2087Reward", Json.ToJsonString(type), data =>
        {
            FreeCount = data.free_time;
            CostCount = data.cost_time;
            IsGetPersonReward = data.get_person_reward;
            Uinfo.Instance.AddItemAndShow(data.get_item);
            PlayerInfo.Instance.AddGold(-data.cost);

            if (callback != null)
                callback();

            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
   

    //个人排名
    public void GetRankList(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2087RankData>("getSupremacyKillRankInfo", null, data =>
          {
              RankData = data;

              PersonRewards.Clear();

              if (!string.IsNullOrEmpty(data.reward))
              {
                  PersonRewards = GlobalUtils.ParseItem(data.reward).ToList();
              }

              if (callback != null)
                  callback();
          });
    }
}

public class P_Act2087Rank
{
    public string state_name;

    public int score;

    public int rank;

    public int fire_status;//战斗状态 1-平静 2-正常 3-激烈

    public int show_free_times;
}

public class P_Act2087Building
{
    public int id;
    public int num;
}

public class Act2087RankData
{
    public int current_kill;
    public int current_lv;
    public int next_kill;
    public int next_lv;
    public string reward;
    public List<Act2087RankItemData> kill_rank_info;
}

public class Act2087RankItemData
{
    public int rank;
    public string uname;
    public int kill_sum;
    public int ustate;
}

public class Act2087Reward
{
    public string get_item;
    public int free_time;
    public int cost_time;
    public bool get_person_reward;
    public int cost;
}
