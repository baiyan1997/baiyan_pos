using UnityEngine;
using UnityEngine.UI;


public class _ActRank2Item : ListItem
{
    private Text _textRank;
    private _StateFlag _union;
    private Text _textPlayer;
    private Text _textPpt;
    private Text _textReward;
    public override void OnCreate()
    {
        _textRank = transform.Find<Text>("Text_rank");

        _union = new _StateFlag(transform.Find<RectTransform>("Img_state"));

        _textPlayer = transform.Find<Text>("Text_name");
        _textPpt = transform.Find<Text>("Text_count");
        _textReward = transform.Find<Text>("Text_reward");
    }

    public void Refresh(P_ActRank2Item item, int state, int rank, int rewardLv)
    {
        Color color;
        switch (rank)
        {
            case 1:
                color = _ColorConfig.GetQuaColor(6);//紫
                break;
            case 2:
                color = _ColorConfig.GetQuaColor(5);//红
                break;
            case 3:
                color = _ColorConfig.GetQuaColor(4);//橙
                break;
            default:
                color = _ColorConfig.GetQuaColor(2);//蓝
                break;
        }
        _textRank.color = color;
        _textPlayer.color = color;
        _textPlayer.color = color;
        _textPpt.color = color;
        _textReward.color = color;

        _textRank.text = item.rank + "";

        //老活动2029服务端没发势力，按照规则为同一个势力
        if (state == 0)
            _union.SetState(PlayerInfo.Instance.Info.ustate);
        else
            _union.SetState(state);

        _textPlayer.text = item.uname;
        _textPpt.text = GlobalUtils.NumFormatPower(item.upower_history);
        if (rewardLv == 0)
            _textReward.text = Lang.Get("暂无");
        else
            _textReward.text = string.Format(Lang.Get("{0}档"), rewardLv);
    }
}

