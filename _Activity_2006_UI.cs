using System;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2006_UI : ActivityUI
{
    private Text _descText;

    private Text _contentText;

    private Text _rechargeText;

    private Text _rebateText;

    private Text _timeText;

    private Button rechargeBtn;

    private ActInfo_2006 _actInfo;

    private const int AID = 2006;

    public override void Awake()
    {
        _descText = transform.Find<Text>("des_text");
        _contentText = transform.Find<Text>("des_text");
        _rechargeText = transform.Find<Text>("recharge_text");
        _rebateText = transform.Find<Text>("rebate_text");
        _timeText = transform.Find<Text>("time_text");
        rechargeBtn = transform.Find<Button>("recharge_btn");
    }

    public override void OnCreate()
    {
        //Init();
    }

    public override void OnClose()
    {
        base.OnClose();
        //EventCenter.Instance.UpdateActivityUI.RemoveListener(InitUI);
    }

    public override void OnShow()
    {
        Init();
    }

    private void Init()
    {
        InitData();
        InitEvent();
        //InitListener();

        InitUI(AID);
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2006)ActivityManager.Instance.GetActivityInfo(AID);
    }

    private void InitEvent()
    {
        rechargeBtn.onClick.AddListener(OnClickRebateBtn);
    }

    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(InitUI);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        InitUI(aid);
    }

    private void InitUI(int aid)
    {
        if (aid != _actInfo._data.aid)
            return;

        _descText.text = _actInfo._desc;
        _contentText.text = string.Format(Lang.Get("指挥官在删档测试期间通过充值获得的氪晶，在不删档测试开启后，都可获得<Color=#00ff33ff>{0}%</Color>的返还！充值越多返还越高。返利的氪晶将通过邮件发送到您在不删档测试中用本账号创建的第一个角色上！"),
            _actInfo.payRate);

        _timeText.text = GlobalUtils.ActTimeFormat(_actInfo._data.startts, _actInfo._data.endts);

        UpdateRecharge();
    }

    private void OnClickRebateBtn()
    {
        DialogManager.ShowAsyn<_D_Recharge>(OnRebateDialogShowAsynCB);
    }
    private void OnRebateDialogShowAsynCB(_D_Recharge d)
    {
        d?.OnShow(0);
    }
    private void UpdateRecharge()
    {
        _rechargeText.text = _actInfo.payGold + "";
        _rebateText.text = Mathf.CeilToInt(_actInfo.payRate * 0.01f * _actInfo.payGold) + "";
    }
}
