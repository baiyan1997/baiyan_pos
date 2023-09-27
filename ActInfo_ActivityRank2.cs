using System;
using System.Collections.Generic;
using LitJson;
using System.Linq;

public class ActInfo_ActivityRank2 : ActivityInfo
{
    public P_ActRank2DataList _data2029 { private set; get; }

    public List<P_RewardData> _rewardData;

    public int _rewardLv;//奖励档位
    public long freshts;

    public override void InitUnique()
    {
        if (_data2029 == null)
        {
            _data2029 = new P_ActRank2DataList();
            {
                _data2029.rank_list = new List<P_ActRank2Item>();
            }
        }
        //获取列表项
        List<P_ActRank2Item> _list = JsonMapper.ToObject<List<P_ActRank2Item>>(_data.avalue["rank_list"].ToString());
        P_ActRank2Item _item = new P_ActRank2Item();
        _item.rank = Convert.ToInt32(_data.avalue["user_rank"]);
        _item.upower_history = Convert.ToInt32(_data.avalue["upower_history"]);
        _item.get_reward = _data.avalue["get_reward"].ToString();

        _rewardData = JsonMapper.ToObject<List<P_RewardData>>(_data.avalue["cfg_data"].ToString());
        for (int i = 0; i < _rewardData.Count; i++)
        {
            var data = _rewardData[i];
            data.rewards = GlobalUtils.ParseItem3(data.reward);
        }
        freshts = Convert.ToInt64(_data.avalue["freshts"]);
        RefreshInfo(_list, _item);
    }
    private void RefreshInfo(List<P_ActRank2Item> itemList, P_ActRank2Item item)
    {
        _data2029.rank_list.Clear();
        for (int i = 0; i < itemList.Count; i++)
        {
            _data2029.rank_list.Add(itemList[i]);
        }
        _data2029.my_rank = item;
        //设置已领
        string[] gets = _data2029.my_rank.get_reward.Split(',');
        for (int i = 0; i < _rewardData.Count; i++)
        {
            if (gets.Contains(_rewardData[i].id.ToString()))
                _data2029.my_rank.hasGet[_rewardData[i].id] = true;
            else
                _data2029.my_rank.hasGet[_rewardData[i].id] = false;
        }
        _rewardLv = GetRankLv(_data2029.my_rank.rank);
        if (freshts != 0 && freshts > TimeManager.ServerTimestamp && !UpdateManager.Instance.ContainEvent(freshts, RefreshAct))
        {
            UpdateManager.Instance.AddEvent(freshts, RefreshAct);
        }
    }
    public bool isOpen()
    {
        if (TimeManager.ServerTimestamp >= freshts)
        {
            return true;
        }
        return false;
    }
    public void RefreshAct()
    {
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }
    public int GetRankLv(int rank)
    {
        for (int i=0;i< _rewardData.Count;i++)
        {
            var reward = _rewardData[i];
            if (rank >= reward.min_rank && rank <= reward.max_rank)
                return reward.id;
        }
        return 0;
    }
    public void GetAct2029Reward(int id, Action callback)
    {
        if (_aid == 2029)
        {
            Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2029Reward", Json.ToJsonString(id, _aid), data =>
            {
                var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
                Uinfo.Instance.AddItem(rewardsStr, true);
                MessageManager.ShowRewards(data.get_items);
                if (callback != null)
                    callback();
                EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
            });
        }
        else if (_aid == 2050)
        {
            Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2050Reward", Json.ToJsonString(id, _aid), data =>
            {
                var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
                Uinfo.Instance.AddItem(rewardsStr, true);
                MessageManager.ShowRewards(data.get_items);
                if (callback != null)
                    callback();
                EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
            });
        }
    }

    public override bool IsAvaliable()
    {
        if (isOpen())
        {
            foreach (var kv in _data2029.my_rank.hasGet)
            {
                if (!kv.Value && kv.Key >= _rewardLv && _rewardLv != 0)//没领 && 可领 && 有奖励
                    return true;
            }
            return false;
        }
        return false;
    }
}

//活动显示玩家表数据
public class P_ActRank2DataList
{
    public P_ActRank2Item my_rank;
    public List<P_ActRank2Item> rank_list;
}
public class P_ActRank2Item
{
    public int uid;
    public string uname;
    public int rank;
    public int upower_history;
    public int ustate;

    public Dictionary<int, bool> hasGet = new Dictionary<int, bool>(4);
    public string get_reward;
}
