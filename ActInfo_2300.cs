using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2300 : ActivityInfo
{
    public List<P_PageM> mission_pages;//活动数据
    public override void InitUnique()
    {
        var jsonStr = _data.avalue["data"].ToString();
        mission_pages = JsonMapper.ToObject<List<P_PageM>>(jsonStr);
//      _data.bg_url可以为空
//        if (string.IsNullOrEmpty(_data.bg_url))
//            throw new Exception("actid 2300 bg_url is null");
    }

    public void RequestGetReward(P_OneM info)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2300Reward", Json.ToJsonString(info.tid), data =>
        {
            info.get_reward = 1;
            _data.can_get_reward = IsCanGet();

            if (Uinfo.Instance != null)
            {
                Uinfo.Instance.AddItem(data.get_items, true);
                MessageManager.ShowRewards(data.get_items);
            }
            EventCenter.Instance.RemindActivity.Broadcast(_data.aid, _data.can_get_reward);
            EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        });
    }
    public override bool IsAvaliable()
    {
        return IsDuration() && IsCanGet();
    }
    private bool IsCanGet()
    {
        for (int i = 0; i < mission_pages.Count; i++)
        {
            var page = mission_pages[i];
            if (IsCanGet(page.page_missions))
                return true;
        }
        return false;
    }
    public bool IsCanGet(List<P_OneM> missions)
    {
        for (int i = 0; i < missions.Count; i++)
        {
            var m = missions[i];
            if (m.finished == 1 && m.get_reward == 0)
                return true;
        }
        return false;
    }

    public P_PageM GetMissionsByPage(int pagId)
    {
        for (int i = 0; i < mission_pages.Count; i++)
        {
            if (mission_pages[i].pageid == pagId)
                return mission_pages[i];
        }

        return null;
    }
    public string GetBannerName()
    {
        //活动名时间戳 唯一确定banner名称
        var strs = _data.bg_url.Split('.');
        return _data.aid.ToString() + _data.startts.ToString() +"." + strs[strs.Length - 1];
    }
    public string GetBannerFullPath()
    {
        return FileStrategy.WritableDir + "/ActivityImg/"+ GetBannerName();
    }
}
public class P_PageM
{
    public string name;//切页名称
    public int pageid;//切页id
    public List<P_OneM> page_missions = new List<P_OneM>();
    public int page_refresh_ts;//切页倒计时  0则不显示
}
public class P_OneM
{
    public int tid;//唯一标记任务 领奖传参
    public int do_number;
    public int need_count;
    public string reward;
    public int finished;
    public string click;
    public int get_reward;//是否领过奖

    public string _name;//任务名称

    public string name { get { return Lang.TranslateFromDb(_name); } set { _name = value; } }
}