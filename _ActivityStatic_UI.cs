
using UnityEngine.UI;

public class _ActivityStatic_UI : ActivityUI
{
    private int _aid;
    private ObjectGroup _ui;
    public override void OnCreate()
    {
        _ui = gameObject.GetComponent<ObjectGroup>();
        InitUi();
    }

    public override void SetAid(int aid)
    {
        _aid = aid;
    }

    public override void OnShow()
    {
        
    }

    private void InitUi()
    {
        _ui.Get<JDText>("Text_desc").text = Cfg.Act.GetData(_aid).act_desc;
    }
}
