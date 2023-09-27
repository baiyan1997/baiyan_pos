using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2091_UI : ActivityUI
{
    private Text _nameText;
    private Text _timeText;
    private Button _helpBtn;
    //邀请方
    private GameObject _ui1;
    private Button _shareBtn;
    private Text _codeText;
    //被邀请方
    private GameObject _ui2;
    private InputField _inputField;
    private Button _btnBand;
    private GameObject _partGo1;
    private GameObject _partGo2;

    //任务
    private Transform _listTrans1;
    private ListView _listView1;
    private Transform _listTrans2;
    private ListView _listView2;

    private ActInfo_2091 _config = null;
    private const int _aid = 2091;
    private string _otherCode;


    public override void OnCreate()
    {
        _nameText = transform.Find<JDText>("Title");
        _ui1 = transform.Find<GameObject>("Main_01");
        _ui2 = transform.Find<GameObject>("Main_02");

        _shareBtn = transform.Find<Button>("Main_01/ButtonShare");
        _codeText = transform.Find<JDText>("Main_01/TextCode");
        _inputField = transform.Find<InputField>("Main_02/Part1/InputField");
        UIHelper.FixInputInWebGL(_inputField);
        _btnBand = transform.Find<Button>("Main_02/Part1/ButtonBand");
        _partGo1 = transform.Find<GameObject>("Main_02/Part1");
        _partGo2 = transform.Find<GameObject>("Main_02/Part2");
        _listTrans1 = transform.Find("Main_01/Scroll View1");
        _listView1 = ListView.Create<Act2091Mission>(_listTrans1);
        _listTrans2 = transform.Find("Main_02/Scroll View2");
        _listView2 = ListView.Create<Act2091Mission>(_listTrans2);
        _timeText = transform.Find<JDText>("CountDown");
        _helpBtn = transform.Find<Button>("BtnHelp");

        InitEvent();
        //InitListener();
    }

    private void InitEvent()
    {
        _helpBtn.onClick.AddListener(On_helpBtnClick);
        _shareBtn.onClick.SetListener(On_shareBtnClick);
        _btnBand.onClick.SetListener(On_btnBandClick);
        _inputField.onValidateInput += (text, charIndex, addedChar) =>
        {
            return addedChar;
        };
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Top_HelpDesc>(On_helpBtnDialogShowAsynCB);
    }
    private void On_helpBtnDialogShowAsynCB(_D_Top_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2091);
    }
    private void On_shareBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        _config = (ActInfo_2091)ActivityManager.Instance.GetActivityInfo(_aid);
        if (_config == null)
            return;
        if (_config.InviteType != 3)
            return;
        if (CheckPhone())
        {
            _config.SendCodeCheck();
        }
    }
    private void On_btnBandClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        _config = (ActInfo_2091)ActivityManager.Instance.GetActivityInfo(_aid);
        if (_config == null)
            return;
        if (_config.InviteType == 3)
            return;
        if (CheckPhone())
        {
            _otherCode = _inputField.text;
            if (string.IsNullOrEmpty(_otherCode))
                return;
            if (_otherCode.Length != 6)
            {
                MessageManager.Show(Lang.Get("输入的邀请码有误"));
                return;
            }
            string pattern = @"^[a-zA-Z0-9]*$";
            if (Regex.IsMatch(_otherCode, pattern))
            {
                //请求服务端检测是否满足关联
                _config.SendBand(_otherCode);
            }
            else
            {
                MessageManager.Show(Lang.Get("输入的邀请码有误"));
            }
        }
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        _config = (ActInfo_2091)ActivityManager.Instance.GetActivityInfo(_aid);

        _nameText.text = _config._name;

        //展示邀请
        if (_config.InviteType == 3)
        {
            int max = Cfg.Activity2091.GetBoxMaxProgress();
            _ui1.SetActive(true);
            _ui2.SetActive(false);

            if (string.IsNullOrEmpty(_config.MyCode))
                _codeText.text = Lang.Get("前往绑定手机号");
            else
                _codeText.text = Lang.Get("我的邀请码：{0}", _config.MyCode);

            _listView1.Clear();
            for (int i = 0; i < _config.MissionList.Count; i++)
            {
                P_Act2091Mission item = _config.MissionList[i];
                _listView1.AddItem<Act2091Mission>().Refresh(item);
            }
        }
        //展示被邀请
        else
        {
            _ui1.SetActive(false);
            _ui2.SetActive(true);

            if (_config.IsBand)
            {
                _partGo1.SetActive(false);
                _partGo2.SetActive(true);
            }
            else
            {
                _partGo1.SetActive(true);
                _partGo2.SetActive(false);

                //检测邀请码
                if (CheckInvite())
                {
                }
                else
                {
                    _inputField.text = "";
                    _otherCode = "";
                }
            }

            _listView2.Clear();
            for (int i = 0; i < _config.MissionList.Count; i++)
            {
                P_Act2091Mission item = _config.MissionList[i];
                _listView2.AddItem<Act2091Mission>().Refresh(item);
            }
        }
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (_aid != aid)
            return;

        if (!gameObject.activeSelf)
            return;

        OnShow();
    }

    private bool CheckPhone()
    {
        //检测是否绑定手机号
        if (!_config.BandPhone)
        {
            MessageManager.Show(Lang.Get("请先在设置中完成手机号码绑定"));
            return false;
        }
        return true;
    }

    public bool CheckInvite()
    {
        //绑定过手机号的玩家，检测剪切板
        string str = GUIUtility.systemCopyBuffer;

        if (string.IsNullOrEmpty(str))
            return false;

        if (str.Length == 6)
        {
            string pattern = @"^[a-zA-Z0-9]*$";
            if (Regex.IsMatch(str, pattern))
            {
                _inputField.text = str;
                _otherCode = str;
                return true;
            }
        }
        return false;
    }

    public override void UpdateTime(long obj)
    {
        base.UpdateTime(obj);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_config == null)
            return;

        if (!_config.IsDuration())
        {
            var leftTime = _config._data.startts - TimeManager.ServerTimestamp;
            if (leftTime < 0)
                leftTime = 0;
            _timeText.text = Lang.Get("开启倒计时 {0}", GLobal.TimeFormat(leftTime, true));
            return;
        }

        if (_config.LeftTime >= 0)
        {
            _timeText.text = GlobalUtils.ActivityLeftTime(_config.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        _config = null;
    }
}

