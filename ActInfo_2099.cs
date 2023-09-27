using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2099 : ActivityInfo
{
    //胶囊兑换刷新时间
    private long _refreshTime;
    //培养台信息
    private List<Act2099CultureTable> _cultureTablesInfo;
    private List<Act2099ExchangeCapsule> _capsuleShopInfo;
    private List<Act2099GiftInfo> _giftInfo;
    private List<Act2099TaskInfo> _taskInfo;
    private int _step;
    public override void InitUnique()
    {
        _refreshTime = int.Parse(_data.avalue["next_zero_ts"].ToString());
        _step = int.Parse(_data.avalue["map_step"].ToString());
        _taskInfo = JsonMapper.ToObject<List<Act2099TaskInfo>>(_data.avalue["mission_info"].ToString());

        _cultureTablesInfo = JsonMapper.ToObject<List<Act2099CultureTable>>(_data.avalue["cultivar_info"].ToString());
        _capsuleShopInfo = JsonMapper.ToObject<List<Act2099ExchangeCapsule>>(_data.avalue["capsule_info"].ToString());
       

        _giftInfo = JsonMapper.ToObject<List<Act2099GiftInfo>>(_data.avalue["package_info"].ToString());
    }

    public int GetStep()
    {
        return _step;
    }
    public long GetRefreshTime()
    {
        return _refreshTime;
    }

    public List<Act2099CultureTable> GetCultureTableInfo()
    {
        return _cultureTablesInfo;
    }

    public List<Act2099ExchangeCapsule> GetCapsuleShopInfo()
    {
        return _capsuleShopInfo;
    }

    public List<Act2099GiftInfo> GetGiftInfo()
    {
        return _giftInfo;
    }

    public List<Act2099TaskInfo> GetTaskInfo()
    {
        return _taskInfo;
    }

    //领取胶囊币
    public void GetCapsuleCoin(int tid, Action<Act2099TaskInfo> callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2099FinishTask>("getAct2099MissionReward",
            Json.ToJsonString(tid),
            data =>
            {
                Uinfo.Instance.AddItemAndShow(data.get_reward);
                for (int i = 0; i < _taskInfo.Count; i++)
                {
                    if (_taskInfo[i].tid == tid)
                    {
                        _taskInfo[i] = data.mission;
                    }
                }
                callback?.Invoke(data.mission);
            });
    }
    //解锁培养台
    public void UnlockCulture(int tid, Action<Act2099CultureTable> callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2099UnlockTable>("unLockCultivar",
            Json.ToJsonString(tid),
            data =>
            {

                for (int i = 0; i < _cultureTablesInfo.Count; i++)
                {
                    if (_cultureTablesInfo[i].cultivar_id == tid)
                    {
                        _cultureTablesInfo[i] = data.cultivar_info;
                    }
                }
                Uinfo.Instance.AddItem(data.cost, false);
                callback?.Invoke(data.cultivar_info);
            });
    }
    //兑换时空胶囊
    public void ExchangeCapsule(int id, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2099Exchange>("exchangeCapsule",
            Json.ToJsonString(id),
            data =>
            {
                _capsuleShopInfo = data.exchange_capsule;
                Uinfo.Instance.AddItemAndShow(data.get_item);
                Uinfo.Instance.AddItem(data.cost, false);
                callback?.Invoke();
            });
    }
    //领取胶囊奖励
    public void GetCapsuleReward(int tid, Action<string, Act2099CultureTable> callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2099GetReward>("getCapsuleReward",
            Json.ToJsonString(tid),
            data =>
            {
                for (int i = 0; i < _cultureTablesInfo.Count; i++)
                {
                    if (_cultureTablesInfo[i].cultivar_id == tid)
                    {
                        _cultureTablesInfo[i] = data.cultivar_info;
                    }
                }
                Uinfo.Instance.AddItem(data.get_item, true);
                callback?.Invoke(data.get_item, data.cultivar_info);
            });
    }
    //购买胶囊礼包
    public void BuyCapsuleGift(cfg_act_2099_package payInfo, Action callback = null)
    {

        var platform = PlatformSdk.GetInstance();
        if(platform.isPayNeedOrderId()) {
            Rpc.SendWithTouchBlocking("buyCapsuleGift",
            Json.ToJsonString(platform.GetChannel(), payInfo.price_cn * 100, User.Server.index, payInfo.id),
            orderInfo =>
            {
                if ((int)orderInfo[0] == 10) //登录态失效，需要重新刷新
                {
                    platform.Jscode2session();
                    return;
                }
                var data = SdkHelper.CreatePayData(orderInfo[1].ToString(), Convert.ToInt32(orderInfo[2].ToString()), payInfo.id_price_level.ToString(), payInfo.name, payInfo.price_cn, 1);
                platform.DoPay(data);
                callback?.Invoke();
                //ActivityManager.Instance.RequestUpdateActivityById(2099);
            });
        }else {
            var data = SdkHelper.CreatePayData("", 0, payInfo.id.ToString(), payInfo.name, payInfo.price_cn, 1);
            data.funcStr = "buyCapsuleGift";
            data.paramStr = Json.ToJsonString(platform.GetChannel(), payInfo.price_cn * 100, User.Server.index, payInfo.id);
            platform.DoPay(data);
            callback?.Invoke();
        }
    }
    //立即完成
    public void FinishNow(int tid , Action<Act2099CultureTable> callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2099FinishNow>("capsuleDoneNow",
            Json.ToJsonString(tid),
            data =>
            {
                for (int i = 0; i < _cultureTablesInfo.Count; i++)
                {
                    if (_cultureTablesInfo[i].cultivar_id == tid)
                    {
                        _cultureTablesInfo[i] = data.cultivar_info;
                    }
                }
                Uinfo.Instance.AddItem(data.cost,false);
                callback?.Invoke(data.cultivar_info);
            });
    }
    //培育胶囊
    public void StartCultureCapsule(int tid, int cid, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2099StartCulture>("cultivarCapsule",
            Json.ToJsonString(tid, cid),
            data =>
            {
                Uinfo.Instance.AddItem(data.cost, false);
                _cultureTablesInfo = data.cultivar_info;
                callback?.Invoke();
            });
    }
}
//培养台信息
public class Act2099CultureTable
{
    public int cultivar_id;//培养台id
    public int uid;
    public int unlock_value;//是否解锁
    public int capsule_id;//培养的胶囊id 没有时为0
    public long end_ts;//培养截止时间
}
//胶囊兑换信息
public class Act2099ExchangeCapsule
{
    public int id;//胶囊id
    public int exchange_num;//剩余数量

}
//礼包信息
public class Act2099GiftInfo
{

    //public int id_price_level;
    //public int value_oversea;
    //public int purchase_limit;
    //public long price_us;
    //public string contents;
    //public int exchange_num;
    //public string name;
    //public int price_cn;
    //public int step;

    //public int pay_type;
    public int id;//礼包id
    public int exchange_num;//剩余购买次数
    //public int uid;
    //public int aid;
}
//任务信息
public class Act2099TaskInfo
{
    public int tid;//任务id
    public int do_number;
    public int finished;//是否完成
    public int get_reward;//是否领取
}

public class Act2099FinishNow
{
    public string cost;
    public Act2099CultureTable cultivar_info;
}

public class Act2099UnlockTable
{
    public string cost;
    public Act2099CultureTable cultivar_info;
}
public class Act2099GetReward
{
    public string get_item;
    public Act2099CultureTable cultivar_info;
}
public class Act2099Exchange
{
    public string cost;
    public string get_item;
    public List< Act2099ExchangeCapsule> exchange_capsule;
}
public class Act2099FinishTask
{
    public string get_reward;
    public Act2099TaskInfo mission;
}

public class Act2099StartCulture
{
    public string cost;
    public List<Act2099CultureTable> cultivar_info;
}