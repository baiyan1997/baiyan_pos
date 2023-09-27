using UnityEngine.UI;

public class _Activity_2069_UI : ActivityUI
{
    private Text _txtTitle;
    private Text _txtTime;
    private Text _txtDesc1;
    private Text _txtDesc2;
    private Text _txtDesc3;
    private ObjectGroup UI;

    private ActInfo_2069 _actInfo;
    private int _aid = 2069;

    private void InitData()
    {
        _actInfo = (ActInfo_2069)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void OnCreate()
    {
        UI = transform.GetComponent<ObjectGroup>();
        _txtTitle = UI.Get<Text>("Title");
        _txtTime = UI.Get<Text>("Text_time");
        _txtDesc1 = UI.Get<Text>("Desc1");
        _txtDesc2 = UI.Get<Text>("Desc2");
        _txtDesc3 = UI.Get<Text>("Desc3");
        InitData();
    }

    public override void OnShow()
    {
        _txtTitle.text = Lang.Get("跨域夺星");
        _txtDesc1.text = Lang.Get("可参与其他星域星球战的进攻和防守");
        _txtDesc2.text = Lang.Get("只有本星域的指挥官能发起星球战；\n发起星球战后在各大星域的指挥官均可参与；\n攻下的星球只有本星域的指挥官可申请领主；");
        _txtDesc3.text = Lang.Get("Lv.19 星域");
        _txtTime.text = Lang.Get("活动持续至中心宇宙开启");
        //活动还没开始时显示预告
        var startts = _actInfo._data.startts;
        if (TimeManager.ServerTimestamp < startts)
        {
            var startTime = TimeManager.ToServerDateTime(startts);
            _txtTime.text = string.Format(Lang.Get("活动开启时间 {0}月{1}日{2}点{3}分"), startTime.Month, startTime.Day,
                startTime.Hour, startTime.Minute);
        }
    }
}
