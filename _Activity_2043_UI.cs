using System;
using UnityEngine.UI;
using UnityEngine;

public class _Activity_2043_UI : ActivityUI
{
    private Text _actTime;
    private Text _des;
    private Button[] _menuBtns;
    private Act2043Item[] _rewardItems;

    private Button _btnBuy;
    private Text _textGold;

    private Button _btnGet;

    private Text _textReceived;

    private ActInfo_2043 _info;
    private int _selectPage;


    public override void OnCreate()
    {
        _actTime = transform.FindText("Text_Time");
        _des = transform.FindText("Text_Desc");
        _menuBtns = new[]
        {
            transform.FindButton("Menu/Btn_Tab1"),
            transform.FindButton("Menu/Btn_Tab2"),
            transform.FindButton("Menu/Btn_Tab3"),
        };

        _rewardItems = new []
        {
            new Act2043Item(transform.Find("Main/01")),
            new Act2043Item(transform.Find("Main/02")),
            new Act2043Item(transform.Find("Main/03")),
            new Act2043Item(transform.Find("Main/04")),
            new Act2043Item(transform.Find("Main/05")),
        };

        _btnBuy = transform.FindButton("Main/Btn_Buy");
        _textGold = transform.FindText("Main/Btn_Buy/Text_Gold");
        _btnGet = transform.FindButton("Main/Btn_Get");
        _textReceived = transform.FindText("Main/Text_Recevied");

        InitData();
        InitEvent();
        //InitListener();
        InitUI();
    }

    public override void OnShow()
    {
        if (_selectPage > 0)
            OnSelectPage(_selectPage);
        else
            OnSelectPage(1);
    }

    private void InitData()
    {
        _info = (ActInfo_2043)ActivityManager.Instance.GetActivityInfo(2043);
    }

    private void InitEvent()
    {
        for (int i = 0; i < _menuBtns.Length; i++)
        {
            var page = i + 1;
            _menuBtns[i].onClick.AddListener(() =>
            {
                OnSelectPage(page);
            });
        }
    }

    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUI);
    }

    private void InitUI()
    {
        _des.text = _info._desc;

        _btnBuy.onClick.SetListener(On_btnBuyClick);
        _btnGet.onClick.SetListener(On_btnGetClick);
    }
    private void On_btnBuyClick()
    {
        var arr = _info.GetInfoByPage(_selectPage);
        var sample = arr[0];

        var itemGoldEnough = ItemHelper.IsCountEnough(ItemId.Gold, sample.gold_level);
        if (itemGoldEnough)
        {
            string str = "购买";
            var alert = Alert.YesNo(Lang.Get("是否花费{0}氪晶{1}？", sample.gold_level, str));
            alert.SetYesCallback(() =>
            {
                alert.Close();
                _info.RequestBuyBox(_selectPage);
            });
        }
    }
    private void On_btnGetClick()
    {
        _info.RequestGetReward(_selectPage);
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
        for (int i = 0; i < 3; i++)
        {
            GameObject go = _menuBtns[i].gameObject;
            if (go.activeSelf)
            {
                if (i + 1 == page)
                {
                    go.GetComponentInChildren<Text>().color = Color.white;
                    UIHelper.SetImageSprite(go.GetComponent<Image>(),"btn_12");
                }
                else
                {
                    go.GetComponentInChildren<Text>().color = new Color(126/255f, 229/255f, 1f);
                    UIHelper.SetImageSprite(go.GetComponent<Image>(),"btn_13");
                }

                var remindPoint = go.transform.Find("Remind").gameObject;
                remindPoint.SetActive(_info.IsCanGetPage(i+1));
            }
        }

        var infoForPage = _info.GetInfoByPage(page);
        JDDebug.Dump(infoForPage);
        var sample = infoForPage[0];
        _textGold.text = sample.gold_level.ToString();
        for (int i = 0; i < infoForPage.Length; i++)
        {
            _rewardItems[i].Refresh(infoForPage[i]);
        }
        if (sample.today <= 0)
        {
            //未购买
            _btnBuy.gameObject.SetActive(true);
            _btnGet.gameObject.SetActive(false);
            _textReceived.gameObject.SetActive(false);
        }
        else
        {
            _btnBuy.gameObject.SetActive(false);
            var canGet = _info.IsCanGetPage(page);
            _btnGet.gameObject.SetActive(canGet);
            _textReceived.gameObject.SetActive(!canGet);
        }
    }
}

public class Act2043Item
{
    private Transform transform;
    private Image _imgIcon;
    private Button _btnIcon;
    private Image _imgQua;
    private Text _textCount;
    private Text _textDay;
    private GameObject _goTip;


    public Act2043Item(Transform root)
    {
        transform = root;
        _imgIcon = transform.Find<Image>("Img_Icon");
        _btnIcon = transform.FindButton("Img_Icon");
        _imgQua = transform.Find<Image>("Img_Qua");
        _textCount = transform.Find<Text>("Text_Count");
        _textDay = transform.Find<Text>("Text_Day");
        _goTip = transform.Find<GameObject>("Img_Tip");

    }


    public void Refresh(P_Act2043Info info)
    {
        if (info == null)
        {
            transform.gameObject.SetActive(false);
            return;
        }
        transform.gameObject.SetActive(true);
        var arr = info.reward.Split('|');
        int id = Convert.ToInt32(arr[0]);
        int count = Convert.ToInt32(arr[1]);
        var itemForShow = ItemForShow.Create(id,count);
        itemForShow.SetIcon(_imgIcon);
        _imgQua.color = _ColorConfig.GetQuaColorHSV(itemForShow.GetQua());
        _textCount.text = "x" + GLobal.NumFormat(itemForShow.GetCount()); 
        _textDay.text = string.Format(Lang.Get("第{0}天"), info.day);
        _goTip.SetActive(info.get_reward == 1);

        _btnIcon.onClick.SetListener(() =>
        {
            ItemHelper.ShowTip(id, count, _btnIcon.transform);
        });
    }
}