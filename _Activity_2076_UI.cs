using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2076_UI : ActivityUI
{
    private ObjectGroup UI;
    private JDText _textCountDown;
    private JDText _textJoinCount;
    private List<_2076Item> _itemList;
    private Button _btnHelp;
    private Button _btnJoin;
    private GameObject _krTag;
    private JDText _textJoinCost;

    private ActInfo_2076 _actInfo;
    private int _aid = 2076;

    public override void OnCreate()
    {
        UI = gameObject.GetComponent<ObjectGroup>();
        _textCountDown = UI.Get<JDText>("TextCountDown");
        _textJoinCount = UI.Get<JDText>("TextJoinCount");
        _btnHelp = UI.Get<Button>("BtnHelp");
        _btnJoin = UI.Get<Button>("BtnJoin");
        _krTag = UI.Get<GameObject>("KrTag");
        _textJoinCost = UI.Get<JDText>("TextJoinCost");
        _itemList = new List<_2076Item>();
        var itemRoot = UI.Get<Transform>("Icon_Mid");
        for (int i = 0; i < itemRoot.childCount; i++)
            _itemList.Add(new _2076Item(itemRoot.GetChild(i)));
        InitData();
        //点击参与夺宝
        _btnJoin.onClick.AddListener(On_btnJoinClick);
        _btnHelp.onClick.AddListener(On_btnHelpClick);
    }
    private void On_btnJoinClick()
    {
        var alert = Alert.YesNo(Lang.Get("是否花费{0}氪晶参与星际夺宝？", _actInfo.CostGold));
        alert.SetYesCallback(() =>
        {
            alert.Close();
            if (ItemHelper.IsCountEnoughWithFalseHandle(ItemId.Gold, _actInfo.CostGold, null))
            {
                _actInfo.SeizeTreasure(Refresh);
            }
        });
    }
    private void On_btnHelpClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnHelpDialogShowAsynCB);
    }
    private void On_btnHelpDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2076, _btnHelp.transform.position, Direction.LeftDown, 400);
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2076)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.RemindActivity.AddListener(OnRemind);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.RemindActivity.RemoveListener(OnRemind);
    }

    public override void OnShow()
    {
        //每次进入界面刷新数据
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
        Refresh();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    private void Refresh()
    {
        InitData();
        if (_actInfo == null)
            return;
        UpdateJoinCount();
        UpdateTime(0);
        UpdateRewardPool();
        UpdateJoinBtn();
    }

    private void UpdateJoinCount()
    {
        _textJoinCount.text = Lang.Get("参与指挥官人数达到{1}全部解锁（<Color={2}>{0}</Color>/{1}）", _actInfo.JoinNum, _actInfo.MaxJoinNum, "#00ff66");
    }

    public override void UpdateTime(long ts)
    {
        base.UpdateTime(ts);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!DialogManager.IsDialogShown<_D_ActCalendar>() || _actInfo == null || _actInfo._data == null || !_actInfo.IsDuration())
            return;
        var leftTs = _actInfo._data.endts - TimeManager.ServerTimestamp;
        leftTs = leftTs < 0 ? 0 : leftTs;
        _textCountDown.text = Lang.Get("活动倒计时 {0}", GLobal.TimeFormat(leftTs, true));
    }

    //刷新奖励池
    private void UpdateRewardPool()
    {
        //普通奖励
        var normalData = _actInfo.NormalRewardPool;
        for (int i = 0; i < normalData.Length; i++)
        {
            var item = _itemList[i];
            var data = normalData[i];
            item.Refresh(data.id, data.count, false);
        }
        //超过30人参与解锁特殊奖励
        var specialData = _actInfo.SpecialRewardPool;
        for (int i = 0; i < specialData.Length; i++)
        {
            var item = _itemList[i + normalData.Length];
            var data = specialData[i];
            item.Refresh(data.id, data.count, _actInfo.JoinNum < _actInfo.MaxJoinNum);
        }
    }

    private void UpdateJoinBtn()
    {
        if (_actInfo.CanJoin)
        {
            _btnJoin.interactable = true;
            _krTag.SetActive(true);
            _textJoinCost.text = Lang.Get("{0}参与夺宝", _actInfo.CostGold);
        }
        else
        {
            _btnJoin.interactable = false;
            _krTag.SetActive(false);
            _textJoinCost.text = Lang.Get("已参与夺宝");
        }
    }

    private void OnRemind(int aid, bool remind)
    {
        if (aid == _aid)
        {
            Refresh();
        }
    }
}

class _2076Item
{
    public Transform transform;
    private Image _icon;
    private Text _textCount;
    private Transform _lock;
    private Button _btnDetail;

    private int _itemId;
    private int _count;

    public _2076Item(Transform t)
    {
        transform = t;
        _icon = t.Find<Image>("Img_icon");
        _textCount = t.Find<Text>("Text_num");
        _lock = t.Find("Lock");
        _btnDetail = t.GetComponent<Button>();
        _btnDetail.onClick.SetListener(On_btnDetailClick);
    }
    private void On_btnDetailClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_btnDetailDialogShowAsynCB);
    }
    private void On_btnDetailDialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(_itemId, _count, transform.position);
    }
    public void Refresh(int itemId, int count, bool isLock = false)
    {
        _itemId = itemId;
        _count = count;
        Cfg.Item.SetItemIcon(_icon, itemId);
        _textCount.text = Lang.Get("x{0}", count);
        if (_lock != null)
        {
            _lock.gameObject.SetActive(isLock);
        }
    }
}
