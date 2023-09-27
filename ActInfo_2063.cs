using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;
using UnityEngine;

public class ActInfo_2063 : ActivityInfo
{
    public List<P_ActMission2063> _rewardData { get; private set; }
    //普通扭蛋机下的候选奖励key:Id value:itemId
    private Dictionary<int, List<int>> _selectNormalData = new Dictionary<int, List<int>>();
    //高级扭蛋机下的候选奖励key:Id value:itemId
    private Dictionary<int, List<int>> _selectSeniorData = new Dictionary<int, List<int>>();
    //根据服务器传的的消息建立数据字典 key:AdvanceType value[key:index,value:id]
    private Dictionary<int, Dictionary<int, int>> _IndexToId = new Dictionary<int, Dictionary<int, int>>();
    public AdvanceType type = AdvanceType.Normal;
    //总积分
    public int _totalScore { private set; get; }
    //商店数据初始化
    public List<P_ShopItem2063> _shopData { private set; get; }
    //活动配置数据
    public List<P_Act_2063> _actData { private set; get; }
    //排序后的奖励
    private List<string> itemList = new List<string>();
    private StringBuilder _sb = new StringBuilder();
    public Action<int> _callBackSelectData;
    public bool _canClick;//是否可以点击
    //奖池数据
    public P_InfoPool _poolData { private set; get; }
    public override void InitUnique()
    {
        if (_data.avalue.Count > 0)
        {
            _totalScore = Convert.ToInt32(_data.avalue["score"].ToString());
            _rewardData = JsonMapper.ToObject<List<P_ActMission2063>>(_data.avalue["act_mission"].ToString());
            _shopData = JsonMapper.ToObject<List<P_ShopItem2063>>(_data.avalue["cfg_act_shop_info"].ToString());
            _actData = JsonMapper.ToObject<List<P_Act_2063>>(_data.avalue["cfg_act_info"].ToString());
            _poolData = JsonMapper.ToObject<P_InfoPool>(_data.avalue["pool_info"].ToString());

            for (int i = 0; i < _rewardData.Count; i++)
            {
                _rewardData[i].dataDic = JsonMapper.ToObject<Dictionary<string, int>>(_rewardData[i].data);
            }
            InitGetDic();
            InitRewarPool();
        }
    }

    public override bool NeedDailyRemind()
    {
        return true;
    }

    //抽奖
    public void ExtractMachine(int roundCound, Action<int, List<P_Item3>> ac)
    {
        Rpc.SendWithTouchBlocking<P_GetReward2063>("extractMachine", Json.ToJsonString((int)type, roundCound, SetSelectDataToSend()), data =>
         {
             Uinfo.Instance.Player.AddGold(-data.cost_gold);
             var index = GetRewardIndex(data.rewards);
             ActivityManager.Instance.RequestUpdateActivityById(_aid);//更新活动信息
             if (ac != null)
                 ac(index, data.rewards);
         });
    }
    //商店购买,itemId
    public void BuyScoreShop(int itemId, Action ac)
    {
        Rpc.SendWithTouchBlocking<P_Shop2063>("buyScoreShop", Json.ToJsonString(itemId), data =>
        {
            _totalScore -= data.cost_score;
            Uinfo.Instance.AddItem(data.get_item, true);
            MessageManager.ShowRewards(data.get_item);
            if (ac != null)
                ac();
        });
    }
    //获取对应等级下的扭蛋机候选的数据
    private Dictionary<int, List<int>> GetSelectDic(AdvanceType type)
    {
        switch (type)
        {
            case AdvanceType.Normal:
                return _selectNormalData;
            case AdvanceType.Senior:
                return _selectSeniorData;
        }
        throw new Exception("can not find type:" + type);
    }
    //将数据变成id|itemid,itemid;id|itemid,itemid;id|itemid,itemid这样传递给服务器
    private string SetSelectDataToSend()
    {
        _sb.Length = 0;
        var selectData = GetSelectDic(type);
        foreach (var keyValue in selectData)
        {
            _sb.Append(keyValue.Key);
            _sb.Append('|');
            for (int i = 0; i < keyValue.Value.Count; i++)
            {
                _sb.Append(keyValue.Value[i]);
                if (i < keyValue.Value.Count - 1)
                    _sb.Append(',');
                else
                    _sb.Append(';');
            }
        }
        return _sb.ToString();
    }
    //从候选奖励池移除
    public void RemoveSelectDate(int id, int itemId)
    {
        var selectData = GetSelectDic(type);
        List<int> tempList = null;
        if (selectData.TryGetValue(id, out tempList))
        {
            // var tempList = selectData[id];
            tempList.Remove(itemId);
        }
        if (_callBackSelectData != null)
        {
            _callBackSelectData(GetCandidacyRewardNum());
        }
    }
    //添加到候选奖励池
    public void AddSelectDate(int id, int itemId)
    {
        var selectData = GetSelectDic(type);
        List<int> itemList = null;
        if (selectData.TryGetValue(id, out itemList))
        {
            itemList.Add(itemId);
        }
        else
        {
            itemList = new List<int>();
            itemList.Add(itemId);
            selectData.Add(id, itemList);
        }

        if (_callBackSelectData != null)
        {
            _callBackSelectData(GetCandidacyRewardNum());
        }
    }
    //得到一种扭蛋机中待选区域的道具数量
    public int GetCandidacyRewardNum()
    {
        int num = 0;
        var selectData = GetSelectDic(type);
        foreach (var temp in selectData)
        {
            num += temp.Value.Count;
        }
        return num;
    }

