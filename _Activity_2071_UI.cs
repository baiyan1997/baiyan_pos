using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2071_UI : ActivityUI
{
    private _RebelActMod _mod0;
    private _RebelActMod _mod1;
    private Text _textTotalTime;
    private Text _desc;
    private Button _btnShop1;
    private Button _btnShop2;
    private Button _helpBtn;
    private ObjectGroup UI;

    private ActInfo_2071 _actInfo;
    private P_RebelBattleInfo _rebelData;

    private int _aid = 2071;

    private void InitData()
    {
        _actInfo = (ActInfo_2071)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    public override void OnCreate()
    {
        UI = transform.GetComponent<ObjectGroup>();
        _mod0 = new _RebelActMod(UI.Get<ObjectGroup>("Mod0"));
        _mod1 = new _RebelActMod(UI.Get<ObjectGroup>("Mod1"));

        _textTotalTime = UI.Get<Text>("Text_time");
        _desc = UI.Get<Text>("TipDesc");
        _btnShop1 = UI.Get<Button>("_btnShop1");
        _btnShop2 = UI.Get<Button>("_btnShop2");
        _helpBtn = UI.Get<Button>("_btnManual");

        _helpBtn.onClick.AddListener(On_helpBtnClick);
        _btnShop1.onClick.AddListener(On_btnShop1Click);
        _btnShop2.onClick.AddListener(On_btnShop2Click);
        InitData();
    }
    private void On_helpBtnClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_helpDialogShowAsynCB);
    }
    private void On_helpDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.RebelBattle, _helpBtn.transform.position, Direction.RightDown, 600);
    }
    private void On_btnShop1Click()
    {
        DialogManager.ShowAsyn<_D_RebelBattle_Shop1>(On_btnShop1DialogShowAsynCB);
    }
    private void On_btnShop1DialogShowAsynCB(_D_RebelBattle_Shop1 d)
    {
        d?.OnShow();
    }
    private void On_btnShop2Click()
    {
        DialogManager.ShowAsyn<_D_RebelBattle_Shop2>(On_btnShop2DialogShowAsynCB);
    }
    private void On_btnShop2DialogShowAsynCB(_D_RebelBattle_Shop2 d)
    {
        d?.OnShow();
    }

    public override void OnShow()
    {
        _rebelData = ActInfo_2071.Inst.RebelData;
        if (_rebelData == null)
            return;
        Refresh();
        ShowAttackInfo();
        UpdateTime(0);
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.RebelBattleChange.AddListener(ShowAttackInfo);
    }

    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.RebelBattleChange.RemoveListener(ShowAttackInfo);
    }

    private void Refresh()
    {
        _mod0.SetCloseInfo(MapActID.REBEL_HUNTING);
        _mod1.SetCloseInfo(MapActID.REBEL_DEFENSE);
        switch (_rebelData.aid)
        {
            case MapActID.REBEL_HUNTING:
                _mod0.SetInfo(_rebelData);
                _desc.gameObject.SetActive(false);
                break;
            case MapActID.REBEL_DEFENSE:
                _mod1.SetInfo(_rebelData);
                _desc.gameObject.SetActive(true);
                break;
        }
    }

    public void ShowAttackInfo()
    {
        if (_rebelData.status == MapActID.REBEL_DEFENSE && _rebelData != null)
        {
            if (_rebelData.status == 0)
                _desc.text = string.Format(Lang.Get("您已成功抵御 {0}/{1}波"), _rebelData.def_succ, _rebelData.max_def);
            else if (_rebelData.status == 1)
                _desc.text = string.Format(Lang.Get("您最终成功抵御了 {0}/{1}波"), _rebelData.def_succ, _rebelData.max_def, _rebelData.def_succ);
            else if (_rebelData.status == 2)
                _desc.text = Lang.Get("您成功抵御了全部叛军");
        }
        else
        {
            _desc.text = "";
        }
    }

    public override void UpdateTime(long time)
    {
        base.UpdateTime(time);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        TimeSpan span = new TimeSpan(0, 0, (int)_actInfo.LeftTime);
        _textTotalTime.text = string.Format(Lang.Get("活动倒计时 {0}天{1}小时{2}分{3}秒"), span.Days, span.Hours,
            span.Minutes, span.Seconds);
        _mod0.Update();
        _mod1.Update();
    }
}

class _RebelActMod
{
    private ObjectGroup UI;
    private P_RebelBattleInfo _rebelData;
    private Image _itemIcon;
    private Text _itemName;
    private Text _time1;
    private Text _time2;
    private Text _desc;
    private GameObject _mask;
    private bool _isActive;


    public _RebelActMod(ObjectGroup ui)
    {
        UI = ui;
        _itemIcon = UI.Get<Image>("_itemIcon");
        _itemName = UI.Get<Text>("_itemName");
        _time1 = UI.Get<Text>("_time1");
        _time2 = UI.Get<Text>("_time2");
        _desc = UI.Get<Text>("_desc");
        _mask = UI.Get<GameObject>("_mask");
    }

    public void SetInfo(P_RebelBattleInfo info)
    {
        _isActive = true;
        _mask.SetActive(false);
        _rebelData = info;
        _time1.gameObject.SetActive(true);
        _time2.gameObject.SetActive(false);
        ShowActInfo(_rebelData.aid);
        Update();
    }

    public void SetCloseInfo(int aid)
    {
        _isActive = false;
        _mask.SetActive(true);
        _time1.gameObject.SetActive(false);
        _time2.gameObject.SetActive(true);
        _time2.text = Lang.Get("活动未开启");
        ShowActInfo(aid);
    }

    private void ShowActInfo(int aid)
    {
        int itemid = 0;
        switch (aid)
        {
            case MapActID.REBEL_HUNTING:
                itemid = ItemId.RebelBattleScore1;
                _desc.text =
                    string.Format(Lang.Get("击杀地图上的{0}可获得<Color=#{2}>{1}</Color>，凭借<Color=#{2}>{1}</Color>可在勋章兑换处兑换道具。"), Lang.Get("叛军"), Cfg.Item.GetItemName(itemid), Cfg.Item.GetItemQuaColor_FF(itemid));
                break;
            case MapActID.REBEL_DEFENSE:
                itemid = ItemId.RebelBattleScore2;
                _desc.text =
                    string.Format(Lang.Get("每隔<Color=#00ff33ff>5分钟</Color>，{0}会对基地发起攻击，成功抵御进攻可获得<Color=#{2}>{1}</Color>，积分可在积分商城兑换奖励。"), Lang.Get("叛军"), Cfg.Item.GetItemName(itemid), Cfg.Item.GetItemQuaColor_FF(itemid));
                break;
        }
        Cfg.Item.SetItemIcon(_itemIcon, itemid);
        _itemName.text = Cfg.Item.GetItemName(itemid);
    }

    public void Update()
    {
        if (_isActive)
        {
            _time1.text = string.Format(Lang.Get("结束倒计时 {0}"), WorldUtils.getLastTime_DHMS(_rebelData.endts));
        }
    }
}