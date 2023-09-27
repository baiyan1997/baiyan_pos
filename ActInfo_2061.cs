using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2061 : ActivityInfo
{
    private int _day;
    public int Day
    {
        get { return _day; }
    }

    private int _canGetReward;

    public List<int> dayGetList;

    public List<P_Act2061Item> itemList = new List<P_Act2061Item>();

    public override void InitUnique()
    {
        _canGetReward = Convert.ToInt32(_data.avalue["can_get_reward"]);//是否有奖励未领取
        _day = Convert.ToInt32(_data.avalue["do_number"]);
        string s = _data.avalue["data"].ToString();

        List<int> list = new List<int>();
        if (!string.IsNullOrEmpty(s))
        {
            string[] ss = s.Split(',');
            for (int i = 0; i < ss.Length; i++)
            {
                string str = ss[i];
                list.Add(int.Parse(str));
            }
        }
        dayGetList = list;
        RefreshInfo(JsonMapper.ToObject<List<P_Act2061RewardData>>(_data.avalue["cfg_data"].ToString()));
    }

    private void RefreshInfo(List<P_Act2061RewardData> rewards)
    {
        itemList.Clear();

        for (int i = 0; i < rewards.Count; i++)
        {
            P_Act2061Item itemdata = new P_Act2061Item();

            itemdata.rewards = GlobalUtils.ParseItem3(rewards[i].reward);
            itemdata.dayIndex = rewards[i].mission_finish;

            if (_day >= rewards[i].mission_finish)
            {
                if (dayGetList.Contains(rewards[i].mission_finish))
                {
                    itemdata.statu = 2;
                }
                else
                {
                    itemdata.statu = 0; //可领
                }
            }
            else
            {
                itemdata.statu = 1;//未达成
            }
            itemList.Add(itemdata);
        }

        //itemList.Sort((a, b) =>
        //{
        //    if (a.statu != b.statu)
        //    {
        //        return a.statu - b.statu;
        //    }
        //    else
        //    {
        //        return a.dayIndex - b.dayIndex;
        //    }
        //});
        itemList.Sort(Sort_act2061);
    }
    private int Sort_act2061(P_Act2061Item a, P_Act2061Item b)
    {
        if (a.statu != b.statu)
        {
            return a.statu - b.statu;
        }
        else
        {
            return a.dayIndex - b.dayIndex;
        }
    }

    public override bool IsAvaliable()
    {
        return _canGetReward == 1;
    }

    public void GetRewardById(int day, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2061Reward", Json.ToJsonString(day), data =>
        {
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(data.get_items);

            if (callback != null)
                callback();

            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());

            ActivityManager.Instance.RequestUpdateActivityById(2061);
        });
    }
}

public class P_Act2061RewardData
{
    public string reward;
    public int id;
    public int mission_finish;
}

public class P_Act2061Item
{
    public int dayIndex;
    public int statu;
    public P_Item3[] rewards;
}

