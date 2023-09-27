using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2106_UI : ActivityUI
{
    private _Act2106MissionItem _mainMission;
    private ListView _listViewMission;
    private JDText _textTitle;

    private ActInfo_2106 _actInfo;

    private readonly int _aid = 2106;

    public override void OnCreate()
    {
        _listViewMission = ListView.Create<_Act2106MissionItem>(transform.Find("Scroll View"));
        _mainMission = new _Act2106MissionItem { gameObject = transform.Find("MainMission").gameObject };
        _mainMission.OnCreate();
        _textTitle = transform.Find<JDText>("Title/Text");

        EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
        EventCenter.Instance.Act2106MissionUpdate.AddListener(OnShow);
    }

    public override void OnShow()
    {
        UpdateUi(_aid);
    }

    private void UpdateUi(int aid)
    {
        if(transform == null){
            return;
        }
        if (aid != _aid)
            return;
        _actInfo = (ActInfo_2106)ActivityManager.Instance.GetActivityInfo(_aid);
        if (_actInfo == null || !_actInfo.IsDuration())
            return;
        var missionList = _actInfo.MissionList;
        _listViewMission.Clear();
        for (int i = 0; i < missionList.Count; i++)
        {
            var info = missionList[i];
            if(Cfg.Activity2106.IsMainMission(info.tid))
            {
                _mainMission.Refresh(info);
                _textTitle.text = Lang.Get("主线: {0}", Cfg.Activity2106.GetTitle(info.tid));
                continue;
            }
            _listViewMission.AddItem<_Act2106MissionItem>().Refresh(info);
        }
    }
}

class _Act2106MissionItem : ListItem
{
    private JDText _textTitle;
    private Button _btnGo;
    private Button _btnGetReward;
    private GameObject _objGot;
    private GameObject _objTagFinish;
    private ListView _listRewards;

    private P_Act2106Mission _info;
    private readonly int _aid = 2106;

    public override void OnCreate()
    {
        _textTitle = transform.Find<UnityEngine.UI.JDText>("TextTitle");
        _btnGo = transform.Find<UnityEngine.UI.Button>("ButtonGo");
        _btnGetReward = transform.Find<UnityEngine.UI.Button>("ButtonGet");
        _objGot = transform.Find("ButtonGot").gameObject;
        _objTagFinish = transform.Find<GameObject>("TagFinish", false);
        _listRewards = ListView.Create<_ActRewardItem>(transform.Find("Scroll View"));

        _btnGo.onClick.AddListener(() =>
        {
            if(_info != null)
            {
                //引导前默认关闭活动窗口
                DialogManager.CloseDialog<_D_ActCalendar>();
                DialogManager.CloseDialog<_D_ActCalendar>();//执行两遍以保证关掉
                var data = Cfg.Activity2106.GetData(_info.tid);
                MissionUtils.DoCustomFlow(data.guide);
            }
        });
        _btnGetReward.onClick.AddListener(() =>
        {
            var actInfo = (ActInfo_2106)ActivityManager.Instance.GetActivityInfo(_aid);
            if(actInfo != null && _info != null)
            {
                actInfo.GetMissionReward(_info.tid, null);
            }
        });
    }

    public void Refresh(P_Act2106Mission info)
    {
        _info = info;
        var data = Cfg.Activity2106.GetData(_info.tid);
        _textTitle.text = Cfg.Activity2106.IsMainMission(_info.tid) ? Lang.Get("<Color=#ffcc00>任务详情</Color>: {0}", data.name) : data.name;
        var status = _info.finished == 0 ? 0 : (_info.get_reward == 0 ? 1 : 2);
        _btnGo.gameObject.SetActive(status == 0);
        _btnGetReward.gameObject.SetActive(status == 1);
        _objGot.SetActive(status == 2);
        _objTagFinish?.SetActive(status == 2);

        var rewards = GlobalUtils.ParseItem3(data.reward);
        _listRewards.Clear();
        for (int i = 0; i < rewards.Length; i++)
            _listRewards.AddItem<_ActRewardItem>().Refresh(rewards[i]);
    }
}
