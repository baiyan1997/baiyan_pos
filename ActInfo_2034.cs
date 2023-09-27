using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2034 : ActivityInfo
{
    public List<P_Act2034Data> _missionInfo { private set; get; }
    public override void InitUnique()
    {
        if (_data.avalue == null)
        {
            throw new Exception("ActInfo_2034 info avalue should not be null");
        }

        object infoObj;
        _data.avalue.TryGetValue("mission_info", out infoObj);
        if (infoObj == null)
        {
            throw new Exception("ActInfo_2034 info avalue[mission_info] should not be null");
        }

        _missionInfo = JsonMapper.ToObject<List<P_Act2034Data>>(infoObj.ToString());
        if (_missionInfo.Count < 1)
        {
            throw new Exception("ActInfo_2034 info avalue[mission_info] Count should not >= 1");
        }
    }



    public override bool IsAvaliable()
    {
        if (_data.can_get_reward)
            return true;

        return false;
    }

    public void GetReward(Action<P_ActCommonReward> callback)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2034Reward", null, data =>
        {
            if (data.get_items.Count < 1)
                throw new Exception("getAct2034Reward get_items count should >0");

            string get = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(get, true, true);
            _data.can_get_reward = false;
            _data.get_all_reward = true;
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
            EventCenter.Instance.RemindActivity.Broadcast(_data.aid,IsAvaliable());
            if (callback != null)
                callback(data);
        });
    }
}

public class P_Act2034Data
{
    public int finished;
    public int do_number;
    public int tid;
}