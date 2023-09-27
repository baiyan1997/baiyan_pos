using UnityEngine;
using UnityEngine.UI;

public class _Activity_2012_UI : ActivityUI
{
    private int _aid = 2012;
    private ObjectGroup UI;
    private Text _txtTime;
    private ActInfo_2012 _actInfo;

    private void InitData()
    {
        _actInfo = (ActInfo_2012)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void OnCreate()
    {
        UI = gameObject.GetComponent<ObjectGroup>();
        //UI.Get<Text>("_txtTitle").text = MapActConfig.Get[MapActID.PIRATE_SPEEDUP].name;
        //UI.Get<Text>("_txtDesc").text = Lang.Get("活动期间，攻击海盗有几率获得建造、研究、募兵加速5分钟的效果（获得后可在主基地看到，点击图标即可使用）");
        _txtTime = UI.Get<Text>("_txtTime");
        _txtTime.text = "";
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        InitData();
        //InitListener();
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        UpdateTime(TimeManager.ServerTimestamp);
    }

    public override void UpdateTime(long serverTime)
    {
        base.UpdateTime(serverTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (serverTime > _actInfo._data.endts)
        {
            Alert.Ok(Lang.Get("{0}已结束", Lang.Get("海盗加速")));
        }
        else if (serverTime < _actInfo._data.startts)
        {
            _txtTime.text = string.Format(Lang.Get("开启倒计时 {0}"), WorldUtils.getLastTime_DHMS(_actInfo._data.startts));
        }
        else
        {
            _txtTime.text = string.Format(Lang.Get("活动倒计时 {0}"), WorldUtils.getLastTime_DHMS(_actInfo._data.endts));
        }
    }
}
