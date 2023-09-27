using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 庆典红包 活动
/// </summary>
public class _Activity_2062_UI : ActivityUI
{
    const float EffectTime = 1f;
    const int DayTabCount = 8;//第8天是领取按钮

    private ActInfo_2062 _actInfo;
    /// <summary>
    /// 选中的天数
    /// </summary>
    private int _selectDay = 1;
    private bool _locked = false;
    ActInfo_2062.ActivityStatus _activityStatus;
    /// <summary>
    /// 活动开始到现在经过的天数，活动开始当天为0
    /// </summary>
    private int _dayOffset = 0;
    /// <summary>
    /// 下一次状态改变的时间戳
    /// </summary>
    private long _nextStatusTs;
    /// <summary>
    /// 界面当前是否打开着
    /// </summary>
    bool _isShow;

    ///////////////UI
    JDText _title;            //活动名
    JDText _endTime;          //倒计时{x日x小时x分x秒}
    JDText _desc;             //活动介绍
    JDText _dayTitle;         //第x天
    JDText _savedAmount;      //已存氪金数量{xx}
    JDText _consumeAmount;    //当日消耗氪晶：xx
    JDText _totalGetAmount;   //累计可领取氪金:xx

    Button _getReward;          //领取按钮
    JDText _gotTip;             //已领取
    GameObject _effect;         //特效

    private TabBtnHelper _tabBtnHelper;
    DayTab[] _dayTabs;
    private ObjectGroup UI;
    Sprite _spButtonNormal;
    Sprite _spButtonSelect;
    Sprite _spButtonUnlock;

    public override void OnCreate()
    {
        InitData();
        InitEvent();
        InitUI();
        //InitListener();
    }

