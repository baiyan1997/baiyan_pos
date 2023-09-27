using System;
using System.Collections.Generic;
using UnityEngine;

public class ActInfo_2040 : ActivityInfo
{

    private Dictionary<int, List<int>> _areaToPlanetList;

    public override void InitUnique()
    {
        base.InitUnique();
        _areaToPlanetList = new Dictionary<int, List<int>>();
        RefreshPlanetList();
    }

    private void RefreshPlanetList()
    {
        foreach (KeyValuePair<int, List<int>> _planets in _areaToPlanetList)
        {
            _planets.Value.Clear();
        }
        string[] planetIds = _data.avalue["protect_castle"].ToString().Split(',');
        for (int i = 0; i < planetIds.Length; i++)
        {
            string planetid_str = planetIds[i];
            if (string.IsNullOrEmpty(planetid_str))
                continue;
            int planetID;
            if (!Int32.TryParse(planetid_str, out planetID))
                continue;

            Debug.Log(planetid_str + Cfg.CastleName.GetPlanetNameSimple(planetID));
            var areaID = WorldPositionCul.GetAreaIndex(planetID);
            List<int> temp = null;
            if (!_areaToPlanetList.TryGetValue(areaID, out temp))
            {
                temp = new List<int>();
                _areaToPlanetList.Add(areaID, temp);
            }
            // _areaToPlanetList[areaID].Add(planetID);
            temp.Add(planetID);
        }
        JDDebug.Dump(_areaToPlanetList, "Activity2040 : _areaToPlanetList");
    }

    private int paramAreaID;
    public bool CheckPlanetIDinAction(int id)
    {
        paramAreaID = WorldPositionCul.GetAreaIndex(id);
        List<int> panelList = null;
        if (_areaToPlanetList.TryGetValue(paramAreaID, out panelList))
        {
            return panelList.Contains(id);
        }
        else return false;
    }

    public bool CheckMineInAction(int minePlanetID)
    {
        paramAreaID = WorldPositionCul.GetAreaIndex(minePlanetID);
        List<int> temp = null;
        if (_areaToPlanetList.TryGetValue(paramAreaID, out temp))
        {
            for (int i = 0; i < temp.Count; i++)
            {
                int planetID = temp[i];
                var vec0 = Vector2Extension.VectorForPlanetID(minePlanetID);
                var vec1 = Vector2Extension.VectorForPlanetID(planetID);
                if (vec1.x - vec0.x <= 9 && vec0.x - vec1.x <= 10 && vec1.y - vec0.y <= 9 && vec0.y - vec1.y <= 10)
                {
                    return true;
                }
            }
        }
        return false;
    }
}