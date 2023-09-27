
public class ActInfo_2042:ActivityInfo
{
    private int _count;
    public int Count
    {
        get { return _count; }
    }

    private long _dcTime;
   
    public long _leftDcTime
    {
        get { return _dcTime - TimeManager.ServerTimestamp; }
    }

    private int _price;
    public int Price
    {
        get { return _price; }
    }

    public override void InitUnique()
    {
        if (_data.avalue == null)
            return;

        _count = int.Parse(_data.avalue["today_count"].ToString());

        _dcTime = long.Parse(_data.avalue["end_cdts"].ToString());

        _price = int.Parse(_data.avalue["cd_gold"].ToString());

    }

    public override bool IsAvaliable()
    {
        return _leftDcTime < 0 && Count > 0;
    }

    public void RequestCleanDcTime()
    {
        if(IsDuration())
        {
            _AlertYesNo a = Alert.YesNo(string.Format(Lang.Get("是否花费{0}氪晶消除CD？"), _price));
            a.SetYesCallback(() =>
            {
                //Rpc.SendWithTouchBlocking<int>("clearAct2042CdTs", null, data =>
                //{
                //    Uinfo.Instance.Player.AddGold(-data);

                //    MessageManager.Show(Lang.Get("消耗{0}氪晶清除CD"), data);

                //    ActivityManager.Instance.RequestUpdateActivityById(_aid);
                //});
                Rpc.SendWithTouchBlocking<int>("clearAct2042CdTs", null, On_clearAct2042CdTs_SC);
               a.Close();
            });
        }
    }
    private void On_clearAct2042CdTs_SC(int data)
    {
        Uinfo.Instance.Player.AddGold(-data);

        MessageManager.Show(Lang.Get("消耗{0}氪晶清除CD"), data);

        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }

    public void GetReward()
    {
        //Rpc.SendWithTouchBlocking<string>("getAct2042Reward", null, data =>
        //{
        //    Uinfo.Instance.AddItem(data, true);

        //    MessageManager.Show(Lang.Get("可在背包中查看获得的红包,红包将在本次活动结束无法使用"));

        //    ActivityManager.Instance.RequestUpdateActivityById(_aid);
        //});
        Rpc.SendWithTouchBlocking<string>("getAct2042Reward", null, On_getAct2042Reward_SC);
    }
    private void On_getAct2042Reward_SC(string data)
    {
        Uinfo.Instance.AddItem(data, true);

        MessageManager.Show(Lang.Get("可在背包中查看获得的红包,红包将在本次活动结束无法使用"));

        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }
}

