using System.Collections.Generic;
using LitJson;

public class ActInfo_2074 : ActivityInfo
{
    private ActInfo_2074_Data _data2074;
    public override bool OnInited()
    {
        EventCenter.Instance.UpdatePlayerItem.AddListener(CheckRemind);
        return true;
    }

    private void CheckRemind()
    {
        var actInfo = ActivityManager.Instance.GetActivityInfo(_aid);
        if(actInfo!=null && actInfo.IsDuration())
            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
    }

    public override void OnRemove()
    {
        base.OnRemove();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(CheckRemind);
    }

    public override void InitUnique()
    {
        _data2074 = JsonMapper.ToObject<ActInfo_2074_Data>(_data.avalue["data"].ToString());
    }

    //抽1次
    public void DrawLotteryTicketOne(int index)
    {
        //Rpc.SendWithTouchBlocking<ActInfo_2074_Data>("drawOneLotteryTicket",  Json.ToJsonString(index), data =>
        //{
        //    _data2074 = data;
        //    _data2074.drawFive = false;
        //    //additem 会触发EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
        //    Uinfo.Instance.AddItem(data.cost,false);
        //    if(!string.IsNullOrEmpty(data.get))
        //        Uinfo.Instance.AddItem(data.get,true);

        //    EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        //});
        Rpc.SendWithTouchBlocking<ActInfo_2074_Data>("drawOneLotteryTicket", Json.ToJsonString(index), On_drawOneLotteryTicket_CS);
    }
    private void On_drawOneLotteryTicket_CS(ActInfo_2074_Data data)
    {
        _data2074 = data;
        _data2074.drawFive = false;
        //additem 会触发EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
        Uinfo.Instance.AddItem(data.cost, false);
        if (!string.IsNullOrEmpty(data.get))
            Uinfo.Instance.AddItem(data.get, true);

        EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
    }
//    private bool CheckAllDraw(Dictionary<string,int> dic)
//    {
//        foreach (var v in dic.Values)
//        {
//            if (v == 0)
//                return false;
//        }
//
//        return true;
//    }
//抽多次
    public void DrawLotteryTicketFive()
    {
        //Rpc.SendWithTouchBlocking<ActInfo_2074_Data>("drawFiveLotteryTicket",  null, data =>
        //{
        //    _data2074 = data;
        //    _data2074.drawFive = true;
        //    //additem 会触发EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
        //    Uinfo.Instance.AddItem(data.cost,false);
        //    if(!string.IsNullOrEmpty(data.get))
        //        Uinfo.Instance.AddItem(data.get,true);
        //    EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        //});
        Rpc.SendWithTouchBlocking<ActInfo_2074_Data>("drawFiveLotteryTicket", null, On_drawFiveLotteryTicket_CS);
    }
    private void On_drawFiveLotteryTicket_CS(ActInfo_2074_Data data)
    {
        _data2074 = data;
        _data2074.drawFive = true;
        //additem 会触发EventCenter.Instance.RemindActivity.Broadcast(_data.aid, IsAvaliable());
        Uinfo.Instance.AddItem(data.cost, false);
        if (!string.IsNullOrEmpty(data.get))
            Uinfo.Instance.AddItem(data.get, true);
        EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
    }
    public override bool IsAvaliable()
    {
        return BagInfo.Instance.GetItemCount(ItemId.StarTicket) > 0;
    }

    public int TodayGetTicketNum()
    {
        return _data2074.today_get;
    }

    public ActInfo_2074_Data GetInfo()
    {
        return _data2074;
    }

    public bool IndexCanDraw(int index)
    {
        var v = _data2074.draw_list[index.ToString()];
        return (Reward2073Type) v == Reward2073Type.NotDraw;
    }

    public bool IsAllClose()
    {
        foreach (var v in _data2074.draw_list.Values)
        {
            if ((Reward2073Type) v != Reward2073Type.NotDraw)
                return false;
        }

        return true;
    }

    public void ClearDrawResult()
    {
        _data2074.draw_result.Clear();
        _data2074.get = string.Empty;
    }
    public bool DrawResultHasReward()
    {
//        //谢谢惠顾也有奖励  所以只要抽了就有奖励
//        return _data2074.draw_result.Count > 0;
        if (_data2074.draw_result.Count > 0)
        {
            //抽一次 有奖励
            if (!_data2074.drawFive)
            {
                foreach (var v in _data2074.draw_result.Values)
                {
                    if ((Reward2073Type) v == Reward2073Type.Lv1
                        || (Reward2073Type) v == Reward2073Type.Lv2
                        || (Reward2073Type) v == Reward2073Type.Lv3)
                        return true;
                }
            }
            //抽五次有 奖励
            else
            {
                foreach (var v in _data2074.draw_result.Values)
                {
                    if ((Reward2073Type) v == Reward2073Type.Lv1
                        || (Reward2073Type) v == Reward2073Type.Lv2
                        || (Reward2073Type) v == Reward2073Type.Lv3)
                        return true;
                }

                foreach (var v in _data2074.draw_list.Values)
                {
                    if ((Reward2073Type) v == Reward2073Type.Lv1
                        || (Reward2073Type) v == Reward2073Type.Lv2
                        || (Reward2073Type) v == Reward2073Type.Lv3)
                        return true;
                }
            }
        }
        return false;
    }
}

public class ActInfo_2074_Data
{
    public List<string> level_reward;
    public Dictionary<string,int> draw_list;//待抽的东西 key:index  value: 0:还没抽 1:一等奖 2:二等奖 3:三等奖 4:谢谢惠顾:
    public int today_get;//今日获取的抽奖券

    public Dictionary<string, int> draw_result;//抽奖结果
    public string cost; //抽奖消耗
    public string get;//奖励 仅放谢谢惠顾奖励

    public bool drawFive;//记录抽了1次还是5次客户端用
}

public enum Reward2073Type
{
    Undefined = -1,
    NotDraw = 0,//还没抽
    Lv1 = 1,//一等奖
    Lv2 = 2,//二等奖
    Lv3 = 3,//三等奖
    Thx = 4,//谢谢惠顾
}