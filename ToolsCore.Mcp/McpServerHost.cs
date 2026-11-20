using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Serilog;

namespace ToolsCore.Mcp;

/// <summary>
/// Server MCP vnutri programu (Streamable HTTP bez relacie) - agent cita otvorene data aj s neulozenymi zmenami. Pocuva
/// len na tomto pocitaci, vyzaduje pristupovy kluc a odmieta poziadavky z cudzej stranky v prehliadaci. Nastroje su
/// staticke metody tried <see cref="McpServerToolTypeAttribute" /> v zostave programu; nastroje so zmenami sa registruju
/// len s povolenim zmien (klient ich bez neho nevidi).
/// </summary>
public sealed class McpServerHost(McpServerDefinition definition) : IMcpServerStatus, IDisposable
{
    /// <summary>Cesta servera MCP.</summary>
    public const string Pattern = "/mcp";

    private const string Bearer = "Bearer ";

    /// <summary>Vysledky nastrojov aj s null - schema vlastnosti vyzaduje (klient by vysledok bez nich odmietol).</summary>
    private static readonly JsonSerializerOptions ToolJson = new(McpJsonUtilities.DefaultOptions) { DefaultIgnoreCondition = JsonIgnoreCondition.Never };

    /// <summary>
    /// Schemy nastrojov: <see cref="TimeOnly" /> a <see cref="DateTime" /> sa zapisu bez casoveho pasma, co formaty
    /// <c>time</c> a <c>date-time</c> (RFC 3339) nepripustaju - klient by vysledok odmietol, preto bez formatu.
    /// </summary>
    private static readonly AIJsonSchemaCreateOptions ToolSchemas = new()
    {
        TransformSchemaNode = (context, node) =>
        {
            var type = Nullable.GetUnderlyingType(context.TypeInfo.Type) ?? context.TypeInfo.Type;
            if ((type == typeof(TimeOnly) || type == typeof(DateTime)) && node is JsonObject schema)
                schema.Remove("format");
            return node;
        }
    };

    private WebApplication? _app;

    /// <inheritdoc />
    public string? Address { get; private set; }

    /// <inheritdoc />
    public string? Error { get; private set; }

    /// <inheritdoc />
    public McpSettings? Settings { get; private set; }

    /// <summary>
    /// Server podla <paramref name="settings" />: zastavi beziaci a pri zapnutom nastaveni spusti novy. Chybu nehadze -
    /// je v <see cref="Error" /> (program bez servera funguje dalej).
    /// </summary>
    /// <param name="settings">nastavenie servera</param>
    /// <param name="services">sluzby pre parametre nastrojov (napr. pristup k otvorenym datam programu)</param>
    public async Task ApplyAsync(McpSettings settings, Action<IServiceCollection> services)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(services);
        await StopAsync().ConfigureAwait(false);
        Settings = settings.Clone();
        Error = null;
        if (!settings.Enabled)
            return;
        var key = settings.Key;
        if (key.Length == 0)
        {
            Error = "no access key";
            Log.Warning("MCP: server je zapnuty, ale nema pristupovy kluc - nespusti sa");
            return;
        }

        try
        {
            // zostavenie a start servera mimo vlakna okna - pri starte programu by ho inak na chvilu zablokovali
            _app = await Task.Run(() => StartAsync(settings.Port, key, settings.AllowWrites, services)).ConfigureAwait(false);
            Address = settings.Address;
            Log.Information("MCP: server bezi na {Address} (zmeny povolene: {AllowWrites})", Address, settings.AllowWrites);
        }
        catch (Exception e) when (e is IOException or InvalidOperationException or SocketException)
        {
            Error = e.Message;
            Log.Error(e, "MCP: server sa na porte {Port} nespustil", settings.Port);
        }
    }

    private async Task<WebApplication> StartAsync(int port, string key, bool allowWrites, Action<IServiceCollection> services)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory });
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));
        // do zaznamu programu len upozornenia a chyby servera (napr. vynimka v nastroji)
        builder.Logging.ClearProviders().AddSerilog().SetMinimumLevel(LogLevel.Warning);
        services(builder.Services);
        builder.Services.AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation { Name = definition.Name, Version = Application.ProductVersion };
                options.ServerInstructions = definition.Instructions + (allowWrites ? definition.WriteInstructions : definition.ReadOnlyInstructions);
            })
            .WithHttpTransport(options => options.Stateless = true);
        // nastroje su internal - SDK ich zo zostavy samo nenajde
        foreach (var method in ToolMethods(definition.ToolAssembly).Where(m => allowWrites || IsReadOnly(m)))
            builder.Services.AddSingleton(provider => McpServerTool.Create(method, (object?)null,
                new McpServerToolCreateOptions { Services = provider, SerializerOptions = ToolJson, SchemaCreateOptions = ToolSchemas }));

        var app = builder.Build();
        var expected = Encoding.UTF8.GetBytes(key);
        app.MapMcp(Pattern).Add(endpoint =>
        {
            var inner = endpoint.RequestDelegate ?? throw new InvalidOperationException("MCP endpoint has no request delegate.");
            endpoint.RequestDelegate = context =>
            {
                if (!IsSameOrigin(context.Request))
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                else if (!HasKey(context.Request, expected))
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                else
                    return inner(context);
                return Task.CompletedTask;
            };
        });
        try
        {
            await app.StartAsync().ConfigureAwait(false);
        }
        catch
        {
            await app.DisposeAsync().ConfigureAwait(false);
            throw;
        }
        return app;
    }

    /// <summary>Staticke metody nastrojov z tried <see cref="McpServerToolTypeAttribute" /> v zostave.</summary>
    public static IReadOnlyList<MethodInfo> ToolMethods(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        return assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<McpServerToolTypeAttribute>() is not null)
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() is not null)
            .ToList();
    }

    /// <summary>Nastroj data nemeni.</summary>
    public static bool IsReadOnly(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);
        return method.GetCustomAttribute<McpServerToolAttribute>()?.ReadOnly == true;
    }

    /// <summary>Hlavicka <c>Authorization: Bearer</c> s klucom - porovnanie v konstantnom case.</summary>
    private static bool HasKey(HttpRequest request, byte[] expected)
    {
        var header = request.Headers.Authorization.ToString();
        return header.StartsWith(Bearer, StringComparison.OrdinalIgnoreCase)
               && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(header[Bearer.Length..].Trim()), expected);
    }

    /// <summary>Bez hlavicky Origin (nie prehliadac) alebo z rovnakeho servera - ochrana pred DNS rebinding.</summary>
    private static bool IsSameOrigin(HttpRequest request)
    {
        var origin = request.Headers.Origin.ToString();
        return origin.Length == 0
               || Uri.TryCreate(origin, UriKind.Absolute, out var uri) && string.Equals(uri.Authority, request.Host.Value, StringComparison.OrdinalIgnoreCase);
    }

    private async Task StopAsync()
    {
        if (_app is not { } app)
            return;
        _app = null;
        Address = null;
        await app.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose() =>
        // pri ukonceni programu - na vlakne okna sa na zastavenie neda cakat cez await
        Task.Run(StopAsync).Wait(TimeSpan.FromSeconds(2));
}
