using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2032 : ActivityInfo
{
    public P_Act2032UserData _info;
    //是否存在前天未领取的奖励
    public bool isPre;
    public bool isCan;
    private bool[] _isCans;
    private static int _MaxCheck = 10;
    private int _checkCounDownNum = _MaxCheck; //重试10次
    private bool _isBegin = false;
    public override void InitUnique()
    {
        if (_info == null)
        {
            _info = new P_Act2032UserData();
        }
        _info.user_startts = Convert.ToInt64(_data.avalue["user_startts"]);
        _info.which_state = Convert.ToInt32(_data.avalue["which_state"]);
        _info.previous_state_reward = Convert.ToInt32(_data.avalue["previous_state_reward"]);
        _info.refresh_time = Convert.ToInt64(_data.avalue["refresh_time"]);
        _info.cfgData = JsonMapper.ToObject<List<P_Act2032Cfg>>(_data.avalue["cfg_data"].ToString());
        RegisterInfo(_info.cfgData);
        SetRefreshTime();
        _DoCheckCountDown();
    }

    private void SetRefreshTime()
    {
        for (int i = 0; i < _info.cfgData.Count; i++)
        {
            if (i == _info.which_state - 1&&_info.user_startts>0)
            {
                var time =_info.user_startts + _info.cfgData[i].time + 1; //多等1s再请求
                if (time !=0 &&time>TimeManager.ServerTimestamp && !UpdateManager.Instance.ContainEvent(time, RefreshAct))
                {
                    UpdateManager.Instance.AddEvent(time, RefreshAct);
                }
            }
        }
    }
    private void RegisterInfo(List<P_Act2032Cfg> rewards)
    {
        isPre = false;
        isCan = false;
        _isCans = new bool[rewards.Count];
        if (_info.refresh_time != 0 && _info.refresh_time > TimeManager.ServerTimestamp &&
          !UpdateManager.Instance.ContainEvent(_info.refresh_time, RefreshAct))
        {
            UpdateManager.Instance.AddEvent(_info.refresh_time, RefreshAct);
        }
        for (int i = 0; i < rewards.Count; i++)
        {
            _info.cfgData[i].rewards = GlobalUtils.ParseItem3(_info.cfgData[i].reward);
            SetAvaliable(i, _isCans);
        }
        SetAvaliable(_info.previous_state_reward);
    }

    private void SetAvaliable(int state)
    {
        if (state > 0)
        {
            isPre = true;
        }
        else
        {
            isPre = false;
        }
    }
    private void SetAvaliable(int index, bool[] isCans)
    {
        if (index == _info.which_state - 1 &&
            TimeManager.ServerTimestamp >= _info.user_startts + _info.cfgData[index].time)
        {
            isCans[index] = true;
        }
        else
        {
            isCans[index] = false;
        }
        if (_info.which_state > 0)
            isCan = isCans[_info.which_state - 1];
    }
    //领取之后再次刷新数据
    public void RefreshAct()
    {
        _isBegin = true;
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }
    private void _DoCheckCountDown()
    {
        if(_checkCounDownNum <= 0) {
            _isBegin = false;
            _checkCounDownNum = _MaxCheck;
            return;
        }

        if(!isCan && _isBegin) {
            _Scheduler.Instance.PerformWithDelay(1, RefreshAct);
            _checkCounDownNum--;
        } else {
            _isBegin = false;
            _checkCounDownNum = _MaxCheck;
        }
    }

    public override bool IsAvaliable()
    {
        if (isPre)
        {
            return true;
        }
        if (isCan)
        {
            return true;
        }
        return false;
    }
    public void GetReward(int state, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2032Reward", Json.ToJsonString(state, null), data =>
            {
                var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
                Uinfo.Instance.AddItem(rewardsStr, true);
                MessageManager.ShowRewards(data.get_items);
                if (callback != null)//先ac 再广播
                    callback();
                EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
            });
    }
}

public class P_Act2032UserData
{
    //某一阶段奖励开始计时的时间
    public long user_startts;
    //处于哪阶段
    public int which_state;
    //前一天没领奖的阶段
    public int previous_state_reward;
    //当天的刷新时间
    public long refresh_time;

    public List<P_Act2032Cfg> cfgData;
}

public class P_Act2032Cfg
{
    public string reward;
    public int step;
    public int id;
    public int time;

    public P_Item3[] rewards;
    //客户端使用阶段状态0:未达成,1:领取计时,2:领取,3:已领
    public int canGet;
}