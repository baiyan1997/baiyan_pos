using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2104_UI : ActivityUI
{
    private Button _btnTips;
    private List<_Act2104GameBox> _gameBoxList;//格子
    private GameObject _effectHighLight;//移动位置高亮特效
    private Button _btnDraw;//抽奖
    private Button _btnDraw5Times;//抽奖5次
    private JDText _textCountDown;
    private JDText _textRewardPriview;
    private Button _btnAddEnergyCrystal;
    private GameObject _objAddECrystalRemind;
    private JDText _textECrystalCount;
    private Button _btnRank;
    private JDText _textTotalDrawCount;
    private List<_Act2104SumDrwaRewardItem> _listSumDrwaReward;
    private GameObject _objBlock;//阻止点击活动界面
    private List<Transform> _listEffectPool;//5连抽特效
    private Button _btnRankReward;//活动结束时显示的领奖按钮

    private Tween _tween;//动画
    private int _curIndex;//当前位置
    private int _targetIndex;//目标位置
    private int _targetOrderInIdList;//目标id在id列表中的位置
    private bool _turnCircle;//标记是否转过一圈 转完一圈后才能停止
    private int _maxBoxCount;//格子总数
    private List<int> _listToDrawId;
    private readonly float _animeFrame = 0.05f;//经过每个格子需要的时间
    private readonly float _passTimePerBox = 0.08f;//经过每个格子需要的时间
    private readonly float _waitTimeAfterDraw = 1f;//每次抽到后的停顿时间
    private Action _drawCallback;//抽奖后回调


    private ActInfo_2104 _actInfo;
    private readonly int ActId = 2104;

    public override void OnCreate()
    {
        _btnTips = transform.Find<UnityEngine.UI.Button>("BtnHelp");
        _effectHighLight = transform.Find<GameObject>("IconMid/EffectLighLight");
        _btnDraw = transform.Find<UnityEngine.UI.Button>("DrawBtns/btnDraw");
        _btnDraw5Times = transform.Find<UnityEngine.UI.Button>("DrawBtns/btnDraw5Times");
        _textCountDown = transform.Find<UnityEngine.UI.JDText>("TextTime");
        _textRewardPriview = transform.Find<JDText>("TextRewardPriview");
        _btnAddEnergyCrystal = transform.Find<UnityEngine.UI.Button>("BtnAddEnergyCrystal");
        _objAddECrystalRemind = transform.Find<GameObject>("BtnAddEnergyCrystal/RedPoint");
        _textECrystalCount = transform.Find<JDText>("BtnAddEnergyCrystal/Text");
        _btnRank = transform.Find<UnityEngine.UI.Button>("Icon02/BtnRank");
        _textTotalDrawCount = transform.Find<UnityEngine.UI.JDText>("Icon02/Title");
        _objBlock = transform.Find<GameObject>("Block");
        var effectRoot = transform.Find("IconMid/LockOnEffect");
        _btnRankReward = transform.Find<UnityEngine.UI.Button>("ButtonRankReward");
        _listEffectPool = new List<Transform>();
        for (int i = 0; i < effectRoot.childCount; i++)
        {
            var item = effectRoot.GetChild(i);
            _listEffectPool.Add(item);
            item.gameObject.SetActive(false);
        }
        var poolRewardRoot = transform.Find("IconMid/Scroll View/Viewport/Content");
        _maxBoxCount = poolRewardRoot.childCount;
        _gameBoxList = new List<_Act2104GameBox>();
        for (int i = 0; i < poolRewardRoot.childCount; i++)
        {
            var item = poolRewardRoot.GetChild(i);
            _gameBoxList.Add(item.gameObject.AddBehaviour<_Act2104GameBox>().Init(i));
        }
        var sunRewardRoot = transform.Find("Icon02/Items");
        _listSumDrwaReward = new List<_Act2104SumDrwaRewardItem>();
        for (int i = 0; i < sunRewardRoot.childCount; i++)
        {
            var item = sunRewardRoot.GetChild(i);
            _listSumDrwaReward.Add(item.gameObject.AddBehaviour<_Act2104SumDrwaRewardItem>());
        }

        //点击文本跳转到对应战舰预览界面
        _textRewardPriview.AddHyperlinkCallback((text, url) =>
        {
            int shipId;
            if (int.TryParse(url, out shipId))
            {
                DialogManager.ShowAsyn<_D_ShipWay>(d=>{ d?.OnShow(ShipYardInfo.Instance.GetShipBaseInfo(shipId));});
            }
        });

        InitButtons();
    }

    public override void InitListener()
    {
        base.InitListener();
        TimeManager.Instance.TimePassSecond += UpdateCountDown;
        EventCenter.Instance.UpdatePlayerItem.AddListener(_OnUpdatePlayerItem);
    }

    public override void UnInitListener()
    {
        base.UnInitListener();
        TimeManager.Instance.TimePassSecond -= UpdateCountDown;
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(_OnUpdatePlayerItem);
    }

    protected void _OnUpdatePlayerItem()
    {
        if (!DialogManager.IsDialogShown<_D_ActCalendar>())
            return;
        if (_actInfo == null || !_actInfo.IsDuration())
            return;
        RefreshECrystal();
    }

    private void InitButtons()
    {
        _btnDraw.onClick.AddListener(() =>
        {
            //判断源晶数量是否足够
            if(BagInfo.Instance.GetItemCount(ItemId.Act2104ECrystal) < 1)
            {
                //弹出源晶购买界面
                DialogManager.ShowAsyn<_D_Top_2104BuyECrystal>(d => {d?.OnShow();});
                return;
            }
            Clear();
            _actInfo.Draw(data =>
            {
                _listToDrawId = _actInfo.DrawIdList;
                DoDraw(data, true);
            });
        });

        _btnDraw5Times.onClick.AddListener(() =>
        {
            //判断源晶数量是否足够
            if (BagInfo.Instance.GetItemCount(ItemId.Act2104ECrystal) < 5)
            {
                //弹出源晶购买界面
                DialogManager.ShowAsyn<_D_Top_2104BuyECrystal>(d => {d?.OnShow();});
                return;
            }
            Clear();
            _actInfo.Draw5Times(data =>
            {
                _listToDrawId = _actInfo.DrawIdList;
                DoDraw(data, false);
            });
        });

        _btnRank.onClick.AddListener(() =>
        {
            DialogManager.ShowAsyn<_D_Top_2104Rank>(d => {d?.OnShow();});
        });
        _btnRankReward.onClick.AddListener(() =>
        {
            DialogManager.ShowAsyn<_D_Top_2104Rank>(d => {d?.OnShow();});
        });

        _btnTips.onClick.AddListener(() =>
        {
            DialogManager.ShowAsyn<_D_Tips_HelpDesc>(d => {d?.OnShow(HelpType.Act2104, _btnTips.transform.position, Direction.LeftDown, 350);});
        });

        _btnAddEnergyCrystal.onClick.AddListener(() =>
        {
            DialogManager.ShowAsyn<_D_Top_2104BuyECrystal>(d => {d?.OnShow();});
        });
    }

    public override void OnShow()
    {
        //清空状态
        Clear();
        //刷新活动数据
        _actInfo = (ActInfo_2104)ActivityManager.Instance.GetActivityInfo(ActId);
        //刷新奖池
        RefreshRewardPool();
        //刷新额外奖池
        RefreshSumReward();
        //刷新稀有战舰
        var shipId = _actInfo.RewardShipId;
        _textRewardPriview.text = Lang.Get("S战舰{0}限时抽取<MixType=link color = #00ffff url = {1}>[查看战舰]</MixType>", Cfg.Ship.GetShipName(shipId), shipId);
        //刷新源晶
        RefreshECrystal();
        //刷新倒计时
        UpdateCountDown(TimeManager.ServerTimestamp);
    }

    private void RefreshRewardPool()
    {
        var poolList = _actInfo.ListDrawPool;
        for (int i = 0; i < _gameBoxList.Count; i++)
        {
            _gameBoxList[i].Refresh(poolList[i]);
        }
    }

    private void RefreshSumReward()
    {
        var sumRewardList = _actInfo.ListSumDrawReward;
        //先全部隐藏
        for (int i = 0; i < _listSumDrwaReward.Count; i++)
        {
            _listSumDrwaReward[i].gameObject.SetActive(false);
        }
        //只显示存在的奖励item
        for (int i = 0; i < sumRewardList.Count; i++)
        {
            _listSumDrwaReward[i].gameObject.SetActive(true);
            _listSumDrwaReward[i].Refresh(sumRewardList[i]);
        }
        //刷新抽奖次数
        _textTotalDrawCount.text = Lang.Get("额外奖励:本期累计<Color=#00ff00ff>{0}</Color>次", _actInfo.DrawCount);
    }

    private void RefreshECrystal()
    {
        var eCrystalCount = BagInfo.Instance.GetItemCount(ItemId.Act2104ECrystal);
        _textECrystalCount.text = string.Format("x{0}", GLobal.NumFormat(eCrystalCount));
        //源晶少于5个时不显示5连抽按钮
        _btnDraw5Times.gameObject.SetActive(eCrystalCount >= 5);
    }

    //设置高光特效位置
    private void SetMoveEffectPosition(int id)
    {
        var pos = GetBoxByIndex(id).transform.position;
        //设置格子高亮显示
        _effectHighLight.SetActive(true);
        _effectHighLight.transform.position = pos;
    }

    //设置奖励特效位置
    private void SetRewardEffectPosition(int orderInIdList)
    {
        var id = _listToDrawId[orderInIdList];
        for(int i = 0; i < orderInIdList; i++)
        {
            //如果一个格子被连续选中那只需要加上一个特效
            if (_listToDrawId[i] == id)
                return;
        }
        for (int i = 0; i < _listEffectPool.Count; i++)
        {
            //找到第一个没有显示的特效改变位置
            var effect = _listEffectPool[i];
            if (!effect.gameObject.activeSelf)
            {
                var pos = GetBox(id).transform.position;
                effect.gameObject.SetActive(true);
                effect.position = pos;
                return;
            }
        }
    }

    private void DoMovePath(Action callback)
    {
        Clear();
        //根据勾选和抽奖次数决定是否跳过动画
        if(_listToDrawId.Count == 1 || PromptInfo.Instance.GetValue(PromptOpcode.Skip2104DrawAnime))
        {
            var curId = _listToDrawId[_targetOrderInIdList];
            _targetIndex = GetBox(curId).Index;
            _drawCallback = callback;
            SetMoveEffectPosition(_curIndex);//初始化高亮位置
                                             //播放动画时阻止点击活动界面
            if (!_objBlock.activeSelf)
                _objBlock.SetActive(true);
            //开始播放动画
            float v = 0;
            _tween = DOTween.To(() => v, value => v = value, 1, _animeFrame).SetLoops(-1).OnStepComplete(Update);
        }
        else
        {
            callback?.Invoke();
        }
    }

    private _Act2104GameBox GetBoxByIndex(int index)
    {
        for (int i = 0; i < _gameBoxList.Count; i++)
        {
            if (_gameBoxList[i].Index == index)
                return _gameBoxList[i];
        }
        return null;
    }

    private _Act2104GameBox GetBox(int id)
    {
        for (int i = 0; i < _gameBoxList.Count; i++)
        {
            if (_gameBoxList[i].Id == id)
                return _gameBoxList[i];
        }
        return null;
    }

    //抽奖
    private void DoDraw(P_Act2104Draw data, bool isSingleDraw)
    {
        DoMovePath(() =>
        {
            //弹出奖励
            DialogManager.ShowAsyn<_D_Top_2104GetRewards>(d => {d?.OnShow(data.get_items, isSingleDraw, () =>
            {
                //再抽5次
                Clear();
                _actInfo.Draw5Times(data2 =>
                {
                    _listToDrawId = _actInfo.DrawIdList;
                    DoDraw(data2, false);
                });
            });});

            Clear();
            //刷新奖池
            RefreshRewardPool();
            //刷新累计奖励
            RefreshSumReward();
            //刷新源晶数量
            RefreshECrystal();
        });
    }

    private float _ts;
    private float _tmpAddTs;//临时增加的等待时间
    private void Update()
    {
        _ts += _passTimePerBox;
        //每过特定时间前进一格
        if (_ts >= _passTimePerBox + _tmpAddTs)
        {
            _ts -= _passTimePerBox + _tmpAddTs;
            _tmpAddTs = 0;
            _curIndex++;
            if (_curIndex >= _maxBoxCount)
            {
                _curIndex = 0;
                _turnCircle = true;
            }
            SetMoveEffectPosition(_curIndex);
            //绕完一圈后到指定id停止
            if (_turnCircle)
            {
                if (_curIndex == _targetIndex)
                {
                    //设置奖励选中特效
                    SetRewardEffectPosition(_targetOrderInIdList);
                    //如果抽中了稀有奖 刷新奖池
                    var targetBox = GetBox(_listToDrawId[_targetOrderInIdList]);
                    if (targetBox.IsRare)
                        RefreshRewardPool();
                    //判断是否继续抽奖
                    _targetOrderInIdList++;
                    if(_listToDrawId.Count > _targetOrderInIdList)
                    {
                        _targetIndex = GetBox(_listToDrawId[_targetOrderInIdList]).Index;//设置新的目标位置
                        _tmpAddTs = _waitTimeAfterDraw;//每次抽完后停顿一定时间
                    }
                    else
                    {
                        _tween.Kill();//终止动画
                        _effectHighLight.SetActive(false);
                        _objBlock.SetActive(false);
                        _drawCallback?.Invoke();//执行回调
                    }
                }
            }
        }
    }

    private void UpdateCountDown(long ts)
    {
        var leftTime = (int)(_actInfo.DrawEndTs - ts);
        if(leftTime > 0)
        {
            _textCountDown.text = Lang.Get("抽奖倒计时{0}", GLobal.TimeFormat(leftTime, true));
            _btnDraw.transform.parent.gameObject.SetActive(true);
            _btnRankReward.gameObject.SetActive(false);
        }
        else
        {
            leftTime = (int)_actInfo.LeftTime;
            if (leftTime < 0)
                leftTime = 0;
            _textCountDown.text = Lang.Get("领奖倒计时{0}", GLobal.TimeFormat(leftTime, true));
            _btnDraw.transform.parent.gameObject.SetActive(false);
            _btnRankReward.gameObject.SetActive(true);
        }
    }

    private void Clear()
    {
        _ts = 0;
        _tmpAddTs = 0;
        _curIndex = 0;
        _targetIndex = 0;
        _targetOrderInIdList = 0;
        if(_tween != null)
        {
            _tween.Kill();
            _tween = null;
        }
        _turnCircle = false;
        _objBlock.SetActive(false);
        _effectHighLight.SetActive(false);
        for (int i = 0; i < _listEffectPool.Count; i++)
        {
            _listEffectPool[i].gameObject.SetActive(false);
        }
    }
}

