using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2074_UI : ActivityUI
{
    private ActInfo_2074 _actInfo;
    private const int Aid = 2074;

    //标题和倒计时
    private Text _actTime;

    //一二三等奖 奖励预览
    private Act2074RewardLevelItem[] _list;

    //掀开抽奖
    private Act2074Grid[] _listDraw;

    //按钮 /  宝券剩余数量
    private const int TicketGetLimit = 50;//每日获取上限
    private Button _btnOpen5;//打开五个
    private Button _getTicket;//去获取宝券
    private Text _left;//剩余数量
    private Text _todayGetNum;//今天获取的
    private Button _helpBtn;
    private Sequence _seq;


    private ObjectGroup _obGroup;
    public override void OnCreate()
    {
        var root1 = transform.Find<RectTransform>("Inf01");
        _list = new Act2074RewardLevelItem[3]
        {
           root1.Find("01").gameObject.AddBehaviour<Act2074RewardLevelItem>(),
           root1.Find("02").gameObject.AddBehaviour<Act2074RewardLevelItem>(),
           root1.Find("03").gameObject.AddBehaviour<Act2074RewardLevelItem>(),
        };
        var root2 = transform.Find<RectTransform>("Inf02");
        _obGroup = transform.GetComponent<ObjectGroup>();
        _listDraw = new[]
        {
            root2.Find("model").gameObject.AddBehaviour<Act2074Grid>(),
            root2.Find("model (1)").gameObject.AddBehaviour<Act2074Grid>(),
            root2.Find("model (2)").gameObject.AddBehaviour<Act2074Grid>(),
            root2.Find("model (3)").gameObject.AddBehaviour<Act2074Grid>(),
            root2.Find("model (4)").gameObject.AddBehaviour<Act2074Grid>(),
        };
        _actTime = transform.Find<Text>("TimeText");
        _btnOpen5 = transform.Find<Button>("Btn_open5");
        _getTicket = transform.Find<Button>("Btn_get");
        _left = transform.Find<Text>("Text_left");
        _todayGetNum = transform.Find<Text>("Text_todayGetNum");
        _helpBtn = transform.Find<Button>("ButtonHelp");

        _btnOpen5.onClick.AddListener(On_btnOpen5Click);
        _getTicket.onClick.AddListener(On_getTicketClick);
        _helpBtn.onClick.SetListener(On_helpBtnClick);
    }
    private void On_btnOpen5Click()
    {
        if (IsDrawing())
            return;
        var forShow = ItemForShow.Create(ItemId.StarTicket, 5);
        bool enough = ItemHelper.IsCountEnough(ItemId.StarTicket, 5);
        if (enough)
        {
            _actInfo.DrawLotteryTicketFive();
        }
        else
        {
            Alert.Ok(Lang.Get("<color={0}>{1}</color>不足{2}",
                _ColorConfig.GetQuaColorText(forShow.GetQua()),
                forShow.GetName(),
                forShow.GetCount()));
        }
    }
    private void On_getTicketClick()
    {
        DialogManager.CloseAllDialog();
        //切换到世界场景
        if (_GameSceneManager.Instance.CurrentScene.Name != GameSceneName.World)
            _GameSceneManager.Instance.SwitchScene(GameSceneName.World);
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpBtnDialogShowAsynCB);
    }
    private void On_helpBtnDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2074, _helpBtn.transform.position, Direction.LeftDown, 350);
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshTicketNum);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(RefreshTicketNum);
    }

    public override void OnClose()
    {
        base.OnClose();
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

    public override void OnShow()
    {
        _actInfo = (ActInfo_2074)ActivityManager.Instance.GetActivityInfo(Aid);
        UpdateUi(Aid, true);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid, false);
    }

    private void UpdateUi(int aid, bool byShow = false)
    {
        if (Aid != aid)
            return;
        var info = _actInfo.GetInfo();

        //奖励预览
        for (int i = 0; i < _list.Length; i++)
        {
            _list[i].gameObject.SetActive(false);
        }
        var rewards = info.level_reward;
        for (int i = 0; i < rewards.Count; i++)
        {
            var rewardItem = _list[i];
            rewardItem.gameObject.SetActive(true);
            rewardItem.Refresh(rewards[i], i, _obGroup);
        }

        UpdateTime(TimeManager.ServerTimestamp);
        //抽奖部分
        _seq = DOTween.Sequence();

        //刷新抽奖结果
        if (info.draw_result.Count > 0 && !byShow)
        {
            AddRefreshAnim(info.draw_result);

            //需要合上再开一下
            if (info.drawFive && !_actInfo.IsAllClose())
            {
                _seq.AppendInterval(0.5f);
                CloseAllAnim();
                _seq.AppendInterval(0.2f);
            }

            //打开最后一个 需要重新合上(不需要再开了)
            if (_actInfo.IsAllClose())
            {
                _seq.AppendInterval(0.5f);
            }
        }
        //刷新奖池状态
        AddRefreshAnim(info.draw_list);

        //飘提示 奖品已通过邮件发送
        if (_actInfo.DrawResultHasReward() && !byShow)
        {
            _seq.AppendCallback(ShowRewardTip);
        }
        //飘提示 谢谢惠顾发的基地攻伐令
        string reward = info.get;
        if (!string.IsNullOrEmpty(reward) && !byShow)
        {
            _seq.AppendCallback(() => MessageManager.ShowRewards(reward));
        }
        _actInfo.ClearDrawResult();

        RefreshTicketNum();
        _todayGetNum.text = Lang.Get("今日获取宝券数：{0}/{1}", _actInfo.TodayGetTicketNum(), TicketGetLimit);
    }

    private void RefreshTicketNum()
    {
        _left.text = Lang.Get("剩余宝券：{0}", GLobal.NumFormat(BagInfo.Instance.GetItemCount(ItemId.StarTicket)));
    }
    private void ShowRewardTip()
    {
        MessageManager.Show(Lang.Get("奖品已通过邮件发送"));
    }

    //刷新5个待抽卡的状态
    private void AddRefreshAnim(Dictionary<string, int> waitDrawDic)
    {
        bool first = true;
        for (int i = 0; i < 5; i++)
        {
            int index = i + 1;
            int type = 0;
            if (waitDrawDic.TryGetValue(index.ToString(), out type))
            {
                // int type = waitDrawDic[index.ToString()];
                var t = ConvertToRewardType(type);
                if (first)
                {
                    _seq.Append(_listDraw[i].Refresh(t, index, _actInfo, _obGroup));
                    first = false;
                }
                else
                {
                    _seq.Join(_listDraw[i].Refresh(t, index, _actInfo, _obGroup));
                }
            }
        }
    }

    public bool IsDrawing()
    {
        return _seq != null && _seq.IsPlaying();
    }
    //抽完合上再抽
    private void CloseAllAnim()
    {
        for (int i = 0; i < 5; i++)
        {
            int index = i + 1;
            if (index == 1)
            {
                _seq.Append(_listDraw[i].Refresh(Reward2073Type.NotDraw, index, _actInfo, _obGroup));
            }
            else
            {
                _seq.Join(_listDraw[i].Refresh(Reward2073Type.NotDraw, index, _actInfo, _obGroup));
            }
        }
    }
    private Reward2073Type ConvertToRewardType(int t)
    {
        return (Reward2073Type)t;
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
    }
}
//一等二等三等 奖励预览
public class Act2074RewardLevelItem : JDBehaviour
{
    private Text _level;
    private ListView _list;//奖励物品
    private EffectTextGradient _gradient;//颜色渐变
    private Outline _outline;//外框
    private Image _bg;//每个奖背景不一样
    public override void Awake()
    {
        _level = transform.Find<Text>("Text_level");
        var root = transform.Find<RectTransform>("Content");
        _bg = transform.GetComponent<Image>();
        _list = ListView.Create<Act2074RewardItem>(root, root.GetChild(0).gameObject);
        _gradient = _level.GetComponent<EffectTextGradient>();
        _outline = _level.GetComponent<Outline>();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void Refresh(string rewards, int index, ObjectGroup obGroup)
    {
        switch (index)
        {
            case 0:
                _level.text = Lang.Get("一等奖");
                _gradient.colorTop = new Color(1, 247 / 255f, 198 / 255f);
                _outline.effectColor = new Color(116 / 255f, 1 / 255f, 49 / 255f);
                _bg.sprite = obGroup.Ref<Sprite>("Top_lv1");
                break;
            case 1:
                _level.text = Lang.Get("二等奖");
                _gradient.colorTop = new Color(1, 246 / 255f, 174 / 255f);
                _outline.effectColor = new Color(135 / 255f, 93 / 255f, 40 / 255f);
                _bg.sprite = obGroup.Ref<Sprite>("Top_lv2");
                break;
            case 2:
                _level.text = Lang.Get("三等奖");
                _gradient.colorTop = new Color(158 / 255f, 1, 1);
                _outline.effectColor = new Color(1 / 255f, 64 / 255f, 116 / 255f);
                _bg.sprite = obGroup.Ref<Sprite>("Top_lv3");
                break;
        }

        _list.Clear();
        if (string.IsNullOrEmpty(rewards))
            throw new Exception("activity 2074 level_reward should not null");

        var items = GlobalUtils.ParseItem(rewards);
        for (int i = 0; i < items.Length; i++)
        {
            _list.AddItem<Act2074RewardItem>().Refresh(items[i]);
        }
    }
}
public class Act2074RewardItem : ListItem
{
    private Image _icon;
    private Text _num;
    private Button _detail;
    private P_Item _item;
    public override void OnCreate()
    {
        _icon = transform.Find<Image>("Img_icon");
        _num = transform.Find<Text>("Text_num");
        _detail = transform.Find<Button>("Img_icon");

        _detail.onClick.AddListener(On_detailClick);
    }
    private void On_detailClick()
    {
        ItemHelper.ShowTip(_item.id, _item.count, transform);
    }

