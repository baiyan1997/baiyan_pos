using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Act2094 排行榜前十阵容 及下一期boss阵容
public class _D_Act2094Lineup : Dialog
{
    private Text _title;
    private Button _close;
    private _sectShipItem[] _sectInfos;
    private ArenaShipsDefItem[] _shipsShow;
    private GameObject _captFrontLine;
    private GameObject _captBehindLine;
    private GameObject _shipFrontLine;
    private GameObject _shipBehindLine;
    private ActInfo_2094 _actInfo;
    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    protected override void InitRef()
    {
        _title = transform.FindText("Main/Bg/Text_Title");
        _close = transform.FindButton("Main/CloseBtn");
        _captFrontLine = transform.Find("Main/Inf_03").gameObject;
        _captBehindLine = transform.Find("Main/Inf_04").gameObject;
        _shipFrontLine = transform.Find("Main/Inf_01").gameObject;
        _shipBehindLine = transform.Find("Main/Inf_02").gameObject;
        _sectInfos = new[]
        {
           new _sectShipItem(transform.Find("Main/Inf_03/01")),
           new _sectShipItem(transform.Find("Main/Inf_03/02")),
           new _sectShipItem(transform.Find("Main/Inf_03/03")),
           new _sectShipItem(transform.Find("Main/Inf_04/04")),
           new _sectShipItem(transform.Find("Main/Inf_04/05")),
           new _sectShipItem(transform.Find("Main/Inf_04/06")),
        };
        _shipsShow = new[]
        {
            _shipFrontLine.transform.Find("01").gameObject.AddBehaviour<ArenaShipsDefItem>(),
            _shipFrontLine.transform.Find("01 (1)").gameObject.AddBehaviour<ArenaShipsDefItem>(),
            _shipFrontLine.transform.Find("01 (2)").gameObject.AddBehaviour<ArenaShipsDefItem>(),
            _shipBehindLine.transform.Find("01").gameObject.AddBehaviour<ArenaShipsDefItem>(),
            _shipBehindLine.transform.Find("01 (1)").gameObject.AddBehaviour<ArenaShipsDefItem>(),
            _shipBehindLine.transform.Find("01 (2)").gameObject.AddBehaviour<ArenaShipsDefItem>(),
        };
        _actInfo = ActivityManager.Instance.GetActivityInfo(2094) as ActInfo_2094;

    }

    public override bool IsFullScreen()
    {
        return false;
    }

    protected override void OnCreate()
    {
        _close.onClick.AddListener(Close);
    }

    //type==1 下一期boss 阵容 type==2 排行榜玩家阵容
    public void OnShow(int type, P_Act2094RankItemInfo info = null, P_ShipLineupInfo shipInfo = null)
    {
        if (type == 1)
        {
            _title.text = Lang.Get("下期阵容");
            RefreshCapt();

        }
        else if (type == 2)
        {
            _title.text = Lang.Get("Lv.{0} {1} 的战斗阵容", info?.u_lv, info?.uname);
            RefreshShips(shipInfo);
        }

        ShowCaptOrShip(type);
    }

    private List<int> _posArr = new List<int> { 2, 1, 3, 4, 5, 6 };
    private void RefreshShips(P_ShipLineupInfo infos)
    {
        if (infos == null)
        {
            throw new Exception($"ship lineup info can not be null");
        }

        var list = infos.ShipInfos;
        int len = _shipsShow.Length;
        int len2 = list.Count;
        for (int i = 0; i < len; i++)
        {
            P_2094ShipInfo info = null;
            for (int j = 0; j < len2; j++)
            {
                if (list[j].pos == _posArr[i])
                {
                    info = list[j];
                    break;
                }
            }
            _shipsShow[i].Refresh(info);
        }
    }

    private void RefreshCapt()
    {
        var nextBoss = Cfg.Act2094.GetBossInfo(_actInfo.InitInfo.next_boss_id);
        int len = nextBoss.Count;
        for (int i = 0; i < len; i++)
        {
            P_Act2094BossInfo info = nextBoss[i];
            int index = 0;
            for (int j = 0; j < 6; j++)
            {
                if (_posArr[j] == info.pos)
                {
                    index = j;
                    break;
                }
            }
            _sectInfos[index].Refresh(info.captain_id, info.radar_id, info.ship_id);
        }
    }

    private void ShowCaptOrShip(int type)
    {
        _captFrontLine.SetActive(type == 1);
        _captBehindLine.SetActive(type == 1);
        _shipFrontLine.SetActive(type == 2);
        _shipBehindLine.SetActive(type == 2);
    }
}

