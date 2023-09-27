using UnityEngine;
using UnityEngine.UI;

public class _D_Act2094Ranking : Dialog
{
    private Button _btnClose;
    private ListReuse3<Act2094RankItem> _list;
    private ActInfo_2094 _actInfo;
    private Text _myRankNum;
    private Text _myDamage;
    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    protected override void InitRef()
    {
        _btnClose = transform.FindButton("Main/CloseBtn");
        var root = transform.Find("Main/Scrollview");
        _list = ListReuse3<Act2094RankItem>.Create(root, RefreshIndex,
            ScrollDirectionEnum.Vertical, 12, 0.3f,
            new RectOffset(0, 0, 12, 0));
        _myRankNum = transform.FindText("Main/Me/Text_rank");
        _myDamage = transform.FindText("Main/Me/damage");
        _actInfo = ActivityManager.Instance.GetActivityInfo(2094) as ActInfo_2094;
    }

    private void RefreshIndex(ListItem item, int index)
    {
        var list = _actInfo.RankingInfo.AllRankInfo;
        P_Act2094RankItemInfo info = index >= list.Count ? null : list[index];
        ((Act2094RankItem)item).Refresh(info, index + 1);
    }

    public override bool IsFullScreen()
    {
        return false;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_list != null)
        {
            _list.OnDestroy();
            _list = null;
        }
    }

    protected override void OnCreate()
    {
        _btnClose.onClick.AddListener(Close);
    }

    public void OnShow()
    {
        _list.SetBeginAndRefresh(50);
        RefreshMe();
    }

    private void RefreshMe()
    {
        _myRankNum.text = GetMyRankNum();
        _myDamage.text = _actInfo.RankingInfo.u_score.ToString();
    }

    private string GetMyRankNum()
    {
        int myRank = _actInfo.RankingInfo.u_rank;
        if (myRank == 0)
        {
            return Lang.Get("无");
        }
        return myRank > 50 ? ">50" : myRank.ToString();
    }
}

public class Act2094RankItem : ListItem
{
    private Text _textRankNum;
    private Text _textUserName;
    private Text _textDamage;
    private Act2094Reward[] _rewards;
    private Button _btnSeeLineup;
    private P_Act2094RankItemInfo _rankInfo;
    private ActInfo_2094 _actInfo;
    public override void OnCreate()
    {
        _actInfo = ActivityManager.Instance.GetActivityInfo(2094) as ActInfo_2094;
        _textRankNum = transform.FindText("Text_rankNum");
        _textUserName = transform.FindText("Text_name");
        _textDamage = transform.FindText("Text_damage");
        _btnSeeLineup = transform.FindButton("Button");
        _rewards = new[]
        {
            new Act2094Reward(transform.Find("reward1")),
            new Act2094Reward(transform.Find("reward2")),
            new Act2094Reward(transform.Find("reward3"))
        };
        _btnSeeLineup.onClick.AddListener(On_btnSeeLineupClick);
    }
    private void On_btnSeeLineupClick()
    {
        _actInfo.GetShipLine(_rankInfo);
    }

    public void Refresh(P_Act2094RankItemInfo info, int index)
    {
        _rankInfo = info;
        _textRankNum.text = index.ToString();
        _textUserName.text = info == null ? Lang.Get("未上榜") : $"Lv.{info.u_lv} {info.uname}";
        _textDamage.text = info == null ? Lang.Get("伤害值：{0}", 0) : Lang.Get("伤害值：{0}", info.score);
        RefreshRewards(index);
        _btnSeeLineup.gameObject.SetActive(true);
        if (index > 10 || info == null)
        {
            _btnSeeLineup.gameObject.SetActive(false);
        }
    }

    private void RefreshRewards(int index)
    {
        P_Item[] items = Cfg.Act2094.GetRewardItemsByRankNumber(index);
        int len = items.Length;
        for (int i = 0; i < len; i++)
        {
            _rewards[i].SetVisible(true);
            _rewards[i].Refresh(items[i]);
        }

        for (int i = len; i < _rewards.Length; i++)
        {
            _rewards[i].SetVisible(false);
        }
    }
}
public class Act2094Reward
{
    private Transform _transform;
    private Image _icon;
    private Image _imgQua;
    private Text _countText;
    private Button _btn;
    private int _itemId;
    private int _itemCount;
    public Act2094Reward(Transform transform)
    {
        _transform = transform;
        OnCreate();
    }

    private void OnCreate()
    {
        _icon = _transform.FindImage("img_icon");
        _imgQua = _transform.FindImage("Img_qua");
        _countText = _transform.FindText("Text");
        _btn = _icon.GetComponent<Button>();
        _btn.onClick.AddListener(On_btnClick);
    }
    private void On_btnClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_btnClickDialogShowAsynCB);
    }
    private void On_btnClickDialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(_itemId, _itemCount, _transform.position);
    }

    public void Refresh(P_Item item)
    {
        _itemId = item.Id;
        _itemCount = item.Num;
        _countText.text = $"x{_itemCount}";
        Cfg.Item.SetItemIcon(_icon, _itemId);
        _imgQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(_itemId));
    }

    public void SetVisible(bool type)
    {
        _transform.gameObject.SetActive(type);
    }
}