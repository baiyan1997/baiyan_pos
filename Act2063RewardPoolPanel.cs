using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//这个是奖励池界面
public class Act2063RewardPoolPanel
{
    private GameObject _obj;
    private Button[] _tabBtns;
    private GameObject[] _tabViews;
    private TabHelper _tabViewHelper;
    private TabBtnHelper _tabBtnHelper;
    private Button _btnConfirm;
    private Action _ac;
    private Text _txtSelect;//已经选择的数量
    private ActInfo_2063 _actInfo;
    private Text[] _txtReds;//红点
    private const int RewardNum = 10;
    private const int ActId = 2063;
    public Act2063RewardPoolPanel(GameObject obj, Action ac)
    {
        _obj = obj;
        _ac = ac;
        _tabBtns = new[]
        {
            _obj.transform.Find<Button>("Btns/01"),
            _obj.transform.Find<Button>("Btns/02"),
            _obj.transform.Find<Button>("Btns/03"),
            _obj.transform.Find<Button>("Btns/04"),
        };
        _txtReds = new[]
        {
            _obj.transform.Find<Text>("Btns/01/Red/Text"),
            _obj.transform.Find<Text>("Btns/02/Red/Text"),
            _obj.transform.Find<Text>("Btns/03/Red/Text"),
            _obj.transform.Find<Text>("Btns/04/Red/Text"),
        };
        _tabViews = new[]
        {
            _obj.transform.Find("ActRewardPanel1").gameObject,
            _obj.transform.Find("ActRewardPanel2").gameObject,
            _obj.transform.Find("ActRewardPanel3").gameObject,
            _obj.transform.Find("ActRewardPanel4").gameObject,
        };
        _txtSelect = _obj.transform.Find<Text>("TextSelect/Text");
        _btnConfirm = _obj.transform.Find<Button>("Button/Btn_close");
        _tabViewHelper = new TabHelper();
        _tabBtnHelper = new TabBtnHelper();
        for (int i = 0; i < _tabBtns.Length; i++)
        {
            _tabBtnHelper.RegistTabBtn(_tabBtns[i].gameObject.AddBehaviour<BtnTabRatingPool>(), i);
        }
        for (int i = 0; i < _tabViews.Length; i++)
        {
            _tabViews[i].SetActive(false);
        }
        _actInfo = (ActInfo_2063)ActivityManager.Instance.GetActivityInfo(ActId);
        _actInfo._callBackSelectData += RefreshSelectNum;
        _tabBtnHelper.OnTabSwitch += SwitchTab;
        _btnConfirm.onClick.SetListener(OnBtnConfirmClick);
    }

