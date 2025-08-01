using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using XiaWorld;

namespace MandateHelper;
using static Utils;

public class MandateHelper {
    public static void OnInit() {
        PatchStories();
    }
    private static void Info(string fmt, params object[] args) {
        KLog.Info($"[{nameof(MandateHelper)}] {fmt}", args);
    }
    private static void Warn(string fmt, params object[] args) {
        KLog.Warn($"[{nameof(MandateHelper)}] {fmt}", args);
    }

    static readonly Regex ManifestingMandateRegex = new(@"^Secrets_Gong\d+");
    static readonly Regex LawMatchRegex = new(@"GetMatchingWithGong\(['""](.+?)['""]\)");
    public static void PatchStories() {
        var m_mapStories = Traverse.Create(typeof(MapStoryMgr)).Field("m_mapStories").GetValue<Dictionary<string, MapStoryDef>>();
        foreach (var kv in m_mapStories) {
            var sdef = kv.Value;
            var sname = sdef.Name;
            if (!ManifestingMandateRegex.IsMatch(sname)) {
                continue;
            }
            Info("Patching mandate story '{0}'", sname);
            var acceptChoice = sdef.Selections.Find(x => x.OKResult?.Contains("GetMatchingWithGong") ?? false);
            if (acceptChoice is null) {
                Warn("Found no 'accept teachings' choice for story '{0}'", sname);
                continue;
            }
            // Find what Law is this for:
            var lawMatch = LawMatchRegex.Match(acceptChoice.OKResult);
            if (!lawMatch.Success) {
                Warn("Couldn't parse what Law story '{0}' is matching with!", sname);
                continue;
            }
            var law = lawMatch.Groups[1].Value.Trim();
            // It's easier to inject my own function than to figure out whether this can be done from the API.
            AddCodeToStart(ref acceptChoice.OKResult, $"""
            CS.{nameof(MandateHelper)}.{nameof(Lua)}.{nameof(Lua.MandateHookFor)}("{law}", me);
            """);
        }
    }

}