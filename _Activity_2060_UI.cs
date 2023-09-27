using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2060_UI : ActivityUI
{
    private Text _title;
    private JDText _txtTime;//时间显示
    private Button[] _tabBtns;
    private GameObject[] _tabViews;
    private ActInfo_2060 _actInfo;
    private const int ActId = 2060;

    private TabHelper _tabViewHelper;
    private TabBtnHelper _tabBtnHelper;
    private EffectTextGradient _txtEffectColor;
    private Outline _txtOutLine;

    private Color32 txtEffect1 = new Color32(255, 254, 210, 255);
    private Color32 txtOutLine1 = new Color32(98, 50, 50, 255);

    private Color32 txtEffect2 = new Color32(210, 252, 255, 255);
    private Color32 txtOutLine2 = new Color32(19, 53, 101, 255);

    public override void OnCreate()
    {
        _txtTime = transform.Find<JDText>("Text_time");
        _txtEffectColor = transform.Find<EffectTextGradient>("Text_time");
        _txtOutLine = transform.Find<Outline>("Text_time");
        _tabBtns = new[]
        {
            transform.Find<Button>("Btns/Btn_Tab1"),
            transform.Find<Button>("Btns/Btn_Tab2"),
        };
        _tabViews = new[]
        {
            transform.Find("Tab01").gameObject,
            transform.Find("Tab02").gameObject,
        };
        _tabViewHelper = new TabHelper();
        _tabBtnHelper = new TabBtnHelper();
        for (int i = 0; i < _tabBtns.Length; i++)
        {
            _tabBtnHelper.RegistTabBtn(_tabBtns[i].gameObject.AddBehaviour<BtnTab2060>(), i);
            _tabBtnHelper.RegistTabBtn(_tabBtns[i].gameObject.AddBehaviour<BtnTab2060>(), i);
        }
        for (int i = 0; i < _tabViews.Length; i++)
        {
            _tabViews[i].SetActive(false);
        }
        _tabBtnHelper.OnTabSwitch += SwitchTab;

        //TimeManager.Instance.TimePassSecond += RefreshTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(aid =>
        //{
        //    if (aid != ActId)
        //        return;
        //    var tab = _tabViewHelper.GetCurrentTab(true);
        //    tab.Select();//刷新信息
        //});
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
    public override void UpdateTime(long ts)
    {
        base.UpdateTime(ts);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_txtTime != null && _actInfo != null)
        {
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
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);

        if (aid != ActId)
            return;
        _actInfo = (ActInfo_2060)ActivityManager.Instance.GetActivityInfo(ActId);
        if (_tabViewHelper == null)
        {
            return;
        }
        var tab = _tabViewHelper.GetCurrentTab(true);
        if (tab == null)
        {
            return;
        }
        tab.Select();//刷新信息
    }

    public void SwitchTab(int oldIndex, int newIndex)
    {
        if (_tabViewHelper == null)
        {
            return;
        }
        var view = _tabViewHelper.GetTabBySpId(newIndex, true);
        if (view == null)
        {
            if (newIndex == 0)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActGiftBoxView2060>(), newIndex);
            if (newIndex == 1)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActHolidayView2060>(), newIndex);
        }
        _tabViewHelper.ClickBySpId(newIndex);

        if (newIndex == 0)
        {
            _txtEffectColor.colorTop = txtEffect1;
            _txtOutLine.effectColor = txtOutLine1;
        }
        else
        {
            _txtEffectColor.colorTop = txtEffect2;
            _txtOutLine.effectColor = txtOutLine2;
        }
    }
    public override void OnShow()
    {
        _actInfo = (ActInfo_2060)ActivityManager.Instance.GetActivityInfo(ActId);
        UpdateTime(TimeManager.ServerTimestamp);
        _tabBtnHelper.Finish();
        var tab = _tabViewHelper.GetCurrentTab(true);
        tab.Select();//刷新信息
    }
}

