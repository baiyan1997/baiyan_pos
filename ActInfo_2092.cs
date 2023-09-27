using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2092 : ActivityInfo
{
    public P_Act2092UniqueInfo UniqueInfo = new P_Act2092UniqueInfo();
    public Action OnPoolTypeChanged { get; set; }
    public Action OnStateChanged { get; set; }
    public int State { get; private set; }//0 初始状态 1随机雷达  2 随机技能
    private int _lastState;
    private int _lastPoolType;
    public Dictionary<int, P_RadarExtraAttr> RadarExtraAttrDic { get; private set; }//key extraAttr id
    public bool Tag = true;
    public override void InitUnique()
    {
        UniqueInfo.radar_id = Convert.ToInt32(_data.avalue["radar_id"]);
        UniqueInfo.main_attr = Convert.ToInt32(_data.avalue["main_attr"]);
        UniqueInfo.radar_lock = Convert.ToInt32(_data.avalue["radar_lock"]);
        UniqueInfo.upgrade_lv = Convert.ToInt32(_data.avalue["upgrade_lv"]);
        UniqueInfo.num = Convert.ToInt32(_data.avalue["num"]);
        UniqueInfo.extra_attr = JsonMapper.ToObject<List<P_RadarExtraAttr>>(_data.avalue["extra_attr"].ToString());
        UniqueInfo.pool_type = Convert.ToInt32(_data.avalue["pool_type"]);
        UniqueInfo.exchange_info =
            JsonMapper.ToObject<List<P_2092ExchangeInfo>>(_data.avalue["exchange_info"].ToString());
        UniqueInfo.exchange_info.Sort(Compare);
        RadarExtraAttrDic = new Dictionary<int, P_RadarExtraAttr>();
        RefreshState();
        RefreshExtraAttrDic();
    }

    private int Compare(P_2092ExchangeInfo a, P_2092ExchangeInfo b)
    {
        int numA = Cfg.Act2092.GetExchangeCostNum(a.id);
        int numB = Cfg.Act2092.GetExchangeCostNum(b.id);
        if (numA == numB)
        {
            return 0;
        }
        else if (numA < numB)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
    private void RefreshExtraAttrDic(bool refresh = false)
    {
        var list = UniqueInfo.extra_attr;
        int len = list.Count;
        if (refresh) RadarExtraAttrDic.Clear();
        for (int i = 0; i < len; i++)
        {
            P_RadarExtraAttr extraAttr = list[i];
            RadarExtraAttrDic.Add(extraAttr.extra_id, extraAttr);
        }
    }
    public void DrawRadar(Action callBack, int drawType)
    {
        _lastPoolType = UniqueInfo.pool_type;
        var lastUpgradeLv = UniqueInfo.upgrade_lv;
        Rpc.SendWithTouchBlocking<P_Act2092UniqueInfo>("rand2092Radar", Json.ToJsonString(drawType), data =>
        {
            RefreshData(data);
            RefreshState();
            RefreshExtraAttrDic(true);
            Uinfo.Instance.AddItem(data.cost_item, false);
            if (lastUpgradeLv < data.upgrade_lv && lastUpgradeLv <= 3 && data.pool_type > 1)
            {
                MessageManager.Show(Lang.Get("恭喜!本次随机雷达强化等级提升至+{0}", data.upgrade_lv));
            }
            if (_lastPoolType != data.pool_type)
            {
                OnPoolTypeChanged?.Invoke();
                _lastPoolType = data.pool_type;
            }
            callBack?.Invoke();
        });
    }

    public void Exchange(int id, Action call)
    {
        Rpc.SendWithTouchBlocking<P_2092ExchangeReturn>("exchange2092Good", Json.ToJsonString(id), data =>
         {
             Uinfo.Instance.AddItem(data.get_item, true);
             Uinfo.Instance.AddItem(data.cost_item, false);
             UniqueInfo.exchange_info = data.exchange_info;
             UniqueInfo.exchange_info.Sort(Compare);
             call?.Invoke();
         });
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }

    public override bool IsAvaliable()
    {
        return CheckPropEnough() || (CheckCanExchange() && Tag);
    }

    public bool CheckPropEnough()
    {
        return Cfg.Act2092.GetCostNum(UniqueInfo.pool_type, State) <= BagInfo.Instance.GetItemCount(70048);
    }

    private void RefreshData(P_Act2092UniqueInfo data)
    {
        UniqueInfo.radar_id = data.radar_id;
        UniqueInfo.main_attr = data.main_attr;
        UniqueInfo.radar_lock = data.radar_lock;
        UniqueInfo.upgrade_lv = data.upgrade_lv;
        UniqueInfo.num = data.num;
        UniqueInfo.extra_attr = data.extra_attr;
        UniqueInfo.pool_type = data.pool_type;
    }
    private void RefreshState()
    {
        if (UniqueInfo.radar_id == 0)
        {
            State = 0;
        }
        else if (UniqueInfo.radar_lock == 1 && UniqueInfo.pool_type == 3)
        {
            State = 2;
        }
        else if (UniqueInfo.radar_lock == 0)
        {
            State = 1;
        }
        if (_lastState != State)
        {
            if (_lastState != 0)
            {
                OnStateChanged?.Invoke();
            }
            _lastState = State;
        }
    }
    public void SetRadarLock(Action callBack)
    {
        Rpc.SendWithTouchBlocking<P_Act2092UniqueInfo>("set2092RadarLock", null, data =>
            {
                UniqueInfo.radar_lock = data.radar_lock;
                RefreshExtraAttrDic(true);
                RefreshState();
                callBack?.Invoke();
            });
    }

    public bool CheckCanExchange()
    {
        int len = UniqueInfo.exchange_info.Count;
        for (int i = 0; i < len; i++)
        {
            var info = UniqueInfo.exchange_info[i];
            if (info.num > 0)
            {
                continue;
            }

            var need = Cfg.Act2092.GetExchangeCostNum(info.id);
            var have = BagInfo.Instance.GetItemCount(ItemId.Line);
            if (need <= have)
            {
                return true;
            }
        }

        return false;
    }

}

public class P_Act2092UniqueInfo
{
    public int radar_id;//雷达id  
    public int main_attr;//雷达主属性
    public int radar_lock;//雷达锁 0-未锁，1-已锁
    public int upgrade_lv;//强化等级
    public int num;//雷达随机次数（达到保底次数会重置）进入第二阶段之后 不锁定雷达进行雷达强化这个值随机雷达时不改变
    public List<P_RadarExtraAttr> extra_attr;//雷达副属性
    public int pool_type;//雷达池阶段
    public string cost_item;//随机雷达消耗
    public List<P_2092ExchangeInfo> exchange_info;
}

public class P_RadarExtraAttr
{
    public int extra_id;// 副属性id
    public int qua;//品质 = 0 未得到的属性
    public int is_lock;//0-未锁 1-已锁
}

public class P_2092ExchangeInfo
{
    public int id;
    public int num;//兑换次数
}

public class P_2092ExchangeReturn
{
    public string cost_item;
    public string get_item;
    public List<P_2092ExchangeInfo> exchange_info;
}