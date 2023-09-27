using System;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2403_UI : ActivityUI
{
    private Button _btnHelp;
    private Button _btnBuy;
    private Button _btnGet;
    private GameObject _itemTemplate;
    private Transform _contentDaliy;
    private Transform _contentBuy;
    private GameObject _imgDaliyGet;
    private GameObject _imgBuyGet;
    private ActInfo_2403 _actInfo;
    public override void OnCreate()
    {
        InitRef();
        InitButton();
    }

    public void InitRef()
    {
        Transform kuang = transform.Find("Kuang");
        _btnHelp = transform.Find<Button>("Helpbtn");
        _btnBuy = transform.Find<Button>("BtnBuy");
        _btnGet = transform.Find<Button>("BtnGet");
        _itemTemplate = transform.Find<GameObject>("Icon_Item");
        _contentDaliy = kuang.Find("ContentDaliy");
        _contentBuy = kuang.Find("ContentBuy");
        _imgDaliyGet = kuang.Find<GameObject>("Image_daliyGet");
        _imgBuyGet = kuang.Find<GameObject>("Image_buyGet");
        _actInfo = (ActInfo_2403)ActivityManager.Instance.GetActivityInfo(ActivityID.SpanOrder);
    }

    public void InitButton()
    {
        _btnHelp.onClick.AddListener(OnClickHelp);
        _btnBuy.onClick.AddListener(() =>
        {
            _actInfo.BuyAct();
        });
        _btnGet.onClick.AddListener(_OnBtnGet);
    }

    private void _OnBtnGet()
    {
        if(_actInfo.get_buy_reward == 0)
        {
            if(_actInfo.IsBuyServer())
            {
                _actInfo.GetRewrad(1, _ChangeToAreadyGet);
                _actInfo.GetRewrad(2);
            }
            else
            {
                _actInfo.GetRewrad(2, _ChangeToAreadyGet);
            }
        }
        else if(_actInfo.get_buy_reward > 0)
        {
            _actInfo.GetRewrad(2, _ChangeToAreadyGet);
        }
    }

    private void _ChangeToAreadyGet()
    {
        _btnGet.transform.Find<Text>("Text").text = "已领取";
        UIHelper.SetButtonInteractable(_btnGet, false);
    }

    // public override void InitListener()
    // {
    //     base.InitListener();
    // }
    private void OnClickHelp()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(ShowTipsHelp);
    }

    private void ShowTipsHelp(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2403, _btnHelp.transform.position, Direction.RightDown, 323);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == ActivityID.SpanOrder)
        {
            Refresh();
        }
    }

    public override void OnShow()
    {
        _InitReward();
        Refresh();
    }

    private void _InitReward()
    {
        _contentBuy.DestroyChildren();
        _contentDaliy.DestroyChildren();
        //购买奖励
        P_Item[] rewards = GlobalUtils.ParseItem(Cfg.Activity2403.GetRewardById(1));
        if(rewards != null)
        {
            for(int i=0;i<rewards.Length;++i)
            {
                GameObject item = GameObject.Instantiate(_itemTemplate);
                item.SetActive(true);
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;
                item.transform.SetParent(_contentBuy, false);
                item.AddComponent<SimpleItemShowMono>().ShowReward(rewards[i]);
            }
        }
        //每日奖励
        P_Item[] rewards2 = GlobalUtils.ParseItem(Cfg.Activity2403.GetRewardById(2));
        if(rewards2 != null)
        {
            for(int i=0;i<rewards2.Length;++i)
            {
                GameObject item = GameObject.Instantiate(_itemTemplate);
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = Vector3.one;
                item.SetActive(true);
                item.transform.SetParent(_contentDaliy, false);
                item.AddComponent<SimpleItemShowMono>().ShowReward(rewards2[i]);
            }
        }
    }

    private void Refresh()
    {
        if(_actInfo.get_buy_reward < 0 && _actInfo.last_daliy_reward < 0) //此时还没购买
        {
            _imgDaliyGet.SetActive(false);
            _imgBuyGet.SetActive(false);
            _btnBuy.gameObject.SetActive(true);
            _btnGet.gameObject.SetActive(false);
        }
        else if(_actInfo.get_buy_reward >= 0) //此时已经购买
        {
            bool bIsAvaliable = _actInfo.IsCanGet();
            _imgBuyGet.SetActive(_actInfo.get_buy_reward > 0 || !_actInfo.IsBuyServer());
            _imgDaliyGet.SetActive(!bIsAvaliable);
            _btnBuy.gameObject.SetActive(false);
            _btnGet.gameObject.SetActive(true);
            _btnGet.transform.Find("Image").gameObject.SetActive(bIsAvaliable);
            if(bIsAvaliable)
            {
                _btnGet.transform.Find<Text>("Text").text = "领取";
            }
            else
            {
                _btnGet.transform.Find<Text>("Text").text = "已领取";
            }
            UIHelper.SetButtonInteractable(_btnGet, bIsAvaliable);
        }
    }

    // public override void UpdateTime(long currentTime)
    // {
    //     base.UpdateTime(currentTime);
    //     if (gameObject == null || !gameObject.activeInHierarchy)
    //         return;
    //     if (_actInfo == null)
    //         return;

    //     if (_actInfo.LeftTime >= 0)
    //     {
    //         _timeText.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
    //     }
    //     else
    //     {
    //         _timeText.text = Lang.Get("活动已经结束");
    //     }
    // }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _actInfo = null;
    }
}

public class SimpleItemShowMono : MonoBehaviour
{
    private Image _icon;
    private Image _qua;
    private Text _textNum;
    private int _rewardId;

    void Awake()
    {
        InitRef();
    }

    private void InitRef()
    {
        _icon = transform.Find<Image>("img_icon");
        _qua = transform.Find<Image>("Img_qua");
        _textNum = transform.Find<Text>("Text");

        _qua.GetComponent<Button>().onClick.AddListener(() =>
        {
            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(_rewardId, 1, _qua.transform.position); });
        });
    }

    public void ShowReward(P_Item item)
    {
        if(item == null)
        {
            return;
        }
        _rewardId = item.Id;
        Cfg.Item.SetItemIcon(_icon, _rewardId);
        _qua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(_rewardId));
        _textNum.text = item.Num.ToString();
    }
}