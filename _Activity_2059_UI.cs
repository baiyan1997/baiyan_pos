using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2059_UI : ActivityUI
{
    private Text _time;
    private Text _title;
    private Button[] _tabBtns;
    private GameObject[] _tabViews;
    private ActInfo_2059 _actInfo;
    private const int ActId = 2059;

    private TabHelper _tabViewHelper;
    private TabBtnHelper _tabBtnHelper;
    public override void OnCreate()
    {
        _time = transform.Find<Text>("Text_time");
        _tabBtns = new[]
        {
            transform.Find<Button>("Btns/Btn_Tab1"),
            transform.Find<Button>("Btns/Btn_Tab2"),
            transform.Find<Button>("Btns/Btn_Tab3"),
        };
        _tabViews = new[]
        {
            transform.Find("Tab01").gameObject,
            transform.Find("Tab02").gameObject,
            transform.Find("Tab03").gameObject,
        };

        _tabViewHelper = new TabHelper();
        _tabBtnHelper = new TabBtnHelper();
        for (int i = 0; i < _tabBtns.Length; i++)
        {
            _tabBtnHelper.RegistTabBtn(_tabBtns[i].gameObject.AddBehaviour<BtnTab2059>(), i);
            _tabBtnHelper.RegistTabBtn(_tabBtns[i].gameObject.AddBehaviour<BtnTab2059>(), i);
            _tabBtnHelper.RegistTabBtn(_tabBtns[i].gameObject.AddBehaviour<BtnTab2059>(), i);
        }
        for (int i = 0; i < _tabViews.Length; i++)
        {
            _tabViews[i].SetActive(false);
        }
        _tabBtnHelper.OnTabSwitch += SwitchTab;

        //InitListener();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_tabViewHelper != null)
        {
            _tabViewHelper.OnDestroy();
            _tabViewHelper = null;
        }
        if (_tabBtnHelper != null)
        {
            _tabBtnHelper.OnDestroy();
            _tabBtnHelper = null;
        }
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    public override void UpdateTime(long time)
    {
        base.UpdateTime(time);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _time.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _time.text = Lang.Get("活动已经结束");
        }
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != ActId)
            return;
        var tab = _tabViewHelper.GetCurrentTab(true);
        tab.Select();//刷新信息
    }

    public void SwitchTab(int oldIndex, int newIndex)
    {
        var view = _tabViewHelper.GetTabBySpId(newIndex, true);
        if (view == null)
        {
            if (newIndex == 0)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActTransportShipView2059>(), newIndex);
            if (newIndex == 1)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActBlackMarketView2059>(), newIndex);
            if (newIndex == 2)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActCoinDrawView2059>(), newIndex);
        }
        _tabViewHelper.ClickBySpId(newIndex);
    }
    public override void OnShow()
    {
        _actInfo = (ActInfo_2059)ActivityManager.Instance.GetActivityInfo(ActId);
        _tabBtnHelper.Finish();
        UpdateTime(TimeManager.ServerTimestamp);
        _actInfo.Showed();
    }
}

public class BtnTab2059 : TabBtnBase
{
    public override void Select()
    {
        transform.GetComponentInChildren<Text>().color = Color.white;
        UIHelper.SetImageSprite(GetButton().image, "btn_12");
    }
    public override void Unselect()
    {
        transform.GetComponentInChildren<Text>().color = new Color(126 / 255f, 229 / 255f, 1);
        UIHelper.SetImageSprite(GetButton().image, "btn_13");
    }
}
public class ActTransportShipView2059 : TabViewBase2 //货运船界面
{
    private Text _title;
    private Text _get;
    private Button _jump;
    private const int ActId = 2059;
    public override void Awake()
    {
        base.Awake();
        _title = transform.Find<Text>("Text_title");
        _get = transform.Find<Text>("Text_get");
        _jump = transform.Find<Button>("Btn_jump");

        _jump.onClick.AddListener(On_jumpClick);
    }
    private void On_jumpClick()
    {
        var basePos = PlayerInfo.Instance.Info.castle_id;
        WorldConfig.WorldController.EnterWorld(true, basePos);
    }
    public override void Select()
    {
        base.Select();
        var act2059 = (ActInfo_2059)ActivityManager.Instance.GetActivityInfo(ActId);
        _title.text = Cfg.Act.GetData(ActId).act_desc;
        _get.text = string.Format(Lang.Get("黑市货运船金币每日获取上限:{0}/{1}"),
                        act2059.Info.pirate_drop_coin,
                        Cfg.Act2059.PirateDrawLimit);
    }
}