public class BtnTab2060 : TabBtnBase//TabButton
{
    private ObjectGroup _objGroup;
    public Sprite[] _sprite;
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
}
public class ActGiftBoxView2060 : TabViewBase2 //礼盒界面
{
    private Button _btnDetail;//详情
    private Button _btnTreasureNotes;//使用宝券
    private JDText _txtTreasureNotes;
    private Button _btnBuyCarnival;
    private JDText _txtBuyCarnival;
    private JDText _txtCurrentLv;//当前等级
    private Slider _sliderLv;//等级进度条
    private JDText _txtSlider;//进度条等级显示
    private JDText _txtRemainTicket;//剩余宝券
    private int _currentstep;//当前进程
    private int _celebrationTicketNum;//庆典宝券数量
    private Image _head;//头像
    private int _remain = 0;//半级宝券数量
    private int _totalCount = 0;//宝券总数
    private JDText _txtLvTitle;
    private JDText _txtCelebrationTitle;
    private JDText _txtCarnivalTitle;
    private ActInfo_2060 _actInfo;
    private ListView _list;
    private const int ActId = 2060;
    private const float MAXLEVEL = 15;//最大等级
    private const int MAX_MONEY = 5000;
    private bool isInit = true;

    public override void Awake()
    {
        base.Awake();
        _list = ListView.Create<RewardGiftItem>(transform.Find("ScrollView"));
        _txtLvTitle = transform.Find<JDText>("Title/LvText");
        _txtCelebrationTitle = transform.Find<JDText>("Title/CelebrationText");
        _txtCarnivalTitle = transform.Find<JDText>("Title/CarnivalText");

        _txtRemainTicket = transform.Find<JDText>("ReaminText");
        _sliderLv = transform.Find<Slider>("Slider");
        _txtSlider = transform.Find<JDText>("Slider/Time");
        _txtCurrentLv = transform.Find<JDText>("CurrentLvText");
        _btnDetail = transform.Find<Button>("BtnDetail");
        _btnTreasureNotes = transform.Find<Button>("Btns/BtnTreasureNotes");
        _txtTreasureNotes = transform.Find<JDText>("Btns/BtnTreasureNotes/Text");
        _btnBuyCarnival = transform.Find<Button>("Btns/BtnBuyCarnival");
        _txtBuyCarnival = transform.Find<JDText>("Btns/BtnBuyCarnival/Text");
        _head = transform.Find<Image>("Img_head");

        _txtTreasureNotes.text = Lang.Get("使用宝券");
        string str = "购买";
        _txtBuyCarnival.text = Lang.Get("{0}狂欢礼盒", str);
        _txtLvTitle.text = Lang.Get("等级");
        _txtCelebrationTitle.text = Lang.Get("庆典礼盒");
        _txtCarnivalTitle.text = Lang.Get("狂欢礼盒");

        _btnDetail.onClick.AddListener(On_btnDetailClick);
        _btnTreasureNotes.onClick.AddListener(On_btnTreasureNotesClick);
        _btnBuyCarnival.onClick.AddListener(On_btnBuyCarnivalClick);
        InitEvents();
    }
    private void On_btnDetailClick()
    {
        //详情显示
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(d => { d?.OnShow(HelpType.CelebrationGift, new Vector3(0, 3, 100), Direction.RightDown, 323); });
    }
    private void On_btnTreasureNotesClick()
    {
        if (_actInfo.Info.curLv < MAXLEVEL)
        {
            var count = (int)BagInfo.Instance.GetItemCount(ItemId.CelebrationTicket);
            if (count > 0)
            {
                //使用宝券
                _actInfo.UpGradeBox(On_btnTreasureNotesUpGradeCB);
            }
        }
    }
    private void On_btnTreasureNotesUpGradeCB(P_Lottery2060 info)
    {
        _totalCount = _remain + GlobalUtils.ParseItem(info.cost_item)[0].count;
        ShowUpGradeEffect(info);
    }
    private void On_btnBuyCarnivalClick()
    {
        var d = Alert.YesNo(Lang.Get("是否花费{0}氪晶解锁狂欢礼盒？", MAX_MONEY));
        d.SetYesButonText(Lang.Get("确定"));
        d.SetNoCallback(() =>
        {
            d.Close();
        });
        d.SetYesCallback(() =>
        {
            d.Close();
            var itemGoldEnough = ItemHelper.IsCountEnough(ItemId.Gold, MAX_MONEY);
            if (itemGoldEnough)
            {
                //解锁狂欢礼盒
                _actInfo.UnLockCarnivalBox();
            }
        });
    }
    private void InitEvents()
    {
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdatePlayerItem);
    }

    private void UnInitEvents()
    {
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdatePlayerItem);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        UnInitEvents();
    }

    private void UpdatePlayerItem()
    {
        var dia = DialogManager.GetInstanceOfDialog<_D_ActCalendar>();
        if (dia != null && dia.IsShowing)
        {
            ActivityManager.Instance.RequestUpdateActivityById(ActId);
            RefreshCelebrationTickets();
        }
    }

    //Slider 动画特效和文字跳动特效
    private void ShowUpGradeEffect(P_Lottery2060 info)
    {
        if (info.preLv <= info.nextLv)
        {
            var nextLevel = (int)Mathf.Clamp(info.preLv + 1, 0, MAXLEVEL);
            var dataLimt = Cfg.Act2060.GetGiftReward(Act_GiftsType.CelebrationGift, _currentstep, nextLevel).scroll_count;
            _sliderLv.maxValue = dataLimt;
            _sliderLv.DOValue(_totalCount, 0.3f).OnUpdate(() =>
            {
                _txtSlider.text = Lang.Get("{0}/{1}", _sliderLv.value, dataLimt);
            }).OnComplete(() =>
            {
                if (_totalCount >= dataLimt)
                {
                    _sliderLv.value = 0;
                    info.preLv++;
                    _totalCount = _totalCount - dataLimt;
                    ShowUpGradeEffect(info);
                }
                else
                {
                    _remain = _totalCount;
                }
            }).SetEase(Ease.Linear);
        }
    }

    public override void Select()
    {
        base.Select();
        Cfg.Role.SetHeadIcon(_head, Uinfo.Instance.Player.Info.uphoto, 1);
        _actInfo = (ActInfo_2060)ActivityManager.Instance.GetActivityInfo(ActId);
        if (_actInfo == null)
        {
            return;
        }
        if (isInit)
        {
            var nextLevel = (int)Mathf.Clamp(_actInfo.Info.curLv + 1, _actInfo.Info.curLv, MAXLEVEL);
            var data = Cfg.Act2060.GetGiftReward(Act_GiftsType.CelebrationGift, _currentstep, nextLevel);
            _txtSlider.text = Lang.Get("{0}/{1}", _actInfo.Info.progress, data.scroll_count);
            _sliderLv.maxValue = data.scroll_count;
            _sliderLv.value = _actInfo.Info.progress;
            _remain = _actInfo.Info.progress;
            isInit = false;
        }
        _txtCurrentLv.text = Lang.Get("等级{0}", _actInfo.Info.curLv);
        _currentstep = _actInfo.Info.step;
        RefreshCelebrationTickets();
        _list.Clear();
        var celebrationDataDic = Cfg.Act2060.GetGiftReward(Act_GiftsType.CelebrationGift, _currentstep);
        var lvList = celebrationDataDic.Keys.ToList();
        lvList.Sort(Sort_lv);
        var carnivalDataDic = Cfg.Act2060.GetGiftReward(Act_GiftsType.CarnivalGift, _currentstep);
        //数量等同采用一个
        for (int i = 0; i < lvList.Count; i++)
        {
            var lv = lvList[i];
            _list.AddItem<RewardGiftItem>().Refresh(celebrationDataDic[lv], carnivalDataDic[lv], lv, _actInfo, CallBackGetReward);
        }

        if (_actInfo.Info.curLv >= MAXLEVEL)
        {
            _btnTreasureNotes.interactable = false;
            _txtTreasureNotes.text = Lang.Get("已到顶级");
        }
        //未解锁时显示
        _btnBuyCarnival.gameObject.SetActive(_actInfo.Info.unlock == 0 && _actInfo.Info.curLv != 0);
    }
    private int Sort_lv(int a, int b)
    {
        return a - b;
    }


    //获取奖励
    private void CallBackGetReward(Act_GiftsType giftsType, int lv)
    {
        var data = Cfg.Act2060.GetGiftReward(giftsType, _currentstep, lv);
        _actInfo.GetAct2060Reward(data.tid);
    }
    //刷新庆典宝券
    private void RefreshCelebrationTickets()
    {
        _celebrationTicketNum = (int)BagInfo.Instance.GetItemCount(ItemId.CelebrationTicket);
        _txtRemainTicket.text = string.Format(Lang.Get("剩余宝券：{0}"), GLobal.NumFormat(_celebrationTicketNum));
    }

    private class RewardGiftItem : ListItem
    {
        private Text _txtLevel;//等级显示
        private int _level;//等级
        private ActInfo_2060 _act2060;
        //庆典礼盒
        private GameObject _objIcon;
        private Image _icon;
        private Image _qua;
        private Text _num;
        private GameObject _mask;
        private GameObject _objGet;
        // private GameObject _objEffect;
        //狂欢礼盒
        private GameObject[] _objIcons;
        private Image[] _rewardIcons;
        private Image[] _rewardQua;
        private JDText[] _rewardCount;
        private GameObject[] _masks;
        private GameObject[] _objGets;
        private GameObject[] _objEffects;
        private const int MAX_REWARD_COUNT = 2;
        private const int MAX_MONEY = 5000;
        private bool[] states = new bool[3];

        private Color32 TxtLockColor = new Color32(104, 225, 255, 120);
        private Color32 TxtNormalColor = new Color32(104, 225, 255, 255);
        public override void OnCreate()
        {
            _objIcon = transform.Find<GameObject>("Icon1");
            _icon = transform.Find<Image>("Icon1/Img_icon");
            _qua = transform.Find<Image>("Icon1/Img_qua");
            _num = transform.Find<Text>("Icon1/Text_num");
            _mask = transform.Find<GameObject>("Icon1/Mask");
            _objGet = transform.Find<GameObject>("Icon1/Img_get");

            _objEffects = new GameObject[3];


            _objIcons = new[]
            {
                 transform.Find<GameObject>("Icon2"),
                 transform.Find<GameObject>("Icon3")
            };
            _rewardIcons = new[]
            {
                transform.FindImage("Icon2/Img_icon"),
                transform.FindImage("Icon3/Img_icon")
            };
            _rewardQua = new[]
            {
                 transform.FindImage("Icon2/Img_qua") ,
                 transform.FindImage("Icon3/Img_qua")
            };
            _rewardCount = new[]
            {
                transform.Find<JDText>("Icon2/Text_num"),
                transform.Find<JDText>("Icon3/Text_num")
             };
            _masks = new[]
            {
                transform.Find<GameObject>("Icon2/Mask"),
                 transform.Find<GameObject>("Icon3/Mask")
            };
            _objGets = new[]
            {
                transform.Find<GameObject>("Icon2/Img_get"),
                transform.Find<GameObject>("Icon3/Img_get")
            };

            LoadEffect(_objIcon.transform, 2);
            LoadEffect(_objIcons[0].transform, 0);
            LoadEffect(_objIcons[1].transform, 1);


            // _objEffects = new[]
            // {
            //     LoadEffect(_objIcons[0].transform),
            //     LoadEffect(_objIcons[1].transform)
            // };
            _txtLevel = transform.Find<JDText>("LvText");
            _objIcon.SetActive(false);

            _objGet.SetActive(false);
            _objGets[0].SetActive(false);
            _objGets[1].SetActive(false);

            // _objEffect.SetActive(false);
            // _objEffects[0].SetActive(false);
            // _objEffects[1].SetActive(false);
            for (int i = 0; i < states.Length; i++)
                states[i] = false;

            UpdateState();
        }

        private async void LoadEffect(Transform parent, int idx)
        {
            // _objEffect = null
            var obj = await ResHelper.LoadInstanceByUniTask("PFB_Scroll View2", parent);
            obj.transform.localPosition = Vector3.zero;
            // _objEffect = obj;
            _objEffects[idx] = obj;
            UpdateState();
        }

        private void UpdateState(int idx, bool state)
        {
            states[idx] = state;
            UpdateState(idx);
        }

        private void UpdateState(int idx)
        {
            _objEffects[idx]?.SetActive(states[idx]);
        }

        private void UpdateState()
        {
            for (int i = 0; i < states.Length; i++)
                UpdateState(i);
        }

        //设置特效
        // private GameObject SetEffect(Transform parent)
        // {
        //     var go = JDResources.Load<GameObject>("PFB_Scroll View2");
        //     var obj = GameObject.Instantiate(go, parent);
        //     obj.transform.localPosition = Vector3.zero;
        //     return obj;
        // }
        //解锁狂欢礼盒
        private void UnLockCarnivalGift()
        {
            var d = Alert.YesNo(Lang.Get("是否花费{0}氪晶解锁狂欢礼盒？", MAX_MONEY));
            d.SetYesButonText(Lang.Get("确定"));
            d.SetNoCallback(() =>
            {
                d.Close();
            });
            d.SetYesCallback(() =>
            {
                d.Close();
                var itemGoldEnough = ItemHelper.IsCountEnough(ItemId.Gold, MAX_MONEY);
                if (itemGoldEnough)
                {
                    //解锁狂欢礼盒
                    _act2060.UnLockCarnivalBox();
                }
            });

        }
        public void Refresh(cfg_act_2060_box data1, cfg_act_2060_box data2, int lv, ActInfo_2060 info, Action<Act_GiftsType, int> callBack)
        {
            _act2060 = info;
            _level = lv;
            _txtLevel.text = Lang.Get("{0}", lv.ToString());
            //庆典礼包
            _objIcon.SetActive(true);
            var pItem = GlobalUtils.ParseItem(data1.items)[0];
            var item = ItemForShow.Create(pItem.id, pItem.count);
            item.SetIcon(_icon);
            _qua.color = _ColorConfig.GetQuaColorHSV(item.GetQua());
            _num.text = "x" + GLobal.NumFormat(item.GetCount());
            _icon.GetComponent<Button>().onClick.SetListener(() =>
            {
                if (_act2060.Info.curLv >= lv && !_act2060.IsGetReward(data1.tid))
                {
                    //获取奖励
                    if (callBack != null)
                    {
                        callBack(Act_GiftsType.CelebrationGift, lv);
                    }
                }
                else
                {
                    //显示tip
                    DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(pItem.id, pItem.count, _icon.transform.position); });
                }
            });

            //狂欢礼包
            var items = GlobalUtils.ParseItem(data2.items);
            for (int i = 0, max = items.Length; i < max; i++)
            {
                _objIcons[i].SetActive(true);
                var itemData = items[i];
                var showItem = ItemForShow.Create(itemData.id, itemData.count);
                showItem.SetIcon(_rewardIcons[i]);
                _rewardQua[i].color = _ColorConfig.GetQuaColorHSV(showItem.GetQua());
                _rewardCount[i].text = "x" + GLobal.NumFormat(showItem.GetCount());
                //添加道具描述
                var i1 = i;
                _rewardIcons[i].GetComponent<Button>().onClick.SetListener(() =>
                {
                    if (_act2060.Info.unlock != 0)
                    {
                        if (_act2060.Info.curLv >= lv && !_act2060.IsGetReward(data2.tid))
                        {
                            //获取奖励
                            if (callBack != null)
                            {
                                callBack(Act_GiftsType.CarnivalGift, lv);
                            }
                        }
                        else
                        {
                            //显示tip
                            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(itemData.id, itemData.count, _rewardIcons[i1].transform.position); });
                        }
                    }
                    else
                    {
                        if (_act2060.Info.curLv != 0)
                        {
                            //解锁狂欢礼盒
                            UnLockCarnivalGift();
                        }
                        else
                        {
                            //显示tip
                            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(itemData.id, itemData.count, _rewardIcons[i1].transform.position); });
                        }

                    }
                });
            }

            RefreshState(data1.tid, data2.tid, items.Length);
        }
        //刷新Icon得状态
        private void RefreshState(int data1Tid, int data2Tid, int max)
        {
            if (_act2060.Info.curLv < _level)
            {
                //未到等级
                _mask.SetActive(true);
                _txtLevel.color = TxtLockColor;
                for (int i = 0; i < max; i++)
                {
                    _masks[i].SetActive(true);
                }

            }
            else
            {
                //已到等级
                _mask.SetActive(false);
                if (_act2060.IsGetReward(data1Tid))
                {
                    //已经领取
                    _objGet.SetActive(true);
                    UpdateState(2, false);
                    // _objEffect.SetActive(false);
                }
                else
                {
                    UpdateState(2, true);
                    //未领取
                    // _objEffect.SetActive(true);
                }

                _txtLevel.color = TxtNormalColor;
                if (_act2060.Info.unlock != 0)
                {
                    //解锁
                    for (int i = 0; i < max; i++)
                    {
                        _masks[i].SetActive(false);
                        if (_act2060.IsGetReward(data2Tid))
                        {
                            //已经领取
                            _objGets[i].SetActive(true);
                            UpdateState(i, false);
                            // _objEffects[i].SetActive(false);
                        }
                        else
                        {
                            UpdateState(i, true);
                            //未领取
                            // _objEffects[i].SetActive(true);
                        }

                    }
                }
                else
                {
                    //未解锁
                    for (int i = 0; i < max; i++)
                    {
                        _masks[i].SetActive(true);
                    }
                }

            }
        }
    }
}

