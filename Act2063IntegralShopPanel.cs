using System;
using UnityEngine;
using UnityEngine.UI;

//积分商店界面
public class Act2063IntegralShopPanel : Dialog
{
    private GameObject _obj;
    private Text _txtTitle;
    private Text _txtRemain;
    private ListView _list;
    private Button _btnClose;
    private ActInfo_2063 _act2063;
    private Button _btnDetail;
    private const int ActId = 2063;
    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override bool IsFullScreen()
    {
        return false;
    }
    protected override void InitRef()
    {
        _txtTitle = transform.Find<Text>("Bg/Text_title");
        _txtRemain = transform.Find<Text>("Bg/Text");
        _btnClose = transform.Find<Button>("Bg/CloseBtn");
        _btnDetail = transform.Find<Button>("Bg/BtnDetail");
        _txtTitle.text = Lang.Get("积分商店");

        var model = transform.Find("Bg/Scroll View/Viewport/Content/ListItemModel").gameObject;
        _list = ListView.Create<IntegralShopItem>(transform.Find<RectTransform>("Bg/Scroll View/Viewport/Content"), model);
    }
    protected override void OnCreate()
    {
        //_btnClose.onClick.SetListener(() =>
        //{
        //    Close();
        //});
        _btnClose.onClick.SetListener(Close);
        _btnDetail.onClick.SetListener(OnBtnDetailClickEvent);
        _act2063 = (ActInfo_2063)ActivityManager.Instance.GetActivityInfo(ActId);
    }

    private void OnBtnDetailClickEvent()
    {
        // DialogManager.ShowAsyn<_D_Tips_HelpDesc>(d => { d?.OnShow(HelpType.IntegralShop, _btnDetail.transform.position, Direction.RightDown, 323); });
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(OnHelpAsynShow);
    }

    private void OnHelpAsynShow(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.IntegralShop, _btnDetail.transform.position, Direction.RightDown, 323);
    }

    public void OnShow()
    {
        var infoData = _act2063._shopData;
        //infoData.Sort((a, b) =>
        //{
        //    if (a.score != b.score)
        //        return (b.score - a.score);//积分降序
        //    return (a.itemid - b.itemid);
        //});
        infoData.Sort(Sort);
        RefreshScore();
        //读表拿数据
        _list.Clear();
        for (int i = 0; i < infoData.Count; i++)
        {
            _list.AddItem<IntegralShopItem>().Refresh(infoData[i], RefreshScore);
        }
    }

    private static int Sort(P_ShopItem2063 a,P_ShopItem2063 b)
    {
        if (a.score != b.score)
            return (b.score - a.score);//积分降序
        return (a.itemid - b.itemid);
    }

    private void RefreshScore()
    {
        _txtRemain.text = Lang.Get("当前拥有积分：{0}", _act2063._totalScore.ToString());
    }
    private class IntegralShopItem : ListItem
    {
        private int _itemId;
        private Image _icon;
        private Image _qua;
        private Button _btnTip;
        private Button _btnExchange;
        private Text _txtNum;
        private int _needScore;
        private Text _txtCanGetNum;
        private Action _ac;
        public override void OnCreate()
        {
            _icon = transform.Find<Image>("Icon/icon");
            _qua = transform.Find<Image>("Icon/Img_qua");
            _btnTip = transform.Find<Button>("Icon/icon");
            _btnExchange = transform.Find<Button>("Button");
            _txtNum = transform.Find<Text>("Icon/Text");
            _txtCanGetNum = transform.Find<Text>("Icon/TextNum");
            var _act2063 = (ActInfo_2063)ActivityManager.Instance.GetActivityInfo(ActId);
            //_btnExchange.onClick.SetListener(() =>
            //{
            //    //积分兑换
            //    if (_act2063._totalScore < _needScore)
            //    {
            //        Alert.Ok(Lang.Get("积分不足{0}", _needScore));
            //    }
            //    else
            //    {
            //        //积分兑换接口
            //        _act2063.BuyScoreShop(_itemId, _ac);
            //    }
            //});

            _btnExchange.onClick.SetListener(OnExchangeBtnClick);
        }

        private void OnExchangeBtnClick()
        {
            var _act2063 = (ActInfo_2063)ActivityManager.Instance.GetActivityInfo(ActId);
            //积分兑换
            if (_act2063._totalScore < _needScore)
            {
                Alert.Ok(Lang.Get("积分不足{0}", _needScore));
            }
            else
            {
                //积分兑换接口
                _act2063.BuyScoreShop(_itemId, _ac);
            }
        }


        public void Refresh(P_ShopItem2063 data, Action ac)
        {
            _ac = ac;
            _itemId = data.itemid;
            _txtNum.text = data.score.ToString();
            _needScore = data.score;
            var item = ItemForShow.Create(data.itemid, data.count);
            _txtCanGetNum.text = Lang.Get("x{0}", data.count);
            item.SetIcon(_icon);
            _qua.color = _ColorConfig.GetQuaColorHSV(item.GetQua());
            _btnTip.onClick.SetListener(() =>
            {
                ItemHelper.ShowTip(data.itemid, data.count, this.transform);
            });
        }
    }
}
