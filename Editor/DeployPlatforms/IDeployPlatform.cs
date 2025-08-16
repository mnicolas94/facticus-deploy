using System;
using System.Collections.Generic;
using System.Linq;
using Deploy.Editor.Utility;
using UnityEngine;
using Utils.Attributes;

namespace Deploy.Editor.DeployPlatforms
{
    public interface IDeployPlatform
    {
        string GetPlatformName();
        string GetFieldsPrefix();
    }

    [Serializable]
    public class Telegram : IDeployPlatform
    {
        public string GetPlatformName() => "Telegram";
        public string GetFieldsPrefix() => "telegram";

        [SerializeField] private string message;

        public string Message
        {
            get => message;
            set => message = value;
        }
    }
    
    [Serializable]
    public class Steam : IDeployPlatform
    {
        public string GetPlatformName() => "Steam";
        public string GetFieldsPrefix() => "steam";

        [SerializeField] private string releaseBranch;
    }
    
    [Serializable]
    public class Itch : IDeployPlatform
    {
        public string GetPlatformName() => "Itch";
        public string GetFieldsPrefix() => "itch";

        [SerializeField] private string channel;
    }
    
    [Serializable]
    public class GithubRelease : IDeployPlatform
    {
        public string GetPlatformName() => "GithubRelease";
        public string GetFieldsPrefix() => "ghR";
    }
    
    [Serializable]
    public class PlayStore : IDeployPlatform, IJsonSerializable
    {
        public string GetPlatformName() => "PlayStore";
        public string GetFieldsPrefix() => "playstore";

        [SerializeField, Dropdown(nameof(GetTrackValues))]
        private string track;
        [SerializeField, Dropdown(nameof(GetStatusValues))]
        private string status;
        [SerializeField, Range(0, 5)] private int inAppUpdatePriority;
        [SerializeField, Range(0.0001f, 0.9999f)] private float userFraction = 0.5f;

        public string ToJson()
        {
            var dict = new Dictionary<string, string>
            {
                {"track", $"\"{track}\""},
                {"status", $"\"{status}\""},
                {"inAppUpdatePriority", inAppUpdatePriority.ToString()},
                {"userFraction", userFraction.ToString()},
            };
            
            if (status is not "inProgress" and not "halted" )
            {
                dict.Remove("userFraction");
            }
            
            var json = DictToJson(dict);
            return json;
        }

        private string DictToJson(Dictionary<string, string> dict)
        {
            var keyValuesEnumerable = dict.Select(d => $"\"{d.Key}\":{d.Value}");
            var keyValues = string.Join(",", keyValuesEnumerable);
            return $"{{{keyValues}}}";
        }

        private List<string> GetTrackValues()
        {
            return new List<string>
            {
                "production",
                "beta",
                "alpha",
                "internalsharing",
                "internal",
            };
        }
        
        private List<string> GetStatusValues()
        {
            return new List<string>
            {
                "completed",
                "inProgress",
                "halted",
                "draft",
            };
        }
    }
}