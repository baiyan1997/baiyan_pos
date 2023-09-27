using System;
using UnityEngine.UI;
using UnityEngine;

public class _Activity_2042_UI : ActivityUI
{
    //private Text _tittleText;

    private Text _timeText;

    private Text _btnText;

    private Text _descText;

    private Text _dcText;

    private Text _tipText;

    private Text _countText;

    private Text _priceText;

    private Button _getBtn;

    private Button _dcBtn;

    private ActInfo_2042 _actInfo;
    private int _aid = 2042;
    private bool _isFresh = false;

    public override void OnCreate()
    {
        InitData();
        InitEvent();
        //InitListener();
        //UpdateUI(_aid);
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2042)ActivityManager.Instance.GetActivityInfo(_aid);

        //_tittleText = transform.Find<JDText>("TextTittle");
        _timeText = transform.Find<JDText>("Text_Time");
        _descText = transform.Find<Text>("TextDesc");
        _dcText = transform.Find<JDText>("DCText");
        _tipText = transform.Find<JDText>("TipText");
        _countText = transform.Find<JDText>("CountText");
        _priceText = transform.Find<JDText>("DCButton/PriceText");
        _getBtn = transform.Find<Button>("GetButton");
        _dcBtn = transform.Find<Button>("DCButton");
    }

    private void InitEvent()
    {
        _getBtn.onClick.SetListener(On_getBtnClick);
        _dcBtn.onClick.SetListener(On_dcBtnClick);
    }
    private void On_getBtnClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2065);
        _actInfo.GetReward();
    }
    private void On_dcBtnClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

        int gold = Uinfo.Instance.Player.Info.ugold;
        if (_actInfo.Price <= gold)
        {
            _actInfo.RequestCleanDcTime();
        }
        else
        {
            DialogManager.ShowAsyn<_D_JumpConfirm>(On_dcBtnDialogShowAsynCB);
        }
    }
    private void On_dcBtnDialogShowAsynCB(_D_JumpConfirm d)
    {
        d?.OnShow(JumpType.Kr, _actInfo.Price);
    }
    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUI);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        _actInfo = (ActInfo_2042)ActivityManager.Instance.GetActivityInfo(_aid);

        if (_actInfo == null)
            return;

        if (aid != _actInfo._data.aid)
            return;

        if (gameObject == null)
            return;

        //_tittleText.text = _actInfo._name;
        _descText.text = _actInfo._desc;

        if (_actInfo._leftDcTime < 0)
        {
            _dcText.gameObject.SetActive(false);
            _dcBtn.gameObject.SetActive(false);

            if (_actInfo.Count > 0)
            {
                _getBtn.gameObject.SetActive(true);
                _countText.gameObject.SetActive(true);
                _countText.text = string.Format(Lang.Get("今日还可领取{0}个"), _actInfo.Count);
                _tipText.gameObject.SetActive(false);
            }
            else
            {
                _getBtn.gameObject.SetActive(false);
                _countText.gameObject.SetActive(false);
                _tipText.gameObject.SetActive(true);
            }
        }
        else
        {
            _countText.gameObject.SetActive(false);
            _getBtn.gameObject.SetActive(false);

            if (_actInfo.Count > 0)
            {
                _dcBtn.gameObject.SetActive(true);
                _dcText.gameObject.SetActive(true);
                _tipText.gameObject.SetActive(false);
                _priceText.text = _actInfo.Price.ToString();
            }
            else
            {
                _dcBtn.gameObject.SetActive(false);
                _dcText.gameObject.SetActive(false);
                _tipText.gameObject.SetActive(true);
            }
        }

        _isFresh = true;
    }


    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy || _actInfo == null)
            return;
        if (_timeText != null)
        {
            if (stamp - _actInfo._data.startts < 0)
            {
                _timeText.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
            }
            else if (_actInfo.LeftTime >= 0)
            {
                TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
                _timeText.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                    span.Minutes, span.Seconds);
            }
            else
            {
                _timeText.text = Lang.Get("活动已经结束");
            }
        }

        //ac time
        if (_dcText != null)
        {
            long leftTime = _actInfo._leftDcTime;

            if (leftTime < 0 && !_isFresh)
            {
                ActivityManager.Instance.RequestUpdateActivityById(_actInfo._aid);
                _isFresh = true;
            }
            else if (leftTime >= 0)
            {
                _dcText.text = string.Format(Lang.Get("{0}后可再领{1}个红包"), GLobal.TimeFormat(leftTime, true), _actInfo.Count);
                _isFresh = false;
            }
        }
    }

    public override void OnShow()
    {
        UpdateUI(_aid);
    }

}
