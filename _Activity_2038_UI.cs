using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class _Activity_2038_UI : ActivityUI
{

    private Transform _listRoot;
    private ObjectGroup _item0;
    private Text _txtTime;
    private List<MissionListItem> _itemList;
    private ActInfo_2038 _actInfo;

    private Button _btnTab0;
    private Button _btnTab1;
    private Image _redPoint0;
    private Image _redPoint1;

    private int _switchTab;

    public override void OnCreate()
    {
        ObjectGroup UI = gameObject.GetComponent<ObjectGroup>();
        _listRoot = UI.Get<Transform>("Content");
        _item0 = UI.Get<ObjectGroup>("List_01");
        _item0.gameObject.SetActive(false);
        _txtTime = UI.Get<Text>("_txtTime");
        _itemList = new List<MissionListItem>();

        _btnTab0 = UI.Get<Button>("_btnTab0");
        _btnTab1 = UI.Get<Button>("_btnTab1");

        _redPoint0 = UI.Get<Image>("_redPoint0");
        _redPoint1 = UI.Get<Image>("_redPoint1");

        _actInfo = (ActInfo_2038)ActivityManager.Instance.GetActivityInfo(ActivityID.PlanetWar);
        //TimeManager.Instance.TimePassSecond += UpdateTime;

        _btnTab0.onClick.AddListener(On_btnTab0Click);
        _btnTab1.onClick.AddListener(On_btnTab1Click);
        UI.Get<Text>("Desc").text = _actInfo._desc;

        //InitListener();
    }
    private void On_btnTab0Click()
    {
        SwitchTab(0);
    }
    private void On_btnTab1Click()
    {
        SwitchTab(1);
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    private void SwitchTab(int index)
    {
        _switchTab = index;
        switch (index)
        {
            case 0:
                UIHelper.SetImageSprite(_btnTab0.image, "Button/btn_489");
                UIHelper.SetImageSprite(_btnTab1.image, "Button/btn_490");

                break;
            case 1:
                UIHelper.SetImageSprite(_btnTab0.image, "Button/btn_490");
                UIHelper.SetImageSprite(_btnTab1.image, "Button/btn_489");

                break;
        }

        for (int i = 0; i < _itemList.Count; i++)
        {
            _itemList[i].gameObject.SetActive(false);
        }

        int startTid = 1;
        int endTid = 3;
        if (index > 0)
        {
            startTid = 4;
            endTid = 9;
        }

        for (int i = 0; i < _missionList.Count; i++)
        {
            if (_missionList[i].tid >= startTid && _missionList[i].tid <= endTid)
            {
                if (_itemList.Count == i) AddRewardItem();
                _itemList[i].SetInfo(_missionList[i]);
                _itemList[i].gameObject.SetActive(true);
            }
        }
    }

    private void AddRewardItem()
    {
        MissionListItem newOne = new MissionListItem(Object.Instantiate<ObjectGroup>(_item0), GetReward);
        newOne.transform.SetParent(_listRoot, false);
        newOne.gameObject.SetActive(false);
        _itemList.Add(newOne);
    }

    List<Act2038Mission> _missionList;

    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (st - _actInfo._data.startts < 0)
        {
            _txtTime.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _txtTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _txtTime.text = Lang.Get("活动已经结束");
        }
    }

    public void GetReward(int tid)
    {
        _actInfo.GetReward(tid, Refresh);
    }

    public override void OnShow()
    {
        _switchTab = 0;
        Refresh();
        UpdateTime(TimeManager.ServerTimestamp);
    }

    public void Refresh()
    {
        _missionList = _actInfo.GetMissionList();
        _redPoint0.gameObject.SetActive(false);
        _redPoint1.gameObject.SetActive(false);
        for (int i = 0; i < _missionList.Count; i++)
        {
            if (_missionList[i].tid >= 1 && _missionList[i].tid <= 3)
            {
                if (_missionList[i].finished > 0 && _missionList[i].get_reward == 0)
                    _redPoint0.gameObject.SetActive(true);
            }
            else
            {
                if (_missionList[i].finished > 0 && _missionList[i].get_reward == 0)
                    _redPoint1.gameObject.SetActive(true);

            }
        }
        SwitchTab(_switchTab);
    }



    class MissionListItem
    {
        public GameObject gameObject;
        public Transform transform;
        private List<MissionRewardItem> _rewardList;
        private Button _btnGet;
        private Button _btnDoing;
        private Button _btnDone;
        private Act2038Mission _info;
        private Text _txtTitle;
        public MissionListItem(ObjectGroup ui, Action<int> getReward)
        {
            gameObject = ui.gameObject;
            transform = ui.transform;
            _rewardList = new List<MissionRewardItem>();
            _rewardList.Add(new MissionRewardItem(ui.Get<ObjectGroup>("Icon_01")));
            _rewardList.Add(new MissionRewardItem(ui.Get<ObjectGroup>("Icon_02")));
            _rewardList.Add(new MissionRewardItem(ui.Get<ObjectGroup>("Icon_03")));

            _btnGet = ui.Get<Button>("_btnGet");
            _btnDoing = ui.Get<Button>("_btnDoing");
            _btnDone = ui.Get<Button>("_btnDone");
            _btnGet.onClick.AddListener(() =>
            {
                getReward(_info.tid);
            });

            _txtTitle = ui.Get<Text>("_txtTitle");
        }



        public void SetInfo(Act2038Mission info)
        {
            _info = info;
            if (_info.tid > 3)
                _txtTitle.text = string.Format("{0}（<Color=#00ff00ff>{1}</Color>/{2}）", Lang.TranslateFromDb(_info.detail.mission_desc), _info.do_number, _info.detail.need_count);
            else
                _txtTitle.text = string.Format("{0}（<Color=#00ff00ff>{1}</Color>/{2}）", Lang.TranslateFromDb(_info.detail.mission_desc), _info.do_number, _info.detail.need_count);
            for (int i = 0; i < _rewardList.Count; i++)
            {
                _rewardList[i].SetEmpty();
            }
            int count = 0;
            Debug.Log("Reward " + info.detail.reward);
            var items = GlobalUtils.ParseItem(info.detail.reward);
            for (int i = 0; i < items.Length; i++)
            {
                P_Item item = items[i];
                _rewardList[count].SetItem(item);
                count++;
            }

            _btnGet.gameObject.SetActive(false);
            _btnDoing.gameObject.SetActive(false);
            _btnDone.gameObject.SetActive(false);
            if (info.get_reward > 0) _btnDone.gameObject.SetActive(true);
            else if (info.finished > 0) _btnGet.gameObject.SetActive(true);
            else _btnDoing.gameObject.SetActive(true);
        }
    }

    class MissionRewardItem
    {
        private Image _imgIcon;
        private Image _imgQua;
        private Text _txtCount;
        private P_Item _rewardItem = new P_Item();

        public MissionRewardItem(ObjectGroup ui)
        {
            _imgIcon = ui.Get<Image>("_imgIcon");
            _imgQua = ui.Get<Image>("_imgQua");
            _txtCount = ui.Get<Text>("_txtCount");
            ui.Get<Button>("_imgIcon").onClick.AddListener(On_imgIconClick);
        }
        private void On_imgIconClick()
        {
            DialogManager.ShowAsyn<_D_ItemTip>(On_imgIconDialogShowAsynCB);
        }
        private void On_imgIconDialogShowAsynCB(_D_ItemTip d)
        {
            d?.OnShow(_rewardItem.id, _rewardItem.count, _imgIcon.transform.position);
        }
        public void SetItem(P_Item item)
        {
            _rewardItem = item;
            Cfg.Item.SetItemIcon(_imgIcon, _rewardItem.id);
            _txtCount.text = _rewardItem.count.ToString();
            _imgQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(_rewardItem.id));
            _imgIcon.gameObject.SetActive(true);
            _txtCount.gameObject.SetActive(true);
        }

        public void SetEmpty()
        {
            _imgQua.color = _ColorConfig.GetQuaColorHSV(1);
            _imgIcon.gameObject.SetActive(false);
            _txtCount.gameObject.SetActive(false);
        }
    }

}
