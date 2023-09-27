using UnityEngine;
using UnityEngine.UI;

public class _Activity_2401_UI : ActivityUI
{
    private Text _countDown;
    private Text _goldCount;
    private RecycleView _rewardList;
    private ActInfo_2401 _actInfo;
    private int _aid = ActivityID.AccumulatedRecharge;
    private int currentSelectType = 0;
    private Button _helpBtn = null;
    public override void OnCreate()
    {
        _countDown = transform.Find<Text>("CountDown");
        _goldCount = transform.Find<Text>("GoldCount");
        _helpBtn = transform.Find<Button>("FQA_Btn");

        _rewardList = transform.Find("Scroll View").GetComponent<RecycleView>();
        _rewardList.Init(onListRenderReward);
        _helpBtn.onClick.AddListener(OnClickHelp);
    }
    public override void OnShow()
    {
        RefreshUI();
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void UnInitListener()
    {
        base.UnInitListener();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid != _aid)
            return;
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        RefreshUI();

    }
    private void RefreshUI()
    {
        _actInfo = (ActInfo_2401)ActivityManager.Instance.GetActivityInfo(_aid);
        _rewardList.ShowList(_actInfo.actData.Count);
        _goldCount.text = _actInfo.totalCharge;
    }

    public override void UpdateTime(long currentTime)
    {
        base.UpdateTime(currentTime);
        if (gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (!gameObject.activeSelf)
            return;

        if (_actInfo == null)
            return;

        if (_actInfo.LeftTime >= 0)
        {
            _countDown.text = GlobalUtils.ActivityLeftTime(_actInfo.LeftTime, true);
        }
        else
        {
            _countDown.text = Lang.Get("活动已经结束");
        }
    }

    private void OnClickHelp()
    {
        DialogManager.ShowAsyn<_D_Tips_HelpDesc>(ShowTipsHelp);
    }

    private void ShowTipsHelp(_D_Tips_HelpDesc d)
    {
        d?.OnShow(HelpType.Act2401, _helpBtn.transform.position, Direction.RightDown, 323);
    }
    protected void onListRenderReward(GameObject obj, int index)
    {
        var rewardData = _actInfo.actData[index];
        var dataCfg = rewardData.dataCfg;
        if (obj == null || dataCfg == null || rewardData == null)
        {
            return;
        }

        var btnGet = obj.transform.Find<Button>("Btn_get");
        var btnGoto = obj.transform.Find<Button>("Btn_goto");
        var btnGeted = obj.transform.Find<GameObject>("Btn_geted");
        var titleTxt = obj.transform.Find<JDText>("Title_Txt");

        titleTxt.text = string.Format(Lang.Get("累计充值 <Color=#73FF48>{0}氪晶</Color> 可领取"), dataCfg.mission_finish.Split("|")[0]);


        btnGet.onClick.RemoveAllListeners();
        btnGoto.onClick.RemoveAllListeners();
        btnGet.onClick.AddListener(() => {
            _actInfo.GetReward(dataCfg.id);
        });
        btnGoto.onClick.AddListener(() => {
            DialogManager.ShowAsyn<_D_Recharge>(d => { d?.OnShow(); });
        });
        
        btnGet.gameObject.SetActive(rewardData.order == 0);
        btnGoto.gameObject.SetActive(rewardData.order == 1);
        btnGeted.SetActive(rewardData.order == 2);


        var actItem = obj.GetComponent<_Act2401Item>();
        if(actItem == null) {
            actItem = obj.AddComponent<_Act2401Item>();
            actItem.OnInit(obj);
        }
        actItem.Refresh(dataCfg.reward);
    }    
}

public class _Act2401Item : MonoBehaviour
{
    public ListView itmeList;
    public void OnInit(GameObject obj)
    {
        itmeList = ListView.Create<_ActRewardItem>(obj.transform.Find("ScrollView"));
    }

    public void Refresh(string reward) {
        itmeList.Clear();
        if (reward == null) return;
        var rewardItems = GlobalUtils.ParseItem3(reward);
        var len = rewardItems.Length;
        for (int i = 0; i < len; i++)
        {
            itmeList.AddItem<_ActRewardItem>().Refresh(rewardItems[i]);
        }
    }

}