    public void Refresh(P_Item item)
    {
        _item = item;
        var forshow = ItemForShow.Create(item.id, item.count);
        forshow.SetIcon(_icon);
        _num.text = "x" + GLobal.NumFormat(item.count);
    }
}

//五个抽奖格子
public class Act2074Grid : JDBehaviour
{

    private ActInfo_2074 _actInfo;
    private int _index;
    protected Button _draw;
    //奖励等级
    protected Text _rewardLv;
    protected EffectTextGradient _gradient;//颜色渐变
    protected Outline _outline;//外框
    private Image _bg;//背景

    protected Transform _content;// 抽奖结果
    protected Transform _cover;//封面 (点击抽奖)

    private Reward2073Type _type = Reward2073Type.Undefined;
    private ObjectGroup _obGroup;
    public override void Awake()
    {
        _cover = transform.Find("cover");
        _draw = _cover.GetComponent<Button>();
        _draw.onClick.AddListener(DrawOneTicket);

        _content = transform.Find("content");
        _bg = transform.Find<Image>("content");

        _rewardLv = _content.Find<Text>("Title");

        _gradient = _rewardLv.GetComponent<EffectTextGradient>();
        _outline = _rewardLv.GetComponent<Outline>();
    }

    public Sequence Refresh(Reward2073Type nowT, int index, ActInfo_2074 info, ObjectGroup obGroup)
    {
        _obGroup = obGroup;
        _actInfo = info;
        _index = index;

        if (_type == nowT)
            return null;

        if (_type == Reward2073Type.Undefined)
        {
            _type = nowT;
            RefreshValue(nowT);
            return null;
        }
        else
        {
            _type = nowT;
            return AddSequence(nowT);
        }
    }

