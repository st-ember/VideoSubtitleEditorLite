
using SubtitleEditor.ActivationKeyGenerator.Infrastructure;

Console.WriteLine("Welcome to Video Subtitle Editor (VSE) activation key generator!");

uint targetAction = 0;

while (true)
{
    Console.WriteLine("");
    Console.WriteLine("可執行的操作：");
    Console.WriteLine("[1] 產生新的授權金鑰");
    Console.WriteLine("[2] 解析現有授權金鑰的內容");
    Console.WriteLine("請輸入欲執行的數字代碼：");

    var targetActionText = Console.ReadLine();
    if (uint.TryParse(targetActionText, out targetAction) && (targetAction == 1 || targetAction == 2))
    {
        break;
    }
    else
    {
        Console.Clear();
        Console.WriteLine("");
        Console.WriteLine("無效的數字代碼，請再重試一次。");
    }
}

if (targetAction == 1)
{
    Console.Clear();
    Console.WriteLine("");
    Console.WriteLine("操作: 產生新的授權金鑰");
    Console.WriteLine("請提供必要的資訊來建立新的授權金鑰。");

    string publisher;
    string target;
    bool asrAccess;
    DateTime? due = null;
    uint calCount = 0;

    while (true)
    {
        Console.WriteLine("");
        Console.WriteLine("發行者：");
        Console.WriteLine("(發行此授權的公司或個人名稱)");

        var publisherText = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(publisherText))
        {
            publisher = publisherText.Trim();
            break;
        }
        else
        {
            Console.WriteLine("");
            Console.WriteLine("錯誤：發行者不可空白");
        }
    }

    while (true)
    {
        Console.WriteLine("");
        Console.WriteLine("被授權者：");
        Console.WriteLine("(要收受此授權的公司或個人名稱)");

        var targetText = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(targetText))
        {
            target = targetText.Trim();
            break;
        }
        else
        {
            Console.WriteLine("");
            Console.WriteLine("錯誤：被授權者不可空白");
        }
    }

    while (true)
    {
        Console.WriteLine("");
        Console.WriteLine("版本別：");
        Console.WriteLine("設定ASR版本請按[1]，設定限制權限版本請按[2]");

        var asrAccessText = Console.ReadLine();
        uint asrAccessOutText;

        if (uint.TryParse(asrAccessText, out asrAccessOutText) && (asrAccessOutText == 1 || asrAccessOutText == 2))
        {
            if (asrAccessOutText == 1)
            {
                asrAccess = true;
                break;
            } else if (asrAccessOutText == 2)
            {
                asrAccess = false;
                break;
            }
        }
        else
        {
            Console.WriteLine("");
            Console.WriteLine("無效的數字代碼，請再重試一次。");
        }
    }

    while (true)
    {
        Console.WriteLine("");
        Console.WriteLine("有效期限：");
        Console.WriteLine("(不設定有效期限時可空白，輸入格式為 yyyy-MM-dd)");

        var dueDateText = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(dueDateText))
        {
            if (DateTime.TryParse(dueDateText, out var d))
            {
                due = d;
                break;
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine($"錯誤：有效期限的格式錯誤，請以 yyyy-MM-dd 的格式提供，例如 {DateTime.Today:yyyy-MM-dd}");
            }
        }
        else
        {
            break;
        }
    }

    while (true)
    {
        Console.WriteLine("");
        Console.WriteLine("CAL 數量：");
        Console.WriteLine("(Client Access License 的數量，不提供或輸入 0 代表不限制)");

        var calCountText = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(calCountText))
        {
            if (uint.TryParse(calCountText, out var c))
            {
                calCount = c;
                break;
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine($"錯誤：CAL 的格式錯誤，請提供大於 0 的數字。");
            }
        }
        else
        {
            break;
        }
    }

    try
    {
        var activationData = ActivationService.Generate(publisher, target, asrAccess, due, calCount);
        var key = ActivationService.GenerateKey(activationData);

        Console.Clear();
        Console.WriteLine("授權金鑰已成功產生：");
        Console.WriteLine(key);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }

    Console.ReadLine();
}
else if (targetAction == 2)
{
    Console.Clear();
    Console.WriteLine("");
    Console.WriteLine("請提供欲解析的金鑰：");

    var keyText = Console.ReadLine();

    try
    {
        var activationData = ActivationService.ResolveKey(keyText);
        if (activationData != null)
        {
            Console.WriteLine("");
            Console.WriteLine("已完成解析，此金鑰的資訊如下：");
            Console.WriteLine($"ID:        {activationData.Id}");
            Console.WriteLine($"Version:   {activationData.Version}");
            Console.WriteLine($"Publisher: {activationData.Publisher}");
            Console.WriteLine($"Target:    {activationData.Target}");
            Console.WriteLine($"Editions:  {(activationData.Editions != null ? string.Join(", ", activationData.Editions) : "-")}");
            Console.WriteLine($"AsrAccess: {(activationData.AsrAccess == false && activationData.Version == 1 ? true : activationData.AsrAccess)}"); 
            // 若為V1，ASR權限回傳true。之後版本以金鑰資訊回傳。
            Console.WriteLine($"Date:      {activationData.PublishDate:yyyy-MM-dd}");
            Console.WriteLine($"DueDate:   {(activationData.DueDate.HasValue ? activationData.DueDate.Value.ToString("yyyy-MM-dd") : "無限制")}");
            Console.WriteLine($"CalCount:  {(activationData.CalCount > 0 ? activationData.CalCount.ToString() : "無限制")}");
            Console.WriteLine($"Meta:      {activationData.Meta ?? "-"}");
        }
        else
        {
            Console.WriteLine("");
            Console.WriteLine("提供的金鑰無效。");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }

    Console.ReadLine();
}
else
{
    Console.WriteLine("無效的數字代碼。");
    Console.ReadLine();
}