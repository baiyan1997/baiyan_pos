using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2085_UI : ActivityUI
{
    private int _aid = ActivityID.StarRuins;
    private ActInfo_2085 _actInfo;
    private Text _textTitle;
    private Text _textCountDown;
    private Button _btnManual;
    private Button _btnGoNext;
    private GameObject _imgMask;
    private Text _textItemNum;
    private Button _finalReward;
    private Text _textName;
    private Image _finalRewardIcon;
    private Image _finalRewardQuqImg;
    private Text _finalRewardNum;
    private GameObject _gotten;
    private Transform _contentTrans;
    private StarRuinItem[] _rewards;
    private int _rewardsTransArrayLen = 25;
    private int _floorNum;//当前层数
    private int _finalItemId;//这一层的终极宝物id
    private int _finalItemNum;//这一层的终极宝物数量
    private Vector3 _animEndPos;
    private Transform _animEndTrans;
    private _D_ActCalendar _rootDialog;
    private bool _isShowingFinalRewardAnim;
    public override void OnCreate()
    {
        InitRef();
        InitButtonClick();
        //InitListener();
    }

    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += RefreshTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(RefreshUi);
        EventCenter.Instance.AddPushListener(OpcodePush.ACT_2085_GO_NEXT, UpdateOpcAct2085);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.RemovePushListener(OpcodePush.ACT_2085_GO_NEXT, UpdateOpcAct2085);
    }

    private IEnumerator DelayGoNextFloor()
    {
        while (true)
        {
            if (_actInfo.TurnOverAnimCompleted && !_isShowingFinalRewardAnim)
            {
                GoNext();
                break;
            }
            else
            {
                yield return null;
            }
        }
    }

    private void InitButtonClick()
    {
        _btnManual.onClick.AddListener(On_btnManualClick);
        _finalReward.onClick.AddListener(On_finalRewardClick);
        _btnGoNext.onClick.AddListener(GoNext);
    }
    private void On_btnManualClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnManualDialogShowAsynCB);
    }
    private void On_btnManualDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2085, _btnManual.transform.position, Direction.LeftDown, 350);
    }
    private void On_finalRewardClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_finalRewardDialogShowAsynCB);
    }
    private void On_finalRewardDialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(_finalItemId, 1, transform.position);
    }
    private void GoNext()
    {
        Rpc.SendWithTouchBlocking<P_GoNextInfo>("goTo2085NextFloor", null, On_goTo2085NextFloor_SC);
    }
    private void On_goTo2085NextFloor_SC(P_GoNextInfo data)
    {
        _actInfo.RefreshInfoForGoNext(data.cur_height, data.final_reward);
        _textTitle.text = Lang.Get("星海遗迹{0}层", data.cur_height);
        P_Item item = GlobalUtils.ParseItem(data.final_reward)[0];
        ShowFinalReward(item);
        bool tag = _actInfo.UniqueInfo.IsGotten;
        _imgMask.SetActive(tag);
        HideAllCards();
        GoNextAnim();
    }
    private void HideAllCards()
    {
        for (int i = 0; i < _rewardsTransArrayLen; i++)
        {
            _rewards[i].HideCard();
        }
    }

    private void GoNextAnim()
    {
        _rootDialog.SetBlock(true);
        for (int i = 0; i < _rewardsTransArrayLen; i++)
        {
            StartCoroutine(DoGoNextAnim(i));
            //_Scheduler.Instance.StartCoroutine(DoGoNextAnim(i));
        }
    }

    private IEnumerator DoGoNextAnim(int i)
    {
        float time = 0.1f * i;
        yield return new _WaitForSeconds(time);
        _rewards[i].GoNextAnim(ShowGetFinalRewardAnim, _finalItemId, _finalItemNum);
        if (i == _rewardsTransArrayLen - 1)
        {
            _rootDialog.SetBlock(false);
        }
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (_aid != aid)
        {
            return;
        }
        bool tag = _actInfo.UniqueInfo.IsGotten;
        _gotten.SetActive(tag);
        _imgMask.SetActive(tag);
        //_btnGoNext.gameObject.SetActive(tag);
        _textItemNum.text = Lang.Get("拥有遗迹芯片:{0}",
            BagInfo.Instance.GetItemCount(ItemId.RuinChipId));
        if (_actInfo.UniqueInfo.FloorNum == 5)
        {
            _btnGoNext.GetComponentInChildren<Text>().text = Lang.Get("已达到最高层");
            _btnGoNext.interactable = false;
        }
    }

    private void UpdateOpcAct2085(int opcode, string msg)
    {
        StartCoroutine(DelayGoNextFloor());
    }

    private void InitRef()
    {
        _textTitle = transform.FindText("_title");
        _textCountDown = transform.FindText("TextCountDown");
        _btnManual = transform.FindButton("_btnManual");
        _imgMask = transform.Find("Image").gameObject;
        _btnGoNext = transform.FindButton("Image/btn_GoNext");
        _textItemNum = transform.FindText("Text_ItemNum");
        _finalReward = transform.FindButton("FinalReward/Prop");
        _textName = transform.FindText("FinalReward/TextName");
        _finalRewardIcon = _finalReward.transform.FindImage("ImageIcon");
        _finalRewardQuqImg = _finalReward.transform.FindImage("ImageQua");
        _finalRewardNum = _finalReward.transform.FindText("CountText");
        _gotten = _finalReward.transform.Find("Gotten").gameObject;
        _contentTrans = transform.Find("Scroll_root/Viewport/Content");
        _animEndTrans = transform.Find("AnimImage");
        _animEndPos = transform.Find("AnimImage").position;
        _rootDialog = DialogManager.GetInstanceOfDialog<_D_ActCalendar>();
        _animEndTrans.gameObject.SetActive(false);
        _rewards = new StarRuinItem[_rewardsTransArrayLen];
        _actInfo = ActivityManager.Instance.GetActivityInfo(_aid) as ActInfo_2085;
        InitRewardsTransArr();
    }

    private void InitRewardsTransArr()
    {
        for (int i = 0; i < _rewardsTransArrayLen; i++)
        {
            _rewards[i] = new StarRuinItem(_contentTrans.GetChild(i), i, _actInfo);
        }
    }

    public override void OnShow()
    {
        _actInfo = ActivityManager.Instance.GetActivityInfo(_aid) as ActInfo_2085;
        UpdateTime(TimeManager.ServerTimestamp);
        //Debug.Log($"@@@{BagInfo.Instance.GetItemCount(70042)}");
        RefreshAll();
    }

    private void RefreshAll()
    {
        _textTitle.text = Lang.Get("星海遗迹{0}层", _actInfo.UniqueInfo.FloorNum);
        //展示本层终极宝物
        P_Item item = GlobalUtils.ParseItem(_actInfo.UniqueInfo.FinalReward)[0];
        ShowFinalReward(item);
        _imgMask.SetActive(_actInfo.UniqueInfo.IsGotten);
        if (_actInfo.UniqueInfo.FloorNum == 5)
        {
            _btnGoNext.GetComponentInChildren<Text>().text = Lang.Get("已达到最高层");
            _btnGoNext.interactable = false;
        }
        _textItemNum.text = Lang.Get("拥有遗迹芯片:{0}",
            BagInfo.Instance.GetItemCount(ItemId.RuinChipId));
        //展示本层已经翻开的宝物
        ShowRewards();
    }

    private void ShowRewards()
    {
        InitRewards();
        RefreshRewards();
    }

    private void RefreshRewards()
    {
        List<P_StarRuinItem> rewards = _actInfo.UniqueInfo.StarRuinItems;
        int len = rewards.Count;
        for (int i = 0; i < len; i++)
        {
            P_StarRuinItem item = rewards[i];
            int index = item.position;
            P_Item reward = GlobalUtils.ParseItem(item.reward)[0];
            _rewards[index].OnShow(reward.Id, reward.Num);
        }
    }

    private void InitRewards()
    {
        for (int i = 0; i < _rewardsTransArrayLen; i++)
        {
            _rewards[i].Init(ShowGetFinalRewardAnim, _finalItemId, _finalItemNum);
        }
    }

    private void ShowGetFinalRewardAnim(Vector3 data)
    {
        _rootDialog.SetBlock(true);
        _isShowingFinalRewardAnim = true;
        _animEndTrans.gameObject.SetActive(true);
        _animEndTrans.position = data;
        var canvasGroup = _animEndTrans.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        _animEndTrans.DOMove(_animEndPos, 0.8f);
        _animEndTrans.DOScale(new Vector3(2, 2, 2), 0.8f).OnComplete(() =>
        {
            float alpha = 1;
            DOTween.To(() => alpha, x => alpha = x, 0f, 1.5f).OnUpdate(() =>
            {
                canvasGroup.alpha = alpha;
                if (alpha <= 0.3f)
                {
                    _rootDialog.SetBlock(false);
                }
            }).OnComplete(
                () =>
                {
                    _animEndTrans.gameObject.SetActive(false);
                    _isShowingFinalRewardAnim = false;
                });
        });
    }
    private void ShowFinalReward(P_Item item)
    {
        _finalItemId = item.Id;
        _finalItemNum = item.Num;
        int num = item.Num;
        Cfg.Item.SetItemIcon(_finalRewardIcon, _finalItemId);
        Cfg.Item.SetItemIcon(_animEndTrans.FindImage("ImageIcon"), _finalItemId);
        var color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(item.id));
        _finalRewardQuqImg.color = color;
        _animEndTrans.FindImage("ImageQua").color = color;
        _finalRewardNum.text = Lang.Get("x{0}", num);
        _animEndTrans.FindText("CountText").text = Lang.Get("x{0}", num);
        _gotten.SetActive(_actInfo.UniqueInfo.IsGotten);
        _textName.text = Cfg.Item.GetItemName(_finalItemId);
        _textName.color = color;
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

    public override void OnClose()
    {
        base.OnClose();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}

