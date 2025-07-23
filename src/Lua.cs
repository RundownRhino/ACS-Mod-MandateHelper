using System.Linq;
using UnityEngine;
using XiaWorld;

namespace MandateHelper;
using static Utils;

// Make methods exposed to Lua:
public class Lua {
    public static void MandateHookFor(string law, NpcLuaHelper me) {
        var thisMatch = me.GetMatchingWithGong(law);
        AddStoryMsg($"[{nameof(MandateHelper)}] {me.npcObj.GetName()}'s Law Match is {Mathf.FloorToInt(thisMatch * 100f)}%.");
        if (thisMatch >= 1) {
            return;
        }
        var lawDef = PracticeMgr.Instance.GetGongDef(law);
        var npcs = WorldMgr.Instance.curWorld.map.Things.GetPlayerActiveNpcs();
        var others = npcs.Where(x => x != me.npcObj).ToDictionary(npc => npc, npc => NpcPractice.GetFiveBaseEfficiency(lawDef, npc)).OrderByDescending(x => x.Value);
        if (others?.Count() == 0) {
            return;
        }
        var MAX = 5;
        AddStoryMsg($"Here's {(others.Count() > MAX ? $"the {MAX} best" : "all the")} law matches for other disciples:");
        foreach (var other in others.Take(MAX)) {
            var npc = other.Key;
            var matchPart = $"{Mathf.FloorToInt(other.Value * 100)}%";
            if (other.Value >= 1) {
                matchPart = $"[color=#0000FF]{matchPart}[/color]";
            }
            // This looks a bit ugly but the game's font is not monospace so I can't format it better.
            AddStoryMsg($"{npc.GetName()} ({(npc.PropertyMgr.Practice.Gong is null ? "Outer" : "Inner")}): {matchPart}");
        }
        AddStoryMsg("");

    }
}