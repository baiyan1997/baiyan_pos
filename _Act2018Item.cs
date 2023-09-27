using System;
using UnityEngine;
using UnityEngine.UI;

public class _Act2018Item : ListItem
{
    private Text _textTitle;
    private GameObject _btnGot;
    private Button _btnGet;
    private GameObject _btnHavent;

    private GameObject[] _bg;

    private GameObject[] _rewards;
    private Image[] _rewardIcons;
    private Image[] _rewardQua;
    private Text[] _rewardCount;

    private Action<_Act2018Item> _btnCallback;

    private const int MAX_REWARD_COUNT = 3;

    public override void OnCreate()
    {
        _textTitle = transform.Find<Text>("Title");
        _btnGot = transform.Find("GotBtn").gameObject;
        _btnGet = transform.FindButton("GetBtn");
        _btnHavent = transform.Find("HaventBtn").gameObject;
        _bg = new[]
        {
            transform.Find("FreeBg").gameObject,
            transform.Find("BusyBg").gameObject,
        };
        _rewards = new[]
        {
            transform.Find("Icon_01").gameObject,
            transform.Find("Icon_02").gameObject,
            transform.Find("Icon_03").gameObject,
        };
        _rewardIcons = new[]
        {
            _rewards[0].transform.FindImage("img_icon"),
            _rewards[1].transform.FindImage("img_icon"),
            _rewards[2].transform.FindImage("img_icon"),
        };
        _rewardQua = new[]
        {
            _rewards[0].transform.FindImage("Img_qua"),
            _rewards[1].transform.FindImage("Img_qua"),
            _rewards[2].transform.FindImage("Img_qua"),
        };
        _rewardCount = new[]
        {
            _rewards[0].transform.FindText("Text"),
            _rewards[1].transform.FindText("Text"),
            _rewards[2].transform.FindText("Text"),
        };
        //领取按钮回调
        //_btnGet.onClick.AddListener(() =>
        //{
        //    if (_btnCallback != null)
        //        _btnCallback(this);
        //});
        _btnGet.onClick.AddListener(On_btnGetClick);
    }
    private void On_btnGetClick()
    {
        if (_btnCallback != null)
            _btnCallback(this);
    }
    public void Refresh(P_Act2018Item info, Action<_Act2018Item> callback)
    {
        _btnCallback = callback;
        //刷新按钮状态
        UpdateUI(info);
    }

    public void UpdateUI(P_Act2018Item info)
    {
        var items = GlobalUtils.ParseItem(info.rewards);
        //刷新按钮状态
        switch ((Act2018ItemState)info.state)
        {
            case Act2018ItemState.Lock:
                _bg[0].SetActive(true);
                _bg[1].SetActive(false);

                _btnGot.SetActive(false);
                _btnHavent.SetActive(true);
                _btnGet.gameObject.SetActive(false);
                break;

            case Act2018ItemState.Unlock:
                _bg[0].SetActive(false);
                _bg[1].SetActive(true);

                _btnGot.SetActive(false);
                _btnHavent.SetActive(false);
                _btnGet.gameObject.SetActive(true);
                break;

            case Act2018ItemState.Got:
                _bg[0].SetActive(false);
                _bg[1].SetActive(true);

                _btnGot.SetActive(true);
                _btnHavent.SetActive(false);
                _btnGet.gameObject.SetActive(false);
                break;
            default:
                throw new AccessViolationException("have no this state:" + info.state);
        }
        //刷新标题
        _textTitle.text = string.Format(Lang.Get("需{0}人达标VIP{1} (<Color=#00ff00ff>{2}</Color>/{0})"), info.vip_num, info.vip_level, info.vip_havenum);
        //刷新奖励
        var len = items.Length;
        for (int i = 0, max = MAX_REWARD_COUNT; i < max; i++)
        {
            if (len > i)
            {
                var item = items[i];
                _rewards[i].SetActive(true);
                var showItem = ItemForShow.Create(item.id, item.count);
                 showItem.SetIcon(_rewardIcons[i]);
                _rewardQua[i].color = _ColorConfig.GetQuaColorHSV(showItem.GetQua());
                _rewardCount[i].text = "x" + GLobal.NumFormat(showItem.GetCount());
                //添加道具描述
                var i1 = i;
                _rewardIcons[i].GetComponent<Button>().onClick.SetListener(() =>
                {
                    ItemHelper.ShowTip(item.id, item.count, _rewardIcons[i1].transform);
                });
            }
            else
                _rewards[i].SetActive(false);
        }
    }
}

public enum Act2018ItemState
{
    Lock = 0,
    Unlock = 1,
    Got = 2
}
