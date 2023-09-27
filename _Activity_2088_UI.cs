using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class _Activity_2088_UI : ActivityUI
{

    //打开商店
    private Button _shopButton;
    //查看奖励
    private Button _viewRewardsButton;
    //抽奖一次
    private Button _drawOnceButton;
    //抽奖十次
    private Button _drawTenTimesButton;
    //活动时间
    private Text _timeText;
    //盲盒币数量
    private Text _blindBoxCoinsNum;
    //帮助按钮
    private Button _helpButton;

    private Text _drawOnceButtonText;

    private Text _drawTenTimesButtonText;

    private Text _shopButtonText;

    private Text _viewRewardsButtonText;
    //奖池抽完后显示
    private Text _endPrompt;
    //
    private Sequence _sequence;
    private _D_ActCalendar _actCalendar;


    private const int _aid = 2088;

    private Animator _anim;
    private ActInfo_2088 _actInfo;
    private GameObject _prompt;
    private Button _btnPrompt;
    private Button _btnCancelrompt;
    private Button _btnPromptArea;
    private Button _btnCancelromptArea;
    private int isReturn
    {
        get { return PlayerPrefs.GetInt(User.Uid + "isReturn2088", 0); }
        set { PlayerPrefs.SetInt(User.Uid + "isReturn2088", value); }
    }


    public override void OnCreate()
    {
        InitRef();
        //InitListener();
        InitButtons();

    }

    public override void OnShow()
    {
        _anim.enabled = false;
        StopAllCoroutines();
        Refresh();

    }


    private void InitRef()
    {
        _actCalendar = DialogManager.GetInstanceOfDialog<_D_ActCalendar>();
        _actInfo = (ActInfo_2088)ActivityManager.Instance.GetActivityInfo(_aid);
        //初始化ui
        _drawOnceButton = transform.FindButton("DrawButton/Btn1");

        _drawOnceButtonText = _drawOnceButton.transform.Find<JDText>("Text");
        _drawTenTimesButton = transform.FindButton("DrawButton/Btn2");

        _drawTenTimesButtonText = _drawTenTimesButton.transform.Find<JDText>("Text");
        _shopButton = transform.FindButton("ShopButton");

        _shopButtonText = _shopButton.transform.Find<JDText>("Text");
        _viewRewardsButton = transform.FindButton("RewardListButton");

        _viewRewardsButtonText = _viewRewardsButton.transform.Find<JDText>("Text");
        _timeText = transform.Find<JDText>("TimeText");
        _blindBoxCoinsNum = transform.FindText("ImageIcon/Text");
        _helpButton = transform.Find<Button>("Helpbtn");
        _anim = transform.Find<Animator>("pfb_Blind_box/ani_blind_box");
        _endPrompt = transform.FindText("Text");
        _anim.enabled = false;

        _prompt = transform.Find("Prompt").gameObject;
        _btnPrompt = transform.Find<Button>("Prompt/ButtonYesArea/ButtonYes");
        _btnPromptArea = transform.Find<Button>("Prompt/ButtonYesArea");
        _btnCancelrompt = transform.Find<Button>("Prompt/ButtonNoArea/ButtonNo");
        _btnCancelromptArea = transform.Find<Button>("Prompt/ButtonNoArea");
        _btnCancelromptArea.gameObject.SetActive(isReturn == 1);
    }

    private void InitButtons()
    {
        //
        _drawOnceButtonText.text = Lang.Get("开启1次");

        _drawTenTimesButtonText.text = Lang.Get("开启10次");

        _shopButtonText.text = Lang.Get("兑换商店");

        _viewRewardsButtonText.text = Lang.Get("查看奖励");

        //binding ui点击事件
        _shopButton.onClick.AddListener(OpenShop);
        _viewRewardsButton.onClick.AddListener(OpenRewardsList);
        _drawOnceButton.onClick.AddListener(DrawOnce);
        _drawTenTimesButton.onClick.AddListener(DrawTenTimes);
        //帮助按钮
        _helpButton.onClick.AddListener(On_helpButtonClick);

        _btnPrompt.onClick.AddListener(OnBtnPromptClick);
        _btnPromptArea.onClick.AddListener(OnBtnPromptClick);
        _btnCancelrompt.onClick.AddListener(OnBtnCancelRomptClick);
        _btnCancelromptArea.onClick.AddListener(OnBtnCancelRomptClick);
    }
    private void OnBtnPromptClick()
    {
        _btnCancelromptArea.gameObject.SetActive(true);
        isReturn = 1;
    }
    private void OnBtnCancelRomptClick()
    {
        _btnCancelromptArea.gameObject.SetActive(false);
        isReturn = 0;
    }
    private void On_helpButtonClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpButtonDialogShowAsynCB);
    }
    private void On_helpButtonDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2088, _helpButton.transform.position, Direction.LeftDown, 350);
    }
    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(ActivityUpdate);
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdateBoxCoin);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdateBoxCoin);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == 2088)
        {
            Refresh();
        }

        if (aid == 208801)
        {
            DrawOnce();
        }

        if (aid == 208810)
        {
            DrawTenTimes();
        }
    }

    private void Refresh()
    {
        if (_actInfo.UniqueInfo.DrawRemainingNum < 10)
        {

            _drawTenTimesButton.gameObject.SetActive(false);
            _drawOnceButton.gameObject.transform.localPosition = Vector3.zero;
        }

        if (_actInfo.UniqueInfo.DrawRemainingNum <= 0)
        {
            _drawOnceButton.gameObject.SetActive(false);
            _endPrompt.text = Lang.Get("恭喜您已获得本期活动的全部奖励！");
        }
        UpdateBoxCoin();
    }

    //刷新盲盒币数量
    private void UpdateBoxCoin()
    {
        _blindBoxCoinsNum.text = BagInfo.Instance.GetItemCount(ItemId.BlindBoxCoin).ToString();
    }


    private void OpenShop()
    {
        DialogManager.ShowAsyn<_D_Act2088Shop>(OnOpenShopDialogShowAsynCB);
    }
    private void OnOpenShopDialogShowAsynCB(_D_Act2088Shop d)
    {
        d?.OnShow();
    }
    private void OpenRewardsList()
    {
        DialogManager.ShowAsyn<_D_Act2088RewardList>(OnOpenRewardsDialogShowAsynCB);
    }
    private void OnOpenRewardsDialogShowAsynCB(_D_Act2088RewardList d)
    {
        d?.OnShow();
    }
    private void DrawOnce()
    {
        if (BagInfo.Instance.GetItemCount(ItemId.BlindBoxCoin) < _actInfo.once_price)
        {
            MessageManager.Show(Lang.Get("盲盒币不足"));
            return;
        }

        _actInfo.StartRaffle(1, ShowGetRewards);
    }

    private void DrawTenTimes()
    {
        if (BagInfo.Instance.GetItemCount(ItemId.BlindBoxCoin) < _actInfo.ten_times_price)
        {
            MessageManager.Show(Lang.Get("盲盒币不足"));
            return;
        }

        _actInfo.StartRaffle(10, ShowGetRewards);
    }
    private void ShowGetRewards()
    {
        if (isReturn == 1) {
            Refresh();
            DialogManager.ShowAsyn<_D_Act2088GottenRewards>(OnShowRewardsDialogShowAsynCB);
            return;
        }
        _actCalendar.SetBlock(true);
        Refresh();
        _anim.enabled = true;
        _anim.Play("ani_blind_box");
        StartCoroutine(ShowRewards());

    }
    private IEnumerator ShowRewards()
    {
        yield return null;
        AnimatorStateInfo stateinfo = _anim.GetCurrentAnimatorStateInfo(0);
        if (stateinfo.IsName("ani_blind_box") && stateinfo.normalizedTime > 1.0f)
        {
            _actCalendar.SetBlock(false);
            _anim.enabled = false;
            _anim.Rebind();
            DialogManager.ShowAsyn<_D_Act2088GottenRewards>(OnShowRewardsDialogShowAsynCB);
        }
        else
        {
            StartCoroutine(ShowRewards());
        }
    }
    private void OnShowRewardsDialogShowAsynCB(_D_Act2088GottenRewards d)
    {
        d?.OnShow();
    }

    public override void UpdateTime(long currentTime)
    {
        base.UpdateTime(currentTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!gameObject.activeSelf)
            return;

        if (_actInfo == null)
            return;

        if (_actInfo.LeftTime >= 0)
        {
            _timeText.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }

    public override void OnClose()
    {
        base.OnClose();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_sequence != null)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }
}


