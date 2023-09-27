
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2058_UI : ActivityUI
{
    private _RewardBoxItem[] _rewardBoxs;//可点击的宝箱
    private JDText _timeText;//活动时间
    private JDText _txtTip;//签到天数提醒
    private JDText _txtDes;//活动描述
    private JDText _txtDay;//天数
    private JDText _txtGift;//签到礼
    // private ListView _listRewards;
    private RecycleView _recycleRewards;
    private ActInfo_2058 _actInfo;
    private Transform _content;
    // private VerticalLayoutGroup _verticalGroup;
    private int _aid = 2058;
    //特殊时间节点
    private const int daynode_5 = 5;
    private const int daynode_10 = 10;
    private const int daynode_15 = 15;
    private const int daynode_25 = 25;
    //Item的高
    private const float item_Height = 90;
    private List<cfg_act_2058> _cfgDatas = new List<cfg_act_2058>();
    private void InitData()
    {
        _actInfo = (ActInfo_2058)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void OnCreate()
    {
        InitData();
        // _listRewards = ListView.Create<_Act2058Item>(transform.Find("ScrollView"));
        Transform scrollView = transform.Find("ScrollView");
        _recycleRewards = scrollView.GetComponent<RecycleView>();
        _recycleRewards.Init(onListRender);
        _content = scrollView.Find<Transform>("Viewport/Content");
        // _verticalGroup = transform.Find<VerticalLayoutGroup>("ScrollView/Viewport/Content");
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
                        _actInfo.GetAct2058Reward(_rewardBoxs[j]._dayNodeTid);
                    }
                    else
                    {
                        //领取完奖励可预览
                        ShowDayNodeReward(_rewardBoxs[j]._dayNumber);
                    }
                }
            });
        }
        //InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUI);
        //TimeManager.Instance.TimePassSecond += UpdateTime;
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
        UpdateUi();
    }

    //显示节点宝箱的奖励
    private void ShowDayNodeReward(int dayNum)
    {
        var cfg_Data = Cfg.Activity2058.GetCfgDetail(dayNum, _actInfo._map_step);
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
        _actInfo.HasRemind = true;
        UpdateUi();
        UpdateTime(TimeManager.ServerTimestamp);
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

        var actItem = obj.GetComponent<Act2058Item>();
        if(actItem == null) {
            actItem = obj.AddComponent<Act2058Item>();
        }
        actItem.CreateItem();
        actItem.item.Refresh(info, _actInfo);
    }

    public void UpdateUi()
    {
        // _listRewards.Clear();
        if(_cfgDatas != null) {
            _cfgDatas.Clear();
        }
        _cfgDatas = Cfg.Activity2058.GetCfgDetailList(_actInfo._map_step);
        _cfgDatas.Sort(Sort_act2058_day);//升序 
        // for (int i = 0, max = cfg_Data.Count; i < max; i++)
        // {
        //     var itemInfo = cfg_Data[i];
        //     _listRewards.AddItem<_Act2058Item>().Refresh(itemInfo, _actInfo);
        // }
        _recycleRewards.ShowList(_cfgDatas.Count);
        _txtTip.text = Lang.Get("当前已累计签到{0}天", _actInfo._dayCount);
        //刷新节点宝箱状态
        for (int i = 0; i < _rewardBoxs.Length; i++)
        {
            _rewardBoxs[i].Refresh(_actInfo.IsGetDayReward(_rewardBoxs[i]._dayNodeTid), _actInfo._dayCount);
        }

        AutoScrollToDayRow(_actInfo._dayCount);
    }
    private int Sort_act2058_day(cfg_act_2058 a, cfg_act_2058 b)
    {
        return a.day - b.day;
    }

    //自动跳转到领奖的那天
    private void AutoScrollToDayRow(int day)
    {
        var length = day * (item_Height + _recycleRewards.col);
        var ViewPortL = item_Height * 3;
        // var ViewProtH = (_listRewards._listItems.Count - 5) * (item_Height + _verticalGroup.spacing);
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
    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy || _actInfo == null)
            return;
        if (_timeText != null)
        {
            if (stamp - _actInfo._data.startts < 0)
            {
                _timeText.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
            }
            else if (_actInfo.LeftTime >= 0)
            {
                TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
                _timeText.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                    span.Minutes, span.Seconds);
            }
            else
            {
                _timeText.text = Lang.Get("活动已经结束");
            }
        }
    }
}
public class _RewardBoxItem
{
    public Button _btnGet;
    public int _dayNodeTid;//也是tid
    public int _dayNumber;//真实的天数
    private GameObject _getImg;
    private GameObject _canGetImg;
    private JDText _txtRefrom;
    private JDText _txtGet;
    private JDText _txtCanGet;
    private GameObject _objEffect;
    private Color32 TxtGetColor = new Color32(0, 255, 153, 255);
    private Color32 TxtNormalColor = new Color32(0, 255, 255, 255);
    private const int LerpNum = 100;
    private bool _objEffectLoaded = false;
    private bool state = false;
    public _RewardBoxItem(Transform trans, int daynum)
    {
        _btnGet = trans.Find<Button>("BoxImg");
        _txtRefrom = trans.Find<JDText>("Text");
        _txtGet = trans.Find<JDText>("GetImg/Text");
        _txtCanGet = trans.Find<JDText>("CanGetImg/Text");
        _getImg = trans.Find<GameObject>("GetImg");
        _canGetImg = trans.Find<GameObject>("CanGetImg");

        LoadEffect(trans);
        _dayNodeTid = daynum + LerpNum;
        _dayNumber = daynum;
        _txtRefrom.text = Lang.Get("{0}天", _dayNumber);
        _txtGet.text = Lang.Get("已领取");
        _txtCanGet.text = Lang.Get("可领取");
        _canGetImg.SetActive(false);
        _getImg.SetActive(false);
        state = false;
        // _objEffect.SetActive(false);
    }

