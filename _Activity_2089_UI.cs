using UnityEngine.UI;

public class _Activity_2089_UI : ActivityUI
{
    public ObjectGroup UI;
    private Button _btnGo;//前往讨伐
    private Button _btnKillNum;//杀敌数量
    private Button _btnHelp;//帮助
    private JDText _textCountDown;//活动倒计时

    private JDText _textFormation;
    private JDText _textDesc;
    private JDText _textTips;
    private Button _btnLeft;
    private Button _btnRight;
    private JDText _textLeftBtn;
    private JDText _textRightBtn;
    private _StarFormationDisplay _formationDisplay;

    private int _aid;
    private long _startts;
    private long _endts;
    private ActInfo_2089 _actInfo;

    private int _formationType = 1;

    public override void OnCreate()
    {
        UI = transform.GetComponent<ObjectGroup>();
        _btnGo = UI.Get<Button>("BtnGo");
        _btnKillNum = UI.Get<Button>("BtnKillNum");
        _btnHelp = UI.Get<Button>("Helpbtn");
        _textCountDown = UI.Get<JDText>("TimeText");

        _textFormation = transform.Find<JDText>("Main/Title/Text");
        _textDesc = transform.Find<JDText>("Main/Image/TextDesc");
        _textTips = transform.Find<JDText>("Main/TextTips");
        _btnLeft = transform.Find<Button>("Main/BtnLeft");
        _btnRight = transform.Find<Button>("Main/BtnRight");
        _textLeftBtn = transform.Find<JDText>("Main/BtnLeft/Text");
        _textRightBtn = transform.Find<JDText>("Main/BtnRight/Text");
        _formationDisplay = transform.Find("Main/Formation").gameObject.AddBehaviour<_StarFormationDisplay>();

        _aid = 2089;

        _btnGo.onClick.AddListener(On_btnGoClick);
        _btnKillNum.onClick.AddListener(On_btnKillNumClick);
        _btnHelp.onClick.AddListener(On_btnHelpClick);
        _btnLeft.onClick.AddListener(On_btnLeftClick);
        _btnRight.onClick.AddListener(On_btnRightClick);
        _btnGo.gameObject.SetActive(false);
    }
    private void On_btnGoClick()
    {
        var formCenter = _actInfo.Info.form_center;
        if (formCenter > 0)
        {
            //关闭小地图和活动界面并跳转到世界
            GameStage.Instance.CloseStage();
            DialogManager.CloseDialog<_D_ActCalendar>();
            WorldConfig.WorldController.EnterWorld(false, formCenter);
        }
        else
        {
            Alert.Ok(Lang.Get("在占领阶段未形成阵型，无法搜寻到星体眷族"));
        }
    }
    private void On_btnKillNumClick()
    {
        //打开杀敌数量界面
        DialogManager.ShowAsyn<_D_Top_2089KillNum>(On_btnKillNumDialogShowAsynCB);
    }
    private void On_btnKillNumDialogShowAsynCB(_D_Top_2089KillNum d)
    {
        d?.OnShow();
    }
    private void On_btnHelpClick()
    {
        //详情显示
        DialogManager.ShowAsyn<_D_Top_HelpDesc>(On_btnHelpDialogShowAsynCB);
    }
    private void On_btnHelpDialogShowAsynCB(_D_Top_HelpDesc d)
    {
        d?.OnShow(HelpType.StarLink);
    }
    private void On_btnLeftClick()
    {
        if (Cfg.Activity2089.IsTypeExist(_formationType - 1))
        {
            _formationType--;
            OnShow();
        }
    }
    private void On_btnRightClick()
    {
        if (Cfg.Activity2089.IsTypeExist(_formationType + 1))
        {
            _formationType++;
            OnShow();
        }
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        var actInfo = ActivityManager.Instance.GetActivityInfo(_aid);
        if (actInfo == null)
            actInfo = ActivityManager.Instance.GetFutureActivityInfo(_aid);
        if (actInfo == null)
            return;
        _actInfo = (ActInfo_2089)actInfo;
        _startts = _actInfo.Info.step_info.start_ts;
        _endts = _actInfo.Info.step_info.end_ts;
        //刷新活动倒计时
        UpdateTime(0);
        //刷新阵型
        _formationDisplay.Refresh(_formationType);
        //刷新按钮显示
        RefreshBtns();
        //刷新文本
        RefreshText();
    }

    public override void UpdateTime(long time)
    {
        base.UpdateTime(time);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        var leftTime = _endts - TimeManager.ServerTimestamp;
        if (leftTime < 0)
            leftTime = 0;
        if (leftTime == 0)
        {
            _textCountDown.gameObject.SetActive(false);
        }
        else
        {
            _textCountDown.gameObject.SetActive(true);
            if(_actInfo.Info.step_info.step == Act2089Step.STEP_OCCUPY)
                _textCountDown.text = Lang.Get("占领阶段倒计时 {0}", GLobal.TimeFormat(leftTime, true));
            else if(_actInfo.Info.step_info.step == Act2089Step.STEP_CRUSADE)
                _textCountDown.text = Lang.Get("讨伐阶段倒计时 {0}", GLobal.TimeFormat(leftTime, true));
            else
                _textCountDown.text = string.Empty;
        }
    }

    private void RefreshText()
    {
        _textFormation.text = Cfg.Activity2089.GetStarFormationName(_formationType);
        _textDesc.text = Cfg.Activity2089.GetStarFormationDesc(_formationType);
        var actInfo = ActInfo_2089.Inst;
        if (actInfo != null)
        {
            _textTips.text = actInfo.Info.banList.Contains(_formationType) ? Lang.Get("（此阵型本周无效）") : string.Empty;
        }
    }

    private void RefreshBtns()
    {
        var beforeFormName = Cfg.Activity2089.GetStarFormationName(_formationType - 1);
        if (!string.IsNullOrEmpty(beforeFormName))
        {
            _btnLeft.interactable = true;
            _textLeftBtn.text = beforeFormName;
        }
        else
        {
            _btnLeft.interactable = false;
            _textLeftBtn.text = string.Empty;
        }

        var nextFormName = Cfg.Activity2089.GetStarFormationName(_formationType + 1);
        if (!string.IsNullOrEmpty(nextFormName))
        {
            _btnRight.interactable = true;
            _textRightBtn.text = nextFormName;
        }
        else
        {
            _btnRight.interactable = false;
            _textRightBtn.text = string.Empty;
        }
        _btnGo.gameObject.SetActive(_actInfo.Info.step_info.step == Act2089Step.STEP_CRUSADE);
    }
}
