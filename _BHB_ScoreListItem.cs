using UnityEngine;
using UnityEngine.UI;

public class _BHB_ScoreListItem
{

    public Transform transform
    {
        get
        {
            if (UI == null) return null;
            return UI.transform;
        }
    }
    public GameObject gameObject
    {
        get
        {
            if (UI == null) return null;
            return UI.gameObject;
        }
    }
    private ObjectGroup UI;
    private Text _txtIndex;
    private Text _txtKillCount;
    private Text _txtName;
    private Text _txtFleetName;
    private Text _txtRewardScore;
    private _StateFlag _iconState;

    public void InitUI(ObjectGroup ui)
    {
        UI = ui;
        _txtIndex = UI.Get<Text>("_txtIndex");
        _txtKillCount = UI.Get<Text>("_txtKillCount");
        _txtName = UI.Get<Text>("_txtName");
        _txtFleetName = UI.Get<Text>("_txtFleetName");
        _txtRewardScore = UI.Get<Text>("_txtRewardScore");
        _iconState = new _StateFlag(UI.Get<RectTransform>("_iconState"));
    }

    public void SetInfo(P_BlackholeBattle_PlayerRank info)
    {
        _txtIndex.text = info.rank.ToString();
        _txtKillCount.text = GLobal.NumFormat(info.kill_sum);
        _txtName.text = info.uname;
        // _txtFleetName.text =   GameRules.TeamIdToCornerName_MidLen(info.team_id);
        _txtFleetName.text = GameText.GetFleetNameOfPos(info.pos, info.team_seq); ;
        _txtRewardScore.text = GLobal.NumFormat(info.reward_score);
        _iconState.SetState(info.ustate);
    }
}
