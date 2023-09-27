using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

public class _Activity_2080_UI : ActivityUI
{
    private Text _leftTimeText;
    private Button _shopBtn;
    private Button _countBtn;
    private Button _helpBtn;
    //游戏界面
    private Image _captainHead;
    private Text _captainName;
    private GameObject _playGo;
    private Text _scoreText1;
    private Text _gameTimeText;
    private GameObject _goalPref;
    private Transform[] _diTrans;
    private Transform _listTrans;
    private string[] _goalImgesPath;
    private Button[] _boxBtnList;
    private Vector3[] _fromPosList = new Vector3[]
      {
         new Vector3(-220,188,0),
         new Vector3(0,188,0),
         new Vector3(220,188,0),
       };
    private Vector3[] _endPosList = new Vector3[]
     {
         new Vector3(-220,-158,0),
         new Vector3(0,-178,0),
         new Vector3(220,-158,0),
     };
    private Vector2[] _rangHeight = new Vector2[]
    {
         new Vector3(-190,-126),
         new Vector3(-210,-146),
         new Vector3(-190,-126),
    };

    private Button _maskBtn;
    //准备界面
    private GameObject _readyGo;
    private Button _playBtn;
    private Text _countText;
    private Text _scoreText2;
    //结算
    private GameObject _tip1;
    private Text _scoreText;
    private Text _krText;
    private Button _getBtn;
    //次数提示
    private GameObject _tip2;
    private Text _tipText;
    private Text _contentText1;
    private Text _contentText2;
    private Button _btn1;
    private Button _btn2;
    //商店
    private GameObject _shopGo;
    private ListView _listView;
    private Button _closeShopBtn;
    private Text _shopScore;

    private ActInfo_2080 _actInfo;
    private const int _aid = 2080;
    private Pool<GameObject> _pool;
    private bool _startGame;
    private List<P_Act2080ItemData> _dataList = new List<P_Act2080ItemData>();
    private Coroutine _coroutine;
    private Coroutine _coroutine2;
    private int _getScore;
    private int _getKr;
    private long _gameEndTime;
    private int _capId;
    private List<Transform> _allShowItemList = new List<Transform>();
    private bool[] _isHit = new bool[] { false, false, false };
    private long _getTime = 0;

    public override void OnCreate()
    {
        _playGo = transform.Find<GameObject>("Mid_01");
        _readyGo = transform.Find<GameObject>("Mid_02");
        _leftTimeText = transform.Find<JDText>("TextTime");
        _shopBtn = transform.Find<Button>("BtnShop");
        _countBtn = transform.Find<Button>("BtnCount");
        _scoreText1 = transform.Find<JDText>("Mid_01/TextScore");
        _scoreText2 = transform.Find<JDText>("Mid_02/TextScore");
        _countText = transform.Find<JDText>("Mid_02/TextCount");
        _gameTimeText = transform.Find<JDText>("Mid_01/Title/TextTime");
        _goalPref = transform.Find<GameObject>("Mid_01/GoalPref");
        _helpBtn = transform.Find<Button>("ButtonHelp");
        _captainHead = transform.Find<Image>("Mid_02/Icon/Img_Hero");
        _captainName = transform.Find<JDText>("Mid_02/TextCapName");
        _boxBtnList = new Button[]
        {
           transform.Find<Button>("Mid_01/01/Pirate"),
           transform.Find<Button>("Mid_01/02/Pirate"),
           transform.Find<Button>("Mid_01/03/Pirate"),
        };
        _diTrans = new Transform[]
        {
            transform.Find("Mid_01/01/Hero"),
            transform.Find("Mid_01/02/Hero"),
            transform.Find("Mid_01/03/Hero"),
        };
        _playBtn = transform.Find<Button>("Mid_02/BtnPlay");
        _tip1 = transform.Find<GameObject>("Tip1");
        _tip2 = transform.Find<GameObject>("Tip2");
        _scoreText = transform.Find<JDText>("Tip1/TextScore/Text");
        _krText = transform.Find<Text>("Tip1/Icon/TextKr");
        _getBtn = transform.Find<Button>("Tip1/Button");
        _tipText = transform.Find<JDText>("Tip2/01/Text");
        _contentText1 = transform.Find<Text>("Tip2/02/Image/Text");
        _contentText2 = transform.Find<Text>("Tip2/03/Image/Text");
        _btn1 = transform.Find<Button>("Tip2/02/Image/Button");
        _btn2 = transform.Find<Button>("Tip2/03/Image/Button");
        _maskBtn = transform.Find<Button>("MaskGo");
        _listTrans = transform.Find("Mid_01/GoalList");
        _closeShopBtn = transform.Find<Button>("Shop/CloseBtn");
        _shopGo = transform.Find<GameObject>("Shop");
        _shopScore = transform.Find<JDText>("Shop/TextScore");
        _goalImgesPath = new string[]
        {
            "Activity/imgactivity_2080_03",
            "Icon/icon_149_02",
            "Activity/imgactivity_2080_04",
        };

        _pool = PoolFactory.Create(() =>
        {
            return Object.Instantiate(_goalPref, _listTrans);
        }, goalGo => goalGo.SetActive(true), goalGo => goalGo.SetActive(false), 3);

        _listView = ListView.Create<Act2080ShopItem>(_shopGo.transform.Find("Scroll View"));

        InitEvent();
        //InitListener();
    }

