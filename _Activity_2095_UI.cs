using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class _Activity_2095_UI : ActivityUI
{
    private Text _timeText;//活动时间
    private Button _helpButton;//帮助按钮
    private ActInfo_2095 _actInfo;
    public override void OnCreate()
    {
        InitRef();
        InitButton();
        //InitListener();

        ShipSelection_2095.Instance.OnCreate(transform.Find("Main0"));
        ShipStatus_2095.Instance.OnCreate(transform.Find("Main1"));
        EquipmentPage_2095.Instance.OnCreate(transform.Find("Tips_02"));
        ExplorePage_2095.Instance.OnCreate(transform.Find("Tips_01"));
        PortWarehouse_2095.Instance.OnCreate(transform.Find("Tips_03"));
        GetFuelPage_2095.Instance.OnCreate(transform.Find("Tips_04"));
    }
    private void InitRef()
    {
        _timeText = transform.Find<Text>("Text");
        _helpButton = transform.Find<Button>("Btn");
        _actInfo = (ActInfo_2095)ActivityManager.Instance.GetActivityInfo(2095);
    }
    private void InitButton()
    {
        _helpButton.onClick.AddListener(On_helpButtonClick);
    }
    private void On_helpButtonClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpButtonDialogShowAsynCB);
    }
    private void On_helpButtonDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2095, _helpButton.transform.position, Direction.LeftDown, 350);
    }
    public override void InitListener()
    {
        base.InitListener();
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdateActivityUI.AddListener(Refresh);

    }
    public override void OnShow()
    {
        UpdateTime(0);
        //根据是否选择战舰来决定是否显示当前界面
        if (_actInfo.GetShipBeenSelected())
        {
            ShipStatus_2095.Instance.Show();
        }
        else
        {
            ShipSelection_2095.Instance.Show();
        }
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == 2095)
        {
            ShipStatus_2095.Instance.Refresh();
            ExplorePage_2095.Instance.Refresh();
            OnShow();
        }
        if (aid == 209501)
        {
            ShipStatus_2095.Instance.RefreshShipStatus();
        }
    }

    public override void UpdateTime(long currentTime)
    {
        base.UpdateTime(currentTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (_actInfo == null)
            return;
        _timeText.text = _actInfo.LeftTime >= 0 ? GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true) : Lang.Get("活动已经结束");
    }

    public override void OnClose()
    {
        base.OnClose();
        CloseUI();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CloseUI();
        EquipmentPage_2095.Instance.OnDestroy();
        ExplorePage_2095.Instance.OnDestroy();
        PortWarehouse_2095.Instance.OnDestroy();
        GetFuelPage_2095.Instance.OnDestroy();
    }

    private void CloseUI()
    {
        if (ShipStatus_2095.Instance.IsShow()) ShipStatus_2095.Instance.Close();
        if (EquipmentPage_2095.Instance.IsShow()) EquipmentPage_2095.Instance.Close();
        if (ExplorePage_2095.Instance.IsShow()) ExplorePage_2095.Instance.Close();
        if (PortWarehouse_2095.Instance.IsShow()) PortWarehouse_2095.Instance.Close();
        if (GetFuelPage_2095.Instance.IsShow()) GetFuelPage_2095.Instance.Close();
    }
}

public class ShipSelection_2095 : Singleton<ShipSelection_2095>
{
    //战舰图片
    class ShipIcon
    {
        public int ShipId;
        public Image Icon;
        public Button SelectButton;
        public Image Que;
        public Text Name;
        public GameObject SelectionBox;
        public GameObject Root;
    }
    //选择战舰界面
    private Transform _trans;
    private Button _selectShip;//选择战舰按钮
    private List<ShipIcon> _selectShips;//备选战舰列表
    private int _selectedWarship;//当前选择的战舰id
    private ActInfo_2095 _actInfo;


    //选中的战舰显示区域
    private Text _currentSelectName;
    private Image _shipImage;
    public void OnCreate(Transform trans)
    {
        _trans = trans;
        _trans.gameObject.SetActive(false);
        InitRef();
        InitButton();
    }

    public bool IsShow()
    {
        return _trans.gameObject.activeSelf;
    }
    private void InitRef()
    {
        _actInfo = (ActInfo_2095)ActivityManager.Instance.GetActivityInfo(2095);
        _selectShip = _trans.Find<Button>("AirShip/Btn");
        _currentSelectName = _trans.Find<Text>("AirShip/Text1");
        _shipImage = _trans.Find<Image>("AirShip/AirShipImg/AirShipImg");
        _selectShips = new List<ShipIcon>();
        //初始化四艘战舰图标
        for (int i = 0; i < 4; i++)
        {
            ShipIcon tempShipIcon = new ShipIcon
            {
                Icon = _trans.Find<Image>("AirShipMenu/Viewport/Content/Icon" + i + "/Img_icon"),
                Name = _trans.Find<Text>("AirShipMenu/Viewport/Content/Icon" + i + "/Text"),
                Que = _trans.Find<Image>("AirShipMenu/Viewport/Content/Icon" + i + "/Img_bg"),
                SelectButton = _trans.Find<Button>("AirShipMenu/Viewport/Content/Icon" + i),
                SelectionBox = _trans.Find<GameObject>("AirShipMenu/Maskbtn" + i),
                Root = _trans.Find<GameObject>("AirShipMenu/Viewport/Content/Icon" + i),
            };
            tempShipIcon.Root.SetActive(false);
            _selectShips.Add(tempShipIcon);
            var i1 = i;
            _selectShips[i].SelectButton.onClick.AddListener(() =>
            {
                SelectShip(_selectShips[i1].ShipId, i1);
            });
        }
    }

    private void InitButton()
    {
        _selectShip.onClick.AddListener(SelectWarShip);
    }
    public void Show()
    {
        _trans.gameObject.SetActive(true);
        RefreshSelectShip();
    }
    public void Close()
    {
        _trans.gameObject.SetActive(false);
    }
    private void RefreshSelectShip()
    {
        _selectedWarship = -1;
        _shipImage.gameObject.SetActive(false);
        _currentSelectName.gameObject.SetActive(false);
        List<int> shipList = _actInfo.GetShipList();
        ShipIcon shipIcon = null;
        for (int i = 0; i < shipList.Count; i++)
        {
            int shipId = shipList[i];
            var ship = ShipYardInfo.Instance.FindShip(shipId);
            shipIcon = _selectShips[i];
            shipIcon.Root.SetActive(true);
            shipIcon.SelectionBox.SetActive(false);
            Cfg.Ship.SetShipIcon(shipIcon.Icon, shipId);
            shipIcon.Name.text = Lang.Get("Lv.{0} {1}", ship.ship_lv, Cfg.Ship.GetShipName(shipId));
            shipIcon.Que.color = _ColorConfig.GetQuaColor(ship.qua);
            shipIcon.Name.color = _ColorConfig.GetQuaColor(ship.qua);
            shipIcon.ShipId = shipId;
        }
    }
    private void ShowShip(int shipId)
    {

        var ship = ShipYardInfo.Instance.FindShip(shipId);
        string shipName = Cfg.Ship.GetShipName(shipId);
        _currentSelectName.text = Lang.Get("[{0}] Lv.{1} {2} ", Cfg.Ship.GetShipTypeNameByType(ship.type), ship.ship_lv, shipName);
        _currentSelectName.color = _ColorConfig.GetQuaColor(ship.qua);
        Cfg.Ship.SetShipIcon(_shipImage, shipId);
    }
    private void SelectShip(int id, int num)
    {
        if (!_currentSelectName.IsActive())
        {
            _shipImage.gameObject.SetActive(true);
            _currentSelectName.gameObject.SetActive(true);
        }

        if (id == _selectedWarship)
        {
            return;
        }
        _selectedWarship = id;
        for (int i = 0; i < _selectShips.Count; i++)
        {
            _selectShips[i].SelectionBox.SetActive(i == num);
        }

        ShowShip(id);
    }
    //选择战舰
    private void SelectWarShip()
    {
        if (_selectedWarship == -1)
        {
            string msg = Lang.Get("请选择一艘战舰!");
            MessageManager.Show(msg);
        }
        else
        {
            var tempAlert = Alert.YesNo(Lang.Get("确认选择后今日不可变更!"));
            tempAlert.SetYesCallback(() =>
            {
                tempAlert.Close();
                //提交_selectedWarship
                _actInfo.SelectShip(_selectedWarship, () =>
                {
                    Close();
                    ShipStatus_2095.Instance.Show();
                });
            });
            tempAlert.SetNoCallback(() =>
            {
                tempAlert.Close();
            });
        }
    }
}

