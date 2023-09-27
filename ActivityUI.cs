public abstract class ActivityUI : JDBehaviour
{
    public abstract void OnCreate();

    public abstract void OnShow();

    private bool IsInit = false;

    public virtual void OnClose()
    {
        if (IsInit == true)
        {
            UnInitListener();
        }
    }

    public virtual void InitListener()
    {
        IsInit = true;
        EventCenter.Instance.UpdateActivityUI.AddListener(UpdateUI);
        TimeManager.Instance.TimePassSecond += UpdateTime;
    }

    public virtual void UnInitListener()
    {
        EventCenter.Instance.UpdateActivityUI.RemoveListener(UpdateUI);
        TimeManager.Instance.TimePassSecond -= UpdateTime;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsInit == true)
        {
            UnInitListener();
        }
    }

    public virtual void SetAid(int aid)
    {
    }

    public virtual void UpdateTime(long time)
    {

    }

    public virtual void UpdateUI(int aid)
    {

    }
}
