using System;
using UnityEngine.UI;

public class _Activity_2041_UI : ActivityUI
{
    private Text _txtName;
    private Text _txtTime;
    private Text _txtTimeSpan;
    private Text _txtDesc;
    private ActInfo_2041 _actInfo;

    public override void OnCreate()
    {
        _txtName = transform.FindText("Text_title");
        _txtTime = transform.FindText("Text_time");
        _txtTimeSpan = transform.FindText("Text_timespan");
        _txtDesc = transform.FindText("Text_desc");
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        _txtName.text = Lang.Get("遍 地 氪 晶");
        //InitListener();
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2041)ActivityManager.Instance.GetActivityInfo(2041);

 
        _txtDesc.text = _actInfo._desc;
        //_txtName.text = _actInfo._name;

        DateTime startTime = TimeManager.ToServerDateTime(_actInfo.startTS);
        DateTime finishTime = TimeManager.ToServerDateTime(_actInfo.endTS);
        _txtTimeSpan.text =   string.Format(Lang.Get("{0}至{1}"),GetTimeDesc(startTime),GetTimeDesc(finishTime));

        UpdateTime(TimeManager.ServerTimestamp);
    }

    private string GetTimeDesc(DateTime _dateTime)
    {
        return string.Format(Lang.Get("{0}年{1}月{2}日"),_dateTime.Year,_dateTime.Month,_dateTime.Day);
    }

    public override void UpdateTime(long servserTime)
    {
        base.UpdateTime(servserTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_actInfo != null && transform.gameObject.activeSelf)
        {
            _txtTime.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
    }
}