public class ShipStatus_2095 : Singleton<ShipStatus_2095>
{
    private Transform _trans;
    //三个装备栏
    class ShipEquipment2095
    {
        public Image Icon;//图标
        public Button OpenChangeEquipPage;//打开详细装备面板按钮
        public GameObject StarRoot;
        public List<Image> Star;
    }
    //战舰碎片
    private Image _shipDebrisIcon;//战舰碎片图标
    private Image _shipDebrisQue;//战舰碎片品质
    private Text _shipDebrisName;//战舰碎片名称
    private Text _shipDebrisNum;//战舰碎片数量
    private Text _puzzleNum;//拼图数量
    private Button _shipDebrisDesc;//展示战舰碎片描述
    private Text _fuel;//当前燃料
    //private Text _combatEffectiveness;//战斗力
    private Image _shipImage;
    private Text _propertyPanel;//属性面板
    //按钮部分
    private Button _awardButton;//领奖按钮
    private Button _explore;//探索按钮
    private Button _getFuel;//获取燃料按钮
    private Button _warehouse;//仓库按钮
    //选中的战舰显示区域
    //是否是第一次打开
    private bool _firstOpen;

    private List<ShipEquipment2095> _shipEquipments;//装备栏
    private ActInfo_2095 _actInfo;
    public void OnCreate(Transform trans)
    {
        _trans = trans;
        _trans.gameObject.SetActive(false);
        _firstOpen = true;
        InitRef();
        InitButton();
        InitEvent();
    }

    public bool IsShow()
    {
        return _trans.gameObject.activeSelf;
    }
    public void Refresh()
    {
        _firstOpen = true;
    }
    private void InitRef()
    {
        _actInfo = (ActInfo_2095)ActivityManager.Instance.GetActivityInfo(2095);
        //战舰碎片部分
        _shipDebrisIcon = _trans.Find<Image>("AirShipNub/Icon/Icon");
        _shipDebrisQue = _trans.Find<Image>("AirShipNub/Icon/Qua");
        _shipDebrisName = _trans.Find<Text>("AirShipNub/DebrisText");
        _shipDebrisNum = _trans.Find<Text>("AirShipNub/Icon/Text");
        _puzzleNum = _trans.Find<Text>("AirShipNub/PuzzleText");
        _awardButton = _trans.Find<Button>("AirShipNub/GetButton");
        _shipDebrisDesc = _trans.Find<Button>("AirShipNub/Icon");
        //属性部分
        _propertyPanel = _trans.Find<Text>("AirShip/TextProperty");
        //燃料
        _fuel = _trans.Find<Text>("AirShip/TextFuel");
        _getFuel = _trans.Find<Button>("AirShip/Btn2");
        //探索和仓库按钮
        _explore = _trans.Find<Button>("AirShip/Btn3");
        _warehouse = _trans.Find<Button>("AirShip/Btn1");

        //战舰显示部分
        _shipImage = _trans.Find<Image>("AirShip/AirShipImg/AirShipImg");

        if (_shipEquipments == null)
        {
            _shipEquipments = new List<ShipEquipment2095>();
        }
        _shipEquipments.Clear();

        //三件装备
        for (int i = 1; i <= 3; i++)
        {
            ShipEquipment2095 temp = new ShipEquipment2095();
            temp.Icon = _trans.Find<Image>("AirShip/Equip0" + i + "/Image");
            temp.OpenChangeEquipPage = _trans.Find<Button>("AirShip/Equip0" + i);
            temp.StarRoot = _trans.Find("AirShip/Equip0" + i + "/Star").gameObject;
            if (temp.Star == null)
            {
                temp.Star = new List<Image>();
            }
            temp.Star.Clear();

            for (int j = 0; j < 5; j++)
            {
                Image tempStar = _trans.Find<Image>("AirShip/Equip0" + i + "/Star/Image" + j);
                tempStar.gameObject.SetActive(false);
                temp.Star.Add(tempStar);
            }

            var i1 = i;
            temp.OpenChangeEquipPage.onClick.AddListener(() =>
            {
                OpenEquipment(i1);
            });

            _shipEquipments.Add(temp);
        }
    }