    private void InitUI()
    {
        UI = gameObject.GetComponent<ObjectGroup>();
        _spButtonNormal = UI.Ref<Sprite>("_SpButtonNormal");
        _spButtonSelect = UI.Ref<Sprite>("_SpButtonSelect");
        _spButtonUnlock = UI.Ref<Sprite>("_SpButtonUnlock");

        _title = transform.Find<JDText>("Title");
        _endTime = transform.Find<JDText>("TimeText");
        _desc = transform.Find<JDText>("TextDes");
        _dayTitle = transform.Find<JDText>("Text_Day");
        _savedAmount = transform.Find<JDText>("Text_Save");
        _consumeAmount = transform.Find<JDText>("Text_Consume");
        _totalGetAmount = transform.Find<JDText>("TextTip");
        _getReward = transform.Find<Button>("Button_Get");
        _gotTip = transform.Find<JDText>("Text_Got");

        _tabBtnHelper = new TabBtnHelper();
        _dayTabs = new DayTab[DayTabCount];
        for (int i = 0; i < DayTabCount; i++)
        {
            int day = i + 1;
            Button btn = transform.FindButton(string.Format("TabDays/day{0:D2}", day));
            DayTab dayTab = btn.gameObject.AddBehaviour<DayTab>();
            _dayTabs[i] = dayTab;
            dayTab.SetButtonSpriteRef(_spButtonNormal, _spButtonSelect, _spButtonUnlock);
            dayTab.SetDay(day);
            _tabBtnHelper.RegistTabBtn(dayTab, day);
        }
        _tabBtnHelper.OnTabSwitch += OnDayChanged;

        _getReward.onClick.SetListener(On_getRewardClick);
    }
    private void On_getRewardClick()
    {
        if (_actInfo.GetTotalRewardGold() > 0)
        {
            _actInfo.GetReward();
        }
        else
        {
            Alert.Ok(Lang.Get("没有可以领取的奖励"));
        }
    }
    private void InitEvent()
    {
        _actInfo.GetAct2062Reward.AddListener(OnGetRewardEvent);
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdateGold.AddListener(OnGoldUpdate);
    }

    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdateGold.RemoveListener(OnGoldUpdate);
    }

    private void ClearEventListener()
    {
        base.OnClose();
        _actInfo.GetAct2062Reward.RemoveListener(OnGetRewardEvent);
    }

    public override void OnDestroy()
    {
        ClearEventListener();
        base.OnDestroy();
        if(_tabBtnHelper != null)
        {
            _tabBtnHelper.OnDestroy();
            _tabBtnHelper = null;
        }
    }

    private void OnDayChanged(int lastDay,int curtDay)
    {
        _selectDay = curtDay;
        RefreshUI();
    }

    public override void OnShow()
    {
        _isShow = true;
        _actInfo.RequestRefresh();

        _dayOffset = _actInfo.DaysFromStart();
        _nextStatusTs = _actInfo.NextStatusChangeTs();
        _selectDay = _dayOffset + 1;
        _selectDay = Mathf.Clamp(_selectDay, 1, DayTabCount);
        RefreshUI();
        _tabBtnHelper.ClickBySpId(_selectDay);
    }

    public override void OnClose()
    {
        base.OnClose();
        _isShow = false;
    }

    private void InitData()
    {
        _actInfo = ActInfo_2062.GetData();
    }

    private void RefreshUI()
    {
        if (_locked) { return; }
        _activityStatus = _actInfo.GetStatus();

        _title.text = _actInfo._name;
        _desc.text = _actInfo._desc;
        UpdateTime(TimeManager.ServerTimestamp);
        _totalGetAmount.text = Lang.Get("累计可领取氪晶:{0}", _actInfo.GetTotalRewardGold());

        if (_selectDay<=ActInfo_2062.DayCount)
        {
            //前面7天
            _dayTitle.text = Lang.Get("第{0}天", _selectDay);
            _savedAmount.text = Lang.Get("已存氪晶:{0}", _actInfo.GetSavedGold(_selectDay));
            _consumeAmount.text = Lang.Get("当日消耗氪晶:{0}", _actInfo.GetConsumedGold(_selectDay));
            _getReward.gameObject.SetActive(false);
            _gotTip.gameObject.SetActive(false);
        }
        else
        {
            //第八天这些文本都不显示
            _dayTitle.text = "";
            _savedAmount.text = "";
            _consumeAmount.text = "";

            if (_actInfo.HasGotReward())
            {
                _gotTip.gameObject.SetActive(true);
                _getReward.gameObject.SetActive(false);
            }
            else
            {
                _gotTip.gameObject.SetActive(false);
                _getReward.gameObject.SetActive(true);
                if (_actInfo.GetTotalRewardGold() > 0)
                {
                    _getReward.image.color = _getReward.colors.normalColor;
                }
                else
                {
                    _getReward.image.color = _getReward.colors.disabledColor;
                }
            }
        }

        for (int i = 0; i < DayTabCount; i++)
        {
            _dayTabs[i].RefreshUI(_dayOffset);
        }
    }

    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_actInfo.GetStatus()!=_activityStatus)
        {
            //状态从 累计->领奖
            _activityStatus = _actInfo.GetStatus();
            _dayOffset = _actInfo.DaysFromStart();
            _nextStatusTs = _actInfo.NextStatusChangeTs();
            RefreshUI();
            EventCenter.Instance.RemindActivity.Broadcast(_actInfo._aid, _actInfo.IsAvaliable());
        }
        else
        {
            if (nowTs >= _nextStatusTs)
            {
                _dayOffset = _actInfo.DaysFromStart();
                _nextStatusTs = _actInfo.NextStatusChangeTs();
                RefreshUI();
            }
            else
            {
                if (_activityStatus == ActInfo_2062.ActivityStatus.Waitting)
                {
                    //这个活动的倒计时是RewardTs
                    long leftTime = _actInfo.RewardTs - TimeManager.ServerTimestamp;
                    _endTime.text = GlobalUtils.ActivityLeftTime(leftTime, true);
                    TimeSpan span = TimeSpan.FromSeconds(leftTime);
                    string endTimeString = Lang.Get("{0}日{1}小时{2}分{3}秒后于本页领取。", span.Days, span.Hours, span.Minutes, span.Seconds);
                    _desc.text = string.Format("{0}{1}", _actInfo._desc, endTimeString);
                }
                else //第8天
                {
                    TimeSpan span = TimeSpan.FromSeconds(_actInfo.LeftTime);
                    _endTime.text = Lang.Get("领奖倒计时 {0}日{1}小时{2}分{3}秒", span.Days, span.Hours, span.Minutes, span.Seconds);
                    _desc.text = _actInfo._desc;
                }
            }
        }
    }

    private void OnGetRewardEvent(int getGold)
    {
        PlayGetRewardEffect();
    }

    private async void PlayGetRewardEffect()
    {
        _locked = true;
        _getReward.gameObject.SetActive(false);

        var activity = DialogManager.GetInstanceOfDialog<_D_ActCalendar>();
        Transform target =  activity.EffectTarget;

        if (_effect==null)
        {
            _effect = await ResHelper.LoadInstanceByUniTask("PFB_UI_ring_Trail", transform);
            _effect.AddComponent<EffectOrderUtil>();
            _effect.SetLayerToChildren(Layer.Effects);
        }
        _effect.SetActive(true);
        Transform _effHarvestTail = _effect.transform;
        _effHarvestTail.localScale = Vector3.one;
        _effHarvestTail.position = _getReward.transform.position;
        _effHarvestTail.DOMove(target.position, EffectTime).SetEase(Ease.InOutQuint).OnComplete(() =>
        {
            _effect.SetActive(false);
            //动画结束后刷新下
            _locked = false;
            //_getReward.gameObject.SetActive(true); //RefreshUI会重新设置按钮是否显示
            RefreshUI();
        });
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == _actInfo._aid)
        {
            _dayOffset = _actInfo.DaysFromStart();
            _nextStatusTs = _actInfo.NextStatusChangeTs();
            RefreshUI();
        }
    }

    private void OnGoldUpdate()
    {
        if (_isShow)
        {
            //每次界面打开的时候刷新一下，界面打开的情况下且氪金有变化才重新请求
            _actInfo.RequestRefresh();
        }
    }
}

