using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class _Activity_2001_UI : ActivityUI
{
    private Text[] _nameTexts;

    private Text[] _countTexts;

    private Image[] _iconImages;

    //品质框
    private Image[] _iconQuas;

    private Text _time;

    private Button _rechargeBtn;

    private Button _getAwardBtn;

    private GameObject _tipClaimed;

    private Button _shipDisplayBtn;

    private ActInfo_2001 _firstRechargeActivity;
    private List<RewardItem> _rewardList;
    private int _shipId = -1;

    public override void Awake()
    {
        // _nameTexts = new[]
        // {
        //     transform.Find<Text>("Icon_Reward/01/Text_Name"),
        //     transform.Find<Text>("Icon_Reward/02/Text_Name"),
        //     transform.Find<Text>("Icon_Reward/03/Text_Name"),
        // };
        // _countTexts = new[]
        // {
        //     transform.Find<Text>("Icon_Reward/01/Text_Count"),
        //     transform.Find<Text>("Icon_Reward/02/Text_Count"),
        //     transform.Find<Text>("Icon_Reward/03/Text_Count"),
        // };
        // _iconImages = new[]
        // {
        //     transform.Find<Image>("Icon_Reward/01/Img_Icon"),
        //     transform.Find<Image>("Icon_Reward/02/Img_Icon"),
        //     transform.Find<Image>("Icon_Reward/03/Img_Icon"),
        // };
        // _iconQuas = new[]
        // {
        //     transform.Find<Image>("Icon_Reward/01/Img_qua"),
        //     transform.Find<Image>("Icon_Reward/02/Img_qua"),
        //     transform.Find<Image>("Icon_Reward/03/Img_qua")
        // };
        // _time = transform.Find<Text>("Text_Desc");
        // _rechargeBtn = transform.Find<Button>("Btn_Recharge");
        // _getAwardBtn = transform.Find<Button>("Btn_Get");
        // _tipClaimed = transform.Find("Img_Claimed").gameObject;
        // _shipDisplayBtn = transform.Find<Button>("ShowShip/RawImage");
    }

    public override void OnCreate()
    {
        // InitData();
        // InitEvent();
        // //InitListener();
        // InitUI();
    }

    public override void OnShow()
    {
        // if (_shipId != -1)
        // {
        //     _ShipDisplayControl.Instance.ShowShip(_shipId, _ShipDisplayControl.DisplayMode.AutoRotateOnly);
        // }
    }

    public override void OnClose()
    {
        base.OnClose();
        _ShipDisplayControl.Instance.CloseShipShow();
    }

    private void InitData()
    {
        // _firstRechargeActivity = (ActInfo_2001)ActivityManager.Instance.GetActivityInfo(2001);
        // _rewardList = _firstRechargeActivity.RewardList;
    }

    private void InitEvent()
    {
        // _rechargeBtn.onClick.AddListener(OnClickRechargeBtn);
        // _getAwardBtn.onClick.AddListener(OnClickGetAwardBtn);
        // _shipDisplayBtn.onClick.SetListener(On_shipDisplayBtnClick);
    }
    private void On_shipDisplayBtnClick()
    {
        // DialogManager.ShowAsyn<_D_ShareShipShow>(On_shipDisplayDialogShowAsynCB);
    }

    private void On_shipDisplayDialogShowAsynCB(_D_ShareShipShow d)
    {
        // d?.Show(_shipId, _shipDisplayBtn.transform.position, Direction.Center);
    }

    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUI);
    }

    private void InitUI()
    {
        // var mainReward = _firstRechargeActivity.MainReward;
        // var shipid = mainReward.id;
        // _shipId = shipid;
        // _ShipDisplayControl.Instance.ShowShip(shipid, _ShipDisplayControl.DisplayMode.AutoRotateOnly);

        // JDDebug.Dump(_rewardList);
        // for (int i = 0; i < _rewardList.Count && i < _iconImages.Length; i++)
        // {
        //     var _itemShow = ItemForShow.Create(_rewardList[i].id, _rewardList[i].count);
        //     _itemShow.SetIcon(_iconImages[i]);
        //     _iconQuas[i].color = _ColorConfig.GetQuaColorHSV(_itemShow.GetQua());

        //     var i1 = i;
        //     _iconImages[i].GetComponent<Button>().onClick.SetListener(() =>
        //     {
        //         ItemHelper.ShowTip(_rewardList[i1].id, _rewardList[i1].count, _iconImages[i1].transform);
        //     });
        //     _countTexts[i].text = "x" + GLobal.NumFormat(_itemShow.GetCount());
        //     _nameTexts[i].text = _itemShow.GetName();
        // }

        // UpdateUI(_firstRechargeActivity._data.aid);
    }

    public override void UpdateTime(long stamp)
    {
        // base.UpdateTime(stamp);
        // if (gameObject == null || !gameObject.activeInHierarchy)
        //     return;
        // if (stamp - _firstRechargeActivity._data.startts < 0)
        // {
        //     _time.text = GlobalUtils.GetActivityStartTimeDesc(_firstRechargeActivity._data.startts);
        // }
        // else if (_firstRechargeActivity.LeftTime >= 0)
        // {
        //     TimeSpan span = new TimeSpan(0, 0, (int)_firstRechargeActivity.LeftTime);
        //     _time.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
        //         span.Minutes, span.Seconds);
        // }
        // else
        // {
        //     _time.text = Lang.Get("活动已经结束");
        // }
    }

    public override void UpdateUI(int aid)
    {
        // base.UpdateUI(aid);
        // if (aid != _firstRechargeActivity._data.aid)
        //     return;
        // if (gameObject == null)
        //     return;

        // // 可领状态 、充值状态、 已领状态

        // if (!_firstRechargeActivity._data.get_all_reward)
        // {
        //     _rechargeBtn.gameObject.SetActive(!_firstRechargeActivity._data.can_get_reward);
        //     _getAwardBtn.gameObject.SetActive(_firstRechargeActivity._data.can_get_reward);
        //     _tipClaimed.SetActive(false);
        // }
        // else // 已领
        // {
        //     _rechargeBtn.gameObject.SetActive(false);
        //     _getAwardBtn.gameObject.SetActive(false);
        //     _tipClaimed.SetActive(true);
        // }
    }

    private void OnClickRechargeBtn()
    {
        // DialogManager.ShowAsyn<_D_Recharge>(OnRechargeDialogShowAsynCB);
    }
    private void OnRechargeDialogShowAsynCB(_D_Recharge d)
    {
        // d?.OnShow(0);
    }

    private void OnClickGetAwardBtn()
    {
        // _firstRechargeActivity.RequestGetAward(0);
    }
}
