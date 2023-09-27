using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class _Activity_2090_UI : ActivityUI
{

    //转盘上的奖励图标
    struct ReceivingRecord
    {
        public Image itemIcon;
        public Image itemQue;
        public Text itemNum;
        public Button showButton;
    }


    //夺宝按钮
    private Button _snatchTreasureButton;

    //夺宝按钮上的图标
    private GameObject _snatchTreasureButtonIcon;

    //夺宝按钮上消耗数量的文字显示
    private Text _snatchTreasureCostText;

    //夺宝按钮上消红点
    private Image _redPoint;

    //奖励转盘
    private GameObject _rewardTurntable;
    //次数转盘
    private GameObject _timesTurntable;
    //查看记录
    private Button _rewardsRecordButton;
    //帮助按钮
    private Button _helpButton;
    //夺宝卷数量
    private Text _drawRollsText;
    //时间
    private Text _timeText;

    private ActInfo_2090 _actInfo;

    //当前转盘1停留的角度
    private float _currentRewardRotation;

    //当前转盘2停留的角度
    private float _currentTimesRotation;


    //转盘上的奖励显示
    private ReceivingRecord[] _allRewards;

    //转盘2显示的倍数

    private Text[] _allMultiple;

    //夺宝记录页
    private GameObject _rewardsRecord;
    private ListView _recordListView;
    private Transform _recordListViewRoot;
    private Button _closeRecordButton;

    //按钮上的显示
    private Text _snatchTreasureButtonText;
    //
    private int _aid = 2090;

    private bool _isShow;


    private Sequence _sequ1;

    private Animator _animInner;
    private Animator _animOuter;
    private Animator _animStar;
    private GameObject _prompt;
    private Button _btnPrompt;
    private Button _btnCancelrompt;
    private Button _btnPromptArea;
    private Button _btnCancelromptArea;
    private int isReturn
    {
        get { return PlayerPrefs.GetInt(User.Uid + "isReturn2090", 0); }
        set { PlayerPrefs.SetInt(User.Uid + "isReturn2090", value); }
    }
    public override void OnCreate()
    {
        IntRef();
        InitButton();
        //InitListener();


        InitRewards();

        InitRotaryTablePosition();
    }


    //第一次打开时初始化角度和奖励
    private void InitRotaryTablePosition()
    {
        _currentRewardRotation = 22.5f;

        _currentTimesRotation = 22.5f;
    }

    //设置转盘奖励
    private void InitRewards()
    {
        _allRewards = new ReceivingRecord[8];
        var tempRewardInfo = _actInfo.GetAllRewardsInfo();
        for (int i = 0; i < tempRewardInfo.Count; i++)
        {
            _allRewards[i].itemIcon = _rewardTurntable.transform.Find<Image>("Icon_0" + (i + 1).ToString() + "/img_icon");
            _allRewards[i].itemQue = _rewardTurntable.transform.Find<Image>("Icon_0" + (i + 1).ToString() + "/Img_qua");
            _allRewards[i].itemNum = _rewardTurntable.transform.Find<Text>("Icon_0" + (i + 1).ToString() + "/Text");
            _allRewards[i].showButton = _rewardTurntable.transform.Find<Button>("Icon_0" + (i + 1).ToString() + "/img_icon");

            Cfg.Item.SetItemIcon(_allRewards[i].itemIcon, tempRewardInfo[i].id);
            _allRewards[i].itemQue.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(tempRewardInfo[i].id));
            _allRewards[i].itemNum.text = tempRewardInfo[i].Num.ToString();
            var i1 = i;
            _allRewards[i].showButton.onClick.AddListener(() =>
            {
                DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(tempRewardInfo[i1].id, tempRewardInfo[i1].count, _allRewards[i1].showButton.transform.position); });
            });
        }


        var times = _actInfo.GetTimesInfo();
        _allMultiple = new Text[8];
        for (int i = 0; i < times.Count; i++)
        {
            _allMultiple[i] = _timesTurntable.transform.Find<Text>("Text" + (i + 1).ToString());
            _allMultiple[i].text = "X" + times[i].ToString();

            if (times[i] == 1)
            {
                _allMultiple[i].color = new Color(159.0f / 256.0f, 87.0f / 256.0f, 83.0f / 256.0f);
            }
            else if (times[i] == 2)
            {
                _allMultiple[i].color = new Color(231.0f / 256.0f, 78.0f / 256.0f, 0);
            }
            else
            {
                _allMultiple[i].color = new Color(172.0f / 256.0f, 19.0f / 256.0f, 195.0f / 256.0f);
            }
        }
    }

    //刷新记录信息
    private void RefreshRecord()
    {
        var recordList = _actInfo.GetRecordList();

        if (recordList == null || recordList.Count <= 0)
        {
            return;
        }
        _recordListView.Clear();

        for (int i = recordList.Count - 1; i >= 0; i--)
        {
            _recordListView.AddItem<RotaryRecordItem>().Show(recordList[i]);
        }
    }
    private void IntRef()
    {
        _snatchTreasureButton = transform.Find<Button>("Button");
        _rewardTurntable = transform.Find("Main/Mid_02").gameObject;
        _timesTurntable = transform.Find("Main/Mid_01").gameObject;
        _rewardsRecordButton = transform.Find<Button>("ShopButton");
        _helpButton = transform.Find<Button>("Helpbtn");
        _drawRollsText = transform.Find<Text>("Icon/Text");
        _timeText = transform.Find<Text>("CountDown");

        _snatchTreasureButtonIcon = transform.Find("Button/cost1num").gameObject;
        _snatchTreasureCostText = transform.Find<Text>("Button/cost1num/costText");
        _snatchTreasureButtonText = transform.Find<Text>("Button/Text");
        _redPoint = transform.Find<Image>("Button/Image");
        //记录页面
        _rewardsRecord = transform.Find("Main_Inf").gameObject;
        _recordListViewRoot = _rewardsRecord.transform.Find("ScrollView");
        _recordListView = ListView.Create<RotaryRecordItem>(_recordListViewRoot);
        _closeRecordButton = _rewardsRecord.transform.Find<Button>("CloseBtn");


        _animInner = transform.Find<Animator>("Main/ani_eff_huan_nei_hd");
        _animOuter = transform.Find<Animator>("Main/ani_eff_huan_wai_hd");
        _animStar = transform.Find<Animator>("ani_eff_zpdb_star");

        _prompt = transform.Find("Prompt").gameObject;
        _btnPrompt = transform.Find<Button>("Prompt/ButtonYesArea/ButtonYes");
        _btnPromptArea = transform.Find<Button>("Prompt/ButtonYesArea");
        _btnCancelrompt = transform.Find<Button>("Prompt/ButtonNoArea/ButtonNo");
        _btnCancelromptArea = transform.Find<Button>("Prompt/ButtonNoArea");
        _btnCancelromptArea.gameObject.SetActive(isReturn == 1);

        _actInfo = (ActInfo_2090)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void OnClose()
    {
        base.OnClose();
        _isShow = false;
        _sequ1.Complete();
        _rewardsRecord.SetActive(false);

        _animStar.gameObject.SetActive(false);
        _animInner.gameObject.SetActive(false);
        _animOuter.gameObject.SetActive(false);
    }
    private void InitButton()
    {
        _snatchTreasureButton.onClick.AddListener(SnatchTreasure);
        _rewardsRecordButton.onClick.AddListener(ShowRewardRecord);
        _closeRecordButton.onClick.AddListener(On_closeRecordButtonClick);
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
    private void On_closeRecordButtonClick()
    {
        _rewardsRecord.gameObject.SetActive(false);
    }
    private void On_helpButtonClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpButtonDialogShowAsynCB);
    }
    private void On_helpButtonDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.ActRotaryIndiana, _helpButton.transform.position, Direction.LeftDown, 350);
    }

    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdateDrawRolls);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdateDrawRolls);
    }

    //判断是否为免费夺宝
    private void IsFree()
    {
        int freeTimes = _actInfo.GetFreeTimes();
        if (freeTimes < 3)
        {
            _snatchTreasureButtonIcon.SetActive(false);
            _snatchTreasureButtonText.text = Lang.Get("免费夺宝");
            _redPoint.gameObject.SetActive(true);
        }
        else
        {
            _snatchTreasureButtonIcon.SetActive(true);
            _snatchTreasureCostText.text = "X 1";
            _snatchTreasureButtonText.text = Lang.Get("夺宝");
            _redPoint.gameObject.SetActive(false);
        }
    }

    //转动转盘动画
    private void SwingRotation(int target1, int target2, P_Item reward)
    {
        _snatchTreasureButton.interactable = false;
        _rewardsRecordButton.interactable = false;

        if (isReturn == 1) {
            float Rotation1 = target1 * 45.0f + 22.5f;
            _currentRewardRotation = Rotation1;
            float Rotation2 = target2 * 45.0f + 22.5f;
            _currentTimesRotation = Rotation2;
            InitRotation();
            ShowRewards(reward);
            if (_animInner.gameObject.activeSelf) {
                _animInner.gameObject.SetActive(false);
            }
            if (_animOuter.gameObject.activeSelf) {
                _animOuter.gameObject.SetActive(false);
            }
            _snatchTreasureButton.interactable = true;
            _rewardsRecordButton.interactable = true;
            IsFree();
            return;
        }
        //奖励转盘
        int rotatingLaps1 = 10;
        float tempCurrentRotation1 = target1 * 45.0f + 22.5f;

        float rotation1 = 360 * rotatingLaps1 + tempCurrentRotation1;

        _currentRewardRotation = tempCurrentRotation1;

        _sequ1 = DOTween.Sequence();
        _animStar.gameObject.SetActive(true);
        _sequ1.Append(_rewardTurntable.transform.DORotate(new Vector3(0, 0, rotation1), 8, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _animOuter.gameObject.SetActive(true);

                _animStar.gameObject.SetActive(false);
                _animStar.enabled = false;


            }));

        //次数转盘

        int rotatingLaps2 = rotatingLaps1 + 2;
        float tempCurrentRotation2 = target2 * 45.0f + 22.5f;

        float rotation2 = tempCurrentRotation2 - 360 * rotatingLaps2;

        _currentTimesRotation = tempCurrentRotation2;
        _sequ1.Join(_timesTurntable.transform.DORotate(new Vector3(0, 0, rotation2), 10, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad).OnComplete(() =>
            {
                if (reward.extra > 1)
                {
                    _animInner.gameObject.SetActive(true);
                }
            }));

        _sequ1.AppendInterval(0.5f).AppendCallback(() =>
        {
            ShowRewards(reward);
            if (_animInner.gameObject.activeSelf)
            {
                _animInner.gameObject.SetActive(false);
            }
            _animOuter.gameObject.SetActive(false);

            _snatchTreasureButton.interactable = true;
            _rewardsRecordButton.interactable = true;
        });
        IsFree();

    }


    //展示奖励
    private void ShowRewards(P_Item reward)
    {

        if (_isShow)
        {
            List<P_Item> tempItem = new List<P_Item>();
            for (int i = 0; i < reward.extra; i++)
            {
                tempItem.Add(reward);
            }

            if (reward.count == 1)
            {
                DialogManager.ShowAsyn<_D_ShowRewards>(d =>
                {
                    d.ShowCustonRewards(tempItem,
Lang.Get("转盘夺宝奖励"),
Lang.Get("恭喜获得[{0}] X {1}", Cfg.Item.GetItemName(reward.id), reward.extra.ToString()),
Lang.Get("确定"));
                });
            }
            else
            {
                DialogManager.ShowAsyn<_D_ShowRewards>(d =>
                {
                    d.ShowCustonRewards(tempItem,
                   Lang.Get("转盘夺宝奖励"),
                   Lang.Get("恭喜获得[{0} {1}] X {2}", Cfg.Item.GetItemName(reward.id), reward.count, reward.extra.ToString()),
                   Lang.Get("确定"));
                });
            }
        }

    }
    //转动转盘
    private void SnatchTreasure()
    {
        if (BagInfo.Instance.GetItemCount(ItemId.TreasureRoll) > 0 || _actInfo.GetFreeTimes() < 3)
        {
            _actInfo.TurnTurntable(SwingRotation);
        }
        else
        {
            string msg = Lang.Get("夺宝卷数量不足");
            MessageManager.Show(msg);
        }
    }



    //打开夺宝记录
    private void ShowRewardRecord()
    {
        _rewardsRecord.SetActive(true);
        RefreshRecord();
    }
    public override void OnShow()
    {

        UpdateDrawRolls();
        InitRotation();
        ProcessingForcedShutdown();
        IsFree();
        _isShow = true;
        UpdateTime(0);
    }

    //转动时强制关闭下次进入时初始化
    private void ProcessingForcedShutdown()
    {
        if (!_snatchTreasureButton.interactable)
        {
            _snatchTreasureButton.interactable = true;
        }


        if (!_rewardsRecordButton.interactable)
        {
            _rewardsRecordButton.interactable = true;
        }

        if (!_rewardsRecord.activeSelf)
        {
            _rewardsRecord.SetActive(false);
        }
    }

    //每次打开时初始化到上次停下的角度
    private void InitRotation()
    {


        Vector3 temp1 = new Vector3(_rewardTurntable.transform.localRotation.x,
            _rewardTurntable.transform.localRotation.y,
            _currentRewardRotation);
        _rewardTurntable.transform.localRotation = Quaternion.Euler(temp1);

        Vector3 temp2 = new Vector3(_timesTurntable.transform.localRotation.x,
            _timesTurntable.transform.localRotation.y,
            _currentTimesRotation);
        _timesTurntable.transform.localRotation = Quaternion.Euler(temp2);

    }
    //更新夺宝卷信息
    private void UpdateDrawRolls()
    {
        _drawRollsText.text = BagInfo.Instance.GetItemCount(ItemId.TreasureRoll).ToString();
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

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_sequ1 != null)
        {
            _sequ1.Kill();
            _sequ1 = null;
        }
        _allRewards = null;
        _actInfo = null;
    }
}

public class RotaryRecordItem : ListItem
{

    private Text _record;
    private Text _date;
    public override void OnCreate()
    {
        _record = transform.Find<Text>("Text");
        _date = transform.Find<Text>("TimeText");
    }

    public void Show(TurntableRewardRecordInfo record)
    {

        P_Item tempRecord = new P_Item(record.msg);

        var time = TimeManager.ToServerDateTime(record.time_stamp);
        string date = string.Format("{0}.{1}.{2}", time.Year, time.Month, time.Day);

        string color = _ColorConfig.GetQuaColorText(Cfg.Item.GetItemQua(tempRecord.id));
        string name = Cfg.Item.GetItemName(tempRecord.id);
        string num = tempRecord.count.ToString();
        string multiple = tempRecord.extra.ToString();
        _date.text = date;

        if (tempRecord.count == 1)
        {
            _record.text = string.Format(Lang.Get("获得<color={0}>[{1}]</color>X{2}"), color, name, multiple);
        }
        else
        {
            _record.text = string.Format(Lang.Get("获得<color={0}>[{1} {2}]</color>X{3}"), color, name, num, multiple);
        }


    }


}