using System;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2051_UI : ActivityUI
{
    private const int Aid = 2051;
    private ActInfo_2051 _actInfo;
    private Text _time;
    private Text _desc;

    //等待选中的回归船
    public Act2051ShipItem[] _waitChoose;

    //已经选中的回归船   当等待选中的回归船是1个的时候也使用Act2051ShipItemChoosed
    public Act2051ShipItemChoosed _choosed;

    private Vector2[][] _pos = new Vector2[4][];

    public override void UpdateTime(long stamp)
    {
        base.UpdateTime(stamp);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;

        if (_actInfo.LeftTime >= 0)
        {
            _time.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _time.text = Lang.Get("活动已经结束");
        }
    }
    public override void Awake()
    {
        _time = transform.FindText("Text_time");
        _desc = transform.FindText("Text_desc");
        _choosed = transform.Find("Airship").gameObject.AddBehaviour<Act2051ShipItemChoosed>();
        _waitChoose = new Act2051ShipItem[4];
        for (int i = 0; i < 4; i++)
        {
            _waitChoose[i] = transform.Find("ShipRoot/Airship" + i).gameObject.AddBehaviour<Act2051ShipItem>();
        }

        _pos[0] = _1ShipPos;
        _pos[1] = _2ShipPos;
        _pos[2] = _3ShipPos;
        _pos[3] = _4ShipPos;
    }
    private Vector2[] _4ShipPos = new[]
    {
        new Vector2(-132,190.5f),
        new Vector2(132,190.5f),
        new Vector2(-132,-136),
        new Vector2(132,-136),
    };
    private Vector2[] _3ShipPos = new[]
    {
        new Vector2(0,190.5f),
        new Vector2(-132,-83),
        new Vector2(132,-83),
    };
    private Vector2[] _2ShipPos = new[]
    {
        new Vector2(-132,-31),
        new Vector2(132,-31),
    };
    private Vector2[] _1ShipPos = new[]
    {
        Vector2.zero,
    };
    public override void OnCreate()
    {
        _actInfo = (ActInfo_2051)ActivityManager.Instance.GetActivityInfo(Aid);
        //InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(RefrshUI);
        //TimeManager.Instance.TimePassSecond += UpdateTime;
    }

    public override void OnShow()
    {
        RefrshUI(Aid);
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        RefrshUI(aid);
    }

    private void RefrshUI(int aid)
    {
        if (aid != Aid)
            return;
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;

        UpdateTime(TimeManager.ServerTimestamp);
        _desc.text = Cfg.Act.GetData(Aid).act_desc;

        //选中的回归船
        var setId = _actInfo.GetSetId();

        var waitList = _actInfo.GetWaitForChooseId();

        if (waitList.Count == 1 || setId != -1)
        {
            int shipId;
            if (setId != -1)
            {
                shipId = setId;
            }
            else
            {
                shipId = waitList[0];
            }


            for (int i = 0; i < 4; i++)
            {
                _waitChoose[i].gameObject.SetActive(false);
            }
            _choosed.gameObject.SetActive(true);
            _choosed.Refresh(_actInfo, shipId, setId != -1);
            _ShipDisplayControl.Instance.ShowShip(shipId, _ShipDisplayControl.DisplayMode.AutoRotateOnly);
            return;
        }

        _choosed.gameObject.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            if (i < waitList.Count)
            {
                _waitChoose[i].gameObject.SetActive(true);
                _waitChoose[i].transform.localPosition = GetLocalPos(i, waitList.Count);
                _waitChoose[i].Refresh(_actInfo, waitList[i]);
            }
            else
            {
                _waitChoose[i].gameObject.SetActive(false);
            }
        }

    }

    public Vector2 GetLocalPos(int index, int shipCount)
    {
        shipCount = Mathf.Min(4, shipCount);//最多四个
        return _pos[shipCount - 1][index];

    }
    public override void OnClose()
    {
        base.OnClose();
        _ShipDisplayControl.Instance.CloseShipShow();
    }
}

