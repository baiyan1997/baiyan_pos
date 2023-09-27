using System;
using UnityEngine;
using UnityEngine.UI;

public class _Act2058Item : ListItem
{
    private Button _btnGet;
    private GameObject _objAlreadyGet;
    private JDText _txtDay;
    private JDText _txtGet;
    private GameObject _getImg;
    private Image _bg;
    private Image[] _rewardIcons;
    private Image[] _rewardQua;
    private Text[] _rewardCount;
    private GameObject[] _rewardMask;
    private const int MAX_REWARD_COUNT = 2;
    private Color32 LockAlpha = new Color32(255,255,255,150);
    private Color32 UnLockAlpha = new Color32(255,255,255,255);
    private Color32 TxtLockAlpha = new Color32(0,255,255,84);
    private Color32 TxtUnLockAlpha = new Color32(0, 255, 255, 255);
    private int _doNum;//已经签到的天数
    private ActInfo_2058 _actInfo_2058;
    private cfg_act_2058 _info;
    public override void OnCreate()
    {
        _bg = transform.Find<Image>("Bg");
        _txtDay = transform.Find<JDText>("Text_Day");
        _getImg = transform.Find<GameObject>("getImg");
        _txtGet = transform.Find<JDText>("getImg/Text");
        _btnGet = transform.Find<Button>("BtnGet");
        _objAlreadyGet = transform.Find<GameObject>("BtnAlreadyGet");
        _rewardIcons = new[]
        {
           transform.FindImage("01/Icon"),
           transform.FindImage("02/Icon"),
        };
        _rewardQua = new[]
        {
           transform.FindImage("01/Qua"),
           transform.FindImage("02/Qua"),
        };
        _rewardCount = new[]
        {
           transform.FindText("01/Text"),
           transform.FindText("02/Text"),
        };
        _rewardMask = new[]
        {
            transform.Find<GameObject>("01/Mask"),
            transform.Find<GameObject>("02/Mask"),
        };
        _txtGet.text = Lang.Get("已领取");
        _btnGet.gameObject.SetActive(false);
        _objAlreadyGet.SetActive(false);
        //_btnGet.onClick.AddListener(()=> {
        //    //领取每日的奖励
        //    _actInfo_2058.GetAct2058Reward(_info.day);
        //});
        _btnGet.onClick.AddListener(On_btnGetClick);
    }
    private void On_btnGetClick()
    {
        _actInfo_2058.GetAct2058Reward(_info.day);
    }
    public void Refresh(cfg_act_2058 info, ActInfo_2058 actInfo_2058)
    {
        _info = info;
        _actInfo_2058 = actInfo_2058;
        _doNum = actInfo_2058._dayCount;
        //刷新按钮状态
        UpdateUI();
    }
    private void UpdateUI()
    {
        _txtDay.text = Lang.Get("{0}", _info.day);
        var items = GlobalUtils.ParseItem(_info.reward);
        //刷新奖励
        for (int i = 0, max = MAX_REWARD_COUNT; i < max; i++)
        {
            var item = items[i];
            var showItem = ItemForShow.Create(item.id, item.count);
             showItem.SetIcon(_rewardIcons[i]);
            _rewardQua[i].color = _ColorConfig.GetQuaColorHSV(showItem.GetQua());
            _rewardCount[i].text = "x" + GLobal.NumFormat(showItem.GetCount());
            //添加道具描述
            var i1 = i;
            _rewardIcons[i].GetComponent<Button>().onClick.SetListener(() =>
            {
                DialogManager.ShowAsyn<_D_ItemTip>(d=>{ d?.OnShow(item.id, item.count, _rewardIcons[i1].transform.position); });
            });
        }
        RefreshState();
    }
    //刷新状态，已领取和未领取
    private void RefreshState()
    {
        if (_info.day > _doNum)
        {
            //还未到达签到时间
            for (int i = 0; i < MAX_REWARD_COUNT; i++)
            {
                _rewardMask[i].SetActive(true);
            }
            _bg.color = LockAlpha;
            _txtDay.color = TxtLockAlpha;
            _getImg.SetActive(false);

            _objAlreadyGet.SetActive(false);
            _btnGet.gameObject.SetActive(false);
        }
        else
        {
            //已经到达时间
            for (int i = 0; i < MAX_REWARD_COUNT; i++)
            {
                _rewardMask[i].SetActive(false);
            }
            _bg.color = UnLockAlpha;
            _txtDay.color = TxtUnLockAlpha;
            if (_actInfo_2058.IsGetDayReward(_info.day))
            {
                //已经领取
                _getImg.SetActive(true);
                _objAlreadyGet.SetActive(true);
                _btnGet.gameObject.SetActive(false);
            }
            else
            {
                //未领取
                _getImg.SetActive(false);
                _objAlreadyGet.SetActive(false);
                _btnGet.gameObject.SetActive(true);
            }
        }
       
    }
}