    private void InitEvent()
    {
        _shopBtn.onClick.SetListener(SetShopUI_true);
        _countBtn.onClick.SetListener(SetCountUI_true);
        _closeShopBtn.onClick.SetListener(SetShopUI_false);
        _playBtn.onClick.SetListener(On_playBtnClick);
        //领取
        _getBtn.onClick.SetListener(On_getBtnClick);
        //跳到世界地图上
        _btn1.onClick.SetListener(On_btn1Click);
        //和叛军副本界面
        _btn2.onClick.SetListener(On_btn2Click);
        _maskBtn.onClick.SetListener(On_maskBtnClick);
        _helpBtn.onClick.SetListener(On_helpBtnClick);
        for (int i = 0; i < _boxBtnList.Length; i++)
        {
            int index = i;
            _boxBtnList[index].onClick.SetListener(() =>
            {
                int type = CheckDropByPos(index);
                if (type < 0)
                    return;
                if (type == 4)
                {
                    _startGame = false;
                    _Scheduler.Instance.PerformWithDelay(0.2f, () =>
                    {
                        SetResultUI(true);
                    });
                }
                else if (type < 4)
                {
                    HitHeadByIndex(index, type);
                }
            });
        }
    }
    private void SetShopUI_true()
    {
        SetShopUI(true);
    }
    private void SetShopUI_false()
    {
        SetShopUI(false);
    }
    private void SetCountUI_true()
    {
        SetCountUI(true);
    }
    private void On_playBtnClick()
    {
        if (_actInfo == null)
            return;
        if (_actInfo.PlayTimes <= 0)
        {
            Alert.Ok(Lang.Get("今日剩余游戏次数不足，无法进行游戏"));
            return;
        }
        SetGameSate(true);
    }
    private void On_getBtnClick()
    {
        if (_actInfo == null)
            return;
        if (TimeManager.ServerTimestamp - _getTime < 2.0)
        {
            Alert.Ok(Lang.Get("请勿快速重复领取"));
            return;
        }
        _getTime = TimeManager.ServerTimestamp;
        _actInfo.RequestEndGame(_getScore, _getKr, OnRequestEndGameCB);
    }
    private void OnRequestEndGameCB()
    {
        SetResultUI(false);
        SetGameSate(false);
    }
    private void On_btn1Click()
    {
        SetCountUI(false);
        WorldConfig.WorldController.EnterWorld();
    }
    private void On_btn2Click()
    {
        SetCountUI(false);
        DialogManager.ShowAsyn<_D_Stages>(On_btn2DialogShowAsynCB);
    }
    private void On_btn2DialogShowAsynCB(_D_Stages d)
    {
        d?.OnShow();
    }
    private void On_maskBtnClick()
    {
        SetCountUI(false);
        SetShopUI(false);
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpBtnDialogShowAsynCB);
    }
    private void On_helpBtnDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2080, _helpBtn.transform.position, Direction.LeftDown, 350);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_pool != null)
        {
            _pool.Clear();
            _pool = null;
        }
        _dataList.Clear();
        _allShowItemList.Clear();
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2080)ActivityManager.Instance.GetActivityInfo(_aid);
        if (_actInfo == null)
            return;

        SetGameSate(false);
        SetCountUI(false);

        SetShopUI(_shopGo.activeSelf);
    }

    public override void OnClose()
    {
        base.OnClose();
        if (_coroutine != null)
        {
            _Scheduler.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }
        if (_coroutine2 != null)
        {
            _Scheduler.Instance.StopCoroutine(_coroutine2);
            _coroutine2 = null;
        }

        _startGame = false;
        _getKr = 0;
        _getScore = 0;
        for (int i = 0; i < _isHit.Length; i++)
            _isHit[i] = false;
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (_aid != aid)
            return;

        if (!gameObject.activeSelf)
            return;

        if (_startGame)
            return;

        OnShow();
    }

    private void SetGameSate(bool isPlay)
    {
        if (isPlay)
        {
            SetReadyUI(false);
            SetGameUI(true);
            DoGame();
        }
        else
        {
            SetReadyUI(true);
            SetGameUI(false);
        }
    }

    private void SetReadyUI(bool isShow)
    {
        if (isShow)
        {
            SatCaptianByRandom();
            _readyGo.SetActive(true);
            _countText.text = Lang.Get("今日剩余游戏次数:{0}", _actInfo.PlayTimes);
            _scoreText2.text = Lang.Get("拥有福气值:{0}", _actInfo.Score);
        }
        else
        {
            _readyGo.SetActive(false);
        }
    }

    private void SetGameUI(bool isShow)
    {
        if (isShow)
        {
            _playGo.SetActive(true);
            _scoreText1.text = Lang.Get("本轮获得福气值:{0}", _actInfo.Score);
            _startGame = false;
            HideAllDi();
            _shopBtn.gameObject.SetActive(false);
            _countBtn.gameObject.SetActive(false);
        }
        else
        {
            _shopBtn.gameObject.SetActive(true);
            _countBtn.gameObject.SetActive(true);
            _playGo.SetActive(false);
        }
    }

    private void DoGame()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();

        _getScore = 0;
        _getKr = 0;
        for (int i = 0; i < _isHit.Length; i++)
            _isHit[i] = false;
        _allShowItemList.Clear();
        _coroutine = _Scheduler.Instance.StartCoroutine(DoAnim());
        _coroutine2 = _Scheduler.Instance.StartCoroutine(DoHitTip());
    }

    private IEnumerator DoAnim()
    {
        float time = Time.realtimeSinceStartup;
        _gameEndTime = TimeManager.ServerTimestamp + ActInfo_2080.TotalGameTime;
        int i = 0;
        _startGame = true;
        _dataList.Clear();
        _dataList.AddRange(_actInfo.ItemList);

        while (Time.realtimeSinceStartup - time <= ActInfo_2080.TotalGameTime)
        {
            if (!_startGame)
                break;
            int pos = Random.Range(0, 3);
            DoAnimByPos(pos);
            float waiteTime = Random.Range(0.7f, 1.2f);
            yield return new WaitForSeconds(waiteTime);
        }

        if (_startGame)
        {
            _startGame = false;
            _actInfo = (ActInfo_2080)ActivityManager.Instance.GetActivityInfo(_aid);
            SetResultUI(true);
        }
    }

    private void DoAnimByPos(int Pos)
    {
        if (_dataList.Count <= 0)
            return;

        P_Act2080ItemData data = GetItem();
        GameObject go = _pool.Get();
        go.SetActive(true);
        RectTransform rect = go.GetComponent<RectTransform>();
        Image icon = go.GetComponentInChildren<Image>();
        go.name = (int)data.Type + "";

        if ((int)data.Type < 3)
            UIHelper.SetImageSprite(icon, _goalImgesPath[0]);
        else
            UIHelper.SetImageSprite(icon, _goalImgesPath[(int)data.Type - 2]);

        icon.transform.localScale = data.Scale;

        _allShowItemList.Add(rect);
        rect.anchoredPosition = _fromPosList[Pos];
        Tween tween = rect.DOLocalMoveY(_endPosList[Pos].y, data.Duration).OnComplete(() =>
         {
             _Scheduler.Instance.PerformWithDelay(0.2f, () =>
             {
                 _allShowItemList.Remove(rect);
                 _pool.Put(go);
             });
         });
    }

    //每帧检测是否展示舰长头像提示
    private IEnumerator DoHitTip()
    {
        while (_startGame)
        {
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < 3; i++)
            {
                if (CheckDropByPos(i) >= 0)
                {
                    _diTrans[i].gameObject.SetActive(true);
                }
                else
                {
                    if (!_isHit[i])
                        _diTrans[i].gameObject.SetActive(false);
                }
            }
        }
    }

    //判断是否有道具掉入Pos对应的盘子
    private int CheckDropByPos(int pos)
    {
        for (int i = 0; i < _allShowItemList.Count; i++)
        {
            Vector2 postion = _allShowItemList[i].GetComponent<RectTransform>().anchoredPosition;
            if (postion.y >= _rangHeight[pos].x && postion.y <= _rangHeight[pos].y && postion.x == _fromPosList[pos].x)
            {
                return int.Parse(_allShowItemList[i].name);
            }
        }
        return -1;
    }

    private void CalculateScore(Act2080Type type)
    {
        int addScore = 0;
        int addKr = 0;

        switch (type)
        {
            case Act2080Type.FuSmall:
                {
                    addScore = 1;
                    addKr = 0;
                }
                break;
            case Act2080Type.FuMiddle:
                {
                    addScore = 3;
                    addKr = 0;
                }
                break;
            case Act2080Type.FuBig:
                {
                    addScore = 5;
                    addKr = 0;
                }
                break;
            case Act2080Type.Kr:
                {
                    addScore = 0;
                    addKr = 2;
                }
                break;
            case Act2080Type.Bomb:
                {
                    addScore = 0;
                    addKr = 0;
                }
                break;
        }
        _getScore += addScore;
        _getKr += addKr;
        string msg = "";
        if (addKr > 0)
        {
            msg += Lang.Get("氪晶 + {0}   ", addKr);
        }
        else if (addScore > 0)
        {
            msg += Lang.Get("福气值 + {0}   ", addScore);
        }

        if (!string.IsNullOrEmpty(msg))
            MessageManager.Show(msg);
    }

    private void SatCaptianByRandom()
    {
        int count = CaptainInfo.Instance.CaptainList.Count;
        if (count > 0)
        {
            int index = Random.Range(0, count);
            _capId = CaptainInfo.Instance.CaptainList[index].captain_id;
        }
        else
        {
            List<int> clist = Cfg.Captain.GetAllCaptainIdList();
            int index = Random.Range(0, count);
            _capId = clist[index];
        }

        Cfg.Captain.SetCaptainPhoto(_captainHead, _capId, 0);
        _captainName.text = Lang.Get("执行本次任务舰长：{0}", Cfg.Captain.GetCaptainName(_capId));

        for (int i = 0; i < _diTrans.Length; i++)
        {
            Cfg.Captain.SetCaptainPhoto(_diTrans[i].Find<Image>("Icon/Mask/Icon"), _capId, 0);
        }
    }

    //丁舰长敲击效果
    private void HitHeadByIndex(int Pos, int type)
    {
        if (_isHit[Pos])
            return;

        CalculateScore((Act2080Type)type);
        _isHit[Pos] = true;
        AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2094);
        Transform dTrans = _diTrans[Pos];
        dTrans.gameObject.SetActive(true);
        Tweener tween1 = dTrans.DOLocalMove(new Vector3(0, -32, 0), 0.2f);
        Tweener tween2 = dTrans.DOLocalMove(new Vector3(70, 10, 0), 0.2f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(tween1).AppendInterval(0.08f).Append(tween2).AppendCallback(() =>
        {
            dTrans.gameObject.SetActive(false);
            _isHit[Pos] = false;
        });
    }

    //隐藏丁舰长
    private void HideAllDi()
    {
        for (int i = 0; i < _diTrans.Length; i++)
            _diTrans[i].gameObject.SetActive(false);
    }

    //结算界面
    private void SetResultUI(bool isShow)
    {
        if (isShow)
        {
            HideAllDi();
            _tip1.SetActive(true);
            _scoreText.text = Lang.Get("福气值 +{0}", _getScore);
            _krText.text = string.Format("+{0}", _getKr);
        }
        else
        {
            _tip1.SetActive(false);
        }
    }

    //额外次数界面
    private void SetCountUI(bool isShow)
    {
        if (isShow)
        {
            _maskBtn.gameObject.SetActive(true);
            _tip2.SetActive(true);
            _tipText.text = Lang.Get("今日可获得额外游戏次数（{0}/{1}）", _actInfo.GainTimes, ActInfo_2080.LimitTimes);

            if (_actInfo.GainTimes < ActInfo_2080.LimitTimes)
            {
                _btn1.gameObject.SetActive(true);
                _btn2.gameObject.SetActive(true);
            }
            else
            {
                _btn1.gameObject.SetActive(false);
                _btn2.gameObject.SetActive(false);
            }

            cfg_act_2080_task task1 = Cfg.Activity2080.GetTaskData(_actInfo.MissionList[0].tid);
            cfg_act_2080_task task2 = Cfg.Activity2080.GetTaskData(_actInfo.MissionList[1].tid);
            _contentText1.text = Lang.Get(task1.name + " (<Color=#00ff33>{0}</Color>/{1})", _actInfo.MissionList[0].do_number, task1.need_count);
            _contentText2.text = Lang.Get(task2.name + " (<Color=#00ff33>{0}</Color>/{1})", _actInfo.MissionList[1].do_number, task2.need_count);
        }
        else
        {
            _maskBtn.gameObject.SetActive(false);
            _tip2.SetActive(false);
        }
    }

    //随机取出掉落目标物体
    public P_Act2080ItemData GetItem()
    {
        int index = Random.Range(0, _dataList.Count);
        P_Act2080ItemData data = _dataList[index];
        _dataList.RemoveAt(index);
        return data;
    }

    //商店
    private void SetShopUI(bool isShow)
    {
        _actInfo = (ActInfo_2080)ActivityManager.Instance.GetActivityInfo(_aid);
        if (_actInfo == null)
            return;

        if (isShow)
        {
            _shopScore.text = Lang.Get("我的福气值:{0}", _actInfo.Score);

            _shopGo.SetActive(true);

            _listView.Clear();

            for (int i = 0; i < _actInfo.ExchangeList.Count; i++)
            {
                _listView.AddItem<Act2080ShopItem>().Refresh(_actInfo.ExchangeList[i]);
            }

            _maskBtn.gameObject.SetActive(true);
        }
        else
        {
            _shopGo.SetActive(false);
            _maskBtn.gameObject.SetActive(false);
        }
    }

    public override void UpdateTime(long obj)
    {
        base.UpdateTime(obj);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!gameObject.activeSelf)
            return;

        if (_actInfo == null)
            return;

        if (_actInfo.LeftTime >= 0)
        {
            _leftTimeText.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _leftTimeText.text = Lang.Get("活动已经结束");
        }

        //如果游戏状态
        if (_startGame)
        {
            long lefttime = _gameEndTime - TimeManager.ServerTimestamp;
            if (lefttime < 0)
            {
                _gameTimeText.text = Lang.Get("游戏结束");
            }
            else
            {
                _gameTimeText.text = Lang.Get("游戏倒计时:{0}秒", (int)lefttime);
            }
        }
        else
        {
            _gameTimeText.text = Lang.Get("游戏未开始");
        }
    }
}

