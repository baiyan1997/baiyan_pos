using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _CalendarTab_UI : ActivityUI
{
    private Text[] _dateList;
    //private RectTransform _bgTrans;
    private ListView _listView;
    //private Sprite _bg1;
    //private Sprite _bg2;
    //tip界面
    private GameObject _tipMainGo;
    private Button _tipBtn;
    private Text _tittleText;
    private Text _timeText;
    private Text _descText;
    private Transform[] _rewards;
    private Image _icon;
    private RectTransform _goRect;
    private GameObject _rewardParent;
    private HorizontalLayoutGroup _layout;

    private string[] _weekStr = new string[]
    {
        "周日\n{0}/{1}",
        "周一\n{0}/{1}",
        "周二\n{0}/{1}",
        "周三\n{0}/{1}",
        "周四\n{0}/{1}",
        "周五\n{0}/{1}",
        "周六\n{0}/{1}",
    };

    private int _curWeek;
    private long _earlyWeekTs;
    private long _lateWeekTs;
    private ActivityInfo _info;

    public override void Awake()
    {
        _dateList = new Text[]
        {
            transform.Find<Text>("Time/Day7/Text"),
            transform.Find<Text>("Time/Day1/Text"),
            transform.Find<Text>("Time/Day2/Text"),
            transform.Find<Text>("Time/Day3/Text"),
            transform.Find<Text>("Time/Day4/Text"),
            transform.Find<Text>("Time/Day5/Text"),
            transform.Find<Text>("Time/Day6/Text"),
        };

        //_bgTrans = transform.Find<RectTransform>("TodayImg");

        _listView = ListView.Create<CalendarItem>(transform.Find("ScrollView"));

        //tip
        _tipMainGo = transform.Find<GameObject>("Tip/Main");
        _tipBtn = transform.Find<Button>("Tip");

        _rewards = new Transform[]
        {
            transform.Find("Tip/Main/Rewards/Grid/Reward1"),
            transform.Find("Tip/Main/Rewards/Grid/Reward2"),
            transform.Find("Tip/Main/Rewards/Grid/Reward3"),
            transform.Find("Tip/Main/Rewards/Grid/Reward4"),
        };

        _rewardParent = transform.Find<GameObject>("Tip/Main/Rewards");

        _goRect = transform.Find<RectTransform>("Tip/Main/GoBtn");

        _tittleText = transform.Find<Text>("Tip/Main/Title");

        _timeText = transform.Find<Text>("Tip/Main/Text_time");

        _descText = transform.Find<Text>("Tip/Main/Desc/ListView/Viewport/TextDes");

        _icon = transform.Find<Image>("Tip/Main/Icon");

        _layout = transform.Find<HorizontalLayoutGroup>("Tip/Main/Rewards/Grid");
    }

    public override void OnCreate()
    {
        //InitListener();
    }

    public override void InitListener()
    {
        base.InitListener();
        //0点刷新日期
        EventCenter.Instance.ServerRefreshHour.AddListener(RefreshTime);

        //TimeManager.Instance.TimePassSecond += OnTimePass;

        EventCenter.Instance.UpdateAllActivity.AddListener(RefreshActUI);

        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateActivityUI);

        EventCenter.Instance.RemindActivity.AddListener(RemindActivity);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        //0点刷新日期
        EventCenter.Instance.ServerRefreshHour.RemoveListener(RefreshTime);

        //TimeManager.Instance.TimePassSecond -= OnTimePass;

        EventCenter.Instance.UpdateAllActivity.RemoveListener(RefreshActUI);

        //EventCenter.Instance.UpdateActivityUI.RemoveListener(UpdateActivityUI);

        EventCenter.Instance.RemindActivity.RemoveListener(RemindActivity);
    }

    public override void OnShow()
    {
        RectTransform rect = transform.GetComponent<RectTransform>();
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        gameObject.SetActive(true);
        _tipBtn.gameObject.SetActive(false);

        FreshTime();

        RefreshActUI();
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        RefreshActUI();
    }
    private void RemindActivity(int aid, bool show)
    {
        RefreshActUI();
    }
    private void RefreshActUI()
    {
        if (gameObject == null || !gameObject.activeInHierarchy || !gameObject.activeSelf)
            return;

        _listView.Clear();

        //获取活动列表
        var actlist = ActivityManager.Instance.GetActivityList();
        int index = 0;
        for (int i = 0; i < actlist.Count; ++i)
        {
            var actInfo = actlist[i];

            // 中秋活动不显示在活动日历里头
            if (actInfo.IsMidAutumnAct)
            {
                continue;
            }
            
            if (!IsActInTheWeek(actInfo) || actInfo._aid == ActivityID.FirstPay)
                continue;
            index++;
            int aid = actInfo._aid;
            _listView.AddItem<CalendarItem>().Refresh(aid, index, false, () =>
            {
                ShowTip(aid, false);
            });
        }

        //获取预告活动
        actlist = ActivityManager.Instance.GetFutureActivityList();
        for (int i = 0; i < actlist.Count; ++i)
        {
            var actInfo = actlist[i];
            if (!IsActInTheWeek(actInfo) || actInfo._aid == ActivityID.FirstPay)
                continue;
            index++;
            int aid = actInfo._aid;
            _listView.AddItem<CalendarItem>().Refresh(aid, index, true, () =>
            {
                ShowTip(aid, true);
            });
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        _info = null;
        gameObject.SetActive(false);
    }
    private void ShowTip(int aid, bool isFuture)
    {
        _info = isFuture ? ActivityManager.Instance.GetFutureActivityInfo(aid) : ActivityManager.Instance.GetActivityInfo(aid);
        if (_info == null)
        {
            //Debug.LogError($"活动已结束：aid:{aid}, isFuture:{isFuture}");
            Alert.Ok(Lang.Get("活动已结束"));
            return;
        }
        _tipBtn.gameObject.SetActive(true);
        _tittleText.text = _info._name;
        if (aid == 2040)
        {
            _descText.text = Lang.Get("活动期间，处于保护区的资源矿归属于占领保护区星球的势力，只有该势力玩家可以采集，同时玩家之间禁止相互攻击。");
        }
        else if (aid == 2084)
        {
            _descText.text = string.Format(Cfg.Act.GetData(aid).act_desc, ((ActInfo_2084)_info).Lv, ((ActInfo_2084)_info).Buff);
        }
        else
        {
            _descText.text = _info._desc;
        }
        UIHelper.SetImageSprite(_icon, _info._icon);

        _timeText.gameObject.SetActive(_info._no_end == 0);

        UpdateTime(TimeManager.ServerTimestamp);

        _goRect.GetComponentInChildren<Button>().onClick.SetListener(() =>
        {
            DialogManager.GetInstanceOfDialog<_D_ActCalendar>().JumpToActivity(aid);

            OnClose();
        });

        _tipBtn.onClick.SetListener(On_tipBtnClick);

        List<P_Item> showRewards = _info.ShowRewards;

        if (showRewards == null || showRewards.Count < 1)
        {
            _rewardParent.SetActive(false);
            _goRect.anchoredPosition = new Vector2(0, -404);
            _tipMainGo.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 480);
            return;
        }

        _goRect.anchoredPosition = new Vector2(0, -590);
        _tipMainGo.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 666);
        _rewardParent.SetActive(true);
        int count = showRewards.Count;

        for (int i = 0; i < _rewards.Length; ++i)
        {
            if (i < count)
            {
                _rewards[i].gameObject.SetActive(true);

                DefineItem(_rewards[i], showRewards[i]);
            }
            else
            {
                _rewards[i].gameObject.SetActive(false);
            }
        }

        switch (count)
        {
            case 4:
                {
                    _layout.spacing = -30;
                }
                break;
            case 3:
                {
                    _layout.spacing = -100;
                }
                break;
            default:
                {
                    _layout.spacing = -200;
                }
                break;
        }
    }
    private void On_tipBtnClick()
    {
        _tipBtn.gameObject.SetActive(false);
    }

    private void DefineItem(Transform trans, P_Item item)
    {
        Image icon = trans.Find<Image>("Icon");
        Image qua = trans.Find<Image>("Qua");

        ItemForShow itemshow = new ItemForShow(item.id, item.count);
        itemshow.SetIcon(icon);
        qua.color = _ColorConfig.GetQuaColor(itemshow.GetQua());

        trans.GetComponent<Button>().onClick.SetListener(() =>
        {
            DialogManager.ShowAsyn<_D_ItemTip>(d=>{ d?.OnShow(item.id, item.count, trans.position); });
        });
    }

    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_tipBtn == null || !_tipBtn.gameObject.activeSelf || !gameObject.activeSelf)
            return;

        if (_info == null)
            return;

        if (_info._no_end == 1)
            return;

        //特殊处理2062活动
        if (_info._aid == 2062)
        {
            ActInfo_2062 info = (ActInfo_2062)_info;

            if (info.GetStatus() == ActInfo_2062.ActivityStatus.Waitting)
            {
                //这个活动的倒计时是RewardTs
                long lefttime = info.RewardTs - TimeManager.ServerTimestamp;
                _timeText.text = GlobalUtils.ActivityLeftTime(lefttime, true);
            }
            else //第8天
            {
                TimeSpan span = TimeSpan.FromSeconds(info.LeftTime);
                _timeText.text = Lang.Get("领奖倒计时 {0}日{1}小时{2}分{3}秒", span.Days, span.Hours, span.Minutes, span.Seconds);
            }
            return;
        }

        if (st - _info._data.startts < 0)
        {
            _timeText.text = GlobalUtils.GetActivityStartTimeDesc(_info._data.startts);
        }
        else if (_info.LeftTime >= 0)
        {
            _timeText.text = GlobalUtils.ActivityLeftTime(_info.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        _dateList = null;
        _rewards = null;
    }


    //今日固定显示在第三列
    private void FreshTime()
    {
        _curWeek = (int)TimeManager.ServerDateTime.DayOfWeek;
        int curIndex = _curWeek % 7;
        //根据星期和当前时间戳计算出这周的起始 截止时间戳
        var passedTime0 = ((TimeManager.ServerTimestamp + 3600 * TimeManager.svcTimeZone) / 86400) * 86400 - 3600 * TimeManager.svcTimeZone;
        _earlyWeekTs = passedTime0 - 2 * 86400;
        _lateWeekTs = passedTime0 + 5 * 86400;
        for (int i = 0; i < _dateList.Length; i++)
        {
            int index = (i + curIndex + 5) % 7;
            DateTime date = TimeParse.TimeStamp2CertainDate(_earlyWeekTs + i * 86400, TimeManager.svcTimeZone);
            _dateList[i].text = Lang.Get(_weekStr[index], date.Month, date.Day);
        }
    }

    private void RefreshTime(int hour)
    {
        if (gameObject && hour == 0 && gameObject.activeSelf)
            FreshTime();
    }


    private bool IsActInTheWeek(ActivityInfo actInfo)
    {
        var startTs = actInfo._data.startts;
        var endTs = actInfo._data.endts;

        if (startTs >= _lateWeekTs || endTs < _earlyWeekTs)
            return false;
        else
            return true;
    }
}

