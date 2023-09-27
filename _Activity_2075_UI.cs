using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class _Activity_2075_UI : ActivityUI
{
    private Text _txtTime;
    private Text _txtFreeCount;
    private Text _txtOtherCount;
    private Button _btnStartGame;
    private Button _btnGo;
    private Button _btnTip;

    private ListView _listView;
    private RectTransform _contentTrans;
    private RectTransform _view;
    private GameObject _node;

    private Dictionary<int, Act2077_Row> _dicRow;
    private Dictionary<int, Act2077_Node> _dicNode;

    private bool _init;
    private const float _detalH = 115f;


    private ActInfo_2075 _actInfo;

    public override void OnCreate()
    {
        _actInfo = (ActInfo_2075)ActivityManager.Instance.GetActivityInfo(2075);
        _txtTime = transform.Find<JDText>("TimeText");
        _txtFreeCount = transform.Find<JDText>("TextFree");
        _txtOtherCount = transform.Find<JDText>("TextBattle");
        _btnStartGame = transform.Find<Button>("StartGame");
        _btnGo = transform.Find<Button>("ButtonGo");
        _btnTip = transform.Find<Button>("ButtonTip");

        _listView = ListView.Create<Act2077_Row>(transform.Find("Scroll View"));
        _contentTrans = transform.Find<RectTransform>("Scroll View/Viewport/Content");
        _view = transform.Find<RectTransform>("Scroll View/Viewport");
        _node = transform.Find("Node").gameObject;

        _dicRow = new Dictionary<int, Act2077_Row>();
        _dicNode = new Dictionary<int, Act2077_Node>();
        _btnStartGame.onClick.AddListener(On_btnStartGameClick);
        _btnGo.onClick.AddListener(On_btnGoClick);
        _btnTip.onClick.AddListener(On_btnTipClick);
    }
    private void On_btnStartGameClick()
    {
        if (ActInfo_2075.Inst.GetGameCount() < 0)
        {
            MessageManager.Show(Lang.Get("今日机会已用完"));
            return;
        }
        ActInfo_2075.Inst.StartGame(On_btnStartGameCB);
    }
    private void On_btnStartGameCB()
    {
        MessageManager.Show("开始战斗");
        DialogManager.ShowAsyn<_Activity_2075_Fighting>(On_btnStartGameDialogShowAsynCB);
    }
    private void On_btnStartGameDialogShowAsynCB(_Activity_2075_Fighting d)
    {
        d?.OnShow();
    }
    private void On_btnGoClick()
    {
        DialogManager.CloseAllDialog();
        _GameSceneManager.Instance.SwitchScene(GameSceneName.World);
    }
    private void On_btnTipClick()
    {
        DialogManager.ShowAsyn<_D_Top_HelpDesc>(On_btnTipDialogShowAsynCB);
    }
    private void On_btnTipDialogShowAsynCB(_D_Top_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2075);
    }


    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdateActById.AddListener(UpdateActById);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdateActById.RemoveListener(UpdateActById);
    }

    public override void OnClose()
    {
        base.OnClose();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_dicRow != null)
        {
            _dicRow.Clear();
            _dicRow = null;
        }
        if (_dicNode != null)
        {
            _dicNode.Clear();
            _dicNode = null;
        }
        _actInfo = null;
    }

    public override void OnShow()
    {
        if (!_init)
        {
            _listView.Clear();
            var rowCount = Cfg.Activity2075.Row;
            for (int i = rowCount - 1; i >= 0; i--)
            {
                _dicRow[i + 1] = _listView.AddItem<Act2077_Row>();
            }
            RefreshNode();
            _Scheduler.Instance.PerformInNextFrame(RefreshLine);
            _init = true;
        }
        else
        {
            RefreshNode();
        }
        Move(null);

        _txtFreeCount.text = Lang.Get("今日免费次数：{0}/3", 3 - ActInfo_2075.Inst.FreeCount);
        _txtOtherCount.text = Lang.Get("基地战成功奖励次数：{0}/3", ActInfo_2075.Inst.BattleCount - ActInfo_2075.Inst.UseCount);
    }

    private void UpdateActById(int aid)
    {
        if (aid == _actInfo._aid)
            OnShow();
    }

    private void RefreshNode()
    {
        var nodeData = Cfg.Activity2075.GetAllData();
        for (int i = 0; i < nodeData.Count; i++)
        {
            var node = nodeData[i];
            var data = Cfg.Activity2075.GetData(node.tid);
            int row = Cfg.Activity2075.GetRow(node.tid);
            int pos = Cfg.Activity2075.GetPos(node.tid);
            Act2077_Row temp = null;
            if (_dicRow.TryGetValue(row, out temp))
            {
                temp.Refresh(_node, data);
                Act2077_Node oldNode = null;
                var idx = i + 1;
                if (!_dicNode.TryGetValue(idx, out oldNode))
                {
                    _dicNode.Add(idx, temp.GetNode(pos));
                }
            }
        }
    }

    public void RefreshLine()
    {
        var count = _dicNode.Keys.Count;
        for (int i = 0; i < count; i++)
        {
            var idx1 = i + 1;
            var idx2 = i + 2;
            Act2077_Node node = null;
            Act2077_Node node2 = null;
            if (_dicNode.TryGetValue(idx1, out node) && _dicNode.TryGetValue(idx2, out node2))
            {
                node.WriteLine(node.LineStartPos, node2.LineStartPos);
            }
        }
    }

    private void Move(Action callback)
    {
        int row = Cfg.Activity2075.Row - ActInfo_2075.Inst.CurrentPos - 1;
        if (row <= 4)
        {
            _listView.ScrollRect.DOVerticalNormalizedPos(1, 0.3f).OnComplete(() =>
            {
                if (callback != null)
                {
                    callback();
                }
            });
        }
        else if (row >= Cfg.Activity2075.Row - 3)
        {
            _listView.ScrollRect.DOVerticalNormalizedPos(0, 0.3f).OnComplete(() =>
            {
                if (callback != null)
                {
                    callback();
                }
            });
        }
        else
        {
            float y = row * _detalH;
            if (y >= GetEnd())
            {
                y = GetEnd();
            }
            _contentTrans.DOLocalMoveY(y, 0.3f).OnComplete(() =>
            {
                if (callback != null)
                {
                    callback();
                }
            });
        }
    }

    private float GetEnd()
    {
        int count = _listView._listItems.Count;
        float pivotY = _contentTrans.pivot.y;
        int _span = 20;
        float origin = count * (_detalH + _span) * (pivotY - 1);
        var end = origin + count * (_detalH + _span) - _view.rect.height;// - 336f;
        if (end < 0)
        {
            end = 0;
        }
        return end;
    }

    public override void UpdateTime(long st)
    {
        base.UpdateTime(st);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (st - _actInfo._data.startts < 0)
        {
            _txtTime.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
            _txtTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
                span.Minutes, span.Seconds);
        }
        else
        {
            _txtTime.text = Lang.Get("活动已经结束");
        }
    }
}