    private void InitButton()
    {
        _awardButton.onClick.AddListener(ReceiveReward);
        _explore.onClick.AddListener(Explore);
        _getFuel.onClick.AddListener(On_getFuelClick);
        _warehouse.onClick.AddListener(On_warehouseClick);
        _shipDebrisDesc.onClick.AddListener(On_shipDebrisDescClick);
    }
    private void On_getFuelClick()
    {
        GetFuelPage_2095.Instance.Show();
    }
    private void On_warehouseClick()
    {
        PortWarehouse_2095.Instance.Show();
    }
    private void On_shipDebrisDescClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_shipDebrisDescDialogShowAsynCB);
    }
    private void On_shipDebrisDescDialogShowAsynCB(_D_ItemTip d)
    {
        int shipId = _actInfo.GetCurrentShip();
        var shipDebrisId = Cfg.Ship.GetPlayerShipData(shipId).itemid;
        d?.OnShow(shipDebrisId, 1, _shipDebrisDesc.transform.position);
    }

    private void InitEvent()
    {
        EventCenter.Instance.UpdateActivityUI.AddListener(UpdateActivityUI);
        EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshPuzzleNum);
    }

    private void UnInitEvent()
    {
        EventCenter.Instance.UpdateActivityUI.RemoveListener(UpdateActivityUI);
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(RefreshPuzzleNum);
    }

    private void UpdateActivityUI(int aid)
    {
        if (aid == 209501)//更换装备时刷新战舰属性
        {
            RefreshShipStatus();
        }
    }
    public void Show()
    {
        _trans.gameObject.SetActive(true);
        //只在第一次进入时加载战舰碎片,战舰模型,战舰装备和状态
        if (_firstOpen)
        {
            _firstOpen = false;
            ShowShip();
            ShowShipDebris();
            RefreshShipStatus();
        }
        RefreshPuzzleNum();
        RefreshFuel();
    }
    public void Close()
    {
        UnInitEvent();
        PortWarehouse_2095.Instance.Close();
        EquipmentPage_2095.Instance.Close();
        GetFuelPage_2095.Instance.Close();
        _trans.gameObject.SetActive(false);
    }


    private void ShowShipDebris()
    {
        //显示战舰碎片
        int shipId = _actInfo.GetCurrentShip();
        var shipDebrisId = Cfg.Ship.GetPlayerShipData(shipId).itemid;
        Cfg.Item.SetItemIcon(_shipDebrisIcon, shipDebrisId);
        _shipDebrisName.text = Cfg.Item.GetItemName(shipDebrisId);
        _shipDebrisName.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(shipDebrisId));
        _shipDebrisQue.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(shipDebrisId));
        bool checkUnlock = BuildingInfo.Instance.IsBuildingActive(BuildingPos.Base_Dock);
        if (checkUnlock)
        {
            int qua = Cfg.Ship.GetShipQua(shipId);
            switch (qua)
            {
                case 6:
                    _shipDebrisNum.text = "X" + 15.ToString();
                    break;
                case 5:
                    _shipDebrisNum.text = "X" + 35.ToString();
                    break;
                default:
                    _shipDebrisNum.text = "X" + 50.ToString();
                    break;
            }
        }
        else
        {
            _shipDebrisNum.text = "X" + 20.ToString();
        }

    }

    private void ShowShip()
    {
        //显示战舰
        int shipId = _actInfo.GetCurrentShip();
        Cfg.Ship.SetShipIcon(_shipImage, shipId);
    }
    private void RefreshFuel()
    {
        int cur = _actInfo.GetFuel();
        _fuel.text = Lang.Get("航行燃料: {0}", cur.ToString()); ;
    }
    private void RefreshPuzzleNum()
    {
        if (_puzzleNum == null)
        {
            return;
        }
        if (_actInfo.IsCompleteShipPuzzle())
        {
            _awardButton.interactable = false;
            _puzzleNum.text = Lang.Get("今日已完成战舰拼图");
        }
        else
        {
            _awardButton.interactable = true;
            var puzzlesNum = BagInfo.Instance.GetItemCount(ItemId.WarshipPuzzle2095);
            _puzzleNum.text = string.Format("{0} / 9", puzzlesNum.ToString());
        }

    }
    private void OpenEquipment(int id)
    {
        Close();
        EquipmentPage_2095.Instance.Show(id);
    }
    //探索
    private void Explore()
    {
        ExplorePage_2095.Instance.Show();
        EquipmentPage_2095.Instance.Close();
        _trans.gameObject.SetActive(false);
    }

    //刷新战舰状态
    public void RefreshShipStatus()
    {
        if (_actInfo.GetCurrentShip() <= 0)
        {
            return;
        }
        //占位数据
        var tempInfo = _actInfo.GetShipAttribute();
        int shipId = _actInfo.GetCurrentShip();


        int combatEffectiveness = tempInfo.fight;
        int atk = tempInfo.atk;
        int def = tempInfo.def;
        int hp = tempInfo.hp;

        var ship = ShipYardInfo.Instance.FindShip(shipId);
        string shipName = Cfg.Ship.GetShipName(shipId);
        string shipColor = _ColorConfig.GetQuaColorText(ship.qua);

        _propertyPanel.text =
            Lang.Get(
                "<Color={0}>[{1}] Lv.{2} {3}</Color>\n<Color=#33ccff>ATK:</Color><Color=#66ffff>{4}</Color>\n<Color=#33ccff>DEF:</Color><Color=#66ffff>{5}</Color>\n<Color=#33ccff>HP:</Color><Color=#66ffff>{6}</Color>\n<Color=#33ccff>战斗力:</Color><Color=#66ffff>{7}</Color>",
        shipColor, Cfg.Ship.GetShipTypeNameByType(ship.type),
        ship.ship_lv, shipName, atk, def, hp, combatEffectiveness);
        for (int i = 0; i < _shipEquipments.Count; i++)
        {
            var one = _shipEquipments[i];
            one.Icon.gameObject.SetActive(false);
            one.StarRoot.SetActive(false);
        }
        var shipInfo = _actInfo.GetShipAttribute();
        for (int i = 0; i < shipInfo.equip.Count; i++)
        {
            var one = shipInfo.equip[i];
            _shipEquipments[one.etype - 1].Icon.gameObject.SetActive(true);
            Cfg.Item.SetItemIcon(_shipEquipments[one.etype - 1].Icon, ItemId.WarshipWeapon2095 + one.etype - 1);
            _shipEquipments[one.etype - 1].StarRoot.SetActive(true);
            int num = 0;
            for (; num < one.elv; num++)
            {
                _shipEquipments[one.etype - 1].Star[num].gameObject.SetActive(true);
            }

            while (num < _shipEquipments[one.etype - 1].Star.Count)
            {
                _shipEquipments[one.etype - 1].Star[num].gameObject.SetActive(false);
                num++;
            }
        }

    }
    //获取奖励
    private void ReceiveReward()
    {
        //如果战舰拼图不足
        int maxPuzzlesNum = 9;
        long tempPuzzleNum = BagInfo.Instance.GetItemCount(ItemId.WarshipPuzzle2095);
        if (tempPuzzleNum < maxPuzzlesNum)
        {
            MessageManager.Show(Lang.Get("拼图数量不足"));
            return;
        }
        _actInfo.GetPuzzlesReward(RefreshPuzzleNum);
    }
}

public class EquipmentPage_2095 : Singleton<EquipmentPage_2095>
{
    private Transform _trans;
    private Transform _listViewRoot1;
    private Transform _listViewRoot2;
    private Text _title;
    private int _equipmentType;//当前显示的装备类型 
    private Button _changeEquipment;//点击切换到装备选择按钮
    private Button _upgradeEquipment;//点击切换到装备合成按钮
    private Image _changeButtonImage;
    private Image _upgradeButtonImage;
    //private Sprite _selectSprite;//装备选择按钮的颜色
    //private Sprite _unselectSprite;//装备合成按钮的颜色
    private ListView _equipmentListView1;//装备展示栏的装备
    private ListView _equipmentListView2;//合成装备展示栏的装备
    private Text _upgradeTip;//合成时的提示
    private Button _upgrade;//合成按钮
    private Button _change;//更换装备按钮
    private int _type;//当前界面的类型 0 切换装备 1 合成装备
    private ShipEquipment_2095[] _shipSelectionList;//当前选择的装备 0 更换装备时选择的装备 1 合成装备1 2 合成装备2
    private ShipEquipment_2095 _currentEquipment;//当前装备的装备

    private Button _closeButton;

    private ActInfo_2095 _actInfo;
    public void OnCreate(Transform trans)
    {
        _trans = trans;
        _trans.gameObject.SetActive(false);
        InitRef();
        InitButton();
    }
    private void InitRef()
    {

        _actInfo = (ActInfo_2095)ActivityManager.Instance.GetActivityInfo(2095);

        _title = _trans.Find<Text>("Title");

        _shipSelectionList = new ShipEquipment_2095[3]; //
        _changeEquipment = _trans.Find<Button>("Menu_01_01"); //切换到更换装备
        _upgradeEquipment = _trans.Find<Button>("Menu_02_01");//切换到合成装备
        _changeButtonImage = _changeEquipment.transform.GetComponent<Image>();//更换装备按钮颜色
        _upgradeButtonImage = _upgradeEquipment.transform.GetComponent<Image>();//合成装备按钮颜色
        _listViewRoot1 = _trans.Find("Scroll View_01");//
        _listViewRoot2 = _trans.Find("Scroll View_02");//
        _equipmentListView1 = ListView.Create<ShipEquipment_2095>(_listViewRoot1);//装备列表
        _equipmentListView2 = ListView.Create<ShipEquipment_2095>(_listViewRoot2);//合成列表

        _upgradeTip = _trans.Find<Text>("Button_02_02/Tip");//合成装备时提示
        _upgrade = _trans.Find<Button>("Button_02_02");//合成装备
        _change = _trans.Find<Button>("Button_01_02");//更换装备

        _closeButton = _trans.Find<Button>("Button");
    }
    public bool IsShow()
    {
        return _trans.gameObject.activeSelf;
    }
    private void InitButton()
    {
        _changeEquipment.onClick.AddListener(On_changeEquipmentClick);
        _upgradeEquipment.onClick.AddListener(On_upgradeEquipmentClick);
        _upgrade.onClick.AddListener(ConfirmSelection);
        _change.onClick.AddListener(ConfirmSelection);
        _closeButton.onClick.AddListener(Close);
    }
    private void On_changeEquipmentClick()
    {
        ChangeEquipmentPage(0);
    }
    private void On_upgradeEquipmentClick()
    {
        ChangeEquipmentPage(1);
    }

    public void Show(int eid)
    {
        _type = -1;
        _trans.gameObject.SetActive(true);
        _equipmentType = eid;
        int itemId = ItemId.WarshipWeapon2095 + _equipmentType - 1;
        _title.text = Cfg.Item.GetItemName(itemId);
        InitItems();
        ChangeEquipmentPage(0);
    }

