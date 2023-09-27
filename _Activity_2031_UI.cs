using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2031_UI : ActivityUI
{
    private int _aid = 2031;
    private ObjectGroup _ui;
    private ActInfo_2031 _actInfo;
    private ListView list;
    private void InitData()
    {
        _actInfo = (ActInfo_2031)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void InitListener()
    {
        base.InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
    }

    public override void Awake()
    {
    }

    public override void OnCreate()
    {
        _ui = gameObject.GetComponent<ObjectGroup>();
        Transform rootView = transform.Find<Transform>("Scrollview");
        list = ListView.Create<_ActRewardList>(rootView);
        InitData();
        //InitListener();
        InitUI();
    }

    private void InitUI()
    {
        UpdateUi(_aid);
        _ui.Get<Text>("Text_timespan").text = GlobalUtils.ActTimeFormat(_actInfo._data.startts, _actInfo._data.endts);
        _ui.Get<Text>("des_text").text = _actInfo._desc;
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid);
    }

    private void UpdateUi(int aid)
    {
        if (aid == _aid)
        {
            list.Clear();
            for (int i = 0; i < _actInfo.data.Count; i++)
            {
                string note1 = string.Format(Lang.Get("累计充值:{0}氪晶"), _actInfo.data[i].needNum);
                int step = Mathf.Min(_actInfo.data[i].do_number, _actInfo.data[i].needNum);
                string note2 = string.Format("<Color=#00ff00ff>{0}</Color>/{1}", step, _actInfo.data[i].needNum);
                list.AddItem<_ActRewardList>().
                    Refresh(_actInfo.data[i].rewards, _actInfo.GetRewardById, _actInfo.Status, _actInfo.data[i].tid, note1, note2);
            }
        }
    }
    public override void OnShow()
    {
        InitData();
        InitUI();
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
public class _ActRewardList : ListItem
{
    private ListView _list;
    private int _id;
    private Button _getReward;
    private Button _toCharge;
    private Action<int, Action> _getRewardCb;
    private Dictionary<int, int> _status;//0未达成 1达成未领奖 2已领奖 3充值
    private GameObject _notReach;
    private GameObject _get;
    private GameObject _claimed;
    private GameObject _charge;
    private Text _note1;
    private Text _note2;
    private GameObject _busyBg;
    private GameObject _freeBg;

    public override void OnCreate()
    {
        _note1 = transform.Find<Text>("Text_title");
        _note2 = transform.Find<Text>("Text_step");
        _busyBg = transform.Find("BusyBg").gameObject;
        _freeBg = transform.Find("FreeBg").gameObject;

        _notReach = transform.Find("Btn_notReach").gameObject;
        _claimed = transform.Find("Btn_claimed").gameObject;
        _getReward = transform.Find<Button>("Btn_get");
        _toCharge = transform.Find<Button>("Btn_charge");
        _get = _getReward.gameObject;
        _charge = _toCharge.gameObject;
        _list = ListView.Create<_ActRewardItem>(transform.Find("ScrollView"));

        _getReward.onClick.AddListener(On_getRewardClick);
        _toCharge.onClick.AddListener(OnChargeBtnClick);
    }
    private void On_getRewardClick()
    {
        if (_getRewardCb != null)
        {
            _getRewardCb(_id, On_getRewardCB);
            _getRewardCb = null;
        }
    }
    private void On_getRewardCB()
    {
        _status[_id] = 2;
        ResetState();
        _freeBg.SetActive(true);
        _claimed.SetActive(true);
    }
    private void OnChargeBtnClick()
    {
        DialogManager.ShowAsyn<_D_Recharge>(OnChargeDialogShowAsynCB);
    }

    private void OnChargeDialogShowAsynCB(_D_Recharge d)
    {
        d?.OnShow(0);
    }

    public void Refresh(P_Item3[] rewards, Action<int, Action> ac, Dictionary<int, int> status, int id, string note1, string note2)
    {
        _getRewardCb = ac;
        _status = status;
        _id = id;
        _note1.text = note1;
        _note2.text = note2;

        _list.Clear();
        for (int i = 0; i < rewards.Length; i++)
        {
            _list.AddItem<_ActRewardItem>().Refresh(rewards[i]);
        }
        _list.ScrollRect.horizontalNormalizedPosition = 0;
        _list.ScrollRect.enabled = rewards.Length >= 4;//大于等于4个可以滑动
        int type;
        status.TryGetValue(_id, out type);
        ResetState();
        switch (type)//0未达成 1达成未领奖 2已领奖 3充值
        {
            case 0:
                _freeBg.SetActive(true);
                _notReach.SetActive(true);
                break;
            case 1:
                _busyBg.SetActive(true);
                _get.SetActive(true);
                break;
            case 2:
                _freeBg.SetActive(true);
                _claimed.SetActive(true);
                break;
            case 3:
                _freeBg.SetActive(true);
                _charge.SetActive(true);
                break;
            default:
                throw new Exception("can't find reward type " + type);
        }
    }

    private void ResetState()
    {
        _notReach.SetActive(false);
        _get.SetActive(false);
        _claimed.SetActive(false);
        _charge.SetActive(false);
        _busyBg.SetActive(false);
        _freeBg.SetActive(false);
    }
}