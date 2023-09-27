using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2051 : ActivityInfo
{
    private int setShipId = -1;
    private List<int> shipIdList = new List<int>(4);//等待选取的回归造舰列表
    public override void InitUnique()
    {
        object setIdObj = null;
        if (!_data.avalue.TryGetValue("set_ship", out setIdObj))
        {
            throw new Exception("Act 2051 avalue find no key set_ship");
        }
        object idListObj = null;
        if (!_data.avalue.TryGetValue("miss_ships", out idListObj))
        {
            throw new Exception("Act 2051 avalue find no key miss_ships");
        }
        // var setId =_data.avalue["set_ship"].ToString();
        var setId = setIdObj.ToString();
        //var idList =_data.avalue["miss_ships"].ToString();
        var idList = idListObj.ToString();
        if (!string.IsNullOrEmpty(setId))
        {
            if (!int.TryParse(setId, out setShipId))
            {
                throw new Exception("Act 2051 avalue set_ship parse wrong set_ship =" + setId);
            }
        }
        if (string.IsNullOrEmpty(idList))
            throw new Exception("Act 2051 avalue->miss_ships is null or empty");

        var list = idList.Split(',');
        shipIdList.Clear();
        for (int i = 0; i < list.Length; i++)
        {
            int shipId;
            if (!int.TryParse(list[i], out shipId))
            {
                throw new Exception("Act 2051 avalue miss_ships parse wrong miss_ships =" + list);
            }
            shipIdList.Add(shipId);
        }
    }

    public int GetSetId()
    {
        return setShipId;
    }

    public List<int> GetWaitForChooseId()
    {
        return shipIdList;
    }

    public override bool IsAvaliable()
    {
        return setShipId == -1;
    }

    public void SetReopenShip(int setId, Action callback)
    {
        Rpc.SendWithTouchBlocking<P_None>("setReopenShip", Json.ToJsonString(setId), data =>
        {
            setShipId = setId;
            EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
            EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());
            if (callback != null)
                callback();
        });
    }
}