    private Sequence AddSequence(Reward2073Type t)
    {
        var seq = DOTween.Sequence();
        var tween4 = transform.DORotate(new Vector3(-90, 0, 0), 0.1f, RotateMode.FastBeyond360)
            .OnComplete(() =>
            {
                transform.transform.eulerAngles = new Vector3(90, 0, 0);
                RefreshValue(t);
            });
        var tween5 = transform.DORotate(new Vector3(0, 0, 0), 0.1f, RotateMode.FastBeyond360);
        seq.Append(tween4).Append(tween5);
        return seq;
    }

    private void RefreshValue(Reward2073Type t)
    {
        switch (t)
        {
            case Reward2073Type.Lv1:
                _rewardLv.fontSize = 24;
                _cover.gameObject.SetActive(false);
                _content.gameObject.SetActive(true);
                //名称
                _rewardLv.text = Lang.Get("一等奖");
                //按钮
                _draw.interactable = false;
                //渐变色
                _gradient.colorTop = new Color(1, 247 / 255f, 198 / 255f);
                //外框色
                _outline.effectColor = new Color(77 / 255f, 19 / 255f, 43 / 255f);
                //背景
                _bg.sprite = _obGroup.Ref<Sprite>("Bottom_lv1");
                break;
            case Reward2073Type.Lv2:
                _rewardLv.fontSize = 24;
                _cover.gameObject.SetActive(false);
                _content.gameObject.SetActive(true);
                _rewardLv.text = Lang.Get("二等奖");
                _draw.interactable = false;
                //渐变色
                _gradient.colorTop = new Color(1, 246 / 255f, 174 / 255f);
                //外框色
                _outline.effectColor = new Color(38 / 255f, 49 / 255f, 60 / 255f);
                //背景
                _bg.sprite = _obGroup.Ref<Sprite>("Bottom_lv2");
                break;
            case Reward2073Type.Lv3:
                _rewardLv.fontSize = 24;
                _cover.gameObject.SetActive(false);
                _content.gameObject.SetActive(true);
                _rewardLv.text = Lang.Get("三等奖");
                _draw.interactable = false;
                //渐变色
                _gradient.colorTop = new Color(158 / 255f, 1, 1);
                //外框色
                _outline.effectColor = new Color(1 / 255f, 64 / 255f, 116 / 255f);
                //背景
                _bg.sprite = _obGroup.Ref<Sprite>("Bottom_lv3");
                break;
            case Reward2073Type.NotDraw:
                _cover.gameObject.SetActive(true);
                _content.gameObject.SetActive(false);
                _rewardLv.fontSize = 32;
                _rewardLv.text = Lang.Get("点击揭开宝券");
                _draw.interactable = true;
                //渐变色
                _gradient.colorTop = new Color(158 / 255f, 1, 1);
                //外框色
                _outline.effectColor = new Color(1 / 255f, 38 / 255f, 116 / 255f);
                break;
            case Reward2073Type.Thx:
                _rewardLv.fontSize = 24;
                _cover.gameObject.SetActive(false);
                _content.gameObject.SetActive(true);
                _rewardLv.text = Lang.Get("谢谢惠顾");
                _draw.interactable = false;
                //渐变色
                _gradient.colorTop = new Color(158 / 255f, 1, 1);
                //外框色
                _outline.effectColor = new Color(1 / 255f, 64 / 255f, 116 / 255f);
                //背景
                _bg.sprite = _obGroup.Ref<Sprite>("Thanx");
                break;
        }

    }

    private void DrawOneTicket()
    {
        if (!_actInfo.IndexCanDraw(_index))
            return;


        var forShow = ItemForShow.Create(ItemId.StarTicket, 1);
        bool enough = ItemHelper.IsCountEnough(ItemId.StarTicket, 1);

        if (enough)
        {
            _actInfo.DrawLotteryTicketOne(_index);
        }
        else
        {
            Alert.Ok(Lang.Get("<color={0}>{1}</color>不足{2}",
                _ColorConfig.GetQuaColorText(forShow.GetQua()),
                forShow.GetName(),
                forShow.GetCount()));
        }
    }



    public interface ICardState
    {
        Reward2073Type GetRewardState();
        void RefreshValue();
        void AnimToState(ICardState cardState);
    }
}

