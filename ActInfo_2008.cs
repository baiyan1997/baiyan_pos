using System;
using System.Collections.Generic;
using LitJson;
public class ActInfo_2008 : ActivityInfo
{
    public int open = 0;
    public Dictionary<int, int> reward_list;
    public string get_reward = "";

    public Dictionary<int, Act2008_rewardData> cfg_data;
    public Act2008_statuData avalue_data;

    public override void InitUnique()
    {
        base.InitUnique();
        avalue_data = JsonMapper.ToObject<Act2008_statuData>(_data.avalue["data"].ToString());
        open = avalue_data.open;
        get_reward = avalue_data.get_reward;
        Dictionary<string, Act2008_rewardData> cfgData = JsonMapper.ToObject<Dictionary<string, Dictionary<string, Act2008_rewardData>>>(_data.avalue["cfg_data"].ToString())["cfg_data"];
        cfg_data = new Dictionary<int, Act2008_rewardData>();
        string[] a = new string[cfgData.Count];

        foreach (KeyValuePair<string, Act2008_rewardData> kp in cfgData)
        {
            a[int.Parse(kp.Key) - 1] = kp.Key;
        }
        for (int index = 0; index < a.Length; index++)
        {
            string i = a[index];
            Act2008_rewardData value = cfgData[i];
            cfg_data.Add(value.id, value);
        }


        reward_list = new Dictionary<int, int>();
        foreach (KeyValuePair<int, Act2008_rewardData> kp in cfg_data)
        {
            reward_list.Add(kp.Key, 0);
        }
        if (string.IsNullOrEmpty(get_reward) == false)
        {
            string[] rewardIds = get_reward.Split(',');
            for (int i = 0; i < rewardIds.Length; i++)
            {
                reward_list[int.Parse(rewardIds[i])] = 1;
            }
        }
    }

    public override bool NeedDailyRemind()
    {
        //已购买就不需要每日提醒
        if (open == 1)
            return false;

        long startday = GetTotalDaysFromTimeStamp(_data.startts);
        long curday = GetTotalDaysFromTimeStamp(TimeManager.ServerTimestamp);
        return curday - startday <= 2; //前3天增加每日小红点提醒
    }

    //从时间戳,获取在当前时区,累计过了几天
    private long GetTotalDaysFromTimeStamp(long ts)
    {
        return (ts + 3600 * TimeManager.svcTimeZone) / 86400;
    }


    public override bool IfRefreshOnPush(int opcode)
    {
        return opcode == OpcodePush.Recharge;
    }

    public override bool IsAvaliable()
    {
        int getreward = 0;
        object value = null;
        if (_data.avalue.TryGetValue("get_reward", out value))
        {
            getreward = (int)value;
        }

        bool isA = getreward == 0 && avalue_data.do_number >= 7;

        if (isA == false && open == 1)
        {
            int playerLv = Uinfo.Instance.Player.Info.ulevel;
            foreach (KeyValuePair<int, Act2008_rewardData> kp in cfg_data)
            {
                Act2008_rewardData lvData = kp.Value;
                int lv = lvData.need_level;
                int id = lvData.id;
                if (playerLv >= lv && (reward_list[id] != 1))
                {
                    isA = true;
                    break;
                }
            }
        }
        return isA;
    }
    public void updateAvalue()
    {
        EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
    }
    public void ActiveFund2008(Action callback = null)
    {
        int shopLv = VipShopInfo.Instance.GetShopInfo().shop_level;
        if (shopLv >= 4)
        {
            if (ItemHelper.IsCountEnough(ItemId.Gold, 1000))
            {
                Rpc.Send<int>("openTheAct", null, data =>
                {
                    open = 1;
                    Uinfo.Instance.AddItem(String.Format("10|{0}", data), false, false);
                    MessageManager.Show(String.Format(Lang.Get("成长基金已开启")));
                    updateAvalue();
                    if (callback != null)
                        callback();
                });
            }
        }
        else
        {
            var d = Alert.Ok(Lang.Get("您的VIP等级不到4级!\n是否前往充值?"));
            d.SetButtonText(Lang.Get("前往充值"));
            //d.SetCallback(() =>
            //{
            //    DialogManager.ShowAsyn<_D_Recharge>(_d => { _d?.OnShow(0); });
            //});
            d.SetCallback(OnAlertOKCB_4);
        }
    }
    private void OnAlertOKCB_4()
    {
        // DialogManager.ShowAsyn<_D_Recharge>(_d => { _d?.OnShow(0); });
        DialogManager.ShowAsyn<_D_Recharge>(OnAlertOKDialogShowCB);
    }

    private void OnAlertOKDialogShowCB(_D_Recharge _d)
    {
        _d?.OnShow(0);
    }

    public void GetReward2008(int id, Action callback)
    {
        Rpc.Send<Dictionary<string, P_getAct2008Reward_item[]>>("getAct2008Reward", Json.ToJsonString(id), data =>
        {//getAct2008Reward - [1,{"get_items":[{"itemid":10,"count":800}]}]
            get_reward = String.Format("{0},{1}", get_reward, id);
            int temp = 0;
            if (reward_list.TryGetValue(id, out temp))
            {
                reward_list[id] = 1;
            }
            foreach (KeyValuePair<int, Act2008_rewardData> kp in cfg_data)
            {
                if (kp.Key == id)
                {
                    Uinfo.Instance.AddItem(kp.Value.reward, true, false);
                    MessageManager.ShowRewards(kp.Value.reward);
                    break;
                }
            }
            updateAvalue();
            if (callback != null)
                callback();
        });
    }
}
public class P_getAct2008Reward_item
{
    public int itemid;
    public int count;
}
public class Act2008_statuData
{
    public int open;
    public string get_reward;
    public int do_number;
}
public class Act2008_rewardData
{
    public int id;
    public int need_level;
    public string reward;
}