    //将候选的item设置到抽奖空格里逆时针从等级1-4 [itemid|count]
    public List<string> GetCandidacyReward()
    {
        itemList.Clear();
        var selectData = GetSelectDic(type);
        var keys = selectData.Keys.ToList();
        //keys.Sort((a, b) =>
        //{
        //    return a - b;
        //});
        keys.Sort(Sort_int);
        for (int i = 0; i < keys.Count; i++)
        {
            var data = _actData.Find(item => item.id == keys[i]).reward;
            var items = GlobalUtils.ParseItem(data);
            for (int j = 0; j < items.Length; j++)
            {
                if (selectData[keys[i]].Contains(items[j].id))
                {
                    itemList.Add(items[j].id + "|" + items[j].count);
                }
            }
        }
        return itemList;
    }
    private int Sort_int(int a, int b)
    {
        return a - b;
    }


    //通过index拿到已经选择的列表
    public List<int> GetReawrdPanel(int index)
    {
        var selectData = GetSelectDic(type);
        var id = GetIndexToId(index);
        List<int> data;
        if (selectData.TryGetValue(id, out data))
        {
            return data;
        }
        return null;
    }
    //根据服务器返回的中奖itemid,返回一个位置
    private int GetRewardIndex(List<P_Item3> dataReward)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            var data = GlobalUtils.ParseItem(itemList[i])[0];
            // var match = dataReward.Find(a => a.itemid == data.id);

            P_Item3 match = null;
            for (int k = 0; k < dataReward.Count; k++)
            {
                P_Item3 temp = dataReward[k];
                if (temp.itemid == data.id)
                {
                    match = temp;
                    break;
                }
            }
            if (match != null)
            {
                return i;
            }
        }
        Debug.LogError("can not find match itemid");
        return -1;
    }
    //已经使用得数量
    public int GetAlreadyUseCount(int id, int itemId)
    {
        int count = 0;
        //  var datas = _rewardData.Find(a => a.tid == id);
        P_ActMission2063 datas = null;
        for (int i = 0; i < _rewardData.Count; i++)
        {
            P_ActMission2063 temp = _rewardData[i];
            if (temp.tid == id)
            {
                datas = temp;
                break;
            }
        }
        if (datas != null && datas.dataDic != null)
            datas.dataDic.TryGetValue(itemId.ToString(), out count);
        return count;
    }

    //初始化字典
    private void InitGetDic()
    {
        _IndexToId.Clear();
        for (int i = 0; i < _actData.Count; i++)
        {
            var advance = _actData[i].advance;
            var index = _actData[i].index;
            Dictionary<int, int> tempDic = null;
            if (_IndexToId.TryGetValue(advance, out tempDic))
            {
                tempDic.Add(index, _actData[i].id);
            }
            else
            {
                var dic = new Dictionary<int, int>();
                dic.Add(index, _actData[i].id);
                _IndexToId.Add(advance, dic);
            }
        }
    }
    //通过Index取到Id[由服务器可确定]
    public int GetIndexToId(int index)
    {
        Dictionary<int, int> data;
        int id;
        if (_IndexToId.TryGetValue((int)type, out data))
        {
            if (data.TryGetValue(index, out id))
            {
                return id;
            }
        }
        throw new Exception("can not find index :" + index);
    }
    //初始化设置两个奖池
    private void InitRewarPool()
    {
        _selectNormalData.Clear();
        _selectSeniorData.Clear();

        var normalData = _poolData.low_extract_pool;
        var idItem_N = normalData.Split(';');
        for (int i = 0; i < idItem_N.Length; i++)
        {
            if (string.IsNullOrEmpty(idItem_N[i]))
                continue;
            var idItem = idItem_N[i].Split('|');
            var id = Convert.ToInt32(idItem[0]);
            var items = idItem[1].Split(',');
            var tempList = new List<int>();
            for (int j = 0; j < items.Length; j++)
            {
                tempList.Add(Convert.ToInt32(items[j]));
            }
            _selectNormalData.Add(id, tempList);
        }
        var seniorData = _poolData.high_extract_pool;
        var idItem_S = seniorData.Split(';');
        for (int i = 0; i < idItem_S.Length; i++)
        {
            if (string.IsNullOrEmpty(idItem_S[i]))
                continue;
            var idItem = idItem_S[i].Split('|');
            var id = Convert.ToInt32(idItem[0]);
            var items = idItem[1].Split(',');
            var tempList = new List<int>();
            for (int j = 0; j < items.Length; j++)
            {
                tempList.Add(Convert.ToInt32(items[j]));
            }
            _selectSeniorData.Add(id, tempList);
        }
    }
}
public class P_ActMission2063
{
    public int tid;//对应表中的id
    public string data;//itemid count 数据集合
    public Dictionary<string, int> dataDic;//key:itemid value:count[已经使用的数量]
}
public class P_GetReward2063
{
    public List<P_Item3> rewards;//获得的奖励
    public int cost_gold;//花费的氪晶
    public int get_score;//得到的积分
}
public class P_Shop2063
{
    public string get_item;//获得的奖励
    public int cost_score;//花费的积分
}
public class P_ShopItem2063
{
    public int itemid;//id
    public int score;//积分
    public int count;//数量
    public int limit;//最大值
    public int step;// 服务器阶段
}
public class P_Act_2063
{
    public int id;// 道具Id
    public int step;// 服务器阶段
    public int advance;// 扭蛋机级别
    public int index;// 奖池等级
    public int weight;// 权重
    public string reward;// itemId|个数|数量限制
}
public class P_InfoPool
{
    public string high_extract_pool;//高级奖池
    public string low_extract_pool;//低级奖池
}
public enum AdvanceType
{
    Normal = 0,//普通扭蛋机
    Senior = 1//高级扭蛋机
}
