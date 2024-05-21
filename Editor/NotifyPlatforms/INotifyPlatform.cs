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
    
    [Serializable]
    [AddTypeMenu("Discord")]
    public class DiscordNotifyPlatform : INotifyPlatform
    {
        public string GetPlatformName() => "Discord";
    }
}