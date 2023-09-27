using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2073_UI : ActivityUI
{
    private Text _tittleText;

    //货架1
    private Activity2073ItemUI[] _goodsList1;
    //货架2
    private Activity2073ItemUI[] _goodsList2;

    private Activity2073ItemUI[] _goodsList3;

    private Button _buyBtn;

    private Button _resetBtn;

    private Button _helpBtn;

    private Text _priceText;

    private Text _leftTimeText;

    private ActInfo_2073 _actInfo;

    //货架1的购物车
    private List<Activity2073ItemUI> _selectList1 = new List<Activity2073ItemUI>();
    //购物车2
    private List<Activity2073ItemUI> _selectList2 = new List<Activity2073ItemUI>();
    private List<Activity2073ItemUI> _selectList3 = new List<Activity2073ItemUI>();
    //3种货架上分别能选购商品的种类数量
    private int[] _buyMaxCount = new int[] { 1, 2, 4 };

    private const int _aid = 2073;

    public override void OnCreate()
    {
        InitRef();
        InitEvent();
        //InitListener();
    }

    private void InitEvent()
    {
        _buyBtn.onClick.SetListener(On_buyBtnClick);
        _resetBtn.onClick.SetListener(ClearAllSelectList);
        _helpBtn.onClick.SetListener(On_helpBtnClick);
    }
    private void On_buyBtnClick()
    {
        if (_actInfo != null)
        {
            if (_selectList1.Count == 1 && _selectList2.Count == 2 && _selectList3.Count == 4)
            {
                string id2 = _selectList2[0].Data.Id + "," + _selectList2[1].Data.Id;
                string id3 = _selectList3[0].Data.Id + "," + _selectList3[1].Data.Id + "," + _selectList3[2].Data.Id + "," + _selectList3[3].Data.Id;
                _actInfo.Buy(_selectList1[0].Data.Pay, _selectList1[0].Data.Id, id2, id3);
            }
            else
            {
                if (_selectList1.Count != 1)
                {
                    MessageManager.Show(Lang.Get("礼物货柜须选购1件商品"));
                }
                else if (_selectList2.Count != 2)
                {
                    MessageManager.Show(Lang.Get("赠品货柜1须选购2件商品"));
                }
                else if (_selectList3.Count != 4)
                {
                    MessageManager.Show(Lang.Get("赠品货柜2须选购4件商品"));
                }
            }
        }
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpBtnDialogShowAsynCB);
    }
    private void On_helpBtnDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2073, _helpBtn.transform.position, Direction.LeftDown, 350);
    }

    private void InitData()
    {
        _actInfo = (ActInfo_2073)ActivityManager.Instance.GetActivityInfo(_aid);

        ClearAllSelectList();
    }

    public override void InitListener()
    {
        base.InitListener();
    }


    private void InitRef()
    {
        Transform commodityList1 = transform.Find("01/CommodityList");
        Transform commodityList2 = transform.Find("02/CommodityList");
        Transform commodityList3 = transform.Find("03/CommodityList");
        _goodsList1 = new Activity2073ItemUI[]
        {
            new Activity2073ItemUI(commodityList1.Find("01")),
            new Activity2073ItemUI(commodityList1.Find("02")),
            new Activity2073ItemUI(commodityList1.Find("03")),
            new Activity2073ItemUI(commodityList1.Find("04")),
            new Activity2073ItemUI(commodityList1.Find("05")),
            new Activity2073ItemUI(commodityList1.Find("06")),
        };

        _goodsList2 = new Activity2073ItemUI[]
       {
            new Activity2073ItemUI(commodityList2.Find("01")),
            new Activity2073ItemUI(commodityList2.Find("02")),
            new Activity2073ItemUI(commodityList2.Find("03")),
            new Activity2073ItemUI(commodityList2.Find("04")),
            new Activity2073ItemUI(commodityList2.Find("05")),
            new Activity2073ItemUI(commodityList2.Find("06")),
       };

        _goodsList3 = new Activity2073ItemUI[]
       {
            new Activity2073ItemUI(commodityList3.Find("01")),
            new Activity2073ItemUI(commodityList3.Find("02")),
            new Activity2073ItemUI(commodityList3.Find("03")),
            new Activity2073ItemUI(commodityList3.Find("04")),
            new Activity2073ItemUI(commodityList3.Find("05")),
            new Activity2073ItemUI(commodityList3.Find("06")),
       };

        _buyBtn = transform.Find<Button>("ButtonBuy");
        _resetBtn = transform.Find<Button>("ButtonReset");
        _helpBtn = transform.Find<Button>("ButtonHelp");
        _priceText = _buyBtn.transform.Find<Text>("Text");
        _leftTimeText = transform.Find<JDText>("TimeText");
        _tittleText = transform.Find<JDText>("Title");
    }

    public override void OnShow()
    {
        InitData();
        _OnShow();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == 2073 && gameObject.activeSelf)
            OnShow();
    }

    private void _OnShow()
    {
        _tittleText.text = _actInfo._name;

        //初始化商品
        Activity2073ItemUI item1 = null;

        //货架1
        var itemlist1 = _actInfo.GetGoodListByType(1);
        for (int i = 0; i < _goodsList1.Length; i++)
        {
            Activity2073ItemUI item = _goodsList1[i];
            if (_selectList1.Contains(_goodsList1[i]))
            {
                item1 = item;
                _goodsList1[i].Refresh(itemlist1[i], true, (click) =>
                {
                    OnClickItem(item1, click);
                });
            }
            else
            {
                _goodsList1[i].Refresh(itemlist1[i], false, (click) =>
                {
                    OnClickItem(item, click);
                });
            }
        }

        //货架2
        var itemlist2 = _actInfo.GetGoodListByType(2);
        for (int i = 0; i < _goodsList2.Length; i++)
        {
            Activity2073ItemUI item = _goodsList2[i];
            if (_selectList2.Contains(_goodsList2[i]))
            {
                _goodsList2[i].Refresh(itemlist2[i], true, (click) =>
                {
                    OnClickItem(item, click);
                });
            }
            else
            {
                _goodsList2[i].Refresh(itemlist2[i], false, (click) =>
                {
                    OnClickItem(item, click);
                });
            }
        }

        //货架3
        var itemlist3 = _actInfo.GetGoodListByType(3);
        for (int i = 0; i < _goodsList3.Length; i++)
        {
            Activity2073ItemUI item = _goodsList3[i];
            if (_selectList3.Contains(_goodsList3[i]))
            {
                _goodsList3[i].Refresh(itemlist3[i], true, (click) =>
                {
                    OnClickItem(item, click);
                });
            }
            else
            {
                _goodsList3[i].Refresh(itemlist3[i], false, (click) =>
                {
                    OnClickItem(item, click);
                });
            }
        }

        if (item1 == null)
        {
            _priceText.text = Lang.Get("未选择礼包");
        }
        else
        {
            _priceText.text = item1.Data.Pay._price;
        }
    }

    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (stamp - _actInfo._data.startts < 0)
        {
            _leftTimeText.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _leftTimeText.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _leftTimeText.text = Lang.Get("活动已经结束");
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        ClearAllSelectList();
    }

    private void OnClickItem(Activity2073ItemUI item, bool selected)
    {
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2002);

        if (selected)
            RemoveItem(item);
        else
            AddItem(item);

        if (_selectList1.Count <= 0)
        {
            _priceText.text = Lang.Get("未选择礼包");
        }
        else
        {
            Activity2073ItemUI item1 = _selectList1[0];
            _priceText.text = item1.Data.Pay._price;
        }
    }

    private void AddItem(Activity2073ItemUI item)
    {
        int maxcount = _buyMaxCount[item.Data.Type - 1];

        List<Activity2073ItemUI> list = null;

        switch (item.Data.Type)
        {
            case 1:
                list = _selectList1;
                break;
            case 2:
                list = _selectList2;
                break;
            case 3:
                list = _selectList3;
                break;
        }

        if (list.Count >= maxcount)
        {
            MessageManager.Show(Lang.Get("该货架最多选购{0}", maxcount));
        }
        else
        {
            list.Add(item);
            item.ResetSelectState();
        }
    }

    private void RemoveItem(Activity2073ItemUI item)
    {
        List<Activity2073ItemUI> list = null;

        switch (item.Data.Type)
        {
            case 1:
                list = _selectList1;
                break;
            case 2:
                list = _selectList2;
                break;
            case 3:
                list = _selectList3;
                break;
        }

        if (list.Count > 0)
        {
            list.Remove(item);
            item.ResetSelectState();
        }
    }

    //清空所有购物车
    private void ClearAllSelectList()
    {
        for (int i = 0; i < _selectList1.Count; i++)
        {
            var item = _selectList1[i];
            item.SetTip(false);
        }

        for (int i = 0; i < _selectList2.Count; i++)
        {
            var item = _selectList2[i];
            item.SetTip(false);
        }

        for (int i = 0; i < _selectList3.Count; i++)
        {
            var item = _selectList3[i];
            item.SetTip(false);
        }

        _selectList1.Clear();
        _selectList2.Clear();
        _selectList3.Clear();

        _priceText.text = Lang.Get("未选择礼包");
    }
}

