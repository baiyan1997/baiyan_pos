using System;
using UnityEngine.UI;

public class _ActivityDouble_UI : ActivityUI
{
    private int _aid;
    private ObjectGroup _ui;
    private ActivityInfo _actInfo;

    private void InitData()
    {
        _actInfo = ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void SetAid(int aid)
    {
        _aid = aid;
    }
    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
        //TimeManager.Instance.TimePassSecond += UpdateTime;
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == _aid)
        {
            _ui.Get<Text>("Text_desc").text = Cfg.Act.GetData(_aid).act_desc;
            UpdateTime(TimeManager.ServerTimestamp);
        }
    }

    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;

        if (stamp - _actInfo._data.startts < 0)
        {
            _ui.Get<Text>("Text_time").text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
            _ui.Get<Text>("Text_timespan").text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _ui.Get<Text>("Text_time").text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
            _ui.Get<Text>("Text_timespan").text = GlobalUtils.ActTimeFormat(_actInfo._data.startts, _actInfo._data.endts, false);
        }
        else
        {
            _ui.Get<Text>("Text_time").text = Lang.Get("活动已经结束");
        }
    }
    public override void Awake()
    {
    }

    public override void OnCreate()
    {
        _ui = gameObject.GetComponent<ObjectGroup>();
        InitData();
        //InitListener();
        InitUi();
    }

    private void InitUi()
    {
        UpdateUI(_aid);
    }

    public override void OnShow()
    {
        UpdateUI(_aid);
    }

    public override void OnClose()
    {
        base.OnClose();
    }
}
