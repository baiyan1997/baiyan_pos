using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2060 : ActivityInfo
{
    public P_ActInfo2060 Info;
    public List<P_Act2060Mission> _rewardData { get; private  set; }
    private Dictionary<int, P_Act2060Mission> _dicGetReward = new Dictionary<int, P_Act2060Mission>(15);

    public override void InitUnique()
    {

        if (_data.avalue.Count > 0)
        {
            Info = JsonMapper.ToObject<P_ActInfo2060>(_data.avalue["data"].ToString());
            _rewardData = JsonMapper.ToObject<List<P_Act2060Mission>>(_data.avalue["mission_info"].ToString());
            InitData();
        }
    }
    public override bool OnInited()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.GAIN_ITEM, CheckRefresh);
        return true;
    }
    private void CheckRefresh(int opcode, string data)
    {
        string itemList = data;
        var pitems = GlobalUtils.ParseItem(itemList);//击败度假海盗更新宝券掉落
        for (int i = 0; i < pitems.Length; i++)
        {
            if (pitems[i].id == ItemId.CelebrationTicket && IsDuration())
            {
                ActivityManager.Instance.RequestUpdateActivityById(_aid);
            }
        }
    }
    private void InitData()
    {
        _dicGetReward.Clear();
        for (int i=0;i<_rewardData.Count;i++)
        {
            _dicGetReward.Add(_rewardData[i].tid,_rewardData[i]);
        }
    }
    //是否获得奖励
    public bool IsGetReward(int tid)
    {
        P_Act2060Mission data;
        _dicGetReward.TryGetValue(tid, out data);
        if (data == null)
            return false;
        return (data.get_reward!=0);
    }
    public override bool IsAvaliable()
    {
        for (int j = 0; j < _rewardData.Count; j++)
        {
            if ((_rewardData[j].get_reward == 0) && IsDuration())
                return true;
        }
        return false;
    }
    //解锁狂欢礼盒
    public void UnLockCarnivalBox()
    {
        //Rpc.SendWithTouchBlocking<P_Lottery2060>("unlockCelebrationBox", null, data =>
        //{
        //    ItemHelper.AddItem(data.cost_item, false);
        //    //更新活动信息
        //    ActivityManager.Instance.RequestUpdateActivityById(_aid);
        //});
        Rpc.SendWithTouchBlocking<P_Lottery2060>("unlockCelebrationBox", null, On_unlockCelebrationBox_SC);
    }
    private void On_unlockCelebrationBox_SC(P_Lottery2060 data)
    {
        ItemHelper.AddItem(data.cost_item, false);
        //更新活动信息
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }


    //升级
    public void UpGradeBox(Action<P_Lottery2060> ac)
    {
        Rpc.SendWithTouchBlocking<P_Lottery2060>("upgradeCelebrationBox", null, data =>
        {
            ItemHelper.AddItem(data.cost_item, false);
            //升级完成重新请求数据
            ActivityManager.Instance.RequestUpdateActivityById(_aid);
            if (ac!=null)
            {
                ac(data);
            }
        });
    }
    //获取奖励
    public void GetAct2060Reward(int id, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("get2060Reward", Json.ToJsonString(id), data =>
        {
            //添加道具
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(rewardsStr);
            ActivityManager.Instance.RequestUpdateActivityById(_aid);//更新活动信息
            if (callback != null)
                callback();
        });
    }
}
public class P_ActInfo2060
{
    public int curLv ;//当前等级
    public int pirate_drop;//今日宝券数量上限
    public int step;//当前服务端进程
    public int unlock;//是否解锁狂欢礼盒，0未解锁 1解锁
    public int progress;//已经消耗但是未升级的宝券
}
//已经获得得奖励
public class P_Act2060Mission
{
    public int tid;//当前等级得id
    public int get_reward;//是否已经领取 0未领取 1 已经领取
}
public class P_Lottery2060
{
    public string cost_item;//消耗的物品
    public int preLv;//升级前的等级
    public int nextLv;//升级后的等级
}