public class ActBlackMarketView2059 : TabViewBase2 //黑市界面
{
    private Text _coins;
    private ListView _list;
    private const int ActId = 2059;
    public override void Awake()
    {
        base.Awake();
        _coins = transform.Find<Text>("Text_coin");
        _list = ListView.Create<BlackMarketItem>(transform.Find("ScrollView"));
        InitEvents();
    }

    private void InitEvents()
    {
        EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshPirateCoin);
    }

    private void UnInitEvents()
    {
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(RefreshPirateCoin);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnInitEvents();
    }

    public override void Select()
    {
        base.Select();
        var act2059 = (ActInfo_2059)ActivityManager.Instance.GetActivityInfo(ActId);
        RefreshPirateCoin();
        _list.Clear();
        var list = act2059.Info.user_exchange_list;
        for (int i = 0; i < list.Count; i++)
        {
            _list.AddItem<BlackMarketItem>().Refresh(list[i]);
        }
    }

    private void RefreshPirateCoin()
    {
        if (_coins)
        {
            var num = BagInfo.Instance.GetItemCount(ItemId.PirateCoin);
            _coins.text = string.Format(Lang.Get("拥有海盗金币数：{0}"), GLobal.NumFormat(num));
        }
    }

    private class BlackMarketItem : ListItem
    {
        private Image _icon;
        private Image _qua;
        private Text _num;
        private Button _exchange;
        private Text _cost;
        private Text _alreadyGet;//已兑换次数
        private GameObject _objLimit;//兑换次数达上限
        private P_BlackmarketItem _info;
        private ActInfo_2059 _act2059;
        public override void OnCreate()
        {
            _icon = transform.Find<Image>("Img_bg/Img_icon");
            _qua = transform.Find<Image>("Img_bg/Img_qua");
            _num = transform.Find<Text>("Img_bg/Text_num");
            _alreadyGet = transform.Find<Text>("Text_getTimes");
            _exchange = transform.Find<Button>("Btn_exchange");
            _cost = transform.Find<Text>("Btn_exchange/Text_cost");
            _objLimit = transform.Find("Img_limit").gameObject;

            _exchange.onClick.AddListener(On_exchangeClick);
        }
        private void On_exchangeClick()
        {
            var data = Cfg.Act2059.GetExchangeData(_info.id);
            if (!ItemHelper.IsCountEnoughWithFalseHandle(ItemId.PirateCoin, data.cost_coin, null))
            {
                return;
            }
            var opcode = PromptOpcode.ActExchange2059;
            bool prompt = PromptInfo.Instance.GetValue(opcode);
            if (!prompt)
            {
                _act2059.ExchangeItemByPirateCoin(_info, null);
            }
            else
            {
                _AlertWithPrompt.YesNo(string.Format(Lang.Get("是否花费{0}海盗金币兑换"), data.cost_coin), d =>
                {
                    d.SetYesCallbackWithPrompt(() =>
                    {
                        PromptInfo.Instance.SetPrompt(opcode, d.setPrompt);
                        _act2059.ExchangeItemByPirateCoin(_info, null);
                        d.Close();
                    });
                }, data.cost_coin);
            }
        }
        public void Refresh(P_BlackmarketItem info)
        {
            _info = info;
            _act2059 = (ActInfo_2059)ActivityManager.Instance.GetActivityInfo(ActId);

            var data = Cfg.Act2059.GetExchangeData(info.id);
            var pItem = GlobalUtils.ParseItem(data.item)[0];
            var item = ItemForShow.Create(pItem.id, pItem.count);
            item.SetIcon(_icon);
            _qua.color = _ColorConfig.GetQuaColorHSV(item.GetQua());
            _num.text = item.GetCount().ToString();

            if (data.limit_times == 0)
            {
                _alreadyGet.text = string.Format(Lang.Get("不限次兑换：<Color=#00ff00ff>{0}</Color>"),
                        info.exchange_times);
            }
            else
            {
                _alreadyGet.text = string.Format(Lang.Get("可兑换个数：<Color=#00ff00ff>{0}</Color>/{1}"),
                        info.exchange_times,
                        data.limit_times);
            }


            _icon.GetComponent<Button>().onClick.SetListener(() =>
            {
                ItemHelper.ShowTip(pItem.id, pItem.count, _icon.transform);
            });

            if (info.exchange_times >= data.limit_times && data.limit_times != 0)
            {
                _exchange.gameObject.SetActive(false);
                _objLimit.SetActive(true);
            }
            else
            {
                _exchange.gameObject.SetActive(true);
                _cost.text = data.cost_coin.ToString();
                _objLimit.SetActive(false);
            }

            _cost.rectTransform.sizeDelta = new Vector2(_cost.preferredWidth + 5, _cost.rectTransform.sizeDelta.y);
        }
    }
}


