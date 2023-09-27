using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2031 : ActivityInfo
{
    public Dictionary<int, int> Status = new Dictionary<int, int>(8);//<tid, 0未达成 1达成未领奖 2已领奖 3去充值>
    public List<Act2031Data> data;
    public override void InitUnique()
    {
        data = JsonMapper.ToObject<List<Act2031Data>>(_data.avalue["mission_info"].ToString());
        for (int i = 0; i < data.Count; i++)
        {
            data[i].rewards = GlobalUtils.ParseItem3(data[i].reward);
            data[i].needNum = Int32.Parse(data[i].mission_finish.Split('|')[0]);
            if (data[i].finished == 1)
            {
                if (data[i].get_reward == 1)
                    Status[data[i].tid] = 2;
                else
                    Status[data[i].tid] = 1;
            }
            else
                Status[data[i].tid] = 3;

        }
    }

    public override bool IsAvaliable()
    {
        for (int i = 0; i < data.Count; i++)
        {
            int v = 0;
            if (Status.TryGetValue(data[i].tid, out v))
            {
                if (v == 1)
                    return true;
            }
            else
            {
                throw new Exception("Status can't find tid key" + data[i].tid);
            }
        }
        return false;
    }
    public override bool IfRefreshOnPush(int opcode)
    {
        return opcode == OpcodePush.Recharge;
    }
    public void GetRewardById(int id, Action ac)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2031Reward", Json.ToJsonString(id), data =>
        {
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(data.get_items);
            if (ac != null)//先ac 再广播
                ac();
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
}

public class Act2031Data
{
    //服务器参数
    public string reward;
    public string mission_finish;
    public int finished;//0未完成 1完成
    public int do_number;
    public int get_reward;//0未领 1已领
    public int tid;
    //客户端自用
    public int needNum;
    public P_Item3[] rewards;
}