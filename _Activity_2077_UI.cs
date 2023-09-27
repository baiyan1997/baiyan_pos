using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

//寻找舰长活动
public class _Activity_2077_UI : ActivityUI
{
    private ActInfo_2077 _actInfo;
    private const int Aid = 2077;

    //标题和倒计时
    private Text _actTime;
    private Button _begin;//开始按钮
    private GameObject _mask;//未开始时遮挡

    //奖励展示
    private RewardItem[] _rewards;

    private Text _leftCount;

    //卡牌翻转特效
    private Sequence _seq;

    private Card[] _cardItems;

    private Button _helpBtn;

    Transform _cardRoot;
    public override void OnCreate()
    {
        _actTime = transform.Find<Text>("Text_time");
        _begin = transform.Find<Button>("Mask/Btn");
        _leftCount = transform.Find<Text>("Text_leftCount");
        _mask = transform.Find<GameObject>("Mask");
        _helpBtn = transform.Find<Button>("ButtonHelp");
        Transform root = transform.Find("Root_reward");
        _rewards = new[]
        {
            root.GetChild(0).gameObject.AddBehaviour<RewardItem>(),
            root.GetChild(1).gameObject.AddBehaviour<RewardItem>(),
        };

        _cardRoot = transform.Find("Icon_Mid");
        _cardItems = new Card[15];
        for (int i = 0; i < 15; i++)
        {
            _cardItems[i] = _cardRoot.GetChild(i).gameObject.AddBehaviour<Card>();
        }
        InitEffect();

        _begin.onClick.AddListener(On_beginClick);
        _helpBtn.onClick.AddListener(On_helpBtnClick);
    }
    private void On_beginClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);
        _actInfo.ShowSearchCaptActInfo();
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpBtnDialogShowAsynCB);
    }
    private void On_helpBtnDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2077, _helpBtn.transform.position, Direction.LeftDown, 350);
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.TurnOverCard.AddListener(TurnOverCard);
        EventCenter.Instance.SearchCaptActJudge.AddListener(LeftCountAndLineEff);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.TurnOverCard.RemoveListener(TurnOverCard);
        EventCenter.Instance.SearchCaptActJudge.RemoveListener(LeftCountAndLineEff);
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2077)ActivityManager.Instance.GetActivityInfo(Aid);
        //刷新奖励
        SetReward();
        UpdateTime(TimeManager.ServerTimestamp);
        UpdateUI(Aid);
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        _seq = DOTween.Sequence();
        if (aid != Aid)
            return;

        SetBeginBtn();
        SetLeftChance();
        SetCardAnim();
    }
    private void TurnOverCard(int open)
    {
        //翻开时使用新的Sequence
        if (open == 1)
        {
            _seq = DOTween.Sequence();
        }
        //同步数据时判断有没有结束
        else
        {
            var note = _actInfo.GetEndNote();
            if (!string.IsNullOrEmpty(note))
                _seq.OnComplete(() => MessageManager.Show(note));
        }
        SetCardAnim();
    }
    private void LeftCountAndLineEff()
    {
        SetLeftChance();
        SetLineEffect();
    }

    private void SetCardAnim()
    {
        var cardIds = _actInfo.GetCards();

        bool first = true;
        for (int i = 0; i < _cardItems.Length; i++)
        {
            var tween = _cardItems[i].Refresh(i, cardIds[i], _actInfo);
            if (tween != null && first)
            {
                _seq.Append(tween);
                first = false;
            }
            else
            {
                _seq.Join(tween);
            }
        }
    }

    //今日剩余次数显示
    private void SetLeftChance()
    {
        int leftCount = _actInfo.LeftCount();
        _leftCount.text = Lang.Get("今日剩余次数：{0}", leftCount);
    }

    //设置连线特效
    Dictionary<int, ObjectPool<RectTransform>> _effPool = new Dictionary<int, ObjectPool<RectTransform>>(3);//key:0:卡片特效 1:成功连线 2:失败连线
    //锁链特效

    private RectTransform cardEff;
    private RectTransform successLine;
    private RectTransform failLine;

    private void InitEffect()
    {
        cardEff = transform.Find<RectTransform>("PFB_Activity_items");
        successLine = transform.Find<RectTransform>("LightingBlend");
        failLine = transform.Find<RectTransform>("LightingBlendBroken");

        for (int i = 0; i < 3; i++)
        {
            RectTransform go = null;
            if (i == 0)
                go = cardEff;
            if (i == 1)
                go = successLine;
            if (i == 2)
                go = failLine;

            _effPool[i] = new ObjectPool<RectTransform>(
                () => Object.Instantiate(go, _cardRoot, false),
                g =>
                {
                    g.SetAsLastSibling();
                },
                g =>
                {
                    g.gameObject.SetActive(false);
                    g.SetAsFirstSibling();
                });
        }
    }

    private void SetLineEffect()
    {
        var indexList = _actInfo.GetTripleCards();
        if (indexList == null)
            return;

        _seq.AppendInterval(0.5f);
        for (int i = 0; i < indexList.Count; i++)
        {
            //卡片特效
            var cardIndex = indexList[i];
            if (cardIndex >= _cardItems.Length)
                throw new Exception("turnOverCard -> turn_over_index out of range now =" + cardIndex);
            var cardEff = _effPool[0].Get();
            cardEff.SetParent(_cardItems[cardIndex].transform, false);
            var t = 0;
            var tween1 = DOTween.To(() => t = 0, x => t = x, 10, 1)
                .OnStart(() => cardEff.gameObject.SetActive(true))
                .OnComplete(() => _effPool[0].Push(cardEff));
            _seq.Join(tween1);

            //连线特效
            int fromCardIndex;
            if (i == 0)
            {
                fromCardIndex = indexList[indexList.Count - 1];
            }
            else
            {
                fromCardIndex = indexList[i - 1];
            }

            var toCardIndex = indexList[i];
            var fromTrans = _cardItems[fromCardIndex];
            var toTrans = _cardItems[toCardIndex];

            //两个卡片是否相同
            bool success = _actInfo.IsSameCid(fromCardIndex, toCardIndex);
            int effIndex = success ? 1 : 2;
            if (success)
            {
                _seq.Join(GetEffectTween(effIndex, fromTrans.transform, toTrans.transform, true));
            }
            else
            {
                //失败 需要用两个展示断开效果
                _seq.Join(GetEffectTween(effIndex, fromTrans.transform, toTrans.transform, false));
                _seq.Join(GetEffectTween(effIndex, toTrans.transform, fromTrans.transform, false));
            }
        }
    }

    private Tween GetEffectTween(int effIndex, Transform from, Transform to, bool needScale)
    {
        var eff = _effPool[effIndex].Get();
        eff.localPosition = from.localPosition;

        var diff = to.localPosition - from.localPosition;
        //大小
        if (needScale)
        {
            eff.localScale = new Vector3(diff.magnitude / 150.0f, 1, 1);
        }
        else
        {
            eff.localScale = new Vector3(1, 1, 1);
        }
        //方向
        var normalized = diff.normalized;
        var neg = normalized.y > 0 ? 1 : -1;
        var angleZ = Mathf.Acos(normalized.x) * Mathf.Rad2Deg * neg;
        eff.localEulerAngles = new Vector3(0, 0, angleZ);

        var t = 0;
        var tween = DOTween.To(() => t = 0, x => t = x, 10, 1)
            .OnStart(() => eff.gameObject.SetActive(true))
            .OnComplete(() => _effPool[effIndex].Push(eff));
        return tween;
    }
    //奖励显示
    private void SetReward()
    {
        var items = GlobalUtils.ParseItem(_actInfo.GetRewards());

        for (int i = 0; i < _rewards.Length; i++)
        {
            if (items.Length > i)
            {
                _rewards[i].gameObject.SetActive(true);
                _rewards[i].Refresh(items[i]);
            }
            else
            {
                _rewards[i].gameObject.SetActive(false);
            }
        }
    }
    //开始按钮
    private void SetBeginBtn()
    {
        //未展示过 显示按钮
        bool showBtn = _actInfo.NotShown();
        _mask.SetActive(showBtn);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_seq != null)
        {
            _seq.Kill();
            _seq = null;
        }
    }

    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (nowTs - _actInfo._data.startts < 0)
        {
            _actTime.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            _actTime.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _actTime.text = Lang.Get("活动已经结束");
        }
    }

    public class Card : JDBehaviour
    {
        public Button _click;
        //舰长icon
        public Image _icon;

        //正面人物
        public GameObject _busy;
        //翻面背景
        public GameObject _free;

        //_cid为0 表示背面
        private int _cid = -1;

        private ActInfo_2077 _actInfo;
        private int _index;
        private Sequence _seq;
        public override void Awake()
        {
            base.Awake();
            _icon = transform.Find<Image>("Busy/Img_Hero");
            _free = transform.Find<GameObject>("Free");
            _click = transform.Find<Button>("Free");
            _busy = transform.Find<GameObject>("Busy");
            _click.onClick.AddListener(On_click);
        }
        private  void On_click()
        {
            //自己的翻牌动画播完才能点击，避免双击
            if (_seq != null && _seq.IsPlaying())
                return;
            if (!_actInfo.HaveLeftChance())
            {
                MessageManager.Show(Lang.Get("您的机会已用完"));
                return;
            }
            _actInfo.TurnOverCard(_index);
        }

        public Tween Refresh(int index, int cid, ActInfo_2077 actInfo)
        {
            _index = index;
            _actInfo = actInfo;
            //背面的时候可以点击
            _click.interactable = cid == 0;
            if (_cid == cid)
                return null;
            return DoAnim(cid);
        }

        private Sequence DoAnim(int cid)
        {
            //转180度
            Tween tween4 = transform.DORotate(new Vector3(0, -90, 0), 0.1f, RotateMode.FastBeyond360).SetDelay(0.2f).
                    OnComplete(() =>
                    {
                        transform.localEulerAngles = new Vector3(0, 90, 0);
                        SetCid(cid);
                    });
            Tween tween5 = transform.DORotate(new Vector3(0, 0, 0), 0.1f, RotateMode.FastBeyond360);

            _cid = cid;

            _seq = DOTween.Sequence();
            _seq = _seq.AppendCallback(() => Game.Instance.AudioManager.PlaySound(AudioType.AS_Operation, SoundType.ID_2036))
                .Append(tween4)
                .Append(tween5);
            return _seq;
        }

        private void SetCid(int cid)
        {
            if (cid == 0)
            {
                _busy.SetActive(false);
                _free.SetActive(true);
            }
            else
            {
                _busy.SetActive(true);
                Cfg.Captain.SetCaptainPhoto(_icon, cid, 0);
                _free.SetActive(false);
            }
        }
    }
    public class RewardItem : JDBehaviour
    {
        private Image _icon;
        //        private Image _iconQua;
        private Text _text;
        //        private  Text _name;
        public override void Awake()
        {
            _icon = transform.Find<Image>("Img_icon");
            //            _iconQua = transform.Find<Image>("Img_qua");
            _text = transform.Find<Text>("Text_num");
            //            _name = transform.Find<Text>("Text_name");
        }


        public void Refresh(P_Item reward)
        {
            var itemShow = ItemForShow.Create(reward.id, reward.count);
            itemShow.SetIcon(_icon);
            //            _iconQua.color = _ColorConfig.GetQuaColorHSV(itemShow.GetQua());
            _text.text = "x" + GLobal.NumFormat(itemShow.GetCount());
            //            _name.text = itemShow.GetName();
            transform.GetComponent<Button>().onClick.SetListener(() =>
            {
                ItemHelper.ShowTip(reward.id, reward.count, transform);
            });
        }
    }
}
