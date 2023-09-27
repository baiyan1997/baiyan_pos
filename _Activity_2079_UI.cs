using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class _Activity_2079_UI : ActivityUI
{
    //探索分页
    private GameObject _tabGo2;
    private Text _countText1;
    private Text _countText2;
    private Slider _slider;
    private List<Transform> _mapTransList;
    private Transform _pieceTrans;
    private Button _achieveBtn;
    private Button _exploreBtn1;
    private Button _exploreBtn2;
    private Button _backBtn;
    private Text _priceText1;
    private Text _priceText2;
    private Text _proText;
    private GameObject _tipGo;
    private Animator _anim;
    private GameObject _effectGo;
    private Button _item1Btn;
    private Button _item2Btn;
    private GameObject _maskGo;

    //定向选择子界面
    private GameObject _selectGo;
    private Button[] _numberBtn;
    private Button _cancelBtn;
    private Button _okBtn;

    //任务分页
    private GameObject _tabGo1;
    private ListView _listView;
    private Text _timeText;
    private Button _exploreBtn;
    private Text _descText;
    private Button _helpBtn;
    private Text _tipText;

    private int _steps;
    private ActInfo_2079 _actInfo;
    private const int _aid = 2079;
    private const int _pieceCount = 16;
    private Coroutine _coroutine;
    private bool _isMoving;
    private int _curPos;
    private int _tab;
    private float _height = 144f;
    private float _width = 134f;
    private Vector2 fromPos = new Vector2(-268, -251);
    private Color _colorPos = new Color(0.5f, 1, 0, 1);
    private Color _colorStep = new Color(1, 0.22f, 1, 1);

    public override void OnCreate()
    {
        InitRef();
        InitEvent();
        //InitListener();
    }

    private void InitRef()
    {
        _tabGo1 = transform.Find<GameObject>("Main_01");
        _tabGo2 = transform.Find<GameObject>("Main_02");
        _timeText = transform.Find<JDText>("Main_01/Text_time");
        _exploreBtn = transform.Find<Button>("Main_01/BtnExplore");
        _descText = transform.Find<JDText>("Main_01/TextDesc");
        _listView = ListView.Create<Act2079TaskItem>(transform.Find("Main_01/Scroll View"));
        _achieveBtn = transform.Find<Button>("Main_02/Achieve");
        _countText1 = transform.Find<JDText>("Main_02/Icon01/TextCount");
        _countText2 = transform.Find<JDText>("Main_02/Icon02/TextCount");
        _slider = transform.Find<Slider>("Main_02/Slider");
        _exploreBtn1 = transform.Find<Button>("Main_02/Bottom/ButtonExplore1");
        _exploreBtn2 = transform.Find<Button>("Main_02/Bottom/ButtonExplore2");
        _backBtn = transform.Find<Button>("Main_02/ButtonBack");
        _mapTransList = new List<Transform>();
        for (int i = 0; i < 16; i++)
        {
            string name = "Main_02/Cards/" + i;
            _mapTransList.Add(transform.Find(name));
        }
        _selectGo = transform.Find<GameObject>("Main_02/Select");
        _numberBtn = new Button[]
        {
             transform.Find<Button>("Main_02/Select/Icon/M/1"),
             transform.Find<Button>("Main_02/Select/Icon/M/2"),
             transform.Find<Button>("Main_02/Select/Icon/M/3"),
             transform.Find<Button>("Main_02/Select/Icon/M/4"),
             transform.Find<Button>("Main_02/Select/Icon/M/5"),
             transform.Find<Button>("Main_02/Select/Icon/M/6"),
        };
        _cancelBtn = transform.Find<Button>("Main_02/Select/CloseBtn");
        _okBtn = transform.Find<Button>("Main_02/Select/ButtonOK");
        _priceText1 = transform.Find<JDText>("Main_02/Bottom/ButtonExplore1/TextPrice");
        _priceText2 = transform.Find<JDText>("Main_02/Bottom/ButtonExplore2/TextPrice");
        _proText = transform.Find<JDText>("Main_02/Slider/Text");
        _tipGo = transform.Find<GameObject>("Main_02/Achieve/Tip");
        _pieceTrans = transform.Find("Main_02/ChessPieces");
        _helpBtn = transform.Find<Button>("Main_01/HelpBtn");
        _anim = transform.Find<Animator>("Main_02/Effect/RollTheDice/RollTheDice");
        _effectGo = transform.Find<GameObject>("Main_02/Effect");
        _item1Btn = transform.Find<Button>("Main_02/Icon01");
        _item2Btn = transform.Find<Button>("Main_02/Icon02");
        _tipText = transform.Find<JDText>("Main_01/TextTip");
        _maskGo = transform.Find<GameObject>("Main_02/Mask");
    }

    private void InitEvent()
    {
        _item1Btn.onClick.AddListener(On_item1BtnClick);
        _item2Btn.onClick.AddListener(On_item2BtnClick);
        _helpBtn.onClick.AddListener(On_helpBtnClick);
        //普通探索
        _exploreBtn1.onClick.SetListener(OnClickExploreBtn1);
        _exploreBtn2.onClick.SetListener(On_exploreBtn2Click);
        //定向探索
        for (int i = 0; i < _numberBtn.Length; i++)
        {
            int index = i;
            _numberBtn[i].onClick.SetListener(() =>
            {
                if (_actInfo == null)
                    return;

                AudioManager.Instace.PlaySound(AudioType.AS_Operation, SoundType.ID_2092);
                _steps = index + 1;
                SetButtonState();
            });
        }
        _okBtn.onClick.SetListener(On_okBtnClick);
        _cancelBtn.onClick.SetListener(On_cancelBtnClick);
        _achieveBtn.onClick.AddListener(On_achieveBtnClick);
        _exploreBtn.onClick.AddListener(On_exploreBtnClick);
        _backBtn.onClick.AddListener(On_backBtnClick);
    }
    private void On_item1BtnClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_item1DialogShowAsynCB);
    }
    private void On_item1DialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(ItemId.Act2079Energy, 1, _item1Btn.transform.position);
    }
    private void On_item2BtnClick()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_item2DialogShowAsynCB);
    }
    private void On_item2DialogShowAsynCB(_D_ItemTip d)
    {
        d?.OnShow(ItemId.Act2079Crystal, 1, _item2Btn.transform.position);
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Top_HelpDesc>(On_helpDialogShowAsynCB);
    }
    private void On_helpDialogShowAsynCB(_D_Top_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2079);
    }
    private void On_exploreBtn2Click()
    {
        if (_isMoving)
            return;
        AudioManager.Instace.PlaySoundOfNormalBtn();
        ShowExploreUI();
    }
    private void On_okBtnClick()
    {
        if (_actInfo == null)
            return;
        if (ItemHelper.IsCountEnough(ItemId.Act2079Crystal, _actInfo.ExploreNuclear))
        {
            if (_steps > 0 && _steps < 7)
            {
                _actInfo.RequestDirectExplore(_steps, OnRequestDirectExploreCB);
            }
            else
            {
                MessageManager.Show(Lang.Get("请选择探索前的格数"));
            }
        }
        else
        {
            MessageManager.Show(Lang.Get("{0}不足", Cfg.Item.GetItemName(ItemId.Act2079Crystal)));
        }
    }
    private void OnRequestDirectExploreCB(string reward)
    {
        _selectGo.SetActive(false);
        _coroutine = _Scheduler.Instance.StartCoroutine(DoMovePiece(_steps, reward));
    }

    private void On_cancelBtnClick()
    {
        _selectGo.SetActive(false);
    }
    private void On_achieveBtnClick()
    {
        if (_isMoving)
            return;
        DialogManager.ShowAsyn<_D_Act2079Achieve>(On_achieveDialogShowAsynCB);
    }
    private void On_achieveDialogShowAsynCB(_D_Act2079Achieve d)
    {
        d?.OnShow();
    }

    private void On_exploreBtnClick()
    {
        _OnShow(2);
    }
    private void On_backBtnClick()
    {
        if (_isMoving)
            return;
        _OnShow(1);
    }


    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(UpdatePlayerItem);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(UpdatePlayerItem);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_mapTransList != null)
        {
            _mapTransList.Clear();
            _mapTransList = null;
        }
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _aid)
            return;

        if (gameObject.activeSelf && !_isMoving)
            _OnShow(_tab);
    }

    public override void OnShow()
    {
        //打开默认tab1界面
        _tab = 1;
        _selectGo.SetActive(false);
        _maskGo.SetActive(false);
        _anim.SetInteger("step", 1);
        _anim.SetBool("reset", false);
        _effectGo.SetActive(false);
        _exploreBtn1.transform.parent.gameObject.SetActive(true);
        _isMoving = false;
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }

    private void UpdatePlayerItem()
    {
        if (gameObject.activeSelf)
        {
            _actInfo = (ActInfo_2079)ActivityManager.Instance.GetActivityInfo(_aid);
            if (_actInfo == null)
                return;

            UpdatePregress();
            UpdateCount();
        }
    }

    private void _OnShow(int tab)
    {
        _actInfo = (ActInfo_2079)ActivityManager.Instance.GetActivityInfo(_aid);

        if (_actInfo == null)
            return;

        _tab = tab;
        if (tab == 1)
        {
            _tabGo1.SetActive(true);
            _tabGo2.SetActive(false);
            UpdateTab1();
        }
        else
        {
            _tabGo2.SetActive(true);
            _tabGo1.SetActive(false);
            UpdateTab2();
        }
    }

    private void UpdateTab1()
    {
        _descText.text = _actInfo._desc;

        _listView.Clear();

        for (int i = 0; i < _actInfo.TaskList.Count; i++)
        {
            _listView.AddItem<Act2079TaskItem>().Refresh(_actInfo.TaskList[i]);
        }

        _tipText.gameObject.SetActive(_actInfo.TaskList.Count < 1);
    }

    private void UpdateTab2()
    {
        _actInfo = (ActInfo_2079)ActivityManager.Instance.GetActivityInfo(_aid);

        _priceText1.text = _actInfo.ExploreEnergy.ToString();
        _priceText2.text = _actInfo.ExploreNuclear.ToString();

        _item1Btn.transform.Find<Image>("Qua").color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(ItemId.Act2079Energy));
        _item2Btn.transform.Find<Image>("Qua").color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(ItemId.Act2079Crystal));

        UpdateMap();

        UpdatePregress();

        UpdateCount();
    }

    private void UpdateCount()
    {
        _countText1.text = _actInfo.EnergyCount.ToString();
        _countText2.text = _actInfo.CrystalCount.ToString();
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
            _timeText.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _timeText.text = Lang.Get("活动已经结束");
        }
    }

    private void UpdatePregress()
    {
        int tid = _actInfo.GetSliderTid();
        if (tid < 0)
        {
            _slider.gameObject.SetActive(false);
        }
        else
        {
            int sum = Cfg.Activity2079.GetTaskData(tid).need_count;
            long have = _actInfo.ExploreCount;
            _slider.gameObject.SetActive(true);
            if (have < sum)
                _proText.text = Lang.Get("收集{0}/{1}个{2}", have, sum, Cfg.Item.GetItemName(ItemId.Act2079Medal));
            else
                _proText.text = Lang.Get("收集<Color=#33ff66>{0}</Color>/{1}个{2}", have, sum, Cfg.Item.GetItemName(ItemId.Act2079Medal));
            float oldValue = _slider.value;
            float newValue = (float)_actInfo.ExploreCount / sum;
            if (newValue > oldValue)
            {
                DOTween.To(() => _slider.value, x => _slider.value = x, newValue, 0.2f).SetEase(Ease.OutQuad);
            }
            else
            {
                _slider.value = newValue;
            }
        }
        //tip
        _tipGo.SetActive(_actInfo.IsAchieveAvailable());
    }

    private void UpdateMap()
    {
        if (!gameObject.activeSelf)
            return;

        _curPos = _actInfo.PosId;

        _pieceTrans.localPosition = GetPosByIndex(_actInfo.PosId);

        List<P_Act2079BoardInfo> itemList = _actInfo.CellList;

        for (int i = 0; i < _mapTransList.Count; i++)
        {
            //初始化每个格子
            UpdateCell(_mapTransList[i].transform, itemList[i]);

            if (_actInfo.PassPos.Contains(i))
                _mapTransList[i].GetComponent<Image>().color = _colorPos;
            else
                _mapTransList[i].GetComponent<Image>().color = _ColorConfig.ButtonRed;
        }
    }

    private void UpdateCell(Transform trans, P_Act2079BoardInfo info)
    {
        Text countText = trans.Find<Text>("Text_num");
        Text lvText = trans.Find<Text>("Lv/TextLv");
        Image icon = trans.Find<Image>("Img_icon");
        Image qua = trans.Find<Image>("qua");

        P_Item item = new P_Item(info.reward);

        lvText.text = Lang.Get("Lv.{0}", info.lv);

        ItemForShow itemForShow = new ItemForShow(item.id, item.count);
        itemForShow.SetIcon(icon);
        qua.color = _ColorConfig.GetQuaColorHSV(itemForShow.GetQua());
        countText.text = "x" + GLobal.NumFormat(itemForShow.GetCount());

        icon.GetComponent<Button>().onClick.SetListener(() =>
        {
            AudioManager.Instace.PlaySoundOfNormalBtn();
            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(item.id, item.count, icon.transform.position); });
        });
    }

    private void MovePiece(int pos)
    {
        Vector2 endPos = GetPosByIndex(pos);
        _pieceTrans.DOLocalMove(endPos, 0.2f);
    }

    private IEnumerator DoMovePiece(int step, string rewards)
    {
        _isMoving = true;
        _maskGo.SetActive(true);
        _exploreBtn1.transform.parent.gameObject.SetActive(false);
        //扔骰子
        _effectGo.SetActive(true);

        _anim.SetInteger("step", step);

        yield return new WaitForSeconds(2.0f);

        for (int i = 1; i <= step; i++)
        {
            int index = (_curPos + i) % _pieceCount;
            MovePiece(index);
            yield return new WaitForSeconds(0.2f);
        }

        _curPos = (_curPos + step) % _pieceCount;
        //弹出奖励
        MessageManager.ShowRewards(rewards);

        yield return new WaitForSeconds(1.0f);
        _isMoving = false;
        _maskGo.SetActive(false);
        _exploreBtn1.transform.parent.gameObject.SetActive(true);
        _effectGo.SetActive(false);
        //刷新活动数据
        ActivityManager.Instance.RequestUpdateActivityById(_aid);
    }


    private void OnClickExploreBtn1()
    {
        if (_actInfo == null)
            return;

        if (_isMoving)
            return;

        AudioManager.Instace.PlaySoundOfNormalBtn();

        if (ItemHelper.IsCountEnough(ItemId.Act2079Energy, _actInfo.ExploreEnergy))
        {
            _actInfo.RequestNormalExplore(OnRequestNormalExploreCB);
        }
        else
        {
            MessageManager.Show(Lang.Get("{0}不足", Cfg.Item.GetItemName(ItemId.Act2079Energy)));
        }
    }
    private void OnRequestNormalExploreCB(string reward,int step)
    {
        _coroutine = _Scheduler.Instance.StartCoroutine(DoMovePiece(step, reward));
    }

    //定向探索
    private void ShowExploreUI()
    {
        _steps = -1;
        SetButtonState();
        _selectGo.SetActive(true);
    }

    private void SetButtonState()
    {
        for (int i = 0; i < _numberBtn.Length; i++)
        {
            if (_steps - 1 == i)
                _numberBtn[i].GetComponent<Image>().color = _colorStep;
            else
                _numberBtn[i].GetComponent<Image>().color = _ColorConfig.ButtonRed;
        }
    }

    public Vector3 GetPosByIndex(int pos)
    {
        int deltaX = 0;
        int deltaY = 0;

        if (pos < 4)
        {
            deltaX = 0;
            deltaY = pos;
        }
        else if (pos < 8)
        {
            deltaX = pos - 4;
            deltaY = 4;
        }
        else if (pos < 13)
        {
            deltaX = 4;
            deltaY = 12 - pos;
        }
        else
        {
            deltaX = 16 - pos;
            deltaY = 0;
        }

        return fromPos + new Vector2(_width * deltaX, _height * deltaY);
    }


    public override void OnClose()
    {
        base.OnClose();

        if (_coroutine != null)
            _Scheduler.Instance.StopCoroutine(_coroutine);
        _isMoving = false;
        _anim.SetInteger("step", 1);
        _anim.SetBool("reset", false);
        _effectGo.SetActive(false);
        _maskGo.SetActive(false);
        _tabGo1.SetActive(true);
        _tabGo2.SetActive(false);
    }
}

