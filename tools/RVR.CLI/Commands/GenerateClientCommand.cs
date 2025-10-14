using System.Text;
using System.Text.Json;
using Spectre.Console;

namespace RVR.CLI.Commands;

/// <summary>
/// Generates a typed C# HTTP client from an OpenAPI (Swagger) specification.
/// </summary>
public static class GenerateClientCommand
{
    /// <summary>
    /// Executes the client generation command.
    /// </summary>
    /// <param name="url">API base URL.</param>
    /// <param name="output">Output directory for generated files.</param>
    /// <param name="name">Generated client class name.</param>
    public static async Task ExecuteAsync(string url, string output, string name)
    {
        AnsiConsole.MarkupLine("[bold blue]Generating API client from OpenAPI spec...[/]");

        var swaggerUrl = $"{url.TrimEnd('/')}/swagger/v1/swagger.json";
        AnsiConsole.MarkupLine("[grey]Fetching spec from:[/] {0}", swaggerUrl);

        JsonDocument spec;
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var json = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Downloading OpenAPI spec...", async _ =>
                {
                    return await httpClient.GetStringAsync(swaggerUrl);
                });
            spec = JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Failed to fetch OpenAPI spec:[/] {0}", ex.Message);
            AnsiConsole.MarkupLine("[yellow]Generating a sample client stub instead...[/]");
            await GenerateStubClientAsync(output, name);
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Generating client code...", async _ =>
            {
                await GenerateFromSpecAsync(spec, output, name);
            });

        AnsiConsole.MarkupLine("[bold green]Client generation complete![/]");
    }

    private static async Task GenerateFromSpecAsync(JsonDocument spec, string output, string name)
    {
        Directory.CreateDirectory(output);
        var modelsDir = Path.Combine(output, "Models");
        Directory.CreateDirectory(modelsDir);

        var root = spec.RootElement;
        var endpoints = new List<EndpointInfo>();
        var modelNames = new HashSet<string>();

        // Extract schemas / models
        if (root.TryGetProperty("components", out var components) &&
            components.TryGetProperty("schemas", out var schemas))
        {
            foreach (var schema in schemas.EnumerateObject())
            {
                modelNames.Add(schema.Name);
                var modelCode = GenerateModelClass(schema.Name, schema.Value);
                var modelPath = Path.Combine(modelsDir, $"{SanitizeName(schema.Name)}.cs");
                await File.WriteAllTextAsync(modelPath, modelCode);
                AnsiConsole.MarkupLine("[green]  + Created[/] Models/{0}.cs", SanitizeName(schema.Name));
            }
        }

        // Extract endpoints
        if (root.TryGetProperty("paths", out var paths))
        {
            foreach (var path in paths.EnumerateObject())
            {
                foreach (var method in path.Value.EnumerateObject())
                {
                    var httpMethod = method.Name.ToUpperInvariant();
                    if (httpMethod is not ("GET" or "POST" or "PUT" or "DELETE" or "PATCH"))
                        continue;

                    var operationId = method.Value.TryGetProperty("operationId", out var opId)
                        ? opId.GetString() ?? BuildOperationId(httpMethod, path.Name)
                        : BuildOperationId(httpMethod, path.Name);

                    string? returnType = null;
                    if (method.Value.TryGetProperty("responses", out var responses))
                    {
                        if (responses.TryGetProperty("200", out var ok200))
                            returnType = ExtractResponseType(ok200);
                        else if (responses.TryGetProperty("201", out var ok201))
                            returnType = ExtractResponseType(ok201);
                    }

                    string? requestBodyType = null;
                    if (method.Value.TryGetProperty("requestBody", out var reqBody))
                    {
                        requestBodyType = ExtractRequestBodyType(reqBody);
                    }

                    var parameters = new List<ParameterInfo>();
                    if (method.Value.TryGetProperty("parameters", out var parms))
                    {
                        foreach (var p in parms.EnumerateArray())
                        {
                            var pName = p.GetProperty("name").GetString() ?? "param";
                            var pIn = p.GetProperty("in").GetString() ?? "query";
                            var pType = "string";
                            if (p.TryGetProperty("schema", out var pSchema) &&
                                pSchema.TryGetProperty("type", out var pTypeEl))
                            {
                                pType = MapJsonTypeToCSharp(pTypeEl.GetString());
                            }
                            parameters.Add(new ParameterInfo(pName, pType, pIn));
                        }
                    }

                    endpoints.Add(new EndpointInfo(
                        path.Name, httpMethod, SanitizeMethodName(operationId),
                        returnType, requestBodyType, parameters));
                }
            }
        }

        // Generate the client class
        var clientCode = GenerateClientClass(name, endpoints);
        var clientPath = Path.Combine(output, $"{name}.cs");
        await File.WriteAllTextAsync(clientPath, clientCode);
        AnsiConsole.MarkupLine("[green]  + Created[/] {0}.cs", name);
    }

    private static string GenerateClientClass(string name, List<EndpointInfo> endpoints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System.Net.Http.Json;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine();
        sb.AppendLine("namespace Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Auto-generated API client.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {name} : IDisposable");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly HttpClient _httpClient;");
        sb.AppendLine("    private static readonly JsonSerializerOptions JsonOptions = new()");
        sb.AppendLine("    {");
        sb.AppendLine("        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,");
        sb.AppendLine("        PropertyNameCaseInsensitive = true");
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine($"    public {name}(HttpClient httpClient)");
        sb.AppendLine("    {");
        sb.AppendLine("        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    public {name}(string baseUrl)");
        sb.AppendLine("    {");
        sb.AppendLine("        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };");
        sb.AppendLine("    }");

        foreach (var ep in endpoints)
        {
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// {ep.HttpMethod} {ep.Path}");
            sb.AppendLine("    /// </summary>");

            var returnCSharp = ep.ReturnType ?? "object";
            var asyncReturn = $"Task<{returnCSharp}?>";

            // Build parameter list
            var paramList = new List<string>();
            foreach (var p in ep.Parameters)
            {
                paramList.Add($"{p.CSharpType} {SanitizeParameterName(p.Name)}");
            }
            if (ep.RequestBodyType != null)
            {
                paramList.Add($"{ep.RequestBodyType} body");
            }
            paramList.Add("CancellationToken cancellationToken = default");

            sb.AppendLine($"    public async {asyncReturn} {ep.MethodName}Async({string.Join(", ", paramList)})");
            sb.AppendLine("    {");

            // Build URL with path and query parameters
            var pathParams = ep.Parameters.Where(p => p.Location == "path").ToList();
            var queryParams = ep.Parameters.Where(p => p.Location == "query").ToList();

            var urlExpr = ep.Path;
            foreach (var pp in pathParams)
            {
                urlExpr = urlExpr.Replace($"{{{pp.Name}}}", $"{{{SanitizeParameterName(pp.Name)}}}");
            }

            if (queryParams.Count > 0)
            {
                sb.AppendLine($"        var queryParts = new List<string>();");
                foreach (var qp in queryParams)
                {
                    var pn = SanitizeParameterName(qp.Name);
                    sb.AppendLine($"        if ({pn} != null) queryParts.Add($\"{qp.Name}={{{pn}}}\");");
                }
                sb.AppendLine($"        var url = $\"{urlExpr}\" + (queryParts.Count > 0 ? \"?\" + string.Join(\"&\", queryParts) : \"\");");
            }
            else
            {
                sb.AppendLine($"        var url = $\"{urlExpr}\";");
            }

            // HTTP call
            switch (ep.HttpMethod)
            {
                case "GET":
                    sb.AppendLine($"        var response = await _httpClient.GetAsync(url, cancellationToken);");
                    break;
                case "DELETE":
                    sb.AppendLine($"        var response = await _httpClient.DeleteAsync(url, cancellationToken);");
                    break;
                case "POST":
                    if (ep.RequestBodyType != null)
                        sb.AppendLine($"        var response = await _httpClient.PostAsJsonAsync(url, body, JsonOptions, cancellationToken);");
                    else
                        sb.AppendLine($"        var response = await _httpClient.PostAsync(url, null, cancellationToken);");
                    break;
                case "PUT":
                    if (ep.RequestBodyType != null)
                        sb.AppendLine($"        var response = await _httpClient.PutAsJsonAsync(url, body, JsonOptions, cancellationToken);");
                    else
                        sb.AppendLine($"        var response = await _httpClient.PutAsync(url, null, cancellationToken);");
                    break;
                case "PATCH":
                    if (ep.RequestBodyType != null)
                        sb.AppendLine($"        var content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, \"application/json\");");
                    else
                        sb.AppendLine($"        var content = new StringContent(\"\", Encoding.UTF8, \"application/json\");");
                    sb.AppendLine($"        var response = await _httpClient.PatchAsync(url, content, cancellationToken);");
                    break;
            }

            sb.AppendLine("        response.EnsureSuccessStatusCode();");
            sb.AppendLine($"        return await response.Content.ReadFromJsonAsync<{returnCSharp}>(JsonOptions, cancellationToken);");

            sb.AppendLine("    }");
        }

        sb.AppendLine();
        sb.AppendLine("    public void Dispose() => _httpClient.Dispose();");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateModelClass(string name, JsonElement schema)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine();
        sb.AppendLine("namespace Generated.Models;");
        sb.AppendLine();
        var safeName = SanitizeName(name);
        sb.AppendLine($"public class {safeName}");
        sb.AppendLine("{");

        if (schema.TryGetProperty("properties", out var props))
        {
            var requiredSet = new HashSet<string>();
            if (schema.TryGetProperty("required", out var reqArr))
            {
                foreach (var r in reqArr.EnumerateArray())
                {
                    var rv = r.GetString();
                    if (rv != null) requiredSet.Add(rv);
                }
            }

            foreach (var prop in props.EnumerateObject())
            {
                var propType = "object";
                if (prop.Value.TryGetProperty("type", out var typeEl))
                {
                    propType = MapJsonTypeToCSharp(typeEl.GetString(), prop.Value);
                }
                else if (prop.Value.TryGetProperty("$ref", out var refEl))
                {
                    propType = ExtractRefName(refEl.GetString());
                }

                var nullable = !requiredSet.Contains(prop.Name) ? "?" : "";
                var csharpPropName = ToPascalCase(prop.Name);

                sb.AppendLine($"    [JsonPropertyName(\"{prop.Name}\")]");
                sb.AppendLine($"    public {propType}{nullable} {csharpPropName} {{ get; set; }}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static async Task GenerateStubClientAsync(string output, string name)
    {
        Directory.CreateDirectory(output);
        var modelsDir = Path.Combine(output, "Models");
        Directory.CreateDirectory(modelsDir);

        var stubClient = $@"using System.Net.Http.Json;
using System.Text.Json;

namespace Generated;

/// <summary>
/// Auto-generated API client stub. Re-run with a live API to get full endpoints.
/// </summary>
public class {name} : IDisposable
{{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {{
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    }};

    public {name}(HttpClient httpClient)
    {{
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }}

    public {name}(string baseUrl)
    {{
        _httpClient = new HttpClient {{ BaseAddress = new Uri(baseUrl) }};
    }}

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    public async Task<string?> HealthCheckAsync(CancellationToken cancellationToken = default)
    {{
        var response = await _httpClient.GetAsync(""/health"", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }}

    public void Dispose() => _httpClient.Dispose();
}}
";
        await File.WriteAllTextAsync(Path.Combine(output, $"{name}.cs"), stubClient);
        AnsiConsole.MarkupLine("[green]  + Created[/] {0}.cs (stub)", name);
        AnsiConsole.MarkupLine("[bold green]Stub client generated.[/] Run again with a live API for full generation.");
    }

    #region Helpers

    private static string MapJsonTypeToCSharp(string? jsonType, JsonElement? schema = null)
    {
        return jsonType switch
        {
            "integer" => schema.HasValue && schema.Value.TryGetProperty("format", out var fmt) && fmt.GetString() == "int64" ? "long" : "int",
            "number" => schema.HasValue && schema.Value.TryGetProperty("format", out var nfmt) && nfmt.GetString() == "float" ? "float" : "double",
            "boolean" => "bool",
            "string" => schema.HasValue && schema.Value.TryGetProperty("format", out var sfmt)
                ? sfmt.GetString() switch
                {
                    "date-time" => "DateTime",
                    "uuid" => "Guid",
                    "date" => "DateOnly",
                    "time" => "TimeOnly",
                    _ => "string"
                }
                : "string",
            "array" => schema.HasValue && schema.Value.TryGetProperty("items", out var items)
                ? $"List<{GetItemType(items)}>"
                : "List<object>",
            _ => "object"
        };
    }

    private static string GetItemType(JsonElement items)
    {
        if (items.TryGetProperty("$ref", out var refEl))
            return ExtractRefName(refEl.GetString());
        if (items.TryGetProperty("type", out var t))
            return MapJsonTypeToCSharp(t.GetString());
        return "object";
    }

    private static string ExtractRefName(string? refString)
    {
        if (string.IsNullOrEmpty(refString)) return "object";
        var parts = refString.Split('/');
        return SanitizeName(parts[^1]);
    }

    private static string? ExtractResponseType(JsonElement response)
    {
        if (response.TryGetProperty("content", out var content))
        {
            JsonElement? mediaType = null;
            if (content.TryGetProperty("application/json", out var appJson))
                mediaType = appJson;
            else if (content.TryGetProperty("text/plain", out var textPlain))
                return "string";

            if (mediaType.HasValue && mediaType.Value.TryGetProperty("schema", out var schema))
            {
                if (schema.TryGetProperty("$ref", out var refEl))
                    return ExtractRefName(refEl.GetString());
                if (schema.TryGetProperty("type", out var typeEl))
                {
                    if (typeEl.GetString() == "array" && schema.TryGetProperty("items", out var items))
                        return $"List<{GetItemType(items)}>";
                    return MapJsonTypeToCSharp(typeEl.GetString(), schema);
                }
            }
        }
        return null;
    }

    private static string? ExtractRequestBodyType(JsonElement reqBody)
    {
        if (reqBody.TryGetProperty("content", out var content) &&
            content.TryGetProperty("application/json", out var appJson) &&
            appJson.TryGetProperty("schema", out var schema))
        {
            if (schema.TryGetProperty("$ref", out var refEl))
                return ExtractRefName(refEl.GetString());
            if (schema.TryGetProperty("type", out var typeEl))
                return MapJsonTypeToCSharp(typeEl.GetString(), schema);
        }
        return null;
    }

    private static string SanitizeName(string name)
    {
        var result = new StringBuilder();
        foreach (var ch in name)
        {
            if (char.IsLetterOrDigit(ch)) result.Append(ch);
        }
        if (result.Length == 0) return "Unknown";
        if (char.IsDigit(result[0])) result.Insert(0, '_');
        return result.ToString();
    }

    private static string SanitizeMethodName(string name)
    {
        var pascal = ToPascalCase(name);
        return string.IsNullOrEmpty(pascal) ? "Call" : pascal;
    }

    private static string SanitizeParameterName(string name)
    {
        var result = new StringBuilder();
        bool nextUpper = false;
        for (int i = 0; i < name.Length; i++)
        {
            var ch = name[i];
            if (ch == '-' || ch == '.' || ch == '_')
            {
                nextUpper = true;
                continue;
            }
            if (!char.IsLetterOrDigit(ch)) continue;
            if (i == 0)
                result.Append(char.ToLowerInvariant(ch));
            else if (nextUpper)
            {
                result.Append(char.ToUpperInvariant(ch));
                nextUpper = false;
            }
            else
                result.Append(ch);
        }
        var s = result.ToString();
        return string.IsNullOrEmpty(s) ? "param" : s;
    }

    private static string ToPascalCase(string input)
    {
        var sb = new StringBuilder();
        bool upper = true;
        foreach (var ch in input)
        {
            if (ch == '_' || ch == '-' || ch == '.')
            {
                upper = true;
                continue;
            }
            if (upper)
            {
                sb.Append(char.ToUpperInvariant(ch));
                upper = false;
            }
            else
            {
                sb.Append(ch);
            }
        }
        return sb.ToString();
    }

    private static string BuildOperationId(string method, string path)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !p.StartsWith('{'))
            .Select(ToPascalCase);
        return method.Substring(0, 1).ToUpper() + method.Substring(1).ToLower() + string.Concat(parts);
    }

    #endregion

    private record EndpointInfo(
        string Path,
        string HttpMethod,
        string MethodName,
        string? ReturnType,
        string? RequestBodyType,
        List<ParameterInfo> Parameters);

    private record ParameterInfo(string Name, string CSharpType, string Location);
}