public class Act2077_Row : ListItem
{
    public Dictionary<int, Act2077_Node> NodeCache; //记录这一行pos信息 

    public override void OnCreate()
    {
        NodeCache = new Dictionary<int, Act2077_Node>(4);
    }

    public void Refresh(GameObject go, cfg_act_2075 info)
    {
        int pos = Cfg.Activity2075.GetPos(info.tid);
        var item = GetNode(pos);
        if (item != null)
        {
            item.Refresh(info);
        }
        else
        {
            Act2077_Node node = new Act2077_Node();
            var t = Object.Instantiate(go, transform);
            NodeCache.Add(pos, node);
            node.OnCreate(t.transform);
            node.Refresh(info);
        }
    }

    public Act2077_Node GetNode(int pos)
    {
        Act2077_Node node = null;
        NodeCache.TryGetValue(pos, out node);
        return node;
    }
}

public class Act2077_Node
{
    private Transform _transform;
    private GameObject _objNormal;
    private GameObject _objNow;
    private Text _txtName;
    private GameObject _objIcon1;
    private GameObject _objIcon2;
    private Image _imgReward1;
    private Image _imgReward2;
    private GameObject _objBg1;
    private GameObject _objBg2;
    private Button _btnIcon1;
    private Button _btnIcon2;
    private Text _txtCount1;
    private Text _txtCount2;
    private cfg_act_2075 _info;
    private Act2075UILine _line;
    public Vector3 LineStartPos;

    private const float PosX1 = -128f;
    private const float PosX2 = 0;
    private const float PosX3 = 128f;
    private const float PosY = -62f;

