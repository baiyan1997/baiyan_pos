using LitJson;
using System;
using System.Collections.Generic;

public class ActInfo_2084 : ActivityInfo
{
    public P_Item[] Rewards { private set; get; }

    public bool IsGet { private set; get; }

    public int Lv { private set; get; }

    public double Buff { private set; get; }

    public override void InitUnique()
    {
        base.InitUnique();
        IsGet = Convert.ToInt32(_data.avalue["get_reward"].ToString()) == 1 ? true : false;
        Lv = Convert.ToInt32(_data.avalue["tid"].ToString());
        Rewards = GlobalUtils.ParseItem(_data.avalue["data"].ToString());
        Buff = Convert.ToDouble(_data.avalue["buff_rate"].ToString()) * 100;
    }

    public override bool IsAvaliable()
    {
        return !IsGet;
    }

    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }

    //领取奖励
    public void RequestRewards(Action callback= null)
    {
        Rpc.SendWithTouchBlocking<P_Act2084Reward>("get2084Reward", null, data =>
          {
              IsGet = true;
              Uinfo.Instance.AddItemAndShow(data.get_items);

              EventCenter.Instance.RemindActivity.Broadcast(2084, IsAvaliable());

              if (callback != null)
                  callback();
          });
    }
}

public class P_Act2084Reward
{
    public string get_items;
}