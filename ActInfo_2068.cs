using LitJson;
using System;

public class ActInfo_2068 : ActivityInfo
{
    private static ActInfo_2068 _inst;

    private int lottery_had_num = 0;// 玩家已经抽奖的次数
    private int lottery_total_num = 0;// 玩家可抽奖的总数
    private int _needGold = 0;//抽奖花费氪晶数

    private bool _waitBanquetInitData;//标记是否需要等待庆功宴数据刷新

    public static ActInfo_2068 Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2068)ActivityManager.Instance.GetActivityInfo(2068);
            }
            return _inst;
        }
    }

    public override bool  OnInited()
    {
        //黑洞战阶段更新时
        EventCenter.Instance.BHBattleStatusChange.AddListener(OnEvent_BHBattleStatusChange);
        return true;
    }

    public override void OnRemove()
    {
        EventCenter.Instance.BHBattleStatusChange.RemoveListener(OnEvent_BHBattleStatusChange);
    }

    private void OnEvent_BHBattleStatusChange()
    {
        //只在收到服务端推送才会刷新活动
        if (_BHB_STATUS.Inst.NeedRefreshBlackHoleAct)
        {
            _BHB_STATUS.Inst.NeedRefreshBlackHoleAct = false;
            ActivityManager.Instance.RequestUpdateActivityById(ActivityID.BlackHoleBattle);
        }
    }

    public override void InitUnique()
    {
        base.InitUnique();
        if (!_data.IsDuration())
            return;
        //重置单例指向
        _inst = (ActInfo_2068)ActivityManager.Instance.GetActivityInfo(2068);
        //刷新黑洞战信息
        var step = _BHB_STATUS.Inst.GetStep();
        //主动获取一次庆功晚宴信息
        if (step == BLACKHOLE_STEP.CELEBRATION)
            GetBlackHoleBanquetInfo(null);
    }

    //刷新黑洞争夺战数据
    public void RefreshBlackHoleBattleKillCount(Action<P_BHB_getBlackHoleBattleKillCount> callback)
    {
        Rpc.Send<P_BHB_getBlackHoleBattleKillCount>("getBlackHoleBattleKillCount", null, data =>
        {
            if(callback != null)
            {
                callback(data);
            }
        });
    }

    public void OpenBlackHole(Action callback = null)
    {
        //向服务端发送打开黑洞争夺战界面信号
        Rpc.Send<P_None>("openBlackHole", Json.ToJsonString(1), data => {
            if (callback != null)
                callback();
        });
    }

    public void CloseBlackHole(Action callback = null)
    {
        //向服务端发送关闭黑洞争夺战界面信号
        Rpc.Send<P_None>("openBlackHole", Json.ToJsonString(0), data => {
            if (callback != null)
                callback();
        });
    }

    //获取庆功晚宴信息
    public void GetBlackHoleBanquetInfo(Action<P_BlackHoleBanquetInfo> callback)
    {
        //等待数据刷新
        _waitBanquetInitData = true;
        Rpc.Send<P_BlackHoleBanquetInfo>("getBlackHoleBanquetInfo", null, data =>
        {
            lottery_had_num = data.lottery_had_num;
            lottery_total_num = data.lottery_total_num;
            _needGold = data.need_gold;
            //标记庆功宴数据已刷新
            _waitBanquetInitData = false;
            //刷新活动小红点
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());

            if (callback != null)
                callback(data);
        }, (err)=> {
            //标记庆功宴数据已刷新
            _waitBanquetInitData = false;
        });
    }

    //庆功晚宴抽奖
    public void BanquetPrizeDraw(Action<P_getBlackHoleBanquetPrize> callback)
    {
        Rpc.Send<P_getBlackHoleBanquetPrize>("getBlackHoleBanquetPrize", null, data =>
        {
            lottery_had_num = data.lottery_had_num;
            lottery_total_num = data.lottery_total_num;
            _needGold = data.need_gold;

            //抽奖后刷新活动小红点
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());

            if (callback != null)
                callback(data);
        });
    }

    public override bool IsAvaliable()
    {
        //领奖阶段 领奖数据刷新结束且有免费奖励的情况显示小红点
        var step = _BHB_STATUS.Inst.GetStep();
        if (step == BLACKHOLE_STEP.CELEBRATION && _needGold == 0 && !_waitBanquetInitData)
            return true;
        return false;
    }

    //需要登录提醒
    public override bool NeedDailyRemind()
    {
        return true;
    }
}

public class P_BHB_getBlackHoleBattleKillCount
{
    public long cur_kill_sum = 0;
    public P_KillingCount[] cfg_lv = null;
}

public class P_getBlackHoleBanquetPrize
{
    public string get_item = null;
    public int lottery_had_num = 0;
    public string cost_item = null;
    public int lottery_total_num = 0;
    public int need_gold = 0;
}

public class P_BlackHoleBanquetInfo
{
    public int lottery_had_num = 0;// 玩家已经抽奖的次数
    public int lottery_total_num = 0;// 玩家可抽奖的总数
    public int need_gold = 0;//  下次抽奖需要的氪金数
    public P_BanquetBlueprintInfo red_graph_info = null;// 红图纸有关信息
}

public class P_BanquetBlueprintInfo
{
    public int red_graph_total_num = 0;// 本国总共投放的红图纸数量
    public int red_graph_get_num = 0;// 已经被抽走的红图纸数量
    public P_BanquetPrize[] red_graph_list = null;// 得到红图的人数
}

public class P_BanquetPrize
{
    public int uid = 0;
    public int itemid = 0;
    public string name = null;
}

public class P_BlackHoleTreasure
{
    public string get_items;
}

public class P_2068DailyNotice
{
    public int CheckDay;//上一次每日登录检查日期
}