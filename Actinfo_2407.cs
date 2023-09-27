using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2407: ActivityInfo
{
    public List<P_Act2047_GiftItem> actData;
    
    public List<P_Act2047_GiftItem> orderData = new List<P_Act2047_GiftItem>();


    public string totalCharge;

    private bool _IsAvaliable;
    
    public override void InitUnique()
    {
        actData = JsonMapper.ToObject<List<P_Act2047_GiftItem>>(_data.avalue["act_2407_data"].ToString());
        // var _actCfgData = JsonMapper.ToObject<_ActInfo_2402_CfgData>(_data.avalue["cfg_data"].ToString());

        orderData = JsonMapper.ToObject<List<P_Act2047_GiftItem>>(_data.avalue["act_2407_data"].ToString());
        orderData.Sort(new Act2047ItemComparer());

        _IsAvaliable = false;
        for (int i = 0; i < orderData.Count; i++)
        {
            var item = orderData[i];
            if (item.buy_type == 2 && item.IsCanBuy == 1)
            {
                _IsAvaliable = true;
                break;
            }
        }
    }
    
    public override bool IsAvaliable()
    {

        return IsDuration() && _IsAvaliable;
    }

    public void GetOrderData()
    {
        
    }
}


public class P_Act2407_Data
{
    public List<P_Act2047_GiftItem> act_2407_data;
}

public class P_Act2047_GiftItem
{
    public int tid;
    public int step;
    public int day;                             //限购次数
    public string mission_desc;                 // 礼包名称
    public string price;                        //货币购买（货币id+数量），非货币购买则留空，免费礼包填0
    public int pay_id;
    public string reward;
    public int buy_count;                       // 购买次数
    public int buy_type;                        // 礼包类型 (1-道具兑换，2-免费购买，3-人民币购买)

    public int IsFree
    {
        get
        {
            var res = buy_type == 2 ? 1 : 0;
            return res;
        }
    }
    
    public int IsCanBuy
    {
        get
        {
            var left = day - buy_count;
            var res = left == 0 ? 0 : 1;
            return res;
        }
    }
}

/// <summary>
/// 人民币支付
/// </summary>
public class P_PayForAct2407
{
    public string reward;
    public string cost;
    public int tid;
}

public class P_BuyAct2407ShopItem
{
    public string reward;
    public string cost;
    public int tid;
}

public class Act_2407_NetMgr : Singleton<Act_2407_NetMgr>
{
    public override void OnBegin()
    {
        base.OnBegin();
        EventCenter.Instance.AddPushListener(new[] { OpcodePush.BUY_ACT_2407_GIFT_SUC }, OnPush);
    }

    public override void OnEnd()
    {
        base.OnEnd();
        EventCenter.Instance.RemovePushListener(new[] { OpcodePush.BUY_ACT_2407_GIFT_SUC }, OnPush);
    }

    private void OnPush(int opcode, string data)
    {
        if (opcode == OpcodePush.BUY_ACT_2407_GIFT_SUC)
        {
            ActivityManager.Instance.RequestUpdateActivityById(ActivityID.MidAutumnLiBao);//更新活动信息
            Uinfo.Instance.AddItemAndShow(data);
        }
    }
    
    public void Pay(int gid, int pay_id, Action callBack = null)
    {
        P_Pay pay = PayConfig.GetAct2104PayData(pay_id);
        var platform = PlatformSdk.GetInstance();
        string platName = platform.GetChannel();
        int priceLevel = Cfg.Payment.GetPriceLevel(pay._id);
        if(platform.isPayNeedOrderId()) {
            Rpc.SendWithTouchBlocking("payForAct2407", Json.ToJsonString(platName, pay.GetPriceNum() * 100, User.Server.index, gid), orderInfo =>
            {
                if ((int)orderInfo[0] == 10) //登录态失效，需要重新刷新
                {
                    platform.Jscode2session();
                    return;
                }
                var data = SdkHelper.CreatePayData(orderInfo[1].ToString(), Convert.ToInt32(orderInfo[2].ToString()), priceLevel.ToString(), pay.GetName(), pay.GetPriceNum(), 1);
                platform.DoPay(data);
                //UniqueInfo.lv_price = 2;
                callBack?.Invoke();
            });
        }
        else 
        {
            var data = SdkHelper.CreatePayData("", 0, pay._id.ToString(), pay.GetName(), pay.GetPriceNum(), 1);
            data.funcStr = "payForAct2407";
            data.paramStr = Json.ToJsonString(platName, pay.GetPriceNum() * 100, User.Server.index, gid);
            platform.DoPay(data);
            callBack?.Invoke();
        }
        
    }

    public void BuyGift(int gid, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking<P_BuyAct2407ShopItem>("buyAct2407ShopItem", Json.ToJsonString(gid), data =>
        {
            ItemHelper.AddAndReduceItem(data.reward, data.cost);
            // MessageManager.Show(Lang.Get("爆竹购买成功"));
            MessageManager.ShowRewards(data.reward);
            
            ActivityManager.Instance.RequestUpdateActivityById(ActivityID.MidAutumnLiBao);//更新活动信息
            if (callBack != null)
                callBack();
        });
    }
}

public class Act2047ItemComparer : IComparer<P_Act2047_GiftItem>
{
    public int Compare(P_Act2047_GiftItem a, P_Act2047_GiftItem b)
    {
        if (a.IsCanBuy < b.IsCanBuy)
        {
            return 1;
        }
        else if (a.IsCanBuy == b.IsCanBuy)
        {
            if (a.IsFree > b.IsFree)
            {
                return -1;
            }
            else if (b.IsFree > a.IsFree)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return -1;
        }
    }
}