using System;
using System.Collections.Generic;
using UnityEngine.UI;
public class _Activity_Rank_Inf : JDBehaviour
{
    private ListView _list;
    private Text _note;
    private Button _ok;
    private List<P_RewardData> _rewards;
    private int _rewardLv;
    private string _tip;
    private Action<int, Action> _getReward;
    private bool _isOpen;
    private bool _singleReward;

    public override void Awake()
    {
        _note = transform.Find<Text>("Text_note");
        _ok = transform.Find<Button>("Btn_ok");
        _list = ListView.Create<_ActRankRewards>(transform.Find("ScrollView"));
    }

    /// <summary>
    /// rewards奖励列表 rewardLv玩家当前奖励档位 tip按钮上提示 isOpen领取奖励是否开启 getReward领取后回调 hasGet:key-奖励档位 value-是否已领
    /// singleReward-是否只有一个档位可领取
    /// </summary>
    public void OnShow(List<P_RewardData> rewards, string note, int rewardLv, string tip, bool isOpen, Action<int, Action> getReward, Dictionary<int, bool> hasGet, bool singleReward = false)
    {
        //_list.ScrollRect.verticalNormalizedPosition = 1;
        _note.text = note;
        OnShow(rewards, rewardLv, tip, isOpen, getReward, hasGet, singleReward);
    }
    public void OnShow(List<P_RewardData> rewards, int rewardLv, string tip, bool isOpen, Action<int, Action> getReward, Dictionary<int, bool> hasGet, bool singleReward = false)
    {
        _singleReward = singleReward;
        _rewards = rewards;
        _rewardLv = rewardLv;
        _tip = tip;
        _getReward = getReward;
        _isOpen = isOpen;
        RefreshUi(hasGet);
    }

    public void RefreshUi(Dictionary<int, bool> hasGet)
    {
        _list.Clear();
        for (int i = 0; i < _rewards.Count; i++)
        {
            _rewards[i].state = GetBtnState(_isOpen, _rewards[i].id, _rewardLv, hasGet);
        }
        for (int i = 0; i < _rewards.Count; i++)
        {
            var reward = _rewards[i];
            _list.AddItem<_ActRankRewards>().Refresh(reward, _tip, _getReward, hasGet, RefreshUi);
        }
    }
    public void SetCloseCb(Action ac)
    {
        _ok.onClick.SetListener(() => ac?.Invoke());
    }
    //0达成未领奖 1未达成 2已领取 3未开启 4不显示
    private int GetBtnState(bool isOpen, int id, int rewardLv, Dictionary<int, bool> hasGet)
    {
        if (isOpen)
        {
            if (id >= rewardLv && rewardLv > 0)
            {
                bool get;
                hasGet.TryGetValue(id, out get);
                if (get)//已领
                {
                    return 2;
                }
                if (_singleReward && id > rewardLv)
                    return 4;//只显示达到等级那一档
                else 
                    return 0;//可领
            }

            if (!_singleReward)
                return 1; //未达成
            else
                return 4;//不显示
        }
        if (!_singleReward)
            return 3; //未开启
        else
            return 4;//不显示
    }
}


