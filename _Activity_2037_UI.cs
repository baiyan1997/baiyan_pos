using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class _Activity_2037_UI : ActivityUI
{
    private Sprite _spBtnSelect;
    private Sprite _spBtnUnselect;

    private Text _actTime;
    private Text _des;
    private Button[] _menuBtns;
    private ListView _list;

    private Button _btnGo;//前往新服
    private GameObject _tip;//新服创角完成

    private ActInfo_2037 _info;
    private int _selectPage = 1;
    private Dictionary<int, List<cfg_act_2037>> _missionDict;
    private Color _colorSelect = Color.white;
    private Color _colorUnSelect = new Color(0.5f, 0.9f, 1, 1);

    public override void OnCreate()
    {
        ObjectGroup UI = gameObject.GetComponent<ObjectGroup>();
        _spBtnSelect = UI.Ref<Sprite>("_SpBtnSelect");
        _spBtnUnselect = UI.Ref<Sprite>("_SpBtnUnselect");

        _actTime = transform.FindText("Text_Time");
        _des = transform.FindText("des");
        _menuBtns = new[]
        {
            transform.FindButton("Meu/Btn_Tab1"),
            transform.FindButton("Meu/Btn_Tab2"),
            transform.FindButton("Meu/Btn_Tab3"),
        };
        _list = ListView.Create<_Act2037Item>(transform.Find("Scroll View"));

        _btnGo = transform.FindButton("BtnGo");
        _tip = transform.Find("Tip").gameObject;

        InitData();
        InitEvent();
        //InitListener();
        InitUI();
    }
    public override void OnShow()
    {

    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    private void InitData()
    {
        _info = (ActInfo_2037)ActivityManager.Instance.GetActivityInfo(2037);
    }

    private void InitEvent()
    {
        for (int i = 0; i < _menuBtns.Length; i++)
        {
            int page = i + 1;
            _menuBtns[i].onClick.AddListener(() =>
            {
                OnSelectPage(page);
            });
        }
        _btnGo.onClick.AddListener(On_btnGoClick);
    }
    private void On_btnGoClick()
    {
        DialogManager.ShowAsyn<_D_ServerList>(OnBtnGoDialogShowAsynCB);
    }
    private void OnBtnGoDialogShowAsynCB(_D_ServerList d)
    {
        d?.OnShowInGame();
    }

    public override void InitListener()
    {
        base.InitListener();

        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUI);
    }

    private void InitUI()
    {
        _missionDict = _info.GetMissionsByType();//拿取任务数据

        //默认加载
        OnUpdatePage(_selectPage);
        _btnGo.gameObject.SetActive(_info.go_to_new_server == 0);
        _tip.gameObject.SetActive(_info.go_to_new_server != 0);
        _des.text = _info._desc;
    }
    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (st - _info._data.startts < 0)
        {
            _actTime.text = GlobalUtils.GetActivityStartTimeDesc(_info._data.startts);
        }
        else if (_info.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_info.LeftTime);
            _actTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _actTime.text = Lang.Get("活动已经结束");
        }
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _info._data.aid)
            return;
        OnUpdatePage(_selectPage);
    }

    private void OnSelectPage(int page)
    {
        if (_selectPage == page)
            return;
        _selectPage = page;
        OnUpdatePage(page);
    }
    private void OnUpdatePage(int page)
    {
        _list.Clear();
        List<cfg_act_2037> list = _missionDict[page];
        Dictionary<int, int> dictTidWeight = new Dictionary<int, int>();
        for (int i = list.Count - 1; i >= 0; i--)
        {
            cfg_act_2037 cfg = list[i];
            var minfo = _info.FindMissionInfo(cfg.tid);
            if (minfo != null)
            {
                if (minfo.finished == 0)
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
        }
        list.Sort((a, b) =>
        {
            if (dictTidWeight[a.tid] > dictTidWeight[b.tid])
                return -1;
            if (dictTidWeight[a.tid] < dictTidWeight[b.tid])
                return 1;
            if (a.tid < b.tid)
                return -1;
            if (a.tid > b.tid)
                return 1;
            return 0;
        });
        for (int i = 0; i < list.Count; i++)
        {
            var item = _list.AddItem<_Act2037Item>();
            item.Refresh(list[i], _info.FindMissionInfo(list[i].tid));
        }
        for (int i = 0; i < 3; i++)
        {
            GameObject go = _menuBtns[i].gameObject;
            if (go.activeSelf)
            {
                if (i + 1 == page)
                {
                    go.GetComponent<Image>().sprite = _spBtnSelect;
                    go.GetComponentInChildren<Text>().color = _colorSelect;
                }
                else
                {
                    go.GetComponent<Image>().sprite = _spBtnUnselect;
                    go.GetComponentInChildren<Text>().color = _colorUnSelect;
                }

                var remindPoint = go.transform.Find("Remind").gameObject;
                bool need = false;
                var pageList = _missionDict[i + 1];
                for (int j = 0; j < pageList.Count; j++)
                {
                    var minfo = _info.FindMissionInfo(pageList[j].tid);
                    if (minfo != null && minfo.finished == 1 && minfo.get_reward == 0)
                        need = true;
                }
                remindPoint.SetActive(need);
            }
        }
    }
}