    public void OnCreate(Transform t)
    {
        _transform = t;
        _objNormal = _transform.Find("Image01").gameObject;
        _objNow = _transform.Find("Image02").gameObject;
        _objIcon1 = _transform.Find("Image01/Icon01").gameObject;
        _objIcon2 = _transform.Find("Image02/Icon02").gameObject;
        _imgReward1 = _transform.Find<Image>("Image01/Icon01/Image");
        _imgReward2 = _transform.Find<Image>("Image02/Icon02/Image");
        _btnIcon1 = _transform.Find<Button>("Image01/Icon01/Image");
        _btnIcon2 = _transform.Find<Button>("Image02/Icon02/Image");
        _objBg1 = _transform.Find("Image01/Icon01/ImageBg").gameObject;
        _objBg2 = _transform.Find("Image02/Icon02/ImageBg").gameObject;
        _txtCount1 = _transform.Find<JDText>("Image01/Icon01/Image/Text");
        _txtCount2 = _transform.Find<JDText>("Image02/Icon02/Image/Text");
        _txtName = _transform.Find<JDText>("Text");
        _line = new Act2075UILine(_transform.Find("Line"));
        _Scheduler.Instance.PerformInNextFrame(OnNextFrame);
        _btnIcon1.onClick.AddListener(On_btnIcon1Click);
        _btnIcon2.onClick.AddListener(On_btnIcon2Click);
    }
    private void OnNextFrame()
    {
        LineStartPos = _transform.Find("Line").position;
    }
    private void On_btnIcon1Click()
    {
        DialogManager.ShowAsyn<_D_ItemTip>(On_btnIcon1DialogShowAsynCB);
    }
    private void On_btnIcon1DialogShowAsynCB(_D_ItemTip d)
    {
        var itemId = int.Parse(_info.reward.Split('|')[0]);
        d?.OnShow(itemId, 1, _btnIcon1.transform.position);
    }
    private void On_btnIcon2Click()
    {      
        DialogManager.ShowAsyn<_D_ItemTip>(On_btnIcon2DialogShowAsynCB);
    }
    private void On_btnIcon2DialogShowAsynCB(_D_ItemTip d)
    {
        var itemId = int.Parse(_info.reward.Split('|')[0]);
        d?.OnShow(itemId, 1, _btnIcon1.transform.position);
    }

    public void Refresh(cfg_act_2075 info)
    {
        _info = info;
        var now = _info.tid == ActInfo_2075.Inst.CurrentPos;
        if (now)
        {
            _objNormal.SetActive(false);
            _objNow.SetActive(true);
            if (!string.IsNullOrEmpty(_info.reward))
            {
                _objIcon2.SetActive(true);
                var itemId = int.Parse(_info.reward.Split('|')[0]);
                var count = int.Parse(_info.reward.Split('|')[1]);
                Cfg.Item.SetItemIcon(_imgReward2, itemId);
                _txtCount2.text = "x" + count;
                _objBg2.SetActive(ActInfo_2075.Inst.CurrentPos >= info.tid);
            }
            else
            {
                _objIcon2.SetActive(false);
            }
        }
        else
        {
            _objNormal.SetActive(true);
            _objNow.SetActive(false);
            if (!string.IsNullOrEmpty(_info.reward))
            {
                _objIcon1.SetActive(true);
                var itemId = int.Parse(_info.reward.Split('|')[0]);
                var count = int.Parse(_info.reward.Split('|')[1]);
                Cfg.Item.SetItemIcon(_imgReward1, itemId);
                _txtCount1.text = "x" + count;
                _objBg1.SetActive(ActInfo_2075.Inst.CurrentPos >= info.tid);
            }
            else
            {
                _objIcon1.SetActive(false);
            }
        }
        _txtName.text = _info.tid.ToString();
        int pos = Cfg.Activity2075.GetPos(_info.tid);
        _transform.localPosition = new Vector3(GetPos(pos), PosY);
        _transform.gameObject.SetActive(true);
    }

    public void WriteLine(Vector2 posStart, Vector2 posEnd)
    {
        _line.Init(posStart, posEnd);
    }

    private float GetPos(int pos)
    {
        switch (pos)
        {
            case 1:
                return PosX1;
            case 2:
                return PosX2;
            case 3:
                return PosX3;
        }
        return 0;
    }
}

public class Act2075UILine
{
    private Transform _transform;
    //连接线节点
    //private RectTransform _lineNormal;
    private RectTransform _lineDoing;

    private Vector3 _posStart;
    private Vector3 _posEnd;

    private const float PosOffset = 2.4f;
    private const float LenOffset = 76f;
    private bool _init;
    public Act2075UILine(Transform t)
    {
        _transform = t;
        //_lineNormal = _transform.Find<RectTransform>("Line_01");
        _lineDoing = _transform.Find<RectTransform>("Line_02");
    }

    public void Init(Vector3 posStart, Vector3 posEnd)
    {
        if (!_init)
        {
            _posStart = posStart;
            _posEnd = posEnd;

            var vec = (posEnd - posStart).normalized;
            _transform.position += vec * PosOffset;
            _transform.position.Set(_transform.position.x, _transform.position.y, 0);
            _transform.localEulerAngles = Vector3.forward * (Mathf.Atan2(vec.y, vec.x) * 180 / Mathf.PI);
            var len = (_transform.InverseTransformPoint(_posEnd) - _transform.InverseTransformPoint(_posStart)).magnitude - LenOffset;
            //_lineNormal.sizeDelta = new Vector2(len, _lineNormal.sizeDelta.y);
            _lineDoing.sizeDelta = new Vector2(len, _lineDoing.sizeDelta.y);
            _init = true;
        }
    }
}