using System;
using LitJson;

public class ActInfo_2005 : ActivityInfo
{
    public int SHIP_ID = 51206103;

    public P_Act2005Data avalueData { private set; get; }

    private void UpdateData(P_Act2005Data newData)
    {
        _data.avalue["do_number"] = newData.do_number;
        avalueData = newData;
        EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
    }

    public override void InitUnique()
    {
        avalueData = JsonMapper.ToObject<P_Act2005Data>(_data.avalue["data"].ToString());
        SHIP_ID = avalueData.ship;
    }

    public override bool IsAvaliable()
    {
        return  Convert.ToInt32(_data.avalue["get_reward"]) == 0 && avalueData.do_number >= 7;
    }

    public void SendRefreshActItems(Action callback)
    {
        Rpc.SendWithTouchBlocking("refreshCard", null, data =>
        {
            if ((int)data[0] != 1)
            {
                Alert.Ok(Lang.TranslateJsonString((string)data[1]));
                return;
            }
            Uinfo.Instance.AddItem(ItemId.Gold, -(int)data[1]);
            UpdateData(JsonMapper.ToObject<P_Act2005Data>(data[2].ToString()));
            if (callback != null)
                callback();
        });
    }

    public void SendBuyOneItem(int index, Action callback)
    {
        Rpc.SendWithTouchBlocking("buyOneCard", Json.ToJsonString(index), data =>
        {
            if ((int)data[0] != 1)
            {
                Alert.Ok(Lang.TranslateJsonString((string)data[1]));
                return;
            }
            Uinfo.Instance.AddItem(ItemId.Gold, -(int)data[3]);
            var itemStr = data[1].ToString();
            Uinfo.Instance.AddItem(itemStr, true);
            MessageManager.ShowRewards(itemStr);
            UpdateData(JsonMapper.ToObject<P_Act2005Data>(data[2].ToString()));
            if (callback != null)
                callback();
        });
    }

    public void SendGetAct2005Reward(Action callback)
    {
        Rpc.SendWithTouchBlocking<P_Act2005Reward>("getAct2005Reward", null, data =>
        {
            Uinfo.Instance.AddItem(data.get_items,true);
            MessageManager.ShowRewards(data.get_items);

            _data.avalue["get_reward"] = 1;
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());

            if (callback != null)
                callback();
        });
    }
}

public class P_Act2005Data
{
    public string buy_gold;
    public string buy_index;
    public int buy_num;
    public int do_number;
    public int free_count;
    public string items;
    public long recover_ts;
    public int refresh_count;
    public int refresh_gold;
    public int ship;
}

public class P_Act2005Reward : IProtocolPostprocess
{
    public P_ShipInfo[] get_ships;
    public P_ShipEquip[] get_equips;
    public string get_items;

    public void OnToObject()
    {
        if (get_ships != null && Uinfo.Instance != null)
        {
            for (int i = 0; i < get_ships.Length; i++)
            {
                var ship = get_ships[i];
                if (!ship.IsEmpty())
                    Uinfo.Instance.Temp.PushShipInfo(ship);
            }
        }
        if (get_equips != null && Uinfo.Instance != null)
        {
            for (int i = 0; i < get_equips.Length; i++)
            {
                var equip = get_equips[i];
                if (!equip.IsEmpty())
                    Uinfo.Instance.Temp.PushEquipInfo(equip);
            }
        }
    }
}