using UnityEngine;
using UnityEngine.UI;

public class _Act2003Item : ListItem
{
    private Text _textDesc;
    private GameObject _btnForward;
    private GameObject _btnGet;
    private GameObject _btnOut;
    private GameObject _tipClaimed;
    private RectTransform _transformBg;
    private RectTransform rectTransform;
    private string _programIcon;
    private int[] _bgHeightSize = new[]
    {
        128,
        164
    };

    private int _descLineHeight = 25;

    private GameObject[] _rewards;

    public override void OnCreate()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        _transformBg = transform.Find<RectTransform>("Image");
        _textDesc = transform.Find<Text>("Image/Text_Desc");
        _btnForward = transform.Find("Image/Btn_Forward").gameObject;
        _btnGet = transform.Find("Image/Btn_Get").gameObject;
        _btnOut = transform.Find("Image/Btn_Out").gameObject;
        _tipClaimed = transform.Find("Image/Tip_Claimed").gameObject;
        _rewards = new[]
        {
            transform.Find("Image/Reward_1").gameObject,
            transform.Find("Image/Reward_2").gameObject,
            transform.Find("Image/Reward_3").gameObject,
            transform.Find("Image/Reward_4").gameObject
        };
        _programIcon = "Item/20445";
    }

    public void UpdateUI(cfg_act_2003 cfg,P_Act2003Mission info)
    {
        if (!_textDesc)
            return;
        if (info != null)
        {
            if (cfg.need_count > 0)
            {
                int num = info.do_number;
                //加速道具特殊处理
                if (cfg.name.Contains("加速道具"))
                {
                    num = info.do_number / 60;
                }
                _textDesc.text = cfg.name + string.Format("({0}/{1})", GLobal.NumFormat(num), GLobal.NumFormat(cfg.need_count));
            }
            else
                _textDesc.text = cfg.name;
        }
        else
        {
            _textDesc.text = cfg.name;
        }

        var items = GlobalUtils.ParseItem(cfg.reward);
        for (int i = 0; i < _rewards.Length; i++)
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
            }else if (i == items.Length && cfg.reward_value > 0)
            {
                _rewards[i].SetActive(true);
                var img = _rewards[i].transform.Find("Image");
                Cfg.Item.SetItemIcon(img.GetComponent<Image>(),ItemId.Act2064Point);
                _rewards[i].transform.Find("Text").GetComponent<Text>().text = "x" + GLobal.NumFormat(cfg.reward_value);
                _rewards[i].transform.Find("Qua").GetComponent<Image>().color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(ItemId.Act2064Point));
                img.GetComponent<Button>().onClick.SetListener(() =>
                {
                    DialogManager.ShowAsyn<_D_ItemTip>(d=>{ d?.OnShow(Lang.Get("进度值"), Cfg.Item.GetItemQua(ItemId.Act2064Point), Lang.Get("累积进度值可领取对应奖励"), img.position); });
                });
            }
            else
            {
                _rewards[i].SetActive(false);
            }
        }
        SetButtonState(cfg,info);

        //设置背景大小
        var lineCount = _textDesc.preferredHeight/_descLineHeight;
        if(lineCount > 1)
            _transformBg.sizeDelta = new Vector2(_transformBg.sizeDelta.x, _bgHeightSize[1]);
        else
            _transformBg.sizeDelta = new Vector2(_transformBg.sizeDelta.x, _bgHeightSize[0]);
        rectTransform.sizeDelta = _transformBg.sizeDelta;
    }

    private void SetButtonState(cfg_act_2003 cfg, P_Act2003Mission info)
    {
        //已过期
        if (info == null)
        {
            _btnForward.SetActive(false);
            _btnGet.SetActive(false);
            _btnOut.SetActive(true);
            _tipClaimed.SetActive(false);
        }
        else if (info.finished == 0) //未完成
        {
            _btnForward.SetActive(true);
            _btnGet.SetActive(false);
            _btnOut.SetActive(false);
            _tipClaimed.SetActive(false);

            Button btn = _btnForward.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                MissionUtils.DoCustomFlow(cfg.click);
            });
        }
        else if (info.finished == 1 && info.get_reward == 0) //完成未领取
        {
            _btnForward.SetActive(false);
            _btnGet.SetActive(true);
            _btnOut.SetActive(false);
            _tipClaimed.SetActive(false);

            Button btn = _btnGet.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                var actInfo = (ActInfo_2003)ActivityManager.Instance.GetActivityInfo(2003);
                if (actInfo != null)
                {
                    Uinfo.Instance.Bag.CheckBlueDrawAlert(cfg.reward,()=> actInfo.RequestGetReward(info.tid));
                }
            });
        }
        else //已领取
        {
            _btnForward.SetActive(false);
            _btnGet.SetActive(false);
            _btnOut.SetActive(false);
            _tipClaimed.SetActive(true);
        }
    }
}
