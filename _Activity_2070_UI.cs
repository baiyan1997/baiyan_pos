using UnityEngine.UI;

public class _Activity_2070_UI : ActivityUI
{
    private Text _txtTitle;
    private Text _txtTime;
    private Text _txtDesc1;
    private Text _txtDesc2;
    private Text _txtDesc3;
    private ObjectGroup UI;

    private ActInfo_2070 _actInfo;
    private int _aid = 2070;

    private void InitData()
    {
        _actInfo = (ActInfo_2070)ActivityManager.Instance.GetActivityInfo(_aid);
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
        _txtTitle.text = Lang.Get("跨域歼敌");
        _txtDesc1.text = Lang.Get("可参与其他星域基地战的进攻和防守");
        _txtDesc2.text = Lang.Get("在各大星域的指挥官均能发起基地战;\n跨星域发起基地战行军耗时会更长;");
        _txtDesc3.text = Lang.Get("Lv.25 其他国家玩家");
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
