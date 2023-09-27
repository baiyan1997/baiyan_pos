using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2046_UI : ActivityUI
{
    private Text _lvText;
    private Text _proText;
    private Slider _slider;
    private Text _leftTimeText;
    private Button _getBtn;
    private Text _descText;
    private ScrollRect _scrollRect;
    private GameObject _taskPref;
    private Button _detailBtn;
    private Text _missionTimeText;

    private ActInfo_2046 _actInfo;
    private Dictionary<int, GameObject> _missionGoDict;
    private bool _isUpdate;

    public override void OnCreate()
    {
        InitData();
        InitEvent();
        //InitListener();
    }

    private void InitEvent()
    {
        _detailBtn.onClick.SetListener(On_detailBtnClick);
        _getBtn.onClick.SetListener(On_getBtnClick);
    }
    private void On_detailBtnClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);
        if (_actInfo != null)
            DialogManager.ShowAsyn<ActRewardList>(detailBtnDialogShowAsynCB);
    }
    private void detailBtnDialogShowAsynCB(ActRewardList d)
    {
        d?.OnShow(_actInfo.ShowItemList, Lang.Get("百宝箱奖池"));
    }
    private void On_getBtnClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);
        if (_actInfo != null)
            _actInfo.GetBoxReward();
    }

    public override void InitListener()
    {
        base.InitListener();
    }


    private void InitData()
    {
        _lvText = transform.Find<Text>("Box/LvText");
        _proText = transform.Find<JDText>("Slider/TextExp");
        _slider = transform.Find<Slider>("Slider");
        _leftTimeText = transform.Find<JDText>("LeftTime");
        _getBtn = transform.Find<Button>("ButtonGet");
        _descText = transform.Find<JDText>("TextDesc");
        _scrollRect = transform.Find<ScrollRect>("Scroll View");
        _taskPref = transform.Find<GameObject>("missionPref");

        _detailBtn = transform.Find<Button>("Box/ButtonDetail");
        _missionTimeText = transform.Find<JDText>("TextMissionTime");

        _missionGoDict = new Dictionary<int, GameObject>();
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2046)ActivityManager.Instance.GetActivityInfo(2046);

        //更新宝箱
        UpdateBox();

        //更新任务
        UpdateMissionUI();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (_actInfo._aid != aid)
            return;

        _isUpdate = true;
        OnShow();
    }

    private void UpdateBox()
    {
        if (_actInfo.getAllBox)
        {
            _lvText.text = Lang.Get("已领完");
        }
        else
        {
            _lvText.text = string.Format(Lang.Get("Lv.{0}"), _actInfo.lv);
        }

        int lastExp = Cfg.Activity2046Box.GetNeedExp(_actInfo.lv - 1, _actInfo.step);
        int curEXP = _actInfo.curExp - lastExp;
        int curTotalExp = _actInfo.totalExp - lastExp;
        _proText.text = string.Format("EXP {0}/{1}", curEXP, curTotalExp);
        float newValue = (float)curEXP / curTotalExp;

        //避免进度条减少的过渡效果，先为0
        if (_slider.value > newValue)
        {
            _slider.value = 0;
        }
        DOTween.To(() => _slider.value, x => _slider.value = x, newValue, 1.0f);

        if (_actInfo.curExp >= _actInfo.totalExp && _actInfo.totalExp != 0 && !_actInfo.getAllBox)
        {
            _getBtn.gameObject.SetActive(true);
            _leftTimeText.gameObject.SetActive(false);
        }
        else
        {
            _getBtn.gameObject.SetActive(false);
            _leftTimeText.gameObject.SetActive(true);
        }

        _descText.text = _actInfo._desc;
    }

    private void UpdateMissionUI()
    {
        Clear();
        var missionList = _actInfo.MissionList;
        missionList.Sort(Sort_act2046Mission);
        for (int i = 0; i < missionList.Count; ++i)
        {
            Act2046Mission mission = missionList[i];
            GameObject go = GameObject.Instantiate(_taskPref, _scrollRect.transform.GetChild(0).GetChild(0));
            go.transform.localScale = Vector3.one;
            go.SetActive(true);
            go.name = mission.id.ToString();
            _missionGoDict.Add(mission.id, go);
            DefineMissionUI(go.transform, mission);
        }
    }
    private int Sort_act2046Mission(Act2046Mission a, Act2046Mission b)
    {
        if (a.isFinished != b.isFinished)
        {
            if (a.isGet != b.isGet)
            {
                return (a.isGet ? 1 : 0) - (b.isGet ? 1 : 0);
            }
            else
            {
                return (a.isFinished ? 0 : 1) - (b.isFinished ? 0 : 1);
            }
        }
        else
        {
            if (a.isGet != b.isGet)
            {
                return (a.isGet ? 1 : 0) - (b.isGet ? 1 : 0);
            }
            else
            {
                return a.id - b.id;
            }
        }
    }

    private void DefineMissionUI(Transform trans, Act2046Mission config)
    {
        Text name = trans.Find<Text>("NameText");
        Button getBtn = trans.Find<Button>("BtnGet");
        GameObject getText = trans.Find<GameObject>("TextHave");
        Button goBtn = trans.Find<Button>("BtnMission");

        name.text = config.desc + string.Format("({0}/{1})", config.getCount, config.totalCount);

        Transform[] itemList = new Transform[]
        {
            trans.Find("ScrollView/Viewport/rewardPref/item0"),
            trans.Find("ScrollView/Viewport/rewardPref/item1"),
            trans.Find("ScrollView/Viewport/rewardPref/item2"),
        };

        for (int i = 0; i < config.itemList.Count; ++i)
        {
            DefineRewardUI(itemList[i], config.itemList[i]);
        }

        if (config.isGet)
        {
            getText.SetActive(true);

            getBtn.gameObject.SetActive(false);
            goBtn.gameObject.SetActive(false);
        }
        else
        {
            getText.SetActive(false);


            if (config.IsAvailable())
            {
                getBtn.gameObject.SetActive(true);
                getBtn.onClick.SetListener(() =>
                {
                    AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

                    _actInfo.GetTaskReward(config.id);
                });

                goBtn.gameObject.SetActive(false);
            }
            else
            {
                goBtn.onClick.SetListener(() =>
                {
                    AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

                    MissionUtils.DoCustomFlow(Cfg.Activity2046.GetClick(config.id));
                });

                getBtn.gameObject.SetActive(false);
                goBtn.gameObject.SetActive(true);
            }
        }
    }

    private void DefineRewardUI(Transform trans, P_Item item)
    {
        Image icon = trans.Find<Image>("Img_icon");
        Image qua = trans.Find<Image>("Img_qua");
        Text count = trans.Find<Text>("Text_count");
        ItemForShow itemforShow = ItemForShow.Create(item.id, item.count);
        itemforShow.SetUI(icon, count, qua, true);

        trans.GetComponent<Button>().onClick.SetListener(() =>
        {
            AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(item.id, item.count, trans.position); });
        });
    }

    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (_actInfo == null || !gameObject.activeSelf)
            return;

        //任务刷新倒计时
        if (_actInfo.missionLeftTime >= 0)
        {
            _missionTimeText.text = string.Format(Lang.Get("任务刷新倒计时 {0}"), GLobal.TimeFormat(_actInfo.missionLeftTime, true));
        }
        else
        {
            if (_isUpdate)
            {
                _isUpdate = false;
                ActivityManager.Instance.RequestUpdateActivityById(_actInfo._aid);
            }
        }

        if (!_leftTimeText.gameObject.activeSelf)
            return;

        //活动时间倒计时
        if (st - _actInfo._data.startts < 0)
        {
            _leftTimeText.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _leftTimeText.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _leftTimeText.text = Lang.Get("活动已经结束");
        }
    }

    private void Clear()
    {
        foreach (KeyValuePair<int, GameObject> pair in _missionGoDict)
        {
            Destroy(pair.Value);
        }

        _missionGoDict.Clear();
    }
}
