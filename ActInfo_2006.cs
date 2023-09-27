using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2006 : ActivityInfo
{
    public int payGold { private set; get; }

    public int payRate { private set; get; }

    private ActInfo_2006_Data _data2006;

    //"data":"{\"pay_gold\":0,\"pay_rate\":150}"
    public override void InitUnique()
    {
        _data2006 = JsonMapper.ToObject<ActInfo_2006_Data>(_data.avalue["data"].ToString());

        payGold = _data2006.pay_gold;

        payRate = _data2006.pay_rate;
    }

    public override bool IfRefreshOnPush(int opcode)
    {
        return opcode == OpcodePush.Recharge;
    }
}

public class ActInfo_2006_Data
{
    public int pay_gold;

    public int pay_rate;
}
