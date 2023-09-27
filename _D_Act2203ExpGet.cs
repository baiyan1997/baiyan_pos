using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _D_Act2203ExpGet : Dialog
{
    private Button _closeBtn;
    private ListView _listView;
    private ActInfo_2203 _actInfo;
    private bool _isShowing;

    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    protected override void InitRef()
    {
        _closeBtn = transform.FindButton("CloseButton");
        var root = transform.Find("Main_01/Inf/Scroll View");
        _listView = ListView.Create<ExpItemAct>(root);
        _actInfo = ActivityManager.Instance.GetActivityInfo(2203) as ActInfo_2203;
    }

    public override bool IsFullScreen()
    {
        return false;
    }

    protected override void OnCreate()
    {
        _closeBtn.onClick.AddListener(Close);
        AddEvent(EventCenter.Instance.UpdateActivityUI, _EventUpdateActivityUI);
    }

    private void _EventUpdateActivityUI(int aid)
    {
        if (aid != 2203 || !_isShowing)
        {
            return;
        }
        OnShow();
    }

    public void OnShow()
    {
        _isShowing = true;
        _listView.Clear();
        var list = Cfg.Activity2203.GetCfgMissionList();
        int len = list.Count;
        for (int i = 0; i < len; i++)
        {
            _listView.AddItem<ExpItemAct>().Refresh(list[i]);
        }
    }

    protected override void OnClose()
    {
        base.OnClose();
        _isShowing = false;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _actInfo = null;
    }
}

public class ExpItemAct : ListItem
{
    private Text _missionName;
    private Text _progress;
    private Button _btnGet;
    private Text _getTxt;
    private GameObject _unReach;
    private Text _expCount;
    private ActInfo_2203 _actInfo;
    private P_2203Mission _Info;
    private int _tid;
    public override void OnCreate()
    {
        _actInfo = ActivityManager.Instance.GetActivityInfo(2203) as ActInfo_2203;
        _missionName = transform.FindText("Text");
        _progress = transform.FindText("Progress");
        _btnGet = transform.FindButton("ButtonGet");
        _getTxt = transform.FindText("ButtonGet/Text");
        _unReach = transform.Find("UnReach").gameObject;
        _expCount = transform.FindText("Icon/TextCount");
        _btnGet.onClick.AddListener(On_btnGetClick);
    }
    private void On_btnGetClick()
    {
        _actInfo.GetExp(_tid, On_btnGetExpCB);
    }
    private void On_btnGetExpCB()
    {
        SetButton(1, 1);
    }
    public void Refresh(cfg_act_2203_mission_config data)
    {
        var info = data;
        _tid = data.tid;
        _missionName.text = info != null ? Lang.TranslateJsonString(info.name) : "";
        _progress.gameObject.SetActive(data.need_count > 0);
        _expCount.text = $"x{data.exp}";
        _Info = _actInfo.getSeverMissionInfo(_tid);
        if (_Info == null) {
            _Info = new P_2203Mission()
            {
                do_number = 0,
                finished = 0,
                get_reward = 0
            };
        }

        _progress.text = $"(<Color=#00ff33>{_Info.do_number}</Color>/{data.need_count})";
        SetButton(_Info.finished, _Info.get_reward);
    }

    private void SetButton(int finished, int getReward)
    {
        if (finished == 0)
        {
            _unReach.SetActive(true);
            _btnGet.gameObject.SetActive(false);
        }
        else if (getReward == 0)
        {
            _unReach.SetActive(false);
            _getTxt.text = Lang.Get("领取奖励");
            _btnGet.gameObject.SetActive(true);
            _btnGet.interactable = true;
        }
        else
        {
            _unReach.SetActive(false);
            _getTxt.text = Lang.Get("已领取");
            _btnGet.gameObject.SetActive(true);
            _btnGet.interactable = false;
        }
    }
}