public class _Act2104GameBox : JDBehaviour
{
    private Image _icon;
    private Image _iconQua;
    private Text _textCount;
    private Text _textLeft;
    private Image _bg;
    private GameObject _objGot;
    private GameObject _objRare;
    private Button _btnPreview;

    private Color _colorNormal = new Color(1, 0, 0);
    private Color _colorHighLight = new Color(0.36f, 1, 0);

    public int Index { get; private set; }
    public int Id { get { return _info.id; } }
    public bool IsRare { get; private set; }

    private P_Act2104DrawPoolReward _info;

    public override void Awake()
    {
        base.Awake();
        _icon = transform.Find<Image>("Img_icon");
        _iconQua = transform.Find<Image>("qua");
        _textCount = transform.Find<Text>("Text_num");
        _bg = transform.GetComponent<Image>();
        _textLeft = transform.Find<Text>("ImageLeft/TextLeftCount");
        _objGot = transform.Find<GameObject>("Got");
        _objRare = transform.Find<GameObject>("ObjRare");
        _btnPreview = transform.Find<Button>("Img_icon");

        _btnPreview.onClick.AddListener(() =>
        {
            var itemInfo = GlobalUtils.ParseItem(_info.item)[0];
            DialogManager.ShowAsyn<_D_ItemTip>(d => {d?.OnShow(itemInfo.id, itemInfo.count, _btnPreview.transform.position);});
        });
    }

