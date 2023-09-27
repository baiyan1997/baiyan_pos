using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class _Activity_2008_UI : ActivityUI
{
    private int AID = 2008;
    private ObjectGroup UI;

    private ActInfo_2008 actInfo;

    private ObjectGroup _listItem0;
    private List<ObjectGroup> _rewardItems;

    private Button _btnBuy;
    //客服端计算获取总计氪金数
    private int total;
    private Text _des;
    private Text _afterSafe;
    private Text _desc;

    public override void Awake()
    {
    }

    public override void OnCreate()
    {
        UI = gameObject.GetComponent<ObjectGroup>();
        //InitListener();
        InitData();
        _desc = transform.Find<JDText>("TextDesc");
        _listItem0 = UI.Get<ObjectGroup>("List_01");
        _listItem0.gameObject.SetActive(false);
        _des = UI.Get<Text>("des_text");
        _afterSafe = UI.Get<Text>("afterSafe_text");
        _rewardItems = new List<ObjectGroup>();
        //首次打开刷新一下
        _btnBuy = UI.Get<Button>("ButtonBuy");
        _btnBuy.onClick.AddListener(On_btnBuyClick);
        ActivityManager.Instance.RequestUpdateActivityById(AID);
        DesText();
    }
    private void On_btnBuyClick()
    {
        actInfo.ActiveFund2008(UpdateUI2008);
    }

    private void DesText()
    {
        foreach (var reward in actInfo.cfg_data)
        {
            Act2008_rewardData lvData = reward.Value;
            string rewards = lvData.reward;
            int num = int.Parse(rewards.Split('|')[1]);
            total += num;
        }
        string str = "购买";
        _des.text = string.Format(Lang.Get("<Color=#ffcc00ff>1000氪晶</Color>{1}成长基金,总计可获得<Color=#ffcc00ff>{0}氪晶</Color>"), total, str);
        _afterSafe.text = string.Format(Lang.Get("努力升级指挥官，总计可获得<Color=#ffcc00ff>{0}氪晶</Color>"), total);
    }

    public override void OnShow()
    {
        UpdateUI2008();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    void InitData()
    {
        actInfo = new ActInfo_2008();
        actInfo = (ActInfo_2008)ActivityManager.Instance.GetActivityInfo(AID);
    }

    public override void InitListener()
    {
        base.InitListener();
    }

    public override void UpdateUI(int aid)
    {
        base.UpdateUI(aid);
        if (aid == AID) UpdateUI2008();
    }

    void UpdateUI2008()
    {
        _desc.text = actInfo._desc;
        _btnBuy.gameObject.SetActive(actInfo.open == 0);
        _des.gameObject.SetActive(actInfo.open == 0);
        _afterSafe.gameObject.SetActive(actInfo.open != 0);

        Dictionary<int, int> rewardlist = actInfo.reward_list;
        for (int i = 0; i < _rewardItems.Count; i++)
        {
            UnityEngine.Object.Destroy(_rewardItems[i].gameObject);
        }
        _rewardItems.Clear();

        Dictionary<int, Act2008_rewardData> Done = new Dictionary<int, Act2008_rewardData>();
        Dictionary<int, Act2008_rewardData> Doing = new Dictionary<int, Act2008_rewardData>();


        foreach (KeyValuePair<int, Act2008_rewardData> kp in actInfo.cfg_data)
        {
            int rewardId = kp.Value.id;
            int tempV = 0;
            rewardlist.TryGetValue(rewardId, out tempV);
            //  if (rewardlist.ContainsKey(rewardId) && rewardlist[rewardId] == 1) //已完成
            if (tempV == 1) //已完成
            {
                Done.Add(kp.Key, kp.Value);
            }
            else //可完成
            {
                Doing.Add(kp.Key, kp.Value);
            }
        }

        PutInItems(Doing, rewardlist);
        PutInItems(Done, rewardlist);
    }

    void PutInItems(Dictionary<int, Act2008_rewardData> data, Dictionary<int, int> rewardlist)
    {
        int playerLv = Uinfo.Instance.Player.Info.ulevel;
        Transform itemRoot = UI.Get<Transform>("Content_ListRoot");
        foreach (KeyValuePair<int, Act2008_rewardData> kp in data)
        {
            Act2008_rewardData lvData = kp.Value;

            int id = lvData.id;
            int lv = lvData.need_level;
            string reward = lvData.reward;

            GameObject obj = UnityEngine.Object.Instantiate(_listItem0.gameObject) as GameObject;
            ObjectGroup newItem = obj.GetComponent<ObjectGroup>();
            _rewardItems.Add(newItem);
            obj.SetActive(true);
            obj.transform.SetParent(itemRoot, false);

            newItem.Get<Text>("TextRewardTitle").text = String.Format(Lang.Get("指挥官等级达到{0}级"), lv);
            newItem.Get<Text>("TextRewardNum").text = String.Format("{0}/{1}", playerLv, lv);

            var rewardStr = "";
            if (reward.Length != 0)
            {
                rewardStr = Lang.Get("{0} x{1}", Cfg.Item.GetItemName(int.Parse(reward.Split('|')[0])), reward.Split('|')[1]);
            }
            newItem.Get<Text>("TextRewardItem").text = rewardStr;
            //-- newItem.IconReward:Image().sprite = CS.Cfg.Item.GetItemIcon(10)[1]--string.split(reward, "|")[1])

            Button ButtonReward = newItem.Get<Button>("ButtonReward");
            Button ButtonRewardFinish = newItem.Get<Button>("ButtonRewardFinish");
            Button ButtonRewardDoing = newItem.Get<Button>("ButtonRewardDoing");
            ButtonReward.gameObject.SetActive(false);
            ButtonRewardFinish.gameObject.SetActive(false);
            ButtonRewardDoing.gameObject.SetActive(false);


            int rewardId = id;

            ButtonReward.name = "btnreward" + rewardId;

            newItem.Get<Transform>("FreeBg").gameObject.SetActive(true);
            newItem.Get<Transform>("BusyBg").gameObject.SetActive(false);
            var hasGetReward = false;
            var rewardValue = 0;
            if (rewardlist.TryGetValue(rewardId, out rewardValue))
            {
                hasGetReward = rewardValue == 1;
            }
            if (actInfo.open == 0)
            {
            }
            else if (playerLv < lv)
            {
                ButtonRewardDoing.gameObject.SetActive(true);
            }
            else if (hasGetReward)
            {
                ButtonRewardFinish.gameObject.SetActive(true);
            }
            else
            {
                ButtonReward.gameObject.SetActive(true);
                ButtonReward.onClick.AddListener(() =>
                {
                    //actInfo.GetReward2008(rewardId, () =>
                    //{
                    //    UpdateUI2008();
                    //});
                    actInfo.GetReward2008(rewardId, UpdateUI2008);
                });
                newItem.Get<Transform>("FreeBg").gameObject.SetActive(false);
                newItem.Get<Transform>("BusyBg").gameObject.SetActive(true);
            }
        }
    }
}
