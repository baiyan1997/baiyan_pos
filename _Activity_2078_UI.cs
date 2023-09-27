using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class _Activity_2078_UI : ActivityUI
{
    private Text _coinCountText;
    private Button _playBtn;
    private Text _timeText;
    private Button _moreBtn;
    private Button _exchangeBtn;
    private Image _iconImg;
    private Image _exchangeImg;
    private Transform[] _cardGoList;
    private GameObject _tipMore;
    private GameObject _exchangeTip;
    private Text _exchangeCount;
    private Text _priceText;
    private Slider _slider;
    private Text _progressText;
    private Button _boxBtn;
    private GameObject _tipGo;
    private GameObject _playBtnCoin;
    private Button _helpBtn;
    private _CaptainStand _captainStand;
    private const int _aid = 2078;
    private ActInfo_2078 _actInfo;
    private bool _needReset = false;
    private bool _isPlaying = false;
    private float _deltaPos = 134.0f;
    private int _gameCost;
    private int _boxValue;
    private bool _isDraw;
    private int _cid = 84;//安妮
    private Vector3 _scaleSelect = new Vector3(1.08f, 1.08f, 1);


    public override void OnCreate()
    {
        InitRef();
        InitEvent();
        //InitListener();
    }

    private void InitRef()
    {
        Transform buttons = transform.Find("Buttons");
        _coinCountText = buttons.Find<JDText>("Coin/TextCount");
        _playBtn = transform.Find<Button>("BtnPlay");
        _timeText = transform.Find<JDText>("Text_time");
        _moreBtn = buttons.Find<Button>("Coin/ButtonMore");
        _exchangeBtn = buttons.Find<Button>("Exchange/ButtonExchange");
        _exchangeTip = buttons.Find<GameObject>("Exchange/ButtonExchange/Tip");
        _iconImg = buttons.Find<Image>("Coin/Image/Icon");
        _exchangeImg = buttons.Find<Image>("Exchange/Image/Icon");
        Transform cards = buttons.Find("Cards");
        _cardGoList = new Transform[]
        {
             cards.Find("01"),
             cards.Find("02"),
             cards.Find("03"),
             cards.Find("04"),
             cards.Find("05"),
         };
        _tipMore = buttons.Find<GameObject>("Coin/ButtonMore/Tip");
        _exchangeCount = buttons.Find<JDText>("Exchange/TextCount");
        _priceText = transform.Find<JDText>("BtnPlay/Text");
        _slider = transform.Find<Slider>("Progress");
        _progressText = transform.Find<JDText>("Progress/Text");
        _boxBtn = transform.Find<Button>("BoxBtn");
        _tipGo = transform.Find<GameObject>("Tips");
        _gameCost = Cfg.FuncAttr.GetIntAttrByName("annie_cost");
        _boxValue = Cfg.FuncAttr.GetIntAttrByName("annie_get_progress_reward");
        _playBtnCoin = _playBtn.transform.Find<GameObject>("Image");
        _helpBtn = transform.Find<Button>("HelpBtn");
        _captainStand = new _CaptainStand(transform.Find("ImageCommander"), null);
    }

    private void InitEvent()
    {
        _playBtn.onClick.SetListener(On_playBtnClick);
        _moreBtn.onClick.SetListener(On_moreBtnClick);
        _exchangeBtn.onClick.SetListener(On_exchangeBtnClick);
        _boxBtn.onClick.SetListener(On_boxBtnClick);
        _helpBtn.onClick.AddListener(On_helpBtnClick);
    }
    private void On_playBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        if (_actInfo == null)
            return;
        if (_isPlaying || _isDraw)
            return;
        //需要重置
        if (_needReset)
        {
            DoResetAnim();
        }
        else
        {
            if (ItemHelper.IsCountEnough(ItemId.Act2078CoinId, _gameCost))
            {
                _actInfo.PlayGame(OnPlayGameCB);
            }
            else
            {
                MessageManager.Show(Lang.Get("{0}不足", Cfg.Item.GetItemName(ItemId.Act2078CoinId)));
            }
        }
    }
    private void OnPlayGameCB()
    {
        _actInfo = (ActInfo_2078)ActivityManager.Instance.GetActivityInfo(_aid);
        _coinCountText.text = _actInfo.CoinCount.ToString();
        _isDraw = true;
        DoAnim();
    }
    private void On_moreBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        if (_actInfo == null)
            return;
        DialogManager.ShowAsyn<_D_Act2078Reward>(On_moreBtnDialogShowAsynCB);
    }
    private void On_moreBtnDialogShowAsynCB(_D_Act2078Reward d)
    {
        d?.OnShow(0);
    }
    private void On_exchangeBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        if (_actInfo == null)
            return;
        DialogManager.ShowAsyn<_D_Act2078Reward>(On_exchangeBtnDialogShowAsynCB);
    }
    private void On_exchangeBtnDialogShowAsynCB(_D_Act2078Reward d)
    {
        d?.OnShow(1);
    }
    private void On_boxBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        if (_actInfo == null)
            return;
        if (_actInfo.Progress >= _boxValue)
        {
            _actInfo.GetProgressReward(() =>
            {
                _actInfo = (ActInfo_2078)ActivityManager.Instance.GetActivityInfo(_aid);
                UpdatePregress();
            });
        }
        //预览宝箱内容
        else
        {
            DialogManager.ShowAsyn<_D_ShowRewards>(On_boxBtnDialogShowAsynCB);
        }
    }
    private void On_boxBtnDialogShowAsynCB(_D_ShowRewards d)
    {
        List<P_Item> list = new List<P_Item>(_actInfo.BoxRewardList);
        d?.PreviewBox(list, Lang.Get("进度宝箱"));
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Top_HelpDesc>(On_helpBtnDialogShowAsynCB);
    }
    private void On_helpBtnDialogShowAsynCB(_D_Top_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2078);
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdatePlayerItem);
        EventCenter.Instance.RemindActivity.AddListener(UpdateRemindAct);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdatePlayerItem);
        EventCenter.Instance.RemindActivity.RemoveListener(UpdateRemindAct);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _aid)
            return;

        if (gameObject.activeSelf)
        {
            OnShow();
        }
    }

    public override void UpdateTime(long obj)
    {
        base.UpdateTime(obj);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!gameObject.activeSelf)
            return;

        if (_actInfo == null)
            return;

        if (_actInfo.LeftTime >= 0)
        {
            _timeText.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2078)ActivityManager.Instance.GetActivityInfo(_aid);

        if (_actInfo == null)
            return;

        UpdateCoinUI();

        UpdateExchangeUI();

        UpdatePregress();

        DefineAllCard();

        _captainStand.SetCaptainStand(_cid);

        //Debug.LogError("是否可抽卡" + _actInfo.IsFree + "   _isPlaying" + _isPlaying + "     _isDraw" + _isDraw);

        if (_isPlaying)
            return;

        //可抽卡
        if (_actInfo.IsFree)
        {
            _playBtn.gameObject.SetActive(false);
            _tipGo.SetActive(true);
            //展示背面
            SetAllCardBack();
            _isDraw = true;
        }
        else
        {
            _playBtn.gameObject.SetActive(true);
            _tipGo.SetActive(false);
            _isDraw = false;
            _priceText.text = Lang.Get("{0} 开始游戏", _gameCost);
            _playBtnCoin.SetActive(true);
            _needReset = false;
            //展示正面初始状态
            SetAllCardFront();
        }
    }

    private void UpdatePlayerItem()
    {
        if (gameObject.activeSelf)
        {
            _actInfo = (ActInfo_2078)ActivityManager.Instance.GetActivityInfo(_aid);
            if (_actInfo == null)
                return;

            _coinCountText.text = _actInfo.CoinCount.ToString();
            _exchangeCount.text = _actInfo.ToyCount.ToString();
        }
    }

    private void UpdateRemindAct(int aid, bool show)
    {
        if (aid != _aid)
            return;

        if (_actInfo == null)
            return;

        if (gameObject.activeSelf)
        {
            UpdateExchangeUI();
        }
    }

    private void DefineAllCard()
    {
        P_Item[] itemlist = _actInfo.GamePool;

        for (int i = 0; i < _cardGoList.Length; i++)
        {
            DefineCard(_cardGoList[i], itemlist[i]);
        }
    }

    //刷新进度
    private void UpdatePregress()
    {
        _progressText.text = string.Format("{0}/{1}", _actInfo.Progress, _boxValue);
        float oldValue = _slider.value;
        float newValue = (float)_actInfo.Progress / _boxValue;
        if (newValue > oldValue)
        {
            DOTween.To(() => _slider.value, x => _slider.value = x, newValue, 0.2f).SetEase(Ease.OutQuad);
        }
        else
        {
            _slider.value = newValue;
        }

        if (_actInfo.Progress < _boxValue)
        {
            _boxBtn.transform.localEulerAngles = Vector3.zero;
            _boxBtn.GetComponent<Animator>().enabled = false;
        }
        else
        {
            _boxBtn.GetComponent<Animator>().enabled = true;
        }
    }

    //刷新游戏币
    private void UpdateCoinUI()
    {
        Cfg.Item.SetItemIcon(_iconImg, ItemId.Act2078CoinId);
        _coinCountText.text = _actInfo.CoinCount.ToString();
        _tipMore.SetActive(_actInfo.IsMissionTip());
    }

    //兑换刷新
    private void UpdateExchangeUI()
    {
        Cfg.Item.SetItemIcon(_exchangeImg, ItemId.Act2078ToyId);
        _exchangeCount.text = _actInfo.ToyCount.ToString();
        _exchangeTip.SetActive(_actInfo.IsExchangeTip());
    }

    //洗牌发牌的动画
    private void DoAnim()
    {
        for (int i = 0; i < _cardGoList.Length; i++)
        {
            Transform trans = _cardGoList[i];

            Sequence tweenSeq = DOTween.Sequence();

            _isPlaying = true;
            _playBtn.gameObject.SetActive(false);

            Transform freeTrans = trans.Find("Free");
            Transform busyTrans = trans.Find("Busy");

            //先翻转
            Tween tween1 = trans.DOLocalRotate(new Vector3(0, -90, 0), 0.2f).OnComplete(() =>
               {
                   busyTrans.gameObject.SetActive(false);
                   freeTrans.gameObject.SetActive(true);
                   trans.DOLocalRotate(new Vector3(0, -180, 0), 0.2f);
               });

            Tween tween2 = trans.DOLocalMoveX(0, 0.6f).SetEase(Ease.OutSine).SetDelay(0.6f);
            Tween tween3 = trans.DOLocalMoveX((-268 + i * _deltaPos), 0.6f).SetDelay(0.8f);

            tweenSeq.Append(tween1).Append(tween2).Append(tween3).AppendCallback(() =>
            {

            }).OnComplete(() =>
            {
                tweenSeq = null;
                _isPlaying = false;
                _isDraw = true;
                _tipGo.SetActive(true);
                trans.localEulerAngles = new Vector3(0, -180, 0);
                busyTrans.localEulerAngles = Vector3.zero;
            });
        }
    }


    private void DefineCard(Transform trans, P_Item item)
    {
        GameObject busy = trans.Find<GameObject>("Busy");
        GameObject free = trans.Find<GameObject>("Free");

        Image icon = busy.transform.Find<Image>("Img_icon");
        Image qua = busy.transform.Find<Image>("qua");
        Text text = busy.transform.Find<Text>("Text_num");
        ItemForShow itemForShow = new ItemForShow(item.id, item.count);
        itemForShow.SetIcon(icon);
        text.text = "x" + GLobal.NumFormat(itemForShow.GetCount());
        qua.color = _ColorConfig.GetQuaColorHSV(itemForShow.GetQua());
        icon.GetComponent<Button>().onClick.SetListener(() =>
        {
            if (_isPlaying)
                return;

            AudioManager.Instace.PlaySoundOfNormalBtn();

            if (!_isDraw)
            {
                DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(item.id, item.count, icon.transform.position); });
            }
        });

        free.GetComponent<Button>().onClick.SetListener(() =>
        {
            if (_isPlaying)
                return;

            if (_isDraw)
            {
                AudioManager.Instace.PlaySoundOfNormalBtn();

                if (_actInfo != null)
                {
                    _actInfo.DrawCard((reward) =>
                    {
                        DoDrawCardAnim(trans, reward);
                        UpdatePregress();
                    });
                }
            }
        });
    }

    //-180到0
    //抽卡的动画
    private void DoDrawCardAnim(Transform trans, string reward)
    {
        _isPlaying = true;

        P_Item item = new P_Item(reward);
        DefineCard(trans, item);

        Sequence tweenSeq = DOTween.Sequence();

        Transform freeTrans = trans.Find("Free");
        Transform busyTrans = trans.Find("Busy");

        busyTrans.gameObject.SetActive(false);
        freeTrans.gameObject.SetActive(true);

        Tween tween1 = trans.DOLocalRotate(new Vector3(0, -90, 0), 0.2f).OnComplete(() =>
        {
            busyTrans.gameObject.SetActive(true);
            freeTrans.gameObject.SetActive(false);
            trans.DOLocalRotate(new Vector3(0, 0, 0), 0.2f);
        });

        Tween tween2 = busyTrans.DOScale(_scaleSelect, 0.2f).SetDelay(0.4f);

        tweenSeq.Append(tween1).Append(tween2).AppendCallback(() =>
        {

        }).OnComplete(() =>
        {
            tweenSeq = null;
            _isDraw = false;
            _needReset = true;
            _priceText.text = Lang.Get("继续游戏");
            _playBtnCoin.SetActive(false);
            _isPlaying = false;
            _tipGo.SetActive(false);
            trans.Find("Effect").gameObject.SetActive(true);
            //剩余4张卡片
            FlipLeftCard(trans.name, item);
            _playBtn.gameObject.SetActive(true);
        });
    }

    private void FlipLeftCard(string name, P_Item item)
    {
        List<P_Item> rewards = new List<P_Item>();
        for (int i = 0; i < _actInfo.GamePool.Length; i++)
        {
            if (_actInfo.GamePool[i].id == item.id && _actInfo.GamePool[i].count == item.count)
                continue;
            rewards.Add(_actInfo.GamePool[i]);
        }

        rewards = _actInfo.ListRandom(rewards);

        int j = 0;
        for (int i = 0; i < _cardGoList.Length; i++)
        {
            if (!name.Equals(_cardGoList[i].name))
            {
                Transform trans = _cardGoList[i];

                Sequence tweenSeq = DOTween.Sequence();

                Transform freeTrans = trans.Find("Free");
                Transform busyTrans = trans.Find("Busy");
                _isPlaying = true;
                //先翻转
                Tween tween1 = trans.DOLocalRotate(new Vector3(0, 270, 0), 0.2f).OnComplete(() =>
                {
                    busyTrans.gameObject.SetActive(true);
                    freeTrans.gameObject.SetActive(false);
                    trans.DOLocalRotate(new Vector3(0, 360, 0), 0.2f).OnComplete(() =>
                    {
                        trans.localEulerAngles = Vector3.zero;
                        _isPlaying = false;
                    });
                }).SetDelay(0.2f);
                DefineCard(_cardGoList[i], rewards[j]);
                j++;
            }
        }
    }

    //设置卡片全部正面的初始状态
    private void SetAllCardFront()
    {
        for (int i = 0; i < _cardGoList.Length; i++)
        {
            GameObject busy = _cardGoList[i].Find<GameObject>("Busy");
            GameObject free = _cardGoList[i].Find<GameObject>("Free");
            _cardGoList[i].localEulerAngles = busy.transform.localEulerAngles = new Vector3(0, 0, 0);
            busy.transform.localScale = Vector3.one;
            busy.SetActive(true);
            free.SetActive(false);
            _cardGoList[i].Find<GameObject>("Effect").SetActive(false);
        }
    }


    //设置卡片全部反面
    private void SetAllCardBack()
    {
        for (int i = 0; i < _cardGoList.Length; i++)
        {
            GameObject busy = _cardGoList[i].Find<GameObject>("Busy");
            GameObject free = _cardGoList[i].Find<GameObject>("Free");
            _cardGoList[i].localEulerAngles = new Vector3(0, -180, 0);
            busy.transform.localEulerAngles = Vector3.zero;
            busy.SetActive(false);
            free.SetActive(true);
            _cardGoList[i].Find<GameObject>("Effect").SetActive(false);
        }
    }

    //重置卡片为初始状态
    private void DoResetAnim()
    {
        //缩小为原来正常大小
        for (int i = 0; i < _cardGoList.Length; i++)
        {
            Transform trans = _cardGoList[i];
            if (trans.localScale != _scaleSelect)
            {
                _isPlaying = true;

                trans.Find("Busy").DOScale(Vector3.one, 0.2f).OnComplete(() =>
                {
                    DefineAllCard();
                    _priceText.text = Lang.Get("{0} 开始游戏", _gameCost);
                    _playBtnCoin.SetActive(true);
                    _needReset = false;
                    _isPlaying = false;
                    trans.Find("Effect").gameObject.SetActive(false);
                });
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_captainStand != null)
        {
            _captainStand.OnDestory();
        }
        _captainStand = null;
    }
}
