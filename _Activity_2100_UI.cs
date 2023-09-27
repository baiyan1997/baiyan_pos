using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Random = System.Random;

public class _Activity_2100_UI : ActivityUI
{
    private Text _des;
    private ListView _rewardList;
    private ActInfo_2100 _actInfo;
    private int _aid = 2100;
    private BtnTab2100[] _tabBtns = new BtnTab2100[3];
    private int currentSelectType = 0;
    private Button helpBtn = null;
    public static long clickShareTime = -1;
    public override void OnCreate()
    {
        _des = transform.Find<Text>("des_text");
        helpBtn = transform.Find<Button>("FQA_Btn");

        _rewardList = ListView.Create<_Act2100Item>(transform.Find("Scroll View"));
        for (int i = 0; i < 3; i++)
        {
            var tabGo = transform.Find("Menu/Button" + (i + 1)).gameObject;
            var tabBtn = tabGo.AddBehaviour<BtnTab2100>();
            var btn = tabGo.GetComponent<Button>();
            _tabBtns[i] = tabBtn;
            var index = i;
            btn.onClick.AddListener(() =>
            {
                OnClickTab(index);
            });
        }

        helpBtn.onClick.AddListener(OnClickHelp);
        //
    }

    private void OnClickHelp()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(ShowTipsHelp);
    }

    private void ShowTipsHelp(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2100, helpBtn.transform.position, Direction.RightDown, 323);
    }

    private void OnClickTab(int selectType)
    {
        if (_actInfo == null || _actInfo.ActDeetail == null)
        {
            return;
        }
        if (currentSelectType == selectType)
        {
            return;
        }
        currentSelectType = selectType;
        for (int i = 0; i < 3; i++)
        {
            var tabBtn = _tabBtns[i];
            if (i == selectType)
            {
                tabBtn.Select();

            }
            else
            {
                tabBtn.Unselect();
            }
        }
        UpdateUi();
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2100)ActivityManager.Instance.GetActivityInfo(_aid);
        _actInfo.GetActDetil();
        //���Դ���
        //var detail = new P_Act2100Detail();
        //_actInfo.ActDeetail = detail;
        //var missionInfos = new List<P_Act2100MissionInfo>();
        //var cfgs = Cfg.ShareReward.GetRewardConfig();
        //var len = cfgs.Count;
        //for (int i = 0; i < len; i++)
        //{
        //    var cfg = cfgs[i];
        //    var missionInfo = new P_Act2100MissionInfo();
        //    missionInfo.Id = cfg.id;
        //    Random r = new Random();
        //    //missionInfo.Progess = r.Next(0, cfg.level);
        //    missionInfo.Progess = 0;
        //    missionInfo.state = i < 3 ? 1 : 0;
        //    missionInfos.Add(missionInfo);
        //}
        //detail.datas = missionInfos.ToArray();
        //EventCenter.Instance.Act2100DetailBack.Broadcast();
    }
    private void Init()
    {
        _des.text = string.Empty;
        //_des.text = string.Format(Lang.Get("�����ʱ ") + GlobalUtils.ActTimeFormat(_actInfo._data.startts, _actInfo._data.endts));
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.Act2100DetailBack.AddListener(OnDetailBack);
        EventCenter.Instance.Act2100ShareSucceed.AddListener(OnShareSucceed);
        MainEventCenter.Instance.OnApplicationFocus.AddListener(OnFocus);
    }

    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.Act2100DetailBack.RemoveListener(OnDetailBack);
        EventCenter.Instance.Act2100ShareSucceed.RemoveListener(OnShareSucceed);
        MainEventCenter.Instance.OnApplicationFocus.RemoveListener(OnFocus);
    }

    public override void OnShow()
    {
        InitData();
        Init();
    }

    private void OnDetailBack()
    {
        UpdateUi();
    }

    private void OnShareSucceed(int result)
    {
        if (result == 1 && _actInfo != null)
        {
            _actInfo.GetActDetil();
        }
    }

    private void OnFocus(bool bFocus)
    {
        if(bFocus) { //回到前台
            if(_Activity_2100_UI.clickShareTime > 0) {
                if(Util.GetTimeStampMilliSecond() - _Activity_2100_UI.clickShareTime >= 2000) {
                    var actInfo = (ActInfo_2100)ActivityManager.Instance.GetActivityInfo(ActivityID.Share);
                    if(actInfo != null) {
                        actInfo.SendShareSucceed();
                    }
                }else {
                    PlatformSdk.GetInstance().ShowModal("提示", "检测分享失败，请分享到不同的群", "确定");
                }
            }
            _Activity_2100_UI.clickShareTime = -1;
        }
    }

    private void UpdateUi()
    {
        _rewardList.Clear();
        var rewardCfgs = Cfg.ShareReward.GetRewardByType(currentSelectType + 1);
        List<P_Act2100MissionInfo> missionInfos = ListPool<P_Act2100MissionInfo>.Get();
        var len = rewardCfgs.Count;
        for (int i = 0; i < len; i++)
        {
            var cfg = rewardCfgs[i];
            // PlatformWrap.Log("cfg---id = " + cfg.id + ",type = " + cfg.type + ",level = " + cfg.level + 
            //     ",reward = " + cfg.reward + ",title = " + cfg.title);
            missionInfos.Add(_actInfo.GetMissionInfo(cfg.id));
        }
        missionInfos.Sort(Sort_mission);
        len = missionInfos.Count;
        for (int i = 0; i < len; i++)
        {
            var info = missionInfos[i];
            _rewardList.AddItem<_Act2100Item>().Refresh(Cfg.ShareReward.GetRewardByID(info.id), info);
        }
    }
    private static int Sort_mission(P_Act2100MissionInfo a, P_Act2100MissionInfo b)
    {
        return a.state - b.state;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        helpBtn.onClick.RemoveListener(OnClickHelp);
        for (int i = 0; i < 3; i++)
        {
            _tabBtns[i] = null;
        }
        _tabBtns = null;
        _actInfo = null;
        _rewardList = null;
    }
}

