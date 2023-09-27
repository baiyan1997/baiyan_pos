using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2063_UI : ActivityUI
{
    private Text _title;//标题
    private JDText _txtTime;//时间显示
    private Button[] _tabBtns;
    private ActInfo_2063 _actInfo;

    private BtnTabGrade[] _tabBtnGrade;
    private TabBtnHelper _tabBtnHelper;
    private int _selectGrade = 0;

    private const int ActId = 2063;
    private const int BtnCount = 2;

    private Act2063RewardPoolPanel _rewardPoolPanel;
    private GameObject _objRewardPoolPanel;

    private GameObject _objRewardPanel;//奖励展示界面
    private Button _btnConfirm;//奖励展示界面确认按钮
    private Text _txtRewardTitle;

    private Button _btnRewardPool;
    private GameObject _objEffect;//特效
    private GameObject _objEgg_Normal;//扭蛋特效
    private GameObject _objEgg_Senior;//扭蛋特效
    private Animator _animator_Normal;//滚动动画
    private Animator _animator_Senior;//滚动动画
    private Button _btnBuyOne;
    private Button _btnBuyTen;
    private Button _btnShop;
    private Image _imgRating;
    private Text _txtRating;
    private Text _txtOneGold;
    private Text _txtTenGold;
    private Text _txtRatingDes;
    private GameObject _objGuide;//引导
    private Text _txtGuideTip;//引导提示
    private GameObject _objRedPoint;//小红点
    private GameObject _objRewardItems;//抽奖池
    private Button _btnDetail;//详情
    private const int Lottery_One = 1;//抽一次
    private const int Lottery_Ten = 10;//十连抽
    private Color32 _color_Normal = new Color32(0, 192, 255, 255);
    private Color32 _color_Senior = new Color32(255, 206, 0, 255);
    private ListView _listReward;//奖励展示
                                 // private bool _canClick;//是否可点击
                                 //氪晶数量常量
    private const int NormalCostGold_One = 50;
    private const int NormalCostGold_Ten = 480;
    private const int SeniorCostGold_One = 150;
    private const int SeniorCostGold_Ten = 1440;

    //关于转盘
    private int _speed = 90;//转盘速度90
    private int _index = -1;//当前所在位置
    private int _gridsNum;//需要转的格子数[目前是五圈+到达目标的数值]
    private GameObject[] _selectEffect;//跑马灯特效
    private int A0 = 5;//转动加速度
    private List<int> indexRule = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };//转动规则
    private const int reWardNum = 10;
    private int ringNum = 10 * 5;
    private GameObject[] _objCandidacyItems;
    private RewardCandidacyItem[] _rewardCandidacyItems = new RewardCandidacyItem[reWardNum];//候选框
    private const string EffectName = "IsShowEffect";
    public override void OnCreate()
    {
        _txtTime = transform.Find<JDText>("Main/TimeText");
        _txtRatingDes = transform.Find<Text>("Main/RatingDes");
        _imgRating = transform.Find<Image>("Main/BtnRating");
        _txtRating = transform.Find<Text>("Main/BtnRating/Text");
        _objGuide = transform.Find<GameObject>("Main/Guide");
        _objRewardItems = transform.Find<GameObject>("Main/IconContent");
        _txtGuideTip = transform.Find<Text>("Main/Guide/Text/Text");
        _objRedPoint = transform.Find<GameObject>("Main/RedMind");
        _btnDetail = transform.Find<Button>("Main/BtnDetail");
        _objRewardPanel = transform.Find<GameObject>("Main/RewardPanel");
        _btnConfirm = transform.Find<Button>("Main/RewardPanel/confirm");
        _txtRewardTitle = transform.Find<Text>("Main/RewardPanel/reward_title");
        _objEgg_Normal = transform.Find<GameObject>("Main/IconContent/Img_Normal");
        _objEgg_Senior = transform.Find<GameObject>("Main/IconContent/Img_Senior");

        _animator_Normal = transform.Find<Animator>("Main/IconContent/Img_Normal/pfb_animation/ani_Normal_d");
        _animator_Senior = transform.Find<Animator>("Main/IconContent/Img_Senior/pfb_animation/ani_Senior_d");
        _tabBtns = new[]
        {
            transform.Find<Button>("Main/Btns/Btn_Tab1"),
            transform.Find<Button>("Main/Btns/Btn_Tab2"),
        };
        _objCandidacyItems = new[]
        {
            transform.Find<GameObject>("Main/IconContent/Icon1/1"),
            transform.Find<GameObject>("Main/IconContent/Icon2/1"),
            transform.Find<GameObject>("Main/IconContent/Icon3/1"),
            transform.Find<GameObject>("Main/IconContent/Icon4/1"),
            transform.Find<GameObject>("Main/IconContent/Icon5/1"),
            transform.Find<GameObject>("Main/IconContent/Icon6/1"),
            transform.Find<GameObject>("Main/IconContent/Icon7/1"),
            transform.Find<GameObject>("Main/IconContent/Icon8/1"),
            transform.Find<GameObject>("Main/IconContent/Icon9/1"),
            transform.Find<GameObject>("Main/IconContent/Icon10/1"),
        };
        _selectEffect = new[]
        {
            transform.Find<GameObject>("Main/IconContent/Icon1/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon2/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon3/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon4/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon5/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon6/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon7/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon8/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon9/Effect"),
            transform.Find<GameObject>("Main/IconContent/Icon10/Effect"),
        };
        for (int j = 0; j < _objCandidacyItems.Length; j++)
        {
            _rewardCandidacyItems[j] = new RewardCandidacyItem(_objCandidacyItems[j]);
        }
        _objRewardPoolPanel = transform.Find<GameObject>("ActRewardPoolPanel");
        _rewardPoolPanel = new Act2063RewardPoolPanel(_objRewardPoolPanel, RefreshReward);//

        var modelReward = transform.Find("Main/RewardPanel/IconContent/model0").gameObject;
        _listReward = ListView.Create<Act2063RewardItem>(transform.Find<RectTransform>("Main/RewardPanel/IconContent"), modelReward);

        _btnRewardPool = transform.Find<Button>("Main/BtnRating");
        _objEffect = transform.Find<GameObject>("Main/BtnRating/Effect");
        _btnBuyOne = transform.Find<Button>("Main/BtnBuyOne");
        _btnBuyTen = transform.Find<Button>("Main/BtnBuyTen");
        _btnShop = transform.Find<Button>("Main/BtnShop");
        _txtOneGold = transform.Find<Text>("Main/BtnBuyOne/costCount");
        _txtTenGold = transform.Find<Text>("Main/BtnBuyTen/costCount");
        _actInfo = (ActInfo_2063)ActivityManager.Instance.GetActivityInfo(ActId);
        _actInfo._canClick = true;
        _txtRating.text = Lang.Get("调整奖池");
        _txtRewardTitle.text = Lang.Get("恭喜您，获得以下物品");
        //TimeManager.Instance.TimePassSecond += RefreshTime;
        _tabBtnGrade = new BtnTabGrade[BtnCount];
        _tabBtnHelper = new TabBtnHelper();
        for (int i = 0; i < _tabBtns.Length; i++)
        {
            var _tabBtn = _tabBtns[i].gameObject.AddBehaviour<BtnTabGrade>();
            _tabBtnHelper.RegistTabBtn(_tabBtn, i);
        }
        _tabBtnHelper.OnTabSwitch += SwitchTab;
        _btnBuyOne.onClick.SetListener(On_btnBuyOneClick);
        _btnBuyTen.onClick.SetListener(On_btnBuyTenClick);
        _btnRewardPool.onClick.SetListener(On_btnRewardPoolClick);
        _btnShop.onClick.SetListener(On_btnShopClick);
        _btnDetail.onClick.AddListener(On_btnDetailClick);
        _btnConfirm.onClick.SetListener(On_btnConfirmClick);
    }
    private void On_btnBuyOneClick()
    {
        //买一次的接口
        StartLottery(Lottery_One);
    }
    private void On_btnBuyTenClick()
    {
        //买十次的接口
        StartLottery(Lottery_Ten);
    }
    private void On_btnRewardPoolClick()
    {
        if (!_actInfo._canClick)
        {
            MessageManager.Show(Lang.Get("正在抽奖中，请稍后操作！"));
            return;
        }
        //打开奖池界面
        _objRewardPoolPanel.SetActive(true);
        _rewardPoolPanel.OnShow();
        _objRedPoint.SetActive(false);
        _objEffect.SetActive(false);
    }
    private void On_btnShopClick()
    {
        //打开积分商店
        DialogManager.ShowAsyn<Act2063IntegralShopPanel>(On_btnShopDialogShowAsynCB);
    }
    private void On_btnShopDialogShowAsynCB(Act2063IntegralShopPanel d)
    {
        d?.OnShow();
    }
    private void On_btnDetailClick()
    {
        //详情显示
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnDetailDialogShowAsyn);
    }
    private void On_btnDetailDialogShowAsyn(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.EggMachine, _btnDetail.transform.position, Direction.LeftDown, 323);
    }
    private void On_btnConfirmClick()
    {
        //奖励信息确认
        _btnBuyOne.gameObject.SetActive(true);
        _btnBuyTen.gameObject.SetActive(true);
        _objRewardPanel.SetActive(false);
        RefreshReward();
    }




    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_rewardPoolPanel != null)
        {
            _rewardPoolPanel.OnDestroy();
            _rewardPoolPanel = null;
        }
        if (_tabBtnHelper != null)
        {
            _tabBtnHelper.OnDestroy();
            _tabBtnHelper = null;
        }
        _rewardCandidacyItems = null;
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    //时间
    public override void UpdateTime(long ts)
    {
        base.UpdateTime(ts);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _txtTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _txtTime.text = Lang.Get("活动已经结束");
        }
    }
    //高级与普通扭蛋机切换
    public void SwitchTab(int oldIndex, int newIndex)
    {
        _selectGrade = newIndex;
        _actInfo.type = (AdvanceType)_selectGrade;
        switch (_actInfo.type)
        {
            case AdvanceType.Normal:
                _txtOneGold.text = NormalCostGold_One.ToString();
                _txtTenGold.text = NormalCostGold_Ten.ToString();
                _txtRatingDes.text = Lang.Get("<Color=#00ffff>普通扭蛋机</Color>，奖品自己选，选择您中意的道具加入奖池 !试试您的运气吧！");
                UIHelper.SetImageSprite(_imgRating, "Button/btn_418");
                _txtRating.color = _color_Normal;
                _objEgg_Normal.SetActive(true);
                _objEgg_Senior.SetActive(false);
                break;
            case AdvanceType.Senior:
                _txtOneGold.text = SeniorCostGold_One.ToString();
                _txtTenGold.text = SeniorCostGold_Ten.ToString();
                _txtRatingDes.text = Lang.Get("<Color=#ffcc00ff>高级扭蛋机</Color>，奖品自己选，选择您中意的道具加入奖池 !试试您的运气吧！");
                UIHelper.SetImageSprite(_imgRating, "Button/btn_419");
                _txtRating.color = _color_Senior;
                _objEgg_Normal.SetActive(false);
                _objEgg_Senior.SetActive(true);
                break;
        }
        RefreshReward();
        RefreshRedPoint();
    }
    //进入选择奖励池特效红点
    private void RefreshRedPoint()
    {
        if (_actInfo.GetCandidacyRewardNum() < reWardNum)
        {
            _objRedPoint.SetActive(true);
            _objEffect.SetActive(true);
        }
        else
        {
            _objRedPoint.SetActive(false);
            _objEffect.SetActive(false);
        }
    }
    //刷新数据[普通和高级的奖励不同]
    private void RefreshReward()
    {
        var rewardList = _actInfo.GetCandidacyReward();
        if (rewardList.Count < reWardNum)
        {
            _objGuide.SetActive(true);
            _objRewardItems.SetActive(false);
            if (rewardList.Count != 0)
            {
                _txtGuideTip.text = Lang.Get("您还未完成奖励选择，请您选满10件奖励!");
            }
            else
            {
                _txtGuideTip.text = Lang.Get("指挥官，首次进入请先选择扭蛋奖池的奖品！");
            }
        }
        else
        {
            _objGuide.SetActive(false);
            _objRewardItems.SetActive(true);
            for (int i = 0; i < rewardList.Count; i++)
            {
                var data = GlobalUtils.ParseItem(rewardList[i])[0];
                var item = ItemForShow.Create(data.id, data.count);
                _rewardCandidacyItems[i]._itemId = data.id;
                item.SetIcon(_rewardCandidacyItems[i]._icon);
                _rewardCandidacyItems[i]._qua.color = _ColorConfig.GetQuaColorHSV(item.GetQua());
                _rewardCandidacyItems[i]._txtNum.text = Lang.Get("x{0}", GLobal.NumFormat(item.GetCount()).ToString());
            }
        }
        RefreshRedPoint();
    }
    #region 转盘抽奖模块
    //开始抽奖
    private void StartLottery(int lottery)
    {
        if (!_actInfo._canClick)
        {
            MessageManager.Show(Lang.Get("正在抽奖中，请稍后操作！"));
            return;
        }
        if (_actInfo.GetCandidacyRewardNum() < _rewardCandidacyItems.Length)
        {
            MessageManager.Show(Lang.Get("奖池未选择完成，请先选好奖励哦!"));
            return;
        }
        var d = Alert.YesNo(Lang.Get("确定花费{0}氪晶进行{1}次{2}扭蛋？", GetCostGold(lottery).ToString(), lottery, _actInfo.type == 0 ? "普通" : "高级"));
        d.SetYesCallback(() =>
        {
            if (ItemHelper.IsCountEnough(ItemId.Gold, GetCostGold(lottery)))
            {
                if (_actInfo._canClick)
                {
                    _actInfo._canClick = false;
                    //调用抽奖接口
                    _actInfo.ExtractMachine(lottery, (index, rewards) =>
                    {
                        //执行接口后
                        _Scheduler.Instance.StartCoroutine(Coroutine(index, () =>
                        {
                            //转盘结束后
                            _objRewardPanel.SetActive(true);
                            _objGuide.SetActive(false);
                            _objRewardItems.SetActive(false);
                            _btnBuyOne.gameObject.SetActive(false);
                            _btnBuyTen.gameObject.SetActive(false);

                            _listReward.Clear();
                            for (int i = 0; i < rewards.Count; i++)
                            {
                                _listReward.AddItem<Act2063RewardItem>().Refresh(rewards[i]);
                                Uinfo.Instance.AddItem(rewards[i].itemid, rewards[i].count);
                            }
                            MessageManager.ShowRewards(rewards);
                        }));
                    });
                }
                else
                {
                    MessageManager.Show(Lang.Get("正在抽奖中，请稍后操作！"));
                }
            }
            d.Close();
        });

    }
    //获取所要消耗得氪晶
    private int GetCostGold(int lottery)
    {
        if (_actInfo.type == AdvanceType.Senior)
        {
            //高级
            if (lottery == Lottery_One)
            {
                return SeniorCostGold_One;
            }
            else
            {
                return SeniorCostGold_Ten;
            }
        }
        else
        {
            //普通
            if (lottery == Lottery_One)
            {
                return NormalCostGold_One;
            }
            else
            {
                return NormalCostGold_Ten;
            }
        }

    }
    private IEnumerator Coroutine(int index, Action ac)
    {
        ShowOrStopLuckyDrawingEffect(true);
        float nowV = 0;
        _gridsNum = (GetGridsIndex(index) - _index) % reWardNum + ringNum;
        while (true)
        {
            if (nowV < _speed)
                nowV = nowV + A0;
            yield return new WaitForSeconds(1 / nowV);
            _index = _index + 1;
            _gridsNum = _gridsNum - 1;
            _selectEffect[indexRule[_index % reWardNum]].SetActive(true);
            _selectEffect[indexRule[(_index + reWardNum - 1) % reWardNum]].SetActive(false);
            if (_gridsNum <= 0)
                break;
        }
        ShowOrStopLuckyDrawingEffect(false);
        yield return new WaitForSeconds(0.5f);
        if (ac != null)
        {
            ac();
        }
        _actInfo._canClick = true;
    }
    private int GetGridsIndex(int rewardNum)
    {
        for (int i = 0; i < indexRule.Count; i++)
        {
            if (indexRule[i] == rewardNum)
            {
                return i;
            }
        }
        return -1;
    }
    #endregion
    //抽奖中特效
    private void ShowOrStopLuckyDrawingEffect(bool isShow)
    {
        switch (_actInfo.type)
        {
            case AdvanceType.Normal:
                _animator_Normal.SetBool(EffectName, isShow);
                break;
            case AdvanceType.Senior:
                _animator_Senior.SetBool(EffectName, isShow);
                break;
        }
    }
    public override void OnShow()
    {
        UpdateTime(TimeManager.ServerTimestamp);
        _tabBtnHelper.ClickBySpId(_selectGrade);
        _actInfo.type = (AdvanceType)_selectGrade;

        _btnBuyOne.gameObject.SetActive(true);
        _btnBuyTen.gameObject.SetActive(true);
        _objRewardPanel.SetActive(false);

        _objRewardPoolPanel.SetActive(false);

        RefreshReward();
    }

}
//奖励展示
public class Act2063RewardItem : ListItem
{
    private Image _icon;
    private Image _qua;
    private Button _btnTip;
    private Text _txtNum;
    public override void OnCreate()
    {
        _icon = transform.Find<Image>("icon");
        _qua = transform.Find<Image>("Img_qua");
        _btnTip = transform.Find<Button>("icon");
        _txtNum = transform.Find<Text>("Text");
    }
    public void Refresh(P_Item3 data)
    {
        var item = ItemForShow.Create(data.itemid, data.count);
        item.SetIcon(_icon);
        _qua.color = _ColorConfig.GetQuaColorHSV(item.GetQua());
        _txtNum.text = Lang.Get("x{0}", GLobal.NumFormat(data.count).ToString());
        _btnTip.onClick.SetListener(() =>
        {
            ItemHelper.ShowTip(data.itemid, data.count, this.transform);
        });
    }
}
//候选空格
public class RewardCandidacyItem
{
    public int _itemId;
    public Image _icon;
    public Image _qua;
    private GameObject _obj;
    private Button _btnDes;
    public Text _txtNum;
    public RewardCandidacyItem(GameObject obj)
    {
        _obj = obj;
        _icon = _obj.transform.Find<Image>("icon");
        _qua = _obj.transform.Find<Image>("Img_qua");
        _btnDes = obj.transform.Find<Button>("icon");
        _txtNum = obj.transform.Find<Text>("Text");
        _btnDes.onClick.SetListener(On_btnDesClick);
    }
    private void On_btnDesClick()
    {
        //显示tip
        ItemHelper.ShowTip(_itemId, 1, _icon.transform);

    }
}

//主界面等级按钮[普通 高级]
public class BtnTabGrade : TabBtnBase//TabButton
{
    private ObjectGroup _objGroup;
    public Sprite[] _sprite;
    private const int AcId = 2063;
    public override void Awake()
    {
        _objGroup = transform.parent.GetComponent<ObjectGroup>();
        _sprite = new[]
        {
            _objGroup.Sprite("BtnUnSelected"),
            _objGroup.Sprite("BtnSelected"),
        };
    }
    public override void Select()
    {
        transform.GetComponentInChildren<Text>().color = Color.white;
        GetButton().image.sprite = _sprite[1];
    }
    public override void Unselect()
    {
        transform.GetComponentInChildren<Text>().color = new Color(103 / 255f, 206 / 255f, 240 / 255f);
        GetButton().image.sprite = _sprite[0];
    }
    public override bool CanBeSelect()
    {
        var actInfo = (ActInfo_2063)ActivityManager.Instance.GetActivityInfo(AcId);
        return actInfo._canClick;
    }
}