internal class DayTab : TabBtnBase
{
    static readonly Color TextNormalColor = _ColorConfig.GetColorByRGBA(0xD8FDFFFF);//D8FDFFFF
    static readonly Color TextUnlockColor = _ColorConfig.GetColorByRGBA(0x6AA9CEFF);//6AA9CEFF

    Image _background;
    Image _lock;
    GameObject _lockObj;
    JDText _dayText;
    bool isOpen;

    Sprite _spNormal;
    Sprite _spSelect;
    Sprite _spUnlock;

    public override void Awake()
    {
        base.Awake();
        _lock = transform.Find<Image>("Image_Lock");
        _lockObj = _lock.gameObject;
        _dayText = transform.Find<JDText>("Text");
        _background = gameObject.GetComponent<Image>();
    }

    public void SetDay(int day)
    {
        if (day<=7)
        {
            _dayText.text = Lang.Get("第{0}天",day);
        }
        else
        {
            _dayText.text = Lang.Get("可领取");
        }
    }

    public void SetButtonSpriteRef(Sprite normal,Sprite select,Sprite unlock)
    {
        _spNormal = normal;
        _spSelect = select;
        _spUnlock = unlock;
    }

    public void RefreshUI(int dayFromStart)
    {
        isOpen = (SpId - 1) <= dayFromStart;
        _lockObj.SetActive(!isOpen);
        _dayText.color = isOpen ? TextNormalColor : TextUnlockColor;
        RefreshBackground();
    }

    public override void Select()
    {
        base.Select();
        _background.overrideSprite = _spSelect;
    }

    public override void Unselect()
    {
        base.Unselect();
        _background.overrideSprite = null;
    }

    private void RefreshBackground()
    {
        _background.sprite = isOpen ? _spNormal : _spUnlock;
    }

    public override bool CanBeSelect()
    {
        return isOpen;
    }
}
