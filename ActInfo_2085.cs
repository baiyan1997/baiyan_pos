using System;
using System.Collections.Generic;
using LitJson;

public class ActInfo_2085 : ActivityInfo
{
    public P_Act2085UniqueInfo UniqueInfo = new P_Act2085UniqueInfo();
    public bool TurnOverAnimCompleted { get; set; }
    public override void InitUnique()
    {
        UniqueInfo.FloorNum = Convert.ToInt32(_data.avalue["cur_height"]);
        UniqueInfo.FinalReward = Convert.ToString(_data.avalue["final_reward"]);
        UniqueInfo.IsGotten = Convert.ToInt32(_data.avalue["is_get"]) == 1;
        //UniqueInfo.RuinChipNum = Convert.ToInt32(_data.avalue["relic_chip_num"]);
        UniqueInfo.StarRuinItems = JsonMapper.ToObject<List<P_StarRuinItem>>(_data.avalue["get_reward_info"].ToString());
    }

    public void RefreshInfoForGoNext(int curFloor, string finalReward)
    {
        UniqueInfo.StarRuinItems.Clear();
        UniqueInfo.FloorNum = curFloor;
        UniqueInfo.FinalReward = finalReward;
        UniqueInfo.IsGotten = false;
        TurnOverAnimCompleted = false;
    }

    public void RefreshInfoForTurnOver(int isGet, string getItem, int ruinChipNum, int pos)
    {
        UniqueInfo.IsGotten = isGet == 1;
        UniqueInfo.StarRuinItems.Add(new P_StarRuinItem() { position = pos, reward = getItem });
        //UniqueInfo.RuinChipNum = ruinChipNum;
        EventCenter.Instance.UpdateActivityUI.Broadcast(ActivityID.StarRuins);
    }
    private void _EventGAIN_ITEM(int opcode, string data)
    {
        string itemList = data;
        var pitems = GlobalUtils.ParseItem(itemList);
        for (int i = 0; i < pitems.Length; i++)
        {
            if (pitems[i].id == ItemId.RuinChipId)//活动2085基地站获得遗迹芯片
            {
                ActivityManager.Instance.RequestUpdateActivityById(_aid);
            }
        }
    }
    public override bool OnInited()
    {
        EventCenter.Instance.AddPushListener(OpcodePush.GAIN_ITEM, _EventGAIN_ITEM);
        return true;
    }
    public override void OnRemove()
    {
        EventCenter.Instance.RemovePushListener(OpcodePush.GAIN_ITEM, _EventGAIN_ITEM);
    }
    public override bool IfUpdateAtHour(int hour)
    {
        return hour == 0;
    }
    public override bool NeedDailyRemind()
    {
        return true;
    }
}

public class P_Act2085UniqueInfo
{
    public int FloorNum { get; set; }//层数
    public bool IsGotten { get; set; }//本层终极礼物是否获得 1 获得| 0 未获得
    public string FinalReward { get; set; }//本层终极礼物 itemId|itemNum
    //public int RuinChipNum { get; set; }//拥有的遗迹芯片数量
    public List<P_StarRuinItem> StarRuinItems { get; set; }
}

public class P_StarRuinItem
{
    public int position { get; set; }//0-24
    public string reward { get; set; }//若是打开了，翻出来的东西 "itemId|itemNum"
}

public class P_GoNextInfo
{
    public int cur_height;
    public string final_reward;
}

public class P_TurnOverInfo
{
    public int is_get;
    public string get_item;
    public int relic_chip_num;
}
