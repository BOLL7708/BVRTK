using System.Globalization;
using System.Reflection;

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

    public static string[] GetSupportedLanguageGuiTags()
    {
        List<string> tags = [];
        foreach (var entry in Constants.SupportedLanguages)
        {
            var value = entry.Value.Equals(CultureInfo.InvariantCulture)
                ? new CultureInfo(Constants.SystemDefaultLanguage)
                : entry.Value;
            tags.Add($"{value.NativeName}##{entry.Key}");
        }

        return [.. tags];
    }
}