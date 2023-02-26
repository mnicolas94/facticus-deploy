using System;

namespace Deploy.Editor.NotifyPlatforms
{
    public interface INotifyPlatform
    {
        string PlatformName => default;
    }

    [Serializable]
    [AddTypeMenu("Telegram")]
    public class TelegramNotifyPlatform : INotifyPlatform
    {
        public string PlatformName => "Telegram";
    }
}