public class ActCoinDrawView2059 : TabViewBase2 //金币抽奖界面
{
    private Text _note;
    private Text _coins;
    private ListView _list;
    private Button _reset;
    private Text _resetText;
    private ActInfo_2059 _act2059;
    private const int ActId = 2059;
    public override void Awake()
    {
        base.Awake();
        _note = transform.Find<Text>("Text_note");
        _coins = transform.Find<Text>("Text_coin");
        _reset = transform.Find<Button>("Btn_reset");
        _resetText = transform.Find<Text>("Btn_reset/Text");
        var model = transform.Find("Icon/01").gameObject;
        _list = ListView.Create<CoinDrawItem>(transform.Find<RectTransform>("Icon"), model);
        _note.text = string.Format(Lang.Get("翻开一张卡牌消耗{0}金币，必定中奖哦~！"), Cfg.Act2059.LotteryDraw);
        _resetText.text = string.Format(Lang.Get("{0} 海盗金币重置"), Cfg.Act2059.ResetCost);

        _reset.onClick.AddListener(On_resetClick);
        InitEvents();
    }
    private void On_resetClick()
    {
        if (!ItemHelper.IsCountEnoughWithFalseHandle(ItemId.PirateCoin, Cfg.Act2059.ResetCost, null))
        {
            return;
        }
        var opcode = PromptOpcode.ActResetCard2059;
        bool prompt = PromptInfo.Instance.GetValue(opcode);
        if (!prompt)
        {
            _act2059.FreshLotteryDrawPool();
        }
        else
        {
            _AlertWithPrompt.YesNo(string.Format(Lang.Get("重置需要消耗{0}海盗金币，是否继续"), Cfg.Act2059.ResetCost), alert =>
            {
                alert.SetYesCallbackWithPrompt(() =>
                {
                    _act2059.FreshLotteryDrawPool();
                    PromptInfo.Instance.SetPrompt(opcode, alert.setPrompt);

                    alert.Close();
                });
            }, Cfg.Act2059.ResetCost);
        }
    }

