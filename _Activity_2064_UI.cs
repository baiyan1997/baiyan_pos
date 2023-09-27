using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class _Activity_2064_UI : ActivityUI
{
    private Text _txtTime;
    private Text _txtDesc;
    private Text _txtCount;

    private Button _btnRef;
    private Text _txtBtnRef;
    private Button _btnSel;
    private Button _btnAtkBoss;//讨伐海盗
    private _Act2064RewardItem[] _rewardItems;
    private _Act2064Tips _Tips;

    private Sequence _tweenSeq;

    //位置偏离变量
    private const int HorOffset = 152;
    private const int VerOffset = -154;

    private ActInfo_2064 _info;

    public override void OnCreate()
    {
        _info = ActivityManager.Instance.GetActivityInfo(2064) as ActInfo_2064;

        _txtTime = transform.Find<JDText>("Text_Time");
        _txtDesc = transform.Find<JDText>("Text_Desc");
        _txtCount = transform.Find<JDText>("TextCount");

        _btnRef = transform.Find<Button>("Main/BtnRef");
        _txtBtnRef = transform.Find<JDText>("Main/BtnRef/Text");
        _btnSel = transform.Find<Button>("Main/BtnSel");
        _btnAtkBoss = transform.Find<Button>("Main/BtnAtkBoss");

        _Tips = new _Act2064Tips(transform.Find("_Tips"), _info, SetRewardFalse);

        _rewardItems = new[]
        {
            new _Act2064RewardItem(transform.Find("Main/Icon/01")),
            new _Act2064RewardItem(transform.Find("Main/Icon/02")),
            new _Act2064RewardItem(transform.Find("Main/Icon/03")),
            new _Act2064RewardItem(transform.Find("Main/Icon/04")),
            new _Act2064RewardItem(transform.Find("Main/Icon/05")),
            new _Act2064RewardItem(transform.Find("Main/Icon/06")),
            new _Act2064RewardItem(transform.Find("Main/Icon/07")),
            new _Act2064RewardItem(transform.Find("Main/Icon/08")),
            new _Act2064RewardItem(transform.Find("Main/Icon/09"))
        };

        InitActInfo();
        //TimeManager.Instance.TimePassSecond += RefreshTime;

        _btnSel.onClick.AddListener(OpenTip);
        _btnRef.onClick.AddListener(On_btnRefClick);
        //打开讨伐战入口
        _btnAtkBoss.onClick.AddListener(On_btnAtkBossClick);
        InitListener();
    }
    private void On_btnRefClick()
    {
        //已确定奖励走刷新逻辑 未确定走确定逻辑
        if (_info.Confirm == 1)
        {
            var d = Alert.YesNo(Lang.Get("刷新后已选奖励不可重新选择，是否继续？"));
            d.SetYesCallback(() =>
            {
                _info.RefreshReward(SetRewardFalse);
                d.Close();
            });
        }
        else
        {
            var d = Alert.YesNo(Lang.Get("确认后自选奖励不可更改，是否继续？"));
            d.SetYesCallback(() =>
            {
                _info.ConfirmReward(_info.SelectedId, SetRewardTrue);
                d.Close();
            });
        }
    }
    private void On_btnAtkBossClick()
    {
        DialogManager.ShowAsyn<_D_WorldNYBoss>(On_btnAtkBossDialogShowAsynCB);
    }
    private void On_btnAtkBossDialogShowAsynCB(_D_WorldNYBoss d)
    {
        d?.OnShow();
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshPoint);

    }

    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(RefreshPoint);
    }

    public override void OnShow()
    {
        _Tips.CloseTip();
        UpdateTime(0);
        SetReward();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_tweenSeq != null)
        {
            _tweenSeq.Kill();
            _tweenSeq = null;
        }
        if (_Tips != null)
        {
            _Tips.OnDestroy();
            _Tips = null;
        }
        if (_rewardItems != null)
        {
            _rewardItems = null;
        }
    }

    private void InitActInfo()
    {
        _txtDesc.text = _info._desc;
    }

    private void RefreshPoint()
    {
        _txtCount.text = Lang.Get("当前讨伐点数:{0}", BagInfo.Instance.GetItemCount(ItemId.Act2064Point));
    }

    public override void UpdateTime(long time)
    {
        base.UpdateTime(time);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (TimeManager.ServerTimestamp - _info._data.startts < 0)
        {
            _txtTime.text = GlobalUtils.GetActivityStartTimeDesc(_info._data.startts);
        }
        else if (_info.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_info.LeftTime);
            _txtTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _txtTime.text = Lang.Get("活动已经结束");
        }
    }
    private void SetRewardFalse()
    {
        SetReward(false);
    }
    private void SetRewardTrue()
    {
        SetReward(true);
    }
    private void SetReward(bool needAmi = false)
    {
        RefreshPoint();
        var confirm = _info.Confirm;
        if (confirm == 0)//还没有确定奖励
        {
            _txtBtnRef.text = Lang.Get("确定奖励");
            if (_info.SelectedId == 0)
            {
                _btnSel.interactable = false;
                _btnRef.interactable = false;
                _rewardItems[0].SetAdd(OpenTip);
            }
            else
            {
                _btnSel.interactable = true;
                _btnRef.interactable = true;
                var cfgData = Cfg.Activity2064.GetData(_info.SelectedId);
                Act2064ItemData data = new Act2064ItemData
                {
                    id = _info.SelectedId,
                    index = 1,
                    get_reward = 0,
                    type = 1
                };
                _rewardItems[0].RefreshInit(data);
            }

            var reward = _info.GetInitialReward();
            for (int i = 0; i < reward.Count; i++)
            {
                _rewardItems[i + 1].RefreshInit(reward[i]);
            }
        }
        else
        {
            _btnSel.interactable = false;
            _btnRef.interactable = true;
            _txtBtnRef.text = Lang.Get("刷新奖池");

            _info.CanClick = false;
            var reward = _info.GetConfirmReward();
            for (int i = 0; i < reward.Count; i++)
            {
                var item = reward[i];
                _rewardItems[i].RefreshConfirm(item, _info, RefreshPoint);
            }

            if (needAmi)
            {
                _tweenSeq = DOTween.Sequence();
                for (int i = 0; i < _rewardItems.Length; i++)
                {
                    _tweenSeq.Join(_rewardItems[i].Transform.DOLocalMove(Vector3.zero, 0.1f));
                }

                _tweenSeq.AppendInterval(0.3f);
                for (int i = 0; i < _rewardItems.Length; i++)
                {
                    int x = (i % 3 - 1) * HorOffset;
                    int y = (i / 3 - 1) * VerOffset;
                    if (i != 4)
                        _tweenSeq.Append(_rewardItems[i].Transform.DOLocalMove(new Vector3(x, y, 0), 0.1f));
                }

                _tweenSeq.OnComplete(() =>
                {
                    _tweenSeq.Kill();
                    _tweenSeq = null;
                    _info.CanClick = true;
                });
            }
            else
            {
                _info.CanClick = true;
            }
        }
    }

    public void OpenTip()
    {
        _Tips.OpenTip();
    }

    public override void OnClose()
    {
        base.OnClose();
        _info.CanClick = true;
    }
}


