using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Act2064Tips
{
    private Button _btnClose;
    private Button _btnConfirm;
    private Button _btnExpand;
    private JDText _textBtnExpand;
    private Transform _tran;

    private RectTransform _content;
    private ListView _list;
    private Canvas _canvas;

    private ActInfo_2064 _info;
    private Action _cb;
    public const int UnlockPrice = 680; //解锁奖励固定的金额

    public _Act2064Tips(Transform tran, ActInfo_2064 info, Action ac)
    {
        _tran = tran;
        _info = info;
        _cb = ac;
        _canvas = _tran.GetComponent<Canvas>();
        _btnClose = _tran.Find<Button>("CloseBtn");
        _btnConfirm = _tran.Find<Button>("ButtonLeft");
        _btnExpand = _tran.Find<Button>("ButtonRight");
        _textBtnExpand = _btnExpand.transform.Find<JDText>("Text");
        _textBtnExpand.text = Lang.Get("{0} 扩充", UnlockPrice);
        //Screen.width - 640f + (Screen.width - 640f) / 2 + 25f;
        _tran.localPosition = Vector3.zero;
        _content = _tran.Find<RectTransform>("Scroll View/Content/Icon");
        GameObject model = _tran.Find("Scroll View/Content/Icon/Icon").gameObject;
        _list = ListView.Create<_2064TipItem>(_content, model);
        _btnClose.onClick.AddListener(On_btnCloseClick);
        _btnExpand.onClick.AddListener(On_btnExpandClick);
        _btnConfirm.onClick.AddListener(On_btnConfirmClick);
    }

    private void On_btnCloseClick()
    {
        if (_info.SelectedId != 0)
            if (_cb != null)
                _cb();
        CloseTip();
    }
    private void On_btnExpandClick()
    {
        var d = Alert.YesNo(Lang.Get("是否花费{0}氪晶解锁所有奖励", UnlockPrice));
        d.SetYesCallback(() =>
        {
            if (ItemHelper.IsCountEnough(ItemId.Gold, UnlockPrice))
            {
                _info.ExpandReward(Refresh);
            }
            d.Close();
        });
    }
    private void On_btnConfirmClick()
    {
        if (_info.SelectedId == 0)
        {
            Alert.Ok(Lang.Get("请指挥官选择一份奖励"));
            return;
        }

        if (_cb != null)
            _cb();
        CloseTip();
    }
    public void OpenTip()
    {
        _tran.gameObject.SetActive(true);
        var rootDialog = DialogManager.GetInstanceOfDialog<_D_ActCalendar>();
        rootDialog.SetBlock(true);
        _canvas.sortingOrder = rootDialog.Canvas.sortingOrder + 1;//将sortingOrder设置为比底层界面高1
        Refresh();
    }

    public void CloseTip()
    {
        _tran.gameObject.SetActive(false);
        DialogManager.GetInstanceOfDialog<_D_ActCalendar>().SetBlock(false);
    }

    private void Refresh()
    {
        _list.Clear();
        bool hasUnlock = _info.Unlock == 1;
        _btnExpand.interactable = !hasUnlock;

        List<int> ids = new List<int>();
        var cantSelectIds = _info.GetNotSelectableIds();
        var expandReward = Cfg.Activity2064.GetSelectData(_info.Step);
        var unlockReward = Cfg.Activity2064.GetLockData(_info.Step);

        for (int i = 0; i < expandReward.Count; i++)
        {
            var item = expandReward[i];
            //if (!cantSelectIds.Contains(item.id))
            {
                _list.AddItem<_2064TipItem>().OnRefresh(item, _info, SetState);
            }
        }

        for (int i = 0; i < unlockReward.Count; i++)
        {
            var item = unlockReward[i];
            //if (!cantSelectIds.Contains(item.id))
            {
                if (hasUnlock)
                {
                    _list.AddItem<_2064TipItem>().OnRefresh(item, _info, SetState);
                }
                else
                {
                    _list.AddItem<_2064TipItem>().OnRefreshLock(item, _info, SetState);
                }
            }
        }
    }

    private void SetState()
    {
        for (int i = 0; i < _list._listItems.Count; i++)
        {
            var item = _list._listItems[i] as _2064TipItem;
            item.SetState();
        }
    }
    public void OnDestroy()
    {
        _info = null;
    }
}

