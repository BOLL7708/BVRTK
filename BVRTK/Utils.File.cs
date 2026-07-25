namespace BVRTK;

public static partial class Utils
{
    /// <summary>
    /// Will build an absolute file path from the current directory with a system specific directory separator.
    /// </summary>
    /// <param name="relativeParts">An array of subfolders where the last value is the filename.</param>
    /// <returns></returns>
    public static string GetAbsoluteFilePath(string[] relativeParts)
    {
        var dsc = Path.DirectorySeparatorChar;
        var relativePath = string.Join(dsc, relativeParts);
        return $"{Directory.GetCurrentDirectory()}{dsc}{relativePath}";
    }
}