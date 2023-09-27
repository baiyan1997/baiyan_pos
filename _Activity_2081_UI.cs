using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2081_UI : ActivityUI
{
    private JDText _textDesc;
    private JDText _textCountDown;
    private Button _btnHelpTips;

    private Button _btnSelectSupportArm;//选择辅助武装入口
    private Button _btnSupportArm;//辅助武装入口
    private Button _btnActMission;//活动任务入口
    private Button _btnActShop;//活动商店入口

    private GameObject _objSupportArmRedPoint;

    //private Transform _modelRoot;//战舰模型根节点

    private ActInfo_2081 _actInfo;

    private const int BossId = 170400;

    public override void OnCreate()
    {
        _btnHelpTips = transform.Find<Button>("_btnManual");
        _textDesc = transform.Find<JDText>("TextDesc");
        _textCountDown = transform.Find<JDText>("TextCountDown");
        _btnSelectSupportArm = transform.Find<Button>("BtnSelectArm");
        _btnSupportArm = transform.Find<Button>("BtnSupportArm");
        _btnActMission = transform.Find<Button>("BtnActMission");
        _btnActShop = transform.Find<Button>("BtnActShop");
        _objSupportArmRedPoint = transform.Find("BtnSupportArm/corner").gameObject;

        _actInfo = (ActInfo_2081)ActivityManager.Instance.GetActivityInfo(2081);

        //InitListener();
        //活动倒计时
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //活动任务入口
        _btnActMission.onClick.AddListener(On_btnActMissionClick);
        //奖励兑换入口
        _btnActShop.onClick.AddListener(On_btnActShopClick);
        //辅助武装强化入口
        _btnSupportArm.onClick.AddListener(On_btnSupportArmClick);
        //选择辅助武装入口
        _btnSelectSupportArm.onClick.AddListener(On_btnSelectSupportArmClick);
        //帮助按钮
        _btnHelpTips.onClick.AddListener(On_btnHelpTipsClick);

        //添加点击坐标回调
        _textDesc.AddHyperlinkCallback(_HyperLinkCallback.CoordCallback);

        //辅助武装列表刷新时刷新小红点
        //EventCenter.Instance.Act2081SupportArmListUpdate.AddListener(RefreshRedPoint);

        //战斗结晶数量变化时刷新小红点
        //EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshRedPoint);

        //boss坐标刷新
        //EventCenter.Instance.RemindActivity.AddListener((aid, data) => {
        //    if (aid == ActivityID.GeminiInvasion && DialogManager.IsDialogShown<_D_ActCalendar>())
        //        OnShow();
        //});

        //每秒刷新描述文本
        //TimeManager.Instance.TimePassSecond += (ts) =>
        //{
        //    UpdateDesc();
        //};
    }
    private void On_btnActMissionClick()
    {
        DialogManager.ShowAsyn<_D_Top_2081Mission>(On_btnActMissionDialogShowAsynCB);
    }
    private void On_btnActMissionDialogShowAsynCB(_D_Top_2081Mission d)
    {
        d?.OnShow();
    }
    private void On_btnActShopClick()
    {
        DialogManager.ShowAsyn<_D_Top_2081Exchange>(On_btnActShopDialogShowAsynCB);
    }
    private void On_btnActShopDialogShowAsynCB(_D_Top_2081Exchange d)
    {
        d?.OnShow();
    }
    private void On_btnSupportArmClick()
    {
        DialogManager.ShowAsyn<_D_Top_Upgrade2081SupportArm>(On_btnSupportArmDialogShowAsynCB);
    }
    private void On_btnSupportArmDialogShowAsynCB(_D_Top_Upgrade2081SupportArm d)
    {
        d?.OnShow();
    }
    private void On_btnSelectSupportArmClick()
    {
        DialogManager.ShowAsyn<_D_Top_Select2081SupportArm>(On_btnSelectSupportArmDialogShowAsynCB);
    }
    private void On_btnSelectSupportArmDialogShowAsynCB(_D_Top_Select2081SupportArm d)
    {
        d?.OnShow();
    }
    private void On_btnHelpTipsClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnHelpTipsDialogShowAsynCB);
    }
    private void On_btnHelpTipsDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2081, _btnHelpTips.transform.position, Direction.LeftDown, 350);
    }
    public override void InitListener()
    {
        base.InitListener();
        //辅助武装列表刷新时刷新小红点
        EventCenter.Instance.Act2081SupportArmListUpdate.AddListener(RefreshRedPoint);

        //战斗结晶数量变化时刷新小红点
        EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshRedPoint);

        //boss坐标刷新
        EventCenter.Instance.RemindActivity.AddListener(UpdateRemindAct);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        //辅助武装列表刷新时刷新小红点
        EventCenter.Instance.Act2081SupportArmListUpdate.RemoveListener(RefreshRedPoint);
        //战斗结晶数量变化时刷新小红点
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(RefreshRedPoint);
        //boss坐标刷新
        EventCenter.Instance.RemindActivity.RemoveListener(UpdateRemindAct);
    }

    public override void OnShow()
    {
        //固定展示boss模型 new_boss_008 苏尔特的狂热巨兽
        ShowShip(BossId);
        //刷新挑战boss描述
        UpdateDesc();
        //刷新小红点
        RefreshRedPoint();
    }

    private void UpdateRemindAct(int aid, bool data)
    {
        if (aid == ActivityID.GeminiInvasion && DialogManager.IsDialogShown<_D_ActCalendar>())
            OnShow();
    }

    private void UpdateDesc()
    {
        //活动未开启时不刷新
        if (!ActivityManager.Instance.IsActDuration(ActivityID.GeminiInvasion))
            return;
        var leftTime = _actInfo.Info.end_turn_ts - TimeManager.ServerTimestamp;
        if (leftTime < 0)
            leftTime = 0;
        _textDesc.text = Lang.Get("前往宇宙中心{0}，挑战{1}\n本轮BOSS剩余时间{2}", _actInfo.GetBossCoordStr(),
            Cfg.Ship.GetShipName(BossId), GLobal.TimeFormat(leftTime));
    }

    private void RefreshRedPoint()
    {
        //辅助武装小红点只在没有辅助武装解锁且拥有的战斗结晶足够解锁一个辅助武装时才显示
        _objSupportArmRedPoint.SetActive(_actInfo.GetUnlockSupportArmCount() == 0
            && BagInfo.Instance.GetItemCount(ItemId.BattleCrystal) >= Cfg.Activity2081.MinUnlockBattleCrystal);
    }

    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        UpdateDesc();
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;

        if (stamp - _actInfo._data.startts < 0)
        {
            _textCountDown.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _textCountDown.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _textCountDown.text = Lang.Get("活动已经结束");
        }
    }

    //展示boss模型
    private void ShowShip(int shipId)
    {
        _ShipDisplayControl.Instance.ShowShip(shipId, _ShipDisplayControl.DisplayMode.AutoRotateOnly);
    }

    //关闭活动对话框时隐藏boss模型
    public override void OnClose()
    {
        base.OnClose();
        _ShipDisplayControl.Instance.CloseShipShow();
    }
}
