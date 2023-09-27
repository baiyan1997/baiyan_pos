using UnityEngine.UI;

public class _Activity_Notice : JDBehaviour
{
    private JDText _textActName;
    private JDText _textCountDown;
    private JDText _textDesc;
    private Button _btnShowMore;
    private ScrollRect _scrollView;

    private int _aid;
    private long _startts;
    private long _endts; 

    public override void Awake()
    {
        _textActName = transform.Find<JDText>("Name");
        _textCountDown = transform.Find<JDText>("Text_time");
        _textDesc = transform.Find<JDText>("Scroll View/Viewport/_txtMainDesc");
        _scrollView = transform.Find<ScrollRect>("Scroll View");
        _btnShowMore = transform.Find<Button>("_btnNextPage");

        //TimeManager.Instance.TimePassSecond += UpdateTime;

        InitListener();
    }

    public void InitListener()
    {
        _btnShowMore.onClick.AddListener(ClickBtnShowMore);
        _scrollView.onValueChanged.AddListener(ScrollViewChange);
        TimeManager.Instance.TimePassSecond += UpdateTime;
    }

    public void UnInitListener()
    {
        _btnShowMore.onClick.RemoveListener(ClickBtnShowMore);
        _scrollView.onValueChanged.RemoveListener(ScrollViewChange);
        TimeManager.Instance.TimePassSecond -= UpdateTime;
    }

    private void ScrollViewChange(UnityEngine.Vector2 vec)
    {
        //滚动时判断是否滚到底 滚到底部就不显示
        _btnShowMore.gameObject.SetActive(_scrollView.verticalNormalizedPosition > 0.05f);
    }

    private void ClickBtnShowMore()
    {
        _scrollView.verticalNormalizedPosition = 0;//滑动列表移动到底
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnInitListener();
    }

    public void Show(int aid)
    {
        _aid = aid;
        var actInfo = ActivityManager.Instance.GetActivityInfo(_aid);
        if (actInfo == null)
            actInfo = ActivityManager.Instance.GetFutureActivityInfo(_aid);
        if (actInfo == null)
            return;
        gameObject.SetActive(true);
        _startts = actInfo._data.startts;
        _endts = actInfo._data.endts;
        _scrollView.verticalNormalizedPosition = 1;//滑动列表移动到顶部
        _textActName.text = actInfo._name;
        _textDesc.text = string.IsNullOrEmpty(actInfo._pre_act) ? actInfo._desc : actInfo._pre_act;
        UpdateTime(0);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateTime(long time)
    {
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        var leftTime = _startts - TimeManager.ServerTimestamp;
        if (leftTime < 0)
            leftTime = 0;
        _textCountDown.text = Lang.Get("开启倒计时 {0}", GLobal.TimeFormat(leftTime, true));
    }
}
