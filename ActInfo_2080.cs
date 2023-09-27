using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;


public class ActInfo_2080 : ActivityInfo
{
    public List<P_Act2080ItemData> ItemList { private set; get; }

    //游戏总次数
    public int PlayTimes { private set; get; }

    //已获得的游戏次数
    public int GainTimes { private set; get; }

    //游戏时间
    public const long TotalGameTime = 30;

    //总体小福占5，中福占2，大福占1.5，炸弹占1，氪晶占0.5,
    //掉落上限 50
    public const int LimitSum = 50;

    //每日获取次数上限5
    public const int LimitTimes = 5;

    //福气值
    public int Score { private set; get; }

    public List<P_Act2080Mission> MissionList { private set; get; }

    public List<P_Act2080Exchange> ExchangeList { private set; get; }


    public override void InitUnique()
    {
        base.InitUnique();

        InitData();
    }

    private void InitData()
    {
        Score = Convert.ToInt32(_data.avalue["lucky_value"].ToString());
        PlayTimes = Convert.ToInt32(_data.avalue["play_count"].ToString());
        ExchangeList = JsonMapper.ToObject<List<P_Act2080Exchange>>(_data.avalue["shop_info"].ToString());
        MissionList = JsonMapper.ToObject<List<P_Act2080Mission>>(_data.avalue["mission_info"].ToString());
        GainTimes = Convert.ToInt32(_data.avalue["gain_count"].ToString());

        ItemList = new List<P_Act2080ItemData>();

        //上限一共50个，其中小福25，中福10，大福7，炸弹5，氪晶占3
        for (int i = 0; i < LimitSum; i++)
        {
            if(i < 25)
            {
                ItemList.Add(new P_Act2080ItemData(Act2080Type.FuSmall));
            }
            else if( i < 35)
            {
                ItemList.Add(new P_Act2080ItemData(Act2080Type.FuMiddle));
            }
            else if(i < 42)
            {
                ItemList.Add(new P_Act2080ItemData(Act2080Type.FuBig));
            }
            else if( i < 47)
            {
                ItemList.Add(new P_Act2080ItemData(Act2080Type.Bomb));
            }
            else
            {
                ItemList.Add(new P_Act2080ItemData(Act2080Type.Kr));
            }
        }
    }

    public override bool IsAvaliable()
    {
        return PlayTimes > 0;
    }

    //提交游戏结果
    public void RequestEndGame(int score, int kr, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_Act2080Result>("endNaFuGames", Json.ToJsonString(score, kr), data =>
          {
              Score = data.lucky_value;
              PlayTimes = data.play_count;

              EventCenter.Instance.RemindActivity.Broadcast(_aid, IsAvaliable());

              if (callback != null)
                  callback();
          });
    }

    //购买商品
    public void RequestBuy(int id, Action callback = null)
    {
        Rpc.SendWithTouchBlocking<P_ActBuyData>("buyGoodsInFuQiShop", Json.ToJsonString(id), data =>
        {
            Uinfo.Instance.AddItemAndShow(data.get_item);
            Score = data.lucky_value;

            ActivityManager.Instance.RequestUpdateActivityById(2080);

            if (callback != null)
                callback();

        });
    }
}

public class P_Act2080Mission
{
    public int mission_group;
    public long end_ts;
    public bool finished;
    public bool get_reward;
    public int do_number;
    public int tid;
}

public class P_Act2080Exchange
{
    public int id;
    public int num;
}

public class P_Act2080Result
{
    public int lucky_value;
    public int play_count;
}

public class P_ActBuyData
{
    public string get_item;
    public int lucky_value;
}

public enum Act2080Type
{
    FuSmall = 0,
    FuMiddle = 1,
    FuBig = 2,
    Kr = 3,
    Bomb = 4
}

public class P_Act2080ItemData
{
    public Act2080Type Type;
    public int Score { private set; get; }

    public float Duration { private set; get; }

    public Vector3 Scale { private set; get; }

    public int Kr { private set; get; }

    public P_Act2080ItemData(Act2080Type type)
    {
        Type = type;
        switch (Type)
        {
            case Act2080Type.FuSmall:
                {
                    Score = 1;
                    Duration = 1.0f;
                    Scale = new Vector3(0.6f, 0.6f, 1);
                    Kr = 0;
                }
                break;
            case Act2080Type.FuMiddle:
                {
                    Score = 3;
                    Duration = 1.0f;
                    Scale = new Vector3(0.8f, 0.8f, 1);
                    Kr = 0;
                }
                break;
            case Act2080Type.FuBig:
                {
                    Score = 5;
                    Duration = 1.0f;
                    Scale = new Vector3(1, 1, 1);
                    Kr = 0;
                }
                break;
            case Act2080Type.Kr:
                {
                    Score = 0;
                    Duration = 0.6f;
                    Scale = new Vector3(1, 1, 1);
                    Kr = 2;
                }
                break;
            case Act2080Type.Bomb:
                {
                    Score = 0;
                    Duration = 0.6f;
                    Scale = new Vector3(1, 1, 1);
                    Kr = 0;
                }
                break;
        }
    }
}
