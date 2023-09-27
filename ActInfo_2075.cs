using System;

public class ActInfo_2075 : ActivityInfo
{
    public int FreeCount; //已使用免费游戏次数
    public int BattleCount; //已获得战斗获得的游戏次数
    public int UseCount; //使用奖励次数
    public int CurrentPos; //当前格子

    private static ActInfo_2075 _inst;
    public static ActInfo_2075 Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2075)ActivityManager.Instance.GetActivityInfo(2075);
            }
            return _inst;
        }
    }
    public override bool OnInited()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.ACT2075UPDATE, _EventACT2075UPDATE);
        return true;
    }
    public override void OnRemove()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.ACT2075UPDATE, _EventACT2075UPDATE);
    }

    public override void InitUnique()
    {
        //重置单例指向
        _inst = (ActInfo_2075)ActivityManager.Instance.GetActivityInfo(2075);
        FreeCount = Convert.ToInt32(_data.avalue["free_time"].ToString());
        BattleCount = Convert.ToInt32(_data.avalue["reward_time"].ToString());
        UseCount = Convert.ToInt32(_data.avalue["use_reward_time"].ToString());
        CurrentPos = Convert.ToInt32(_data.avalue["tid"].ToString());
    }

    private void _EventACT2075UPDATE(int opcode, string data)
    {
        BattleCount = int.Parse(data);
        EventCenter.Instance.UpdateActById.Broadcast(_aid);
    }

    public int GetGameCount()
    {
        return (3 - FreeCount) + (BattleCount - UseCount);
    }

    public void StartGame(Action callback)
    {
        Rpc.SendWithTouchBlocking<P_2075GameResult>("startGalacticExploration", Json.ToJsonString(), data =>
        {
            FreeCount = data.free_time;
            UseCount = data.use_reward_time;
            if (callback != null)
                callback();
        });

    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="result">0-失败 1-成功</param>
    /// <param name="callback"></param>
    public void HandleGameResult(int result, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_2075GameResult>("handleGalacticExploration", Json.ToJsonString(result), data =>
        {
            CurrentPos = data.tid;
            if (!string.IsNullOrEmpty(data.get_rewards))
            {
                Uinfo.Instance.AddItem(data.get_rewards, true);
                MessageManager.ShowRewards(data.get_rewards);
            }
            EventCenter.Instance.UpdateActById.Broadcast(_aid);
            if (callback != null)
                callback();
        });

    }

}

public class P_2075NodeData
{
    public int tid;
    public int state; //
    public int finished; // 0-未完成 1-已完成
    public int get_reward; //0-未领奖 1-已领奖
}

public class P_2075GameResult
{
    public int tid;
    public string get_rewards;
    public int free_time;
    //public int reward_time;
    public int use_reward_time;
}