public class Act2091Mission : ListItem
{
    private Text _tittle;
    private Text _progress;
    private Button _getBtn;
    private GameObject _unreachGo;
    private GameObject _getGo;
    private GameObject[] _rewardGoList;
    private GameObject _unBandGo;
    private int _aid = 2091;
    private P_Act2091Mission _data = null;

    public override void OnCreate()
    {
        _rewardGoList = new GameObject[]
       {
            transform.Find<GameObject>("Icon_01"),
            transform.Find<GameObject>("Icon_02"),
            transform.Find<GameObject>("Icon_03"),
            transform.Find<GameObject>("Icon_04"),
       };
        _tittle = transform.Find<Text>("Title");
        _progress = transform.Find<Text>("TextCount");
        _getBtn = transform.Find<Button>("GetBtn");
        _getGo = transform.Find<GameObject>("GotBtn");
        _unreachGo = transform.Find<GameObject>("HaventBtn");
        _unBandGo = transform.Find<GameObject>("UnBand");

        _getBtn.onClick.SetListener(On_getBtnClick);
    }
    private void On_getBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        ActInfo_2091 actinfo = (ActInfo_2091)ActivityManager.Instance.GetActivityInfo(_aid);
        if (actinfo == null || _data == null)
            return;
        actinfo.GetMissionReward(_data.tid, OnGetMissionRewardCB);
    }
    private void OnGetMissionRewardCB(P_Act2091Mission data)
    {
        if (gameObject.activeSelf && data != null)
            RefrshButtonState(data);
    }

    public void Refresh(P_Act2091Mission data)
    {
        _data = data;
        cfg_act_2091_task task = Cfg.Activity2091.GetTaskData(_data.tid);

        _tittle.text = task.name;

        if (data.do_number >= task.need_count)
        {
            _progress.text = Lang.Get("（<Color=#00ff00ff>{0}</Color>/{1}）", data.do_number, task.need_count);
        }
        else
        {
            _progress.text = Lang.Get("（<Color=#ff0000ff>{0}</Color>/{1}）", data.do_number, task.need_count);
        }

        RefrshButtonState(data);

        P_Item[] rewards = data.ItemList;
        for (int i = 0; i < _rewardGoList.Length; i++)
        {
            GameObject go = _rewardGoList[i];

            if (i < rewards.Length)
            {
                go.SetActive(true);
                DefineReward(go.transform, rewards[i]);
            }
            else
            {
                go.SetActive(false);
            }
        }
    }

    private void DefineReward(Transform trans, P_Item item)
    {
        ItemForShow itemForShow = new ItemForShow(item.id, item.count);
        Image icon = trans.Find<Image>("img_icon");
        itemForShow.SetIcon(icon);
        trans.Find<Text>("Text").text = "x" + GLobal.NumFormat(itemForShow.GetCount());
        trans.Find<Image>("Img_qua").color = _ColorConfig.GetQuaColor(itemForShow.GetQua());

        icon.GetComponent<Button>().onClick.SetListener(() =>
        {
            AudioManager.Instace.PlaySoundOfNormalBtn();
            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(item.id, item.count, icon.transform.position); });
        });
    }

    private void RefrshButtonState(P_Act2091Mission data)
    {
        ActInfo_2091 config = (ActInfo_2091)ActivityManager.Instance.GetActivityInfo(_aid);
        if (config == null)
            return;

        _unBandGo.SetActive(false);

        if (data.get_reward == 1)
        {
            _getBtn.gameObject.SetActive(false);
            _unreachGo.SetActive(false);
            _getGo.SetActive(true);
        }
        else if (data.finished == 1)
        {
            _unreachGo.SetActive(false);
            _getGo.SetActive(false);

            if (!config.IsBand && config.InviteType != 3)
            {
                _unBandGo.SetActive(true);
                _getBtn.gameObject.SetActive(false);
            }
            else
            {
                _getBtn.gameObject.SetActive(true);
            }
        }
        else
        {
            _unreachGo.SetActive(true);
            _getGo.SetActive(false);
            _getBtn.gameObject.SetActive(false);
        }
    }
}
