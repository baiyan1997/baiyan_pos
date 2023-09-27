using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2044_UI : ActivityUI
{
    private int _aid = 2044;
    private ActInfo_2044 actInfo;
    private ObjectGroup UI;
    private List<int> _stopIndex;
    private readonly Vector2[] _rewardPos = new Vector2[11];
    private readonly Vector2[] _modelPos = new Vector2[11];
    private bool _canClick;
    private int _speed;
    private int _index;
    private int _grids;//总共转的格子数
    private Sequence _seq2;
    private Sequence _seq1;
    private GameObject[] _mask;
    private GameObject[] _obj;
    private GameObject[] _model;
    private int A0;//转动加速度
    private int[] indexRule = { 0, 1, 2, 3, 4, 5, 6, 10, 7, 8, 9 };//转动规则
    private void InitData()
    {
        _speed = 90;//转盘速度90
        _index = -1;//当前所在位置
        A0 = 5;
        _mask = new GameObject[11];
        _obj = new GameObject[11];
        _model = new GameObject[11];
        actInfo = (ActInfo_2044)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    private void InitEvent()
    {
        UI.Get<Button>("OnceLottery").onClick.AddListener(OnOnceLotteryClick);
        UI.Get<Button>("TenLottery").onClick.AddListener(OnTenLotteryClick);
        UI.Get<Button>("confirm").onClick.AddListener(OnconfirmClick);
    }
    private void OnOnceLotteryClick()
    {
        var opcode = PromptOpcode.Lottery2044Once;
        bool _prompt = Uinfo.Instance.Prompt.GetValue(opcode);
        if (!_prompt)
        {
            if (ItemHelper.IsCountEnough(ItemId.Gold, actInfo.once_price))
            {
                StartLottery(1);
            }
        }
        else
        {
            string str = "购买";
            _AlertWithPrompt.YesNo(string.Format(Lang.Get("确定花费{0}氪晶{1}10k资源×1转动一次？"), actInfo.once_price, str), d =>
            {
                d.SetYesCallbackWithPrompt(() =>
                {
                    Uinfo.Instance.Prompt.SetPrompt(opcode, d.setPrompt);
                    if (ItemHelper.IsCountEnough(ItemId.Gold, actInfo.once_price))
                    {
                        d.Close();
                        StartLottery(1);
                    }
                    else
                    {
                        d.Close();
                    }
                });
            }, actInfo.once_price, _prompt);
        }
    }
    private void OnTenLotteryClick()
    {
        var opcode = PromptOpcode.Lottert2044Ten;
        bool _prompt = Uinfo.Instance.Prompt.GetValue(opcode);
        if (!_prompt)
        {
            if (ItemHelper.IsCountEnough(ItemId.Gold, actInfo.once_price))
            {
                StartLottery(0);
            }
        }
        else
        {
            string str = "购买";
            _AlertWithPrompt.YesNo(string.Format(Lang.Get("确定花费{0}氪晶{1}10k资源×10转动十次？"), actInfo.ten_times_price, str), d =>
            {
                d.SetYesCallbackWithPrompt(() =>
                {
                    Uinfo.Instance.Prompt.SetPrompt(opcode, d.setPrompt);
                    if (ItemHelper.IsCountEnough(ItemId.Gold, actInfo.ten_times_price))
                    {
                        d.Close();
                        StartLottery(0);
                    }
                    else
                    {
                        d.Close();
                    }
                });
            }, actInfo.ten_times_price, _prompt);
        }
    }
    private void OnconfirmClick()
    {
        UI["Main_Mask"].SetActive(true);
        UI["Main_01"].SetActive(true);
        UI["Main_02"].SetActive(false);
    }


    public override void InitListener()
    {
        base.InitListener();
    }
    private IEnumerator Coroutine(Action ac)
    {
        bool flag = false;
        float nowV = 0;
        //for (int i = 0; i < _stopIndex.Count; i++)
        //{
        //    if (i == 0)
        //    {
        //        _grids = (GetGrids(_stopIndex[i]) - _index) % 11 + 11 * 2;
        //    }
        //    else
        //    {
        //        if (GetGrids(_stopIndex[i]) > _index)
        //        {
        //            _grids = GetGrids(_stopIndex[i] - _index) % 11;
        //            flag = true;
        //        }
        //        else
        //        {
        //            _grids = (GetGrids(_stopIndex[i]) - _index) % 11 + 11;
        //            flag = false;
        //        }
        //    }
        //    while (true)
        //    {
        //        if (nowV < _speed)
        //            nowV = nowV + A0;
        //        yield return new WaitForSeconds(1 / nowV);
        //        _index = _index + 1;
        //        _grids = _grids - 1;
        //        _mask[indexRule[_index % 11]].SetActive(true);
        //        _mask[indexRule[(_index + 11 - 1) % 11]].SetActive(false);
        //        if (flag)
        //            nowV = nowV / 4 * 3;
        //        if (_grids <= 0)
        //            break;
        //    }
        //    _obj[_stopIndex[i]].transform.Find<GameObject>("PFB_obj_flashing").SetActive(true);
        //    _obj[_stopIndex[i]].transform.Find<GameObject>("Image").SetActive(false);
        //    _obj[_stopIndex[i]].transform.Find<GameObject>("Image1").SetActive(true);
        //    yield return new WaitForSeconds(0.2f);
        //    _obj[_stopIndex[i]].transform.Find<GameObject>("PFB_obj_flashing").SetActive(false);
        //}
        _grids = (GetGrids(_stopIndex[0]) - _index) % 11 + 11 * 5;
        while (true)
        {
            if (nowV < _speed)
                nowV = nowV + A0;
            yield return new WaitForSeconds(1 / nowV);
            _index = _index + 1;
            _grids = _grids - 1;
            _mask[indexRule[_index % 11]].SetActive(true);
            _mask[indexRule[(_index + 11 - 1) % 11]].SetActive(false);
            if (_grids <= 0)
                break;
        }
        yield return new WaitForSeconds(0.5f);

        if (ac != null)
            ac();
        _canClick = true;
    }
    private int GetGrids(int index)
    {
        for (int i = 0; i < indexRule.Length; i++)
        {
            if (index == indexRule[i])
                return i;
        }
        return -1;
    }
    private void StartLottery(int type)
    {
        if (_canClick)
        {
            _canClick = false;
            actInfo.SendStartLottory(type, OnStartLottoryCB);
        }
    }
    private void OnStartLottoryCB()
    {
        _stopIndex = actInfo.RewardIndex;
        _Scheduler.Instance.StartCoroutine(Coroutine(ShowReward));
    }

    public void ShowReward()
    {
        for (int i = 0; i < 11; i++)
        {
            _model[i] = UI["model" + i];
            _model[i].SetActive(false);
            _obj[i].transform.Find<GameObject>("Image").SetActive(true);
            _obj[i].transform.Find<GameObject>("Image1").SetActive(false);
        }
        for (int i = 0; i < actInfo.Rewards.Length; i++)
        {
            var item = ItemForShow.Create(actInfo.Rewards[i].itemid, actInfo.Rewards[i].count);
            item.SetIcon(_model[i].transform.Find("icon").GetComponent<Image>());
            _model[i].transform.Find("Img_qua").GetComponent<Image>().color = _ColorConfig.GetQuaColorHSV(item.GetQua());
            var trans = _model[i].transform.Find<Transform>("icon");
            var i1 = i;
            trans.GetComponent<Button>().onClick.SetListener(() =>
            {
                ItemHelper.ShowTip(actInfo.Rewards[i1].itemid, actInfo.Rewards[i1].count, trans);
            });
            Text t = _model[i].transform.Find("Text").GetComponent<Text>();
            if (actInfo.Rewards[i].count == 1)
                t.text = "";
            else
                t.text = "x" + GLobal.NumFormat(item.GetCount());
        }
        TweenEffect2();
        UI["confirm"].SetActive(false);
        UI.Get<Text>("reward_title").text = "";
        UI["Main_Mask"].SetActive(false);
        UI["Main_01"].SetActive(false);
        UI["Main_02"].SetActive(true);
        Tween tween1 = UI.Get<Text>("reward_title").DOText(Lang.Get("恭喜您，获得以下物品"), 0.5f);
        _seq2 = DOTween.Sequence();
        _seq2.Append(tween1).AppendCallback(On_seq2AppendCB);
    }
    private void On_seq2AppendCB()
    {
        UI["confirm"].SetActive(true);
        for (int i = 0; i < actInfo.Rewards.Length; i++)
        {
            Uinfo.Instance.AddItem(actInfo.Rewards[i].itemid, actInfo.Rewards[i].count);
        }
        MessageManager.ShowRewards(actInfo.Rewards);
    }

    private void InitUI()
    {
        SetRewardPos();
        _canClick = true;
        UI.Get<Text>("LotteryDes").text = actInfo._desc;
        UI["OnceLottery"].transform.Find<Text>("costCount").text = actInfo.once_price.ToString();
        UI["TenLottery"].transform.Find<Text>("costCount").text = actInfo.ten_times_price.ToString();
        UI.Get<Text>("Text_Time").text = GlobalUtils.ActTimeFormat(actInfo._data.startts, actInfo._data.endts, true, true);
        if (actInfo.LeftTime < 0)
            UI.Get<Text>("Text_Time").text = Lang.Get("活动已经结束");
        for (int i = 0; i < 11; i++)
        {
            _mask[i] = UI["Mask" + i];
            _obj[i] = UI["obj" + i];
            var item = ItemForShow.Create(actInfo._rewards[i].itemid, actInfo._rewards[i].count);
            if (i < 10)
            {
                item.SetIcon(_obj[i].transform.Find("icon").GetComponent<Image>());
            }
            _obj[i].transform.Find("Img_qua").GetComponent<Image>().color = _ColorConfig.GetQuaColorHSV(item.GetQua());
            _obj[i].transform.Find<GameObject>("Image").SetActive(true);
            _obj[i].transform.Find<GameObject>("Image1").SetActive(false);
            var trans = _obj[i].transform.Find<Transform>("icon");
            var i1 = i;
            trans.GetComponent<Button>().onClick.SetListener(() =>
            {
                ItemHelper.ShowTip(actInfo._rewards[i1].itemid, actInfo._rewards[i1].count, trans);
            });
            Text t = _obj[i].transform.Find("Text").GetComponent<Text>();
            if (actInfo._rewards[i].count == 1)
                t.text = "";
            else
                t.text = "x" + GLobal.NumFormat(item.GetCount());
            _mask[i].SetActive(false);
        }
        //第一次打开效果
        TweenEffect1();
    }
    private void TweenEffect1()
    {
        _seq1 = DOTween.Sequence();
        for (int i = 0; i < 11; i++)
        {
            float x = _rewardPos[i].x;
            float y = _rewardPos[i].y;
            Tween tween = _obj[i].transform.DOLocalMove(new Vector3(x, y, 0), 0.4f);
            _seq1.Insert(i * 0.02f + 0.2f, tween);
        }
    }
    private void TweenEffect2()
    {
        for (int i = 0; i < actInfo.Rewards.Length; i++)
        {
            float x = _modelPos[i].x;
            float y = _modelPos[i].y;
            _model[i].SetActive(true);
            _model[i].transform.DOLocalMove(new Vector3(x, y, 0), 0.4f);
        }
    }
    public override void OnCreate()
    {
        UI = gameObject.GetComponent<ObjectGroup>();
        InitData();
        InitEvent();
        //InitListener();
        InitUI();
    }

    public override void OnShow()
    {
        if (!actInfo.isShow)
        {
            actInfo.isShow = true;
            EventCenter.Instance.RemindActivity.Broadcast(_aid, actInfo.IsAvaliable());
            EventCenter.Instance.UpdateActivityUI.Broadcast(_aid);
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_seq1 != null)
        {
            _seq1.Kill();
            _seq1 = null;
        }
        if (_seq2 != null)
        {
            _seq2.Kill();
            _seq2 = null;
        }
    }
    private void SetRewardPos()
    {
        _rewardPos[0] = new Vector2(-198, 134);
        _rewardPos[1] = new Vector2(-66, 134);
        _rewardPos[2] = new Vector2(66, 134);
        _rewardPos[3] = new Vector2(198, 134);
        _rewardPos[4] = new Vector2(198, 0);
        _rewardPos[5] = new Vector2(198, -134);
        _rewardPos[6] = new Vector2(66, -134);
        _rewardPos[7] = new Vector2(-66, -134);
        _rewardPos[8] = new Vector2(-198, -134);
        _rewardPos[9] = new Vector2(-198, 0);
        _rewardPos[10] = new Vector2(0, 0);

        _modelPos[0] = new Vector2(-198, 134);
        _modelPos[1] = new Vector2(-66, 134);
        _modelPos[2] = new Vector2(66, 134);
        _modelPos[3] = new Vector2(198, 134);
        _modelPos[4] = new Vector2(-198, 0);
        _modelPos[5] = new Vector2(-66, 0);
        _modelPos[6] = new Vector2(66, 0);
        _modelPos[7] = new Vector2(198, 0);
        _modelPos[8] = new Vector2(-198, -134);
        _modelPos[9] = new Vector2(-66, -134);
        _modelPos[10] = new Vector2(66, -134);
    }
}

