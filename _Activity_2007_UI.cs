using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2007_UI : ActivityUI
{
    private int _aid = 2007;
    private readonly Vector2[] _rewardPos = new Vector2[10];
    private ObjectGroup UI;
    private ActInfo_2007 actInfo;
    private bool _canClick;
    private int _circle;
    private int _speed;
    private int _stopIndex;
    private int _index;
    private int _grids;//总共转的格子数
    private Image _rewardImage;
    private Image _rewardQua;
    private Text _rewardNum;
    private Sequence _seq2;
    private Sequence _seq1;
    private GameObject[] _mask;
    private GameObject[] _obj;
    private int A0;//转动加速度
    private void InitData()
    {
        _circle = 8;//转盘转至少8圈
        _speed = 90;//转盘速度90
        _index = -1;//当前所在位置
        A0 = 2;
        _mask = new GameObject[10];
        _obj = new GameObject[10];
        actInfo = (ActInfo_2007)ActivityManager.Instance.GetActivityInfo(_aid);

    }
    //此次抽奖跑的格子数
    private int GetGrids(int stopIndex)
    {
        return (stopIndex - _index) % 10 + _circle * 10;
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == _aid)
            RefreshLeftTicket();
    }

    private void RefreshLeftTicket()
    {
        UI.Get<Text>("left_times").text = string.Format(Lang.Get("抽奖剩余次数: {0}"), actInfo.LeftTickets());
    }

    private void InitEvent()
    {
        UI.Get<Button>("lottery").onClick.AddListener(StartLottory);
        UI.Get<Button>("confirm_btn").onClick.AddListener(ShowMain1);
    }
    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(RefreshLeftTicket);
        //EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUi);
    }

    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(RefreshLeftTicket);
    }

    private void ShowMain1()
    {
        UI["Main_Mask"].SetActive(true);
        UI["Main_01"].SetActive(true);
        UI["Main_02"].SetActive(false);
    }

    private IEnumerator Coroutine(Action ac)
    {
        float nowV = 0;
        bool flag = false;
        int leftGrid = 18;
        int rnd = UnityEngine.Random.Range(1, 4);
        if (rnd == 1)
            leftGrid = 15;
        if (rnd == 2)
            leftGrid = 18;
        if (rnd == 3)
            leftGrid = 22;
        Debug.Log("====rnd=" + rnd);
        while (true)
        {
            if (nowV < _speed && flag == false)
                nowV = nowV + A0;
            else
                flag = true;

            yield return new WaitForSeconds(1 / nowV);
            _index = _index + 1;
            _grids = _grids - 1;
            if (HasDestroy())
                break;
            _mask[_index % 10].SetActive(true);
            _mask[(_index + 10 - 1) % 10].SetActive(false);
            if (_grids <= leftGrid)
            {
                if (rnd == 1)
                    nowV = nowV / 4 * 3;
                if (rnd == 2)
                    nowV = nowV / 5 * 4;
                if (rnd == 3)
                    nowV = nowV / 6 * 5;
            }
            if (_grids <= 0)
                break;
        }

        yield return new WaitForSeconds(1 / nowV);
        if (!HasDestroy())
        {
            if (ac != null)
                ac();
            _canClick = true;
        }
    }
    private void StartLottory()
    {
        if (_canClick)
        {
            _canClick = false;
            if (actInfo.LeftTickets() <= 0)
            {
                Alert.Ok(Lang.Get("您已经没有抽奖券了哦~"));
                _canClick = true;
            }
            else
            {
                actInfo.SendStartLottory(OnLottoryCB);
            }
        }
    }

    private void OnLottoryCB()
    {
        _stopIndex = actInfo.RewardIndex;
        _grids = GetGrids(_stopIndex);
        RefreshLeftTicket();
        _Scheduler.Instance.StartCoroutine(Coroutine(ShowReward));
    }



    public void ShowReward()
    {
        UI.Get<Transform>("reward_obj").localPosition = new Vector3(_rewardPos[_stopIndex].x, _rewardPos[_stopIndex].y, 0);
        UI.Get<Transform>("reward_obj").localScale = Vector3.one;
        var item = ItemForShow.Create(actInfo._rewards[_stopIndex].itemid,
            actInfo._rewards[_stopIndex].count);
        item.SetIcon(_rewardImage);
        _rewardQua.color = _ColorConfig.GetQuaColorHSV(item.GetQua());
        if (item.GetCount() == 1)
            _rewardNum.text = "";
        else
            _rewardNum.text = "x" + GLobal.NumFormat(item.GetCount());
        UI["confirm_btn"].SetActive(false);
        UI.Get<Text>("reward_title").text = "";
        UI["Main_Mask"].SetActive(false);
        UI["Main_01"].SetActive(false);
        UI["Main_02"].SetActive(true);
        Tween tween1 = UI.Get<Text>("reward_title").DOText(Lang.Get("恭喜您，获得"), 1);
        Tween tween2 = UI.Get<Transform>("reward_obj").DOScale(new Vector3(1.2f, 1.2f, 1), 1);
        Tween tween3 = UI.Get<Transform>("reward_obj").DOLocalMove(new Vector3(0, 30, 0), 1);
        _seq2 = DOTween.Sequence();
        _seq2.Append(tween2).Join(tween3).Append(tween1).AppendCallback(On_seq2AppendCB);
    }
    private void On_seq2AppendCB()
    {
        MessageManager.ShowRewards(actInfo._rewards[_stopIndex].itemid + "|" + actInfo._rewards[_stopIndex].count);
        Uinfo.Instance.AddItem(actInfo._rewards[_stopIndex].itemid, actInfo._rewards[_stopIndex].count);
        UI["confirm_btn"].SetActive(true);
    }
    private void InitUI()
    {
        SetRewardPos();
        _canClick = true;
        UI.Get<Text>("act_time").text = GlobalUtils.ActTimeFormat(actInfo._data.startts, actInfo._data.endts);
        UI.Get<Text>("left_times").text = string.Format(Lang.Get("抽奖剩余次数: {0}"), actInfo.LeftTickets());
        if (actInfo.LeftTime < 0)
            UI.Get<Text>("act_time").text = Lang.Get("活动已经结束");

        _rewardImage = UI.Get<Transform>("reward_obj").Find("icon").GetComponent<Image>();
        _rewardNum = UI.Get<Transform>("reward_obj").Find("text").GetComponent<Text>();
        _rewardQua = UI.Get<Transform>("reward_obj").Find("Img_qua").GetComponent<Image>();
        for (int i = 0; i < 10; i++)
        {
            _mask[i] = UI["Mask" + i];
            _obj[i] = UI["obj" + i];
            var item = ItemForShow.Create(actInfo._rewards[i].itemid, actInfo._rewards[i].count);
            item.SetIcon(_obj[i].transform.Find("icon").GetComponent<Image>());
            _obj[i].transform.Find("Img_qua").GetComponent<Image>().color = _ColorConfig.GetQuaColorHSV(item.GetQua());
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
        UI["lottery"].SetActive(false);
        UI["left_times"].SetActive(false);
        for (int i = 0; i < 10; i++)
        {
            float x = _rewardPos[i].x;
            float y = _rewardPos[i].y;
            Tween tween = _obj[i].transform.DOLocalMove(new Vector3(x, y, 0), 0.4f);
            _seq1.Insert(i * 0.02f + 0.2f, tween);
        }
        _seq1.AppendCallback(Onseq1AppendCB);
    }
    private void Onseq1AppendCB()
    {
        UI["lottery"].SetActive(true);
        UI["left_times"].SetActive(true);
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
    }

    public override void Awake()
    {
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
    }

    public override void OnClose()
    {
    }

    public bool HasDestroy()
    {
        return UI == null;
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
}