public class Activity2073ItemUI
{
    private Image _icon;
    private Text _countText;
    private Text _leftText;
    private GameObject _tipGo;
    private Button _selectBtn;
    private GameObject _maskGo;
    private bool _isSelected = false;
    private Action<bool> _callback;
    public Act2073ItemInfo Data { private set; get; }
    private ItemForShow _itemForShow;

    public Activity2073ItemUI(Transform trans)
    {
        _icon = trans.Find<Image>("Icon");
        _leftText = trans.Find<Text>("LeftCount");
        _tipGo = trans.Find<GameObject>("Select");
        _countText = trans.Find<Text>("Text");
        _selectBtn = trans.GetComponent<Button>();
        _maskGo = trans.Find<GameObject>("img");

        _selectBtn.onClick.SetListener(On_selectBtnClick);
    }
    private void On_selectBtnClick()
    {
        if (Data.leftCount <= 0 && Data.IsLimit)
            return;

        if (_callback != null)
        {
            _callback(_isSelected);
            SetTip(_isSelected);
        }
    }


    public void Refresh(Act2073ItemInfo data, bool selected, Action<bool> callback)
    {
        Data = data;
        _callback = callback;
        _itemForShow = new ItemForShow(data.Item.id, data.Item.count);
        _itemForShow.SetIcon(_icon);
        _countText.text = "x" + _itemForShow.GetCount();
        if (data.IsLimit)
        {
            if (data.leftCount > 0)
                _leftText.text = Lang.Get("剩余{0}", data.leftCount);
            else
                _leftText.text = Lang.Get("售罄");
        }
        else
            _leftText.text = "";

        if (data.leftCount <= 0 && data.IsLimit)
        {
            SetTip(false);
        }
        else
        {
            SetTip(selected);
        }
    }

    public void SetTip(bool isSelected)
    {
        //没库存
        if (Data.leftCount <= 0 && Data.IsLimit)
        {
            //置灰
            _maskGo.SetActive(true);
            _tipGo.SetActive(false);
            _isSelected = false;
            _leftText.text = Lang.Get("售罄");
        }
        else
        {
            if (isSelected)
            {
                _leftText.text = _itemForShow.GetName();
                _tipGo.SetActive(true);
                _maskGo.SetActive(true);
            }
            else
            {
                _leftText.text = Data.IsLimit ? Lang.Get("剩余{0}", Data.leftCount) : "";
                _tipGo.SetActive(false);
                _maskGo.SetActive(false);
            }
            _isSelected = isSelected;
        }
    }

    public void ResetSelectState()
    {
        _isSelected = !_isSelected;
    }
}
