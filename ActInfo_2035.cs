using System.Collections.Generic;

public class ActInfo_2035 : ActivityInfo
{
    private List<RewardItem> _itemList = new List<RewardItem>();
    public List<RewardItem> RewardList
    {
        get { return _itemList; }
    }
    public override void InitUnique()
    {
        _itemList.Clear();
        foreach (Dictionary<string, string> reward in _data.rewards)
        {
            string str1 = reward["reward"];
            string[] items = str1.Split(',');

            for (int i=0;i<items.Length;i++)
            {
                string item = items[i];
                RewardItem award = new RewardItem(item);
                _itemList.Add(award);
            }
        }
    }
    public override bool IsAvaliable()
    {
        return IsDuration() && IsCanGet();
    }
    private bool IsCanGet()
    {
        return _data.can_get_reward;
    }
    //领奖接口
    public void GetReward()
    {
        if (IsAvaliable())
        {
            //Rpc.SendWithTouchBlocking<P_ActAward>("getAct2035Reward", null, data =>
            //{
            //    _data.get_all_reward = true;
            //    _data.can_get_reward = false;

            //    Uinfo.Instance.AddItem(data.get_items, true);
            //    MessageManager.ShowRewards(data.get_items);

            //    EventCenter.Instance.RemindActivity.Broadcast(_aid, _data.can_get_reward);
            //    EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
            //});
            Rpc.SendWithTouchBlocking<P_ActAward>("getAct2035Reward", null, On_getAct2035Reward_SC);
        } 
    }
    private void On_getAct2035Reward_SC(P_ActAward data)
    {
        _data.get_all_reward = true;
        _data.can_get_reward = false;

        Uinfo.Instance.AddItem(data.get_items, true);
        MessageManager.ShowRewards(data.get_items);

        EventCenter.Instance.RemindActivity.Broadcast(_aid, _data.can_get_reward);
        EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
    }
}

