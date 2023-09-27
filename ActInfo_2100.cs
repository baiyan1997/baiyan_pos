using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActInfo_2100 : ActivityInfo
{
    public P_Act2100Detail ActDeetail = null; 
    public override void InitUnique()
    {
       
    }

    public void SetMissionState(int id)
    {
        var info = GetMissionInfo(id);
        if(info != null)
        {
            info.state = 1;
        }
    }
    public P_Act2100MissionInfo GetMissionInfo(int id)
    {
        if(ActDeetail != null)
        {
            var datas = ActDeetail.datas;
            var len = datas.Length;
            for (int i = 0; i < len; i++)
            {
                var info = datas[i];
                if(info.id == id)
                {
                    return info;
                }
            }
        }
        return null;
    }

    public void SendShareSucceed()
    {
        Rpc.SendWithTouchBlocking<P_Act2100ShareSucceed>("shareSucceed", null, data =>
        {
            EventCenter.Instance.Act2100ShareSucceed.Broadcast(data.result);
        });
    }

    public void GetActDetil()
    {
        Rpc.SendWithTouchBlocking<P_Act2100Detail>("get2100Detail", Json.ToJsonString(ActivityID.Share), data => 
        {
            ActDeetail = data;
            EventCenter.Instance.Act2100DetailBack.Broadcast();
        });
    }

    public void GetReward(int id)
    {
        Rpc.SendWithTouchBlocking<P_ActCommonReward>("getAct2100Reward", Json.ToJsonString(id), data => 
        {
            var rewardsStr = GlobalUtils.ToItemStr3(data.get_items);
            Uinfo.Instance.AddItem(rewardsStr, true);
            MessageManager.ShowRewards(data.get_items);
            SetMissionState(id);
            EventCenter.Instance.Act2100DetailBack.Broadcast();
        });
    }
}

public class P_Act2100MissionInfo
{
    public int progess;
    public int id;
    public int state;
}
public class P_Act2100Detail
{
    public P_Act2100MissionInfo[] datas;

}

public class P_Act2100ShareSucceed
{
    /// <summary>
    /// ���ͽ��1�ɹ�
    /// </summary>
    public int result;
}