public class ActHolidayView2060 : TabViewBase2 //海盗假日界面
{
    private Text _get;
    private Button _btnJump;
    private JDText _txtJump;
    private ActInfo_2060 _actInfo;
    private Text _txtTis;
    private JDText _txtDes;
    private const int ActId = 2060;
    private const int LimiteNum = 80;//今日上上限
    public override void Awake()
    {
        base.Awake();
        _txtTis = transform.Find<Text>("TipText");
        _get = transform.Find<Text>("Text_get");
        _btnJump = transform.Find<Button>("Btn");
        _txtJump = transform.Find<JDText>("Btn/Text");
        _txtDes = transform.Find<JDText>("Text_title");

        _txtDes.text = Lang.Get(Cfg.Act.GetData(ActId).act_desc);

        _btnJump.onClick.AddListener(On_btnJumpClick);
        _txtTis.text = Lang.Get("每日0点重置");
        _txtJump.text = Lang.Get("前往击杀");
    }
    private void On_btnJumpClick()
    {
        DialogManager.CloseAllDialog();
        var basePos = PlayerInfo.Instance.Info.castle_id;
        WorldTrigger.Inst.SetNextTargetPos(basePos);
        if (_GameSceneManager.Instance.CurrentScene.Name != GameSceneName.World)
        {
            _GameSceneManager.Instance.SwitchScene(GameSceneName.World);
        }
    }


    public override void Select()
    {
        base.Select();
        _actInfo = (ActInfo_2060)ActivityManager.Instance.GetActivityInfo(ActId);
        if (_actInfo == null)
        {
            return;
        }
        _get.text = Lang.Get("今日庆典宝券数量:{0}/{1}", _actInfo.Info.pirate_drop, LimiteNum);
        if (_actInfo.Info.pirate_drop >= LimiteNum)
        {
            _btnJump.interactable = false;
            _txtJump.text = Lang.Get("宝券获取达到上限");
        }
        else
        {
            _btnJump.interactable = true;
            _txtJump.text = Lang.Get("前往击杀");
        }
    }
}

