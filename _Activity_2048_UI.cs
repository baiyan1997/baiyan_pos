using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2048_UI : ActivityUI
{
    private Text _leftTimeText;

    private Text _tittleText;

    private GameObject[] _dayGoList;

    private Transform[] _itemList;

    private Button _moreBtn;

    private Button _getBtn;

    private GameObject _haveGo;

    private Text _tipText;

    private ActInfo_2048 _activityInfo;
    private List<P_Item> _iteminfo;
    private int _showIndex;
    private Color _color = new Color(106f / 255, 169f / 255, 206f / 255);

    public override void OnCreate()
    {
        _leftTimeText = transform.Find<JDText>("Text_Time");

        _tittleText = transform.Find<JDText>("Title");

        _dayGoList = new GameObject[]
        {
             transform.Find<GameObject>("7day/01"),
             transform.Find<GameObject>("7day/02"),
             transform.Find<GameObject>("7day/03"),
             transform.Find<GameObject>("7day/04"),
             transform.Find<GameObject>("7day/05"),
             transform.Find<GameObject>("7day/06"),
             transform.Find<GameObject>("7day/07"),
         };

        _itemList = new Transform[]
        {
            transform.Find("01"),
            transform.Find("02"),
            transform.Find("03"),
        };

        _moreBtn = transform.Find<Button>("MoreBtn");

        _getBtn = transform.Find<Button>("GetBtn");

        _haveGo = transform.Find<GameObject>("HaveGet");

        _tipText = transform.Find<JDText>("Tip/Text");

        _iteminfo = new List<P_Item>();

        InitEvent();
        //InitListener();
    }

    private void InitEvent()
    {
        _moreBtn.onClick.SetListener(On_moreBtnClick);
        _getBtn.onClick.SetListener(On_getBtnClick); 
    }
    private void On_moreBtnClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

        _iteminfo = Cfg.Activity2048.GetRewardByDay(_showIndex + 1, _activityInfo.Step);

        Action action = _showIndex >= _activityInfo.Today || _activityInfo.StateList[_showIndex] ? null : (Action)OnClickGetReward;

        DialogManager.ShowAsyn<_D_ItemList>(d => { d?.OnShow(Lang.Get("所有奖励"), _iteminfo, action); });
    }
    private void On_getBtnClick()
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

        OnClickGetReward();
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        _activityInfo = (ActInfo_2048)ActivityManager.Instance.GetActivityInfo(2048);
        _showIndex = _activityInfo.Today - 1;

        UpdateUI(2048);
    }

    private Color32 _color2 = new Color32(255, 0, 255, 255);
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        _activityInfo = (ActInfo_2048)ActivityManager.Instance.GetActivityInfo(2048);

        if (_activityInfo == null)
            return;

        _tittleText.text = _activityInfo._name;

        for (int i = 0; i < 7; i++)
        {
            GameObject lockGo = _dayGoList[i].transform.Find<GameObject>("Lock");
            GameObject remindGo = _dayGoList[i].transform.Find<GameObject>("Remind");
            Image bg = _dayGoList[i].transform.Find<Image>("Bg");
            Button button = _dayGoList[i].GetComponentInChildren<Button>();
            Text daytext = _dayGoList[i].transform.Find<Text>("Text");

            daytext.text = string.Format(Lang.Get("第{0}天"), i + 1);
            int index = i;
            button.onClick.SetListener(() =>
            {
                AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

                ShowReward(index);

                DoButtonEffect();
            });

            int today = _activityInfo.Today;
            if (i + 1 <= today)
            {
                lockGo.SetActive(false);
                remindGo.SetActive(!_activityInfo.StateList[i]);

                if (i == _showIndex)
                {
                    UIHelper.SetImageSprite(bg,"Image/20190214_03");
                }
                else
                {
                    UIHelper.SetImageSprite(bg,"Image/20190214_02");
                }
                daytext.color = Color.white;
            }
            else
            {
                lockGo.SetActive(true);
                remindGo.SetActive(false);

                if (i == _showIndex)
                {
                    UIHelper.SetImageSprite(bg,"Image/20190214_03");
                    lockGo.GetComponent<Image>().color = _color2;
                    daytext.color = Color.white;
                }
                else
                {
                    UIHelper.SetImageSprite(bg,"Image/20190214_04");
                    daytext.color = _color;
                    lockGo.GetComponent<Image>().color = Color.white;
                }
            }
        }

        ShowReward(_showIndex);
    }

    private void ShowReward(int index)
    {
        _showIndex = index;

        if (_showIndex < _activityInfo.Today)
        {
            _getBtn.gameObject.SetActive(!_activityInfo.StateList[_showIndex]);
            _haveGo.SetActive(_activityInfo.StateList[_showIndex]);
            _tipText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            _tipText.text = string.Format(Lang.Get("第{0}天可领取奖励"), index + 1);

            _tipText.transform.parent.gameObject.SetActive(true);
            _getBtn.gameObject.SetActive(false);
            _haveGo.SetActive(false);
        }

        _iteminfo = Cfg.Activity2048.GetRewardByDay(_showIndex + 1, _activityInfo.Step);

        //展示前三个奖励
        for (int i = 0; i < 3; ++i)
        {
            Transform trans = _itemList[i];
            Image icon = trans.Find<Image>("Icon");
            Image qua = trans.Find<Image>("Qua");
            Text count = trans.Find<Text>("NubText");
            ItemForShow itemforShow = ItemForShow.Create(_iteminfo[i].id, _iteminfo[i].count);
            itemforShow.SetUI(icon, count, qua, true);

            Text name = trans.Find<Text>("Name");
            name.text = itemforShow.GetName();

            P_Item config = _iteminfo[i];

            trans.GetComponent<Button>().onClick.SetListener(() =>
            {
                AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

                DialogManager.ShowAsyn<_D_ItemTip>(d=>{ d?.OnShow(config.id, config.count, trans.position + new Vector3(0, 25, 0)); });
            });
        }
    }

    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (stamp - _activityInfo._data.startts < 0)
        {
            _leftTimeText.text = GlobalUtils.GetActivityStartTimeDesc(_activityInfo._data.startts);
        }
        else if (_activityInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_activityInfo.LeftTime);
            _leftTimeText.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _leftTimeText.text = Lang.Get("活动已经结束");
        }
    }

    private void OnClickGetReward()
    {
        if (_activityInfo != null)
            _activityInfo.RequestRewards(null);
    }

    private void DoButtonEffect()
    {
        for (int i = 0; i < 7; i++)
        {
            Image bg = _dayGoList[i].transform.Find<Image>("Bg");
            Image lockimg = _dayGoList[i].transform.Find<Image>("Lock");
            Text text = _dayGoList[i].transform.Find<Text>("Text");

            if (i == _showIndex)
            {
                UIHelper.SetImageSprite(bg,"Image/20190214_03");
                lockimg.color = _color2;
                text.color = Color.white;
            }
            else
            {
                if (i + 1 <= _activityInfo.Today)
                {
                    UIHelper.SetImageSprite(bg,"Image/20190214_02");
                }
                else
                {
                    UIHelper.SetImageSprite(bg,"Image/20190214_04");
                    text.color = _color;
                    lockimg.color = Color.white;
                }
            }
        }
    }
}