public class Act2080ShopItem : ListItem
{
    private Image icon;
    private Text name;
    private Text count;
    private Image qua;
    private Text costText;
    private Button getBtn;
    private Image btnImg;
    private int _id;
    ActInfo_2080 actInfo;

    public override void OnCreate()
    {
        icon = transform.Find<Image>("Icon");
        name = transform.Find<Text>("NameText");
        count = transform.Find<Text>("NubText");
        qua = transform.Find<Image>("Qua");
        costText = transform.Find<Text>("ButtonGet/Text");
        getBtn = transform.Find<Button>("ButtonGet");
        btnImg = getBtn.GetComponent<Image>();
        getBtn.onClick.SetListener(On_getBtnClick);
    }
    private void On_getBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();

        actInfo = (ActInfo_2080)ActivityManager.Instance.GetActivityInfo(2080);
        if (actInfo == null)
            return;

        if (Cfg.Activity2080.GetShopData(_id).cost <= actInfo.Score)
        {
            actInfo.RequestBuy(_id);
        }
    }

    public void Refresh(P_Act2080Exchange data)
    {
        _id = data.id;
        actInfo = (ActInfo_2080)ActivityManager.Instance.GetActivityInfo(2080);

        cfg_act_2080_shop cfg2080 = Cfg.Activity2080.GetShopData(_id);
        P_Item item = Cfg.Activity2080.GetShopItem(_id);
        ItemForShow itemForShow = new ItemForShow(item.id, item.count);
        itemForShow.SetIcon(icon);

        if (cfg2080.max_time == 0)
        {
            name.text = itemForShow.GetName();
        }
        else
        {
            name.text = Lang.Get("{0} ({1}/{2})", itemForShow.GetName(), data.num, cfg2080.max_time);

            if (data.num < cfg2080.max_time)
            {
                getBtn.gameObject.SetActive(true);
            }
            else
            {
                getBtn.gameObject.SetActive(false);
            }
        }

        count.text = "x" + GLobal.NumFormat(itemForShow.GetCount());
        qua.color = _ColorConfig.GetQuaColorHSV(itemForShow.GetQua());
        costText.text = Lang.Get("{0}福气值", cfg2080.cost);

        if (cfg2080.cost <= actInfo.Score)
        {
            btnImg.color = _ColorConfig.ButtonGolden;
            getBtn.enabled = true;
        }
        else
        {
            btnImg.color = _ColorConfig.ButtonGray;
            getBtn.enabled = false;
        }

        icon.GetComponent<Button>().onClick.SetListener(() =>
        {
            AudioManager.Instace.PlaySoundOfNormalBtn();
            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(item.id, item.count, icon.transform.position); });
        });
    }
}



