using System;
using LitJson;
using System.Collections.Generic;

public class ActInfo_2302 : ActivityInfo
{
    private P_Item _rareReward;//稀有奖励
    private P_Item[] _normalRewardList;//普通奖励
    private int _integral;//积分
    private int _lotteryOneConsumption;//单次花费
    private int _lotteryTenConsumption;//十次花费
    private int _maxIntegral;//保底积分
    public override void InitUnique()
    {
        //获取奖励和抽奖价格、积分 
        _integral = Int32.Parse(_data.avalue["num"].ToString());
        List<server_act_2302_data> tempDate =
            JsonMapper.ToObject<List<server_act_2302_data>>(_data.avalue["cfg_act_2302_data"].ToString());
        List<server_act_2032_reward_config> tempReward =
            JsonMapper.ToObject<List<server_act_2032_reward_config>>(_data.avalue["cfg_act_2302_Reward"].ToString());
        DealData(tempDate, tempReward);
    }
    private void DealData(List<server_act_2302_data> tempDate, List<server_act_2032_reward_config> tempReward)
    {
        //处理抽奖信息
        for (int i = 0; i < tempDate.Count; i++)
        {
            var t = tempDate[i];
            string[] tempItem = t.value.Split('|');
            if (t.type == 1 && t.further == 1)
            {
                _lotteryOneConsumption = Int32.Parse(tempItem[tempItem.Length - 1]);
            }
            else if (t.type == 1 && t.further == 10)
            {
                _lotteryTenConsumption = Int32.Parse(tempItem[tempItem.Length - 1]);
            }
            else
            {
                _maxIntegral = Int32.Parse(tempItem[tempItem.Length - 1]);
            }
        }
        //处理奖励信息
        for (int i = 0; i < tempReward.Count; i++)
        {
            var t = tempReward[i];
            if (t.type == 1)
            {
                P_Item reward = new P_Item(t.reward);
                _rareReward = reward;
            }
            else
            {
                P_Item[] rewards = GlobalUtils.ParseItem(t.reward);
                _normalRewardList = rewards;
            }
        }
    }
    //开始抽卡
    public void StartLottery(int type, Action<string> callback)
    {
        Rpc.SendWithTouchBlocking<Act2095Result>("lotteryDraw2302", Json.ToJsonString(type), data =>
        {
            Uinfo.Instance.AddAndReduceItem(data.get_item, data.cost_item);
            _integral = data.num;
            callback?.Invoke(data.get_item);
        });
    }
    //获得稀有奖励
    public P_Item GetRareReward()
    {
        return _rareReward;
    }
    //获得常规奖励
    public P_Item[] GetNormalRewards()
    {
        return _normalRewardList;
    }
    //获得单抽消耗
    public int GetLotteryOnceConsumption()
    {
        return _lotteryOneConsumption;
    }
    //获得十连抽消耗
    public int GetLotteryTenTimesConsumption()
    {
        return _lotteryTenConsumption;
    }
    //获得保底积分
    public int GetMaxIntegral()
    {
        return _maxIntegral;
    }

    //获得当前积分
    public int GetIntegral()
    {
        return _integral;
    }
    //找到抽奖时奖励对应的位置
    public List<int> GetIndexOfReward(string reward)
    {
        P_Item[] items = GlobalUtils.ParseItem(reward);

        if (items.Length <= 0)
            return null;

        List<int> list = new List<int>();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].id == _rareReward.id && items[i].count == _rareReward.count)
            {
                list.Add(0);
            }
            else
            {
                for (int j = 0; j < _normalRewardList.Length; j++)
                {
                    if (items[i].id == _normalRewardList[j].id && items[i].count == _normalRewardList[j].count)
                    {
                        list.Add(j + 1);
                    }
                }
            }
        }
        return list;
    }
}

public class server_act_2302_data
{
    public int type;
    public int further;
    public string value;
    public string desc;
}

public class server_act_2032_reward_config
{
    public int id;
    public int step;
    public string reward;
    public int type;
}
public class Act2095Result
{
    public string cost_item;
    public string get_item;
    public int num;
}