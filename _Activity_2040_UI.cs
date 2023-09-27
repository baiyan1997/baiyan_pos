using System;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2040_UI : ActivityUI
{
    private Text _txtTime;
    private Text _txtDesc;
    private ActInfo_2040 _actInfo;

    public override void OnCreate()
    {
        ObjectGroup UI = gameObject.GetComponent<ObjectGroup>();
        _txtTime = UI.Get<Text>("_txtTime");
        _txtDesc = UI.Get<Text>("_txtDesc");
        _actInfo = (ActInfo_2040)ActivityManager.Instance.GetActivityInfo(ActivityID.MineReserve);
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //InitListener();
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (st - _actInfo._data.startts < 0)
        {
            _txtTime.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _txtTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _txtTime.text = Lang.Get("活动已经结束");
        }
    }

    public override void OnShow()
    {
        UpdateTime(TimeManager.ServerTimestamp);
        string _planetList = "";
        string[] planetIds = _actInfo._data.avalue["protect_castle"].ToString().Split(',');
        for (int i = 0; i < planetIds.Length; i++)
        {
            string planetid = planetIds[i];
            Debug.Log(planetid + Cfg.CastleName.GetPlanetNameSimple(int.Parse(planetid)));
            if (string.IsNullOrEmpty(_planetList))
                _planetList = string.Format(Lang.Get("{0}区域"), Cfg.CastleName.GetPlanetNameSimple(int.Parse(planetid)));
            else
                _planetList = _planetList + "、" + string.Format(Lang.Get("{0}区域"), Cfg.CastleName.GetPlanetNameSimple(int.Parse(planetid)));
        }
        _planetList = "<Color=#ffff66>" + _planetList + "</Color>";
        _txtDesc.text = string.Format(Cfg.Act.GetData(2040).act_desc, _planetList);
    }
}