    public _Act2104GameBox Init(int index)
    {
        Index = index;
        return this;
    }

    public void Refresh(P_Act2104DrawPoolReward info)
    {
        _info = info;
        var itemInfo = GlobalUtils.ParseItem(info.item)[0];
        var itemForShow = ItemForShow.Create(itemInfo.id, itemInfo.count);
        // _icon.sprite = itemForShow.GetIcon();
        itemForShow.SetIcon(_icon);
        _iconQua.color = _ColorConfig.GetQuaColorHSV(itemForShow.GetQua());
        _textCount.text = string.Format("x{0}", GLobal.NumFormat(itemForShow.GetCount()));
        _bg.color = info.is_win > 0 ? _colorHighLight : _colorNormal;
        _textLeft.text = GLobal.NumFormat(info.count);
        _objGot.SetActive(info.count == 0);
        _textLeft.transform.parent.gameObject.SetActive(info.count > 0 && info.is_win == 0);//剩余数量为0或大奖不显示剩余数量
        IsRare = info.is_win > 0;
        _objRare.SetActive(IsRare);
    }
}

public class _Act2104SumDrwaRewardItem : JDBehaviour
{
    private Image _icon;
    private Image _iconQua;
    private Text _textCount;
    private GameObject _objGot;
    private GameObject _objNotEnough;
    private Button _btnGetReward;
    private JDText _textNeedCount;
    private Image _iconCorner;

