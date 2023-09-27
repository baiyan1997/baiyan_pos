using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ActInfo_2096 : ActivityInfo
{
    public int Progress { private set; get; }

    public int Sum { private set; get; }

    public long BoostTime { private set; get; }

    public long GiftTime { private set; get; }

    public bool IsBuy { private set; get; }

    public int GiftId { private set; get; }

    public P_Pay PayInfo { private set; get; }

    public P_Item[] SpecialItemList { private set; get; }

    public List<P_Act2096Mission> MissionList { private set; get; }

    private Dictionary<int, List<P_Item>> _dict;
    private int _day;
    private int _step;

    public override bool OnInited()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.ACT2096RECHARGE, _EventACT2096RECHARGE);

        return true;
    }
    public override void OnRemove()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.ACT2096RECHARGE, _EventACT2096RECHARGE);
    }

    private void _EventACT2096RECHARGE(int opcode, string data)
    {
        Uinfo.Instance.AddItemAndShow(data);

        IsBuy = true;

        EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
    }

    public override void InitUnique()
    {
        base.InitUnique();

        if (!_data.IsDuration())
            return;
        object avaldata;
        if (_data.avalue.TryGetValue("gift_end_ts", out avaldata))
            GiftTime = Convert.ToInt32(avaldata.ToString());

        if (_data.avalue.TryGetValue("next_share_ts", out avaldata))
            BoostTime = Convert.ToInt32(avaldata.ToString());

        if (_data.avalue.TryGetValue("boost_value", out avaldata))
            Progress = Convert.ToInt32(avaldata.ToString());

        if (_data.avalue.TryGetValue("day", out avaldata))
            _day = Convert.ToInt32(avaldata.ToString());

        if (_data.avalue.TryGetValue("step", out avaldata))
            _step = Convert.ToInt32(avaldata.ToString());

        if (_data.avalue.TryGetValue("is_buy", out avaldata))
            IsBuy = Convert.ToInt32(avaldata.ToString()) == 1;

        Sum = Cfg.Activity2096.GetMaxValue();

        cfg_act_2096_reward rewardcfg = Cfg.Activity2096.GetRewardData(_day, _step);
        if (_data.avalue.TryGetValue("boost_task_info", out avaldata))
            MissionList = JsonMapper.ToObject<List<P_Act2096Mission>>(avaldata.ToString());
        else
            MissionList = new List<P_Act2096Mission>();

        _dict = new Dictionary<int, List<P_Item>>();

        if (rewardcfg != null && !string.IsNullOrEmpty(rewardcfg.boost_reward))
        {
            string[] str = rewardcfg.boost_reward.Split(',');

            for (int i = 0; i < str.Length; i++)
            {
                string ss = str[i];
                string[] item = ss.Split('|');
                int tid = int.Parse(item[0]);
                int itemid = int.Parse(item[1]);
                int count = int.Parse(item[2]);
                List<P_Item> temp = null;
                if (_dict.TryGetValue(tid, out temp))
                {
                    temp.Add(new P_Item(itemid, count));
                }
                else
                {
                    List<P_Item> itemlist = new List<P_Item>();
                    itemlist.Add(new P_Item(itemid, count));
                    _dict.Add(tid, itemlist);
                }
            }
        }

        cfg_act_2096_gift_pack cfggift = Cfg.Activity2096.GetGiftData(_day, _step);

        if (cfggift != null)
        {
            if (!string.IsNullOrEmpty(cfggift.goods))
            {
                SpecialItemList = GlobalUtils.ParseItem(cfggift.goods);
            }
            GiftId = cfggift.id;
            var cfg = Cfg.Payment.GetData(cfggift.payment_id);
            PayInfo = new P_Pay
            {
                _id = cfg.id,
                _name = cfg.name,
                _price = PayConfig.GetPrice(cfg.id),
                _qua = cfg.qua,
                _desc = cfg.desc,
            };
        }
    }

    public override bool IsAvaliable()
    {
        int day = PlayerPrefs.GetInt(User.Uid + "Act2096_day", 0);
        if (day != _day)
            return true;

        for (int i = 0; i < MissionList.Count; i++)
        {
            P_Act2096Mission reward = MissionList[i];
            if (reward.finished == 1 && reward.get_reward == 0)
                return true;
        }
        return false;
    }

    public void UpdateToday()
    {
        PlayerPrefs.SetInt(User.Uid + "Act2096_day", _day);
        EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
    }

    public List<P_Item> GetMissionRewards(int tid)
    {
        List<P_Item> temp = null;
        _dict.TryGetValue(tid, out temp);
        // return _dict[tid];
        return temp;
    }

    public bool IsBoostEffective(long time)
    {
        int curDay = (int)(time - _data.startts) / 86400 + 1;
        return curDay == _day;
    }

    public void BuyGift()
    {
        /*if (PlatformWrap.GetPlatformName() == "local")
        {
            return;
        }*/
        if (PayInfo == null)
        {
            Debug.LogError("配置PayInfo丢失");
            return;
        }
        _AlertYesNo a = Alert.YesNo(Lang.Get("是否前往充值{0}购买礼包", PayConfig.GetPrice(PayInfo._id)));
        a.SetYesCallback(() =>
        {
            var platform = PlatformSdk.GetInstance();
            int priceLevel = Cfg.Payment.GetPriceLevel(PayInfo._id);
            if(platform.isPayNeedOrderId()) {
                Rpc.SendWithTouchBlocking("buyAct2096GiftPack",
                Json.ToJsonString(platform.GetChannel(), PayInfo.GetPriceNum() * 100, User.Server.index, 1, PayInfo._id, GiftId),
                orderInfo =>
                {
                    if ((int)orderInfo[0] == 10) //登录态失效，需要重新刷新
                    {
                        platform.Jscode2session();
                        return;
                    }
                    var data = SdkHelper.CreatePayData(orderInfo[1].ToString(), Convert.ToInt32(orderInfo[2].ToString()), priceLevel.ToString(), PayInfo.GetName(), PayInfo.GetPriceNum(), 1);
                    platform.DoPay(data);
                    a.Close();
                });
            }else {
                var data = SdkHelper.CreatePayData("", 0, PayInfo._id.ToString(), PayInfo.GetName(), PayInfo.GetPriceNum(), 1);
                data.funcStr = "buyAct2096GiftPack";
                data.paramStr = Json.ToJsonString(platform.GetChannel(), PayInfo.GetPriceNum() * 100, User.Server.index, 1, PayInfo._id, GiftId);
                platform.DoPay(data);
                a.Close();
            }
        });

        a.SetNoCallback(a.Close);
    }


    //发起助力
    public void ShareBoost(Action callback = null)
    {
        if (PayInfo == null)
        {
            Debug.LogError("配置PayInfo丢失");
            return;
        }
        Rpc.SendWithTouchBlocking("shareBoostInfo", Json.ToJsonString(User.Uid), data =>
        {
            BoostTime = (long)data[1];

            if (callback != null)
                callback();

            MessageManager.Show(Lang.Get("分享助力成功"));
        });
    }

    //助力
    public void SendBoost(int uid, long time)
    {
        if (PayInfo == null)
        {
            Debug.LogError("配置PayInfo丢失");
            return;
        }
        Rpc.SendWithTouchBlocking("act2096Boost", Json.ToJsonString(uid, time), data =>
         {
             if ((int)data[0] == 1)
             {
                 MessageManager.Show(Lang.Get("感谢您的助力~"));
             }
             else
             {
                 var alert = Alert.Ok(Lang.TranslateJsonString(data[1].ToString()));
             }
         });
    }


    //领取奖励
    public void RequestMissionReward(int tid, Action<P_Act2096Mission> callback = null)
    {
        if (PayInfo == null)
        {
            Debug.LogError("配置PayInfo丢失");
            return;
        }
        Rpc.SendWithTouchBlocking<P_Act2096GetRewardData>("getAct2096BoostReward", Json.ToJsonString(tid), data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_items);

            P_Act2096Mission mission = null;
            for (int i = 0; i < MissionList.Count; i++)
            {
                P_Act2096Mission m = MissionList[i];
                if (m.tid == tid)
                {
                    m.get_reward = 1;
                    mission = m;
                }
            }

            if (callback != null)
                callback(mission);

            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
}

public class P_Act2096Mission
{
    public int tid;
    public int get_reward;
    public int finished;
}

public class P_Act2096GetRewardData
{
    public string get_items;
}
