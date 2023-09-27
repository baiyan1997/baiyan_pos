using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ActInfo_2089 : ActivityInfo
{
    private static ActInfo_2089 _inst;


    public static ActInfo_2089 Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = (ActInfo_2089)ActivityManager.Instance.GetActivityInfo(2089);
            }
            return _inst;
        }
    }

    public P_Act2089Info Info { get; private set; }

    public override void InitUnique()
    {
        base.InitUnique();
        if (!_data.IsDuration())
            return;
        //重置单例指向
        _inst = (ActInfo_2089)ActivityManager.Instance.GetActivityInfo(2089);
        //获取活动信息
        Info = JsonMapper.ToObject<P_Act2089Info>(_data.avalue["data"].ToString());
    }
}

public class P_Act2089Info
{
    public List<P_Act2089StarFormInfo> form_info;//阵型信息 只在讨伐阶段会有
    public P_Act2089StepInfo step_info;//阶段信息
    public string ban_form;//当前轮次活动中被禁止的阵型类型 以逗号作为分隔符
    public int atk_times;//剩余讨伐次数
    public long kill_num;//击杀数量

    private List<int> _banList;
    public List<int> banList
    {
        get
        {
            if (string.IsNullOrEmpty(ban_form))
                return new List<int>();
            if (_banList == null)
            {
                _banList = ban_form.Split(',').Select(int.Parse).ToList();
            }
            return _banList;
        }
    }

    private int _form_center;
    public int form_center
    {
        get
        {
            if (_form_center > 0)
                return _form_center;
            for (int i = 0; i < form_info.Count; i++)
            {
                var info = form_info[i];
                if (_form_center == 0)
                    _form_center = info.form_center;
                else if (info.form_center > 0 && info.state == PlayerInfo.Instance.Info.ustate)
                {
                    _form_center = info.form_center;
                    return _form_center;
                }
            }
            return _form_center;
        }
    }

    private List<int> _formPathList = null;
    public List<int> GetFormPath()
    {
        if (_formPathList != null)
            return _formPathList;
        _formPathList = new List<int>();
        for (int i = 0; i < form_info.Count; i++)
        {
            var info = form_info[i];
            if (!string.IsNullOrEmpty(info.form_path))
            {
                _formPathList.AddRange(info.form_path.Split(',').Select(int.Parse).ToList());
            }
        }
        return _formPathList;
    }
}

public class P_Act2089StarFormInfo
{
    public int state;//国家
    public int form_type;//阵型类型
    public int form_center;//阵型中心的planet_id
    public string form_path;//阵型路径 以逗号分隔的planet_id
}

public class P_Act2089StepInfo
{
    public int step;
    public int start_ts;//阶段开始时间
    public int end_ts;//阶段结束时间
}

public class Act2089Step
{
    public static int STEP_CLOSE = 0;//关闭阶段
    public static int STEP_PREPARE = 1;//预告阶段
    public static int STEP_OCCUPY = 2;//占领阶段
    public static int STEP_CRUSADE = 3;//讨伐阶段
}