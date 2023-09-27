using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2017 : ActivityInfo
{
    private int aid = 2017;
    public P_Act2017Data data2017;
    public override void InitUnique()
    {
        data2017 = JsonMapper.ToObject<P_Act2017Data>(_data.avalue["data"].ToString());
    }

    public override bool IsAvaliable()
    {
        StepType type = SupplyStep();
        if ((data2017.first == 0 && type == StepType.InMidday) || (data2017.second == 0 && type == StepType.InEvening))
            return true;
        return false;
    }


    //每天12,15,18,21,0五个时间点刷新
    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 12 || hour == 15 || hour == 18 || hour == 21 ||hour == 0;
    }

    public void GetAct2017Reward(Action<string> ac)
    {
        Rpc.SendWithTouchBlocking<string>("getAct2017Reward", null, data =>
        {
            if (SupplyStep() == StepType.InMidday)
                data2017.first = 1;
            if (SupplyStep() == StepType.InEvening)
                data2017.second = 1;

            ItemHelper.AddItem(data, true);



            if (SupplyStep() == StepType.WaitEvening)
            {
                data2017.three = 1;
                string reward = ItemId.Gold.ToString() + "|" + 50;
                ItemHelper.AddItem(reward, false);
            }
            if (SupplyStep() == StepType.WaitMidAtNight)
            {
                data2017.four = 1;
                string reward = ItemId.Gold.ToString() + "|" + 50;
                ItemHelper.AddItem(reward, false);
            }
            if (ac != null)
                ac(data);
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
        });
    }
    public StepType SupplyStep()//等待午间补给
    {
        DateTime now = TimeManager.ServerDateTime;
        if (now.Hour >= 15 && now.Hour < 18)
            return StepType.WaitEvening;
        if (now.Hour >= 12 && now.Hour < 15) //12点到15点 午间补给
            return StepType.InMidday;
        if (now.Hour >= 18 && now.Hour < 21) //18点到21点 晚间补给
            return StepType.InEvening;
        if (now.Hour >= 21)//增加晚间补领时间段
            return StepType.WaitMidAtNight;
        return StepType.WaitMid;
    }


}

public enum StepType //处于补给阶段
{
    WaitMid = 0,
    InMidday = 1,
    WaitEvening = 2,
    InEvening = 3,
    WaitMidAtNight = 4,
}

public class P_Act2017Data
{
    public int first;//0没领取  1 领取了
    public int second;//0没领取  1 领取了
    public int three;
    public int four;
    public int refresh_ts;//下次刷新时间
}



