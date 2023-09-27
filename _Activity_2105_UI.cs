using UnityEngine;
using UnityEngine.UI;
public class _Activity_2105_UI : ActivityUI
{
    private JDText _textDesc;
    private JDText _textCountDown;
    private Button _btnHelpTips;
    private Button _btnGo;
    private JDText _textLfetCount;
    private JDText _textCountRunOut;
    private JDText _textRefreshTime;
    private JDText _textAnnounce;//预告
    private JDText _textAnnounceCountDown;//开启倒计时
    private GameObject _objAnnounce;
    private GameObject _objMain;

    private ActInfo_2105 _actInfo;

    private readonly int _actId = 2105;

    public override void Awake()
    {
        _textDesc = transform.Find<JDText>("Main_02/TextDesc");
        _textCountDown = transform.Find<JDText>("Main_02/Text_time");
        _btnHelpTips = transform.Find<Button>("Main_02/Helpbtn");
        _btnGo = transform.Find<Button>("Main_02/ButtonGo");
        _textLfetCount = transform.Find<JDText>("Main_02/TextLeftCount");
        _textCountRunOut = transform.Find<JDText>("Main_02/TextCountRunOut");
        _textRefreshTime = transform.Find<JDText>("Main_02/TextRefreshCountDown");
        _textAnnounce = transform.Find<JDText>("Main_01/Scroll View/Viewport/Text");
        _textAnnounceCountDown = transform.Find<JDText>("Main_01/Text_time");
        _objAnnounce = transform.Find("Main_01").gameObject;
        _objMain = transform.Find("Main_02").gameObject;
    }

    public override void OnCreate()
    {
        InitEvent();
    }

    private void InitEvent()
    {
        _btnHelpTips.onClick.AddListener(() =>
        {
            DialogManager.ShowAsyn<_D_Tips_HelpDesc>(d => { d?.OnShow(HelpType.Act2105, _btnHelpTips.transform.position, Direction.LeftDown, 350); });
        });
        //跳转到临时据点
        _btnGo.onClick.AddListener(() =>
        {
            WorldConfig.WorldController.EnterWorld(false, _actInfo.Info.castle_id);
        });
        _textDesc.SetHyperlinkCallback(_HyperLinkCallback.CoordCallback);

        TimeManager.Instance.TimePassSecond += UpdateTime;
    }
    private void UnInitEvent()
    {
        TimeManager.Instance.TimePassSecond -= UpdateTime;
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2105)ActivityManager.Instance.GetActivityInfo(_actId);
        //活动开启时
        if(_actInfo != null)
        {
            _objAnnounce.SetActive(false);
            _objMain.SetActive(true);
            var coord = WorldPositionCul.PlanetId_to_WorldPos_String(_actInfo.Info.castle_id);
            _textDesc.text = Lang.Get("前往中心宇宙{0}寻找据点前往争夺", coord);
            if (_actInfo.Info.get_reward_count < _actInfo.Info.max_reward_count)
            {
                _textLfetCount.gameObject.SetActive(true);
                _textCountRunOut.gameObject.SetActive(false);
                _textLfetCount.text = Lang.Get("剩余奖励获得次数:{0}/{1}", _actInfo.Info.max_reward_count - _actInfo.Info.get_reward_count, _actInfo.Info.max_reward_count);
            }
            else
            {
                _textLfetCount.gameObject.SetActive(false);
                _textCountRunOut.gameObject.SetActive(true);
            }
        }
        else
        {
            //活动预告时
            _objAnnounce.SetActive(true);
            _objMain.SetActive(false);

            var info = ActivityManager.Instance.GetFutureActivityInfo(_actId);
            _textAnnounce.text = Cfg.Act.GetData(_actId).pre_act;
        }
        UpdateTime(0);
    }

    public override void UpdateTime(long t)
    {
        base.UpdateTime(t);
        if (!ActivityManager.Instance.IsActDuration(_actId))
        {
            var info = ActivityManager.Instance.GetFutureActivityInfo(_actId);
            if(info != null)
                _textAnnounceCountDown.text = Lang.Get("开启倒计时 {0}", GLobal.TimeFormat(info._data.startts - TimeManager.ServerTimestamp, true));
            return;
        }
        if(_actInfo == null)
            _actInfo = (ActInfo_2105)ActivityManager.Instance.GetActivityInfo(_actId);
        _textRefreshTime.text = Lang.Get("本轮据点已刷新 {0}", GLobal.TimeFormat(TimeManager.ServerTimestamp - _actInfo.Info.last_refresh_ts));
        _textCountDown.text = Lang.Get("活动结束倒计时 {0}", GLobal.TimeFormat(_actInfo.LeftTime, true));
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        UnInitEvent();
        _actInfo = null;
    }
}
