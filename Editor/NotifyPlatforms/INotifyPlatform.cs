using System;

namespace Deploy.Editor.NotifyPlatforms
{
    public interface INotifyPlatform
    {
        string GetPlatformName();
    }

    [Serializable]
    [AddTypeMenu("Telegram")]
    public class TelegramNotifyPlatform : INotifyPlatform
    {
        public string GetPlatformName() => "Telegram";
    }
}