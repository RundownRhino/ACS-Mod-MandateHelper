using System;
using System.Linq;
using System.Text.RegularExpressions;
using XiaWorld;

namespace MandateHelper;
public static class Utils {
    public static string LongestCommonPrefix(string str1, string str2) {
        int commonLength = 0;
        while (commonLength < Math.Min(str1.Length, str2.Length) &&
              str1[commonLength] == str2[commonLength]) {
            commonLength++;
        }
        return str1.Substring(0, commonLength);
    }
    public static string CommonIndentation(string[] lines) {
        if (lines.Length == 0) {
            return "";
        }
        var prefix = lines.Aggregate(LongestCommonPrefix);
        return new string(prefix.TakeWhile(x => char.IsWhiteSpace(x)).ToArray());
    }
    public static Regex IndentRegex = new(@"^(\s*)");
    /// <summary>
    /// Appends/prepends code, correctly handling indentation.
    /// </summary>
    public static void AddCodeTo(ref string to, string code, bool end = true) {
        // Find indentation level of first/last nonempty line:
        var origLines = to.Split('\n').Where(x => x.Any(c => !char.IsWhiteSpace(c)));
        var nearestLine = end ? origLines.Last() : origLines.First();
        string nearestIndent = "";
        if (nearestLine is not null) {
            nearestIndent = IndentRegex.Match(nearestLine).Groups[1].Value;
        }
        // Strip equal indentation, readd target one:
        var codeClean = code.Trim('\n').Split('\n');
        var common = CommonIndentation(codeClean);
        var newLines = codeClean.Select(x => nearestIndent + x.Substring(common.Length));
        // Finally, add the new lines:
        if (end) {
            to = to + "\n" + "\n".Join(newLines);
        } else {
            to = "\n".Join(newLines) + "\n" + to;
        }
    }
    public static void AddCodeToStart(ref string to, string code) {
        AddCodeTo(ref to, code, end: false);
    }
    public static void AddStoryMsg(string format, params object[] args) {
        GameDefine.AddTempDescLine(GameDefine.GetFixedStory(null, string.Format(format, args)));
    }
}