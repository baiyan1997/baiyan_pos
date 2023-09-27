using System;
using System.Collections.Generic;

public class ActInfo_2048 : ActivityInfo
{
    private int _step;
    public int Step
    {
        get { return _step; }
    }

    public int Today
    {
        get { return (int)((TimeManager.ServerTimestamp - _data.startts) / 86400) + 1; }
    }

    private List<bool>_stateList = new List<bool>();
    public List<bool> StateList
    {
        get { return _stateList; }
    }

    private List<P_Item> _rewards;
    public List<P_Item> RewardList
    {
        get { return _rewards; }
    }

    private int _capId;
    public int CaptainId
    {
        get { return _capId; }
    }

   public override void InitUnique()
    {
        base.InitUnique();

        _stateList.Clear();

        _step = Convert.ToInt32(_data.avalue["step"]);
       string rewards = _data.avalue["get_reward_info"].ToString();
        for(int i = 0; i < 7; ++i)
        {
            _stateList.Add(rewards[i].Equals('0')?false:true);
        }
    }

   public override bool IfUpdateAtHour(int hour)
   {
       return hour == 0;
   }

    public override bool IsAvaliable()
    {
        if (!IsDuration())
            return false;

       for(int i = 0; i < Today; i++)
        {
            if (!_stateList[i])
                return true;
        }
        return false;
    }

    public void RequestRewards(Action callback)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getQRKHReward", null, data =>
          {
              Uinfo.Instance.AddItem(data.get_items, true);
              MessageManager.ShowRewards(data.get_items);
              _stateList[Today - 1] = true;

              EventCenter.Instance.RemindActivity.Broadcast(_data.aid,IsAvaliable());
              EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);

              if (callback != null)
                  callback();
          });
    }
}