public class Act2079TaskItem : ListItem
{
    private Text _tittle;
    private Text _progress;
    private Button _getBtn;
    private GameObject[] _rewardGoList;
    private GameObject _unreachGo;
    private GameObject _getGo;

    private const int _aid = 2079;
    private int _tid;

    public override void OnCreate()
    {
        _tittle = transform.Find<Text>("Text_Desc");
        _progress = transform.Find<Text>("Text_Count");
        _getBtn = transform.Find<Button>("Btn_Get");
        _getGo = transform.Find<GameObject>("GetGo");
        _unreachGo = transform.Find<GameObject>("UnReachGo");
        _rewardGoList = new GameObject[]
        {
            transform.Find<GameObject>("Reward1"),
            transform.Find<GameObject>("Reward2"),
            transform.Find<GameObject>("Reward3"),
        };
        _getBtn.onClick.SetListener(On_getBtnClick);
    }
    private void On_getBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();
        ActInfo_2079 actinfo = (ActInfo_2079)ActivityManager.Instance.GetActivityInfo(_aid);
        if (actinfo == null)
            return;
        actinfo.GetAct2079Reward(_tid);
    }
    public void Refresh(P_Act2079Reward info)
    {
        _tid = info.tid;
        cfg_act_2079_task task = Cfg.Activity2079.GetTaskData(_tid);
        P_Item[] rewards = Cfg.Activity2079.GetTaskRewards(_tid);

        _tittle.text = task.name;
        _progress.text = Lang.Get("{0}/{1}", info.do_number, task.need_count);

        if (info.get_reward == 1)
        {
            _getBtn.gameObject.SetActive(false);
            _unreachGo.SetActive(false);
            _getGo.SetActive(true);
        }
        else if (info.finished == 1)
        {
            _getBtn.gameObject.SetActive(true);
            _unreachGo.SetActive(false);
            _getGo.SetActive(false);
        }
        else
        {
            _unreachGo.SetActive(true);
            _getGo.SetActive(false);
            _getBtn.gameObject.SetActive(false);
        }

        for (int i = 0; i < _rewardGoList.Length; i++)
        {
            GameObject go = _rewardGoList[i];

            if (i < rewards.Length)
            {
                go.SetActive(true);
                DefineReward(go.transform, rewards[i]);
            }
            else
            {
                go.SetActive(false);
            }
        }
    }

    private void DefineReward(Transform trans, P_Item item)
    {
        ItemForShow itemForShow = new ItemForShow(item.id, item.count);
        Image icon = trans.Find<Image>("Image");
        itemForShow.SetIcon(icon);
        trans.Find<Text>("Text").text = "x" + GLobal.NumFormat(itemForShow.GetCount());
        trans.Find<Image>("Qua").color = _ColorConfig.GetQuaColor(itemForShow.GetQua());

        icon.GetComponent<Button>().onClick.SetListener(() =>
        {
            AudioManager.Instace.PlaySoundOfNormalBtn();
            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(item.id, item.count, icon.transform.position); });
        });
    }
}
