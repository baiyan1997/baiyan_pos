using UnityEngine;
using UnityEngine.UI;

public class _Activity_2068_UI : ActivityUI
{
    private JDText _textCountDown;
    private ScrollRect _scrollView;
    private GameObject _objShowMore;
    private Button _btnShowMore;
    private Button _btnEnterAct;
    private JDText _textDesc;
    public ObjectGroup UI;

    private int _aid;
    private long _startts;
    private long _endts;

    public override void OnCreate()
    {
        UI = transform.GetComponent<ObjectGroup>();
        _textCountDown = UI.Get<JDText>("Text_time");
        _scrollView = UI.Get<ScrollRect>("Scroll View");
        _objShowMore = UI.Get<GameObject>("_btnNextPage");
        _btnShowMore = UI.Get<Button>("_btnNextPage");
        _btnEnterAct = transform.Find<Button>("BtnEnter");
        _textDesc = transform.Find<JDText>("Scroll View/Viewport/_txtMainDesc");

        //TimeManager.Instance.TimePassSecond += UpdateTime;

        //EventCenter.Instance.BHBattleOnPushState.AddListener(RefreshButton);

        _scrollView.onValueChanged.AddListener(OnSVValueChanged);
        _btnShowMore.onClick.AddListener(On_btnShowMoreClick);
        _btnEnterAct.onClick.AddListener(On_btnEnterActClick);
        _aid = 2068;

        //InitListener();
    }
    private void OnSVValueChanged(Vector2 vec)
    {
        //滚动时判断是否滚到底 滚到底部就不显示
        _objShowMore.SetActive(_scrollView.verticalNormalizedPosition > 0.05f);
    }
    private void On_btnShowMoreClick()
    {
        _scrollView.verticalNormalizedPosition = 0;//滑动列表移动到底
    }
    private void On_btnEnterActClick()
    {
        switch (_BHB_STATUS.Inst.GetStep())
        {
            case BLACKHOLE_STEP.BATTLE:
                DialogManager.ShowAsyn<_D_BlackHoleBattle>(d => { d?.OnShow(); });//黑洞战
                break;
            case BLACKHOLE_STEP.CELEBRATION:
                DialogManager.ShowAsyn<_D_BlackHoleBattle_Banquet>(d => { d?.OnShow(); });//庆功宴
                break;
            default:
                break;
        }
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.BHBattleOnPushState.AddListener(RefreshButton);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.BHBattleOnPushState.RemoveListener(RefreshButton);
    }

    public override void OnShow()
    {
        var actInfo = ActivityManager.Instance.GetActivityInfo(_aid);
        if (actInfo == null)
            actInfo = ActivityManager.Instance.GetFutureActivityInfo(_aid);
        if (actInfo == null)
            return;
        var actData = Cfg.Act.GetData(_aid);
        _textDesc.text = string.IsNullOrEmpty(actData.pre_act) ? actData.act_desc : actData.pre_act;
        _startts = actInfo._data.startts;
        _endts = actInfo._data.endts;
        _scrollView.verticalNormalizedPosition = 1;//滑动列表移动到顶部
        UpdateTime(0);

        RefreshButton();
    }

    private void RefreshButton()
    {
        switch (_BHB_STATUS.Inst.GetStep())
        {
            case BLACKHOLE_STEP.BATTLE:
                _btnEnterAct.GetComponentInChildren<Text>().text = Lang.Get("进入");
                UIHelper.SetImageSprite(_btnEnterAct.GetComponent<Image>(),"btn_331");
                //黑洞战
                break;
            case BLACKHOLE_STEP.CELEBRATION:
                _btnEnterAct.GetComponentInChildren<Text>().text = Lang.Get("参加宴会");
                UIHelper.SetImageSprite(_btnEnterAct.GetComponent<Image>(),"btn_332");
                //庆功宴
                break;
            default:
                break;
        }
    }

    public override void UpdateTime(long time)
    {
        base.UpdateTime(time);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        var leftTime = _startts - TimeManager.ServerTimestamp;
        if (leftTime < 0)
            leftTime = 0;
        if (leftTime == 0)
        {
            _btnEnterAct.gameObject.SetActive(true);
            _textCountDown.gameObject.SetActive(false);
        }
        else
        {
            _btnEnterAct.gameObject.SetActive(false);
            _textCountDown.gameObject.SetActive(true);
            _textCountDown.text = Lang.Get("开启倒计时 {0}", GLobal.TimeFormat(leftTime, true));
        }
    }


}