    private async void LoadEffect(Transform parent)
    {
        _objEffect = null;
        var obj = await GetEffect(parent);
        if (obj != null)
        {
            obj.transform.localPosition = Vector3.zero;
            _objEffect = obj;
            UpdateState();
        }
    }

    public async UniTask<GameObject> GetEffect(Transform parent)
    {
        _objEffectLoaded = false;
        var ret = await ResHelper.LoadInstanceByUniTask("PFB_UI_warships_02", parent);
        _objEffectLoaded = true;
        return ret;
    }

    private async void UpdateState()
    {
        await UniTask.WaitUntil(() => _objEffectLoaded);
        if (_objEffectLoaded == true && _objEffect != null)
        {
            _objEffect.SetActive(state);
        }
    }

    //设置特效
    // private async GameObject SetEffect(Transform parent)
    // {
    //     var go = JDResources.Load<GameObject>("PFB_UI_warships_02");
    //     var obj = GameObject.Instantiate(go, parent);
    //     obj.transform.localPosition = Vector3.zero;
    //     return obj;
    // }
    //刷新box的状态
    public void Refresh(bool isGetReward, int dayCount)
    {
        if (isGetReward)
        {
            //已领取
            _getImg.SetActive(true);
            _txtRefrom.color = TxtGetColor;
            _canGetImg.SetActive(false);
            // _objEffect.SetActive(false);
            state = false;
            UpdateState();
        }
        else
        {
            //未领取
            _getImg.SetActive(false);
            _txtRefrom.color = TxtNormalColor;
            _canGetImg.SetActive(dayCount >= _dayNumber);
            // _objEffect.SetActive(dayCount >= _dayNumber);
            state = dayCount >= _dayNumber;
            UpdateState();
        }
    }
}

public class Act2058Item : MonoBehaviour
{
    public _Act2058Item item = null;

    public void CreateItem()
    {
        if(item == null) {
            item = new _Act2058Item();
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

