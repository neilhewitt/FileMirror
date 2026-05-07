using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FileMirror.Core.Config;

public class ConfigParser
{
    public Config Parse(string json)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        return JsonConvert.DeserializeObject<Config>(json, settings) ?? new Config();
    }

    public List<string> Validate(Config config)
    {
        List<string> errors = new();

        if (config.SourceMappings == null || config.SourceMappings.Count == 0)
        {
            errors.Add("At least one source mapping must be configured");
        }

        foreach (SourceMapping mapping in config.SourceMappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.SourcePath))
            {
                errors.Add("SourcePath is required");
            }

            if (string.IsNullOrWhiteSpace(mapping.TargetPath))
            {
                errors.Add("TargetPath is required");
            }
        }

        return errors;
    }
}