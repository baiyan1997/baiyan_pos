using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActInfo_2201 : ActivityInfo
{
    private static ActInfo_2201 _inst;

    private List<Act2201ItemInfo> _itemList1 = new List<Act2201ItemInfo>();

    private List<Act2201ItemInfo> _itemList2 = new List<Act2201ItemInfo>();

    private List<Act2201ItemInfo> _itemList3 = new List<Act2201ItemInfo>();

    public static ActInfo_2201 Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2201)ActivityManager.Instance.GetActivityInfo(2201);
            }
            return _inst;
        }
    }

    public override bool OnInited()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.ACT2201RECHARGE, _EventACT2201RECHARGE);

        return true;
    }
    public override void OnRemove()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.ACT2201RECHARGE, _EventACT2201RECHARGE);
    }
    private void _EventACT2201RECHARGE(int opcode, string data)
    {
        JsonData get = JsonMapper.ToObject(data);
        string gets = get["get"].ToString();
        Uinfo.Instance.AddItemAndShow(gets);
    }

    public override void InitUnique()
    {
        base.InitUnique();
        _inst = (ActInfo_2201)ActivityManager.Instance.GetActivityInfo(2201);

        P_Act2201Data goodsInfo = JsonMapper.ToObject<P_Act2201Data>(_data.avalue["data"].ToString());
        if (goodsInfo == null)
        {
            throw new ArgumentNullException("goodsInfo");
        }

        InitData(goodsInfo);
    }

    private void InitData(P_Act2201Data data)
    {
        _itemList1.Clear();
        _itemList2.Clear();
        _itemList3.Clear();

        for ( int i=0;i< data.goods_info.Count;i++)
        {
            var config = data.goods_info[i];
            Act2201ItemInfo info = new Act2201ItemInfo(config);
            switch (config.type)
            {
                case 1:
                    {
                        _itemList1.Add(info);
                        break;
                    }
                case 2:
                    {
                        _itemList2.Add(info);
                        break;
                    }
                case 3:
                    {
                        _itemList3.Add(info);
                        break;
                    }
            }
        }
    }

    public List<Act2201ItemInfo> GetGoodListByType(int type)
    {
        switch (type)
        {
            case 1:
                {
                    return _itemList1;
                }
            case 2:
                {
                    return _itemList2;
                }
            case 3:
                {
                    return _itemList3;
                }
            default: return null;
        }
    }

    private long _buyTime = 0;
    //下订单
    public void Buy(P_Pay pay, int id1, string id2Str, string id3Str)
    {
        if (TimeManager.ServerTimestamp - _buyTime < 2.0)
        {
            Alert.Ok(Lang.Get("请勿快速重复购买"));
            return;
        }

        var platform = PlatformSdk.GetInstance();

        var a = Alert.YesNo(Lang.Get("确定以{0}价格购买礼包组合?",pay._price));
        a.SetYesCallback(() =>
        {
            _buyTime = TimeManager.ServerTimestamp;
            var param = Json.ToJsonString(platform.GetChannel(), pay.GetPriceNum() * 100, User.Server.index, pay._id, id1, id2Str, id3Str);
            if(platform.isPayNeedOrderId()) {
                Rpc.SendWithTouchBlocking("placeNewGiftBagOrder", param, orderInfo =>
                {
                    if ((int)orderInfo[0] == 10) //登录态失效，需要重新刷新
                    {
                        platform.Jscode2session();
                        return;
                    }
                    //刷新消耗次数
                    ActivityManager.Instance.RequestUpdateActivityById(2201);

                    int priceLevel = Cfg.Payment.GetPriceLevel(pay._id);
                    var data = SdkHelper.CreatePayData(orderInfo[1].ToString(),Convert.ToInt32(orderInfo[2].ToString()), priceLevel.ToString(), pay.GetName(),
                        pay.GetPriceNum(), 1);
                    platform.DoPay(data);
                });
            }else {
                //刷新消耗次数
                ActivityManager.Instance.RequestUpdateActivityById(2201);
                int priceLevel = Cfg.Payment.GetPriceLevel(pay._id);
                var data = SdkHelper.CreatePayData("", 0, pay._id.ToString(), pay.GetName(),
                    pay.GetPriceNum(), 1);
                data.funcStr = "placeNewGiftBagOrder";
                data.paramStr = Json.ToJsonString(platform.GetChannel(), pay.GetPriceNum() * 100, User.Server.index, pay._id, id1, id2Str, id3Str);
                platform.DoPay(data);
            }

            a.Close();
        });
    }
}

public class Act2201ItemInfo
{
    public int Id { private set; get; }

    private int buyCount;
    private int limitCount;

    public int Type { private set; get; }

    public int leftCount
    {
        get
        {
            return limitCount - buyCount;
        }
    }

    public bool IsLimit
    {
        get
        {
            return limitCount != 0;
        }
    }

    public int PayId
    {
        get
        {
            if (Type == 1)
                return Cfg.Activity2201.GetPayId(Id);
            return 0;
        }
    }

    public P_Item Item
    {
        get
        {
            return new P_Item(Cfg.Activity2201.GetData(Id).goods);
        }
    }

    public P_Pay Pay { private set; get; }

    public Act2201ItemInfo(P_Act2201Item data)
    {
        Id = data.id;
        Type = data.type;
        buyCount = data.buy_count;
        limitCount = Cfg.Activity2201.GetData(Id).limit_buy_count;
        if (PayId != 0)
        {
            var cfg = Cfg.Payment.GetData(PayId);
            Pay = new P_Pay
            {
                _id = cfg.id,
                _name = cfg.name,
                _price = PayConfig.GetPrice(cfg.id),
                _qua = cfg.qua,
                _desc = cfg.desc,
            };
        }
    }
}

public class P_Act2201Item
{
    public int buy_count;
    public int id;
    public int type;
}

public class P_Act2201Data
{
    public List<P_Act2201Item> goods_info;
}