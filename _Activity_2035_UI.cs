using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class _Activity_2035_UI : ActivityUI
{
    private Text[] _nameTexts;
    private Text[] _countTexts;
    private Image[] _iconImages;
    private Image[] _iconQuas;
    private Button _btnBind;
    private Button _getReward;
    private GameObject _tipClaimed;
    private List<RewardItem> _rewardList;
    private ActInfo_2035 _actInfo;
    private int _aid = 2035;
    public override void OnCreate()
    {
        _nameTexts = new[]
        {
            transform.Find<Text>("Icon/01/Text_Name"),
            transform.Find<Text>("Icon/02/Text_Name"),
            transform.Find<Text>("Icon/03/Text_Name"),
        };
        _countTexts = new[]
        {
            transform.Find<Text>("Icon/01/Text_Count"),
            transform.Find<Text>("Icon/02/Text_Count"),
            transform.Find<Text>("Icon/03/Text_Count"),
        };
        _iconImages = new[]
        {
            transform.Find<Image>("Icon/01/Img_Icon"),
            transform.Find<Image>("Icon/02/Img_Icon"),
            transform.Find<Image>("Icon/03/Img_Icon"),
        };
        _iconQuas = new[]
        {
            transform.Find<Image>("Icon/01/Img_qua"),
            transform.Find<Image>("Icon/02/Img_qua"),
            transform.Find<Image>("Icon/03/Img_qua")
        };
        _btnBind = transform.Find<Button>("Button_01");
        _getReward = transform.Find<Button>("Button_02");
        _tipClaimed = transform.Find<GameObject>("Img_Claimed");
        InitData();
        InitEvent();
        //InitListener();
        InitUI();
    }
    public override void OnShow()
    {
        if (_actInfo._data.get_all_reward == false)
        {
            ActivityManager.Instance.RequestUpdateActivityById(_aid);
        }
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2035)ActivityManager.Instance.GetActivityInfo(_aid);
        _rewardList = _actInfo.RewardList;
    }

    private void InitEvent()
    {
        _btnBind.onClick.AddListener(On_btnBindClick);
        _getReward.onClick.AddListener(On_getRewardClick);
    }
    private void On_btnBindClick()
    {
        DialogManager.ShowAsyn<_D_Account>(OnbtnBindDialogShowAsynCB);
    }
    private void OnbtnBindDialogShowAsynCB(_D_Account d)
    {
        d?.OnShow();
    }
    private void On_getRewardClick()
    {
        _actInfo.GetReward();
    }
    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);

    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid);
    }

    private void InitUI()
    {
        for (int i = 0; i < _rewardList.Count; i++)
        {
            var _itemShow = ItemForShow.Create(_rewardList[i].id, _rewardList[i].count);
            _itemShow.SetIcon(_iconImages[i]);
            _iconQuas[i].color = _ColorConfig.GetQuaColorHSV(_itemShow.GetQua());
            var i1 = i;
            _iconImages[i].GetComponent<Button>().onClick.SetListener(() =>
            {
                ItemHelper.ShowTip(_rewardList[i1].id, _rewardList[i1].count, _iconImages[i1].transform);
            });
            _countTexts[i].text = "x" + GLobal.NumFormat(_itemShow.GetCount());
            _nameTexts[i].text = _itemShow.GetName();
        }
        UpdateUi(_aid);
    }

    private void UpdateUi(int aid)
    {
        if (aid == _aid)
        {
            //已领状态
            if (_actInfo._data.get_all_reward)
            {
                _getReward.gameObject.SetActive(false);
                _btnBind.gameObject.SetActive(false);
                _tipClaimed.SetActive(true);
            }
            else
            {
                _getReward.gameObject.SetActive(_actInfo._data.can_get_reward);
                _btnBind.gameObject.SetActive(!_actInfo._data.can_get_reward);
                _tipClaimed.SetActive(false);
            }
        }
    }
}