public class _Act2037Item : ListItem
{
    private Text _descTxt;
    private Text _countTxt;
    private GameObject _btnGet;
    private GameObject _btnForward;
    private GameObject _tipClaimed;
    private GameObject[] _rewards;
    private GameObject _tipForward;//未完成提示

    public override void OnCreate()
    {
        _descTxt = transform.FindText("Text_Desc");
        _countTxt = transform.FindText("Text_Count");
        _btnGet = transform.Find("Btn_Get").gameObject;
        _btnForward = transform.Find("Btn_Forward").gameObject;
        _tipClaimed = transform.Find("Tip_Claimed").gameObject;
        _tipForward = transform.Find("Tip_Forward").gameObject;
        _rewards = new[]
        {
            transform.Find("Reward1").gameObject,
            transform.Find("Reward2").gameObject,
            transform.Find("Reward3").gameObject
        };
    }

    public void Refresh(cfg_act_2037 cfg, P_Act2037Info info)
    {
        if (info != null)
        {
            if (cfg.needCount > 0)
            {
                _descTxt.text = cfg.desc;
                if (info.do_number < cfg.needCount)
                    _countTxt.text = string.Format("(<color=#ff004d>{0}</color>/{1})", info.do_number, cfg.needCount);
                else
                    _countTxt.text = string.Format("(<color=#00ff33>{0}</color>/{1})", info.do_number, cfg.needCount);
            }
            else
            {
                _descTxt.text = cfg.desc;
                _countTxt.text = "";
            }
        }
        else
        {
            _descTxt.text = cfg.desc;
            _countTxt.text = "";
        }

        var items = GlobalUtils.ParseItem(cfg.reward);
        for (int i = 0; i < 3; i++)
        {
            if (i < items.Length)
            {
                var item = items[i];
                var itemShow = ItemForShow.Create(item.id, item.count);
                _rewards[i].SetActive(true);
                var img = _rewards[i].transform.Find("Image");
                itemShow.SetIcon(img.GetComponent<Image>());
                img.GetComponent<Button>().onClick.SetListener(() =>
                {
                    ItemHelper.ShowTip(item.id, item.count, img.transform);
                });
                _rewards[i].transform.Find("Text").GetComponent<Text>().text = "x" + GLobal.NumFormat(itemShow.GetCount());
                _rewards[i].transform.Find("Qua").GetComponent<Image>().color = _ColorConfig.GetQuaColorHSV(itemShow.GetQua());
            }
            else
            {
                _rewards[i].SetActive(false);
            }
            SetButtonState(cfg, info);
        }
    }
    private void SetButtonState(cfg_act_2037 cfg, P_Act2037Info info)
    {
        if (info == null)
        {
            _btnForward.SetActive(false);
            _btnGet.SetActive(false);
            _tipClaimed.SetActive(false);
            _tipForward.SetActive(false);
        }
        else if (info.finished == 0) //未完成
        {
            if (cfg.type == 3)
            {
                _btnForward.SetActive(false);
                _btnGet.SetActive(false);
                _tipClaimed.SetActive(false);
                _tipForward.SetActive(true);
            }
            else
            {
                _btnForward.SetActive(true);
                _btnGet.SetActive(false);
                _tipClaimed.SetActive(false);
                _tipForward.SetActive(false);

                Button btn = _btnForward.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    MissionUtils.DoCustomFlow(cfg.click);
                });
            }
        }
        else if (info.finished == 1 && info.get_reward == 0) //完成未领取
        {
            _btnForward.SetActive(false);
            _btnGet.SetActive(true);
            _tipClaimed.SetActive(false);
            _tipForward.SetActive(false);

            Button btn = _btnGet.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                var actInfo = (ActInfo_2037)ActivityManager.Instance.GetActivityInfo(2037);
                if (actInfo != null)
                {
                    Uinfo.Instance.Bag.CheckBlueDrawAlert(cfg.reward, () =>
                    {
                        actInfo.RequestGetReward(info.tid);
                    });
                }
            });
        }
        else //已领取
        {
            _btnForward.SetActive(false);
            _btnGet.SetActive(false);
            _tipClaimed.SetActive(true);
            _tipForward.SetActive(false);
        }
    }
}

