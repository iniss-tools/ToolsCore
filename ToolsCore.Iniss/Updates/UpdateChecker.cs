using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace ToolsCore.Iniss.Updates;

/// <summary>
/// Zistenie novej verzie z indexu aktualizacii a stiahnutie jej instalatora. Chyby siete a neplatneho indexu
/// (<see cref="HttpRequestException" />, <see cref="OperationCanceledException" />, <see cref="System.Text.Json.JsonException" />,
/// <see cref="InvalidDataException" />) vyhadzuje - volajuci rozhodne, ci ich ukaze.
/// </summary>
public static class UpdateChecker
{
    /// <summary>Najdlhsie cakanie na index.</summary>
    public static readonly TimeSpan IndexTimeout = TimeSpan.FromSeconds(15);

    /// <summary>Klient pre index aj instalator - predstavi sa nazvom a verziou programu, bez celkoveho casoveho limitu (stahovanie).</summary>
    public static HttpClient CreateClient(UpdateSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(source.Program, ReleaseVersion.Format(source.Version)));
        return http;
    }

    /// <summary>Nacita index a vrati vydanie programu novsie nez nainstalovana verzia, inak <c>null</c>.</summary>
    public static async Task<UpdateRelease?> FindNewerAsync(HttpClient http, UpdateSource source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(source);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(IndexTimeout);
        var json = await http.GetStringAsync(source.Index, timeout.Token).ConfigureAwait(false);
        return Newer(UpdateIndex.Parse(json), source);
    }

    /// <summary>Vydanie programu z indexu, ak je novsie nez nainstalovana verzia; neplatnu verziu v indexe ignoruje.</summary>
    public static UpdateRelease? Newer(UpdateIndex index, UpdateSource source)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(source);
        if (!index.Programs.TryGetValue(source.Program, out var release) || !ReleaseVersion.TryParse(release.Version, out var version))
            return null;
        return version > ReleaseVersion.Normalize(source.Version) ? release : null;
    }

    /// <summary>
    /// Stiahne instalator do suboru a overi jeho velkost a SHA-256 podla indexu; pri nezhode alebo chybe subor zmaze.
    /// </summary>
    /// <param name="http">klient z <see cref="CreateClient" /></param>
    /// <param name="installer">instalator z indexu</param>
    /// <param name="path">cielovy subor</param>
    /// <param name="progress">stiahnuta cast 0 az 1</param>
    /// <param name="cancellationToken">zrusenie stahovania</param>
    /// <exception cref="InvalidDataException">adresa nie je https alebo subor nezodpoveda indexu</exception>
    public static async Task DownloadAsync(HttpClient http, UpdateInstaller installer, string path, IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(installer);
        if (!Uri.TryCreate(installer.Url, UriKind.Absolute, out var url) || url.Scheme != Uri.UriSchemeHttps || installer.Size <= 0)
            throw new InvalidDataException("The installer in the update index has no https address or size.");

        try
        {
            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            long total = 0;
            var input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using (input.ConfigureAwait(false))
            {
                var output = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await using (output.ConfigureAwait(false))
                {
                    var buffer = new byte[81920];
                    int read;
                    while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        total += read;
                        if (total > installer.Size)
                            throw new InvalidDataException("The downloaded installer is larger than stated in the update index.");
                        hash.AppendData(buffer, 0, read);
                        await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                        progress?.Report((double) total / installer.Size);
                    }
                }
            }
            var sha256 = Convert.ToHexStringLower(hash.GetHashAndReset());
            if (total != installer.Size || !string.Equals(sha256, installer.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The downloaded installer does not match the update index.");
        }
        catch
        {
            TryDelete(path);
            throw;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
