using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2003_UI : ActivityUI
{
    [SerializeField]
    private Text _timeText;
    [SerializeField]
    private Sprite _spUnlock;
    [SerializeField]
    private Sprite _spSelect;
    [SerializeField]
    private Sprite _spLock;
    [SerializeField]
    private GameObject[] _dayItems;
    [SerializeField]
    private ListView _mainList;
    [SerializeField]
    private Text _shipName;

    private Button _shipDisplayBtn;

    private ActInfo_2003 _actInfo;

    private int _selectDay;

    private int _selectPage;

    private List<cfg_act_2003> _missionList;

    private ObjectGroup UI;

    //任务进度
    private Slider _slider;
    private GameObject[] _box;//领奖/预览宝箱
    private Text _programText;//当前进度值

    private string _box1IconPath;
    private string _box2IconPath;
    private string _boxGotPath;

    private Tween[] _anime;//奖励晃动动画
    private float _singleShakeTime = 0.12f;
    private float _shakePauseTime = 1.5f;
    public override void Awake()
    {
        UI = gameObject.GetComponent<ObjectGroup>();

        _timeText = transform.Find<Text>("Text_Time");
        _spUnlock = UI.Ref<Sprite>("_SpUnlock");
        _spSelect = UI.Ref<Sprite>("_SpSelect");
        _spLock = UI.Ref<Sprite>("_SpLock");
        _dayItems = new[]
        {
            transform.Find("7day/01").gameObject,
            transform.Find("7day/02").gameObject,
            transform.Find("7day/03").gameObject,
            transform.Find("7day/04").gameObject,
            transform.Find("7day/05").gameObject,
            transform.Find("7day/06").gameObject,
            transform.Find("7day/07").gameObject
        };
        _box = new[]
        {
            transform.Find("Box/Icon/Icon0").gameObject,
            transform.Find("Box/Icon/Icon1").gameObject,
            transform.Find("Box/Icon/Icon2").gameObject,
            transform.Find("Box/Icon/Icon3").gameObject,
            transform.Find("Box/Icon/Icon4").gameObject
        };
        _mainList = ListView.Create<_Act2003Item>(transform.Find("Scroll View"));
        _shipName = transform.Find<Text>("ship/Text_Name");
        _shipDisplayBtn = transform.Find<Button>("ship/RawImage");

        _box1IconPath = "Icon/icon_362";
        _box2IconPath = "Icon/icon_361";
        _boxGotPath = "Icon/icon_442";
        _slider = transform.Find<Slider>("Box/Slider");
        _programText = transform.Find<Text>("Box/Text");
        InitBoxLocation();
        if (_anime == null)
        {
            _anime = new Tween[_box.Length];
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_anime != null)
        {
            for (int i = 0; i < _anime.Length; i++)
            {
                if (_anime[i] != null)
                {
                    _anime[i].Kill();
                }
            }
            _anime = null;
        }
    }

    public override void OnCreate()
    {
        Init();
    }

    public override void OnShow()
    {
        UpdateTime(0);
        SetShip();
        RefreshSlider();
    }
    public override void OnClose()
    {
        base.OnClose();
        _ShipDisplayControl.Instance.CloseShipShow();
    }

    private void InitBoxLocation()
    {
        var parent = transform.Find("Box/Icon");
        float sum = Cfg.Activity2003.GetReward(_box.Length).need_value;

        var _rectTransform = _box[0].GetComponent<RectTransform>();
        var parentWidth = parent.GetComponent<RectTransform>().rect.width;
        var _startX = _rectTransform.rect.width / 2 - parentWidth / 2;
        var _moveLineWidth = parentWidth - _rectTransform.rect.width;
        for (int i = 0; i < _box.Length; i++)
        {
            float step = Cfg.Activity2003.GetReward(i + 1).need_value;
            var percent = Mathf.Min(step / sum, 1);
            _box[i].transform.localPosition = new Vector3(_startX + _moveLineWidth * percent, _box[i].transform.localPosition.y);
        }
    }
    private void SetShip()
    {
        var shipid = ActInfo_2003.ShipId;
        _ShipDisplayControl.Instance.ShowShip(shipid, _ShipDisplayControl.DisplayMode.AutoRotateOnly);
        _shipName.text = Cfg.Ship.GetShipName(shipid);
    }

    private void Init()
    {
        InitData();
        InitEvent();
        //InitListener();
        InitUI();
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2003)ActivityManager.Instance.GetActivityInfo(2003);
    }

    private void InitEvent()
    {
        for (int i = 0; i < _dayItems.Length; i++)
        {
            int day = i + 1;
            _dayItems[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                OnSelectDay(day);
            });
        }
        for (int i = 0; i < _box.Length; i++)
        {
            var i1 = i;
            _box[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                ClickRewardBox(i1 + 1);
            });
        }
        _shipDisplayBtn.onClick.SetListener(On_shipDisplayBtnClick);
    }
    private void On_shipDisplayBtnClick()
    {
        DialogManager.ShowAsyn<_D_ShareShipShow>(On_shipDisplayDialogShowAsynCB);
    }
    private void On_shipDisplayDialogShowAsynCB(_D_ShareShipShow d)
    {
        d?.Show(ActInfo_2003.ShipId, _shipDisplayBtn.transform.position, Direction.RightDown);
    }
    public override void InitListener()
    {
        base.InitListener();
    }
    private void RefreshSlider()
    {
        if (!_slider)
            return;
        int rate = _actInfo.GetProgram();
        float sum = Cfg.Activity2003.GetReward(_box.Length).need_value;
        _slider.value = (float)rate / sum;
        var record = _actInfo.GetRewardRecord();

        _programText.text = Lang.Get("进度奖励 {0}/{1}", rate, sum);

        for (int i = 0; i < _box.Length; i++)
        {
            if (_anime[i] != null)
            {
                _anime[i].Kill();
                _box[i].transform.localRotation = Quaternion.Euler(Vector3.zero);//角度复原
                _anime[i] = null;
            }
            if (record.Contains(i + 1))
            {
                //已经领取
                UIHelper.SetImageSprite(_box[i].GetComponent<Image>(), _boxGotPath);
                continue;
            }
            var rewardInfo = Cfg.Activity2003.GetReward(i + 1);
            //根据ui处理领取完之后的显示
            if (rewardInfo.need_value <= rate)
            {
                UIHelper.SetImageSprite(_box[i].GetComponent<Image>(), _box2IconPath);
                _anime[i] = DOTween.Sequence().Append(_box[i].transform.DOLocalRotate(new Vector3(0, 0, 10), _singleShakeTime))
                    .Append(_box[i].transform.DOLocalRotate(new Vector3(0, 0, -10), _singleShakeTime * 2))
                    .Append(_box[i].transform.DOLocalRotate(Vector3.zero, _singleShakeTime)).AppendInterval(_shakePauseTime);
                _anime[i].SetLoops(-1);
            }
            else
            {
                UIHelper.SetImageSprite(_box[i].GetComponent<Image>(), _box1IconPath);
            }
        }
    }

    private void ClickRewardBox(int index)
    {
        var rewardInfo = Cfg.Activity2003.GetReward(index);
        int currentProgress = _actInfo.GetProgram();//修改 当前的进度
        var progress = rewardInfo.need_value;
        var getReward = _actInfo.GetRewardRecord();

        P_Item[] tempReward = GlobalUtils.ParseItem(rewardInfo.reward);
        List<P_Item> reward = new List<P_Item>();
        for (int i = 0; i < tempReward.Length; i++)
        {
            var one = tempReward[i];
            reward.Add(one);
        }
        string title = Lang.Get("七日狂欢进度宝箱");
        string note = Lang.Get("进度值达到{0}可领取奖励", rewardInfo.need_value);
        if (currentProgress >= progress && !getReward.Contains(index))
        {
            DialogManager.ShowAsyn<_D_ShowRewards>(d =>
            {
                d?.ShowCustonRewards(reward, title, note, "领取", () =>
                {
                    _actInfo.GetBoxReward(index, UpdateUI);
                });
            });
        }
        else
        {
            DialogManager.ShowAsyn<_D_ShowRewards>(d => { d?.ShowCustonRewards(reward, title, note, "确定"); });
        }
    }
    public override void UpdateUI(int aid)
    {
        // PlatformWrap.Warn("触发了Act2003的UpdateUI aid = " + aid, false);
        base.UpdateUI(aid);
        if (aid != _actInfo._data.aid)
            return;
        if (!DialogManager.IsDialogShown<_D_ActCalendar>())
            return;
        UpdateRemindOfDay();
        //RefreshMission(_selectDay);
        OnUpdateMission();
        RefreshSlider();
    }

    private void UpdateRemindOfDay()
    {
        if (_dayItems == null)
            return;
        bool[] remindDays = new bool[7];
        for (int i = 0; i < _actInfo._missionInfo.Count; i++)
        {
            var mission = _actInfo._missionInfo[i];
            var day = Cfg.Activity2003.GetData(mission.tid).day;
            if (remindDays[day - 1])
                continue;
            if (mission.finished == 1 && mission.get_reward == 0)
                remindDays[day - 1] = true;
        }
        for (int i = 0; i < remindDays.Length; i++)
        {
            if (_dayItems[i])
            {
                _dayItems[i].transform.Find("Remind").gameObject.SetActive(remindDays[i]);
            }
        }
    }

    private void InitUI()
    {
        UpdateTime(TimeManager.ServerTimestamp);
        for (int i = 0; i < 7; i++)
        {
            _dayItems[i].transform.Find("Text").GetComponent<Text>().text = string.Format(Lang.Get("第{0}天"), i + 1);
        }
        //默认选中当前
        _selectDay = _actInfo._today;
        for (int i = 0; i < 7; i++)
        {
            SetDayState(i + 1);
        }
        UpdateRemindOfDay();
        OnSwitchDay(_selectDay);
        //RefreshMission(_selectDay);
        OnUpdateMission();
    }

    public override void UpdateTime(long obj)
    {
        base.UpdateTime(obj);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_timeText == null)
            return;

        if (_actInfo.LeftTime >= 0)
        {
            _timeText.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }

    private void OnUpdateMission()
    {
        if (_mainList == null)
            return;
        // PlatformWrap.Warn("触发了Act2003的OnUpdateMission", false);
        _mainList.RemoveAllItems();
        List<cfg_act_2003> list = _missionList;
        Dictionary<int, int> dictTidWeight = new Dictionary<int, int>();
        for (int i = list.Count - 1; i >= 0; i--)
        {
            cfg_act_2003 cfg = list[i];
            var minfo = _actInfo.FindMissionInfo(cfg.tid);
            if (minfo == null)
            {
                //过滤服务器没有传的任务
                if (cfg.day > _actInfo._today)
                    dictTidWeight.Add(cfg.tid, 0);
                else
                {
                    list.RemoveAt(i);
                }
            }
            else if (minfo.finished == 0)
            {
                dictTidWeight.Add(cfg.tid, 2);
            }
            else if (minfo.finished == 1 && minfo.get_reward == 0)
            {
                dictTidWeight.Add(cfg.tid, 3);
            }
            else
            {
                dictTidWeight.Add(cfg.tid, 1);
            }
        }
        list.Sort((a, b) =>
        {
            if (dictTidWeight[a.tid] > dictTidWeight[b.tid])
            {
                return -1;
            }
            else if (dictTidWeight[a.tid] < dictTidWeight[b.tid])
            {
                return 1;
            }
            else
            {
                if (a.tid < b.tid)
                    return -1;
                else if (a.tid > b.tid)
                    return 1;
                return 0;
            }
        });

        if (_mainList != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = _mainList.AddItem<_Act2003Item>();
                item.UpdateUI(list[i], _actInfo.FindMissionInfo(list[i].tid));
            }
        }
    }


    private void OnSwitchDay(int day)
    {
        _missionList = Cfg.Activity2003.GetNormalMissionsByDay(day);

        _mainList.RemoveAllItems();
    }

    private void OnSelectDay(int day)
    {
        if (day > _actInfo._today || day == _selectDay)
            return;

        int oriDay = _selectDay;
        _selectDay = day;
        SetDayState(oriDay);
        SetDayState(day);
        OnSwitchDay(day);

        //RefreshMission(day);
        OnUpdateMission();
    }

    private void SetDayState(int day)
    {
        GameObject go = _dayItems[day - 1];
        if (day == _selectDay)
        {
            go.GetComponent<Image>().sprite = _spSelect;
            go.GetComponent<Button>().interactable = false;
            go.transform.Find("Lock").gameObject.SetActive(false);
            return;
        }
        else if (day <= _actInfo._today)
        {
            go.GetComponent<Image>().sprite = _spUnlock;
            go.GetComponent<Button>().interactable = true;
            go.transform.Find("Lock").gameObject.SetActive(false);
        }
        else
        {
            go.GetComponent<Image>().sprite = _spLock;
            go.GetComponent<Button>().interactable = false;
            go.transform.Find("Lock").gameObject.SetActive(true);
        }
    }

}
