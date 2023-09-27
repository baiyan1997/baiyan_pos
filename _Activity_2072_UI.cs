using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2072_UI : ActivityUI
{
    [SerializeField]
    Text _txtTitle;
    [SerializeField]
    Text _txtMonstHP;
    [SerializeField]
    Slider _sliderMonstHP;
    [SerializeField]
    Button _btnAttack;
    [SerializeField]
    Button _btnClose2;

    private Button _btnMission;//任务入口
    private Button _btnChallengeTarget;//挑战目标
    private Button _btnHelpTips;//提示信息
    private Button _btnHelpTips2;

    private ObjectGroup UI;

    private GameObject _objTextBusy;
    private GameObject _objTextFree;

    private ActInfo_2072 _actInfo;
    private P_WorldBossInfo _bossInfo;
    private cfg_boss _bossCfg;

    public override void OnCreate()
    {
        UI = transform.GetComponent<ObjectGroup>();
        _txtTitle = UI.Get<Text>("_txtTitle");
        _txtMonstHP = UI.Get<Text>("_txtMonstHP");
        _sliderMonstHP = UI.Get<Slider>("_sliderMonstHP");
        _btnAttack = UI.Get<Button>("_btnAttack");
        _btnClose2 = UI.Get<Button>("_btnClose2");
        _objTextBusy = UI.Get<GameObject>("Text_Busy");
        _objTextFree = UI.Get<GameObject>("Text_Free");
        _btnMission = UI.Get<Button>("BtnMission");
        _btnChallengeTarget = UI.Get<Button>("BtnChallengeTarget");
        _btnHelpTips = UI.Get<Button>("BtnHelpTips");
        _btnHelpTips2 = UI.Get<Button>("BtnHelpTips2");

        _btnAttack.onClick.AddListener(CallAttack);
        //功能入口按钮
        _btnMission.onClick.AddListener(On_btnMissionClick);
        _btnChallengeTarget.onClick.AddListener(On_btnChallengeTargetClick);
        //提示按钮
        _btnHelpTips.onClick.AddListener(On_btnHelpTipsClick);
        _btnHelpTips2.onClick.AddListener(On_btnHelpTips2Click);
        //购买按钮
        _btnClose2.onClick.AddListener(On_btnClose2Click);
    }
    private void On_btnMissionClick()
    {
        DialogManager.ShowAsyn<_D_Top_2072DailyMission>(On_btnMissionDialogShowAsynCB);
    }
    private void On_btnMissionDialogShowAsynCB(_D_Top_2072DailyMission d)
    {
        d?.OnShow();
    }
    private void On_btnChallengeTargetClick()
    {
        DialogManager.ShowAsyn<_D_Top_2072ChallengeTarget>(On_btnChallengeTargetDialogShowAsynCB);
    }
    private void On_btnChallengeTargetDialogShowAsynCB(_D_Top_2072ChallengeTarget d)
    {
        d?.OnShow();
    }
    private void On_btnHelpTipsClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnHelpTipsDialogShowAsynCB);
    }
    private void On_btnHelpTipsDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(Lang.Get("提示"), Lang.Get("世界boss以每分钟0.0025%（兵力上限）的速度缓慢恢复剩余兵力"), _btnHelpTips.transform.position, Direction.LeftDown, 350, new Vector2(-25, -25));
    }
    private void On_btnHelpTips2Click()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnHelpTips2DialogShowAsynCB);
    }
    private void On_btnHelpTips2DialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2072, _btnHelpTips2.transform.position, Direction.LeftDown, 350);
    }

    private void On_btnClose2Click()
    {
        var buyInfos = _bossCfg.buy_times.Split(",");
        var buyTimes = buyInfos.Length - _bossInfo.use_buy_times;
        if (buyTimes < 1) {
            Alert.Ok("购买次数已达到上限");
            return;
        }
        var iNum = int.Parse(buyInfos[_bossInfo.use_buy_times].Split("|")[2]);
        _AlertYesNo a = Alert.YesNo(string.Format(Lang.Get("是否消耗{0}氪晶获得1次额外世界Boss挑战机会（剩余购买次数{1}次）"), iNum, buyTimes));
        a.SetYesCallback(() =>
        {
            long goldNum = BagInfo.Instance.GetItemCount(ItemId.Gold);
            if (goldNum < iNum)
            {
                DialogManager.ShowAsyn<_D_JumpConfirm>(d => { d?.OnShow(JumpType.Kr, (int)goldNum); });
                a.Close();
                return;
            }

            Rpc.SendWithTouchBlocking<int>("buyAtkTimes", null, data =>
            {
                Uinfo.Instance.Player.AddGold(-iNum);
                ActivityManager.Instance.RequestUpdateActivityById(ActivityID.WorldBoss);
            });
            a.Close();
        });
    }
    public override void InitListener()
    {
        base.InitListener();
        //活动数据刷新时刷新界面
        EventCenter.Instance.RemindActivity.AddListener(RefRemindAct);
        EventCenter.Instance.Act2072MissionUpdate.AddListener(RefAct2072Mission);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.RemindActivity.RemoveListener(RefRemindAct);
        EventCenter.Instance.Act2072MissionUpdate.RemoveListener(RefAct2072Mission);
    }

    public override void OnShow()
    {
        //每次进入界面刷新活动数据
        ActivityManager.Instance.RequestUpdateActivityById(2072);

        RefreshUI();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    private void RefRemindAct(int aid, bool isRemind)
    {
        if (aid != ActivityID.WorldBoss)
            return;
        if (DialogManager.IsDialogShown<_D_ActCalendar>())
            RefreshUI();
    }

    private void RefAct2072Mission()
    {
        if (DialogManager.IsDialogShown<_D_ActCalendar>())
            RefreshUI();
    }

    private void RefreshUI()
    {
        _actInfo = ActInfo_2072.Inst;
        _objTextBusy.SetActive(false);
        _objTextFree.SetActive(false);
        _bossInfo = _actInfo.BossInfo;
        _bossCfg = Cfg.WorldBoss.GetData(_bossInfo.boss);
        _txtTitle.text = _bossInfo.GetBossName();
        _txtMonstHP.text = string.Format(Lang.Get("剩余兵力 <Color=#ffcc00ff>{0}</Color>/{1}"), _bossInfo.hp, _bossInfo.hp_max);
        _sliderMonstHP.value = 100f * ((float)_bossInfo.hp / (float)_bossInfo.hp_max);
        //每天只能限定次数世界boss
        var maxCount = _bossInfo.challenge_add_count + 1;
        var haveAtkCount = (_bossInfo.atk_boss_count < maxCount || _bossInfo.use_buy_times < _bossInfo.buy_times);
        _btnAttack.gameObject.SetActive(haveAtkCount);
        _btnClose2.gameObject.SetActive(!haveAtkCount);
        _objTextBusy.SetActive(haveAtkCount);
        _objTextFree.SetActive(!haveAtkCount);
    }

    private void CallAttack()
    {
        ActInfo_2072.Inst.AttackWorldBoss(OnAttackWorldBossCB);
    }

    private void OnAttackWorldBossCB(P_Battle data)
    {
        _Battle.Instance.Show(data, () => { 
            EventCenter.Instance.UpdateAllActivity.Broadcast();
        });
        OnShow();
    }
}