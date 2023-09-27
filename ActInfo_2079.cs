using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2079 : ActivityInfo
{
    //任务
    public List<P_Act2079Reward> TaskList { private set; get; }
    //格子信息 集合
    public List<P_Act2079BoardInfo> CellList { private set; get; }
    //成就奖励
    public List<P_Act2079Reward> AchieveList { private set; get; }

    public List<int> PassPos { private set; get; }

    public long EnergyCount
    {
        get
        {
            return BagInfo.Instance.GetItemCount(ItemId.Act2079Energy);
        }
    }

    //能量碎片
    public long CrystalCount
    {
        get
        {
            return BagInfo.Instance.GetItemCount(ItemId.Act2079Crystal);
        }
    }

    public long ExploreCount
    {
        get
        {
            return BagInfo.Instance.GetItemCount(ItemId.Act2079Medal);
        }
    }

    public int ExploreEnergy
    {
        get
        {
            return Cfg.FuncAttr.GetIntAttrByName("explore_energy_chip_cost");
        }
    }

    public int ExploreNuclear
    {
        get
        {
            return Cfg.FuncAttr.GetIntAttrByName("explore_heteronuclear_nucleus_cost");
        }
    }

    //当前位置
    public int PosId { private set; get; }

    public override void InitUnique()
    {
        if (TaskList == null)
            TaskList = new List<P_Act2079Reward>();
        if (AchieveList == null)
            AchieveList = new List<P_Act2079Reward>();
        if (CellList == null)
            CellList = new List<P_Act2079BoardInfo>();
        if (PassPos == null)
            PassPos = new List<int>();

        TaskList.Clear();
        AchieveList.Clear();
        CellList.Clear();
        PassPos.Clear();

        List<P_Act2079Reward> list = JsonMapper.ToObject<List<P_Act2079Reward>>(_data.avalue["mission_info"].ToString());
        CellList = JsonMapper.ToObject<List<P_Act2079BoardInfo>>(_data.avalue["checkerboard_info"].ToString());

        InitRewardData(list);

        PosId = Convert.ToInt32(_data.avalue["position"].ToString());

        PassPos.Clear();
        string[] posstr = _data.avalue["pass_positions"].ToString().Split(',');
        for (int i = 0; i < posstr.Length; i++)
        {
            string s = posstr[i];
            PassPos.Add(int.Parse(s));
        }
    }

    private void InitRewardData(List<P_Act2079Reward> info)
    {
        for (int i = 0; i < info.Count; i++)
        {
            int type = int.Parse(info[i].data);
            if (type == 0)
            {
                TaskList.Add(info[i]);
            }
            else if (type == 1)
            {
                AchieveList.Add(info[i]);
            }
        }

        TaskList.Sort(Act2079SortCompare);
        AchieveList.Sort(Act2079SortCompare);
    }

    private int Act2079SortCompare(P_Act2079Reward a, P_Act2079Reward b)
    {
        if (a.get_reward == b.get_reward)
        {
            if (a.finished == b.finished)
                return a.tid - b.tid;
            else
                return b.finished - a.finished;
        }
        else
        {
            return a.get_reward - b.get_reward;
        }
    }

    public override bool IsAvaliable()
    {
        return IsAchieveAvailable() || IsTaskAvailable();
    }
    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }
    public bool IsAchieveAvailable()
    {
        for (int i = 0; i < AchieveList.Count; i++)
        {
            var item = AchieveList[i];
            if (item.finished == 1 && item.get_reward == 0)
                return true;
        }
        return false;
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }

    public bool IsTaskAvailable()
    {
        for (int i = 0; i < TaskList.Count; i++)
        {
            var item = TaskList[i];
            if (item.finished == 1 && item.get_reward == 0)
                return true;
        }
        return false;
    }

    //
    public int GetSliderTid()
    {
        if (AchieveList.Count <= 0)
            return -1;

        if (AchieveList[0].get_reward == 1)
            return -1;

        return AchieveList[0].tid;
    }

    //刷新所有格子信息
    public void RefreshMapInfo(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2079ExploreInfo>("getExploreInfo", null, data =>
          {
              PosId = data.position;
              CellList = data.checkerboard_info;
              PassPos.Clear();
              string[] posstr = data.pass_positions.Split(',');
              for (int i=0;i<  posstr.Length;i++)
              {
                  string s = posstr[i];
                  PassPos.Add(int.Parse(s));
              }

              if (callback != null)
                  callback();
          });
    }

    //普通探索
    public void RequestNormalExplore(Action<string, int> callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2079ExploreData>("ruinsExplore", Json.ToJsonString(1, 0), data =>
           {
               Uinfo.Instance.AddItem(data.cost_item, false);
               Uinfo.Instance.AddItem(data.reward, true, true);

               if (callback != null)
                   callback(data.reward, data.move_step);
           });
    }

    //定向探索
    public void RequestDirectExplore(int steps, Action<string> callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2079ExploreData>("ruinsExplore", Json.ToJsonString(2, steps), data =>
         {
             Uinfo.Instance.AddItem(data.cost_item, false);
             Uinfo.Instance.AddItem(data.reward, true, true);

             if (callback != null)
                 callback(data.reward);
         });
    }

    //领取任务奖励
    public void GetAct2079Reward(int tid)
    {
        //Rpc.SendWithTouchBlocking<P_Act2079GetRewardData>("get2079TaskReward", Json.ToJsonString(tid), data =>
        //{
        //    Uinfo.Instance.AddItemAndShow(data.get_items);

        //    ActivityManager.Instance.RequestUpdateActivityById(_aid);
        //});
        Rpc.SendWithTouchBlocking<P_Act2079GetRewardData>("get2079TaskReward", Json.ToJsonString(tid), On_get2079TaskReward_SC);
    }
    private void On_get2079TaskReward_SC(P_Act2079GetRewardData data)
    {
        Uinfo.Instance.AddItemAndShow(data.get_items);

        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }
}


//领取任务奖励
public class P_Act2079GetRewardData
{
    public string get_items;
}

//废墟探索
public class P_Act2079ExploreData
{
    public int move_step;//移动的步数
    public string cost_item;//消耗的物品
    public string reward;//获得的奖励
    public int explore_medal_count;//勋章数量
}

public class P_Act2079Reward
{
    public int tid;
    public int finished;
    public int get_reward;
    public int do_number;
    public string data; //1:任务  2：成就
}

public class P_Act2079ExploreInfo
{
    public int position;
    public List<P_Act2079BoardInfo> checkerboard_info;
    public string pass_positions;
}


public class P_Act2079BoardInfo
{
    public int lv;
    public int position;
    public string reward;
}