public class StarRuinItem
{
    private Transform _transform;
    private Button _btn;
    private Button _btnMask;
    private Transform _animNode;
    private Image _icon;
    private Image _imageQua;
    private Text _countText;
    private int _itemId;
    private int _itemNum;
    private bool _isOpen;//已经被翻开了
    private int _position;
    private ActInfo_2085 _actInfo;
    private string _ruinChip = "70042|1";
    private Action<Vector3> _callBack;
    private GameObject _effect;
    private int _finalRewardId;
    private int _finalRewardNum;

    public StarRuinItem(Transform transform, int position, ActInfo_2085 actInfo)
    {
        _transform = transform;
        _position = position;
        _actInfo = actInfo;
        InitRef();
        InitBtn();
    }

    private void InitBtn()
    {
        _btn.onClick.AddListener(On_btnClick);
        _btnMask.onClick.AddListener(On_btnMaskClick);
    }
    private void On_btnClick()
    {
        if (_isOpen)
        {
            DialogManager.ShowAsyn<_D_ItemTip>(On_btnDialogShowAsynCB);
        }
    }
    private void On_btnDialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(_itemId, _itemNum, _transform.position);
    }
    private void On_btnMaskClick()
    {
        if (!_isOpen)
        {
            if (BagInfo.Instance.GetItemCount(ItemId.RuinChipId) == 0)
            {
                MessageManager.Show(Lang.Get("遗迹芯片数量为0"));
                return;
            }
            Uinfo.Instance.AddItem(_ruinChip, false);
            DoShowReward();
        }
    }

    private void DoShowReward()
    {
        Rpc.SendWithTouchBlocking<P_TurnOverInfo>("turnOver2085Card", Json.ToJsonString(_position), On_turnOver2085Card_SC);
    }
    private void On_turnOver2085Card_SC(P_TurnOverInfo data)
    {
        _actInfo.RefreshInfoForTurnOver(data.is_get, data.get_item, data.relic_chip_num, _position);
        P_Item item = GlobalUtils.ParseItem(data.get_item)[0];
        _itemId = item.Id;
        _itemNum = item.Num;
        Sequence tweenSeq = DOTween.Sequence();
        var tweener1 = _animNode.DOLocalRotate(new Vector3(0, -90, 0), 0.2f).OnComplete(() =>
        {
            _btnMask.gameObject.SetActive(false);
            _btn.gameObject.SetActive(true);
        });
        var tweener2 = _animNode.DOLocalRotate(new Vector3(0, 0, 0), 0.2f).OnComplete(() =>
        {
            if (_actInfo.UniqueInfo.StarRuinItems.Count == 25)
            {
                _actInfo.TurnOverAnimCompleted = true;
            }

            if (_itemId == _finalRewardId && _finalRewardNum == _itemNum)
            {
                _callBack?.Invoke(_transform.position);
                _effect.SetActive(true);
            }
        });
        tweenSeq.Append(tweener1).Append(tweener2);
        Uinfo.Instance.AddItemAndShow(data.get_item);
        ShowReward();
    }

    private void ShowReward()
    {
        _isOpen = true;
        _btn.gameObject.SetActive(true);
        Cfg.Item.SetItemIcon(_icon, _itemId);
        _imageQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(_itemId));
        _countText.text = Lang.Get("x{0}", _itemNum);
    }

    private void InitRef()
    {
        _animNode = _transform.Find("AnimNode");
        _btn = _transform.FindButton("AnimNode/01");
        _btnMask = _transform.FindButton("AnimNode/00");
        _icon = _transform.FindImage("AnimNode/01/ImageIcon");
        _imageQua = _transform.FindImage("AnimNode/01/ImageQua");
        _countText = _transform.FindText("AnimNode/01/CountText");
        _effect = _transform.Find<GameObject>("AnimNode/Effect");
    }

    public void OnShow(int itemId, int itemNum)
    {
        _itemId = itemId;
        _itemNum = itemNum;
        _btnMask.gameObject.SetActive(false);
        _animNode.localEulerAngles = new Vector3(0, 0, 0);
        ShowReward();
    }

    public void Init(Action<Vector3> callBack, int finalRewardId, int finalRewardNum)
    {
        InitDataAndUi(callBack, finalRewardId, finalRewardNum);
        _animNode.localEulerAngles = new Vector3(0, -180, 0);
    }

    private void InitDataAndUi(Action<Vector3> callBack, int finalRewardId, int finalRewardNum)
    {
        _animNode.gameObject.SetActive(true);
        _btnMask.gameObject.SetActive(true);
        _callBack = callBack;
        _finalRewardId = finalRewardId;
        _finalRewardNum = finalRewardNum;
        _isOpen = false;
        _btn.gameObject.SetActive(false);
        _effect.SetActive(false);
    }
    public void HideCard()
    {
        _animNode.gameObject.SetActive(false);
    }
    public void GoNextAnim(Action<Vector3> callBack, int finalRewardId, int finalRewardNum)
    {
        InitDataAndUi(callBack, finalRewardId, finalRewardNum);
        _animNode.localEulerAngles = new Vector3(0, -90, 0);
        _animNode.DOLocalRotate(new Vector3(0, -180, 0), 0.5f);
    }
}
