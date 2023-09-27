using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class _ActRankRewards : ListItem
{
    private Text _title;
    private Text _textReward;
    private ListView _list;
    private int _id;
    private Button _getReward;
    private Action<int, Action> _getRewardCb;
    private Action<Dictionary<int, bool>> _refresh;
    private GameObject _tip;
    private Text _tipText;
    private Dictionary<int, bool> _hasGet;
    public override void OnCreate()
    {
        _title = transform.Find<Text>("Text_title");
        _getReward = transform.Find<Button>("Btn_Reward");
        _textReward = transform.Find<Text>("Btn_Reward/Text_reward");
        _tip = transform.Find<Transform>("TimeTip").gameObject;
        _tipText = transform.Find<Text>("TimeTip/Text_tip");
        _list = ListView.Create<_ActRewardItem>(transform.Find("ScrollView"));

        _getReward.onClick.AddListener(On_getRewardClick);
    }
    private void On_getRewardClick()
    {
        if (_getRewardCb != null)
        {
            _getRewardCb(_id, On_getRewardCB);
        }
    }
    private void On_getRewardCB()
    {
        _hasGet[_id] = true;
        if (_refresh != null)//刷新领奖界面
            _refresh(_hasGet);
    }
    public void Refresh(P_RewardData rewards, string tip, Action<int, Action> ac, Dictionary<int, bool> hasGet, Action<Dictionary<int, bool>> refresh)
    {
        _id = rewards.id;
        _getRewardCb = ac;
        _hasGet = hasGet;
        _refresh = refresh;
        if (rewards.max_rank == 1)
        {
            _title.text = string.Format(Lang.Get("<Color={0}>首名</Color>"), GetColor(rewards.id), rewards.max_rank);
        }
        else
        {
            _title.text = string.Format(Lang.Get("<Color={0}>{1}-{2}名</Color>"), GetColor(rewards.id), rewards.min_rank, rewards.max_rank);
        }
        _list.Clear();
        for (int i = 0; i < rewards.rewards.Length; i++)
        {
            _list.AddItem<_ActRewardItem>().Refresh(rewards.rewards[i]);
        }
        _list.ScrollRect.enabled = rewards.rewards.Length >= 4;//大于等于4个可以滑动
        //state => 0达成未领奖 1未达成 2已领取 3未开启
        switch (rewards.state)
        {
            case 0:
                SetAvailable();
                break;
            case 1:
                SetTip(Lang.Get("未达成"));
                break;
            case 2:
                SetClaimed();
                break;
            case 3:
                SetTip(tip);
                break;
            case 4:
                SetBlank();//不显示任何按钮
                break;
            default:
                throw new Exception("state wrong not state " + rewards.state);
        }
    }
    public void SetClaimed()
    {
        _getReward.gameObject.SetActive(true);
        _tip.SetActive(false);
        _getReward.interactable = false;
        _textReward.text = Lang.Get("已领取");
    }
    public void SetAvailable()
    {
        _getReward.gameObject.SetActive(true);
        _tip.SetActive(false);
        _getReward.interactable = true;
        _textReward.text = Lang.Get("领取");
    }
    public void SetTip(string tip)
    {
        _getReward.gameObject.SetActive(false);
        _tip.SetActive(true);
        _tipText.text = tip;
    }

    public void SetBlank()
    {
        _getReward.gameObject.SetActive(false);
        _tip.SetActive(false);
    }

    private string GetColor(int rank)
    {
        string color = "#7EE5FFFF";
        switch (rank)
        {
            case 1:
                color = _ColorConfig.GetQuaColorText(6);//紫
                break;
            case 2:
                color = _ColorConfig.GetQuaColorText(5);//红
                break;
            case 3:
                color = _ColorConfig.GetQuaColorText(4);//橙
                break;
            case 4:
                color = _ColorConfig.GetQuaColorText(3);//绿
                break;
            case 5:
                color = _ColorConfig.GetQuaColorText(2);//蓝
                break;
            case 6:
                color = "#7EE5FFFF";//淡蓝
                break;
            default:
                break;
        }
        return color;
    }
}
public class _ActRewardItem : ListItem
{
    private Image _icon;
    private Image _iconQua;
    private Text _text;
    private Transform _tempCaptainCorner;

    public override void OnCreate()
    {
        _icon = transform.Find<Image>("Img_icon");
        _iconQua = transform.Find<Image>("Img_qua");
        _text = transform.Find<Text>("Text_count");
        _tempCaptainCorner = transform.Find("Corner");
    }

    public void Refresh(P_Item3 reward)
    {
        var itemShow = ItemForShow.Create(reward.itemid, reward.count);
        itemShow.SetIcon(_icon);
        _iconQua.color = _ColorConfig.GetQuaColorHSV(itemShow.GetQua());
        _text.text = "x" + GLobal.NumFormat(itemShow.GetCount());
        transform.GetComponent<Button>().onClick.SetListener(() =>
        {
            ItemHelper.ShowTip(reward.itemid, reward.count, transform);
        });
        if (_tempCaptainCorner != null)
            _tempCaptainCorner.gameObject.SetActive(reward.Id == ItemId.CaptainSample);
    }

    public void Refresh(P_Item2 reward)
    {
        Refresh(new P_Item3() { itemid = reward.id, count = reward.count });
    }
}