public class Act2051ShipItem : JDBehaviour
{
    private Button _detailBtn;
    private Text _name;
    private Button _choose;
    private int _shipId;
    private Image _shipIcon;
    private ActInfo_2051 _actInfo;
    public override void Awake()
    {
        base.Awake();
        _detailBtn = transform.Find<Button>("Images1/HelpButton");
        _name = transform.Find<Text>("Images1/Text");
        _shipIcon = transform.Find<Image>("Images1/Img_icon");
        _choose = transform.Find<Button>("Button1");

        _choose.onClick.AddListener(On_chooseClick);
        _detailBtn.onClick.AddListener(On_detailBtnClick);
    }
    private void On_chooseClick()
    {
        var qua = Cfg.Ship.GetShipQua(_shipId);
        var color = _ColorConfig.GetQuaColorText(qua);
        var d = Alert.YesNo(Lang.Get("是否将<color={0}>{1}</color>战舰召唤至战舰工厂开始激活，一旦选定后不可更改", color, _name.text));
        d.SetYesCallback(() =>
        {
            d.Close();
            _actInfo.SetReopenShip(_shipId, null);
        });
    }
    private void On_detailBtnClick()
    {
        DialogManager.ShowAsyn<_D_2051ShipShow>(OndetailDialogShowAsynCB);
    }
    private void OndetailDialogShowAsynCB(_D_2051ShipShow d)
    {
        d?.OnShow(_shipId, _detailBtn.transform.position);
    }

    public void Refresh(ActInfo_2051 info, int shipId)
    {
        _actInfo = info;
        _shipId = shipId;
        Cfg.Ship.SetShipIcon(_shipIcon, _shipId);
        _name.text = Cfg.Ship.GetShipName(shipId);
        var qua = Cfg.Ship.GetShipQua(shipId);
        _name.color = _ColorConfig.GetQuaColor(qua);
    }
}
public class Act2051ShipItemChoosed : JDBehaviour
{
    private Button _detailBtn;
    private Text _name;
    private Button _goDrawShip;
    private Button _choose;
    private Text _goDrawShipText;
    private int _shipId;
    private ActInfo_2051 _actInfo;
    public override void Awake()
    {
        base.Awake();
        _detailBtn = transform.Find<Button>("Images1/HelpButton");
        _name = transform.Find<Text>("Images1/Text");
        _goDrawShip = transform.Find<Button>("Button1");
        _choose = transform.Find<Button>("Btn_choose");
        _goDrawShipText = transform.Find<Text>("Button1/Text");
        _choose.onClick.AddListener(On_chooseClick);
        _goDrawShip.onClick.AddListener(On_goDrawShipClick);
        _detailBtn.onClick.AddListener(On_detailBtnClick);
    }

    private void On_chooseClick()
    {
        var qua = Cfg.Ship.GetShipQua(_shipId);
        var color = _ColorConfig.GetQuaColorText(qua);
        var d = Alert.YesNo(Lang.Get("是否将<color={0}>{1}</color>战舰召唤至战舰工厂开始激活，一旦选定后不可更改", color, _name.text));
        d.SetYesCallback(() =>
        {
            d.Close();
            _actInfo.SetReopenShip(_shipId, null);
        });
    }
    private void On_goDrawShipClick()
    {
        //获取该定制舰在哪个基地
        BaseType b = SpecialShipInfo.Instance.GetBaseByShipId(_shipId);
        if (b == BaseType.Undefined)
            return;
        //获取造舰工厂pos
        var pos = Cfg.Building.GetBuildingPosByBaseTBuildT((int)b, BuildingTypes.ShipFactory);
        if (!BuildingInfo.Instance.IsBuildingActive(pos))
            return;

        DialogManager.CloseAllDialog();
        DialogManager.ShowAsyn<_D_ShipFactory>(d => { d?.OnShow(pos, _shipId); });
    }
    private void On_detailBtnClick()
    {
        DialogManager.ShowAsyn<_D_2051ShipShow>(On_detailDialogShowAsynCB);
    }
    private void On_detailDialogShowAsynCB(_D_2051ShipShow d)
    {
        d?.OnShow(_shipId, _detailBtn.transform.position);
    }


    public void Refresh(ActInfo_2051 info, int shipId, bool choosed)
    {
        _actInfo = info;
        _shipId = shipId;
        _name.text = Cfg.Ship.GetShipName(shipId);
        var qua = Cfg.Ship.GetShipQua(shipId);
        _name.color = _ColorConfig.GetQuaColor(qua);


        if (ShipYardInfo.Instance.HasShip(shipId))
        {
            _goDrawShip.interactable = false;
            _goDrawShip.gameObject.SetActive(true);
            _choose.gameObject.SetActive(false);
            _goDrawShipText.text = Lang.Get("已获得");
        }
        else
        {
            //已选中
            if (choosed)
            {
                _choose.gameObject.SetActive(false);
                _goDrawShip.interactable = true;
                _goDrawShip.gameObject.SetActive(true);
                _goDrawShipText.text = Lang.Get("前往造舰");
            }
            else
            {
                _choose.gameObject.SetActive(true);
                _goDrawShip.gameObject.SetActive(false);
            }
        }
    }
}