    //初始化装备栏
    private void InitItems()
    {

        for (int i = 0; i < _shipSelectionList.Length; i++)
        {
            _shipSelectionList[i] = null;
        }
        _currentEquipment = null;
        //占位 加载装备 需要修改 根据 _equipmentType
        List<P_Act2095Equipment> tempEquipments = _actInfo.GetAct2095Equipments();
        var shipInfo = _actInfo.GetShipAttribute();
        _equipmentListView1.Clear();
        int _equipId = -1;
        for (int i = 0; i < shipInfo.equip.Count; i++)
        {
            var one = shipInfo.equip[i];
            if (one.etype == _equipmentType)
            {
                _equipId = one.eid;
            }
        }
        for (int i = 0; i < tempEquipments.Count; i++)
        {
            var t = tempEquipments[i];
            if (t.etype == _equipmentType)
            {
                var tempEquipment = _equipmentListView1.AddItem<ShipEquipment_2095>();
                //如果找到当前装备的装备 赋值_currentEquipment
                if (_equipId == t.eid)
                {
                    _currentEquipment = tempEquipment;
                    _currentEquipment.SetSelectTag(true);
                }
                else
                {
                    tempEquipment.SetSelectedStatus(false);
                    tempEquipment.SetSelectTag(false);
                }
                tempEquipment.OnShow(t.eid, t.etype, t.elv, SelectEquipments);
            }
        }
        _equipmentListView2.Clear();
        for (int i = 0; i < tempEquipments.Count; i++)
        {
            var t = tempEquipments[i];
            if (t.etype == _equipmentType)
            {
                var tempEquipment = _equipmentListView2.AddItem<ShipEquipment_2095>();
                tempEquipment.SetSelectedStatus(false);
                tempEquipment.SetSelectTag(false);
                tempEquipment.OnShow(t.eid, t.etype, t.elv, SelectEquipments);
            }

        }
    }
    //切换装备页面功能
    private void ChangeEquipmentPage(int type)
    {
        if (_type == type)
        {
            return;
        }
        else
        {
            _type = type;
        }
        if (_type == 0)
        {

            _listViewRoot1.gameObject.SetActive(true);
            _listViewRoot2.gameObject.SetActive(false);

            //清空上次选中
            _shipSelectionList[0]?.SetSelectedStatus(false);
            //显示正在装备的装备，因为合成不显示
            //_currentEquipment?.gameObject.SetActive(true);
            _currentEquipment?.SetSelectedStatus(true);
            _shipSelectionList[0] = _currentEquipment;
            UIHelper.SetImageSprite(_changeButtonImage, "Button/btn_801");
            UIHelper.SetImageSprite(_upgradeButtonImage, "Button/btn_802");
            _change.gameObject.SetActive(true);
            _upgrade.gameObject.SetActive(false);
            _upgradeTip.gameObject.SetActive(false);
        }
        else
        {
            _listViewRoot1.gameObject.SetActive(false);
            _listViewRoot2.gameObject.SetActive(true);


            int selectId = -1;
            //不显示正在装备的装备
            if (_currentEquipment != null)
            {
                selectId = _currentEquipment.GetId();
            }
            for (int i = 0; i < _equipmentListView2._listItems.Count; i++)
            {
                var one = _equipmentListView2._listItems[i];
                ShipEquipment_2095 tempItem = (ShipEquipment_2095)one;
                tempItem.gameObject.SetActive(tempItem.GetId() != selectId);
            }
            //清空上次选中的装备
            for (int i = 1; i < _shipSelectionList.Length; i++)
            {
                if (_shipSelectionList[i] != null)
                {
                    _shipSelectionList[i].SetSelectedStatus(false);
                    _shipSelectionList[i] = null;
                }
            }
            UIHelper.SetImageSprite(_changeButtonImage, "Button/btn_802");
            UIHelper.SetImageSprite(_upgradeButtonImage, "Button/btn_801");
            _change.gameObject.SetActive(false);
            _upgrade.gameObject.SetActive(true);
            _upgradeTip.gameObject.SetActive(true);
            _upgradeTip.text = Lang.Get("选择两个相同等级的装备进行升级 {0} / 2", 0);
        }
    }

    private void SelectEquipments(ShipEquipment_2095 nextEquipment)
    {
        //判断是装备选择还是装备合成， 0是装备选择，1是装备合成
        if (_type == 0)
        {
            if (_shipSelectionList[0] == null)//没穿装备
            {
                _shipSelectionList[0] = nextEquipment;
                _shipSelectionList[0].SetSelectedStatus(true);
                return;
            }
            if (nextEquipment.GetId() == _shipSelectionList[0].GetId())
            {
                //如果点击的装备还是原来的装备,取消选定
                _shipSelectionList[0].SetSelectedStatus(false);
                _shipSelectionList[0] = null;
                return;
            }
            else
            {
                //如果点击的装备是其他装备，则更改当前选择的装备
                _shipSelectionList[0].SetSelectedStatus(false);
                _shipSelectionList[0] = nextEquipment;
                _shipSelectionList[0].SetSelectedStatus(true);
            }
        }
        else
        {

            if (nextEquipment.GetLv() >= 4)
            {
                MessageManager.Show(Lang.Get("当前装备已升至最高级!"));
                return;
            }
            if (_shipSelectionList[1] == null)//第一个装备不存在时，选择合成第一个装备
            {
                _shipSelectionList[1] = nextEquipment;
                _shipSelectionList[1].SetSelectedStatus(true);
            }
            else//已经存在第一个选中装备的情况下，选择第二个装备
            {
                //如果之前没有选择第二个装备
                if (_shipSelectionList[2] == null)
                {
                    if (nextEquipment.GetId() == _shipSelectionList[1].GetId())
                    {
                        //如果点击的装备还是原来的装备,取消选定
                        _shipSelectionList[1].SetSelectedStatus(false);
                        _shipSelectionList[1] = null;
                        RefreshUpgradeTip();
                        return;
                    }
                    //如果选择的装备等级不同
                    if (_shipSelectionList[1].GetLv() != nextEquipment.GetLv())
                    {
                        MessageManager.Show(Lang.Get("请选择等级相同的装备"));
                        return;
                    }
                    else
                    {
                        //更新第二个选中框
                        _shipSelectionList[2] = nextEquipment;
                        _shipSelectionList[2].SetSelectedStatus(true);
                    }
                }
                else
                {
                    //如果第二个装备是选择过的装备，则取消选中，如果取消的是第一个，另外把第二个作为第一个
                    for (int i = 1; i < _shipSelectionList.Length; i++)
                    {
                        if (_shipSelectionList[i].GetId() == nextEquipment.GetId())
                        {
                            _shipSelectionList[i].SetSelectedStatus(false);
                            _shipSelectionList[i] = null;
                            if (i == 1)
                            {
                                _shipSelectionList[1] = _shipSelectionList[2];
                                _shipSelectionList[2] = null;
                            }

                            RefreshUpgradeTip();
                            return;
                        }
                    }
                }
            }

        }
        RefreshUpgradeTip();
    }

    private void RefreshUpgradeTip()
    {
        int num = 0;
        for (int i = 1; i < _shipSelectionList.Length; i++)
        {
            if (_shipSelectionList[i] != null)
            {
                num++;
            }
        }
        _upgradeTip.text = Lang.Get("选择两个相同等级的装备进行升级 {0} / 2", num);
    }
    private void ConfirmSelection()
    {
        if (_type == 0)
        {
            //没有装备且选择的装备为空时不能装备
            if ((_shipSelectionList[0] == null && _currentEquipment == null) || _shipSelectionList[0] == _currentEquipment)
            {
                MessageManager.Show(Lang.Get("请选择要更换的装备!"));
                return;
            }
            var tempId = _shipSelectionList[0]?.GetId() ?? _currentEquipment.GetId();
            _actInfo.ChangeEquipment(tempId, OnChangeEquipmentCB);
        }
        else
        {
            if (_shipSelectionList[1] == null || _shipSelectionList[2] == null)
            {
                string mag = Lang.Get("请选择要合成的装备!");
                Alert.Ok(mag);
                return;
            }
            _actInfo.SyntheticEquipment(_shipSelectionList[1].GetId(), _shipSelectionList[2].GetId(), OnSyntheticEquipmentCB);
        }
    }
    private void OnChangeEquipmentCB()
    {
        MessageManager.Show(Lang.Get("更换装备完成"));
        //刷新战舰状态界面属性
        EventCenter.Instance.UpdateActivityUI.Broadcast(209501);
        //更新当前装备

        //更改现有装备的选中状态
        _shipSelectionList[0]?.SetSelectTag(true);

        //取消原来装备的选中状态
        _currentEquipment?.SetSelectTag(false);
        //赋值现有装备
        _currentEquipment = _shipSelectionList[0];
    }

