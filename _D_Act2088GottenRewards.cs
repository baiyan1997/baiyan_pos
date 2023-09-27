using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class _D_Act2088GottenRewards : Dialog
{
    private int _aid = 2088;
    private ActInfo_2088 _actInfo;

    //一发
    private GameObject _get1;
    private Text _blindBoxCoinNumInOnce;
    private Button _drawOnce;
    private Button _okBtnOnce;
    private GameObject Content1;
    //十连
    private GameObject _get10;
    private Text _blindBoxCoinNumInTenTimes;
    private Button _drawTenTimes;
    private Button _okBtnTenTimes;
    private GameObject Content10;

    //
    private List<AwardItem> _allKindsItems;

    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    protected override void InitRef()
    {
        _actInfo = (ActInfo_2088)ActivityManager.Instance.GetActivityInfo(_aid);

        //一发
        _get1 = transform.Find("Get1").gameObject;
        _blindBoxCoinNumInOnce = transform.Find<JDText>("Get1/ImageIcon/Text");
        _drawOnce = transform.FindButton("Get1/Btns/ReOpen");
        Content1 = transform.Find("Get1/Content1").gameObject;

        //十连
        _get10 = transform.Find("Get10").gameObject;
        _blindBoxCoinNumInTenTimes = transform.Find<JDText>("Get10/ImageIcon/Text");
        _drawTenTimes = transform.FindButton("Get10/Btns/ReOpen");
        _okBtnTenTimes = transform.FindButton("Get10/Btns/OkBtn");
        Content10 = transform.Find("Get10/Content1").gameObject;


        InitItems();
        InitBtn();
        InitEvents();
    }

    private void InitItems()
    {
        var root = Content10.gameObject;
        _allKindsItems = new List<AwardItem>();
        for (int i = 0; i < 11; i++)
        {
            AwardItem temp = new AwardItem();
            string iconName = "Icon" + i.ToString();
            if (i == 10)
            {
                root = Content1;
                iconName = "Icon0";
            }
            temp.Icon = root.transform.FindButton(iconName);
            temp.ImageIcon = temp.Icon.transform.Find<Image>("ImageIcon");
            temp.TextCount = temp.Icon.transform.FindText("TextCount");
            temp.TextName = temp.Icon.transform.FindText("TextName");
            temp.Sign = temp.Icon.transform.Find<Image>("Sign");
            temp._imageQua = temp.Icon.transform.Find<Image>("ImageQua");
            _allKindsItems.Add(temp);
        }
    }

    private void InitBtn()
    {

        _okBtnTenTimes.onClick.AddListener(Close);
        _drawOnce.onClick.AddListener(DrawOnceAgain);
        _drawTenTimes.onClick.AddListener(DrawTenTimesAgain);
    }

    private void InitEvents()
    {
        //惊喜盲盒活动关闭时关闭界面
        AddEvent(EventCenter.Instance.UpdateAllActivity, _EventUpdateAllActivity);
        AddEvent(EventCenter.Instance.ActivityOverdue, _EventActivityOverdue);
    }

    private void _EventUpdateAllActivity()
    {
        if (!ActivityManager.Instance.IsActDuration(ActivityID.SupriseBox))
        {
            Close();
        }
    }
    private void _EventActivityOverdue(int aid)
    {
        if (aid == ActivityID.SupriseBox)
        {
            Close();
        }
    }

    private void DrawOnceAgain()
    {
        Close();
        EventCenter.Instance.UpdateActivityUI.Broadcast(208801);
    }

    private void DrawTenTimesAgain()
    {
        Close();
        EventCenter.Instance.UpdateActivityUI.Broadcast(208810);
    }
    public override bool IsFullScreen()
    {
        return false;
    }
    public override bool SkipWhenCloseAll()
    {
        return true;
    }
    public override bool NeedBlurBackground()
    {
        return false;
    }
    protected override void OnCreate()
    {

    }
    public void OnShow()
    {
        List<P_Item> _rewardsList = _actInfo.UniqueInfo.DrawPrizes;
        int length = _rewardsList.Count;
        SetRewardIcon();
        if (length <10 )
        {
            _get1.SetActive(true);
            _get10.SetActive(false);
            _blindBoxCoinNumInOnce.text = BagInfo.Instance.GetItemCount(ItemId.BlindBoxCoin).ToString();
            if (_actInfo.UniqueInfo.DrawRemainingNum <= 0)
            {
                _drawOnce.interactable = false;
            }
        }
        else
        {
            _get1.SetActive(false);
            _get10.SetActive(true);
            _blindBoxCoinNumInTenTimes.text = BagInfo.Instance.GetItemCount(ItemId.BlindBoxCoin).ToString();
            if (_actInfo.UniqueInfo.DrawRemainingNum < _actInfo.ten_times_price)
            {
                _drawTenTimes.interactable = false;
            }
        }

    }

    private void SetRewardIcon()
    {
        var _rewardsList = _actInfo.UniqueInfo.DrawPrizes.ToList();
        int length = _rewardsList.Count;

        int start = 0;
        int end = 10;

        if (length < 10)
        {
            start = 10;
            end = 11;
        }

        for (int i = start; i < end; i++)
        {
            var item = _rewardsList[i - start];
            cfg_act_2088_reward_pool tempItem = Cfg.Activity2088.GetRewardItem(item.Id, item.count);
            P_Item drawReward = new P_Item(tempItem.reward);

            var color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(drawReward.Id));
            _allKindsItems[i]._imageQua.color = color;

            _allKindsItems[i].Sign.gameObject.SetActive(false);
            Cfg.Item.SetItemIcon(_allKindsItems[i].ImageIcon,drawReward.Id);
            _allKindsItems[i].TextCount.text = "x" + drawReward.Num.ToString();
            _allKindsItems[i].TextName.text = Cfg.Item.GetItemName(drawReward.Id);

            _allKindsItems[i].Icon.GetComponent<Button>().onClick.RemoveAllListeners();
            var i1 = i;
            _allKindsItems[i].Icon.GetComponent<Button>().onClick.SetListener(() =>
            {
                DialogManager.ShowAsyn<_D_ItemTip>(d=>{ d?.OnShow(drawReward.Id, drawReward.Num, _allKindsItems[i1].Icon.transform.position); });
            });

            //判断是否为稀有
            _allKindsItems[i].Sign.gameObject.SetActive(tempItem.type != 1);

        }
    }
}

public class AwardItem
{
    public Button Icon;
    public Image ImageIcon;
    public Text TextCount;
    public Text TextName;
    public Image Sign;
    public Image _imageQua;
}