public class CalendarItem : ListItem
{
    private RectTransform trans;
    private JDText nameText;
    private Image image;
    private Image icon;
    private GameObject tipGo;
    private float addWidth = 102f;
    private float height = 56f;
    private float width = 100f;
    private Action _callback;
    private int _aid;
    private GameObject _effectGo;

    public override void OnCreate()
    {
        trans = transform.Find("PrefGo").GetComponent<RectTransform>();
        nameText = trans.Find<JDText>("Text");
        icon = trans.Find<Image>("Icon");
        image = trans.GetComponent<Image>();
        tipGo = trans.Find<GameObject>("Tip");
        _effectGo = trans.Find<GameObject>("PrefLine");
        trans.GetComponent<Button>().onClick.SetListener(OnTransBtnClick);
    }
    private void OnTransBtnClick()
    {
        if (_callback != null)
            _callback();
    }

    public void Refresh(int aid, int index, bool isFuture, Action callback)
    {
        if (!tipGo)
            return;
        _callback = callback;
        _aid = aid;
        ActivityInfo info = null;

        if (isFuture)
        {
            tipGo.SetActive(false);
            info = ActivityManager.Instance.GetFutureActivityInfo(aid);
        }
        else
        {
            info = ActivityManager.Instance.GetActivityInfo(aid);
            tipGo.SetActive(info.IsAvaliable() || info.DoDailyRemind());
        }

        nameText.text = info._name;
        UIHelper.SetImageSprite(icon, info._icon);

        //根据星期和当前时间戳计算出这周的起始 截止时间戳
        var passedTime = ((TimeManager.ServerTimestamp + 3600 * TimeManager.svcTimeZone) / 86400) * 86400 - 3600 * TimeManager.svcTimeZone;
        long earlyWeekTs = passedTime - 2 * 86400;
        long lateWeekTs = passedTime + 5 * 86400;

        long start = info._data.startts < earlyWeekTs ? earlyWeekTs : info._data.startts;
        long end = info._data.endts > lateWeekTs ? lateWeekTs : info._data.endts;

        //特殊处理2062活动
        if (aid == 2062)
        {
            ActInfo_2062 info2062 = (ActInfo_2062)info;
            if (info2062.GetStatus() == ActInfo_2062.ActivityStatus.Waitting)
            {
                //这个活动的倒计时是RewardTs
                end = info2062.RewardTs > lateWeekTs ? lateWeekTs : info2062.RewardTs;
            }
            else
            {
                end = info._data.endts > lateWeekTs ? lateWeekTs : info._data.endts;
            }
        }

        DateTime datestart = TimeParse.TimeStamp2CertainDate(start, TimeManager.svcTimeZone);
        DateTime dateend = TimeParse.TimeStamp2CertainDate(end - 1, TimeManager.svcTimeZone);
        TimeSpan sp = dateend.Subtract(datestart);
        int deltaDay = (int)sp.TotalHours / 24;
        int m = (int)sp.TotalHours - deltaDay * 24;
        if (datestart.Hour + m >= 24)
        {
            deltaDay += 1;
        }

        //Debug.LogError(info._name + "    " + deltaDay + "   " + datestart.ToLongDateString() + datestart.ToLongTimeString() + "    " + dateend.ToLongDateString() + dateend.ToLongTimeString());

        trans.sizeDelta = new Vector2(width + deltaDay * addWidth, height);
        int curWeek = (int)TimeParse.TimeStamp2CertainDate(start, TimeManager.svcTimeZone).DayOfWeek;
        int todayWeek = (int)TimeManager.ServerDateTime.DayOfWeek % 7;

        trans.anchoredPosition = new Vector2(addWidth * ((curWeek - todayWeek + 9) % 7), trans.anchoredPosition.y);

        int color = Cfg.Act.GetData(aid).color;
        image.color = _ColorConfig.CalendarColor[color];
        nameText.GetComponent<Outline>().effectColor = _ColorConfig.CalendarTextColor[color];

        RectTransform icontrans = icon.GetComponent<RectTransform>();
        RectTransform nameTrans = nameText.GetComponent<RectTransform>();

        if (deltaDay < 1)
        {
            nameText.gameObject.SetActive(false);
            icontrans.anchoredPosition = new Vector2(0, -28);
        }
        else
        {
            nameText.gameObject.SetActive(true);
            if (deltaDay < 2)
            {
                if (info._name.Length > 4)
                    nameText.text = info._name.Substring(0, 4) + "...";
                nameTrans.anchoredPosition = new Vector2(100, -28);
                icontrans.anchoredPosition = new Vector2(28, -28);
            }
            else
            {
                float prewidth = nameText.preferredWidth;
                float fromX = (trans.sizeDelta.x - (72 + prewidth)) / 2;
                icontrans.anchoredPosition = new Vector2(fromX, -28);
                nameTrans.anchoredPosition = new Vector2(72 + fromX, -28);
            }
        }

        if (Cfg.Act.GetData(aid).is_key_act == 1)
        {
            _effectGo.SetActive(true);
            _effectGo.GetComponent<RectTransform>().sizeDelta = trans.sizeDelta;
        }
        else
        {
            _effectGo.SetActive(false);
        }
    }

    private void RefreshTip(int aid, bool remind)
    {
        if (gameObject == null)
            return;

        if (aid == _aid)
        {
            ActivityInfo info = ActivityManager.Instance.GetActivityInfo(_aid);
            if (info == null)
                return;
            tipGo.SetActive(info.IsAvaliable() || info.DoDailyRemind());
        }
    }

    public override void OnAddToList()
    {
        base.OnAddToList();
        EventCenter.Instance.RemindActivity.AddListener(RefreshTip);
    }

    public override void OnRemoveFromList()
    {
        EventCenter.Instance.RemindActivity.RemoveListener(RefreshTip);
        base.OnRemoveFromList();
    }
}
