using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2043 : ActivityInfo
{
    private List<P_Act2043Info> _missionInfo; //用于数据交互，任务刷新
    private P_Act2043Info[] _infoForPage1 = new P_Act2043Info[5]; //第一档
    private P_Act2043Info[] _infoForPage2 = new P_Act2043Info[5]; //第二档
    private P_Act2043Info[] _infoForPage3 = new P_Act2043Info[5]; //第三档

    public override void InitUnique()
    {
        if (_data.avalue == null)
            throw new Exception("ActInfo_2043 info can not get availablely");
        var data = JsonMapper.ToObject<List<P_Act2043Info>>(_data.avalue["mission_info"].ToString());
        CoverInfo(data);
    }

    private void CoverInfo(List<P_Act2043Info> data)
    {
        _missionInfo = data;
        //_missionInfo.Sort((a, b) =>
        //{
        //    if (a.page != b.page)
        //    {
        //        return a.page < b.page ? -1 : 1;
        //    }
        //    if (a.day != b.day)
        //    {
        //        return a.day < b.day ? -1 : 1;
        //    }
        //    return 0;
        //});
        _missionInfo.Sort(Sort_act2043);
        for (int i = 0; i < _missionInfo.Count; i++)
        {
            var info = _missionInfo[i];
            if (info.page == 1)
                _infoForPage1[info.day - 1] = info;
            else if (info.page == 2)
                _infoForPage2[info.day - 1] = info;
            else if (info.page == 3)
                _infoForPage3[info.day - 1] = info;
        }
    }
    private int Sort_act2043(P_Act2043Info a, P_Act2043Info b)
    {
        if (a.page != b.page)
        {
            return a.page < b.page ? -1 : 1;
        }
        if (a.day != b.day)
        {
            return a.day < b.day ? -1 : 1;
        }
        return 0;
    }
    public override bool IsAvaliable()
    {
        return IsDuration() && IsCanGet();
    }

    private bool IsCanGet()
    {
        return IsCanGetPage(1) || IsCanGetPage(2) || IsCanGetPage(3);
    }

    public bool IsCanGetPage(int page)
    {
        var arr = GetInfoByPage(page);
        if (arr[0] == null || arr[0].today <= 0) //未购买
            return false;
        for (int i = 0; i < arr.Length; i++)
        {
            var info = arr[i];
            if (info == null)
                continue;

            if (info.finished == 1 && info.get_reward == 0)
                return true;
        }

        return false;
    }

    public P_Act2043Info[] GetInfoByPage(int page)
    {
        switch (page)
        {
            case 1:
                return _infoForPage1;
            case 2:
                return _infoForPage2;
            case 3:
                return _infoForPage3;
            default:
                throw new Exception("wrong page = " + page);
        }
    }

    public P_Act2043Info FindCanGetInfo(int page)
    {
        var arr = GetInfoByPage(page);
        if (arr[0] == null || arr[0].today <= 0) //未购买
            return null;

        for (int i = 0; i < arr.Length; i++)
        {
            var info = arr[i];
            if (info == null)
                continue;

            if (info.finished == 1 && info.get_reward == 0)
                return info;
        }

        return null;
    }

    public void RequestBuyBox(int page)
    {
        var arr= GetInfoByPage(page);
        var sample = arr[0];
        Rpc.SendWithTouchBlocking<List<P_Act2043Info>>("buyAct2043Reward", Json.ToJsonString(page), data =>
        {
            Uinfo.Instance.AddItem(ItemId.Gold, -sample.gold_level);
            CoverInfo(data);
            _data.can_get_reward = IsCanGet();
            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, _data.can_get_reward);
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        });
    }

    // 领取奖励
    public void RequestGetReward(int page)
    {
        var sample = FindCanGetInfo(page);
        if (sample == null)
            return;

        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2043Reward", Json.ToJsonString(sample.tid), data =>
        {
            sample.get_reward = 1;
            _data.can_get_reward = IsCanGet();
            Uinfo.Instance.AddItem(data.get_items, true);
            MessageManager.ShowRewards(data.get_items);
            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, _data.can_get_reward);
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        });
    }
}

public class P_Act2043Info
{
    public long start_ts;
    public string reward;//是否领奖
    public int uid;//只用于显示任务进度
    public int gold_level;//id
    public string data;
    public long end_ts;
    public int finished;
    public int get_reward;
    public int page;
    public int day;
    public int tid;
    public int today; //当前天数
}

