using System;
using System.Collections.Generic;
using System.ComponentModel;
using LitJson;

public class ActInfo_2082 : ActivityInfo
{
    private P_Act2082 _info;
    public override void InitUnique()
    {
        _info = new P_Act2082();
        object temp_luck_buff_v = null;
        if (!_data.avalue.TryGetValue("luck_buff",out temp_luck_buff_v))
            throw new Exception("act 2082 avalue find no \"luck_buff\" key");
        object temp_super_fire_v = null;
        if (!_data.avalue.TryGetValue("is_super_fire",out temp_super_fire_v))
            throw new Exception("act 2082 avalue find no \"is_super_fire\" key");
        object temp_reward_info_v = null;
        if (!_data.avalue.TryGetValue("reward_info",out temp_reward_info_v))
            throw new Exception("act 2082 avalue find no \"reward_info\" key");
        object temp_exchange_info_v = null;
        if (!_data.avalue.TryGetValue("exchange_info",out temp_exchange_info_v))
            throw new Exception("act 2082 avalue find no \"exchange_info\" key");

        _info.luck_buff = Convert.ToInt32(temp_luck_buff_v);
        _info.is_super_fire = Convert.ToInt32(temp_super_fire_v);
        string reward_info = Convert.ToString(temp_reward_info_v);
        string mission_info =  Convert.ToString(_data.avalue["mission_info"]);
        string exchange_info =  Convert.ToString(temp_exchange_info_v);
        _info.rewardList = JsonMapper.ToObject<List<P_2082Reward>>(reward_info);
        _info.missionList = JsonMapper.ToObject<List<P_2082Mission>>(mission_info);
        var list = JsonMapper.ToObject<List<P_2082Exchange>>(exchange_info);
        _info.exchangeDic = new Dictionary<int, int>();
        for (int i = 0; i < list.Count; i++)
        {
            _info.exchangeDic[list[i].exchange_id] = list[i].exchange_num;
        }
    }
    //购买爆竹
    public void BuyFirecrackers(Action ac)
    {
        Rpc.SendWithTouchBlocking<P_2082Get>("buyFirecrackers",  null, data =>
        {
            ItemHelper.AddAndReduceItem(data.reward, data.cost);
            MessageManager.Show(Lang.Get("爆竹购买成功"));
            //不展示奖励因为没图标
//            MessageManager.ShowRewards(data.reward);
            EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);

            EventCenter.Instance.RemindActivity.Broadcast(_aid,IsAvaliable());

            if (ac != null)
                ac();
        });
    }
    //合成爆竹
    public void MakeFirecrackers(Action ac)
    {
        Rpc.SendWithTouchBlocking<P_2082Get>("makeFirecrackers",  null, data =>
        {
            ItemHelper.AddAndReduceItem(data.reward, data.cost);
            MessageManager.Show(Lang.Get("爆竹合成成功"));
            EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
            //不展示奖励因为没图标
//            MessageManager.ShowRewards(data.reward);
            EventCenter.Instance.RemindActivity.Broadcast(_aid,IsAvaliable());

            if (ac != null)
                ac();
        });
    }
    //购买祈福buff  //花氪金买下次概率翻倍
    public void BuyLuckBuff(Action ac)
    {
        Rpc.SendWithTouchBlocking<P_LuckBuff2082>("luckBuff",  null, data =>
        {
            ItemHelper.AddItem(data.cost,false);
            _info.luck_buff = data.buff_count;
            MessageManager.Show(Lang.Get("祈福buff购买成功"));
            EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);

            if (ac != null)
                ac();
        });
    }
    //燃放爆竹
    public void FireFirecrackers(Action<string> ac)
    {
        Rpc.SendWithTouchBlocking<P_2082Get>("fireFirecrackers",  null, data =>
        {
            if(!string.IsNullOrEmpty(data.cost))
                ItemHelper.AddItem(data.cost,false);
            if (!string.IsNullOrEmpty(data.reward))
            {
                ItemHelper.AddItem(data.reward,true);
            }

            //同步信息
            _info.luck_buff = data.luck_buff;
            _info.is_super_fire = data.is_super_fire;
            var eList = JsonMapper.ToObject<List<P_2082Exchange>>(data.exchange_info);;
            for (int i = 0; i < eList.Count; i++)
            {
                _info.exchangeDic[eList[i].exchange_id] = eList[i].exchange_num;
            }

            EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
            EventCenter.Instance.RemindActivity.Broadcast(_aid,IsAvaliable());

            if (ac != null)
                ac(data.reward);
        });
    }

    public int GetLuckBuffCount()
    {
        return _info.luck_buff;
    }
    public bool isSuperFire()
    {
        return _info.is_super_fire == 1;
    }

    public List<P_2082Mission> GetMissionList()
    {
        return _info.missionList;
    }
    public List<P_2082Reward> GetRewardList()
    {
        return _info.rewardList;
    }
    public Dictionary<int,int> GetExchangeDic()
    {
        return _info.exchangeDic;
    }
    public override bool IsAvaliable()
    {
        var hasFirecracker = BagInfo.Instance.GetItemCount(ItemId.Firecracker) > 0;
        return IsDuration() && hasFirecracker;
    }
}

public class P_Act2082
{
    public int luck_buff;//中奖概率翻倍的buff 次数
    public int is_super_fire; //是不是超级爆竹
//    public string exchange_info;

    //本地解析数据
    public List<P_2082Reward> rewardList;
    public List<P_2082Mission> missionList;//今日任务完成情况
    public Dictionary<int,int> exchangeDic;//奖励兑换次数 
}

public class P_2082Exchange
{
    public int exchange_id;//对应P_2082Reward->id
    public int exchange_num;
}
public class P_2082Reward
{
    public string reward;
    public int rate;//概率
    public int buff_rate;//买了buff后的概率
    public int id;//奖励id
    public int limit_count;//获取次数限制
    public int type;//初等 中等 高等
}

//花氪金买下次概率翻倍
public class P_LuckBuff2082
{
    public int buff_count;//中奖概率翻倍的buff 次数
    public string cost;//消耗的物品
}
public class P_2082Get
{
    public string cost;//消耗的物品
    public string reward;//获取的物品


    //fireFirecrackers 接口里加这两个字段同步状态
    public int luck_buff;//中奖概率翻倍的buff 次数
    public int is_super_fire; //下次是不是超级爆竹
    public string exchange_info;//奖励燃放完同步下兑换次数
}
public class P_2082Mission
{
    public int tid; //标识
    public int finished;//任务是否完成
    public int do_number; //当前完成进度
}