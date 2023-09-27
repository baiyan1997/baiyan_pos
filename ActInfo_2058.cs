using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2058 : ActivityInfo
{
    public List<P_MissionData> _dataAllList { get; private set; }//包含时间节点和每日的活动
    public int _dayCount { get; private set; } //已经登录的天数
    public int _map_step { get; private set; }//服务器进程
    private Dictionary<int, P_MissionData> _dicDayInfo = new Dictionary<int, P_MissionData>();
    private const int DAYNODELIMIT = 100;//大于100为宝箱tid

    public bool HasRemind = false; //本次登陆是否提示玩家签到 客户端用
    public override void InitUnique()
    {
        if (_data.avalue.Count > 0)
        {
            //获得活动项和可领取id
            _map_step = Convert.ToInt32(_data.avalue["map_step"].ToString());
            _dataAllList = JsonMapper.ToObject<List<P_MissionData>>(_data.avalue["mission_info"].ToString());
            InitData();
        }
    }
    private void InitData()
    {
        _dicDayInfo.Clear();
        for (int i = 0; i < _dataAllList.Count; i++)
        {
            _dicDayInfo.Add(_dataAllList[i].tid, _dataAllList[i]);
            if (_dataAllList[i].tid < DAYNODELIMIT)
            {
                if (_dataAllList[i].finished == 1 && _dayCount < _dataAllList[i].tid)
                {
                    _dayCount = _dataAllList[i].tid;
                }
            }
        }
    }
    public override bool IsAvaliable()
    {
        if(_dataAllList == null)
        {
            return false;
        }
        for (int i = 0; i < _dataAllList.Count; i++)
        {
            //已完成 && 未领取 && 在时间范围内
            if (_dataAllList[i].get_reward == 0 && _dataAllList[i].finished == 1 && IsDuration())
            {
                return true;
            }
        }
        return false;
    }
    //是否已经获得奖励
    public bool IsGetDayReward(int dayTid)
    {
        P_MissionData d;
        _dicDayInfo.TryGetValue(dayTid, out d);
        if (d == null)
        {
            throw new KeyNotFoundException("Can't find P_MissionData, dayTid = " + dayTid);
        }
        return (d.get_reward != 0);
    }
    //获取对应时间奖励
    public void GetAct2058Reward(int day, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2058Reward", Json.ToJsonString(day), data =>
        {
            //添加道具
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(rewardsStr);
            ActivityManager.Instance.RequestUpdateActivityById(_aid);//更新活动信息
            if (callback != null)
                callback();
        });
    }

}
public class P_MissionData
{
    public int tid;//对应DayNodeTid节点105,110,115,125和每天签到1,2,,,,,,
    public int finished;//是否可领取[任务已完成] 0未完成 1 已经完成
    public int get_reward;//是否已经领取 0未领取 1 已经领取
    public int do_number; //累计奖励是否已经领取
}