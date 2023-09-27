
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2204_UI : ActivityUI
{
    private _RewardBoxItem[] _rewardBoxs;//可点击的宝箱
    private JDText _timeText;//活动时间
    private JDText _txtTip;//签到天数提醒
    private JDText _txtDes;//活动描述
    private JDText _txtDay;//天数
    private JDText _txtGift;//签到礼
    private RecycleView _recycleRewards;
    private ActInfo_2204 _actInfo;
    private Transform _content;
    private int _aid = 2204;
    //特殊时间节点
    private const int daynode_5 = 5;
    private const int daynode_10 = 10;
    private const int daynode_15 = 15;
    private const int daynode_25 = 25;
    //Item的高
    private const float item_Height = 90;
    private List<cfg_act_2204> _cfgDatas = new List<cfg_act_2204>();
    private long _lastTime = -1;
    private void InitData()
    {
        _actInfo = (ActInfo_2204)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void OnCreate()
    {
        InitData();
        Transform scrollView = transform.Find("ScrollView");
        _recycleRewards = scrollView.GetComponent<RecycleView>();
        _recycleRewards.Init(onListRender);
        _content = scrollView.Find<Transform>("Viewport/Content");
        _timeText = transform.Find<JDText>("TimeText");
        _txtTip = transform.Find<JDText>("TextTip");
        _txtDes = transform.Find<JDText>("TextDes");
        _txtDay = transform.Find<JDText>("ScrollViewTitle/TextDay");
        _txtGift = transform.Find<JDText>("ScrollViewTitle/TextGift");

        _txtDes.text = Lang.Get(Cfg.Act.GetData(_aid).act_desc);
        _txtDay.text = Lang.Get("签到天数");
        _txtGift.text = Lang.Get("签到礼包");
        Transform icon = transform.Find("Icon");
        _rewardBoxs = new[]
        {
            new _RewardBoxItem(icon.Find("01"),daynode_5),
            new _RewardBoxItem(icon.Find("02"),daynode_10),
            new _RewardBoxItem(icon.Find("03"),daynode_15),
            new _RewardBoxItem(icon.Find("04"),daynode_25)
        };
        for (int i = 0; i < _rewardBoxs.Length; i++)
        {
            var j = i;
            _rewardBoxs[i]._btnGet.onClick.AddListener(() =>
            {

                if (_actInfo._dayCount < _rewardBoxs[j]._dayNumber)
                {
                    //不满足签到天数可预览
                    ShowDayNodeReward(_rewardBoxs[j]._dayNumber);
                }
                else
                {
                    if (!_actInfo.IsGetDayReward(_rewardBoxs[j]._dayNodeTid))
                    {
                        //执行获取奖励接口
                        _actInfo.GetAct2204Reward(_rewardBoxs[j]._dayNodeTid);
                    }
                    else
                    {
                        //领取完奖励可预览
                        ShowDayNodeReward(_rewardBoxs[j]._dayNumber);
                    }
                }
            });
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _cfgDatas.Clear();
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (_aid != aid)
            return;
        UpdateUi(false);
    }

    //显示节点宝箱的奖励
    private void ShowDayNodeReward(int dayNum)
    {
        var cfg_Data = Cfg.Activity2204.GetCfgDetail(dayNum, _actInfo._map_step);
        //DialogManager.ShowAsyn<_D_GetRewards>(d=>{ d?.ShowByRewards(GetBoxIconId(dayNum), cfg_Data.other_reward); });
        DialogManager.ShowAsyn<_D_ShowRewards>(d => { d?.ShowByRewards(GetBoxIconId(dayNum), cfg_Data.other_reward); });
    }
    //获取宝箱图片Id
    private int GetBoxIconId(int dayNode)
    {
        switch (dayNode)
        {
            case daynode_5:
                return ItemId.BlueHeroBuyGift;
            case daynode_10:
                return ItemId.GreenHeroBuyGift;
            case daynode_15:
                return ItemId.GoldHeroBuyGift;
            case daynode_25:
                return ItemId.RedHeroBuyGift;
            default:
                throw new Exception("find not dayNode:" + dayNode);
        }
    }
    public override void OnShow()
    {
        resetContentPosition();
        ActivityManager.Instance.RequestUpdateActivityById(_aid);//更新活动信息
        _actInfo.HasRemind = true;
        UpdateUi();
        UpdateTime(TimeManager.ServerTimestamp);
    }

    // 重置content位置
    private void resetContentPosition(){
        _content.DOLocalMoveY(1, 0.01f);
    }

    protected void onListRender(GameObject obj, int index)
    {
        if(obj == null) {
            return;
        }

        var info = _cfgDatas[index];
        if(info == null) {
            return;
        }

        var actItem = obj.GetComponent<Act2204Item>();
        if(actItem == null) {
            actItem = obj.AddComponent<Act2204Item>();
        }
        actItem.CreateItem();
        actItem.item.Refresh(info, _actInfo);
    }

    public void UpdateUi(bool bJump = true)
    {
        if(_cfgDatas != null) {
            _cfgDatas.Clear();
        }
        _cfgDatas = Cfg.Activity2204.GetCfgDetailList(_actInfo._map_step);
        _cfgDatas.Sort(Sort_act2204_day);//升序 
        _recycleRewards.ShowList(_cfgDatas.Count);
        _txtTip.text = Lang.Get("当前已累计签到{0}天", _actInfo._dayCount);
        //刷新节点宝箱状态
        for (int i = 0; i < _rewardBoxs.Length; i++)
        {
            _rewardBoxs[i].Refresh(_actInfo.IsGetDayReward(_rewardBoxs[i]._dayNodeTid), _actInfo._dayCount);
        }

        if(bJump)
        {
            AutoScrollToDayRow(_actInfo._dayCount);
        }
    }
    private int Sort_act2204_day(cfg_act_2204 a, cfg_act_2204 b)
    {
        return a.day - b.day;
    }

    //自动跳转到领奖的那天
    private void AutoScrollToDayRow(int day)
    {
        var length = day * (item_Height + _recycleRewards.col);
        var ViewPortL = item_Height * 3;
        var ViewProtH = (_cfgDatas.Count - 5) * (item_Height + _recycleRewards.col);

        if (length < ViewPortL)
        {
            _content.DOLocalMoveY(0, 0.5f);
            return;
        }
        length = length - ViewPortL;
        if (length > ViewProtH)
            length = ViewProtH;
        _content.DOLocalMoveY(length, 0.5f);
    }
    // public override void UpdateTime(long stamp)
    // {
    //     base.UpdateTime(stamp);
    //     if (gameObject == null || !gameObject.activeInHierarchy || _actInfo == null)
    //         return;

    //     if (_timeText != null)
    //     {
    //         if (stamp - _actInfo._data.startts < 0)
    //         {
    //             _timeText.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
    //         }
    //         else if (_actInfo.LeftTime >= 0)
    //         {
    //             TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
    //             _timeText.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
    //                 span.Minutes, span.Seconds);
    //         }
    //         else
    //         {
    //             _timeText.text = Lang.Get("活动已经结束");
    //         }
    //     }
    // }
}

public class _Act2204Item : ListItem
{
    private Button _btnGet;
    private GameObject _objAlreadyGet;
    private JDText _txtDay;
    private JDText _txtGet;
    private GameObject _getImg;
    private Image _bg;
    private Image[] _rewardIcons;
    private Image[] _rewardQua;
    private Text[] _rewardCount;
    private GameObject[] _rewardMask;
    private const int MAX_REWARD_COUNT = 4;
    private Color32 LockAlpha = new Color32(255,255,255,150);
    private Color32 UnLockAlpha = new Color32(255,255,255,255);
    private Color32 TxtLockAlpha = new Color32(0,255,255,84);
    private Color32 TxtUnLockAlpha = new Color32(0, 255, 255, 255);
    private int _doNum;//已经签到的天数
    private ActInfo_2204 _actInfo_2204;
    private cfg_act_2204 _info;
    public override void OnCreate()
    {
        _bg = transform.Find<Image>("Bg");
        _txtDay = transform.Find<JDText>("Text_Day");
        _getImg = transform.Find<GameObject>("getImg");
        _txtGet = transform.Find<JDText>("getImg/Text");
        _btnGet = transform.Find<Button>("BtnGet");
        _objAlreadyGet = transform.Find<GameObject>("BtnAlreadyGet");
        Transform item1 = transform.Find("01");
        Transform item2 = transform.Find("02");
        Transform item3 = transform.Find("03");
        Transform item4 = transform.Find("04");
        _rewardIcons = new[]
        {
           item1.FindImage("Icon"),
           item2.FindImage("Icon"),
           item3.FindImage("Icon"),
           item4.FindImage("Icon"),
        };
        _rewardQua = new[]
        {
           item1.FindImage("Qua"),
           item2.FindImage("Qua"),
           item3.FindImage("Qua"),
           item4.FindImage("Qua"),
        };
        _rewardCount = new[]
        {
           item1.FindText("Text"),
           item2.FindText("Text"),
           item3.FindText("Text"),
           item4.FindText("Text"),
        };
        _rewardMask = new[]
        {
            item1.Find<GameObject>("Mask"),
            item2.Find<GameObject>("Mask"),
            item3.Find<GameObject>("Mask"),
            item4.Find<GameObject>("Mask"),
        };
        _txtGet.text = Lang.Get("已领取");
        _btnGet.gameObject.SetActive(false);
        _objAlreadyGet.SetActive(false);
        _btnGet.onClick.AddListener(On_btnGetClick);
    }
    private void On_btnGetClick()
    {
        _actInfo_2204.GetAct2204Reward(_info.day);
    }
    public void Refresh(cfg_act_2204 info, ActInfo_2204 actInfo_2204)
    {
        _info = info;
        _actInfo_2204 = actInfo_2204;
        _doNum = actInfo_2204._dayCount;
        //刷新按钮状态
        UpdateUI();
    }
    private void UpdateUI()
    {
        _txtDay.text = Lang.Get("{0}", _info.day);
        
        string infoReward = _info.reward;
        if (_info.day == DateTime.DaysInMonth(TimeManager.ServerDateTime.Year, TimeManager.ServerDateTime.Month)) {
            infoReward = _info.last_reward;
        }
        var items = GlobalUtils.ParseItem(infoReward);
        int nLen = items.Length;
        //刷新奖励
        for (int i = 0, max = MAX_REWARD_COUNT; i < max; i++)
        {
            if(i < nLen) {
                _rewardIcons[i].gameObject.SetActive(true);
                _rewardQua[i].gameObject.SetActive(true);
                _rewardCount[i].gameObject.SetActive(true);
                _rewardMask[i].SetActive(true);

                var item = items[i];
                var showItem = ItemForShow.Create(item.id, item.count);
                showItem.SetIcon(_rewardIcons[i]);
                _rewardQua[i].color = _ColorConfig.GetQuaColorHSV(showItem.GetQua());
                _rewardCount[i].text = "x" + GLobal.NumFormat(showItem.GetCount());
                //添加道具描述
                var i1 = i;
                _rewardIcons[i].GetComponent<Button>().onClick.SetListener(() =>
                {
                    DialogManager.ShowAsyn<_D_ItemTip>(d=>{ d?.OnShow(item.id, item.count, _rewardIcons[i1].transform.position); });
                });
            }else {
                _rewardIcons[i].gameObject.SetActive(false);
                _rewardQua[i].gameObject.SetActive(false);
                _rewardCount[i].gameObject.SetActive(false);
                _rewardMask[i].SetActive(false);
            }
        }
        RefreshState();
    }
    //刷新状态，已领取和未领取
    private void RefreshState()
    {
        if (_info.day > _doNum)
        {
            //还未到达签到时间
            for (int i = 0; i < MAX_REWARD_COUNT; i++)
            {
                _rewardMask[i].SetActive(true);
            }
            _bg.color = LockAlpha;
            _txtDay.color = TxtLockAlpha;
            _getImg.SetActive(false);

            _objAlreadyGet.SetActive(false);
            _btnGet.gameObject.SetActive(false);
        }
        else
        {
            //已经到达时间
            for (int i = 0; i < MAX_REWARD_COUNT; i++)
            {
                _rewardMask[i].SetActive(false);
            }
            _bg.color = UnLockAlpha;
            _txtDay.color = TxtUnLockAlpha;
            if (_actInfo_2204.IsGetDayReward(_info.day))
            {
                //已经领取
                _getImg.SetActive(true);
                _objAlreadyGet.SetActive(true);
                _btnGet.gameObject.SetActive(false);
            }
            else
            {
                //未领取
                _getImg.SetActive(false);
                _objAlreadyGet.SetActive(false);
                _btnGet.gameObject.SetActive(true);
            }
        }
       
    }
}

public class Act2204Item : MonoBehaviour
{
    public _Act2204Item item = null;

    public void CreateItem()
    {
        if(item == null) {
            item = new _Act2204Item();
            item.gameObject = gameObject;
            item.OnCreate();
            item.OnAddToList();
        }
    }

    void OnDestroy()
    {
        if(item != null) {
            item.OnRemoveFromList();
        }
    }
}