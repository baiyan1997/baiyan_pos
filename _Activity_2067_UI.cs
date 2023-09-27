using System;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2067_UI : ActivityUI
{
    private Text _txtTime;
    private Text _txtDesc;
    private Button _btnGet;
    private Text _txtGet;
    private Image _imgBtnGet;

    private ListView _listView1;
    private ListView _listView2;

    private ActInfo_2067 _info;
    private P_Slxf _firstMission;

    public override void OnCreate()
    {
        _info = ActivityManager.Instance.GetActivityInfo(2067) as ActInfo_2067;

        _txtTime = transform.Find<JDText>("Text_time");
        _txtDesc = transform.Find<JDText>("Scroll View/Viewport/Text_desc");
        _btnGet = transform.Find<Button>("GetBtn");
        _txtGet = transform.Find<JDText>("GetBtn/Text");
        _imgBtnGet = transform.Find<Image>("GetBtn");

        _listView1 = ListView.Create<_ActRewardItem>(transform.Find("ScrollRect1"));
        _listView2 = ListView.Create<SlxfListItem>(transform.Find("ScrollRect2"));

        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
        //TimeManager.Instance.TimePassSecond += UpdataTime;

        _btnGet.onClick.SetListener(On_btnGetClick);

        //InitListener();
    }
    private void On_btnGetClick()
    {
        _info.GetSlxfReward(_firstMission.tid, null);
    }
    public override void InitListener()
    {
        base.InitListener();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _info = null;
        _firstMission = null;
    }

    public override void OnShow()
    {
        UpdateUi(_info._aid);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid);
    }

    private void UpdateUi(int aid)
    {
        if (aid != _info._aid)
            return;
        if (_info.IsDuration())
        {
            RefreshView1();
            RefreshView2();
            UpdateTime(TimeManager.ServerTimestamp);
        }
    }


    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        long endts = _info.LeftTime;
        if (endts > 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)(endts));
            _txtTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _txtTime.text = Lang.Get("活动已经结束");
        }
    }

    private void RefreshView1()
    {
        _listView1.Clear();

        _firstMission = _info.GetFirstMission();
        if (_firstMission == null)
            return;
        _txtDesc.text = Cfg.Slxf.GetData(_firstMission.tid).mission_desc;
        string rewardstr = Cfg.Slxf.GetData(_firstMission.tid).reward;
        var rewards = GlobalUtils.ParseItem3(rewardstr);
        for (int i = 0; i < rewards.Length; i++)
        {
            _listView1.AddItem<_ActRewardItem>().Refresh(rewards[i]);
        }

        if (_firstMission.get_reward == 0 && _firstMission.finished == 1)
        {
            _btnGet.interactable = true;
            _imgBtnGet.color = _ColorConfig.ButtonGreen;
            _txtGet.text = Lang.Get("领取");
        }
        else if (_firstMission.get_reward == 1)
        {
            _btnGet.interactable = false;
            _imgBtnGet.color = _ColorConfig.ButtonGolden;
            _txtGet.text = Lang.Get("已领取");
        }
        else
        {
            _btnGet.interactable = false;
            _imgBtnGet.color = _ColorConfig.ButtonGolden;
            _txtGet.text = Lang.Get("未达成");
        }
    }

    private void RefreshView2()
    {
        _listView2.Clear();
        var missions = _info.GetAllMission();
        for (int i = 1; i < missions.Count; i++)
        {
            _listView2.AddItem<SlxfListItem>().Refresh(missions[i], _info);
        }
    }
}

public class SlxfListItem : ListItem
{
    private Text _textTitle;
    private GameObject _btnGot;
    private Button _btnGet;
    private GameObject _btnHavent;


    private GameObject[] _rewards;
    private Image[] _rewardIcons;
    private Image[] _rewardQua;
    private Text[] _rewardCount;

    private P_Slxf _info;
    private ActInfo_2067 _actInfo;

    private const int MAX_REWARD_COUNT = 3;
    public override void OnCreate()
    {
        _textTitle = transform.Find<Text>("Title");
        _btnGot = transform.Find("GotBtn").gameObject;
        _btnGet = transform.FindButton("GetBtn");
        _btnHavent = transform.Find("HaventBtn").gameObject;
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
        _btnGet.onClick.AddListener(On_btnGetClick);
    }
    private void On_btnGetClick()
    {
        _actInfo.GetSlxfReward(_info.tid, null);
    }


    public void Refresh(P_Slxf info, ActInfo_2067 actInfo)
    {
        _info = info;
        _actInfo = actInfo;
        //刷新按钮状态
        UpdateUI();
    }
    public void UpdateUI()
    {
        var data = Cfg.Slxf.GetData(_info.tid);
        string rewardstr = Cfg.Slxf.GetData(_info.tid).reward;
        P_Item3[] items = GlobalUtils.ParseItem3(rewardstr);
        //刷新按钮状态
        if (_info.finished == 0)
        {
            _btnGot.SetActive(false);
            _btnHavent.SetActive(true);
            _btnGet.gameObject.SetActive(false);
        }
        else
        {
            if (_info.get_reward == 0)
            {
                _btnGot.SetActive(false);
                _btnHavent.SetActive(false);
                _btnGet.gameObject.SetActive(true);
            }
            else
            {
                _btnGot.SetActive(true);
                _btnHavent.SetActive(false);
                _btnGet.gameObject.SetActive(false);
            }
        }
        //刷新标题
        int count = data.need_count;
        if (count == 0) //不显示0/1
            _textTitle.text = data.mission_desc;
        else
            _textTitle.text = string.Format("{0}({1}/{2})", data.mission_desc,
                "<Color=#00ff00ff>" + _info.do_number + "</Color>",
                count);
        //刷新奖励
        var len = items.Length;
        for (int i = 0, max = MAX_REWARD_COUNT; i < max; i++)
        {
            if (len > i)
            {
                var item = items[i];
                _rewards[i].SetActive(true);
                var showItem = ItemForShow.Create(item.itemid, item.count);
                showItem.SetIcon(_rewardIcons[i]);
                _rewardQua[i].color = _ColorConfig.GetQuaColorHSV(showItem.GetQua());
                _rewardCount[i].text = "x" + GLobal.NumFormat(showItem.GetCount());
                //添加道具描述

                var i1 = i;
                _rewards[i].transform.GetComponent<Button>().onClick.SetListener(() =>
                {
                    ItemHelper.ShowTip(item.itemid, item.count, _rewards[i1].transform);
                });
            }
            else
                _rewards[i].SetActive(false);
        }
    }
}