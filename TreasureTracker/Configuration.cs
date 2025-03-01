using Dalamud.Configuration;
using System;
using TreasureTracker.Data;

namespace TreasureTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public Record Record = new();

    public void Clear()
    {
        Record = new();
        Save();
    }

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
