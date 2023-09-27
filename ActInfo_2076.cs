using LitJson;
using System;
using System.Collections.Generic;

public class ActInfo_2076 : ActivityInfo
{
    private static ActInfo_2076 _inst;

    private P_ActionInfo_2076 _info;

    public bool CanJoin { get; private set; } // 玩家是否可参与夺宝
    public int JoinNum { get; private set; } //参与人数

    public readonly int MaxJoinNum = 30;//最大奖励参与人数

    public int CostGold { get; private set; }//夺宝消耗氪晶

    public P_Item[] NormalRewardPool { get; private set; }//普通奖池
    public P_Item[] SpecialRewardPool { get; private set; }//特殊奖池

    public static ActInfo_2076 Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2076)ActivityManager.Instance.GetActivityInfo(2076);
            }
            return _inst;
        }
    }

    public override bool OnInited()
    {
        return true;
    }

    public override void InitUnique()
    {
        base.InitUnique();
        if (!_data.IsDuration())
            return;
        //重置单例指向
        _inst = (ActInfo_2076)ActivityManager.Instance.GetActivityInfo(2076);
        //解析活动数据
        _info = JsonMapper.ToObject<P_ActionInfo_2076>(_data.avalue["data"].ToString());
        CanJoin = _info.can_join > 0;
        JoinNum = _info.people_num;
        CostGold = _info.cost_gold;
        NormalRewardPool = GlobalUtils.ParseItem(_info.normal_reward_pool);
        SpecialRewardPool = GlobalUtils.ParseItem(_info.special_reward_pool);
    }

    //参与夺宝
    public void SeizeTreasure(Action callback)
    {
        Rpc.SendWithTouchBlocking<P_SeizeTreasure>("starTrek", null, data =>
        {
            CanJoin = data.can_join > 0;
            JoinNum = data.people_num;
            Uinfo.Instance.AddAndReduceItem("", data.cost);
            if (callback != null)
                callback();
        });
    }
}

public class P_ActionInfo_2076
{
    public string normal_reward_pool;//普通奖池
    public string special_reward_pool;//特殊
    public int people_num;//已参与人数
    public int cost_gold;//夺宝消耗氪晶
    public int can_join;//是否可参与夺宝
}

public class P_SeizeTreasure
{
    public string cost;
    public int people_num;
    public int can_join;
}