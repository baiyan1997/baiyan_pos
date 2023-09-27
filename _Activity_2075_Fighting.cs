
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class _Activity_2075_Fighting : Dialog
{
    //背景
    private Transform _tranBg1;
    private Transform _tranBg2;
    private float _speedBg = 8f;

    /// <summary>
    /// 0-未开始阶段
    /// 1-准备阶段倒计时3秒
    /// 2-战斗 0-10秒 234通道 1秒/格 间隔3秒
    /// 3-战斗 11-40秒 12345通道 2秒/格 间隔2秒
    /// 4-战斗 41-60秒 12345通道 3秒/格 间隔1秒
    /// 5-结束游戏
    /// </summary>
    public int Stage;
    //游戏设置参数
    private const float TotalTime = 36f;
    private float _gameTime; //游戏进行时长
    private float _triggerTime;
    private float _triggerTimeMeteorite; //触发生成陨石时间
    private bool _startGame;
    private float _baseSpeed = 4f;
    //private float _collisionDis = 160f;
    private int _hp;
    private float _shipMove = 20f;
    private bool _isPressLeft;
    private bool _isPressRight;

    private float _digH;

    private int _lastTrack; //上次随机出的跑道
    //private int _meteCount = 0;
    //private const int TotalCount = 1;//测试最大产生陨石数量

    private const int StageTime1 = 5;
    private const int StageTime2 = 20;
    private const int StageTime3 = 36;

    private int _bloodCount;//刷新血包的次数
    private int _lastBloodTime;//上一次刷新血包的时间

    private ObjectGroup UI;
    private Text _txtTask;
    private Text _txtTotalTime;
    private GameObject _objCountDown;
    private Text _txtCountDown;
    private Transform _transImage; //倒计时动画

    private Button _btnLeft;
    private Button _btnRight;
    private Transform _meteRoot;
    private Image[] _imgHps;
    private Sprite _spFull;
    private Sprite _spNull;

    private GameObject _objHp;
    private GameObject[] _objMetePrefabs;

    private Act2075_PlayerPlant _player;
    private List<Act2075_Meteorite> _listMeteorite;
    private List<Act2075_HP> _listHp;

    public static _Activity_2075_Fighting Inst;
    public override DialogDestroyPattern DestroyPattern { get { return DialogDestroyPattern.Delay; } }
    protected override void InitRef()
    {
        Inst = this;

        _tranBg1 = transform.Find("Bg");
        _tranBg2 = transform.Find("Bg2");

        UI = gameObject.GetComponent<ObjectGroup>();
        _txtTask = transform.Find<JDText>("Title/Text");
        _txtTotalTime = transform.Find<JDText>("Title/Time");
        _objCountDown = transform.Find("ImageTime").gameObject;
        _txtCountDown = transform.Find<JDText>("ImageTime/Time");
        _transImage = transform.Find("ImageTime/Image");

        _btnLeft = transform.Find<Button>("ButtonLeft");
        _btnRight = transform.Find<Button>("ButtonRight");
        _meteRoot = transform.Find("MeteoriteRoot");
        _player = new Act2075_PlayerPlant(transform.Find("PlayerPlant"));

        _imgHps = new[]
        {
            transform.Find<Image>("Title/Star/Image01"),
            transform.Find<Image>("Title/Star/Image02"),
            transform.Find<Image>("Title/Star/Image03"),
        };
        _spFull = UI.Ref<Sprite>("Star1");
        _spNull = UI.Ref<Sprite>("Star2");


        _objHp = transform.Find("Hp").gameObject;
        _objMetePrefabs = new[]
        {
            transform.Find("Image1").gameObject,
            transform.Find("Image2").gameObject,
            transform.Find("Image3").gameObject,
            transform.Find("Image4").gameObject,
            transform.Find("Image5").gameObject
        };
    }

    public override bool IsFullScreen()
    {
        return true;
    }

    protected override void OnCreate()
    {
        var rect = transform.GetComponent<RectTransform>().rect;
        _digH = rect.height;
        _tranBg1.GetComponent<RectTransform>().sizeDelta = new Vector2(rect.width, rect.height);
        _tranBg2.GetComponent<RectTransform>().sizeDelta = new Vector2(rect.width, rect.height);
        Stage = 0;

        EventTrigger trigger1 = transform.Find<EventTrigger>("ButtonLeft");
        if (trigger1 == null)
        {
            trigger1 = gameObject.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry1 = new EventTrigger.Entry();
        entry1.eventID = EventTriggerType.PointerDown;
        entry1.callback.AddListener(PressLeftBtn);
        trigger1.triggers.Add(entry1);

        EventTrigger.Entry up1 = new EventTrigger.Entry();
        up1.eventID = EventTriggerType.PointerUp;
        up1.callback.AddListener(UpLeftBtn);
        trigger1.triggers.Add(up1);

        EventTrigger trigger2 = transform.Find<EventTrigger>("ButtonRight");
        if (trigger2 == null)
        {
            trigger2 = gameObject.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerDown;
        entry2.callback.AddListener(PressRightBtn);
        trigger2.triggers.Add(entry2);

        EventTrigger.Entry up2 = new EventTrigger.Entry();
        up2.eventID = EventTriggerType.PointerUp;
        up2.callback.AddListener(UpRightBtn);
        trigger2.triggers.Add(up2);

        AddTimePassSecondEvent(UpdateTime);
    }

    private void PressLeftBtn(BaseEventData data)
    {
        _isPressLeft = true;
    }
    private void PressRightBtn(BaseEventData data)
    {
        _isPressRight = true;
    }
    private void UpLeftBtn(BaseEventData data)
    {
        _isPressLeft = false;
        _isPressRight = false;
    }
    private void UpRightBtn(BaseEventData data)
    {
        _isPressLeft = false;
        _isPressRight = false;
    }

    public void OnShow()
    {
        PrepareGame();
        DoCountDown();
    }

    private void UpdateTime(long ts)
    {
        if (Stage >= 2 && Stage <= 4)
        {
            SetLeftTime();
            CheckHpTime();
        }
    }

    private void PrepareGame()
    {
        ResetBg();
        _startGame = false;
        _isPressRight = false;
        _isPressLeft = false;
        _txtTask.text = Lang.Get("银河探险 第{0}关", ActInfo_2075.Inst.CurrentPos + 1);
        _gameTime = 0;
        _bloodCount = 0;
        _lastBloodTime = 0;
        _hp = 3;
        SetHp();
        _btnLeft.interactable = false;
        _btnRight.interactable = false;
        _player.InitPos();
        _listMeteorite = ListPool<Act2075_Meteorite>.Get();
        _listHp = ListPool<Act2075_HP>.Get();
        SetLeftTime();
        _objCountDown.SetActive(false);
    }

    private void ResetBg()
    {
        _isDown = true;
        _tranBg1.localPosition = new Vector3(0, 0, 0);
        _tranBg2.localPosition = new Vector3(0, _digH, 0);
    }

    private bool _isDown;
    private void BgMove()
    {
        if (_isDown)
        {
            _tranBg1.Translate(Vector3.down * Time.deltaTime * _speedBg * GetSpeedPara());
            _tranBg2.transform.localPosition =
                new Vector3(_tranBg2.transform.localPosition.x, _tranBg1.localPosition.y + _digH, 0);
        }
        else
        {
            _tranBg2.Translate(Vector3.down * Time.deltaTime * _speedBg * GetSpeedPara());
            _tranBg1.transform.localPosition =
                new Vector3(_tranBg1.transform.localPosition.x, _tranBg2.localPosition.y + _digH, 0);
        }


        if (_tranBg1.transform.localPosition.y <= -_digH)
        {
            _tranBg1.transform.localPosition = new Vector3(0, _digH, 0);
            _isDown = false;
        }

        if (_tranBg2.transform.localPosition.y <= -_digH)
        {
            _tranBg2.transform.localPosition = new Vector3(0, _digH, 0);
            _isDown = true;
        }
    }

    private void SetHp()
    {
        if (_hp < 0)
            _hp = 0;
        if (_hp > 3)
            _hp = 3;

        for (int i = 0; i < _imgHps.Length; i++)
        {
            _imgHps[i].sprite = i < _hp ? _spFull : _spNull;
        }

        if (_hp <= 0)
        {
            Stage = 5;
            QuitGame();
        }
    }

    private void DoCountDown()
    {
        Stage = 1;
        _objCountDown.SetActive(true);
        _transImage.localRotation = new Quaternion(0, 0, 0, 0);
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(1f);
        _txtCountDown.text = "3";
        sequence.Append(_transImage.DOLocalRotate(new Vector3(0, 0, -90f), 0.1f).OnComplete(() =>
        {
            _txtCountDown.text = "2";
        })).AppendInterval(0.9f).Append(_transImage.DOLocalRotate(new Vector3(0, 0, -180f), 0.1f).OnComplete(() =>
        {
            _txtCountDown.text = "1";
        })).AppendInterval(0.9f).Append(_transImage.DOLocalRotate(new Vector3(0, 0, -270f), 0.1f).OnComplete(() =>
        {
            _txtCountDown.text = "0";
        })).AppendInterval(1f).OnComplete(() =>
        {
            _objCountDown.SetActive(false);
            _btnLeft.interactable = true;
            _btnRight.interactable = true;
            _startGame = true;
            Stage = 2;
            _lastTrack = -99;
            _triggerTime = 1;//第一秒开始产生陨石
        });
    }

    private void CloneMeteorite()
    {
        _triggerTime = 0;
        int num = Random.Range(0, 5);
        var go = Object.Instantiate(_objMetePrefabs[num], _meteRoot);

        int track = 0;
        var stage = Stage == 2 ? 1 : 2;
        do
        {
            track = Random.Range(-stage, stage + 1);

        } while (track == _lastTrack);

        _lastTrack = track;
        var meteorite = new Act2075_Meteorite(go, track);
        _listMeteorite.Add(meteorite);
    }

    private void CloneHp()
    {
        var go = Object.Instantiate(_objHp, _meteRoot);
        var hp = new Act2075_HP(go);
        _listHp.Add(hp);
    }

    private void MeteoriteDown()
    {
        if (_listMeteorite == null || _listMeteorite.Count <= 0)
            return;
        var playerLines = _player.GetLines();//列表池获得的列表
        for (int i = _listMeteorite.Count - 1; i >= 0; i--)
        {
            var mete = _listMeteorite[i];
            var locPos = mete.transform.localPosition;
            var speedPara = GetSpeedPara();
            mete.transform.localPosition = new Vector3(locPos.x, locPos.y - _baseSpeed * speedPara, 0);
            mete.transform.Rotate(new Vector3(0, 0, mete.RotatePara));

            if (locPos.y <= -900f)
            {
                mete.Destroy();
                _listMeteorite.RemoveAt(i);
                continue;
            }

            if (mete.IsTrigger)
                continue;


            if (CheckCollision(mete, playerLines))
            {
                WarningUI.ShowWarningTip();
                _hp--;
                mete.IsTrigger = true;
                mete.Destroy();
                _listMeteorite.RemoveAt(i);
                SetHp();
                if (!_startGame)
                    break;
            }
        }
        ListPool<Act2075Line>.Release(playerLines);//释放列表池列表
    }

    private bool CheckCollision(Act2075_Meteorite mete, List<Act2075Line> playerLine)
    {
        if (mete.transform.localPosition.y >= -280f || mete.transform.localPosition.y <= -640f || mete.IsTrigger)
            return false;
        var meteLine = mete.GetLines();

        for (var i = 0; i < meteLine.Count; i++)
        {
            for (var j = 0; j < playerLine.Count; j++)
            {
                var result = Intersect(meteLine[i].Point1, meteLine[i].Point2, playerLine[j].Point1, playerLine[j].Point2);
                if (result)
                    return true;
            }
        }
        return false;
    }

    private bool CheckHp(Act2075_HP hp, List<Act2075Line> playerLine)
    {
        if (hp.transform.localPosition.y >= -280f || hp.transform.localPosition.y <= -640f || hp.IsTrigger)
            return false;
        var hpLine = hp.GetLines();

        for (var i = 0; i < hpLine.Count; i++)
        {
            for (var j = 0; j < playerLine.Count; j++)
            {
                var result = Intersect(hpLine[i].Point1, hpLine[i].Point2, playerLine[j].Point1, playerLine[j].Point2);
                if (result)
                    return true;
            }
        }
        return false;
    }

    //叉积
    private float Mult(Vector2 vec1, Vector2 vec2)
    {
        return vec1.x * vec2.y - vec1.y * vec2.x;
    }

    private bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        //边界条件,完全不可能相交情况
        if (Math.Max(a.x, b.x) < Math.Min(c.x, d.x))
        {
            return false;
        }
        if (Math.Max(a.y, b.y) < Math.Min(c.y, d.y))
        {
            return false;
        }
        if (Math.Max(c.x, d.x) < Math.Min(a.x, b.x))
        {
            return false;
        }
        if (Math.Max(c.y, d.y) < Math.Min(a.y, b.y))
        {
            return false;
        }

        //利用叉乘判断是否相交
        var cdCrossAb = Mult(c - a, b - a) * Mult(b - a, d - a);//判断直线ab是否穿过线段cd 值>0为穿过 ==0为平行
        var abCrossCd = Mult(a - d, c - d) * Mult(c - d, b - d);//判断直线cd是否穿过线段ab 值>0为穿过 ==0为平行
        //两线段所在直线都穿过另一线段则为相交 因为是循环判断封闭的线段 不考虑平行的情况
        return cdCrossAb > 0 && abCrossCd > 0;
    }


    private void HpDown()
    {
        if (_listHp == null || _listHp.Count <= 0)
            return;
        var playerLines = _player.GetLines();//列表池获得的列表
        for (int i = _listHp.Count - 1; i >= 0; i--)
        {
            var hp = _listHp[i];
            var locPos = hp.transform.localPosition;
            hp.transform.localPosition = new Vector3(locPos.x, locPos.y - _baseSpeed, 0);
            if (locPos.y <= -900f)
            {
                hp.Destroy();
                _listHp.RemoveAt(i);
                continue;
            }

            if (hp.IsTrigger)
                continue;
            if (CheckHp(hp, playerLines))
            {
                _hp++;
                hp.IsTrigger = true;
                hp.Destroy();
                _listHp.RemoveAt(i);
                SetHp();
                if (!_startGame)
                    break;
            }
        }
        ListPool<Act2075Line>.Release(playerLines);//释放列表池列表
    }

    private void ShipMove()
    {
        if (_isPressLeft)
        {
            if (_player.Trans.localPosition.x <= -280f)
                return;
            _player.Trans.Translate(Vector3.left * Time.deltaTime * _shipMove);
        }
        
        if (_isPressRight)
        {
            if (_player.Trans.localPosition.x > 280f)
                return;
            _player.Trans.Translate(Vector3.right * Time.deltaTime * _shipMove);
        }
    }

    private float GetSpeedPara()
    {
        if (Stage == 2)
            return 2.5f;
        if (Stage == 3)
            return 2.8f;
        if (Stage == 4)
            return 3.2f;
        return 0;
    }

    private void QuitGame()
    {
        _startGame = false;
        _isPressRight = false;
        _isPressLeft = false;
        for (int i = _listMeteorite.Count - 1; i >= 0; i--)
        {
            _listMeteorite[i].Destroy();
            _listMeteorite.RemoveAt(i);
        }
        for (int i = _listHp.Count - 1; i >= 0; i--)
        {
            _listHp[i].Destroy();
            _listHp.RemoveAt(i);
        }
        ListPool<Act2075_Meteorite>.Release(_listMeteorite);
        ListPool<Act2075_HP>.Release(_listHp);
        var isSucceed = _hp > 0 ? 1 : 0;
        var msg = isSucceed == 1 ? Lang.Get("战斗成功") : Lang.Get("战斗失败");
        MessageManager.Show(msg);
        Close();
        ActInfo_2075.Inst.HandleGameResult(isSucceed, Close);
    }

    private void SetLeftTime()
    {
        int leftTime = (int)(TotalTime - _gameTime);
        if (leftTime <= 0)
            leftTime = 0;
        _txtTotalTime.text = $"{leftTime / 60:D2}:{leftTime % 60:D2}";
    }

    private void CheckHpTime()
    {
        int gameTime = (int) _gameTime;
        if (_hp < 3 && gameTime - _lastBloodTime >= 5 && _bloodCount < 3)
        {
            _lastBloodTime = gameTime;
            _bloodCount++;
            CloneHp();
        }
    }

    public override void Update()
    {
        if (Stage == 0 || Stage == 5 || !_startGame)
            return;
        BgMove();
        ShipMove();
        _gameTime += Time.deltaTime;
        _triggerTime += Time.deltaTime;
        Stage = SetStage();
        if (_triggerTime >= _triggerTimeMeteorite)
            CloneMeteorite();
        MeteoriteDown();
        HpDown();
    }

    private int SetStage()
    {
        if (_gameTime <= 0)
        {
            _triggerTimeMeteorite = 99;
            return 0;
        }
        if (_gameTime <= StageTime1)
        {
            _triggerTimeMeteorite = 1.5f;
            return 2;
        }
        if (_gameTime <= StageTime2)
        {
            _triggerTimeMeteorite = 1.5f;
            return 3;
        }
        if (_gameTime <= StageTime3)
        {
            _triggerTimeMeteorite = 0.9f;
            return 4;
        }
        if (_gameTime > TotalTime)
        {
            QuitGame();
            return 5;
        }
        return 0;
    }
}


public class Act2075_PlayerPlant
{
    public int Track;//跑道
    private PolygonCollider2D _pc2d;

    public Transform Trans;
    public Act2075_PlayerPlant(Transform trans)
    {
        Trans = trans;
        _pc2d = Trans.Find<PolygonCollider2D>("");
        InitPos();
    }

    public void InitPos()
    {
        Trans.localPosition = new Vector3(0, -480f, 0);
    }

    public Vector2[] GetPoints()
    {
        return _pc2d.points;
    }

    public List<Act2075Line> GetLines()
    {
        List<Act2075Line> lines = ListPool<Act2075Line>.Get();
        var points = GetPoints();
        var delta_x = Trans.localPosition.x;
        var delta_y = Trans.localPosition.y;
        for (int i = 0; i < points.Length; i++)
        {
            Act2075Line line = new Act2075Line();
            if (i + 1 < points.Length)
            {
                var x1 = points[i].x + delta_x;
                var y1 = points[i].y + delta_y;
                var x2 = points[i + 1].x + delta_x;
                var y2 = points[i + 1].y + delta_y;

                line.Point1 = new Vector2(x1, y1);
                line.Point2 = new Vector2(x2, y2);
            }
            else
            {
                var x1 = points[i].x + delta_x;
                var y1 = points[i].y + delta_y;
                var x2 = points[0].x + delta_x;
                var y2 = points[0].y + delta_y;
                line.Point1 = new Vector2(x1, y1);
                line.Point2 = new Vector2(x2, y2);
            }

            lines.Add(line);
        }
        return lines;
    }

    public float GetPos()
    {
        return Trans.localPosition.y;
    }
}

public class Act2075_Meteorite
{
    public bool IsTrigger; //是否触发了扣血
    public float RotatePara;
    public string Name;
    private PolygonCollider2D _pc2d;

    public List<Act2075Line> Lines;
    public GameObject gameObject { get; private set; }

    public Transform transform
    {
        get
        {
            if (gameObject == null)
                return null;
            return gameObject.transform;
        }
    }

    public Act2075_Meteorite(GameObject go, int track)
    {
        gameObject = go;
        Track = track;
        _pc2d = transform.Find<PolygonCollider2D>("");
        InitRef();
    }

    public int Track;

    private void InitRef()
    {
        gameObject.SetActive(true);
        transform.localPosition = new Vector3(140 * Track, 420, 0);
        RotatePara = Random.Range(-6f, 6f);
        IsTrigger = false;
    }


    public List<Act2075Line> GetLines()
    {
        List<Act2075Line> lines = new List<Act2075Line>();
        var points = GetPoints();
        var delta_x = transform.localPosition.x + transform.parent.localPosition.x;
        var delta_y = transform.localPosition.y + transform.parent.localPosition.y;
        for (int i = 0; i < points.Length; i++)
        {
            Act2075Line line = new Act2075Line();
            if (i + 1 < points.Length)
            {
                var newP1 = GetRotatePos(points[i]);
                var newP2 = GetRotatePos(points[i + 1]);

                var x1 = newP1.x + delta_x;
                var y1 = newP1.y + delta_y;
                var x2 = newP2.x + delta_x;
                var y2 = newP2.y + delta_y;

                line.Point1 = new Vector2(x1, y1);
                line.Point2 = new Vector2(x2, y2);
            }
            else
            {
                var newP1 = GetRotatePos(points[i]);
                var newP2 = GetRotatePos(points[0]);
                var x1 = newP1.x + delta_x;
                var y1 = newP1.y + delta_y;
                var x2 = newP2.x + delta_x;
                var y2 = newP2.y + delta_y;
                line.Point1 = new Vector2(x1, y1);
                line.Point2 = new Vector2(x2, y2);
            }

            lines.Add(line);
        }
        return lines;
    }

    private Vector2 GetRotatePos(Vector2 vec_in)
    {
        var rot = transform.localRotation.z;
        var cos = Mathf.Cos(rot);
        var sin = Mathf.Sin(rot);
        Vector2 vec_out = Vector2.zero;
        vec_out.x = vec_in.x * cos - vec_in.y * sin;
        vec_out.y = vec_in.x * sin + vec_in.y * cos;
        return vec_out;
    }

    public Vector2[] GetPoints()
    {
        return _pc2d.points;
    }

    public void Destroy()
    {
        Object.Destroy(gameObject);
    }
}

public class Act2075_HP
{
    public bool IsTrigger; //是否触发了回血
    public GameObject gameObject { get; private set; }
    private PolygonCollider2D _pc2d;
    public Transform transform
    {
        get
        {
            if (gameObject == null)
                return null;
            return gameObject.transform;
        }
    }

    public Act2075_HP(GameObject go)
    {
        gameObject = go;
        _pc2d = transform.Find<PolygonCollider2D>("");
        InitRef();
    }

    public int Track;

    private void InitRef()
    {
        gameObject.SetActive(true);
        var stage = _Activity_2075_Fighting.Inst.Stage;
        stage = stage == 2 ? 1 : 2;
        int random = Random.Range(-stage, stage + 1);
        Track = random + 3;
        transform.localPosition = new Vector3(140 * random, 420, 0);
        IsTrigger = false;
    }

    public List<Act2075Line> GetLines()
    {
        List<Act2075Line> lines = new List<Act2075Line>();
        var delta_x = transform.localPosition.x + transform.parent.localPosition.x;
        var delta_y = transform.localPosition.y + transform.parent.localPosition.y;
        var points = GetPoints();
        for (int i = 0; i < points.Length; i++)
        {
            Act2075Line line = new Act2075Line();
            if (i + 1 < points.Length)
            {
                var x1 = points[i].x + delta_x;
                var y1 = points[i].y + delta_y;
                var x2 = points[i + 1].x + delta_x;
                var y2 = points[i + 1].y + delta_y;

                line.Point1 = new Vector2(x1, y1);
                line.Point2 = new Vector2(x2, y2);
            }
            else
            {
                var x1 = points[i].x + delta_x;
                var y1 = points[i].y + delta_y;
                var x2 = points[0].x + delta_x;
                var y2 = points[0].y + delta_y;
                line.Point1 = new Vector2(x1, y1);
                line.Point2 = new Vector2(x2, y2);
            }

            lines.Add(line);
        }
        return lines;
    }

    public Vector2[] GetPoints()
    {
        return _pc2d.points;
    }

    public void Destroy()
    {
        Object.Destroy(gameObject);
    }
}

public class Act2075Line
{
    public Vector2 Point1;
    public Vector2 Point2;
}