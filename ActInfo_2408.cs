using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2408: ActivityInfo
{
    public List<P_Act2408_GiftItem> actData;
    
    public List<P_Act2408_GiftItem> orderData = new List<P_Act2408_GiftItem>();

    private Act2408ItemComparer sortSeed = new Act2408ItemComparer();

    public string totalCharge;

    private bool _IsAvaliable;
    
    public override void InitUnique()
    {
        _IsAvaliable = false;
        actData = JsonMapper.ToObject<List<P_Act2408_GiftItem>>(_data.avalue["act_2408_data"].ToString());
        // var _actCfgData = JsonMapper.ToObject<_ActInfo_2402_CfgData>(_data.avalue["cfg_data"].ToString());

        orderData = JsonMapper.ToObject<List<P_Act2408_GiftItem>>(_data.avalue["act_2408_data"].ToString());
        orderData.Sort(sortSeed);
    }

    public void SordData()
    {
        if (orderData == null)
        {
            return;
        }
        orderData.Sort(sortSeed);
    }
    
    public override bool IsAvaliable()
    {
        return IsDuration() && _IsAvaliable;
    }

    public void GetOrderData()
    {
        
    }
}


public class P_Act2408_Data
{
    public List<P_Act2408_GiftItem> act_2408_data;
}

public class P_Act2408_GiftItem
{
    public int tid;
    public int step;
    public int limit_times;                             //限购次数
    public int lv;                                   // 礼包名称
    public string cost_item;                            // 消耗道具（无消耗时填0）
    public string get_item;                         
    public int buy_count;                       // 购买次数
    public int recommend;                        // 是否推荐（0不推荐，1推荐）

    public int IsFree
    {
        get
        {
            var res = cost_item == "" ? 1 : 0;
            return res;
        }
    }
    
    public int IsCanBuy
    {
        get
        {
            var left = limit_times - buy_count;
            var tmp = left != 0;
            tmp = tmp;
            
            var res = tmp ? 1 : 0;
            if (limit_times == 0)
            {
                res = 1;
            }
            
            return res;
        }
    }
    
    public int IsLvEnough
    {
        get
        {
            var res = PlayerInfo.Instance.Info.ulevel >= lv;
            var tMpres = res ? 1 : 0;
            return tMpres;
        }
    }
}


public class P_BuyAct2408ShopItem
{
    public string reward;
    public string cost;
    public int tid;
}

public class Act_2408_NetMgr : Singleton<Act_2408_NetMgr>
{
    public override void OnBegin()
    {
        base.OnBegin();
    }

    public override void OnEnd()
    {
        base.OnEnd();
    }
    
    

    public void BuyGift(int gid, int buyCnt, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking<P_BuyAct2408ShopItem>("buyAct2408ShopItem", Json.ToJsonString(gid, buyCnt), data =>
        {
            ItemHelper.AddAndReduceItem(data.reward, data.cost);
            MessageManager.ShowRewards(data.reward);
            
            ActivityManager.Instance.RequestUpdateActivityById(ActivityID.MidAutumnDuiHuan);//更新活动信息
            if (callBack != null)
                callBack();
        });
    }
}

public class Act2408ItemComparer : IComparer<P_Act2408_GiftItem>
{
    public int Compare(P_Act2408_GiftItem a, P_Act2408_GiftItem b)
    {
        if (a.IsFree != b.IsFree)
        {
            return  b.IsFree - a.IsFree;
        }

        if (a.IsCanBuy != b.IsCanBuy)
        {
            return b.IsCanBuy - a.IsCanBuy;
        }

        if (a.IsLvEnough != b.IsLvEnough)
        {
            return b.IsLvEnough - a.IsLvEnough;
        }

        if (a.tid != b.tid)
        {
            return a.tid - b.tid;
        }

        return -1;
        
        if (a.IsFree > b.IsFree) return -1;
        else if (a.IsFree == b.IsFree)
        {
            if (a.IsCanBuy > b.IsCanBuy) return -1;
            else if (a.IsCanBuy == b.IsCanBuy)
            {
                if (a.IsLvEnough > b.IsLvEnough) return -1;
                else if (a.IsLvEnough == b.IsLvEnough)
                {
                    if (a.tid > b.tid) return 1;
                    else if (a.tid == b.tid) return 1;
                    else return -1;
                }
                return -1;
            }
            else return 1;
        }
        else return 1;
        
    }
}