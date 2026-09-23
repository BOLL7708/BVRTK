using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Resources;
using EasyOpenVR.Data;
using EasyOpenVR.Data.Manifest;

namespace BVRTK;

public partial class Utils
{
    public static byte[] LoadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var resourceStream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var memoryStream = new MemoryStream();
        resourceStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    #region Translations

    /// Used to provide values for the GUI
    public static Dictionary<string, string> GetSupportedLanguageGuiIdPairs()
    {
        var values = new Dictionary<string, string>();
        foreach (var entry in Constants.SupportedLanguages)
        {
            var value = entry.Value.Equals(CultureInfo.InvariantCulture)
                ? new CultureInfo(Constants.SystemDefaultLanguage)
                : entry.Value;
            values.Add(entry.Key, value.NativeName);
        }
        return values;
    }

    public static string[] GetSupportedLanguageGuiIds()
    {
        var ids = new List<string>();
        var pairs = GetSupportedLanguageGuiIdPairs();
        foreach (var pair in pairs)
        {
            ids.Add($"{pair.Value}##{pair.Key}");
        }

        return [.. ids];
    }

    // Used to provide values for the action manifest builder
    public static ActionBuilder AddLocalizationsToAction(ActionBuilder actionBuilder, ResourceManager resourceManager, string promptName, ResourceManager? prefixResourceManager = null, string? promptPrefixName = null)
    {
        foreach (var language in Constants.SupportedLanguages)
        {
            var prompt = resourceManager.GetString(promptName, language.Value);
            var code = SharedUtils.FixLanguageTag(language.Key, "");
            if (string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(code)) continue;

            var prefix = prefixResourceManager?.GetString(promptPrefixName ?? "", language.Value);
            actionBuilder.AddLocalization(code, !string.IsNullOrEmpty(prefix) ? $"{prefix} {prompt}" : prompt);
        }

        return actionBuilder;
    }

    public static Func<string> GetPromptWithPrefixFunc(ResourceManager resourceManager, string promptName, ResourceManager? prefixResourceManager = null, string? promptPrefixName = null)
    {
        return () =>
        {
            var prompt = resourceManager.GetString(promptName);
            var prefix = prefixResourceManager?.GetString(promptPrefixName ?? "");
            return !string.IsNullOrEmpty(prefix) ? $"{prefix} {prompt}" : prompt ?? "";
        };
    }

    #endregion
}