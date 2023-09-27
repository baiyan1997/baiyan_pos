using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2095 : ActivityInfo
{
    private List<P_Act2095Mission> _missionInfo;
    private List<MapInfo2095> _mapInfo;
    private List<P_Act2095Equipment> _equipmentList;
    private List<int> _shipList;
    private P_Act2095Attribute _attributeInfo;
    private string _warehouseRewards;
    private int _currentShipId = 0;
    private int _fuelNum;
    private int _puzzleNum;
    private int _puzzleRewardFlag;
    private int _currentPosition;
    public override void InitUnique()
    {
        _missionInfo = JsonMapper.ToObject<List<P_Act2095Mission>>(_data.avalue["mission_info"].ToString());
        _currentShipId = Convert.ToInt32(_data.avalue["cur_ship_id"]);
        _equipmentList = JsonMapper.ToObject<List<P_Act2095Equipment>>(_data.avalue["equip_info"].ToString());
        var tempShips = _data.avalue["choose_ship_ids"].ToString();
        DealShipList(tempShips);
        _fuelNum = Convert.ToInt32(_data.avalue["fuel"]);
        _warehouseRewards = _data.avalue["reward_info"].ToString();
        _puzzleRewardFlag = Convert.ToInt32(_data.avalue["is_get_jigsaw"]);

        Get2095SelectedShipInfo();
        GetMapInfo(() =>
        {
            _currentPosition = Convert.ToInt32(_data.avalue["map_id"]);
        });
    }
    private void DealShipList(string value)
    {
        if (_shipList == null)
        {
            _shipList = new List<int>();
        }

        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        _shipList.Clear();
        string[] tempVal = value.Split(',');
        for (int i = 0; i < tempVal.Length; i++)
        {
            var one = tempVal[i];
            _shipList.Add(Int32.Parse(one));
        }

    }
    //获得地图信息
    public void GetMapInfo(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<List<MapInfo2095>>("getAct2095MapInfo", null, data =>
        {
            _mapInfo = data;
            callback?.Invoke();
        });
    }
    //获取战舰的信息
    public void Get2095SelectedShipInfo(Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2095Attribute>("getAct2095AttrInfo", null, data =>
        {
            _attributeInfo = data;
            callback?.Invoke();
        });
    }
    //合成装备
    public void SyntheticEquipment(int id1, int id2, Action callback)
    {
        Rpc.SendWithTouchBlocking<List<P_Act2095Equipment>>("updateAct2095Equip", Json.ToJsonString(id1, id2), (data) =>
        {
            _equipmentList = data;
            callback?.Invoke();
        });
    }

    //更换装备
    public void ChangeEquipment(int id, Action callback)
    {
        Rpc.SendWithTouchBlocking("putOnAct2095Equip", Json.ToJsonString(id), (data) =>
        {
            Get2095SelectedShipInfo(callback);
        });
    }
    //探索/攻击
    public void Explore(int pos, Action<int, P_Item[]> callback = null)
    {
        Rpc.SendWithTouchBlocking<ExploreRewards_2095>("atkAct2095Boss", Json.ToJsonString(pos), (data) =>
        {
            if (data.win == 0)
            {
                _fuelNum = Math.Max(0, _fuelNum - 10);
            }
            P_Item[] rewards = GlobalUtils.ParseItem(data.get_reward);

            for (int i = 0; i < rewards.Length; i++)
            {
                var one = rewards[i];
                if (one.Id == ItemId.WarshipPuzzle2095)
                {
                    string tempReward = String.Format("{0}|{1}|{2}", one.Id, one.Num, one.Extra);
                    Uinfo.Instance.AddItem(tempReward, true);
                }
            }
            _warehouseRewards = data.reward_info;
            _equipmentList = data.equip_info;
            GetMapInfo(() =>
            {
                callback?.Invoke(data.win, rewards);
            });
        });
    }
    //领取仓库奖励
    public void ReceiveWarehouseRewards(Action callback)
    {
        Rpc.SendWithTouchBlocking<string>("getAct2095Reward", null, (data) =>
        {

            Uinfo.Instance.AddItemAndShow(data);
            _warehouseRewards = "";
            callback?.Invoke();
        });
    }
    //提交选择的战舰
    public void SelectShip(int shipId, Action callback = null)
    {
        Rpc.SendWithTouchBlocking("chooseAct2095Ship", Json.ToJsonString(shipId), (data) =>
        {
            //更改选择战舰
            _currentShipId = shipId;
            Get2095SelectedShipInfo(callback);
        });
    }
    //领取拼图奖励
    public void GetPuzzlesReward(Action callback)
    {
        Rpc.SendWithTouchBlocking<string>("getActJigsawReward", null, (data) =>
        {
            Uinfo.Instance.AddItemAndShow(data);
            _puzzleRewardFlag = 1;
            callback?.Invoke();
        });
    }
    //移动战舰
    public void MoveShip(int pos, Action<List<MapInfo2095>> callback)
    {
        Rpc.SendWithTouchBlocking<List<MapInfo2095>>("moveAct2095Map", Json.ToJsonString(pos), (data) =>
        {
            //燃料--
            _fuelNum = Math.Max(_fuelNum - 1, 0);
            _currentPosition = pos;
            //当前位置更新
            for (int i = 0; i < data.Count; i++)
            {
                var one = data[i];
                _mapInfo[one.map_id].pass = 1;
            }
            callback?.Invoke(data);
        });
    }

    public P_Act2095Attribute GetShipAttribute()
    {
        return _attributeInfo;
    }
    //获取装备列表
    public List<P_Act2095Equipment> GetAct2095Equipments()
    {
        return _equipmentList;
    }
    //获取任务信息
    public List<P_Act2095Mission> GetAct2095Missions()
    {
        return _missionInfo;
    }

    //是否选择战舰
    public bool GetShipBeenSelected()
    {
        return _currentShipId != 0;
    }
    //获取当前选择的战舰
    public int GetCurrentShip()
    {
        return _currentShipId;
    }
    //获得备选战舰列表
    public List<int> GetShipList()
    {
        return _shipList;
    }
    //获取当前战舰位置
    public int GetCurrentShipPosition()
    {
        return _currentPosition;
    }
    //获取航行燃料
    public int GetFuel()
    {
        return _fuelNum;
    }
    //是否完成战舰拼图
    public bool IsCompleteShipPuzzle()
    {
        return _puzzleRewardFlag == 1;
    }
    //判断该位置格子的类型 获得事件id
    public int GetGridEventId(int pos)
    {
        return _mapInfo[pos].event_id;
    }
    //获取格子是否有迷雾
    public bool IsThereFog(int pos)
    {
        return _mapInfo[pos].pass == 0;
    }

    public List<MapInfo2095> GetMap()
    {
        return _mapInfo;
    }
    //得到仓库奖励
    public string GetWarehouseRewardsList()
    {
        return _warehouseRewards;
    }

    //获得玩家生成位置
    public int GetShipGenerationPosition()
    {
        return 21;
    }
}

public class MapInfo2095
{
    public int uid;
    public int xy_x;
    public int xy_y;
    public int event_id;
    public int map_id;
    public int pos;
    public int pass;
}
public class ExploreRewards_2095
{
    public int win;
    public string get_reward;//本次获得
    public string reward_info;//仓库道具
    public List<P_Act2095Equipment> equip_info;
}
public class P_Act2095Mission
{
    public int finished;
    public int get_reward;
    public int do_number;
    public int tid;
}

public class P_Act2095Equipment
{
    public int eid;
    public int etype;
    public int elv;

}

public class P_Act2095Attribute
{
    public int fight;
    public int atk;
    public int def;
    public int hp;
    public List<P_Act2095Equipment> equip;
}