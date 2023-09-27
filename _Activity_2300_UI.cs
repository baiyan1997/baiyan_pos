using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class _Activity_2300_UI : ActivityUI
{
    private int _aid = 2300;
    private ActInfo_2300 _actInfo;

    private Text _title;//标题
    private Text _time;//倒计时

    private TabBtnHelper _tabs;//一般是3个切页

    private ListView _missionList;

    private GameObject _defaultBanner;
    private Image _banner;
    private Sprite _bannerSprite;

    //切页到计时
    private Text _pageTime;
    private GameObject _pageTimeObj;
    private int pageRefreshTs;//每日奖励倒计时时间戳
    private string _pageName;//每日奖励倒计时时间戳

    private Button _btnDesc;//活动描述 服务器传过来的 desc字段
    public override void OnCreate()
    {
        _title = transform.Find<Text>("Text_title");
        _time = transform.Find<Text>("Text_time");
        _pageTime = transform.Find<Text>("Time/Text");
        _pageTimeObj = transform.Find("Time").gameObject;
        _btnDesc = transform.Find<Button>("Btn_desc");

        _defaultBanner = transform.Find("Banner/Empty").gameObject;
        _banner = transform.Find<Image>("Banner/Img_banner");

        var tabRoot = transform.Find<RectTransform>("Menu");
        _tabs = new TabBtnHelper(tabRoot, tabRoot.GetChild(0).gameObject);


        _missionList = ListView.Create<_Act2300Item>(transform.Find("Scroll View"));

        _tabs.OnTabSwitch += SwichToPage;
        //TimeManager.Instance.TimePassSecond += RefreshTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);


        _btnDesc.onClick.AddListener(On_btnDescClick);
    }
    private void On_btnDescClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_btnDescDialogShowAsynCB);
    }
    private void On_btnDescDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(_actInfo._name, _actInfo._desc, _btnDesc.transform.position, Direction.LeftDown, 350, new Vector2(-25, -25));
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_tabs != null)
        {
            _tabs.OnDestroy();
            _tabs = null;
        }
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _aid)
            return;

        if (gameObject == null || !gameObject.activeInHierarchy)
            return;

        OnShow();
    }

    private void SwichToPage(int oldPage, int newPageId)
    {
        _missionList.Clear();
        var page = _actInfo.GetMissionsByPage(newPageId);
        if (page == null)
            return;

        var missions = page.page_missions;
        for (int i = 0; i < missions.Count; i++)
        {
            _missionList.AddItem<_Act2300Item>().Refresh(missions[i]);
        }


        pageRefreshTs = page.page_refresh_ts;
        _pageName = page.name;
        _pageTimeObj.SetActive(pageRefreshTs != 0);
        UpdateTime(TimeManager.ServerTimestamp);
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2300)ActivityManager.Instance.GetActivityInfo(_aid);

        _title.text = _actInfo._name;
        UpdateTime(TimeManager.ServerTimestamp);

        _tabs.Clear();
        var pages = _actInfo.mission_pages;
        for (int i = 0; i < pages.Count; i++)
        {
            _tabs.AddTab<TabBtn>(pages[i].pageid).Refresh(pages[i], _actInfo);
        }
        _tabs.Finish();

        UpdateTime(TimeManager.ServerTimestamp);


        _defaultBanner.SetActive(false);
        _banner.gameObject.SetActive(false);

        if (_bannerSprite != null)
        {
            RefreshBannerImg(_bannerSprite);
        }
        else
        {
            LoadSprite(_actInfo);
        }
    }

    private void LoadSprite(ActInfo_2300 _actInfo)
    {
        var localFullPath = _actInfo.GetBannerFullPath();//本地路径
        var sprite = LoadHelper.LoadSpriteFromLocal(localFullPath);//本地加载
        if (sprite != null)
        {
            RefreshBannerImg(sprite);
        }
        else//网络加载
            LoadHelper.LoadImgFromUrl(gameObject, _actInfo._data.bg_url, localFullPath, RefreshBannerImg);
    }

    private void RefreshBannerImg(Sprite sprite)
    {
        _bannerSprite = sprite;
        if (sprite == null)
        {
            _defaultBanner.SetActive(true);
            _banner.gameObject.SetActive(false);
        }
        else
        {
            _defaultBanner.SetActive(false);
            _banner.gameObject.SetActive(true);
            _banner.sprite = sprite;
        }
    }
    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (nowTs - _actInfo._data.startts < 0)
        {
            _time.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            _time.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _time.text = Lang.Get("活动已经结束");
        }


        if (pageRefreshTs == 0)
            _pageTime.text = string.Empty;
        else
        {
            _pageTime.text = Lang.Get("{0}刷新倒计时 {1}", _pageName, WorldUtils.getLastTime_DHMS(pageRefreshTs));
        }
    }
    public class TabBtn : TabBtnBase
    {
        private Text _tabName;
        private GameObject _redPoint;

        public override void Awake()
        {
            base.Awake();
            _tabName = transform.Find<Text>("Text");
            _redPoint = transform.Find("Remind").gameObject;
        }

        public override void Select()
        {
            _tabName.color = Color.white;
            UIHelper.SetImageSprite(GetButton().image, "Button/btn_801");
        }
        public override void Unselect()
        {
            _tabName.color = new Color(126 / 255f, 229 / 255f, 1);
            UIHelper.SetImageSprite(GetButton().image, "Button/btn_802");
        }

        public void Refresh(P_PageM pPageM, ActInfo_2300 info)
        {
            _tabName.text = Lang.Get(pPageM.name);
            bool red = info.IsCanGet(pPageM.page_missions);
            _redPoint.gameObject.SetActive(red);
        }
    }
    public class _Act2300Item : ListItem
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

        public void Refresh(P_OneM info)
        {
            if (info.need_count > 0)
            {
                _descTxt.text = info.name;
                if (info.do_number < info.need_count)
                    _countTxt.text = string.Format("(<color=#ff004d>{0}</color>/{1})", GLobal.NumFormat(info.do_number), GLobal.NumFormat(info.need_count));
                else
                    _countTxt.text = string.Format("(<color=#00ff33>{0}</color>/{1})", GLobal.NumFormat(info.do_number), GLobal.NumFormat(info.need_count));
            }
            else
            {
                _descTxt.text = info.name;
                _countTxt.text = "";
            }


            var items = GlobalUtils.ParseItem(info.reward);
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
                SetButtonState(info);
            }
        }
        private void SetButtonState(P_OneM info)
        {
            if (info.finished == 0) //未完成
            {
                if (string.IsNullOrEmpty(info.click))
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
                        MissionUtils.DoCustomFlow(info.click);
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
                btn.onClick.SetListener(() =>
                {
                    var actInfo = (ActInfo_2300)ActivityManager.Instance.GetActivityInfo(2300);
                    if (actInfo != null)
                    {
                        Uinfo.Instance.Bag.CheckBlueDrawAlert(info.reward, () =>
                        {
                            actInfo.RequestGetReward(info);
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
}
