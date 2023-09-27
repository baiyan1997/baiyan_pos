using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
public class _Activity_2047_UI : ActivityUI
{
    private const int Aid = 2047;
    private ActInfo_2047 _actInfo;
    private Text _actTime;
    private Image[] _rankImg;//势力值排名信息
    private Text[] _rankPoints;//势力值数据
    private Text _descText;
    private Button _tipsBtn;
    private GameObject[] _bottoms;

    private Text _timeText;
    private Text _pointsText;
    private ListView _list;
    private Act2047BuyReward[] _itemModel;
    private Text _rankText;
    private Button _getBtn;
    private GameObject _gotTip;
    private GameObject _warnTip;

    private Sprite[] _spRank;
    private Sprite[] _spRank1;
    private ObjectGroup _ui;
    private List<P_Item5> _itemDatas;
    public override void OnCreate()
    {
        _ui = gameObject.GetComponent<ObjectGroup>();
        _actTime = transform.FindText("Text_Time");
        _rankImg = new[]
        {
            transform.FindImage("EmpireRankImg"),
            transform.FindImage("FederationRankImg"),
            transform.FindImage("RepublicRankImg"),
        };
        _rankPoints = new[]
        {
            transform.FindText("EmpirePoints"),
            transform.FindText("FederationPoints"),
            transform.FindText("RepublicPoints"),
        };
        _descText = transform.FindText("DescText");
        _tipsBtn = transform.FindButton("Tips");
        _bottoms = new[]
        {
            transform.Find("Bottom1").gameObject,
            transform.Find("Bottom2").gameObject,
        };
        _timeText = transform.FindText("Bottom1/TimeText");
        _pointsText = transform.FindText("Bottom1/PointsText");
        _list = ListView.Create<Act2047BuyReward>(transform.Find("Bottom1/buyRewards"));
        _rankText = transform.FindText("Bottom2/RankText");
        _getBtn = transform.FindButton("Bottom2/GetBtn");
        _gotTip = transform.Find("Bottom2/GotTip").gameObject;
        _warnTip = transform.Find("Bottom2/WarnTip").gameObject;

        _spRank = new[]
        {
             _ui.Ref<Sprite>("_SpFirst"),
             _ui.Ref<Sprite>("_SpSecond"),
             _ui.Ref<Sprite>("_SpThird"),
        };
        _spRank1 = new[]
        {
             _ui.Ref<Sprite>("_SpFirst1"),
             _ui.Ref<Sprite>("_SpSecond1"),
             _ui.Ref<Sprite>("_SpThird1"),
        };
        InitData();
        InitEvent();
        //InitListener();
        InitUi();
    }
    public override void OnShow()
    {
        _actInfo.RefreshAct();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    private void InitData()
    {
        _actInfo = (ActInfo_2047)ActivityManager.Instance.GetActivityInfo(2047);
        _itemDatas = new List<P_Item5>();
    }
    private void InitEvent()
    {
        _getBtn.onClick.AddListener(On_getBtnClick);
        _tipsBtn.onClick.AddListener(On_tipsBtnClick);
    }
    private void On_getBtnClick()
    {
        _actInfo.GetRewardFromState(On_getBtnRewardFromStateCB);
    }
    private void On_getBtnRewardFromStateCB()
    {
        _getBtn.gameObject.SetActive(false);
        _gotTip.SetActive(true);
    }
    private void On_tipsBtnClick()
    {
        if (_itemDatas != null && _itemDatas.Count > 0)
            DialogManager.ShowAsyn<ActRewardList>(On_tipsBtnDialogShowAsynCB);
    }
    private void On_tipsBtnDialogShowAsynCB(ActRewardList d)
    {
        d?.OnShow(_itemDatas, Lang.Get("国家排名奖励"));
    }
    private static string GetKeyName(int num)
    {
        switch (num)
        {
            case 1:
                return "first";
            case 2:
                return "second";
            case 3:
                return "third";
            default:
                return "first";
        }
    }
    public override void InitListener()
    {
        base.InitListener();
    }
    private void InitUi()
    {
        UpdateTime(TimeManager.ServerTimestamp);
        _descText.text = _actInfo._desc;
        _itemModel = new Act2047BuyReward[4];
        for (int i = 0; i < _actInfo.BuyInfo.Count; i++)
        {
            _itemModel[i] = _list.AddItem<Act2047BuyReward>();
            _itemModel[i].InitUi(_actInfo.BuyInfo[i]);
        }

        _itemDatas.Clear();
        for (int i = 0; i < _actInfo.RankItems.Count; i++)
        {
            P_Item5 item5 = new P_Item5()
            {
                Title = Lang.Get("第{0}名", i + 1),
                get_items = GlobalUtils.ParseItem3(_actInfo.RankItems[GetKeyName(i + 1)]),
            };
            _itemDatas.Add(item5);
        }
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (!_pointsText)
            return;
        if (aid == Aid)
        {
            _bottoms[0].SetActive(!_actInfo.IsClosing());
            _bottoms[1].SetActive(_actInfo.IsClosing());
            for (int i = 0; i < 3; i++)
            {
                _rankImg[i].gameObject.SetActive(_actInfo.IsClosing());
                var rank = _actInfo.GetStateRank(i + 1);
                if (rank != -1)
                {
                    _rankImg[i].sprite = i == 2 ? _spRank[rank] : _spRank1[rank];
                }
                _rankPoints[i].text = Lang.Get("势力值: {0}", _actInfo.GetStatePoint(i + 1));
            }
            for (int i = 0; i < _actInfo.BuyInfo.Count; i++)
            {
                _itemModel[i].Refresh(_actInfo.BuyInfo[i], _actInfo);
            }
            _pointsText.text = Lang.Get("当前持有活跃点数: {0}", _actInfo.ActivePoint);
            _rankText.text = Lang.Get("您的国家最终排名: {0}", _actInfo.GetStateRank(PlayerInfo.Instance.Info.ustate) + 1);
            _getBtn.gameObject.SetActive(_actInfo.CanGetReward == 0);
            _gotTip.SetActive(_actInfo.CanGetReward == 1);
            _warnTip.SetActive(_actInfo.CanGetReward == 2);
        }
    }
    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (st - _actInfo._data.startts < 0)
        {
            _actTime.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _actTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _actTime.text = Lang.Get("活动已经结束");
        }
        var getRewardTime = _actInfo._data.endts - ActInfo_2047.CloseTime - st;
        if (getRewardTime > 0)
        {
            TimeSpan sp = new TimeSpan(0, 0, (int)getRewardTime);
            _timeText.text = string.Format(Lang.Get("{0}天{1}小时{2}分{3}秒后结算势力排名"), sp.Days, sp.Hours,
                sp.Minutes, sp.Seconds);
        }
        else
        {
            _timeText.text = "";
        }
    }
}

