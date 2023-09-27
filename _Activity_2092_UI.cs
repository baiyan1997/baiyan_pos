using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static System.String;

public class _Activity_2092_UI : ActivityUI
{
    private int _maxNumPoolType1 = 10;//第一阶段保底次数
    private int _maxUpgradeLv = 4;//第二阶段最大强化等级
    private Text _textCountDown;
    private Button _btnManual;
    private ActInfo_2092 _actInfo;
    private Text _transistorNum;//当前拥有的晶体管道具数量 随机雷达或随机雷达技能时消耗
    private Image _transistorIcon;
    private int _transistorId = 70048;
    private Image _radarIcon;
    private Button _btnRadarLock;
    private Button _btnRadarUnlock;
    private Button _btnDraw;//随机按钮
    private Text _btnDrawText;
    private Transform _normalCost;
    private Transform _krCost;
    private _RadarAttrItem[] _radarAttrs;
    private int _radius = 241;
    private GameObject _radarEff1;
    private GameObject _radarEff2;
    private GameObject _flyEff;
    private float _effFlyTime = 0.8f;//飞行特效飞行时间
    private Vector3 _pos1 = new Vector3(0, -443, 0);//随机按钮位置
    private Vector3 _pos2 = new Vector3(0, 0, 0);//雷达图标位置
    private Text _textMainAttr;
    private Text _extraAttrTips;
    private Tween _flyEffTween;//飞行特效动画
    private Sequence _sequence;//动画序列
    private int _coroutineState;//0 初始状态 1 开始状态  2结束状态
    private Text _radarDesc;
    private _D_Tips_HelpDesc _helpDialog;
    private Slider _slider;
    private Button _btnPool1;
    private Button _btnPool2;
    private Button _btnPool3;
    private RectTransform _sliderTip;
    private float _sliderAnimTime = 0.6f;//进度条变化动画时间
    private bool _sliderAnimDone;
    private GameObject _redPoint;//抽取按钮红点
    private GameObject _redPointExchange;//兑换按钮红点
    private Button _btnExchange;
    private Text _tipText;

    public override void OnCreate()
    {
        InitRef();
        InitEvent();
        //InitListener();
    }