public class BtnTab2100 : TabBtnBase//TabButton
{
    private ObjectGroup _objGroup;
    public Sprite[] _sprite;
    private Button _button = null;
    private Text _text = null;
    public override void Awake()
    {
        _objGroup = transform.parent.GetComponent<ObjectGroup>();
        _sprite = new[]
        {
            _objGroup.Sprite("BtnUnSelected"),
            _objGroup.Sprite("BtnSelected"),
        };
        _button = transform.GetComponent<Button>();
        _text = transform.GetComponentInChildren<Text>();
    }
    public override void Select()
    {
        _text.color = new Color(236 / 255f, 1f, 1f);
        _button.image.sprite = _sprite[1];
    }
    public override void Unselect()
    {
        _text.color = new Color(153f / 255f, 209f / 255f, 236f / 255f);
        _button.image.sprite = _sprite[0];
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _text = null;
        _button = null;
        _objGroup = null;
        _sprite = null;
    }
}

public class _Act2100Item : ListItem
{
    //~_Act2100Item()
    //{
    //    Debug.LogError("~_Act2100Item");
    //}
    private cfg_share_reward rewardConfig;
    private Text _titleTxt;
    private Text _progressTxt;
    private ListView _list;
    private Button _getBtn;
    private GameObject _get;
    private Button _shareBtn;
    private GameObject _shareGo = null;
    private GameObject unGetBgGo = null;
    private GameObject getedBgGo = null;
    private GameObject getedTxtGo = null;
    private Text shareText = null;

    public override void OnCreate()
    {
        _progressTxt = transform.Find<Text>("Progress_Txt");
        _titleTxt = transform.Find<Text>("Title_Txt");
        unGetBgGo = transform.Find("unget_Bg").gameObject;
        getedBgGo = transform.Find("geted_Bg").gameObject;
        getedTxtGo = transform.Find("geted_txt").gameObject;
        _list = ListView.Create<_ActRewardItem>(transform.Find("ScrollView"));
        _getBtn = transform.FindButton("Btn_get");
        _get = _getBtn.gameObject;
        _shareBtn = transform.FindButton("Btn_Share");
        _shareGo = _shareBtn.gameObject;
        shareText = transform.FindText("Btn_Share/Text");
        _getBtn.onClick.AddListener(OnClickGetBtn);

        _shareBtn.onClick.AddListener(OnClickShareBtn);
    }
    private void OnClickGetBtn()
    {
        var actInfo = (ActInfo_2100)ActivityManager.Instance.GetActivityInfo(ActivityID.Share);
        actInfo.GetReward(rewardConfig.id);
    }

    private void OnClickShareBtn()
    {
        string sImgPath = SimpleShare.GetRandomShareImg();
        if (rewardConfig.type == 1)
        {
            PlatformSdk.GetInstance().ShareAppMessage(sImgPath,
                "ZtlRTaWWT7GISe/Al8HpXA==",
                string.Empty,
                string.Empty,
                "养成策略游戏，一起来征服宇宙星际"
                );
            _Activity_2100_UI.clickShareTime = Util.GetTimeStampMilliSecond();
            if(!WXUtil.isWechatGame) { //不是微信
                _Activity_2100_UI.clickShareTime = -1;
                var actInfo = (ActInfo_2100)ActivityManager.Instance.GetActivityInfo(ActivityID.Share);
                actInfo.SendShareSucceed();
            }
        }
        else if (rewardConfig.type == 2)
        {
            PlatformSdk.GetInstance().ShareAppMessage(sImgPath,
                 "ZtlRTaWWT7GISe/Al8HpXA==",
                 string.Empty,
                 "invitation_uid=" + User.Uid.ToString() + "&" + "invitation_sid=" + User.Server.index,
                 "上百舰船，续写史诗星际战争"
                 );
        }
    }

    public void Refresh(cfg_share_reward rewardCfg, P_Act2100MissionInfo missionInfo)
    {
        rewardConfig = rewardCfg;
        _titleTxt.text = string.Format(rewardCfg.title, rewardCfg.level, rewardCfg.condition);
        if(missionInfo.state != 1)
        {
            _progressTxt.text = string.Format("{0}/{1}", missionInfo.progess, rewardCfg.level);
        }
        else
        {
            int nLevel = (missionInfo.progess >= rewardCfg.level) ? missionInfo.progess : rewardCfg.level;
            _progressTxt.text = string.Format("{0}/{1}", nLevel, rewardCfg.level);
        }
        shareText.text = rewardCfg.type == 1 ? "分享" : "邀请";
        var canGet = missionInfo.progess >= rewardCfg.level && missionInfo.state == 0;
        _get.SetActive(canGet);
        _shareGo.SetActive(!canGet && missionInfo.state == 0);
        unGetBgGo.SetActive(missionInfo.state == 0);
        getedBgGo.SetActive(missionInfo.state == 1);
        getedTxtGo.SetActive(missionInfo.state == 1);
        if (rewardCfg.type == 3)
        {
            _shareGo.SetActive(false);
        }
        _list.Clear();
        var rewardItems = GlobalUtils.ParseItem3(rewardCfg.reward);
        var len = rewardItems.Length;
        for (int i = 0; i < len; i++)
        {
            _list.AddItem<_ActRewardItem>().Refresh(rewardItems[i]);
        }

    }

    public override void OnAddToList()
    {
        base.OnAddToList();
    }
    public override void OnRemoveFromList()
    {
        base.OnRemoveFromList();
        rewardConfig = null;
    }
}