    private void OnSyntheticEquipmentCB()
    {
        MessageManager.Show(Lang.Get("合成装备完成"));
        //重新加载装备
        InitItems();
        _type = -1;
        ChangeEquipmentPage(1);
    }

    public void Close()
    {
        _trans.gameObject.SetActive(false);
        if (_actInfo.GetCurrentShip() > 0)
        {
            ShipStatus_2095.Instance.Show();
        }
    }
    public void OnDestroy()
    {
        _actInfo = null;
    }
}

public class ExplorePage_2095 : Singleton<ExplorePage_2095>
{

    private Transform _trans;

    private Transform _listViewRoot;
    private ListView _map;
    //单元地图
    //private List<Image> _unitMap;
    private Image _portImage;
    private int _rowNum;
    private int _colNum;
    //移动按钮
    private Button _rightButton;//右移
    private Button _leftButton;//左移
    private Button _upButton;//上移
    private Button _downButton;//下移

    private Button _return2Port;//返回到港口
    private Button _return2MainInterface;//返回主界面
    private Button _openCargoWarehouse;//打开仓库
    private Text _fuelOnMapPage;//燃料显示

    private Image _ship;//舰体本身
    private Tween _moveTween; //移动动画

    private ScrollRect _mapScrollRect;
    private RectTransform _mapScrollRectTransform;
    private RectTransform _mapViewportRectTransform;
    private RectTransform _mapContentRectTransform;

    private RectTransform _shipRectTransform;

    //奖励
    struct Reward2095
    {
        public GameObject _item;
        public Image icon;
        public Image qua;
        public Button button;
        public Text num;
        public int rewardId;
    }
    //事件界面
    private GameObject _eventPage;
    private Image _eventImage;
    private Text _eventName;//事件名称
    private Text _monsterDesc;//怪物描述
    private Text _loseTip;
    private Text _attackButtonText;//出击按钮文本
    private Button _attackButton;//攻击/探索按钮
    private Button _retreatButton;//撤退按钮
    private Text _retreatButtonText;//撤退按钮文本
    private Button _okButton;//胜利后确认按钮
    private GameObject _rewardPanel;
    private Reward2095[] _rewards;

    private bool _firstOpen;

    private ActInfo_2095 _actInfo;

    //两个选中框
    private MapGrid_2095[] _selectMapGrids;
    public void OnCreate(Transform trans)
    {
        _trans = trans;
        _trans.gameObject.SetActive(false);
        InitRef();
        InitButton();
    }
    public bool IsShow()
    {
        return _trans.gameObject.activeSelf;
    }

    public void Refresh()
    {
        _firstOpen = true;
    }

