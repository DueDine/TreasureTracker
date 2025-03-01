using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TreasureTracker;

#pragma warning disable IDE1006 // Naming Styles
public sealed partial class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    public Configuration Configuration { get; init; }
    private const string CommandName = "/ttracker";

    public readonly WindowSystem WindowSystem = new("TreasureTracker");

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        PluginInterface.UiBuilder.Draw += DrawUI;
        ChatGui.ChatMessage += OnMessage;
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = """
                          展示用法
                          /ttracker output -> 输出记录
                          /ttracker clear -> 清空记录
                          """
        });
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ChatGui.ChatMessage -= OnMessage;
        CommandManager.RemoveHandler(CommandName);
    }

    private void DrawUI() => WindowSystem.Draw();

    private void OnCommand(string command, string args)
    {
        if (command != CommandName) return;

        switch (args)
        {
            case "clear":
                ChatGui.Print("已清空");
                Configuration.Clear();
                break;
            case "output":
                ChatGui.Print(Configuration.Record.Output());
                break;
            default:
                ChatGui.Print($"Usage: {CommandName} [clear|output]");
                break;
        }
    }

    private void OnMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!ChatTypes.Contains((int)type)) return;

        if ((int)type == 2105)
        {
            if (message.ToString().Contains("将陷阱引来的魔物全部打倒了"))
            {
                if (Configuration.Record.StartTime == DateTime.MinValue)
                {
                    Configuration.Record.StartTime = DateTime.Now;
                }
                Configuration.Record.MapTimes++;
            }
            else if (message.ToString().Contains("通往宝物库的传送魔纹出现了"))
            {
                Configuration.Record.PortalTimes++;
            }
            else if (message.ToString().Contains("最终区的封锁解除了"))
            {
                Configuration.Record.FinalLevelTimes++;
            }
        }
        else if ((int)type == 62)
        {
            var match = GilRegex().Match(message.ToString());
            if (match.Success)
            {
                Configuration.Record.GilObtained += int.Parse(match.Groups["gold"].Value.Replace(",", ""));
            }
        }
        else if (type == XivChatType.SystemMessage)
        {
            var match = ItemRegex().Match(message.ToString());
            if (match.Success)
            {
                var count = match.Groups["count"].Value;
                if (string.IsNullOrEmpty(count)) count = "1";
                Configuration.Record.AddItem(match.Groups["item"].Value, int.Parse(count));
            }
        }
        Configuration.Save();
    }

    private readonly HashSet<int> ChatTypes = [
        (int)XivChatType.SystemMessage,
        2105, // 将陷阱引来的魔物全部打倒了 ...
        2110, // 自己获得物品
        62, // 自己获得金币
    ];

    [GeneratedRegex("获得了(?<gold>[\\d,]+)金币。")]
    private static partial Regex GilRegex();

    [GeneratedRegex("获得了新的战利品.(?<item>.+?)×?(?<count>\\d{1,4})?。")]
    private static partial Regex ItemRegex();
}
