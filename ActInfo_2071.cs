using System;

public class ActInfo_2071 : ActivityInfo
{
    private static ActInfo_2071 _inst;

    public static ActInfo_2071 Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2071)ActivityManager.Instance.GetActivityInfo(2071);
            }
            return _inst;
        }
    }

    public P_RebelBattleInfo RebelData { get; private set; }
    public static int Status = 0;
    public static int MapStep = 0;

    public override bool OnInited()
    {
        //推送监听
        EventCenter.Instance.AddPushListener(OpcodePush.MAP_ACT_REBEL_BATTLE, _EventMAP_ACT_REBEL_BATTLE);

        return true;
    }
    public override void OnRemove()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.MAP_ACT_REBEL_BATTLE, _EventMAP_ACT_REBEL_BATTLE);
    }
    private void _EventMAP_ACT_REBEL_BATTLE(int opcode, string data)
    {
        //Refresh(() =>
        //{
        //    EventCenter.Instance.RebelBattleChange.Broadcast();
        //});
        Refresh(EventCenter.Instance.RebelBattleChange.Broadcast);
    }

    public override void InitUnique()
    {
        base.InitUnique();
        //重置单例指向
        _inst = (ActInfo_2071)ActivityManager.Instance.GetActivityInfo(2071);
        //初始化数据
        Refresh(null);
    }

    public void Refresh(Action callback)
    {
        Rpc.Send<P_RebelBattleInfo>("getMapRiotActInfo", null, result =>
        {
            JDDebug.Dump(result, "getMapRiotActInfo");
            if (result == null || result.aid == 0)
            {
                Status = 0;
                MapStep = 0;
                RebelData = null;
            }
            else
            {
                if (result.act_step == 2)
                {
                    result.aid = MapActID.REBEL_DEFENSE;
                }
                if (RebelData != null)
                {
                    if (RebelData.status != result.status)
                        switch (result.status)
                        {
                            case 2:
                                DialogManager.ShowAsyn<_D_BaseRebuild>(d=>{ d?.OnShow_Rebel(true, result.def_succ); });
                                break;
                            case 1:
                                DialogManager.ShowAsyn<_D_BaseRebuild>(d=>{ d?.OnShow_Rebel(false, result.def_succ); });
                                break;
                        }
                    else if (RebelData.status == 0 && RebelData.def_succ != result.def_succ)
                    {
                        MessageManager.Show(Lang.Get("指挥官，您成功抵挡了第{0}波叛军的攻势"), result.def_succ);
                    }
                }
                RebelData = result;
                Status = RebelData.aid;
                MapStep = RebelData.map_step;
            }

            if (callback != null)
                callback();
        });
    }
}

public class P_RebelBattleInfo : P_MapActInfo
{
    public int status;//0存活，1失败，2通关
    public int act_step;
    public int def_succ;
    public int max_def;
    public int map_step;
}