public class _2064TipItem : ListItem
{
    private Image _imgIcon;
    private Text _txtCount;

    private Button _btnTip;
    private GameObject _objSelect;
    private GameObject _objLock;

    private Image _imgQua;
    private Text _txtSelCount;
    private Button _btnSel;
    private Text _txtSel;

    private cfg_act_2064 _info;
    private ActInfo_2064 _actInfo;
    public int Id; //奖品Id->cfg_act_2064->id
    private bool isLock;
    private Action _ac;

    public override void OnCreate()
    {
        _imgIcon = transform.Find<Image>("Img_icon");
        _txtCount = transform.Find<Text>("Text_num");
        _objSelect = transform.Find("Select").gameObject;
        _btnTip = transform.FindButton("");
        _btnSel = transform.FindButton("Button");
        _txtSel = transform.Find<Text>("Button/Text");
        _objLock = transform.Find("Lock").gameObject;
        _txtSelCount = transform.Find<Text>("Image/Text");
        _imgQua = transform.Find<Image>("Img_qua");

        _btnSel.onClick.AddListener(On_btnSelClick);
        _btnTip.onClick.AddListener(On_btnTipClick);
    }
    private void On_btnSelClick()
    {
        _actInfo.SelectedId = Id;
        if (_ac != null)
            _ac();
    }
    private void On_btnTipClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(OnbtnTipDialogShowAsyn);
    }
    private void OnbtnTipDialogShowAsyn(_D_ItemTip d)
    {
        var id = int.Parse(_info.item.Split('|')[0]);
        d?.OnShow(id, 1, _objSelect.transform.position);
    }


    public void OnRefresh(cfg_act_2064 info, ActInfo_2064 actInfo, Action callback)
    {
        isLock = false;
        _info = info;
        _actInfo = actInfo;
        _ac = callback;
        _objLock.SetActive(false);
        bool isSel = _info.id != actInfo.SelectedId;
        _btnSel.interactable = isSel;
        _txtSel.text = isSel ? Lang.Get("选择") : Lang.Get("已选择");
        SetInfo();
    }

    public void OnRefreshLock(cfg_act_2064 info, ActInfo_2064 actInfo, Action callback)
    {
        isLock = true;
        _info = info;
        _actInfo = actInfo;
        _ac = callback;
        _objLock.SetActive(true);
        _btnSel.interactable = false;
        _txtSel.text = Lang.Get("未解锁");
        SetInfo();
    }

    private void SetInfo()
    {
        Id = _info.id;
        int id = int.Parse(_info.item.Split('|')[0]);
        Cfg.Item.SetItemIcon(_imgIcon, id);
        _imgQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(id));
        _txtCount.text = "x" + _info.item.Split('|')[1];
        SetState();
    }

    public void SetState()
    {
        int selCount = _actInfo.GetSelectCount(Id);
        if (isLock)
        {
            _txtSelCount.text = _info.limit > 0 ? Lang.Get("可选 {0}/{1}", _info.limit - selCount, _info.limit) : Lang.Get("可选");
            _txtSel.color = new Color(36f / 255, 116f / 255, 152f / 255);
        }
        else
        {
            _objSelect.SetActive(Id == _actInfo.SelectedId);
            bool isSel = _info.id != _actInfo.SelectedId;
            _btnSel.interactable = isSel;
            _txtSel.text = isSel ? Lang.Get("选择") : Lang.Get("已选择");
            _txtSel.color = isSel ? Color.white : new Color(36f / 255, 116f / 255, 152f / 255);

            if (_info.limit > 0 && selCount >= _info.limit)
            {
                _btnSel.interactable = false;
                _txtSel.text = Lang.Get("已选完");
                _txtSelCount.text = "";
            }
            else
            {
                if (_info.limit == 0)
                    _txtSelCount.text = Lang.Get("无上限");
                else
                    _txtSelCount.text = _info.limit > 0 ? Lang.Get("可选 {0}/{1}",
                        _info.limit - selCount, _info.limit) : Lang.Get("可选");
            }
        }
    }
}