    private void InitEvent()
    {
        _btnManual.onClick.AddListener(On_btnManualClick);
        _btnDraw.onClick.AddListener(On_btnDrawClick);
        _btnRadarLock.onClick.AddListener(LockRadar);
        _btnRadarUnlock.onClick.AddListener(UnlockRadar);
        _actInfo.OnStateChanged = OnStateChanged;
        _actInfo.OnPoolTypeChanged = OnPoolTypeChanged;
        _btnPool1.onClick.AddListener(On_btnPool1Click);
        _btnPool2.onClick.AddListener(On_btnPool2Click);
        _btnPool3.onClick.AddListener(On_btnPool3Click);
        _btnExchange.onClick.AddListener(On_btnExchangeClick);
    }
    private void On_btnManualClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnManualDialogShowAsynCB);
    }
    private void On_btnManualDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2092, _btnManual.transform.position, Direction.LeftDown, 350);
    }
    private void On_btnDrawClick()
    {
        if (CheckAnimIsPlaying())
        {
            return;
        }

        if (!_actInfo.CheckPropEnough())
        {
            var need = Cfg.Act2092.GetCostGoldNum(_actInfo.UniqueInfo.pool_type, _actInfo.State);
            var alert = Alert.YesNo(Lang.Get("晶体管不足，是否消耗{0}氪晶抽取?", need));
            alert.SetYesCallback(() =>
            {
                if (!ItemHelper.IsCountEnough(ItemId.Gold, need))
                {
                    alert.Close();
                    return;
                }
                DoDraw(2);
                alert.Close();
            });
        }
        else
        {
            DoDraw(1);
        }
    }
    private void On_btnExchangeClick()
    {
        DialogManager.ShowAsyn<_D_2092Exchange>(On_btnExchangeDialogShowAsynCB);
        HideAllExtraAttrEffs();
    }
    private void On_btnExchangeDialogShowAsynCB(_D_2092Exchange d)
    {
        d?.OnShow(RefreshExchangeRedPoint);
    }
    private void On_btnPool1Click()
    {
        OnClickPoolBtn(1);
    }
    private void On_btnPool2Click()
    {
        OnClickPoolBtn(2);
    }
    private void On_btnPool3Click()
    {
        OnClickPoolBtn(3);
    }


    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdatePlayerItem);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdatePlayerItem);
    }

    private void UpdatePlayerItem()
    {
        RefreshCostNumInBag();
        RefreshCost();
        RefreshRedPoint();
        RefreshExchangeRedPoint();
    }

    private void OnClickPoolBtn(int type)
    {
        var tip = "";
        switch (type)
        {
            case 1:
                tip = Lang.Get("必出金色品质雷达");
                break;
            case 2:
                tip = Lang.Get("随机雷达必出红色雷达,且有概率强化雷达等级至最高+4");
                break;
            case 3:
                tip = Lang.Get("必出红色+4品质以上雷达，开启雷达锁定功能");
                break;
        }

        Alert.Ok(tip);
    }
    private void DoDraw(int drawType)
    {
        switch (_actInfo.State)
        {
            case 0:
            case 1:
                _actInfo.DrawRadar(OnDrawRadar, drawType);
                break;
            case 2:
                _actInfo.DrawRadar(OnDrawExtraAttr, drawType);
                break;
        }
    }
    private void RefreshRedPoint()
    {

        _redPoint.SetActive(_actInfo.CheckPropEnough());
        EventCenter.Instance.RemindActivity.Broadcast(2092, _actInfo.IsAvaliable());
    }

    private void OnPoolTypeChanged()
    {
        RefreshDrawBtn();
        RefreshTipText();
        RefreshAttrTip();
        MessageManager.Show(Lang.Get("恭喜！奖池阶段已提升！"));
    }

    private void OnStateChanged()
    {
        RefreshDrawBtn();
        RefreshTipText();
    }

    private void RefreshTipText()
    {
        string str = Empty;
        switch (_actInfo.State)
        {
            case 0:
            case 1:
                str = _actInfo.UniqueInfo.pool_type == 2 ? Lang.Get("随机雷达同时可提升强化等级,+4后进入下一阶段") : Lang.Get("活动结束后当前雷达会发放至雷达站");
                break;
            case 2:
                str = Lang.Get("活动结束后当前雷达会发放至雷达站");
                break;
        }
        _tipText.text = str;
    }


    private void UnlockRadar()
    {
        _actInfo.SetRadarLock(OnUnlockRadarCB);
    }
    private void OnUnlockRadarCB()
    {
        SetRadarLockBtn(_actInfo.UniqueInfo.radar_lock == 1);
    }
    private void LockRadar()
    {
        _actInfo.SetRadarLock(OnLockRadarCB);
    }
    private void  OnLockRadarCB()
    {
        SetRadarLockBtn(_actInfo.UniqueInfo.radar_lock == 1);
    }
    private void OnDrawExtraAttr()
    {
        MaskExtraAttrWithAnim();
        ShowDrawExtraAttrEff();
    }

    private void ShowDrawExtraAttrEff()
    {
        ShowFlyEff(OnShowFlyEffCB);
    }
    private void OnShowFlyEffCB()
    {
        _flyEff.SetActive(false);
        string extraAttrGroup = Cfg.Radar.GetData(_actInfo.UniqueInfo.radar_id).extra_attr_group;
        List<int> extraAttrIds = Cfg.RadarAttribute.SortOrder(extraAttrGroup.Split(','));
        int len = extraAttrIds.Count;
        if (_sequence != null && _sequence.IsPlaying())
        {
            _sequence.Complete(true);
        }
        _sequence = DOTween.Sequence();
        _sequence.OnComplete(SetRadarTips);
        for (int i = 0; i < len; i++)
        {
            P_RadarExtraAttr attr = _actInfo.RadarExtraAttrDic[extraAttrIds[i]];
            var isLock = attr.qua == 0;
            var qua = isLock ? Cfg.RadarAttribute.GetDataById(attr.extra_id).min_quality : attr.qua;
            _radarAttrs[i].OnDrawExtraAttr(isLock, attr.extra_id, qua, _sequence);
        }
    }

    private void MaskExtraAttrWithAnim()
    {
        int len = _actInfo.UniqueInfo.extra_attr.Count;
        if (_sequence != null && _sequence.IsPlaying())
        {
            _sequence.Complete(true);
        }
        _sequence = DOTween.Sequence();
        for (int i = 0; i < len; i++)
        {
            _radarAttrs[i].MaskWithAnim(_sequence);
        }
    }

    private void OnDrawRadar()
    {
        ShowOrHideRadar(false);
        HideAllExtraAttr();
        ShowFlyEff(OnDrawRadarShowFlyEffCB);
    }
    private void OnDrawRadarShowFlyEffCB()
    {
        _radarEff1.SetActive(false);
        _radarEff1.SetActive(true);
        _radarEff2.SetActive(false);
        _radarEff2.SetActive(true);
        StartCoroutine(DelayDoDrawRadarAnim());
    }
    //展示抽奖飞行特效 从抽奖按钮射出一道光线到雷达中心
    private void ShowFlyEff(Action onComplete)
    {
        _flyEff.transform.localPosition = _pos1;
        _flyEff.SetActive(true);
        _flyEffTween = _flyEff.transform.DOLocalMove(_pos2, _effFlyTime).SetEase(Ease.InQuart).OnComplete(() =>
          {
              onComplete?.Invoke();
          });
    }
    private IEnumerator DelayDoDrawRadarAnim()
    {
        _coroutineState = 1;
        yield return new _WaitForSeconds(0.5f);
        _coroutineState = 2;
        HideRadarEffs();
        RefreshUi(true);
    }

    private void HideRadarEffs()
    {
        _radarEff1.SetActive(false);
        _radarEff2.SetActive(false);
        _flyEff.SetActive(false);
    }

    private void HideAllExtraAttrEffs()
    {
        int len = _actInfo.UniqueInfo.extra_attr.Count;
        for (int i = 0; i < len; i++)
        {
            _radarAttrs[i].HideEffs();
        }
    }
    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (nowTs - _actInfo._data.startts < 0)
        {
            _textCountDown.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            _textCountDown.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _textCountDown.text = Lang.Get("活动已经结束");
        }
    }

    private void InitRef()
    {
        _textCountDown = transform.FindText("CountDown");
        _btnManual = transform.FindButton("_btnManual");
        _transistorNum = transform.FindText("Main/Inf_01/cost1num/Text");
        _redPointExchange = transform.Find("Main/Inf_01/RedPoint").gameObject;
        _btnExchange = transform.FindButton("Main/Inf_01");
        _radarIcon = transform.FindImage("Main/Img_radar");
        _btnRadarLock = transform.FindButton("Main/Btn_lock");
        _btnRadarUnlock = transform.FindButton("Main/Btn_unlock");
        _btnDraw = transform.FindButton("Btn_Draw");
        _redPoint = _btnDraw.transform.Find("RedPoint").gameObject;
        _btnDrawText = _btnDraw.GetComponentInChildren<Text>();
        _normalCost = _btnDraw.transform.Find("cost1");
        _krCost = _btnDraw.transform.Find("cost2");
        UIHelper.SetImageSprite(_normalCost.GetComponent<Image>(), GetIconPath(_transistorId));
        _transistorIcon = _transistorNum.transform.GetComponentInParent<Image>();
        UIHelper.SetImageSprite(_transistorIcon, GetIconPath(_transistorId));

        _radarAttrs = new[]
        {
            new _RadarAttrItem(transform.Find("Main/01 (0)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (1)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (2)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (3)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (4)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (5)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (6)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (7)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (8)"),true),
            new _RadarAttrItem(transform.Find("Main/01 (9)"),true)
        };
        _radarEff1 = transform.Find("Main/PFB_Main_Eff").gameObject;
        _radarEff2 = transform.Find("Main/PFB_Main2_Eff").gameObject;
        _flyEff = transform.Find("Main/PFB_FlyLight_Eff").gameObject;
        _radarDesc = transform.FindText("Main/Text_radarDesc");
        HideRadarEffs();
        _textMainAttr = transform.FindText("Main/Text_mainAttr");
        _extraAttrTips = transform.FindText("Main/Text_extraAttrTips");
        _slider = transform.Find("Slider").GetComponent<Slider>();
        _btnPool1 = transform.FindButton("Slider/Image/Button1");
        _btnPool2 = transform.FindButton("Slider/Image/Button2");
        _btnPool3 = transform.FindButton("Slider/Image/Button3");
        _tipText = transform.FindText("TipText");
        _sliderTip = transform.Find("Slider/Text").GetComponent<RectTransform>();
        _actInfo = ActivityManager.Instance.GetActivityInfo(2092) as ActInfo_2092;
    }


    private string GetIconPath(int id)
    {
        return "Icon/" + Cfg.Item.GetItemData(id).icon2;
    }

    public override void OnShow()
    {
        _actInfo = ActivityManager.Instance.GetActivityInfo(2092) as ActInfo_2092;
        UpdateTime(TimeManager.ServerTimestamp);
        RefreshTipText();
        RefreshDrawBtn();
        RefreshCostNumInBag();
        RefreshUi();
        RefreshExchangeRedPoint();
    }

    private void RefreshUi(bool refresh = false)
    {
        RefreshRadar();
        RefreshExtraAttr();
        RefreshSlider(refresh);
    }

    private void RefreshExchangeRedPoint()
    {
        _redPointExchange.SetActive(_actInfo.CheckCanExchange() && _actInfo.Tag);
        EventCenter.Instance.RemindActivity.Broadcast(2092, _actInfo.IsAvaliable());
    }

    private void RefreshSlider(bool refresh = false)
    {
        if (_slider.value >= 1)
        {
            return;
        }
        _sliderAnimDone = true;
        float value = 0;
        if (_actInfo.UniqueInfo.pool_type == 1)
        {
            value = 0.5f * ((float)_actInfo.UniqueInfo.num / _maxNumPoolType1);
        }
        else if (_actInfo.UniqueInfo.pool_type == 2)
        {
            float upgradeLv = Cfg.Radar.GetQuailty(_actInfo.UniqueInfo.radar_id) <= 4
                ? 0f
                : _actInfo.UniqueInfo.upgrade_lv;
            value = 0.5f * (upgradeLv / _maxUpgradeLv) + 0.5f;
        }
        else if (_actInfo.UniqueInfo.pool_type == 3)
        {
            value = 1f;
        }
        else
        {
            throw new Exception($"error poo_type{_actInfo.UniqueInfo.pool_type},pool_type should be 1 0r 2 0r 3");
        }

        float posY = _sliderTip.anchoredPosition.y;
        if (refresh)
        {
            _sliderAnimDone = false;
            float curtValue = _slider.value;
            DOTween.To(() => curtValue, x => _slider.value = x, value, _sliderAnimTime).OnUpdate(() =>
            {
                _sliderTip.anchoredPosition = new Vector2(_slider.value * 592, posY);
            }).OnComplete(() => { _sliderAnimDone = true; });
            return;

        }

        _slider.value = value;
        _sliderTip.anchoredPosition = new Vector2(value * 592, posY);
    }



    private void RefreshCostNumInBag()
    {
        _transistorNum.text = BagInfo.Instance.GetItemCount(_transistorId).ToString();
    }

    private void RefreshExtraAttr()
    {
        HideAllExtraAttrEffs();
        if (_actInfo.State == 0)
        {
            HideAllExtraAttr();
            return;
        }
        string extraAttrGroup = Cfg.Radar.GetData(_actInfo.UniqueInfo.radar_id).extra_attr_group;
        List<int> extraAttrIds = Cfg.RadarAttribute.SortOrder(extraAttrGroup.Split(','));
        int slot = extraAttrIds.Count;
        RefreshActiveSlot(slot);
        RefreshSlotInfo(extraAttrIds);
        DoAttrMove(slot);
    }

    private void HideAllExtraAttr()
    {
        for (int i = 0; i < 10; i++)
        {
            _radarAttrs[i].SetVisible(false);
        }
    }

    private void DoAttrMove(int slot)
    {
        if (_sequence != null && _sequence.IsPlaying())
        {
            _sequence.Complete(true);
        }
        _sequence = DOTween.Sequence();
        Tween[] tweens = new Tween[slot];
        float angle = 360.0f / slot;
        float x;
        float y;
        for (int i = 0; i < slot; i++)
        {
            float hudu = (90 - angle * i) / 180 * Mathf.PI;
            x = _radius * Mathf.Cos(hudu);
            y = _radius * Mathf.Sin(hudu);
            tweens[i] = _radarAttrs[i].transform.DOLocalMove(new Vector3(x, y, 0), 0.1f);
            _sequence.Append(tweens[i]);
        }
    }
    private void RefreshSlotInfo(List<int> extraAttrIds)
    {
        int len = extraAttrIds.Count;
        for (int i = 0; i < len; i++)
        {
            int id = extraAttrIds[i];
            _actInfo.RadarExtraAttrDic.TryGetValue(id, out P_RadarExtraAttr extraAttr);
            if (extraAttr == null)
            {
                throw new Exception($"the extra_attr data is wrong,the id{extraAttrIds[i]} is not belong to this radar");
            }

            var isLock = extraAttr.qua == 0;
            var qua = isLock ? Cfg.RadarAttribute.GetDataById(id).min_quality : extraAttr.qua;
            _radarAttrs[i].Refresh(isLock, id, qua);
        }
    }

    private void RefreshActiveSlot(int slot)
    {
        if (slot > 10)
            throw new Exception("slot num can't bigger than 10");
        for (int i = 0; i < slot; i++)
        {
            _radarAttrs[i].transform.localPosition = Vector3.zero;
            _radarAttrs[i].SetVisible(true);
        }
        for (int i = slot; i < 10; i++)
        {
            _radarAttrs[i].SetVisible(false);
        }
    }

    private void RefreshDrawBtn()
    {
        switch (_actInfo.State)
        {
            case 0:
            case 1:
                _btnDrawText.text = Lang.Get("随机雷达");
                break;
            case 2:
                _btnDrawText.text = Lang.Get("随机技能");
                break;
        }

        RefreshCost();
        RefreshRedPoint();
    }

    private void RefreshCost()
    {
        bool enough = _actInfo.CheckPropEnough();
        _normalCost.gameObject.SetActive(enough);
        _krCost.gameObject.SetActive(!enough);
        _krCost.GetComponentInChildren<Text>().text =
            Cfg.Act2092.GetCostGoldNum(_actInfo.UniqueInfo.pool_type, _actInfo.State).ToString();
        int need = Cfg.Act2092.GetCostNum(_actInfo.UniqueInfo.pool_type, _actInfo.State);
        _normalCost.GetComponentInChildren<Text>().text = need.ToString();
    }

    private void RefreshRadar()
    {
        if (_actInfo.State == 0)
        {
            ShowOrHideRadar(false);
            return;
        }
        ShowOrHideRadar(true);
        int radarId = _actInfo.UniqueInfo.radar_id;
        Cfg.Radar.SetIcon2(_radarIcon, radarId);
        SetRadarTips();
        SetRadarLockBtn(_actInfo.UniqueInfo.radar_lock == 1);
    }

    private void SetRadarTips()
    {
        var info = _actInfo.UniqueInfo;
        _textMainAttr.text = Format("<Color=#ffff66ff>{0}</Color>",
            Cfg.RadarAttribute.GetMainDescByLv(info.main_attr, info.upgrade_lv));
        _radarDesc.text = Cfg.Radar.GetName(info.radar_id) + Constans.ReformNum[info.upgrade_lv];
        _radarDesc.color = _ColorConfig.GetQuaColor(Cfg.Radar.GetQuailty(info.radar_id));
        RefreshAttrTip();
    }

    private void RefreshAttrTip()
    {
        _extraAttrTips.gameObject.SetActive(true);
        var str = Empty;
        if (_actInfo.UniqueInfo.pool_type == 1)
        {
            _extraAttrTips.gameObject.SetActive(false);
        }
        else if (_actInfo.UniqueInfo.pool_type == 2 && _actInfo.UniqueInfo.upgrade_lv < 2)
        {
            str = Lang.Get("强化至+2技能数量增加");
        }
        else if (_actInfo.UniqueInfo.pool_type == 2 && _actInfo.UniqueInfo.upgrade_lv >= 2)
        {
            str = Lang.Get("强化至+4技能数量增加");
        }
        else if (_actInfo.UniqueInfo.pool_type == 3)
        {
            str = Lang.Get("此雷达最多随机5个技能");
        }

        _extraAttrTips.text = str;
    }

    private void SetRadarLockBtn(bool isLock)
    {
        _btnRadarUnlock.gameObject.SetActive(isLock && _actInfo.UniqueInfo.pool_type == 3);
        _btnRadarLock.gameObject.SetActive(!isLock && _actInfo.UniqueInfo.pool_type == 3);
        //RefreshAttrTip();
    }

    private void ShowOrHideRadar(bool show)
    {
        _radarIcon.gameObject.SetActive(show);
        _btnRadarUnlock.gameObject.SetActive(show && _actInfo.UniqueInfo.pool_type == 3);
        _btnRadarLock.gameObject.SetActive(show && _actInfo.UniqueInfo.pool_type == 3);
        _textMainAttr.gameObject.SetActive(show);
        _extraAttrTips.gameObject.SetActive(show);
        _radarDesc.gameObject.SetActive(show);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        //TimeManager.Instance.TimePassSecond -= RefreshTime;
        _actInfo.OnStateChanged = null;
        _actInfo.OnPoolTypeChanged = null;
        if (_sequence != null)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        HideAllExtraAttrEffs();
        _flyEffTween = null;
        _sequence = null;
        _coroutineState = 0;
        _helpDialog?.Close();
    }

    private bool CheckAnimIsPlaying()
    {
        bool tag1 = false, tag2 = false;
        if (_flyEffTween != null)
        {
            tag1 = _flyEffTween.IsPlaying();
        }

        if (_sequence != null)
        {
            tag2 = _sequence.IsPlaying();
        }

        return tag1 || tag2 || _coroutineState == 1 || !_sliderAnimDone;
    }
}