    private void OnBtnConfirmClick()
    {
        if (_actInfo.GetCandidacyRewardNum() < RewardNum)
        {
            var d = Alert.YesNo(Lang.Get("奖池奖励未选满，是否离开？"));
            d.SetYesCallback(() =>
            {
                _obj.SetActive(false);
                d.Close();
                if (_ac != null)
                    _ac();
            });
        }
        else
        {
            _obj.SetActive(false);
            if (_ac != null)
                _ac();
        }
    }
    //刷新已经选择的数据
    private void RefreshSelectNum(int getNum)
    {
        if (_txtSelect)
        {
            _txtSelect.text = Lang.Get("已选择{0}/{1}", getNum, RewardNum);
            var tab = _tabViewHelper.GetCurrentTab(true);
            tab.Select();//刷新信息
            if (_txtReds[tab.SpId])
            {
                _txtReds[tab.SpId].transform.parent.gameObject.SetActive((tab as ActRewardPanel).GetRemainNum() != 0);
                _txtReds[tab.SpId].text = (tab as ActRewardPanel).GetRemainNum().ToString();
            }
        }
    }
    public void SwitchTab(int oldIndex, int newIndex)
    {
        var view = _tabViewHelper.GetTabBySpId(newIndex, true);
        if (view == null)
        {
            if (newIndex == 0)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActRewardPanel>(), newIndex);
            if (newIndex == 1)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActRewardPanel>(), newIndex);
            if (newIndex == 2)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActRewardPanel>(), newIndex);
            if (newIndex == 3)
                _tabViewHelper.AddTab(_tabViews[newIndex].AddBehaviour<ActRewardPanel>(), newIndex);
        }
        _tabViewHelper.ClickBySpId(newIndex);
    }
    private void AddTab()
    {
        for (int i = 0; i < 4; i++)
        {
            var view = _tabViewHelper.GetTabBySpId(i, true);
            if (view == null)
            {
                _tabViewHelper.AddTab(_tabViews[i].AddBehaviour<ActRewardPanel>(), i);
            }
        }
    }
    public void OnShow()
    {
        _tabBtnHelper.Finish();
        var tab = _tabViewHelper.GetCurrentTab(true);
        tab.Select();//刷新信息
        AddTab();
        var temp = _tabViewHelper.GetTabList();
        for (int i = 0; i < temp.Count; i++)
        {
            (temp[i] as ActRewardPanel).Refresh();
            _txtReds[temp[i].SpId].transform.parent.gameObject.SetActive((temp[i] as ActRewardPanel).GetRemainNum() != 0);
            _txtReds[temp[i].SpId].text = (temp[i] as ActRewardPanel).GetRemainNum().ToString();
        }
        RefreshSelectNum(_actInfo.GetCandidacyRewardNum());
    }
    public void OnDestroy()
    {
        if (_tabViewHelper != null)
        {
            _tabViewHelper.OnDestroy();
            _tabViewHelper = null;
        }
        if (_tabBtnHelper != null)
        {
            _tabBtnHelper.OnDestroy();
            _tabBtnHelper = null;
        }
    }

}
//等级奖池页面按钮
public class BtnTabRatingPool : TabBtnBase//TabButton
{
    private ObjectGroup _objGroup;
    public Sprite[] _sprite;
    public override void Awake()
    {
        _objGroup = transform.parent.GetComponent<ObjectGroup>();
        _sprite = new[]
        {
            _objGroup.Sprite("BtnUnSelected"),
            _objGroup.Sprite("BtnSelected"),
        };
    }
    public override void Select()
    {
        GetButton().image.sprite = _sprite[1];
    }
    public override void Unselect()
    {
        GetButton().image.sprite = _sprite[0];
    }
}
//奖池分界界面
public class ActRewardPanel : TabViewBase2
{
    private ListView _list;
    private Text _txtTip;
    private int[] _canGetRwardNum;//奖池可选数量
    private List<int> _alreadySelect = new List<int>();//该面板下被选中的item
    private const int ActId = 2063;
    public override void Awake()
    {
        base.Awake();
        _canGetRwardNum = new[] { 4, 3, 2, 1 };
        _txtTip = transform.Find<Text>("Text/Text");
        var model = transform.Find("Scroll View/Viewport/Content/ListItemModel").gameObject;
        _list = ListView.Create<RewardItem>(transform.Find<RectTransform>("Scroll View/Viewport/Content"), model);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void Select()
    {
        base.Select();
        Refresh();
    }
    public void Refresh()
    {
        _txtTip.text = Lang.Get("请从下列道具中选择{0}项", _canGetRwardNum[SpId]);
        _alreadySelect.Clear();
        var _act2063 = (ActInfo_2063)ActivityManager.Instance.GetActivityInfo(ActId);
        var id = _act2063.GetIndexToId(SpId + 1);
        //  var data = _act2063._actData.Find(item => item.id == id);//Cfg.Act2063.GetDataGradeId(id);
        P_Act_2063 data = null;
        for (int i = 0; i < _act2063._actData.Count; i++)
        {
            P_Act_2063 act = _act2063._actData[i];
            if (act.id == id)
            {
                data = act;
                break;
            }
        }
        var items = GlobalUtils.ParseItem(data.reward);
        //该面板下选择的数据
        var totalList = _act2063.GetReawrdPanel(data.index);
        for (int i = 0; i < items.Length; i++)
        {
            if (totalList != null)
            {
                if (totalList.Contains(items[i].id))
                    _alreadySelect.Add(items[i].id);
            }
        }
        //按照是否有临界值排序[有在前，没有在后]
        var temp = new List<P_Item>();
        temp.AddRange(items);
        //temp.Sort((a, b) =>
        //{
        //    return (b.extra - a.extra);
        //});
        temp.Sort(Sort_extra);
        //读表拿数据
        _list.Clear();
        for (int i = 0; i < temp.Count; i++)
        {
            _list.AddItem<RewardItem>().Refresh(data.id, temp[i], _canGetRwardNum[SpId], _alreadySelect);
        }
    }

    private static int Sort_extra(P_Item a, P_Item b)
    {
        return (b.extra - a.extra);
    }

    //剩余可选数量
    public int GetRemainNum()
    {
        return _canGetRwardNum[SpId] - _alreadySelect.Count;
    }

    private class RewardItem : ListItem
    {
        private int _id;//对应表中的id
        private int _itemId;//图片Id
        private Image _icon;
        private Image _qua;
        private Button _btnSelect;
        private Button _btnUnSelect;
        private Button _btnDetail;
        private GameObject _objSelectedSign;//选中图标
        private GameObject _mask;//选中数量达到指定数量后，其他不可选
        private const int ActId = 2063;
        private ActInfo_2063 _actInfo;
        private Text _txtNum;//数量显示
        private Text _txtCanGetNum;//可以得到的数量
        private Text _txtSelect;
        private Text _txtUnSelect;
        private bool _isEnough;//奖池中道具是否足够
        private Color32 Normal = new Color32(176, 243, 255, 255);
        private Color32 Limit = new Color32(255, 204, 0, 255);
        public override void OnCreate()
        {
            _icon = transform.Find<Image>("Icon/icon");
            _qua = transform.Find<Image>("Icon/Img_qua");
            _objSelectedSign = transform.Find<GameObject>("Icon/Sign");

            _mask = transform.Find<GameObject>("Icon/Mask");

            _txtCanGetNum = transform.Find<Text>("Icon/TextNum");
            _txtNum = transform.Find<Text>("Icon/Text");
            _btnSelect = transform.Find<Button>("BtnSelect");
            _btnUnSelect = transform.Find<Button>("BtnUnSelect");
            _txtSelect = transform.Find<Text>("BtnSelect/Text");
            _txtUnSelect = transform.Find<Text>("BtnUnSelect/Text");
            _btnDetail = transform.Find<Button>("Icon/icon");
            _txtSelect.text = Lang.Get("选定");
            _txtUnSelect.text = Lang.Get("取消");
            _btnSelect.gameObject.SetActive(true);
            _btnUnSelect.gameObject.SetActive(false);
            _actInfo = (ActInfo_2063)ActivityManager.Instance.GetActivityInfo(ActId);
            //_btnSelect.onClick.SetListener(() =>
            //{
            //    if (_mask.activeSelf)
            //    {
            //        if (_isEnough)
            //        {
            //            //奖池中仍存在，但是数量足够
            //            MessageManager.Show(Lang.Get("奖池道具可选项达到上限"));
            //        }
            //        else
            //        {
            //            //超过限定值
            //            MessageManager.Show(Lang.Get("本奖励剩余数量为0"));
            //        }

            //        return;
            //    }
            //    _actInfo.AddSelectDate(_id, _itemId);
            //    _objSelectedSign.SetActive(true);
            //    _btnSelect.gameObject.SetActive(false);
            //    _btnUnSelect.gameObject.SetActive(true);
            //});
            _btnSelect.onClick.SetListener(On_btnSelectClick);
            //_btnUnSelect.onClick.SetListener(() =>
            //{
            //    if (_mask.activeSelf)
            //    {
            //        if (_isEnough)
            //        {
            //            //奖池中仍存在，但是数量足够
            //            MessageManager.Show(Lang.Get("奖池道具可选项达到上限"));
            //        }
            //        else
            //        {
            //            //超过限定值
            //            MessageManager.Show(Lang.Get("本奖励剩余数量为0"));
            //        }

            //        return;
            //    }
            //    _actInfo.RemoveSelectDate(_id, _itemId);
            //    _objSelectedSign.SetActive(false);
            //    _btnSelect.gameObject.SetActive(true);
            //    _btnUnSelect.gameObject.SetActive(false);
            //});
            _btnUnSelect.onClick.SetListener(On_btnUnSelectClick);
            //_btnDetail.onClick.SetListener(() =>
            //{
            //    ItemHelper.ShowTip(_itemId, 1, this.transform);
            //});
            _btnDetail.onClick.SetListener(On_btnDetailClick);
        }

        private void On_btnSelectClick()
        {
            if (_mask.activeSelf)
            {
                if (_isEnough)
                {
                    //奖池中仍存在，但是数量足够
                    MessageManager.Show(Lang.Get("奖池道具可选项达到上限"));
                }
                else
                {
                    //超过限定值
                    MessageManager.Show(Lang.Get("本奖励剩余数量为0"));
                }

                return;
            }
            _actInfo.AddSelectDate(_id, _itemId);
            _objSelectedSign.SetActive(true);
            _btnSelect.gameObject.SetActive(false);
            _btnUnSelect.gameObject.SetActive(true);
        }
        private void On_btnUnSelectClick()
        {
            if (_mask.activeSelf)
            {
                if (_isEnough)
                {
                    //奖池中仍存在，但是数量足够
                    MessageManager.Show(Lang.Get("奖池道具可选项达到上限"));
                }
                else
                {
                    //超过限定值
                    MessageManager.Show(Lang.Get("本奖励剩余数量为0"));
                }

                return;
            }
            _actInfo.RemoveSelectDate(_id, _itemId);
            _objSelectedSign.SetActive(false);
            _btnSelect.gameObject.SetActive(true);
            _btnUnSelect.gameObject.SetActive(false);
        }
        private void On_btnDetailClick()
        {
            ItemHelper.ShowTip(_itemId, 1, this.transform);
        }
        public void Refresh(int id, P_Item item, int limitNum, List<int> itemList)
        {
            _id = id;
            _itemId = item.id;
            var alreadyUseCount = _actInfo.GetAlreadyUseCount(_id, _itemId);
            var item1 = ItemForShow.Create(item.id, item.count);
            _txtCanGetNum.text = Lang.Get("x{0}", GLobal.NumFormat(item1.GetCount()));
            _txtNum.gameObject.SetActive(item.extra != 0);
            //剩下可以抽到的
            var remainNum = item.extra - alreadyUseCount;

            var remainCnt = -1;
            // 改为剩余次数
            if (item.count != 0)
            {
                remainCnt = remainNum / item.count;
            }
            else
            {
                Debug.LogErrorFormat("配置道具的可抽取数量为0，有错误！ {0}", item.id);
            }

            var allCanCnt = item.extra / item.count;
            _txtNum.text = Lang.Get("{0}/{1}", remainCnt.ToString(), allCanCnt.ToString());
            _txtNum.color = alreadyUseCount < item.extra ? Normal : Limit;
            item1.SetIcon(_icon);
            _qua.color = _ColorConfig.GetQuaColorHSV(item1.GetQua());
            _mask.SetActive(false);
            //道具有限制得情况下 超出限定数量会被禁止使用得
            if (alreadyUseCount >= item.extra && item.extra != 0)
            {
                _isEnough = false;
                _mask.SetActive(true);
                _objSelectedSign.SetActive(false);
                _btnSelect.gameObject.SetActive(true);
                _btnUnSelect.gameObject.SetActive(false);
            }
            else
            {
                _isEnough = true;
                //是否有mask
                if (itemList.Contains(_itemId))
                {
                    _mask.SetActive(false);
                    _objSelectedSign.SetActive(true);
                    _btnSelect.gameObject.SetActive(false);
                    _btnUnSelect.gameObject.SetActive(true);
                }
                else
                {
                    if (itemList.Count < limitNum)
                    {
                        _mask.SetActive(false);
                    }
                    else
                    {
                        _mask.SetActive(true);
                    }
                    _btnSelect.gameObject.SetActive(true);
                    _btnUnSelect.gameObject.SetActive(false);
                    _objSelectedSign.SetActive(false);
                }
            }

        }
    }
}