using System;
using System.Collections.Generic;
using System.Linq;

namespace TreasureTracker.Data;

public class Record
{
    public DateTime StartTime = DateTime.MinValue;
    public int MapTimes = 0;
    public int PortalTimes = 0;
    public int FinalLevelTimes = 0;
    public int GilObtained = 0;
    public Dictionary<string, int> ItemObtained = [];

    public string Output()
    {
        if (StartTime == DateTime.MinValue) return "未开始记录";
        return $"开始时间: {StartTime:yyyy-MM-dd HH:mm:ss}\n" +
               $"地图次数: {MapTimes}\n" +
               $"传送次数: {PortalTimes}\n" +
               $"最终层数: {FinalLevelTimes}\n" +
               $"金币: {GilObtained}\n" +
               $"物品: {string.Join(", ", ItemObtained.Select(item => $"{item.Key}: {item.Value}"))}";
    }

    public void AddItem(string itemName, int count)
    {
        if (ItemObtained.ContainsKey(itemName))
        {
            ItemObtained[itemName] += count;
        }
        else
        {
            ItemObtained[itemName] = count;
        }
    }
}
