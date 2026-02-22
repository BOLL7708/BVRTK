namespace BVRTK.Utils;

#region Result Structs

public record struct WriteTextResult(
    bool Success = false,
    int CharsWritten = 0,
    string FileName = "",
    string FilePath = "",
    string Message = ""
);

public record struct WriteBinaryResult(
    bool Success = false,
    long BytesWritten = 0,
    string FileName = "",
    string FilePath = "",
    string Message = ""
);

public record struct ReadTextResult(
    bool Success = false,
    int CharsRead = 0,
    string FileName = "",
    string FilePath = "",
    string Message = "",
    string Text = ""
);

public record struct ReadBinaryResult(
    bool Success = false,
    long BytesRead = 0,
    string FileName = "",
    string FilePath = "",
    string Message = "",
    uint[]? Bytes = null
);

public record struct DeleteResult(
    bool Success = false,
    string FileName = "",
    string FilePath = "",
    string Message = ""
);

public record struct EnsureDirectoryResult(
    bool Success = false,
    string Path = "",
    string Message = ""
);

public record struct FileExistsResult(
    bool Success = false,
    bool Exists = false,
    string FileName = "",
    string FilePath = "",
    string Message = ""
);

#endregion

public static class FileUtils
{
    #region Text

    public static WriteTextResult WriteText(string filePath, string contents)
    {
        var fileName = Path.GetFileName(filePath);
        try
        {
            var dirRes = EnsureDirectoryExists(filePath);
            if (!dirRes.Success) return new WriteTextResult(false, 0, dirRes.Message);
            File.WriteAllText(filePath, contents);
            return new WriteTextResult(true, contents.Length, fileName, filePath);
        }
        catch (Exception ex)
        {
            return new WriteTextResult(false, 0, fileName, filePath, ex.Message);
        }
    }

    public static ReadTextResult ReadText(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        try
        {
            var fileRes = FileExists(filePath);
            if (!fileRes.Success) return new ReadTextResult(false, 0, fileName, filePath, fileRes.Message);
            var text = File.ReadAllText(filePath);
            return new ReadTextResult(true, text.Length, fileName, filePath, "", text);
        }
        catch (Exception ex)
        {
            return new ReadTextResult(false, 0, fileName, filePath, ex.Message);
        }
    }

    #endregion

    #region Binary

    public static WriteBinaryResult WriteBinary(string filePath, uint[] data)
    {
        // TODO: Implement
        // Make sure the folder structure to the file exists
        // Write binary to the file, overwrite if it exists.
        // Return true if the writing was successful.
        return new WriteBinaryResult(false);
    }

    public static ReadBinaryResult ReadBinary(string filePath)
    {
        // TODO: Implement
        return new ReadBinaryResult(false);
    }

    #endregion

    #region Generic

    public static DeleteResult Delete(string filePath)
    {
        // TODO: Implement
        // Check so folder path to file exists
        // Try to delete file if it exists, return true if the file does not exist.
        return new DeleteResult(false);
    }

    #endregion

    #region Auxilliary

    private static FileExistsResult FileExists(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        try
        {
            var exists = File.Exists(filePath);
            return new FileExistsResult(true, exists, fileName, filePath);
        }
        catch (Exception ex)
        {
            return new FileExistsResult(false, false, ex.Message);
        }
    }

    private static EnsureDirectoryResult EnsureDirectoryExists(string filePath)
    {
        var path = Path.GetDirectoryName(filePath);
        try
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return new EnsureDirectoryResult(true, path ?? "");
        }
        catch (Exception ex)
        {
            return new EnsureDirectoryResult(false, path ?? "", ex.Message);
        }
    }

    #endregion
}