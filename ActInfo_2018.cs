using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;

public class ActInfo_2018 : ActivityInfo
{
    public P_Act2018Data _data2018 { get; private set; }
    public List<int> _canGotId;
    public Dictionary<int, int> _haveNum;

    public override void InitUnique()
    {
        //获得活动项和可领取id
        _data2018 = JsonMapper.ToObject<P_Act2018Data>(_data.avalue["cfg_data"].ToString());
        _canGotId = JsonMapper.ToObject<P_Act2018ItemState>(_data.avalue["data"].ToString()).Parse();
        _haveNum = new Dictionary<int, int>();
        //获得某等级已到达人数
        foreach (var item in _data2018.cfg_data.Values)
        {
            _haveNum.Add(item.vip_level, (int)_data.avalue[item.vip_level + ""]);
        }
    }

    //获得某一等级的人数
    public int GetCertainLvNum(int lv)
    {
        int rest = 0;
        _haveNum.TryGetValue(lv, out rest);
        return rest;
    }

    public override bool IsAvaliable()
    {
        foreach (var item in _data2018.cfg_data.Values)
        {
            if (item.state == 1)
                return true;
        }
        return false;
    }

    public void GetAct2018Reward(int id, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2018Reward", Json.ToJsonString(id), data =>
        {
            //添加道具
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(rewardsStr);
            //移除可获得奖励id
            _canGotId.Add(id);

            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
            if (callback != null)
                callback();
        });
    }
}

public class P_Act2018Data
{
    public Dictionary<string, P_Act2018Item> cfg_data;
}

public class P_Act2018Item
{
    public int vip_num;

    public int vip_havenum
    {
        get { return ((ActInfo_2018)ActivityManager.Instance.GetActivityInfo(2018)).GetCertainLvNum(vip_level); }
        set { }
    }
    public int vip_level;
    public string rewards;
    //客户端自定义奖励领取状态，0-未满足条件，1-可领取，2-已领取
    public int state
    {
        get
        {
            var act2018 = (ActInfo_2018)ActivityManager.Instance.GetActivityInfo(2018);
            if (act2018 == null || act2018._canGotId == null)
                return 0;
            return act2018._canGotId.Contains(id) ? 2 : (vip_havenum < vip_num ? 0 : 1);
        }
        set { }
    }
    public int id;
}

public class P_Act2018ItemState
{
    public string get_reward;
    public List<int> canGetId;

    public List<int> Parse()
    {
        var canGetIdStrs = get_reward.Split(',').ToList();
        canGetId = new List<int>();
        for (int i = 0; i < canGetIdStrs.Count; i++)
        {
            string str = canGetIdStrs[i];
            var num = 0;
            if (int.TryParse(str, out num))
                canGetId.Add(num);
        }
        return canGetId;
    }
}