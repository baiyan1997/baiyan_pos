
using UnityEngine;
using UnityEngine.UI;

public class _ActRankItem : ListItem
{
    private Text _rank;
    private Text _name;
    private Text _count;
    private Text _reward;
    private _StateFlag _ustate;
    private GameObject _otherBg;
    private GameObject _myBg;
    public override void OnCreate()
    {
        _rank = transform.Find<Text>("Text_rank");
        _name = transform.Find<Text>("Text_name");
        _count = transform.Find<Text>("Text_count");
        _reward = transform.Find<Text>("Text_reward");
        _ustate = new _StateFlag(transform.Find<RectTransform>("Img_state"));
        _otherBg = transform.Find("Img_other").gameObject;
        _myBg = transform.Find("Img_my").gameObject;
    }

    public void Refresh(P_RankUserData data, int rank, int rewardLv)
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
        _rank.color = color;
        _name.color = color;
        _count.color = color;
        _reward.color = color;
        if (rank == 0)
        {
            _rank.text = Lang.Get("无");
        }
        else
        {
            _rank.text = rank.ToString();
        }
        _name.text = data.rankItem.uname;
        _count.text = GLobal.NumFormat_2(data.do_number);
        if (data.uid == User.Uid)
        {
            _name.text = Uinfo.Instance.Player.Info.uname;
            _otherBg.SetActive(false);
            _myBg.SetActive(true);
        }
        else
        {
            _otherBg.SetActive(true);
            _myBg.SetActive(false);
        }
        if (rewardLv == 0)
            _reward.text = string.Empty;
        else
            _reward.text = string.Format(Lang.Get("{0}档"), rewardLv);
        _ustate.SetState(data.rankItem.ustate);
    }
}

public class ActRankItem : MonoBehaviour
{
    public _ActRankItem item = null;

    public void CreateItem()
    {
        if(item == null) {
            item = new _ActRankItem();
            item.gameObject = gameObject;
            item.OnCreate();
            item.OnAddToList();
        }
    }

    void OnDestroy()
    {
        if(item != null) {
            item.OnRemoveFromList();
        }
    }
}