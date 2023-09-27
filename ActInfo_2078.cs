using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ActInfo_2078 : ActivityInfo
{
    public long ToyCount
    {
        get
        {
            return BagInfo.Instance.GetItemCount(ItemId.Act2078ToyId);
        }
    }

    public int ExchangeCount { private set; get; }

    public int Progress { private set; get; }

    public long CoinCount
    {
        get
        {
            return BagInfo.Instance.GetItemCount(ItemId.Act2078CoinId);
        }
    }

    public List<P_Act2078Mission> MissionList { private set; get; }

    public List<P_Act2078Exchange> ExchangeList { private set; get; }

    public P_Item[] BoxRewardList { private set; get; }

    public P_Item[] GamePool { private set; get; }

    public bool IsFree { private set; get; }

    private int _oldExCount = 0;
    private int _canExchangeCount = 0;

    private bool _lastGameAvailable;

    public override bool OnInited()
    {
        //道具刷新时刷新小红点状态
        EventCenter.Instance.UpdatePlayerItem.AddListener(() =>
        {
            var lastHaveUnCheckExchange = _lastGameAvailable;
            if (IsGameAvailable() != _lastGameAvailable)
                ActivityManager.Instance.ReduceActPriority(_aid);
        });
        return true;
    }

    public override void InitUnique()
    {
        GamePool = null;

        IsFree = Convert.ToInt32(_data.avalue["card_status"].ToString()) == 1;
        Progress = Convert.ToInt32(_data.avalue["progress_value"].ToString());
        string poolstr = _data.avalue["game_pool"].ToString();
        if (!string.IsNullOrEmpty(poolstr))
        {
            GamePool = GlobalUtils.ParseItem(poolstr);
        }
        ExchangeList = JsonMapper.ToObject<List<P_Act2078Exchange>>(_data.avalue["exchange_info"].ToString());
        MissionList = JsonMapper.ToObject<List<P_Act2078Mission>>(_data.avalue["mission_info"].ToString());
        BoxRewardList = GlobalUtils.ParseItem(_data.avalue["progress_reward_info"].ToString());
        _canExchangeCount = GetExchangeCount();
    }

    //开始游戏
    public void PlayGame(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2078GameData>("startAnnieGame", null, data =>
          {
              Uinfo.Instance.AddItem(data.cost_item, false);
              IsFree = data.card_status;

              EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());

              if (callback != null)
                  callback();
          });
    }

    //抽卡
    public void DrawCard(Action<string> callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2078DrawData>("drawAnnieCard", null, data =>
        {
            Progress = data.progress_value;
            IsFree = data.card_status;
            Uinfo.Instance.AddItemAndShow(data.get_item);

            if (callback != null)
                callback(data.get_item);
        });
    }

    //完成任务，领取游戏币
    public void GetGameCoin(int tid, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2078GetCoin>("getAnnieGameCoin", Json.ToJsonString(tid), data =>
          {
              Uinfo.Instance.AddItemAndShow(data.get_items);

              ActivityManager.Instance.RequestUpdateActivityById(2078);

              if (callback != null)
                  callback();

          });
    }

    //兑换奖励
    public void RequestExchangeReward(int id, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2078ExchangeData>("exchangeByAnnieToy", Json.ToJsonString(id), data =>
          {
              Uinfo.Instance.AddItem(data.cost_item, false);
              Uinfo.Instance.AddItemAndShow(data.get_item);

              ActivityManager.Instance.RequestUpdateActivityById(2078);

              if (callback != null)
                  callback();
          });
    }

    //领取进度奖励
    public void GetProgressReward(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2078ProgressReward>("getAnnieProgressReward", null, data =>
          {
              Uinfo.Instance.AddItemAndShow(data.reward);
              Progress = data.progress_value;

              if (callback != null)
                  callback();
          });
    }

    public override bool IsAvaliable()
    {
        return IsGameAvailable() || IsMissionTip() || IsExchangeTip();
    }

    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }

    //游戏币数量满足能够开启游戏
    public bool IsGameAvailable()
    {
        int gameCost = Cfg.FuncAttr.GetIntAttrByName("annie_cost");

        if (CoinCount >= gameCost)
        {
            _lastGameAvailable = true;
            return true;
        }
        _lastGameAvailable = false;
        return false;
    }

    //任务红点
    public bool IsMissionTip()
    {
        //MissionList有可领但没领的
        for (int i = 0; i < MissionList.Count; i++)
        {
            var mission = MissionList[i];
            if (mission.finished && !mission.get_reward)
                return true;
        }

        return false;
    }

    //兑换红点
    public bool IsExchangeTip()
    {
        string date = TimeManager.ServerDateTime.ToLongDateString();
        string recordDate = PlayerPrefs.GetString(User.Uid + "Act2078", "");
        //今天没有记录，只要有可兑换的就给红点
        if (!date.Equals(recordDate))
        {
            if (_canExchangeCount > 0)
            {
                _oldExCount = _canExchangeCount;
                return true;
            }
            return false;
        }

        //今天不是第一次打开，那么有新的可兑换才给红点
        if (_canExchangeCount > _oldExCount)
        {
            _oldExCount = _canExchangeCount;
            return true;
        }
        return false;
    }

    public List<P_Item> ListRandom(List<P_Item> myList)
    {

        Random ran = new Random();
        List<P_Item> newList = new List<P_Item>();
        int index = 0;
        P_Item temp = null;
        for (int i = 0; i < myList.Count; i++)
        {

            index = ran.Next(0, myList.Count - 1);
            if (index != i)
            {
                temp = myList[i];
                myList[i] = myList[index];
                myList[index] = temp;
            }
        }
        return myList;
    }

    private int GetExchangeCount()
    {
        int canExchange = 0;
        for (int i = 0; i < ExchangeList.Count; i++)
        {
            var exchange = ExchangeList[i];
            cfg_act_2078_reward info = Cfg.Activity2078.GetRewardData(exchange.exchange_id);
            if (exchange.exchange_num <= info.max_time || info.max_time == 0)
            {
                P_Item item2 = new P_Item(info.cost_item);
                if (ToyCount >= item2.count)
                    canExchange++;
            }
        }
        return canExchange;
    }
}

public class P_Act2078Mission
{
    public int mission_group;
    public long end_ts;
    public bool finished;
    public bool get_reward;
    public int do_number;
    public int tid;
}

public class P_Act2078Exchange
{
    public int exchange_id;
    public int exchange_num;
}


public class P_Act2078GameData
{
    public string cost_item;// 消耗的道具
    public bool card_status;
}

public class P_Act2078DrawData
{
    public string get_item;// 获得的奖品
    public bool card_status;
    public int progress_value;//进度值
}

public class P_Act2078ProgressReward
{
    public string reward;
    public int progress_value;// 进度值
}

public class P_Act2078GetCoin
{
    public string get_items;
}

public class P_Act2078ExchangeData
{
    public string cost_item;// 消耗的道具
    public int exchange_count;//已兑换的次数
    public string get_item;//获得的奖品
}

public enum Act2078RewardType
{
    CardRewardType = 1, //卡片奖励类型
    TaskRewardType = 2,//进度宝箱的奖励
    ExchangeRewardType = 3, //兑换奖励类型
}