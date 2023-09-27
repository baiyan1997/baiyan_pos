using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _D_Act2301ExpGet : Dialog
{
    private Button _closeBtn;
    private ListView _listView;
    private ActInfo_2301 _actInfo;
    private bool _isShowing;

    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    protected override void InitRef()
    {
        _closeBtn = transform.FindButton("CloseButton");
        var root = transform.Find("Main_01/Inf/Scroll View");
        _listView = ListView.Create<ExpItem>(root);
        _actInfo = ActivityManager.Instance.GetActivityInfo(2301) as ActInfo_2301;
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
        if (aid != 2301 || !_isShowing)
        {
            return;
        }
        OnShow();
    }

    public void OnShow()
    {
        _isShowing = true;
        _listView.Clear();
        var list = _actInfo.UniqueInfo.mission_info;
        int len = list.Count;
        for (int i = 0; i < len; i++)
        {
            _listView.AddItem<ExpItem>().Refresh(list[i]);
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

public class ExpItem : ListItem
{
    private Text _missionName;
    private Text _progress;
    private Button _btnGet;
    private GameObject _unReach;
    private Text _expCount;
    private ActInfo_2301 _actInfo;
    private int _tid;
    public override void OnCreate()
    {
        _actInfo = ActivityManager.Instance.GetActivityInfo(2301) as ActInfo_2301;
        _missionName = transform.FindText("Text");
        _progress = transform.FindText("Progress");
        _btnGet = transform.FindButton("ButtonGet");
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
    public void Refresh(P_2301Mission p2301Mission)
    {
        _tid = p2301Mission.tid;
        _missionName.text = Lang.TranslateJsonString(p2301Mission.name);
        _progress.gameObject.SetActive(p2301Mission.need_count > 0);
        _progress.text = $"(<Color=#00ff33>{p2301Mission.do_number}</Color>/{p2301Mission.need_count})";
        _expCount.text = $"x{GlobalUtils.ParseItem(p2301Mission.reward)[0].count}";
        SetButton(p2301Mission.finished, p2301Mission.get_reward);
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
            _btnGet.gameObject.SetActive(true);
            _btnGet.interactable = true;
        }
        else
        {
            _unReach.SetActive(false);
            _btnGet.gameObject.SetActive(true);
            _btnGet.interactable = false;
        }
    }
}
