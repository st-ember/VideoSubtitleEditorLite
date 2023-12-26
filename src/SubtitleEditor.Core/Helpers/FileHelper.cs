namespace SubtitleEditor.Core.Helpers;

/// <summary>
/// 檔案狀態 Helper
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 確認檔案是否可以成功讀取。如果檔案被其他程式鎖定，會每隔 500ms 持續嘗試 30 次。
    /// </summary>
    /// <param name="filePath">檔案絕對路徑。</param>
    /// <returns>如果可以成功讀取則回傳 True。</returns>
    public static async Task<bool> IsFileSuccessLoadAsync(string filePath)
    {
        var file = new FileInfo(filePath);
        var count = 0;
        var success = true;
        while (file.IsFileLocked())
        {
            if (count <= 30)
            {
                count++;
                await Task.Delay(500);
            }
            else
            {
                success = false;
                break;
            }
        }
        return success;
    }

    /// <summary>
    /// 確認檔案是否正被其他應用程式鎖定。
    /// </summary>
    /// <param name="file"></param>
    public static bool IsFileLocked(this FileInfo file)
    {
        FileStream? stream = null;
        try
        {
            stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
        }
        catch (IOException)
        {
            return true;
        }
        finally
        {
            stream?.Dispose();
        }

        return false;
    }
}