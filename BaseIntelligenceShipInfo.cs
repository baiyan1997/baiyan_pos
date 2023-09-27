using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using LitJson;
public class BaseIntelligenceShipInfo : Singleton<BaseIntelligenceShipInfo>
{

    private List<EmergencyInfo> _emergencyInfo;

    private string _allDispatchShips;
    public override void OnBegin()
    {
        //刷新任务推送信息
        EventCenter.Instance.AddPushListener(OpcodePush.INTELLIGENCYSHIP_UPDATE, _EventINTELLIGENCYSHIP_UPDATE);
        EventCenter.Instance.SocketReconnected.AddListener(_OnSocketConnected);
    }
    public override void OnEnd()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.INTELLIGENCYSHIP_UPDATE, _EventINTELLIGENCYSHIP_UPDATE);
        EventCenter.Instance.SocketReconnected.RemoveListener(_OnSocketConnected);
    }
    private void _EventINTELLIGENCYSHIP_UPDATE(int opcode, string data)
    {
        InitEmergencyInfo();
    }

    private void _OnSocketConnected()
    {
        InitEmergencyInfo();
    }

    public void InitEmergencyInfo(Action callback = null)
    {
        //获取活动信息之后
        Rpc.SendWithTouchBlocking<List<EmergencyInfo>>("getEmergencyInfo", null,
            data =>
            {

                _emergencyInfo = data;
                if (data != null)
                {
                    //  _emergencyInfo.Sort((a, b) => a.start_ts > b.start_ts ? 1 : 0);
                    _emergencyInfo.Sort(Sort_start_ts);
                }
                EventCenter.Instance.EmergencyTaskRefresh.Broadcast();//刷新任务列表
                // EventCenter.Instance.RefreshIntelligenceShipLightColor.Broadcast();//刷新情报舰灯效
                for (int i = 0; i < _emergencyInfo.Count; i++)
                {
                    var t = _emergencyInfo[i];
                    var type = Cfg.EmergencyTask.GetTypeByEid(t.eid);
                    if (type == 1)
                    {
                        _allDispatchShips = JsonMapper.ToObject<FleetExploreMission>(t.detail).all_dispatch_shipIds;
                        break;
                    }
                }
                callback?.Invoke();
            });
    }
    private static int Sort_start_ts(EmergencyInfo a, EmergencyInfo b)
    {
        return a.start_ts > b.start_ts ? 1 : 0;
    }
    //紧急特情列表
    public List<EmergencyInfo> GetEmergencyInfoList()
    {
        return _emergencyInfo;
    }
    //得最新刷出的任务类型 用来判断光线颜色
    public EmergencyInfo GetLatestTaskType()
    {
        if (_emergencyInfo == null || _emergencyInfo.Count <= 0)
        {
            return null;
        }
        EmergencyInfo lastTask = _emergencyInfo[_emergencyInfo.Count - 1];
        return lastTask;
    }

    public void RefreshEmergencyInfo(int id, string info, string allShips = null)
    {
        for (int i = 0; i < _emergencyInfo.Count; i++)
        {
            if (id == _emergencyInfo[i].id)
            {
                _emergencyInfo[i].detail = info;
                if (info[0] == '[')
                {
                    _emergencyInfo[i].detail = _emergencyInfo[i].detail.Substring(1, _emergencyInfo[i].detail.Length - 2);
                }

            }
        }
        if (allShips != null)
        {
            _allDispatchShips = allShips;
        }
    }

    public string GetExploringAllShips()
    {
        return _allDispatchShips;
    }

    public bool CurrentTaskExist(int tid)
    {
        for (int i = 0; i < _emergencyInfo.Count; i++)
        {
            var t = _emergencyInfo[i];
            if (tid == t.id)
            {
                return true;
            }
        }
        return false;
    }

    public void ReadTask(int tid, Action callback = null)
    {
        Rpc.SendWithTouchBlocking("readIntelligenceShipEmergency", Json.ToJsonString(tid), data =>
        {
            for (int i = 0; i < _emergencyInfo.Count; i++)
            {
                var one = _emergencyInfo[i];
                if (one.id == tid)
                {
                    one.is_read = 1;
                }
            }
            // EventCenter.Instance.RefreshIntelligenceShipLightColor.Broadcast();
            callback?.Invoke();
        });
    }
}

//紧急特情信息
public class EmergencyInfo
{
    public int id;//唯一id
    public int eid;//对应的任务id
    public long end_ts;
    public long start_ts;//刷新出任务的时间 用来判断刷新顺序
    public int status;//结束时间
    public string detail;//界面信息 json字串
    public int is_read;//是否已读
}