    private void InitEvents()
    {
        EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshPirateCoin);
    }

    private void UnInitEvents()
    {
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(RefreshPirateCoin);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnInitEvents();
    }

    public override void Select()
    {
        base.Select();
        RefreshPirateCoin();

        _act2059 = (ActInfo_2059)ActivityManager.Instance.GetActivityInfo(ActId);
        var info = _act2059.Info.lotteryed_info;
        _list.Clear();
        for (int i = 0; i < 8; i++)
        {
            int temp = 0;
            if (info.TryGetValue(i.ToString(), out temp))
            {
                var data = Cfg.Act2059.GetLotteryData(temp);
                _list.AddItem<CoinDrawItem>().Refresh(i, data.item, _act2059);
            }
            else
            {
                _list.AddItem<CoinDrawItem>().Refresh(i, string.Empty, _act2059);
            }
        }
    }

    private void RefreshPirateCoin()
    {
        if (_coins)
        {
            var num = BagInfo.Instance.GetItemCount(ItemId.PirateCoin);
            _coins.text = string.Format(Lang.Get("拥有海盗金币数：{0}"), GLobal.NumFormat(num));
        }
    }

    private class CoinDrawItem : ListItem
    {
        private GameObject _busy;
        private GameObject _free;
        private Image _icon;
        private Text _num;
        private Button _drawReward;
        private Button _detail;
        private int _index;
        private ActInfo_2059 _act2059;

        private string _reward;
        private Sequence _seq;
        public override void OnCreate()
        {
            _busy = transform.Find("Busy").gameObject;
            _free = transform.Find("Free").gameObject;
            _icon = transform.Find<Image>("Busy/Img_icon");
            _detail = transform.Find<Button>("Busy");
            _num = transform.Find<Text>("Busy/Text_num");
            _drawReward = transform.Find<Button>("Free");

            _detail.onClick.AddListener(On_detailClick);
            _drawReward.onClick.AddListener(On_drawRewardClick);
        }
        private void On_detailClick()
        {
            if (string.IsNullOrEmpty(_reward))
                return;
            var pItem = GlobalUtils.ParseItem(_reward)[0];
            ItemHelper.ShowTip(pItem.id, pItem.count, _icon.transform);
        }
        private void On_drawRewardClick()
        {
            if (_seq != null && _seq.IsPlaying())
                return;
            if (!ItemHelper.IsCountEnoughWithFalseHandle(ItemId.PirateCoin, Cfg.Act2059.LotteryDraw, null))
            {
                return;
            }
            var opcode = PromptOpcode.NeedCoin2059;
            bool prompt = PromptInfo.Instance.GetValue(opcode);
            if (!prompt)
            {
                DrawCard();
            }
            else
            {
                _AlertWithPrompt.YesNo(string.Format(Lang.Get("翻开卡牌需要消耗{0}金币，是否继续"), Cfg.Act2059.LotteryDraw), alert =>
                {
                    alert.SetYesCallbackWithPrompt(() =>
                    {
                        DrawCard();
                        alert.Close();
                        PromptInfo.Instance.SetPrompt(opcode, alert.setPrompt);
                    });
                }, Cfg.Act2059.LotteryDraw);
            }
        }


        private void DrawCard()
        {
            _act2059.LotteryDrawByPirateCoin(_index, OnDrawCardByPirateCoinCB);
        }
        private void OnDrawCardByPirateCoinCB(string reward)
        {
            Refresh(_index, reward, _act2059);
        }
        public void Refresh(int index, string reward, ActInfo_2059 info)
        {
            _act2059 = info;
            _index = index;
            if (string.IsNullOrEmpty(reward))
            {
                if (NeedShowAnim(reward))
                {
                    _seq = DOTween.Sequence();
                    var tween1 = transform.DORotate(new Vector3(0, -90, 0), 0.2f, RotateMode.FastBeyond360).OnComplete(() =>
                    {
                        SetCardEmpty();
                        transform.localEulerAngles = new Vector3(0, 90, 0);
                    }).SetEase(Ease.Linear);

                    var tween2 = transform.DORotate(new Vector3(0, 0, 0), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
                    _seq.Append(tween1).Append(tween2);
                }
                else
                    SetCardEmpty();
            }
            else
            {
                if (NeedShowAnim(reward))
                {
                    _seq = DOTween.Sequence();
                    var tween1 = transform.DORotate(new Vector3(0, -90, 0), 0.2f, RotateMode.FastBeyond360).OnComplete(() =>
                    {
                        SetDrawReward(reward);
                        transform.localEulerAngles = new Vector3(0, 90, 0);
                    }).SetEase(Ease.Linear);

                    var tween2 = transform.DORotate(new Vector3(0, 0, 0), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
                    _seq.Append(tween1).Append(tween2);
                }
                else
                    SetDrawReward(reward);
            }
            _reward = reward;

        }

        private void SetDrawReward(string reward)
        {
            _busy.SetActive(true);
            _free.SetActive(false);
            var pItem = GlobalUtils.ParseItem(reward)[0];
            var item = ItemForShow.Create(pItem.id, pItem.count);
            item.SetIcon(_icon);
            _num.text = "x" + GLobal.NumFormat(item.GetCount());

        }
        private void SetCardEmpty()
        {
            _busy.SetActive(false);
            _free.SetActive(true);

        }
        private bool NeedShowAnim(string reward)
        {
            return _reward != reward && _reward != null;
        }
        public override void OnRemoveFromList()
        {
            base.OnRemoveFromList();
            if (_seq != null)
            {
                _seq.Kill();
                _seq = null;
            }
        }
    }

}
