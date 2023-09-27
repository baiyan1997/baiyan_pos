using System;
using System.Linq;
using UnityEngine.UI;

public class _Activity_2018_UI : ActivityUI
{
    private Text _time;

    private ListView _listRewards;

    private ActInfo_2018 actInfo;

    private int _aid = 2018;

    private void InitData()
    {
        actInfo = (ActInfo_2018)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void OnCreate()
    {
        _time = transform.FindText("CountDown");
        _listRewards = ListView.Create<_Act2018Item>(transform.Find("Scroll View"));
        InitData();
        //InitListener();
        //TimeManager.Instance.TimePass += time =>
        //{
        //    var leftTime = actInfo.LeftTime;
        //    if (leftTime > 0)
        //    {
        //        var span = new TimeSpan(0, 0, (int) actInfo.LeftTime);
        //        _time.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
        //            span.Minutes, span.Seconds);
        //    }
        //    else
        //    {
        //        _time.text = Lang.Get("活动已经结束");
        //    }
        //};
        //EventCenter.Instance.UpdateActivityUI.AddListener(aid =>
        //{
        //    if (_aid != aid)
        //        return;
        //    UpdateUi();
        //});
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        UpdateUi();
    }

    public void UpdateUi()
    {
        _listRewards.Clear();
        var items = actInfo._data2018.cfg_data.Values.ToList();

        //排序
        items.Sort(Sort_act2018);
        for (int i = 0, max = items.Count; i < max; i++)
        {
            var itemInfo = items[i];
            _listRewards.AddItem<_Act2018Item>().Refresh(itemInfo, item =>
            {
                actInfo.GetAct2018Reward(itemInfo.id, () =>
                {
                    //添加已领取奖励
                    if (!actInfo._canGotId.Contains(itemInfo.id))
                        actInfo._canGotId.Add(itemInfo.id);
                    //刷新单个奖励ui
                    item.UpdateUI(itemInfo);
                    EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
                });
            });
        }
    }
    private int Sort_act2018(P_Act2018Item a, P_Act2018Item b)
    {
        var s1 = a.state;
        var s2 = b.state;
        var l1 = a.vip_level;
        var l2 = b.vip_level;
        if (s1 != s2)
        {
            if (s1 == 1 && s2 != 1)
                return -1;
            else if (s2 == 1 && s1 != 1)
                return 1;
            else if (s1 == 0 && s2 != 0)
                return -1;
            else if (s2 == 0 && s1 != 0)
                return 1;
        }
        return l1 > l2 ? 1 : (l1 < l2 ? -1 : 0);
    }

    public override void UpdateTime(long time)
    {
        base.UpdateTime(time);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        var leftTime = actInfo.LeftTime;
        if (leftTime > 0)
        {
            var span = new TimeSpan(0, 0, (int)actInfo.LeftTime);
            _time.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _time.text = Lang.Get("活动已经结束");
        }
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (_aid != aid)
            return;
        UpdateUi();
    }
}
