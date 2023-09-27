using UnityEngine;
using UnityEngine.UI;

public class _Activity_2084_UI : ActivityUI
{
    private Text _descText;
    private Button _getBtn;
    private GameObject[] _rewardGo;
    private GameObject _getGo;
    private const int _aid = 2084;
    private ActInfo_2084 _actInfo;

    public override void OnCreate()
    {
        _descText = transform.Find<JDText>("Desc/Text");
        _getBtn = transform.Find<Button>("BtnGet");
        _getGo = transform.Find<GameObject>("GetGo");
        _rewardGo = new GameObject[]
        {
            transform.Find<GameObject>("IconList/01"),
            transform.Find<GameObject>("IconList/02"),
            transform.Find<GameObject>("IconList/03"),
        };
        _getBtn.onClick.SetListener(On_getBtnClick);
    }
    private void On_getBtnClick()
    {
        AudioManager.Instace.PlaySoundOfNormalBtn();

        if (_actInfo == null)
            return;

        _actInfo.RequestRewards(SetBtnState);
    }
    public override void InitListener()
    {
        base.InitListener();
    }

    public override void OnShow()
    {
        _actInfo = (ActInfo_2084)ActivityManager.Instance.GetActivityInfo(_aid);

        if (_actInfo == null)
            return;

        _descText.text = string.Format(Cfg.Act.GetData(_aid).act_desc, _actInfo.Lv, _actInfo.Buff);

        for (int i = 0; i < _rewardGo.Length; i++)
        {
            if (i < _actInfo.Rewards.Length)
            {
                _rewardGo[i].SetActive(true);

                DefineReward(_rewardGo[i], _actInfo.Rewards[i]);
            }
            else
            {
                _rewardGo[i].SetActive(false);
            }
        }

        SetBtnState();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (_aid != aid)
            return;

        if (gameObject.activeSelf)
        {
            OnShow();
        }
    }

    private void SetBtnState()
    {
        if (_actInfo.IsGet)
        {
            _getGo.SetActive(true);
            _getBtn.gameObject.SetActive(false);
        }
        else
        {
            _getGo.SetActive(false);
            _getBtn.gameObject.SetActive(true);
        }
    }

    private void DefineReward(GameObject go, P_Item data)
    {
        Image icon = go.transform.Find<Image>("Icon");
        Image qua = go.transform.Find<Image>("Qua");
        Text count = go.transform.Find<Text>("Text");
        Button btn = go.GetComponent<Button>();
        ItemForShow itemForShow = new ItemForShow(data.id, data.count);
        itemForShow.SetIcon(icon);
        count.text = "x" + GLobal.NumFormat(itemForShow.GetCount());
        qua.color = _ColorConfig.GetQuaColor(itemForShow.GetQua());
        btn.onClick.SetListener(() =>
        {
            DialogManager.ShowAsyn<_D_ItemTip>(d => { d?.OnShow(data.id, data.count, go.transform.position); });
        });
    }
}