    private void InitRef()
    {
        //初始化地图图片
        _actInfo = (ActInfo_2095)ActivityManager.Instance.GetActivityInfo(2095);


        _rowNum = 20;
        _colNum = 20;

        _listViewRoot = _trans.Find("Scroll View");
        _map = ListView.Create<MapGrid_2095>(_listViewRoot);
        _portImage = _trans.Find<Image>("Sss");
        //初始化移动按钮
        _rightButton = _trans.Find<Button>("Button02_04");
        _leftButton = _trans.Find<Button>("Button02_01");
        _upButton = _trans.Find<Button>("Button02_02");
        _downButton = _trans.Find<Button>("Button02_03");
        //初始化上方按钮
        _return2Port = _trans.Find<Button>("Button01_02");
        _return2MainInterface = _trans.Find<Button>("Button01_01");
        _openCargoWarehouse = _trans.Find<Button>("Button01_03");
        //初始化燃油
        _fuelOnMapPage = _trans.Find<Text>("Text/Text");
        //
        _ship = _trans.Find<Image>("ship");//初始化船
        _shipRectTransform = _ship.GetComponent<RectTransform>();
        _mapScrollRect = _trans.Find<ScrollRect>("Scroll View");
        _mapScrollRectTransform = _mapScrollRect.GetComponent<RectTransform>();
        _mapViewportRectTransform = _mapScrollRect.transform.Find<RectTransform>("Viewport");
        _mapContentRectTransform = _mapScrollRect.transform.Find<RectTransform>("Viewport/Content");
        //初始化事件面板
        _eventPage = _trans.Find("Tips_N1").gameObject;
        _eventImage = _trans.Find<Image>("Tips_N1/Icon/Image");
        _eventName = _trans.Find<Text>("Tips_N1/Name");
        _monsterDesc = _trans.Find<Text>("Tips_N1/Text");
        _loseTip = _trans.Find<Text>("Tips_N1/Text_Lose");
        _attackButtonText = _trans.Find<Text>("Tips_N1/Image/ButtonList/btn_attack/Text");
        _attackButton = _trans.Find<Button>("Tips_N1/Image/ButtonList/btn_attack");
        _retreatButton = _trans.Find<Button>("Tips_N1/Image/ButtonList/btn_retreat");
        _rewardPanel = _trans.Find("Tips_N1/Rewards").gameObject;
        _okButton = _trans.Find<Button>("Tips_N1/Image/ButtonList/btn_ok");
        _retreatButtonText = _retreatButton.transform.Find<Text>("Text");

        _selectMapGrids = new MapGrid_2095[2];

        //初始化奖励
        _rewards = new Reward2095[10];


        for (int i = 1; i <= 2; i++)
        {
            for (int j = 1; j <= 5; j++)
            {
                int num = (i - 1) * 5 + j - 1;
                _rewards[num]._item = _trans.Find("Tips_N1/Rewards/ItemLine" + i + "/0" + j).gameObject;
                _rewards[num].icon = _trans.Find<Image>("Tips_N1/Rewards/ItemLine" + i + "/0" + j + "/Image");
                _rewards[num].num = _trans.Find<Text>("Tips_N1/Rewards/ItemLine" + i + "/0" + j + "/Text");
                _rewards[num].button = _rewards[num].icon.transform.GetComponent<Button>();
                _rewards[num].qua = _rewards[num]._item.GetComponent<Image>();
                var i1 = num;
                _rewards[num].button.onClick.AddListener(() =>
                {
                    DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(_rewards[i1].rewardId, 1, _rewards[i1].button.transform.position); });
                });
            }
        }
        _firstOpen = true;

    }

    private void InitButton()
    {
        _rightButton.onClick.AddListener(On_rightButtonClick);
        _leftButton.onClick.AddListener(On_leftButtonClick);
        _upButton.onClick.AddListener(On_upButtonClick);
        _downButton.onClick.AddListener(On_downButtonClick);
        _return2Port.onClick.AddListener(Return2Port);
        _return2MainInterface.onClick.AddListener(On_return2MainInterfaceClick);
        _openCargoWarehouse.onClick.AddListener(PortWarehouse_2095.Instance.Show);
        _attackButton.onClick.AddListener(Attack);
        _retreatButton.onClick.AddListener(On_retreatButtonClick);
        _okButton.onClick.AddListener(On_okButtonClick);
    }
    private void On_rightButtonClick()
    {
        Move(0, 1);
    }
    private void On_leftButtonClick()
    {
        Move(0, -1);
    }
    private void On_upButtonClick()
    {
        Move(1, 1);
    }
    private void On_downButtonClick()
    {
        Move(1, -1);
    }
    private void On_return2MainInterfaceClick()
    {
        Close();
        ShipStatus_2095.Instance.Show();
    }
    private void On_retreatButtonClick()
    {
        _eventPage.SetActive(false);
    }
    private void On_okButtonClick()
    {
        _eventPage.SetActive(false);
    }

    public void Show()
    {
        if (_firstOpen)
        {
            InitMap();
            InitShipPosition();
            ShowShip();
            _firstOpen = false;
        }
        RefreshFuel();
        MoveMap(Vector3.zero);
        _trans.gameObject.SetActive(true);
    }

    private void ShowShip()
    {
        int shipId = _actInfo.GetCurrentShip();
        Cfg.Ship.SetShipIcon(_ship, shipId);
    }
    //返回港口
    private void Return2Port()
    {
        int currentPos = _actInfo.GetShipGenerationPosition();
        _actInfo.MoveShip(currentPos, OnMoveShipCB);
    }
    private void OnMoveShipCB(List<MapInfo2095> newPosition)
    {
        int currentPos = _actInfo.GetShipGenerationPosition();
        int[] pos = new int[] { currentPos % _colNum, currentPos / _colNum };
        var tempPos = new Vector3((pos[0] + 1) * 112.0f - 56.0f, (pos[1] + 1) * 112.0f - 56.0f);
        _ship.transform.localPosition = new Vector3(tempPos.x, tempPos.y, _ship.transform.localPosition.z);
        MoveMap(Vector3.zero);
        EncounterEvent(currentPos);
    }

    //初始化地图 仓库 可见位置 不可见位置为迷雾
    private void InitMap()
    {
        int num = _rowNum * _colNum;
        if (_map.ItemCount <= 0)
        {
            _map.Clear();

            for (int i = 0; i < num; i++)
            {
                _map.AddItem<MapGrid_2095>().OnShow(i, OnAddItemShowCB);
            }
        }
        else
        {
            for (int i = 0; i < num; i++)
            {
                var tempGrid = _map._listItems[i] as MapGrid_2095;
                tempGrid.Refresh();
            }
        }
    }
    private void OnAddItemShowCB(int pos, MapGrid_2095 select)
    {
        SelectMapGrid(select);
        EncounterEvent(pos);
    }


    private void SelectMapGrid(MapGrid_2095 nextMap)
    {
        _selectMapGrids[0] = _selectMapGrids[1];
        _selectMapGrids[1] = nextMap;


        _selectMapGrids[0]?.Select(false);
        _selectMapGrids[1]?.Select(true);

    }
    private void InitShipPosition()
    {

        //初始化港口位置
        _portImage.transform.SetParent(_mapContentRectTransform.gameObject.transform);
        _portImage.transform.localPosition = new Vector3(113.0f, 115.0f, 0);
        //从本地缓存中获取
        _ship.transform.SetParent(_mapContentRectTransform.gameObject.transform);
        int currentPos = _actInfo.GetCurrentShipPosition();
        int[] pos = new int[] { currentPos % _colNum, currentPos / _colNum };
        var tempPos = new Vector3((pos[0] + 1) * 112.0f - 56.0f, (pos[1] + 1) * 112.0f - 56.0f);
        _ship.transform.localPosition = new Vector3(tempPos.x, tempPos.y, _ship.transform.localPosition.z);
        EncounterEvent(currentPos);
    }
    private void Move(int axis, int direction)//0 x轴 1 y轴
    {
        //读取缓存获得
        int currentPos = _actInfo.GetCurrentShipPosition();
        int[] pos = new int[] { currentPos % _colNum, currentPos / _colNum };
        pos[axis] += direction;
        currentPos = pos[1] * _colNum + pos[0];
        if (pos[axis] >= _colNum || pos[axis] < 0)
        {
            return;
        }
        var opcode = PromptOpcode.Act2095MoveFuel;
        bool prompt = PromptInfo.Instance.GetValue(opcode);
        if (prompt)
        {
            _AlertWithPrompt.YesNo(Lang.Get("点击确认消耗1点航行燃料进行移动!"), d =>
            {
                d.SetYesCallbackWithPrompt(() =>
                {
                    PromptInfo.Instance.SetPrompt(opcode, d.setPrompt);
                    MoveRequest(currentPos, pos);
                    d.Close();
                });
            }, 1);
        }
        else
        {
            MoveRequest(currentPos, pos);
        }
    }

    private void MoveRequest(int currentPos, int[] pos)
    {
        if (_actInfo.GetFuel() < 1)
        {
            MessageManager.Show(Lang.Get("航行燃料不足！"));
            return;
        }
        _actInfo.MoveShip(currentPos, (refreshPos) =>
        {
            _eventPage.SetActive(false);//关闭事件显示
            RefreshFuel();
            //刷新迷雾
            RefreshFogAfterMove(refreshPos);
            //播放移动动画
            AllowMovement(false);
            var targetPos = new Vector3((pos[0] + 1) * 112.0f - 56.0f, (pos[1] + 1) * 112.0f - 56.0f); ;
            var step = new Vector3(targetPos.x - _ship.transform.localPosition.x,
                targetPos.y - _ship.transform.localPosition.y, 0);
            _moveTween = _ship.transform.DOLocalMove(targetPos, 0.8f);
            MoveMap(step);
            _moveTween.OnComplete(() =>
            {
                AllowMovement(true);
                EncounterEvent(currentPos);
            });
        });

    }
    //地图跟随当前战舰位置移动
    private void MoveMap(Vector3 step)
    {
        Vector3 shipLocation = _mapScrollRectTransform.InverseTransformPoint(ConvertLocalPosToWorldPos(_shipRectTransform, step));
        Vector3 mapTargetLocation =
            _mapScrollRectTransform.InverseTransformPoint(ConvertLocalPosToWorldPos(_mapViewportRectTransform, Vector3.zero));

        Vector3 diff = mapTargetLocation - shipLocation;
        Vector3 bias = new Vector3(diff.x / (2242.0f - _mapViewportRectTransform.rect.width),
                diff.y / (2242.0f - _mapViewportRectTransform.rect.height));
        Vector3 targetNormalizedPosition = new Vector3(_mapScrollRect.normalizedPosition.x, _mapScrollRect.normalizedPosition.y) - bias;
        targetNormalizedPosition.x = Mathf.Clamp01(targetNormalizedPosition.x);
        targetNormalizedPosition.y = Mathf.Clamp01(targetNormalizedPosition.y);
        targetNormalizedPosition.x = targetNormalizedPosition.x * 1568.0f;
        targetNormalizedPosition.y = targetNormalizedPosition.y * 1568.0f + 674.0f;
        _mapContentRectTransform.transform.DOLocalMove(new Vector3(-targetNormalizedPosition.x, -targetNormalizedPosition.y), 0.8f);
    }
    //将移动后的位置 从viewport转换到世界坐标 中心点改为物体中心
    private Vector3 ConvertLocalPosToWorldPos(RectTransform target, Vector3 step)
    {
        var pivotOffset = new Vector3(
            (0.5f - target.pivot.x) * target.rect.size.x,
            (0.5f - target.pivot.y) * target.rect.size.y,
            0f);

        var localPosition = target.localPosition + step + pivotOffset;

        return target.parent.TransformPoint(localPosition);
    }
    private void RefreshFuel()
    {
        int cur = _actInfo.GetFuel();
        _fuelOnMapPage.text = Lang.Get("航行燃料: {0}", cur.ToString());
    }
    //移动后刷新迷雾
    private void RefreshFogAfterMove(List<MapInfo2095> refreshPos)
    {

        for (int i = 0; i < refreshPos.Count; i++)
        {
            var one = refreshPos[i];
            MapGrid_2095 tempGrid = _map._listItems[one.map_id] as MapGrid_2095;
            tempGrid.Refresh();
        }
    }
    //遭遇事件 
    private void EncounterEvent(int currentPos)
    {
        int eventId = _actInfo.GetGridEventId(currentPos);

        if (eventId == 0 || eventId == 1)
        {
            return;
        }
        var eventInfo = Cfg.Activity2095.GetEventInfoById(eventId);
        int eventType = eventInfo.e_type;
        int shipPosition = _actInfo.GetCurrentShipPosition();
        bool showButton = currentPos == shipPosition;
        _monsterDesc.gameObject.SetActive(true);
        _monsterDesc.color = new Color(1.0f, 0.92f, 0.016f);
        if (eventType == 2)
        {
            _attackButtonText.text = Lang.Get("进攻");
            _retreatButtonText.text = Lang.Get("撤退");
            int possibility = Cfg.Activity2095.GetFragPossibilityById(eventId);
            if (possibility == 0)
            {
                _monsterDesc.text = Lang.Get("<Color=#ffcc00>战斗力: {0}</Color>", eventInfo.fight);
            }
            else
            {
                _monsterDesc.text = !_actInfo.IsCompleteShipPuzzle() ? Lang.Get("<Color=#ffcc00>战斗力: {0}</Color>\n{1}%概率获得战舰拼图", eventInfo.fight, possibility) : Lang.Get("<Color=#ffcc00>战斗力: {0}</Color>\n今日已完成战舰拼图", eventInfo.fight);
            }
        }
        else if (eventType == 3)
        {
            int possibility = Cfg.Activity2095.GetFragPossibilityById(eventId);
            if (possibility == 0)
            {
                _monsterDesc.gameObject.SetActive(false);
            }
            else
            {
                _monsterDesc.text = Lang.Get("{0}%概率获得战舰拼图", possibility);
            }
            _attackButtonText.text = Lang.Get("探索");
            _retreatButtonText.text = Lang.Get("离开");
        }
        UIHelper.SetImageSprite(_eventImage, "Item/" + eventInfo.icon);

        _attackButton.gameObject.SetActive(showButton);
        _retreatButton.gameObject.SetActive(showButton);
        _okButton.gameObject.SetActive(!showButton);
        _loseTip.gameObject.SetActive(false);
        _eventPage.SetActive(true);
        _eventName.gameObject.SetActive(true);
        _eventName.text = eventInfo.e_name;
        //刷新奖励 空格子隐藏
        _rewardPanel.SetActive(true);
        string allRewards;
        if (eventInfo.rand_reward.Length > 0)
        {
            allRewards = eventInfo.weight_reward + "," + eventInfo.rand_reward;
        }
        else
        {
            allRewards = eventInfo.weight_reward;
        }
        P_Item[] tempItems = GlobalUtils.ParseItem(allRewards);
        RefreshRewards(tempItems, 0);
    }
    private void Attack()
    {
        //判断是否为怪物
        int currentPos = _actInfo.GetCurrentShipPosition();
        int eventId = _actInfo.GetGridEventId(currentPos);
        int eventType = Cfg.Activity2095.GetEventTypeById(eventId);
        if (eventType == 2)
        {
            int tempEnemy = Cfg.Activity2095.GetEventInfoById(eventId).fight;
            var tempShipInfo = _actInfo.GetShipAttribute();
            if (tempEnemy > tempShipInfo.fight)
            {
                var tempAlert = Alert.YesNo(Lang.Get("当前战力不足。强行进攻失败将会失去10点航行燃料！"));
                tempAlert.SetYesCallback(() =>
                {
                    tempAlert.Close();
                    _actInfo.Explore(currentPos, (success, rewards) =>
                    {
                        ShowResult(eventId, success, rewards);
                    });
                });
                tempAlert.SetNoCallback(tempAlert.Close);
                return;
            }
        }
        _actInfo.Explore(currentPos, (success, rewards) =>
        {
            ShowResult(eventId, success, rewards);
            MapGrid_2095 tempGrid = _map._listItems[currentPos] as MapGrid_2095;
            tempGrid.Refresh();
        });
    }
    //是否移动
    private void AllowMovement(bool movable)
    {
        _rightButton.interactable = movable;
        _leftButton.interactable = movable;
        _upButton.interactable = movable;
        _downButton.interactable = movable;
    }
    //显示结果
    private void ShowResult(int eventId, int isSuccess, P_Item[] rewards)
    {
        _eventName.gameObject.SetActive(false);
        _attackButton.gameObject.SetActive(false);
        _retreatButton.gameObject.SetActive(false);
        _okButton.gameObject.SetActive(true);
        _monsterDesc.gameObject.SetActive(true);
        string msg;
        int eventType = Cfg.Activity2095.GetEventTypeById(eventId);
        Color color = new Color(1.0f, 0.92f, 0.016f);
        if (eventType == 2)//怪物
        {

            if (isSuccess == 1)
            {
                msg = Lang.Get("战斗成功");
                _loseTip.gameObject.SetActive(false);
                _rewardPanel.SetActive(true);
                var tempMap = _map._listItems[_actInfo.GetCurrentShipPosition()] as MapGrid_2095;
                tempMap.Refresh();
            }
            else
            {
                msg = Lang.Get("战斗失败");
                color = new Color(1.0f, 0, 0);
                RefreshFuel();
                _loseTip.gameObject.SetActive(true);
                _rewardPanel.SetActive(false);
            }
        }
        else
        {
            msg = Lang.Get("探索成功");
            var tempMap = _map._listItems[_actInfo.GetCurrentShipPosition()] as MapGrid_2095;
            tempMap.Refresh();
            _rewardPanel.SetActive(true);
        }
        _monsterDesc.text = msg;
        _monsterDesc.color = color;
        RefreshRewards(rewards, 1);
    }
    //刷新奖励
    private void RefreshRewards(P_Item[] tempItems, int type)//0 展示 1 获取
    {
        int num = 0;
        for (int i = 0; i < tempItems.Length; i++)
        {
            var one = tempItems[i];
            if (one.id == ItemId.WarshipPuzzle2095)//如果奖励中有战舰拼图
            {
                if (type == 0)//不在图标上展示
                {
                    continue;
                }
                else//展示获得时跳出弹窗
                {
                    Alert.Ok(Lang.Get("获得战舰拼图*{0}", one.Num));
                    continue;
                }
            }
            _rewards[num]._item.SetActive(true);
            Cfg.Item.SetItemIcon(_rewards[num].icon, one.id);
            _rewards[num].rewardId = one.id;
            _rewards[num].num.text = one.Num.ToString();
            _rewards[num].qua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(one.id));
            num++;
        }
        while (num < _rewards.Length)
        {
            _rewards[num]._item.SetActive(false);
            num++;
        }
    }
    public void Close()
    {
        _moveTween.Complete();
        _trans.gameObject.SetActive(false);
        _eventPage.SetActive(false);
    }
    public void OnDestroy()
    {
        _selectMapGrids = null;
        _rewards = null;
        _actInfo = null;
    }
}

