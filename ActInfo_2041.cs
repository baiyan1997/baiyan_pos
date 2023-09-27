public class ActInfo_2041 : ActivityInfo
{
    
    private long start_ts;
    private long end_ts;

    public long startTS
    {
        get { return start_ts; }
    }

    public long endTS
    {
        get { return end_ts; }
    }

    public override void InitUnique()
    {
        JDDebug.Dump(_data,"ActInfo_2041.InitUnique");

        start_ts =long.Parse(_data.avalue["startts"].ToString());
        end_ts = long.Parse(_data.avalue["endts"].ToString());

    }

}