public class _Act2064RewardItem
{
    private GameObject _objBuzy;
    private GameObject _objFree;

    private Button _btnClick;
    private Button _btnDraw;
    private Image _imgIcon;
    private Image _imgQua;
    private Text _txtCount;
    private GameObject _objImgAdd;
    private EquipEffect _effect;

    public Transform Transform;
    private Act2064ItemData _data;
    private ActInfo_2064 _info;
    private Action _ac;

    public int Index; //奖池位置

    public _Act2064RewardItem(Transform tans)
    {
        Transform = tans;
        _objBuzy = Transform.Find("Busy").gameObject;
        _objFree = Transform.Find("Free").gameObject;
        _btnClick = Transform.FindButton("Busy");
        _btnDraw = Transform.FindButton("Free");
        _imgIcon = Transform.Find<Image>("Busy/Img_icon");
        _imgQua = Transform.Find<Image>("Busy/Img_qua");
        _txtCount = Transform.Find<Text>("Busy/Text_num");
        _objImgAdd = Transform.Find("Busy/Image").gameObject;

        _effect = new EquipEffect(_objImgAdd.transform);
    }

    public void RefreshInit(Act2064ItemData data)
    {
        _objBuzy.SetActive(true);
        _objFree.SetActive(false);
        _objImgAdd.SetActive(false);
        _data = data;
        SetInfo();
    }

    public void RefreshConfirm(Act2064ItemData data, ActInfo_2064 actInfo, Action callback = null)
    {
        _objImgAdd.SetActive(false);
        _data = data;
        _info = actInfo;
        _ac = callback;
        Index = _data.index;
        if (data.get_reward == 1)
        {
            _objBuzy.SetActive(true);
            _objFree.SetActive(false);
            SetInfo();
        }
        else
        {
            _objBuzy.SetActive(false);
            _objFree.SetActive(true);
            _btnDraw.onClick.SetListener(On_btnDrawClick);
        }
    }
    private void On_btnDrawClick()
    {
        if (_info.CanClick)
            _info.GetReward(Index, GetReward);
    }
    public void SetAdd(Action ac)
    {
        _objBuzy.SetActive(true);
        _objFree.SetActive(false);
        _objImgAdd.SetActive(true);
        _effect.OnEnable();
        _imgIcon.gameObject.SetActive(false);
        _imgQua.gameObject.SetActive(false);
        _txtCount.text = "";
        _btnClick.onClick.SetListener(() => ac?.Invoke());
    }

    private void SetInfo()
    {
        var info = Cfg.Activity2064.GetData(_data.id).item;
        _imgIcon.gameObject.SetActive(true);
        _imgQua.gameObject.SetActive(true);
        int id = int.Parse(info.Split('|')[0]);
        Cfg.Item.SetItemIcon(_imgIcon, id);
        _imgQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(id));
        _txtCount.text = "x" + info.Split('|')[1];

        _btnClick.onClick.SetListener(() =>
         {
             DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(id, 1, _btnClick.transform.position); });
         });
    }

    private void GetReward()
    {
        _data = _info.GetInfoByIndex(Index);
        _objBuzy.SetActive(true);
        _objFree.SetActive(false);
        SetInfo();
        if (_ac != null)
            _ac();
    }
}

