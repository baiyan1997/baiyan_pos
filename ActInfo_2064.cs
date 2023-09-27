using System;
using System.Collections.Generic;
using LitJson;


public class ActInfo_2064 : ActivityInfo
{
    public int Step;//服务器生产奖励时的阶段 一旦确定奖池不会变
    public int Confirm; //是否确定了奖励
    public int Unlock; //是否解锁扩充奖励
    public string NotSelectable; //不可选奖励
    public string SelectedCount; //已选奖励次数
    public List<Act2064ItemData> Reward;
    public int BossStatus;

    public bool CanClick;
    public int SelectedId;//客户端记录玩家目前选了哪个奖励

    public override void InitUnique()
    {
        Reward = JsonMapper.ToObject<List<Act2064ItemData>>(_data.avalue["reward"].ToString());
        Confirm = Convert.ToInt32(_data.avalue["confirm"].ToString());
        Unlock = Convert.ToInt32(_data.avalue["unlock"].ToString());
        NotSelectable = _data.avalue["not_selectable"].ToString();
        SelectedCount = _data.avalue["selected_count"].ToString();
        Step = Convert.ToInt32(_data.avalue["step"].ToString());
        BossStatus = Convert.ToInt32(_data.avalue["status"].ToString());
    }

    /// <summary>
    /// 初始奖池 根据id显示
    /// </summary>
    /// <returns></returns>
    public List<Act2064ItemData> GetInitialReward()
    {
        List<Act2064ItemData> ret = new List<Act2064ItemData>();
        for (int i = 0; i < Reward.Count; i++)
        {
            var item = Reward[i];
            if (item.index == 0 && item.type == 0)
            {
                ret.Add(item);
            }
        }
        // ret.Sort((a, b) => a.id - b.id);
        ret.Sort(Sort_act2046_id);
        return ret;
    }
    private int Sort_act2046_id(Act2064ItemData a, Act2064ItemData b)
    {
        return a.id - b.id;
    }
    public List<int> GetNotSelectableIds()
    {
        List<int> ret = new List<int>();
        if (string.IsNullOrEmpty(NotSelectable))
            return ret;
        var ids = NotSelectable.Split(',');
        for (int i = 0; i < ids.Length; i++)
        {
            ret.Add(int.Parse(ids[i]));
        }
        return ret;
    }

    public int GetSelectCount(int id)
    {
        if (string.IsNullOrEmpty(SelectedCount))
            return 0;
        var str = SelectedCount.Split(',');
        for (int i = 0; i < str.Length; i++)
        {
            int itemId = int.Parse(str[i].Split('|')[0]);
            int count = int.Parse(str[i].Split('|')[1]);
            if (itemId == id)
                return count;
        }

        return 0;
    }

    /// <summary>
    /// 可抽取奖池 由服务器index决定
    /// </summary>
    /// <returns></returns>
    public List<Act2064ItemData> GetConfirmReward()
    {
        List<Act2064ItemData> ret = new List<Act2064ItemData>();
        for (int i = 0; i < Reward.Count; i++)
        {
            ret.Add(Reward[i]);
        }
        // ret.Sort((a, b) => a.index - b.index);
        ret.Sort(Sort_act2046_index);
        return ret;
    }
    private int Sort_act2046_index(Act2064ItemData a, Act2064ItemData b)
    {
        return a.index - b.index;
    }

    public Act2064ItemData GetInfoByIndex(int index)
    {
        for (int i = 0; i < Reward.Count; i++)
        {
            if (Reward[i].index == index)
                return Reward[i];
        }
        return null;
    }


    //选择奖励
    public void ConfirmReward(int id, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2064Info>("confirmReward", Json.ToJsonString(id), data =>
        {
            Confirm = data.confirm;
            Unlock = data.unlock;
            Reward = data.reward;
            SelectedCount = data.selected_count;
            NotSelectable = data.not_selectable;
            if (callback != null)
                callback();
        });
    }

    //一次性扩充奖池的操作
    public void ExpandReward(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<int>("expandReward", null, data =>
        {
            Unlock = 1;
            Uinfo.Instance.Player.AddGold(-data);
            if (callback != null)
                callback();
        });
    }

    public void RefreshReward(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2064Info>("rewardRefresh", null, data =>
        {
            SelectedId = 0;
            Confirm = data.confirm;
            Unlock = data.unlock;
            NotSelectable = data.not_selectable;
            Reward = data.reward;
            if (callback != null)
                callback();
        });
    }

    public void GetReward(int index, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<Act2064ItemData>("get2064Reward", Json.ToJsonString(index), data =>
        {
            BagInfo.Instance.AddItem(ItemId.Act2064Point, -100);
            for (int i = 0; i < Reward.Count; i++)
            {
                var info = Reward[i];
                if (info.index == index)
                {
                    Reward.Remove(info);
                    Reward.Add(data);
                    var reward = Cfg.Activity2064.GetData(data.id).item;
                    Uinfo.Instance.AddItem(reward, true);
                    MessageManager.ShowRewards(reward);
                    break;
                }
            }
            if (callback != null)
            {
                callback();
            }
        });
    }
}

public class P_Act2064Info
{
    public List<Act2064ItemData> reward;
    public int confirm;
    public int unlock;
    public string not_selectable;
    public string selected_count;
    public int status;
}

public class Act2064ItemData
{
    public int index; //奖池位置
    public int id;  //cfg_act_2064->id
    public int get_reward; //0-未领 1-领取
    public int type; //0-固定 1-可选 2-需要氪金解锁
}

public class P_NYPirateLegion
{
    public int step;
    public int status;  //0-存活 1-死亡
    public int hp;
    public int hp_max;
    public int can_attack; //可攻打次数
}