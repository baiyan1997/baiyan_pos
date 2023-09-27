using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2409: ActivityInfo
{
    public List<ActInfo_2409_Data> _actData;//0，普通；1，付费

    public string totalCharge;

    private bool _IsAvaliable;
    
    public override void InitUnique()
    {
        _IsAvaliable = false;
        _actData = JsonMapper.ToObject<List<ActInfo_2409_Data>>(_data.avalue["act_2409_data"].ToString());
        _IsAvaliable = isNeedRedPoint();
    }
    public bool isNeedRedPoint()
    {
        int allNum = Cfg.MidAutumnAct.getPassDataLength();
        allNum = allNum == 0 ? 7 : allNum;
        for (int i = 0; i < _actData.Count; i++)
        {
            var data = _actData[i];
            if (data != null && data.aid != 0) {
                var number = data.do_number > allNum ? allNum : data.do_number;
                if (number > data.get_reward) {
                    return true;
                }
            }
        }

        return false;
    }
    
    public override bool IsAvaliable()
    {
        return IsDuration() && _IsAvaliable;
    }

    public int GetCurNeedGetReward()
    {
        int allNum = Cfg.MidAutumnAct.getPassDataLength();
        allNum = allNum == 0 ? 6 : allNum - 1;
        if (_actData != null) {
            int n = 0;
            if (_actData[0].aid != 0) {
                var number = _actData[0].do_number > allNum ? allNum : _actData[0].do_number;
                if (number > _actData[0].get_reward) {
                    n = number > 1 ? number - 1 : 0;
                }
                else if (_actData[1].aid != 0 && number > _actData[1].get_reward) {
                    n = number > 1 ? number - 1 : 0;
                }
            }
            return n > allNum ? allNum : n;
        }
        return 0;
    }

    public bool GetIsBuy()
    {
        if (_actData != null) {
            if (_actData[1].aid != 0) {
                return true;
            }
        }
        return false;
    }

    public bool GetOpenDay(int day)
    {
        if (_actData != null) {
            if (_actData[0].aid != 0) {
                return _actData[0].do_number >= day;
            }
        }
        return false;
    }
    public int GetIconType(int day, int type)
    {
        int itype = type == 1 ? 2 : 0;
        if (_actData != null && _actData[type].aid != 0) {
            var v = _actData[type];
            if (GetOpenDay(day)) {
                bool isGet = false;
                string[] dayArr = v.data.Split(",");
                for(int i = 0; i < dayArr.Length; ++i) {
                    if (dayArr[i] != "" && day == Int32.Parse(dayArr[i])) {
                        itype = 1;
                        isGet = true;
                        break;
                    }
                }
                if (!isGet) {
                    itype = 3;
                }
            }
            if (type == 1 && itype == 2) {
                itype = 0;
            }
        }
        return itype;
    }

    public void GetReward(int id, Action callBack = null)
    {
        Rpc.SendWithTouchBlocking<ActInfo_2409_RewardData>("getAct2409Reward", Json.ToJsonString(id), data =>
        {
            string rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(rewardsStr);
            ActivityManager.Instance.RequestUpdateActivityById(_aid);//更新活动信息
            if (callBack != null)
                callBack();
        });
    }

    public void OnPayGift(int payid, Action callBack = null)
    {
        cfg_payment cfg = Cfg.Payment.GetData(payid);
        var price = float.Parse(cfg.price_cn);
        var platform = PlatformSdk.GetInstance();
        if(platform.isPayNeedOrderId()) {
            Rpc.SendWithTouchBlocking("buyAct2409",
            Json.ToJsonString(platform.GetChannel(), price * 100, User.Server.index, cfg.id),
            orderInfo =>
            {
                if ((int)orderInfo[0] == 10) //登录态失效，需要重新刷新
                {
                    platform.Jscode2session();
                    return;
                }
                var data = SdkHelper.CreatePayData(orderInfo[1].ToString(), Convert.ToInt32(orderInfo[2].ToString()), cfg.id_price_level.ToString(), cfg.name, price, 1);
                platform.DoPay(data);
                callBack?.Invoke();
                //ActivityManager.Instance.RequestUpdateActivityById(2099);
            });
        }else {
            var data = SdkHelper.CreatePayData("", 0, cfg.id.ToString(), cfg.name, price, 1);
            data.funcStr = "buyAct2409";
            data.paramStr = Json.ToJsonString(platform.GetChannel(), price * 100, User.Server.index, cfg.id);
            platform.DoPay(data);
            callBack?.Invoke();
        }
    }
    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }
    
    public override void RefreshByEnd()
    {
        if(_data == null)
        {
            return;
        }

        if(!UpdateManager.Instance.ContainEvent(_data.endts + 2, RefreshActData))
        {
            UpdateManager.Instance.AddEvent(_data.endts + 2, RefreshActData);
        }
    }
}

public class ActInfo_2409_RewardData
{
    public int type;
    public List<P_Item3> get_items;
    public string new_data;
}

public class ActInfo_2409_Data
{
    public int uid;
    public int aid;
    public int tid;
    public int start_ts;
    public int end_ts;
    public int do_number;
    public int get_reward;
    public string data;
}