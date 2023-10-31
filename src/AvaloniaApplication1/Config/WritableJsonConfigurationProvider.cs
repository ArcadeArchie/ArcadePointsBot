using AvaloniaApplication1.Auth;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Config
{
    internal class WritableJsonConfigurationProvider : JsonConfigurationProvider
    {
        public WritableJsonConfigurationProvider(JsonConfigurationSource source) : base(source) { }

        public override void Set(string key, string? value)
        {
            base.Set(key, value);
            if (Source.FileProvider == null) throw new InvalidOperationException("The FileProvider cannot be null");
            if (string.IsNullOrEmpty(Source.Path)) throw new InvalidOperationException("The Sourcepath cannot be null");
            var fullPath = Source.FileProvider.GetFileInfo(Source.Path).PhysicalPath;
            var json = File.ReadAllText(fullPath!);

            var config = JsonNode.Parse(json);
            if (config is null) throw new JsonException("Malformed app config");
            var configObj = config.AsObject();
            var keyParts = key.Split(':');
            if (keyParts.Length > 1)
            {
                configObj[keyParts[0]]![keyParts[1]] = value;
            }
            else
            {
                configObj[key] = value;
            }
            File.WriteAllText(fullPath!, configObj.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }

    class ConfigRoot
    {
        public TwitchAuthConfig TwitchAuthConfig { get; set; } = null!;
    }
}