public class PortWarehouse_2095 : Singleton<PortWarehouse_2095>
{
    //港口仓库
    private Transform _trans;//港口仓库
    private Transform _portWarehouseRoot;
    private ListView _portWarehouseList;
    private Button _getAllWarehouseRewardsButton;//获取港口仓库奖励
    private Button _closeButton;
    private ActInfo_2095 _actInfo;
    public void OnCreate(Transform trans)
    {
        _trans = trans;
        _trans.gameObject.SetActive(false);
        InitRef();
        InitButton();
    }
    public bool IsShow()
    {
        return _trans.gameObject.activeSelf;
    }
    private void InitRef()
    {
        _actInfo = (ActInfo_2095)ActivityManager.Instance.GetActivityInfo(2095);
        _portWarehouseRoot = _trans.Find("Scroll View");
        _portWarehouseList = ListView.Create<PortWarehouseReward_2095>(_portWarehouseRoot);
        _getAllWarehouseRewardsButton = _trans.Find<Button>("getButton");
        _closeButton = _trans.Find<Button>("closeButton");
    }
    private void InitButton()
    {
        _getAllWarehouseRewardsButton.onClick.AddListener(ReceiveWarehouseReward);
        _closeButton.onClick.AddListener(Close);
    }
    private void ReceiveWarehouseReward()
    {
        if (_actInfo.GetWarehouseRewardsList() == "")
        {
            MessageManager.Show(Lang.Get("仓库里没有货物可以领取哦"));
            return;
        }
        _actInfo.ReceiveWarehouseRewards(_portWarehouseList.Clear);
    }
    //打开港口仓库
    public void Show()
    {
        _trans.gameObject.SetActive(true);
        _portWarehouseList.Clear();
        P_Item[] tempReward = GlobalUtils.ParseItem(_actInfo.GetWarehouseRewardsList());
        for (int i = 0; i < tempReward.Length; i++)
        {
            var one = tempReward[i];
            _portWarehouseList.AddItem<PortWarehouseReward_2095>().OnShow(one);
        }
    }
    //关闭港口仓库
    public void Close()
    {
        _trans.gameObject.SetActive(false);
    }
    public void OnDestroy()
    {
        _actInfo = null;
    }
}
//装备块
public class GetFuelPage_2095 : Singleton<GetFuelPage_2095>
{
    //获取燃料界面
    private Transform _trans;
    private Transform _listViewRoot;
    private ListView _fuelListView;
    private ActInfo_2095 _actInfo;
    private Button _closeButton;

