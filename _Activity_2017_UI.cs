using UnityEngine;
using UnityEngine.UI;

public class _Activity_2017_UI : ActivityUI
{
    private Text _descText;
    private int _aid = 2017;
    private ObjectGroup UI;
    private ActInfo_2017 actInfo;
    private void InitData()
    {
        actInfo = (ActInfo_2017)ActivityManager.Instance.GetActivityInfo(_aid);
    }

    private void InitEvent()
    {
        _descText = transform.Find<JDText>("Layout/Text_desc");
        UI.Get<Button>("Btn_claim").onClick.AddListener(OnBtn_claimClick);
        UI.Get<Button>("Btn_Replacement").onClick.AddListener(ReplacementTip);
    }
    private void OnBtn_claimClick()
    {
        actInfo.GetAct2017Reward(OnAct2017RewardCB);  
    }
    private void OnAct2017RewardCB(string data)
    {
        UpdateUi(_aid);
        DialogManager.ShowAsyn<_D_Act2017Reward>(d => { d?.OnShow(data); });
    }   

    public override void InitListener()
    {
        base.InitListener();
        EventCenter.Instance.UpdateAllActivity.AddListener(RefreshUi);
    }

    public override void UnInitListener()
    {
        base.UnInitListener();
        EventCenter.Instance.UpdateAllActivity.RemoveListener(RefreshUi);
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        UpdateUi(aid);
    }

    private void ReplacementTip()
    {
        long goldNum = BagInfo.Instance.GetItemCount(ItemId.Gold);
        if (goldNum < 50)
        {
            DialogManager.ShowAsyn<_D_JumpConfirm>(d => { d?.OnShow(JumpType.Kr, (int)goldNum); });
            return;
        }
        StepType type = actInfo.SupplyStep();
        string msg;
        if (type == StepType.WaitEvening)
        {
            msg = Lang.Get("是否消耗50氪晶补领午间补给");
        }
        else
        {
            msg = Lang.Get("是否消耗50氪晶补领晚间补给");
        }
        var temp = Alert.YesNo(msg);
        temp.SetYesCallback(() =>
        {
            actInfo.GetAct2017Reward(data =>
            {

                UpdateUi(_aid);
                DialogManager.ShowAsyn<_D_Act2017Reward>(d => { d?.OnShow(data); });
                temp.Close();
            });
        });
        temp.SetNoCallback(temp.Close);
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

    private void InitUI()
    {
        _descText.text = Cfg.Act.GetData(_aid).act_desc;
        UpdateUi(_aid);
    }

    private void UpdateUi(int aid)
    {
        if (aid == _aid)
        {
            RefreshUi();
        }
    }
    private void RefreshUi()
    {
        StepType type = actInfo.SupplyStep();
        //默认不显示补领补给按钮
        UI.Get<Button>("Btn_Replacement").gameObject.SetActive(false);
        switch (type)
        {
            case StepType.WaitMid:
                UI.Get<Button>("Btn_claim").gameObject.SetActive(false);
                UI.Get<Text>("Text_supplynote").text = Lang.Get("指挥官，您的午间补给箱正在运送途中，请耐心等待");
                break;
            case StepType.InMidday:
                UI.Get<Button>("Btn_claim").gameObject.SetActive(true);
                if (actInfo.data2017.first == 0)
                {
                    UI.Get<Text>("Text_claim").text = Lang.Get("领取补给");
                    UI.Get<Button>("Btn_claim").interactable = true;
                }
                else
                {
                    UI.Get<Text>("Text_claim").text = Lang.Get("已领取");
                    UI.Get<Button>("Btn_claim").interactable = false;
                }
                UI.Get<Text>("Text_supplynote").text = Lang.Get("指挥官，您的补给箱已经到了，快来进行补给吧");
                break;
            case StepType.WaitEvening://15-18点可补领午间补给

                UI.Get<Text>("Text_supplynote").text = Lang.Get("指挥官，您的晚间补给箱正在运送途中，请耐心等待");


                if (actInfo.data2017.three == 0 && actInfo.data2017.first == 0)
                {
                    UI.Get<Button>("Btn_claim").gameObject.SetActive(false);
                    UI.Get<Button>("Btn_Replacement").interactable = true;
                    UI.Get<Button>("Btn_Replacement").gameObject.SetActive(true);
                }
                else
                {
                    UI.Get<Button>("Btn_claim").gameObject.SetActive(true);
                    UI.Get<Text>("Text_claim").text = Lang.Get("已领取");
                    UI.Get<Button>("Btn_claim").interactable = false;
                }
                break;
            case StepType.InEvening:
                UI.Get<Button>("Btn_claim").gameObject.SetActive(true);
                if (actInfo.data2017.second == 0)
                {
                    UI.Get<Text>("Text_claim").text = Lang.Get("领取补给");
                    UI.Get<Button>("Btn_claim").interactable = true;
                }
                else
                {
                    UI.Get<Text>("Text_claim").text = Lang.Get("已领取");
                    UI.Get<Button>("Btn_claim").interactable = false;
                }
                UI.Get<Text>("Text_supplynote").text = Lang.Get("指挥官，您的补给箱已经到了，快来进行补给吧");
                break;
            case StepType.WaitMidAtNight://21-24点可补领晚间补给


                if (actInfo.data2017.four == 0 && actInfo.data2017.second == 0)
                {
                    UI.Get<Button>("Btn_Replacement").interactable = true;
                    UI.Get<Button>("Btn_Replacement").gameObject.SetActive(true);
                    UI.Get<Button>("Btn_claim").gameObject.SetActive(false);
                }
                else
                {
                    UI.Get<Button>("Btn_claim").gameObject.SetActive(true);
                    UI.Get<Text>("Text_claim").text = Lang.Get("已领取");
                    UI.Get<Button>("Btn_claim").interactable = false;
                }

                UI.Get<Text>("Text_supplynote").text = Lang.Get("指挥官，您的午间补给箱正在运送途中，请耐心等待");
                break;
        }

    }



    public override void OnShow()
    {

    }

    public override void OnClose()
    {
        base.OnClose();
    }
}
