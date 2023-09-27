using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

public class ActInfo_2077 : ActivityInfo
{
    private P_ActInfo_2077_Data _info;

    private P_TurnOverCard _turnOverData;
    public override void InitUnique()
    {
        _info = JsonMapper.ToObject<P_ActInfo_2077_Data>(_data.avalue["data"].ToString());
    }

    //展示一下卡片
    public void ShowSearchCaptActInfo()
    {
        //用P_TurnOverCard接收，但是只有cards字段有数据
        //Rpc.SendWithTouchBlocking<P_TurnOverCard>("getSearchCapActInfo", null, data =>
        //{
        //    if (data.cards == null || data.cards.Count < 15)
        //    {
        //        throw new Exception("showSearchCaptActInfo.cards count should == 15");
        //    }
        //    _info.cards = data.cards;
        //    _info.has_shown = 1;//展示过

        //    //验证下服务器数据
        //    if (_info.cards == null || _info.cards.Count < 15)
        //    {
        //        throw new Exception("wrong  _info == null or _info count < 15");
        //    }
        //    EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        //    //五秒后合上
        //    _Scheduler.Instance.PerformWithDelay(5, () =>
        //    {
        //        for (int i = 0; i < _info.cards.Count; i++)
        //        {
        //            _info.cards[i] = 0;
        //        }
        //        EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        //    });
        //});
        Rpc.SendWithTouchBlocking<P_TurnOverCard>("getSearchCapActInfo", null, On_getSearchCapActInfo_SC);
    }
    private void On_getSearchCapActInfo_SC(P_TurnOverCard data)
    {
        if (data.cards == null || data.cards.Count < 15)
        {
            throw new Exception("showSearchCaptActInfo.cards count should == 15");
        }
        _info.cards = data.cards;
        _info.has_shown = 1;//展示过

        //验证下服务器数据
        if (_info.cards == null || _info.cards.Count < 15)
        {
            throw new Exception("wrong  _info == null or _info count < 15");
        }
        EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        //五秒后合上
        //_Scheduler.Instance.PerformWithDelay(5, () =>
        //{
        //    for (int i = 0; i < _info.cards.Count; i++)
        //    {
        //        _info.cards[i] = 0;
        //    }
        //    EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
        //});
        _Scheduler.Instance.PerformWithDelay(5, OnPerformCB);
    }
    private void OnPerformCB()
    {
        for (int i = 0; i < _info.cards.Count; i++)
        {
            _info.cards[i] = 0;
        }
        EventCenter.Instance.UpdateActivityUI.Broadcast(_data.aid);
    }


    //翻开一张卡片
    public void TurnOverCard(int index)
    {
        Rpc.SendWithTouchBlocking<P_TurnOverCard>("turnOverCard",  Json.ToJsonString(index), data =>
        {
            //返回 抽的卡片
            _info.cards[index] = data.cid;
            EventCenter.Instance.TurnOverCard.Broadcast(1);

            if (data.turn_over_index != null)
            {
                _turnOverData = data;
                _info.wrong_count = data.wrong_count;
                _info.finished = data.finished;
                EventCenter.Instance.SearchCaptActJudge.Broadcast();
            }

            //同步数据
            if (data.cards == null || data.cards.Count < 15)
            {
                throw new Exception("turnOverCard.cards count should == 15");
            }
            _info.cards = data.cards;
            EventCenter.Instance.TurnOverCard.Broadcast(0);
        });
    }
    //掀开判定成功还是失败
    public bool IsSuccessTurnOver()
    {
        if (_turnOverData == null)
            return false;

        if (_turnOverData.turn_over_index == null)
            return false;

        var indexes = _turnOverData.turn_over_index;
        if (indexes.Count == 0)
            return false;

        for (int i = 0; i < indexes.Count; i++)
        {
            if(i == 0)
                continue;

            if (_info.cards[indexes[i]] != _info.cards[indexes[i - 1]])
                return false;
        }
        return true;
    }
    //掀开判定成功还是失败
    public bool IsSameCid(int index1, int index2)
    {
        if (_turnOverData == null)
            return false;

        if (_turnOverData.turn_over_index == null)
            return false;

        var count = _turnOverData.turn_over_index.Count;
        if (count == 0)
            return false;

        if (index1 >= _info.cards.Count || index2 >= _info.cards.Count)
            return false;

        return _info.cards[index1] == _info.cards[index2];
    }

    public List<int> GetTripleCards()
    {
        if (_turnOverData == null)
            return null;
        return _turnOverData.turn_over_index;
    }

    public P_ActInfo_2077_Data GetActData()
    {
        return _info;
    }

    public bool HaveLeftChance()
    {
        return _info.LeftCount() > 0;
    }


    //如果结束了返回提示语
    public string GetEndNote()
    {
        if (_info.finished == 0)
            return string.Empty;

        if (_info.finished == 2)
        {
            return Lang.Get("非常遗憾您没有获得今日奖励");
        }
        return Lang.Get("恭喜您获得今日寻找舰长的奖励，已发放到您邮箱");
    }

    class P_TurnOverCard
    {
        //翻开卡片的cid
        public int cid;
        //同步所有数据
        public List<int> cards;

        //剩余尝试机会
        public int wrong_count;

        //是否结束
        public int finished;

        //翻开的两张或三张是不是一样的呢
        public List<int> turn_over_index;
    }

    public List<int> GetCards()
    {
        return _info.cards;
    }

    public int LeftCount()
    {
        return _info.LeftCount();
    }

    public string GetRewards()
    {
        return _info.rewards;
    }

    public bool NotShown()
    {
        return _info.NotShown();
    }
}
public class P_ActInfo_2077_Data
{
    public int has_shown; //是否给玩家展示过所有的卡
    public int wrong_count;//剩下的错误机会
    public string rewards; //奖励列表
    public List<int> cards;//所有待翻开卡的状态  0-未掀开 非0-cid
    public int finished;//游戏结束 0:未结束 1:赢了 2:输了

    //最大次数
    public const int MaxCount = 6;

    public int LeftCount()
    {
        return MaxCount - wrong_count;
    }

    public bool NotShown()
    {
        return has_shown == 0 && finished == 0;
    }
}