    public void OnCreate(Transform transform)
    {
        _trans = transform;
        _trans.gameObject.SetActive(false);
        InitRef();
        InitButton();
    }
    public bool IsShow()
    {
        return _trans.gameObject.activeSelf;
    }
    public void Show()
    {
        _trans.gameObject.SetActive(true);
        _fuelListView.Clear();

        var tempTaskInfo = _actInfo.GetAct2095Missions();

        for (int i = 0; i < tempTaskInfo.Count; i++)
        {
            var one = tempTaskInfo[i];
            var task = Cfg.Activity2095.GetAct2095Task(one.tid);
            _fuelListView.AddItem<FuelTask_2095>().OnShow(task, one);
        }
    }

    private void InitRef()
    {
        _actInfo = (ActInfo_2095)ActivityManager.Instance.GetActivityInfo(2095);
        //燃料获取部分
        _listViewRoot = _trans.Find("Scroll View");
        _fuelListView = ListView.Create<FuelTask_2095>(_listViewRoot);
        _closeButton = _trans.Find<Button>("Button");
    }

    private void InitButton()
    {
        _closeButton.onClick.AddListener(Close);
    }
    public void Close()
    {
        _trans.gameObject.SetActive(false);
    }
    public void OnDestroy()
    {
        _actInfo = null;
    }
}
public class ShipEquipment_2095 : ListItem
{
    private Image _icon;
    private Button _button;
    private GameObject _selectBorder;
    private GameObject _selectMask;
    private GameObject _selectTag;
    private GameObject _selectImage;
    private List<Image> _star;
    private Action<ShipEquipment_2095> _selectEquipment;
    private int _id;
    private int _lv;
    public override void OnCreate()
    {
        InitRef();
    }
    public int GetLv()
    {
        return _lv;
    }
    public int GetId()
    {
        return _id;
    }
    public void SetSelectedStatus(bool check)
    {
        _selectBorder.SetActive(check);
        _selectMask.SetActive(check);
        _selectImage.SetActive(check);
    }
    //设置装备为穿戴状态
    public void SetSelectTag(bool select)
    {
        _selectTag.SetActive(select);
    }
    private void InitRef()
    {
        _icon = transform.Find<Image>("Icon");
        _selectTag = transform.Find("tag").gameObject;
        _button = transform.GetComponent<Button>();
        _selectBorder = transform.Find("ImageBorder").gameObject;
        _selectMask = transform.Find("Mask").gameObject;
        _selectImage = transform.Find("Image").gameObject;
        _button.onClick.AddListener(SelectItem);
        _star = new List<Image>();
        for (int i = 0; i < 5; i++)
        {
            Image star = transform.Find<Image>("Star/Image" + i);
            star.gameObject.SetActive(false);
            _star.Add(star);
        }
    }
    public void OnShow(int id, int type, int lv, Action<ShipEquipment_2095> selectEquipment)
    {
        _id = id;
        _lv = lv;
        int itemId = ItemId.WarshipWeapon2095 + type - 1;
        _selectEquipment = selectEquipment;
        Cfg.Item.SetItemIcon(_icon, itemId);

        _lv = lv > 5 ? 5 : _lv;

        for (int i = 0; i < _lv; i++)
        {
            _star[i].gameObject.SetActive(true);
        }
        for (int i = _lv; i < 5; i++)
        {
            _star[i].gameObject.SetActive(false);
        }
    }
    private void SelectItem()
    {
        _selectEquipment(this);
    }
}
//仓库奖励
public class PortWarehouseReward_2095 : ListItem
{
    private Image _icon;
    private Button _button;
    private Text _num;
    private int _itemId;
    public override void OnCreate()
    {
        _icon = transform.Find<Image>("Image");
        _button = transform.GetComponent<Button>();
        _num = transform.Find<Text>("Text");

        _button.onClick.AddListener(On_buttonClick);
    }
    private void On_buttonClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_buttonDialogShowAsynCB);
    }
    private void On_buttonDialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(_itemId, 1, _button.transform.position);
    }

    public void OnShow(P_Item item)
    {
        _itemId = item.id;
        Cfg.Item.SetItemIcon(_icon, _itemId);
        _num.text = "X" + item.Num.ToString();
    }
}
//任务
public class FuelTask_2095 : ListItem
{
    private Text _name;
    private Text _reward;
    private Button _goButton;
    private Image _completeTip;
    private string _mission_click;

    public override void OnCreate()
    {
        InitRef();

    }
    private void InitRef()
    {
        _name = transform.Find<Text>("name");
        _reward = transform.Find<Text>("count");
        _goButton = transform.Find<Button>("goButton");
        _completeTip = transform.Find<Image>("Button");
        _goButton.onClick.AddListener(On_goButtonClick);
    }
    private void On_goButtonClick()
    {
        MissionUtils.DoCustomFlow(_mission_click);
    }
    public void OnShow(cfg_act_2095_task task, P_Act2095Mission taskInfo)
    {
        //通过id读表获取任务初始化任务奖励
        string name = Lang.Get("{0}(<Color=#00ff33>{1}</Color>/{2})", task.name, taskInfo.do_number,
            task.need_count);
        _name.text = name;
        //rewa.text = task.need_count.ToString();
        _reward.text = Lang.Get("+{0}航行燃料", task.reward);

        _goButton.gameObject.SetActive(taskInfo.finished == 0);
        _completeTip.gameObject.SetActive(taskInfo.finished == 1);

        _mission_click = task.click;

        if (string.IsNullOrEmpty(_mission_click))
        {
            _goButton.gameObject.SetActive(false);
        }

    }
}

public class MapGrid_2095 : ListItem
{

    private GameObject _frog;
    private GameObject _event;
    private Image _frogGrid;//迷雾
    private Image _eventGird;//事件
    private Button _button;
    private Image _mask;
    private ActInfo_2095 _actInfo;
    private int _position;
    private Action<int, MapGrid_2095> _encounterEvent;//事件显示
    public override void OnCreate()
    {
        InitRef();
        InitButton();
    }

    private void InitRef()
    {
        _actInfo = (ActInfo_2095)ActivityManager.Instance.GetActivityInfo(2095);
        _frog = transform.Find("frog").gameObject;
        _event = transform.Find("Event").gameObject;
        _frogGrid = transform.Find<Image>("frog/frog");
        _eventGird = transform.Find<Image>("Event/Event");
        _mask = transform.Find<Image>("Mask");
        _button = transform.GetComponent<Button>();
    }

    private void InitButton()
    {
        _button.onClick.AddListener(On_buttonClick);
    }
    private void On_buttonClick()
    {
        if (_actInfo.IsThereFog(_position))
        {
            return;
        }
        _encounterEvent(_position, this);
    }

    public void OnShow(int position, Action<int, MapGrid_2095> encounterEvent)
    {
        _position = position;
        _encounterEvent = encounterEvent;
        RefreshOneGrid();
    }

    public void Refresh()
    {
        RefreshOneGrid();
    }

    public void Select(bool select)
    {
        _mask.gameObject.SetActive(select);
    }
    private void RefreshOneGrid()
    {
        int eventId = _actInfo.GetGridEventId(_position);
        cfg_act_2095_event eventInfo = null;
        int eventType;
        if (eventId == 1 || eventId == 0)
        {
            eventType = eventId;
        }
        else
        {
            eventInfo = Cfg.Activity2095.GetEventInfoById(eventId);
            eventType = eventInfo.e_type;
        }

        if (eventType == 1)//是港口
        {
            return;
        }
        if (_actInfo.IsThereFog(_position))//有雾
        {
            _frog.SetActive(true);
            _event.SetActive(false);
        }
        else//没雾
        {
            _frog.SetActive(false);
            if (eventType == 2 || eventType == 3)
            {
                _event.SetActive(true);
                UIHelper.SetImageSprite(_eventGird, "Item/" + eventInfo.icon);
            }
            else
            {
                _event.SetActive(false);
            }
        }
    }
}