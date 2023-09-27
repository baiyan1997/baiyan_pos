using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class _Activity_2082_UI : ActivityUI
{
    private Button _getFireMat;//获取爆竹材料 任务完成情况界面
    private Button _buyFireBuff;//祈福一次
    private Text _buyFireBuffCost;//祈福一次 消耗
    private Text _fireBuffText;//本次是否祈福
    private Button _buyFire;//购买爆竹
    private Text _buyFireCost;//购买爆竹 消耗
    private Button _makeFire;//合成爆竹
    private Button _lightFire;//燃放爆竹
    private Button _reward;//查看爆竹奖品
    private Button _tip;//查看活动规则

    private ActInfo_2082 _actInfo;
    private const int Aid = 2082;


    private Text _haveFire;//拥有爆竹数量
    private FireMatItem[] _matItem;//三个材料的数量

    private GameObject _superFire;//超级爆竹
    private GameObject _fireCracker;//低等级爆竹
    //倒计时
    private Text _actTime;



    //动画
    private Animator _anim;
    private Sequence _seq;//记录动画播放时间
    private GameObject _buffEffect;//buff特效
    private GameObject _prompt;
    private Button _btnPrompt;
    private Button _btnCancelrompt;
    private Button _btnPromptArea;
    private Button _btnCancelromptArea;
    private int isReturn
    {
        get { return PlayerPrefs.GetInt(User.Uid + "isReturn2082", 0); }
        set { PlayerPrefs.SetInt(User.Uid + "isReturn2082", value); }
    }
    public override void OnCreate()
    {
        _buyFire = transform.Find<Button>("Btn_buyFire");
        _buyFireCost = transform.Find<Text>("Btn_buyFire/Text_cost");
        _buyFireBuff = transform.Find<Button>("Btn_buyFireBuff");
        _buyFireBuffCost = transform.Find<Text>("Btn_buyFireBuff/Text_cost");
        _fireBuffText = transform.Find<Text>("Btn_buyFireBuff/Text");
        _makeFire = transform.Find<Button>("Mid/Btn_makeFire");
        _lightFire = transform.Find<Button>("Mid/FireCrackers/Ani/baozu");
        _buffEffect = transform.Find<GameObject>("Mid/FireCrackers/Ani/baozu/Image_baozu_qifu_glow");
        _superFire = _lightFire.transform.Find<GameObject>("FireCrackersHigh");
        _fireCracker = _lightFire.transform.Find<GameObject>("FireCrackersLow");

        _getFireMat = transform.Find<Button>("Btn_getFireMat");
        _reward = transform.Find<Button>("Btn_reward");
        _tip = transform.Find<Button>("Btn_tip");
        _actTime = transform.Find<Text>("Text_time");

        _haveFire = transform.Find<Text>("Mid/Text_fireNum");
        _matItem = new[]
        {
            transform.Find<GameObject>("Mid/Icon1").AddBehaviour<FireMatItem>(),
            transform.Find<GameObject>("Mid/Icon2").AddBehaviour<FireMatItem>(),
            transform.Find<GameObject>("Mid/Icon3").AddBehaviour<FireMatItem>(),
        };
        _anim = transform.Find<Animator>("Mid/FireCrackers/Ani");

        _prompt = transform.Find("Prompt").gameObject;
        _btnPrompt = transform.Find<Button>("Prompt/ButtonYesArea/ButtonYes");
        _btnPromptArea = transform.Find<Button>("Prompt/ButtonYesArea");
        _btnCancelrompt = transform.Find<Button>("Prompt/ButtonNoArea/ButtonNo");
        _btnCancelromptArea = transform.Find<Button>("Prompt/ButtonNoArea");
        _btnCancelromptArea.gameObject.SetActive(isReturn == 1);
        //买爆竹
        _buyFire.onClick.AddListener(On_buyFireClick);
        //买buff
        _buyFireBuff.onClick.AddListener(On_buyFireBuffClick);
        //合成爆竹
        _makeFire.onClick.AddListener(On_makeFireClick);
        //点燃爆竹
        _lightFire.onClick.AddListener(On_lightFireClick);
        //查看奖励
        _reward.onClick.AddListener(On_rewardClick);
        //查看任务完成情况
        _getFireMat.onClick.AddListener(On_getFireMatClick);
        //tip按钮
        _tip.onClick.AddListener(On_tipClick);

        _btnPrompt.onClick.AddListener(OnBtnPromptClick);
        _btnPromptArea.onClick.AddListener(OnBtnPromptClick);
        _btnCancelrompt.onClick.AddListener(OnBtnCancelRomptClick);
        _btnCancelromptArea.onClick.AddListener(OnBtnCancelRomptClick);

        //InitListener();
        //EventCenter.Instance.UpdateActivityUI.AddListener(ActivityUpdate);
        //TimeManager.Instance.TimePassSecond += UpdateTime;
        //EventCenter.Instance.UpdatePlayerItem.AddListener(OnItemUpdate);
    }
    private void OnBtnPromptClick()
    {
        _btnCancelromptArea.gameObject.SetActive(true);
        isReturn = 1;
    }
    private void OnBtnCancelRomptClick()
    {
        _btnCancelromptArea.gameObject.SetActive(false);
        isReturn = 0;
    }
    private void On_buyFireClick()
    {
        if (_seq != null && _seq.IsPlaying())
            return;

        //氪晶不足提示
        var cost = Cfg.Activity2082.GetBuyFireCrackerCost();
        var enough = ItemHelper.IsCountEnoughWithFalseHandle(cost.id, cost.count, null);
        if (!enough)
            return;

        //消耗提示
        var forShow = ItemForShow.Create(cost.id, cost.count);
        string str = "购买";
        var alert = Lang.Get("是否消耗<color={0}>{1}x{2}</color>{3}爆竹",
            _ColorConfig.GetQuaColorText(forShow.GetQua()),
            forShow.GetName(),
            GLobal.NumFormat(forShow.GetCount()), str);

        PromptInfo.Instance.Notice(PromptOpcode.BuyFirecracker, alert, On_buyFireNoticeCB);
    }
    private void On_buyFireNoticeCB()
    {
        _actInfo.BuyFirecrackers(MakeFireAnim);
    }
    private void On_buyFireBuffClick()
    {
        if (_seq != null && _seq.IsPlaying())
            return;

        //超级爆竹不用祈福
        if (_actInfo.isSuperFire())
        {
            MessageManager.Show(Lang.Get("超级爆竹不需要祈福"));
            return;
        }

        //没有爆竹不能祈福
        var fireCount = BagInfo.Instance.GetItemCount(ItemId.Firecracker);
        if (fireCount <= 0)
        {
            MessageManager.Show(Lang.Get("没有爆竹，不能祈福"));
            return;
        }

        //氪晶不足提示
        var cost = Cfg.Activity2082.GetBuyBuffCost();
        var enough = ItemHelper.IsCountEnoughWithFalseHandle(cost.id, cost.count, null);
        if (!enough)
            return;


        //消耗提示
        var forShow = ItemForShow.Create(cost.id, cost.count);
        string str = "购买";
        var alert = Lang.Get("是否消耗<color={0}>{1}x{2}</color>{3}祈福buff",
            _ColorConfig.GetQuaColorText(forShow.GetQua()),
            forShow.GetName(),
            GLobal.NumFormat(forShow.GetCount()), str);
        PromptInfo.Instance.Notice(PromptOpcode.BuyFireBuff, alert, On_buyFireBuffNoticeCB);
    }
    private void On_buyFireBuffNoticeCB()
    {
        _actInfo.BuyLuckBuff(FireBuffAnim);
    }
    private void On_makeFireClick()
    {
        if (_seq != null && _seq.IsPlaying())
            return;

        var costs = Cfg.Activity2082.GetMakeFireCrackerCost();
        for (int i = 0; i < costs.Length; i++)
        {
            var cost = costs[i];
            var enough = ItemHelper.IsCountEnough(cost.id, cost.count);
            if (!enough)
            {
                ShowNotEnoughNote(cost);
                return;
            }
        }
        _actInfo.MakeFirecrackers(MakeFireAnim);
    }
    private void On_lightFireClick()
    {
        if (_seq != null && _seq.IsPlaying())
            return;

        var cost = Cfg.Activity2082.GetLightFirecrackersCost();
        var enough = ItemHelper.IsCountEnough(cost.id, cost.count);
        if (enough || _actInfo.isSuperFire())
            _actInfo.FireFirecrackers(LightFireAnim);
        else
            ShowNotEnoughNote(cost);
    }
    private void On_rewardClick()
    {
        DialogManager.ShowAsyn<_D_Act2082Reward>(On_rewardDialogShowAsynCB);
    }
    private void On_rewardDialogShowAsynCB(_D_Act2082Reward d)
    {
        var hasBuff = _actInfo.GetLuckBuffCount() > 0;
        d?.OnShow(_actInfo.GetRewardList(), _actInfo.GetExchangeDic(), hasBuff);
    }
    private void On_getFireMatClick()
    {
        DialogManager.ShowAsyn<_D_Act2082Mission>(On_getFireMatDialogShowAsynCB);
    }
    private void On_getFireMatDialogShowAsynCB(_D_Act2082Mission d)
    {
        d?.OnShow(_actInfo.GetMissionList());
    }
    private void On_tipClick()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(On_tipDialogShowAsynCB);
    }
    private void On_tipDialogShowAsynCB(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2082, _tip.transform.position, Direction.LeftDown, 350);
    }
    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdatePlayerItem.AddListener(OnItemUpdate);
    }
    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdatePlayerItem.RemoveListener(OnItemUpdate);
    }

    //合成爆竹成功动画
    private void MakeFireAnim()
    {
        if (isReturn == 1) {
            SetBaozuOriginState();
            return;
        }
        _seq = DOTween.Sequence();
        _seq.PrependCallback(() =>
            {
                _anim.speed = 1;
                _anim.Play("ani_pfb_eff_baozu_newyear01", -1, 0);
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                SetBaozuOriginState();
            });
    }

    //燃放爆竹动画
    private void LightFireAnim(string reward)
    {
        if (isReturn == 1) {
            if (!string.IsNullOrEmpty(reward)) {
                MessageManager.ShowRewards(reward);
            }
            SetBaozuOriginState();
            return;
        }
        _seq = DOTween.Sequence();
        _seq.PrependCallback(() =>
            {
                _anim.speed = 1;
                _anim.Play("ani_pfb_eff_baozu_newyear03", -1, 0);
            })
            .AppendInterval(2.25f)
            .AppendCallback(() =>
            {
                if (!string.IsNullOrEmpty(reward))
                {
                    MessageManager.ShowRewards(reward);
                }
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                SetBaozuOriginState();
            });
    }
    //祈福动画
    private void FireBuffAnim()
    {
        if (isReturn == 1) {
            SetBaozuOriginState();
            return;
        }
        _seq = DOTween.Sequence();
        _seq.PrependCallback(() =>
            {
                _anim.speed = 1;
                _anim.Play("ani_pfb_eff_baozu_newyear02", -1, 0);
            })
            .AppendInterval(2.3f)
            .AppendCallback(() =>
            {
                SetBaozuOriginState();
            });
    }

    public override void OnClose()
    {
        base.OnClose();

        if (_seq != null && _seq.IsPlaying())
            _seq.Complete(true);
    }

    private void SetBaozuOriginState()
    {
        _anim.Play("ani_pfb_eff_baozu_newyear04", -1, 0);

        //下次燃放是否是高等级爆竹
        if (_actInfo.isSuperFire())
        {
            _fireCracker.SetActive(false);
            _superFire.SetActive(true);
        }
        else
        {
            _fireCracker.SetActive(true);
            _superFire.SetActive(false);
        }

        //祈福buff
        if (_actInfo.GetLuckBuffCount() > 0)
        {
            _buffEffect.SetActive(true);
        }
        else
        {
            _buffEffect.SetActive(false);
        }

    }
    private void ShowNotEnoughNote(P_Item _costItem)
    {
        var forShow = ItemForShow.Create(_costItem.id, _costItem.count);
        Alert.Ok(Lang.Get("<color={0}>{1}</color>不足", //{2}
            _ColorConfig.GetQuaColorText(forShow.GetQua()),
            forShow.GetName())); //forShow.GetCount()
    }

    public override void OnShow()
    {
        Refresh(true);
        UpdateTime(TimeManager.ServerTimestamp);
    }
    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != Aid)
            return;

        Refresh(false);
    }

    private void OnItemUpdate()
    {
        Refresh(false);
    }
    private void Refresh(bool onShow)
    {
        if (!DialogManager.IsDialogShown<_D_ActCalendar>())
            return;
        _actInfo = (ActInfo_2082)ActivityManager.Instance.GetActivityInfo(Aid);
        if (onShow)
        {
            SetBaozuOriginState();
        }

        //拥有爆竹
        var count = BagInfo.Instance.GetItemCount(ItemId.Firecracker);
        _haveFire.text = Lang.Get("当前拥有爆竹：{0}", GLobal.NumFormat(count));

        //祈福消耗
        var buffCost = Cfg.Activity2082.GetBuyBuffCost();
        _buyFireBuffCost.text = Lang.Get("{0}{1}", buffCost.count, Cfg.Item.GetItemName(buffCost.id));

        if (_actInfo.GetLuckBuffCount() > 0)
        {
            _buyFireBuff.interactable = false;
            _fireBuffText.text = Lang.Get("本次已祈福");
        }
        else
        {
            _buyFireBuff.interactable = true;
            _fireBuffText.text = Lang.Get("祈福buff一次");
        }

        //购买爆竹消耗
        var fireCost = Cfg.Activity2082.GetBuyFireCrackerCost();
        _buyFireCost.text = Lang.Get("{0}{1}", fireCost.count, Cfg.Item.GetItemName(fireCost.id));


        RefrshMatCount();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_seq != null)
        {
            _seq.Kill();
            _seq = null;
        }
    }
    public override void UpdateTime(long nowTs)
    {
        base.UpdateTime(nowTs);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (nowTs - _actInfo._data.startts < 0)
        {
            _actTime.text = GlobalUtils.GetActivityStartTimeDesc(_actInfo._data.startts);
        }
        else if (_actInfo.LeftTime >= 0)
        {
            _actTime.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _actTime.text = Lang.Get("活动已经结束");
        }
    }
    private void RefrshMatCount()
    {
        var cost = Cfg.Activity2082.GetMakeFireCrackerCost();
        for (int i = 0; i < cost.Length; i++)
        {
            _matItem[i].Refresh(cost[i].id);
        }
    }
    public class FireMatItem : JDBehaviour
    {
        //        private Image _icon;
        private Text _num;
        private Button _btn;
        public override void Awake()
        {
            base.Awake();
            //            _icon = transform.Find<Image>("Icon");
            _num = transform.Find<Text>("Text");
            _btn = transform.Find<Button>("Icon");
        }

        public void Refresh(int id)
        {
            //            Cfg.Item.SetItemIcon(_icon,id);
            var count = BagInfo.Instance.GetItemCount(id);
            _num.text = Lang.Get("拥有:{0}", GLobal.NumFormat(count));

            _btn.onClick.SetListener(() =>
            {
                ItemHelper.ShowTip(id, 1, _btn.transform);
            });
        }
    }
}
