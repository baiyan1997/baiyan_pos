using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class _D_Act2088RewardList : Dialog
{
    private Transform _listViewRoot1;
    private Transform _listViewRoot2;
    private Transform _listViewRoot3;
    private ListView _listViewRewardList1;
    private ListView _listViewRewardList2;
    private ListView _listViewRewardList3;
    private ActInfo_2088 _actInfo;

    private int _aid = 2088;
    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    protected override void InitRef()
    {
        _actInfo = (ActInfo_2088)ActivityManager.Instance.GetActivityInfo(_aid);

        _listViewRoot1 = transform.Find("Main/ScrollView_Main/Viewport/Content/Scroll View1");
        _listViewRewardList1 = ListView.Create<PoolListItem>(_listViewRoot1);

        _listViewRoot2 = transform.Find("Main/ScrollView_Main/Viewport/Content/Scroll View2");
        _listViewRewardList2 = ListView.Create<PoolListItem>(_listViewRoot2);

        _listViewRoot3 = transform.Find("Main/ScrollView_Main/Viewport/Content/Scroll View3");
        _listViewRewardList3 = ListView.Create<PoolListItem>(_listViewRoot3);

    }

    public override bool IsFullScreen()
    {
        return false;
    }

    protected override void OnCreate()
    {
        InitEvents();
    }

    public void OnShow()
    {

        var _pool_info = _actInfo.UniqueInfo.pool_info;
        _listViewRewardList1.Clear();
        _listViewRewardList2.Clear();
        _listViewRewardList3.Clear();
        for (int i = 0; i < _pool_info.Count; i++)
        {
            var item = _pool_info[i];
            cfg_act_2088_reward_pool data = Cfg.Activity2088.GetRewardData(item.id);
            if (data.type == 3)
            {
                _listViewRewardList1.AddItem<PoolListItem>().OnShow(item);
            }
            else if (data.type == 2)
            {
                _listViewRewardList2.AddItem<PoolListItem>().OnShow(item);
            }
            else
            {
                _listViewRewardList3.AddItem<PoolListItem>().OnShow(item);
            }


        }

    }

    private void InitEvents()
    {
        //惊喜盲盒活动关闭时关闭界面
        AddEvent(EventCenter.Instance.UpdateAllActivity, _EventUpdateAllActivity);
        AddEvent(EventCenter.Instance.ActivityOverdue, _EventActivityOverdue);
    }

    private void _EventUpdateAllActivity()
    {
        if (!ActivityManager.Instance.IsActDuration(ActivityID.SupriseBox))
            Close();
    }
    private void _EventActivityOverdue(int aid)
    {
        if (aid == ActivityID.SupriseBox)
        {
            Close();
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _actInfo = null;
    }
}

public class PoolListItem : ListItem
{
    private Text _itemName;
    private Text _progress;
    private Text _count;
    private GameObject _tag;
    private int _id;
    private Image _icon;
    private Image _imageQua;
    private Button _showButton;
    public override void OnCreate()
    {
        InitRef();
    }

    private void InitRef()
    {
        _itemName = transform.Find<JDText>("TextName");
        _progress = transform.Find<JDText>("ProgressText");
        _count = transform.Find<Text>("TextCount");
        _icon = transform.Find<Image>("ImageIcon");
        _tag = transform.Find("tagText").gameObject;
        _imageQua = transform.Find<Image>("ImageQua");
        _showButton = transform.gameObject.GetComponent<Button>();

    }
    public void OnShow(P_Act2088PoolInfoItem _poolItem)
    {

        cfg_act_2088_reward_pool data = Cfg.Activity2088.GetRewardData(_poolItem.id);
        P_Item poolItem = new P_Item(data.reward);

        var color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(poolItem.Id));
        _imageQua.color = color;

        _showButton.GetComponent<Button>().onClick.SetListener(() =>
        {
            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(poolItem.Id, poolItem.count, _showButton.transform.position); });
        });

        Cfg.Item.SetItemIcon(_icon, poolItem.Id);
        _itemName.text = Cfg.Item.GetItemName(poolItem.Id);
        _count.text = "x" + poolItem.count.ToString();
        _tag.SetActive(data.type != 1);
        _progress.text = Lang.Get("已抽取{0}/{1}", _poolItem.num, data.limit_num);

    }


}