public class Act2047BuyReward : ListItem
{
    private Image _qua;
    private Image _icon;
    private Text _count;
    private Text _times;
    private Button[] _btns;
    private Text[] _btnTexts;
    private ActInfo_2047 _actInfo;
    private Act2047BuyInfo _buyInfo;
    public override void OnCreate()
    {
        _qua = transform.FindImage("qua");
        _icon = transform.FindImage("icon");
        _count = transform.FindText("count");
        _times = transform.FindText("times");
        _btns = new[]
        {
            transform.FindButton("Button"),
            transform.FindButton("GetButton"),
        };
        _btnTexts = new[]
        {
            transform.FindText("Button/Text"),
            transform.FindText("GetButton/Text"),
        };

        _btns[0].onClick.AddListener(On_btns0Click);
        _btns[1].onClick.AddListener(On_btns1Click);
    }
    private void On_btns0Click()
    {
        MessageManager.Show(_buyInfo.already_buy < _buyInfo.buy_count
              ? Lang.Get("当前持有活跃点数不足")
              : Lang.Get("当前兑换道具兑换次数已达上限"));
    }
    private void On_btns1Click()
    {
        _actInfo.ExchangeItem(_buyInfo.id);
    }

    public void Refresh(Act2047BuyInfo buyInfo, ActInfo_2047 actInfo)
    {
        _actInfo = actInfo;
        _buyInfo = buyInfo;
        _times.text = string.Format("{0}/{1}", _buyInfo.already_buy, _buyInfo.buy_count);
        if (_buyInfo.already_buy < _buyInfo.buy_count)
        {
            if (_buyInfo.cost > _actInfo.ActivePoint)
            {
                _btns[0].gameObject.SetActive(true);
                _btns[1].gameObject.SetActive(false);
            }
            else
            {
                _btns[0].gameObject.SetActive(false);
                _btns[1].gameObject.SetActive(true);
            }
        }
        else
        {
            _btns[0].gameObject.SetActive(true);
            _btns[1].gameObject.SetActive(false);
        }
    }
    public void InitUi(Act2047BuyInfo buyInfo)
    {
        var itemId = int.Parse(buyInfo.item.Split('|')[0]);
        var count = int.Parse(buyInfo.item.Split('|')[1]);
        _qua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(itemId));
        Cfg.Item.SetItemIcon(_icon, itemId);
        _count.text = "x" + GLobal.NumFormat(count);
        _btnTexts[0].text = Lang.Get("{0}点活跃度", buyInfo.cost);
        _btnTexts[1].text = Lang.Get("{0}点活跃度", buyInfo.cost);
        _icon.GetComponent<Button>().onClick.SetListener(() =>
        {
            ItemHelper.ShowTip(itemId, count, _icon.transform);
        });
    }
}

