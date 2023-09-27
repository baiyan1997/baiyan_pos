using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActInfo_3000 : ActivityInfo
{
    public P_ActMission3000 _infoDate { get; private set; }
    public P_ProgressData3000 _progressData { get; private set; }
    //星际通行证价格
    public int _buyProportion { get; private set; }
    public bool _isShowRedPoint;
    public int _localTsGroup { get; private set; }//传送配置
    public int _localTsServerOpen { get; private set; }//开服时间
    public Dictionary<string, List<P_CanTransferBattle3000>> _canTransferBattleData { get; private set; }//可转国家数据
    
    public override void InitUnique()
    {
        ChangeRedPointSign();
        _buyProportion = Convert.ToInt32(_data.avalue["transfer_item_price"].ToString());
        _localTsGroup = Convert.ToInt32(_data.avalue["local_transfer_group"].ToString());
        _localTsServerOpen = Convert.ToInt32(_data.avalue["local_open_ts_real"].ToString());
        _canTransferBattleData = JsonMapper.ToObject<Dictionary<string,List<P_CanTransferBattle3000>>>(_data.avalue["can_transfer_battle"].ToString());      
    }
    //改变红点状态
    private void ChangeRedPointSign()
    {
        var ts = _data.endts.ToString();
        if (PlayerPrefs.HasKey(ts))
            _isShowRedPoint = Convert.ToInt32(PlayerPrefs.GetString(ts)) != 0;
        else
            PlayerPrefs.SetString(ts, "0");
    }
    public override bool IsAvaliable()
    {
        ChangeRedPointSign();
        if (!_isShowRedPoint)
        {
            return true;
        }
        return false;
    }
    public override bool OnInited()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.STATE_TRANSFER_PROCESS_CHANGE, _EventSTATE_TRANSFER_PROCESS_CHANGE);
        return true;
    }

    public override void OnRemove()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.STATE_TRANSFER_PROCESS_CHANGE, _EventSTATE_TRANSFER_PROCESS_CHANGE);
    }

    private void _EventSTATE_TRANSFER_PROCESS_CHANGE(int opcode, string data)
    {
        var info = JsonMapper.ToObject<P_ProgressData3000>(data);
        _progressData = info;
        DialogManager.GetInstanceOfDialog<_D_TS_ChangeServerState>().ShowProgress(info);
    }

        //传入战区uid,国家id,获取信息
        public void GetTransferInfo(int uid, int countryId, Action<P_ActMission3000> ac)
    {
        Rpc.SendWithTouchBlocking<P_ActMission3000>("getTransferInfo", Json.ToJsonString(uid, countryId), data =>
         {
             _infoDate = data;
             if (ac != null)
                 ac(data);
         });
    }
    //购买通行证，参数：购买数量
    public void BuyTransferItem(int buyCount, Action ac)
    {
        Rpc.SendWithTouchBlocking<P_BuyTicket3000>("buyTransferItem", Json.ToJsonString(buyCount), data =>
        {
            Uinfo.Instance.Player.AddGold(-data.cost_gold);
            Uinfo.Instance.AddItem(ItemId.InterstellarPass, data.buy_count);
            if (ac != null)
                ac();
        });
    }
    //转国,服务器sid ,战服id,ustate[新势力], 玩家在目标国家的排名
    public void SeasonChangeState(int sid, int battleId, int ustate, int rank, Action ac)
    {
        Rpc.SendWithTouchBlocking<string>("seasonChangeState", Json.ToJsonString(sid, battleId, ustate, rank), data =>
          {
              Uinfo.Instance.AddItem(data, false);
              if (ac != null)
                  ac();
          });
    }
    //检查新入口是否有账号，参数：目标登录服的sid,就是local_sid
    public void CheckHasAccount(int local_sid, Action<P_CheckHasAccount> ac)
    {
        Rpc.SendWithTouchBlocking<P_CheckHasAccount>("checkHasAccount", Json.ToJsonString(local_sid), data =>
        {
            //data 1有 0没有
            if (ac != null)
                ac(data);
        });
    }
    //判断是否有登入口同时满足转服配置和时间限定
    public bool CompareEntrance()
    {
        for (int i = 0; i < _infoDate.sub_state_list.Count; i++)
        {
            if (_infoDate.sub_state_list[i].transfer_group == _localTsGroup)
            {
                if (Mathf.Abs(_infoDate.sub_state_list[i].real_open_ts - _localTsServerOpen) <= 30 * 86400)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //获取所转国名
    public string GetTransferCountryName(int battlePlaceId, int countryId)
    {
        List<P_CanTransferBattle3000> infoList;
        P_CanTransferBattle3000 currenCantTransmit = null;
        if (_canTransferBattleData.TryGetValue(battlePlaceId.ToString(), out infoList))
        {
            for (int i = 0, max = infoList.Count; i < max; i++)
            {
                var info = infoList[i];
                if (info.new_state == countryId)
                    return info.state_name;
            }
            return null;
        }
        else
            return null;
    }
}

public class P_ActMission3000
{
    public int need_item;//需要通行证
    public int transfer_count;//已传送人数
    public List<P_State3000> sub_state_list;//入口列表
    public List<P_Rank3000> rank_list;//战斗榜
    public int active_count;//活跃人数
    public int battle_group_rank;//战区排名
}

//战斗榜
public class P_Rank3000
{
    public int uid;//uid
    public string uname;//名字
    public int ulevel;//等级
    public int base_max_upower_history;//最高战力
    public int cur_rank;//当前排名
}
//入口
public class P_State3000
{
    public int local_sid;//sid
    public int local_state;//国家名称:共和，联邦，帝国
    public string server_name_real;//服务器名称
    public int transfer_group;//转国配置 [相同的服务器之间可以相互转]
    public int real_open_ts;//开服时间

    //判断是否有登入口同时满足转服配置和时间限定
    public bool CompareEntrance()
    {
        var _actInfo = (ActInfo_3000)ActivityManager.Instance.GetActivityInfo(ActivityID.ServerChange);
        var isConfigPlatForm = transfer_group == _actInfo._localTsGroup;
        var isOpenServerTime = Mathf.Abs(real_open_ts - _actInfo._localTsServerOpen) <= 30 * 86400;
        return (isConfigPlatForm && isOpenServerTime);
    }
}
//购买通行证
public class P_BuyTicket3000
{
    public int cost_gold;//花费氪晶
    public int buy_count;//购买的通行证数量
}
//传送中数据
public class P_ProgressData3000
{
    public int max_id;//最大进度
    public int aim_sid;//服务器sid
    public string aim_server_name;//服务器名字
    public int transfer_id;//变化进度
    public string msg;//信息
}
public class P_CheckHasAccount
{
    public int has_account;//是否有角色 0没有 1有
    public string uname;//角色名
    public int ulevel;//角色等级
}
//可转国家的数据
public class P_CanTransferBattle3000
{
    public int flag;//是否可转 0不可转 1可转
    public string state_name;//国家名称
    public int new_state;//国家代号
}
