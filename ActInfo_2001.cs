using System;
using System.Collections.Generic;

public class ActInfo_2001 : ActivityInfo
{
    //大奖励
    private RewardItem _reward2;

    public RewardItem MainReward
    {
        get { return _reward2; }
    }

    // 小奖励
    private Dictionary<int, List<RewardItem>> _itemDict = new Dictionary<int, List<RewardItem>>();
    // private List<RewardItem> _itemList = new List<RewardItem>();
    //当天奖励可领取状态 0不可领取 1可领取 2已领取
    private Dictionary<int, int> _dictDayState = new Dictionary<int, int>();

    public Dictionary<int, int> DayStates
    {
        get { return _dictDayState; }
    }

    // public List<RewardItem> RewardList
    // {
    //     get { return _itemList; }
    // }
    private int _nCurDay;

    public List<RewardItem> GetItemListByDay(int nDay)
    {
        return _itemDict.GetValueOrDefault(nDay, null);
    }

    public override void InitUnique()
    {
        // _itemList.Clear();
        _itemDict.Clear();
        for (int i=0;i<_data.rewards.Count;i++)
        {
            Dictionary<string, string> reward = _data.rewards[i];
            string str1 = reward["reward"];
            string[] items = str1.Split(',');
            foreach (string item in items)
            {
                //获取是第几天
                string[] arrs = item.Split("|");
                int nDay = int.Parse(arrs[0]);
                if(arrs.Length >= 3)
                {
                    RewardItem award = new RewardItem(arrs[1] + "|" + arrs[2]);
                    if (GLobal.IsShip(award.id))
                    {
                        _reward2 = award;
                    }
                    // else
                    // {
                    //     _itemList.Add(award);
                    // }
                    if(!_itemDict.ContainsKey(nDay))
                    {
                        _itemDict[nDay] = new List<RewardItem>();
                    }
                    _itemDict[nDay].Add(award);
                }
            }
        }

        _dictDayState.Clear();
        _dictDayState = new Dictionary<int, int>
        {
            {1,0},
            {2,0},
            {3,0},
        };
        bool bAllGet = true;
        if(_data.rec != null && _data.rec.Length > 0) {
            string[] strArrs = _data.rec.Split("|");
            if(strArrs.Length > 0) {
                for(int i=0;i<strArrs.Length;++i) {
                    int nGet = int.Parse(strArrs[i]);
                    if(nGet == 0) {
                        bAllGet = false;
                        if(_data.act_ts > 0) {
                            DateTime time1 = TimeManager.ServerDateTime;
                            DateTime time2 = TimeManager.ToServerDateTime(_data.act_ts);
                            if(time1.Year == time2.Year && time1.DayOfYear - time2.DayOfYear >= i) {
                                _dictDayState[i + 1] = 1;
                            }
                        }
                    }else {
                        _dictDayState[i + 1] = 2;
                    }
                }
            }
        }else {
            bAllGet = false;
        }
        if(_data.act_ts > 0) {
            _data.can_get_reward = !bAllGet;
            _data.get_all_reward = bAllGet;
        }else {
            _data.can_get_reward = false;
            _data.get_all_reward = false;
        }
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }

    public override bool IsAvaliable()
    {
        return IsDuration() && IsCanGet();
    }

    public override bool IfRefreshOnPush(int opcode)
    {
        return opcode == OpcodePush.Recharge;
    }

    protected bool IsCanGet()
    {
        return _data.can_get_reward;
    }

    //奖励是否全部领完
    public bool IsAllGet()
    {
        return _data.get_all_reward;
    }

    public bool IsCanRealGet(int nDay)
    {
        if(nDay < 0) {
            bool bCanGet = false;
            foreach(var v in DayStates) {
                if(v.Value == 1) {
                    bCanGet = true;
                    break;
                }
            }
            return _data.can_get_reward && bCanGet;
        }else {
            return _data.can_get_reward && (DayStates.GetValueOrDefault(nDay, 0) == 1);
        }
    }

    //判断首充礼包是否可以显示
    public bool IsCanShow(bool bCheckLv)
    {
        if(bCheckLv) {
            cfg_gift_goal info = Cfg.GiftGoal.GetData(1000000000);
            if(info == null) {
                return false;
            }

            string[] arr = info.appear_condition.Split(",");
            if(arr != null && arr.Length > 0) {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                int nLen = arr.Length;
                for(int i=0;i<nLen;++i) {
                    string[] arr1 = arr[i].Split("|");
                    if(arr1 != null && arr1.Length > 1) {
                        dict[arr1[0]] = arr1[1];
                    }
                }

                //玩家等级
                string strLv = dict.GetValueOrDefault("1000", null);
                if(strLv != null && strLv.Length > 0) {
                    int lv = int.Parse(strLv);
                    if(PlayerInfo.Instance.Info.ulevel >= lv) {
                        return IsDuration() && !IsAllGet();
                    }
                }
                //主线任务
                string strMission = dict.GetValueOrDefault("1001", null);
                if(strMission != null && strMission.Length > 0) {
                    int mission = int.Parse(strMission);               
                    if(MissionInfo.Instance.IsMissionFinished(mission)) {
                        return IsDuration() && !IsAllGet();
                    }
                }
            }
            return false;
        }else {
            return IsDuration() && !IsAllGet();
        }
    }

    // 领取奖励
    public void RequestGetAward(int nDay)
    {
        if (IsAvaliable())
        {
            _nCurDay = nDay;
            Rpc.SendWithTouchBlocking<P_ActAward>("getFirtChargeReward", Json.ToJsonString(nDay), On_getFirtChargeReward_SC);
        }
    }
    private void On_getFirtChargeReward_SC(P_ActAward data)
    {
        // 刷新
        // _data.get_all_reward = true;
        // _data.can_get_reward = false;

        Uinfo.Instance.AddItem(data.get_items, true);
        MessageManager.ShowRewards(data.get_items);
        //刷新活动数据
        ActivityManager.Instance.RequestUpdateActivityById(ActivityID.FirstPay); //刷新首充数据
        //上报
        BiReportMgr.GetInstance().Track("首充领取奖励", new Dictionary<string, object>
        {
            {"uid", User.Uid},
            {"day", _nCurDay},
        });

        // EventCenter.Instance.RemindActivity.Broadcast(_data.aid, _data.can_get_reward);
        // EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
    }
}

public class P_ActAward : IProtocolPostprocess
{
    public List<P_ShipInfo> get_ships;
    public List<P_ShipEquip> get_equips; 
    public string get_items;

    public void OnToObject()
    {
        for (int i=0;i<get_ships.Count;i++)
        {
            P_ShipInfo ship = get_ships[i];
            if (!ship.IsEmpty())
                Uinfo.Instance.Temp.PushShipInfo(ship);
        }
        if (get_equips != null && Uinfo.Instance != null)
        {
            for (int i = 0; i < get_equips.Count; i++)
            {
                P_ShipEquip equip = get_equips[i];
                if (!equip.IsEmpty())
                    Uinfo.Instance.Temp.PushEquipInfo(equip);
            }
        }
    }
}