    private Color[] _colorCorner = new Color[]
    {
        new Color(0.84f, 1, 0),
        new Color(1, 0, 0.18f)
    };

    private P_Act2104SumDrawReward _info;
    private readonly int ActId = 2104;

    public override void Awake()
    {
        _icon = transform.Find<Image>("img_icon");
        _iconQua = transform.Find<Image>("Img_qua");
        _textCount = transform.Find<Text>("Text_num");
        _objGot = transform.Find<GameObject>("ObjGot");
        _objNotEnough = transform.Find<GameObject>("ObjNotEnough");
        _btnGetReward = transform.Find<Button>("img_icon");
        _textNeedCount = transform.Find<JDText>("ObjCond/Text");
        _iconCorner = transform.Find<Image>("ObjCond");

        _btnGetReward.onClick.AddListener(() =>
        {
            if (_info.is_get > 0)
                return;
            var actInfo = (ActInfo_2104)ActivityManager.Instance.GetActivityInfo(ActId);
            if (actInfo.DrawCount < _info.num)
            {
                //显示礼包详情
                DialogManager.ShowAsyn<_D_ShowRewards>(d => {d?.ShowCustonRewards(GlobalUtils.ParseItem(_info.reward).ToList(), 
                    Lang.Get("{0}次额外奖励", _info.num), Lang.Get("您可获得以下所有道具"), Lang.Get("确定"));});
            }
            else
            {
                actInfo.TakeSumReward(_info, data =>
                {
                    Refresh(_info);
                });
            }
        });
    }

