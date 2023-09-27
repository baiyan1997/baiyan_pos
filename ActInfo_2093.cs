using System;
using System.Collections.Generic;
using LitJson;
public class ActInfo_2093 : ActivityInfo
{

    private int _currentRechargeNum;


    private List<Act2093Detail> _act2093Detail;


    public bool IsShowBlessingBagRedPoint;

    public bool IsShowStagesRedPoint;


    private int _aid = 2093;

    private bool _first = true;
    public override void InitUnique()
    {
        //初始化是否领取有关阶段奖励、是否可以领取每日礼包、充值点数
        _act2093Detail = JsonMapper.ToObject<List<Act2093Detail>>(_data.avalue["mission_info"].ToString());
        //绑定推送
        _currentRechargeNum = int.Parse(_data.avalue["pay_gold"].ToString());
        IsShowBlessingBagRedPoint = (_act2093Detail[0].get_reward == 0);
        ChangeRedPointFlag();
        EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());

    }


    public List<Act2093Detail> GetAct2093DetailInfo()
    {
        return _act2093Detail;
    }

    public int GetCurrentRechargeNum()
    {
        return _currentRechargeNum;
    }
    public override bool IsAvaliable()
    {
        return IsShowBlessingBagRedPoint || IsShowStagesRedPoint;
    }

    //任何一个阶段奖励没有领取时，红点标志为true
    private void ChangeRedPointFlag()
    {
        for (int i = 0; i < _act2093Detail.Count; i++)
        {
            var one = _act2093Detail[i];
            if (one.finished == 1 && one.get_reward == 0)
            {
                IsShowStagesRedPoint = true;
                return;
            }
        }
        IsShowStagesRedPoint = false;
    }
    //领取阶段奖励
    public void GetReward(int rechargeTarget, Action callback)
    {
        Rpc.SendWithTouchBlocking<GetAct2093rewardInfo>("getAct2093Reward", Json.ToJsonString(rechargeTarget), data =>
        {

            if (rechargeTarget == Cfg.Activity2093.GetDailyTreasureTid())
            {
                IsShowBlessingBagRedPoint = false;
                _act2093Detail[0].get_reward = 1;

                Uinfo.Instance.AddItem(data.get_items, true);

                P_Item[] temp_items = GlobalUtils.ParseItem(data.get_items);
                List<P_Item> tempItem = new List<P_Item>();
                for (int i = 0; i < temp_items.Length; i++)
                {
                    tempItem.Add(temp_items[i]);
                }
                DialogManager.ShowAsyn<_D_ShowRewards>(d =>
                {
                    d?.ShowCustonRewards(tempItem,
                        Lang.Get("每日宝箱奖励"),
                        Lang.Get("恭喜您获得以下奖励"),
                        Lang.Get("确定"));
                });
            }
            else
            {
                for (int i = 0; i < _act2093Detail.Count; i++)
                {
                    var one = _act2093Detail[i];
                    if (one.tid == rechargeTarget)
                    {
                        one.get_reward = 1;
                    }
                }
                ChangeRedPointFlag();
                Uinfo.Instance.AddItemAndShow(data.get_items);
            }

            EventCenter.Instance.RemindActivity.Broadcast(ActivityID.CumulativeRecharge, IsAvaliable());
            callback?.Invoke();
        });
    }


}


public class Act2093Detail
{
    public int tid;
    public int finished;
    public int get_reward;
    public int do_number;
}
public class GetAct2093rewardInfo
{
    public string get_items;
}