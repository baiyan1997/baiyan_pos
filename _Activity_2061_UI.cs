using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2061_UI : ActivityUI
{
    private Text _des;
    private Text _leftTime;
    private ListView _rewardList;
    private Text _tipText;
    private Text _tittle;

    private ActInfo_2061 _actInfo;
    private int _aid = 2061;

    public override void OnCreate()
    {
        _des = transform.Find<JDText>("TextDes");
        _rewardList = ListView.Create<_Act2061Item>(transform.Find("ScrollView"));
        _leftTime = transform.Find<JDText>("TimeText");
        _tipText = transform.Find<JDText>("TextTip");
        _tittle = transform.Find<JDText>("Title");

        InitData();
        Init();
        //InitListener();
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2061)ActivityManager.Instance.GetActivityInfo(_aid);
    }
    private void Init()
    {
        _des.text = _actInfo._desc;
        _tittle.text = _actInfo._name;
    }

    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
        //TimeManager.Instance.TimePassSecond += UpdateTime;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void OnShow()
    {
        UpdateUi(_aid);
        UpdateTime(TimeManager.ServerTimestamp);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid);
    }

    private void UpdateUi(int aid)
    {
        if (aid == _aid)
        {
            _rewardList.Clear();
            for (int i = 0; i < _actInfo.itemList.Count; i++)
            {
                _rewardList.AddItem<_Act2061Item>()
                    .Refresh(_actInfo.itemList[i], _actInfo);
            }

            _tipText.text = Lang.Get("当前已累计登陆{0}天", _actInfo.Day);
        }
    }

    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy || _actInfo == null)
            return;
        if (_leftTime != null)
        {
            if (stamp - _actInfo._data.startts < 0)
            {
                _leftTime.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
            }
            else if (_actInfo.LeftTime >= 0)
            {
                TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
                _leftTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                    span.Minutes, span.Seconds);
            }
            else
            {
                _leftTime.text = Lang.Get("活动已经结束");
            }
        }
    }
}

public class _Act2061Item : ListItem
{
    private int _day;
    private Text _textTime;
    private ListView _list;
    private Button _getBtn;
    private GameObject _claimedGo;
    private Transform _trans1;
    private Transform _trans2;

    private Dictionary<int, int> _status;//0未达成 1未领奖 2已领奖
    private ActInfo_2061 _actInfo;
    _Act2061RewardItem _item1;
    _Act2061RewardItem _item2;


    public override void OnCreate()
    {
        _textTime = transform.Find<Text>("dateTime");
        _getBtn = transform.FindButton("BtnGet");
        _claimedGo = transform.Find("Btn_claimed").gameObject;
        _trans1 = transform.Find("01");
        _trans2 = transform.Find("02");

        _item1 = new _Act2061RewardItem(_trans1);
        _item2 = new _Act2061RewardItem(_trans2);

        _getBtn.onClick.SetListener(On_getBtnClick);
    }
    private void On_getBtnClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);
        if (_actInfo != null)
        {
            _actInfo.GetRewardById(_day);
        }
    }
    public void Refresh(P_Act2061Item itemdData, ActInfo_2061 actInfo)
    {
        _actInfo = actInfo;
        _textTime.text = itemdData.dayIndex.ToString();
        _day = itemdData.dayIndex;

        _item1.Refresh(itemdData.rewards[0], itemdData.statu);
        _item2.Refresh(itemdData.rewards[1], itemdData.statu);

        switch (itemdData.statu)//1未达成 0未领奖 2已领奖
        {
            case 1:
                _getBtn.gameObject.SetActive(false);
                _claimedGo.SetActive(false);
                break;
            case 0:
                _getBtn.gameObject.SetActive(true);
                _claimedGo.SetActive(false);
                break;
            case 2:
                _getBtn.gameObject.SetActive(false);
                _claimedGo.SetActive(true);
                break;
        }
    }

}

public class _Act2061RewardItem
{
    private Transform _trans;
    private Image _icon;
    private Image _iconQua;
    private Text _text;
    private GameObject _maskGo;

    public _Act2061RewardItem(Transform trans)
    {
        _trans = trans;
        _icon = _trans.Find<Image>("Icon");
        _iconQua = _trans.Find<Image>("Qua");
        _text = _trans.Find<Text>("TextCount");
        _maskGo = _trans.Find<GameObject>("Mask");
    }

    public void Refresh(P_Item3 reward, int state)
    {
        var itemShow = ItemForShow.Create(reward.itemid, reward.count);
        itemShow.SetIcon(_icon);
        _iconQua.color = _ColorConfig.GetQuaColorHSV(itemShow.GetQua());
        _text.text = "x" + GLobal.NumFormat(itemShow.GetCount());
        _trans.GetComponent<Button>().onClick.SetListener(() =>
        {
            ItemHelper.ShowTip(reward.itemid, reward.count, _trans);
        });

        switch (state)
        {
            case 0:
                _maskGo.SetActive(false);
                break;
            case 1:
            case 2:
                _maskGo.SetActive(true);
                break;
            default:
                throw new Exception("can't find reward state " + state);
        }
    }
}