    public void Refresh(P_Act2104SumDrawReward info)
    {
        _info = info;
        var actInfo = (ActInfo_2104)ActivityManager.Instance.GetActivityInfo(ActId);
        var itemInfo = GlobalUtils.ParseItem(info.reward)[0];
        UIHelper.SetImageSprite(_icon, Cfg.Item.GetItemIconPath(itemInfo.id));
        _iconQua.color = _ColorConfig.GetQuaColorHSV(Cfg.Item.GetItemQua(itemInfo.id));
        _textCount.text = string.Format("x{0}", GLobal.NumFormat(itemInfo.count));
        _objGot.SetActive(info.is_get > 0);
        _objNotEnough.SetActive(false);

        if(info.is_get == 0 && info.num <= actInfo.DrawCount)
        {
            _textNeedCount.text = Lang.Get("可领取");
            _iconCorner.color = _colorCorner[1];
            //可以领奖时添加按钮特效
            var effect = EffectManager.FindEffect<RectBtnEffect>(transform, IconTag.Act2104SumRewardBtn);
            if (effect == null)
                new RectBtnEffect(transform, Layer.Effects, IconTag.Act2104SumRewardBtn);
            else
                effect.ShowEffect();
        }
        else
        {
            _textNeedCount.text = Lang.Get("{0}次", info.num);
            _iconCorner.color = _colorCorner[0];
            //移除特效
            var effect = EffectManager.FindEffect<RectBtnEffect>(transform, IconTag.Act2104SumRewardBtn);
            if (effect != null)
                effect.HideEffect();
        }
    }
}