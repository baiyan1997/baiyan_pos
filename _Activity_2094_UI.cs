using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2094_UI : ActivityUI
{
    private Text _countDown;
    private Button _btnOpenRanking;
    private Button _btnNextForecast;
    private Text _textItemNum;
    private Text _textCurtLineup;
    private Button _btnHelp;
    private _sectShipItem[] _sectInfos;
    private Act2094Reward[] _rewards;
    private Button _startBtn;
    private GameObject _costObj;
    //private GameObject _free;
    private ActInfo_2094 _actInfo;
    private int _costItemId = 0;
    private GameObject _redPoint;

    public override void OnCreate()
    {
        InitRef();
        InitEvent();
        //InitListener();
    }

    private void InitEvent()
    {
        _btnHelp.onClick.AddListener(On_btnHelpClick);
        _btnOpenRanking.onClick.AddListener(On_btnOpenRankingClick);
        _btnNextForecast.onClick.AddListener(On_btnNextForecastClick);
        _startBtn.onClick.AddListener(On_startBtnClick);
    }

    private void On_btnHelpClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnHelpDialogShowAsynCB);
    }
    private void On_btnHelpDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2094, _btnHelp.transform.position, Direction.LeftDown, 350);
    }
    private void On_btnOpenRankingClick()
    {
        _actInfo.RefreshRankingInfo(OnActRefreshRankingInfoCB);
    }
    private void OnActRefreshRankingInfoCB()
    {
        DialogManager.ShowAsyn<_D_Act2094Ranking>(On_btnOpenRankingDialogShowAsynCB);
    }
    private void On_btnOpenRankingDialogShowAsynCB(_D_Act2094Ranking d)
    {
        d?.OnShow();
    }
    private void On_btnNextForecastClick()
    {
        DialogManager.ShowAsyn<_D_Act2094Lineup>(On_btnNextForecastDialogShowAsyn);
    }
    private void On_btnNextForecastDialogShowAsyn(_D_Act2094Lineup d)
    {
        d?.OnShow(1);
    }
    private void On_startBtnClick()
    {
        if (BagInfo.Instance.GetItemCount(ItemId.ExerciseToken) < 1 && _actInfo.InitInfo.free == 0)
        {
            MessageManager.Show(Lang.Get("演习令不足"));
            return;
        }       
        DialogManager.ShowAsyn<_D_ArenaTeamDispose>(On_startBtnDialogShowAsynCB);
    }
    private void On_startBtnDialogShowAsynCB(_D_ArenaTeamDispose d)
    {
        string title = Lang.Get("演习挑战舰队选择");
        d?.OnShow(title, string.Empty, SelectTeamsCallBack, 1, 1);
    }
    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdatePlayerItem);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdatePlayerItem);
    }

    private void UpdatePlayerItem()
    {
        _textItemNum.text = BagInfo.Instance.GetItemCount(ItemId.ExerciseToken).ToString();
        EventCenter.Instance.RemindActivity.Broadcast(2094, _actInfo.IsAvaliable());
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != 2094)
        {
            return;
        }
        _btnNextForecast.gameObject.SetActive(_actInfo.InitInfo.next_boss_id != 0);
        UpdateTime(TimeManager.ServerTimestamp);
        RefreshBossLineup();
        RefreshStartBtn();
        EventCenter.Instance.RemindActivity.Broadcast(2094, _actInfo.IsAvaliable());
    }

    private void SelectTeamsCallBack(List<int> teams)
    {
        _actInfo.StartExercise(teams, RefreshStartBtn);
    }

    private void InitRef()
    {
        _countDown = transform.FindText("TextCountDown");
        _btnOpenRanking = transform.FindButton("Btn_RankingList");
        _btnNextForecast = transform.FindButton("Btn_NextForecast");
        _textItemNum = transform.FindText("Item/ItemNum/Text");
        _textCurtLineup = transform.FindText("Text_CurrentLineup");
        _btnHelp = transform.FindButton("_btnManual");
        _sectInfos = new[]
        {
            new _sectShipItem(transform.Find("Inf_01/01")),
            new _sectShipItem(transform.Find("Inf_01/02")),
            new _sectShipItem(transform.Find("Inf_01/03")),
            new _sectShipItem(transform.Find("Inf_02/01")),
            new _sectShipItem(transform.Find("Inf_02/02")),
            new _sectShipItem(transform.Find("Inf_02/03"))
        };
        _rewards = new[]
        {
            new Act2094Reward(transform.Find("BottomInf/reward1")),
            new Act2094Reward(transform.Find("BottomInf/reward2")),
            new Act2094Reward(transform.Find("BottomInf/reward3")),
            new Act2094Reward(transform.Find("BottomInf/reward4"))
        };
        _startBtn = transform.FindButton("BottomInf/Btn_start");
        _redPoint = _startBtn.transform.Find("RedPoint").gameObject;
        _costObj = transform.Find("BottomInf/CostItem").gameObject;
        //_free = transform.Find("Free").gameObject;
        _actInfo = ActivityManager.Instance.GetActivityInfo(2094) as ActInfo_2094;
    }

    public override void OnShow()
    {
        _textItemNum.text = BagInfo.Instance.GetItemCount(ItemId.ExerciseToken).ToString();
        _btnNextForecast.gameObject.SetActive(_actInfo.InitInfo.next_boss_id != 0);
        UpdateTime(TimeManager.ServerTimestamp);
        RefreshBossLineup();
        RefreshBottom();
    }

    private void RefreshBottom()
    {
        RefreshStartBtn();
        RefreshRewards();
    }

    private void RefreshRewards()
    {
        P_Item[] items = Cfg.Act2094.GetRewardItemsByRankNumber(1);
        int len = items.Length;
        for (int i = 0; i < len; i++)
        {
            _rewards[i].SetVisible(true);
            _rewards[i].Refresh(items[i]);
        }

        int len2 = _rewards.Length;
        for (int i = len; i < len2; i++)
        {
            _rewards[i].SetVisible(false);
        }
    }

    private void RefreshStartBtn()
    {
        _costObj.SetActive(_actInfo.InitInfo.free == 0);
        _redPoint.SetActive(_actInfo.InitInfo.free > 0);
    }

    private List<int> _posArr = new List<int> { 2, 1, 3, 4, 5, 6 };
    private void RefreshBossLineup()
    {
        var list = _actInfo.BossList;
        int len = list.Count;
        for (int i = 0; i < len; i++)
        {
            P_Act2094BossInfo info = list[i];
            int index = 0;
            for (int j = 0; j < 6; j++)
            {
                if (_posArr[j] == info.pos)
                {
                    index = j;
                    break;
                }
            }
            _sectInfos[index].Refresh(info.captain_id, info.radar_id, info.ship_id);
        }
    }

    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        long leftTime = _actInfo.InitInfo.end_ts - nowTs;
        _countDown.text = Lang.Get("本期挑战剩余时 {0}", GlobalUtils.ActivityLeftTime(leftTime, false));
    }

    public override void OnClose()
    {
        base.OnClose();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
