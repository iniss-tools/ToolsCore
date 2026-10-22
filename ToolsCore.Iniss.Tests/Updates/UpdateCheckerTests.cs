using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using ToolsCore.Iniss.Updates;

namespace ToolsCore.Tests.Updates;

/// <summary>
/// Index aktualizacii, porovnanie verzii vydania a stiahnutie instalatora s kontrolou velkosti a SHA-256.
/// </summary>
[TestClass]
public class UpdateCheckerTests
{
    private const string IndexJson = """
        {
          "schema": 1,
          "programs": {
            "GVDEditor": {
              "version": "1.10",
              "published": "2026-10-22T18:00:00Z",
              "page": "https://github.com/iniss-tools/GVDEditor/releases/tag/v1.10",
              "notes": "Nové: kontrola aktualizácií",
              "installer": { "url": "https://example.test/gvdeditor-setup-1.10.exe", "sha256": "ab", "size": 2 }
            },
            "RawBankEditor": { "version": "nie-verzia" }
          }
        }
        """;

    private static readonly Uri Index = new("https://example.test/updates.json");

    private static UpdateSource Source(string program, string version) => new(program, Version.Parse(version), Index);

    [TestMethod]
    [DataRow("1.10", 1, 10, 0)]
    [DataRow("1.10.2", 1, 10, 2)]
    [DataRow(" 2.0 ", 2, 0, 0)]
    public void ReleaseVersion_CitaXYAjXYZ(string text, int major, int minor, int build)
    {
        Assert.IsTrue(ReleaseVersion.TryParse(text, out var version));
        Assert.AreEqual(new Version(major, minor, build), version);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("1.2.3.4")]
    [DataRow("v1.2")]
    [DataRow("1.-2")]
    [DataRow("")]
    [DataRow(null)]
    public void ReleaseVersion_NeplatnaVerzia(string? text)
    {
        Assert.IsFalse(ReleaseVersion.TryParse(text, out _));
    }

    [TestMethod]
    public void ReleaseVersion_FormatAkoZnacka()
    {
        Assert.AreEqual("1.10", ReleaseVersion.Format(new Version(1, 10, 0, 0)));
        Assert.AreEqual("1.10", ReleaseVersion.Format(new Version(1, 10)));
        Assert.AreEqual("1.10.2", ReleaseVersion.Format(new Version(1, 10, 2, 0)));
    }

    [TestMethod]
    public void Index_CitaZaznamProgramu()
    {
        var index = UpdateIndex.Parse(IndexJson);

        var release = index.Programs["GVDEditor"];
        Assert.AreEqual(1, index.Schema);
        Assert.AreEqual("1.10", release.Version);
        Assert.AreEqual("Nové: kontrola aktualizácií", release.Notes);
        Assert.AreEqual(new DateTimeOffset(2026, 10, 22, 18, 0, 0, TimeSpan.Zero), release.Published);
        Assert.AreEqual(2, release.Installer!.Size);
    }

    [TestMethod]
    public void Index_NeplatnyJson()
    {
        Assert.ThrowsExactly<JsonException>(() => UpdateIndex.Parse("null"));
        Assert.ThrowsExactly<JsonException>(() => UpdateIndex.Parse("<html>"));
    }

    [TestMethod]
    [DataRow("1.9.0.0", true)]
    [DataRow("1.9.5.0", true, DisplayName = "1.9.5 < 1.10 - cisla, nie text")]
    [DataRow("1.10.0.0", false)]
    [DataRow("1.10.0.7", false, DisplayName = "revizia sa neporovnava")]
    [DataRow("1.11.0.0", false)]
    public void Newer_LenNovsiaVerzia(string installed, bool newer)
    {
        var release = UpdateChecker.Newer(UpdateIndex.Parse(IndexJson), Source("GVDEditor", installed));

        Assert.AreEqual(newer, release is not null);
    }

    [TestMethod]
    public void Newer_ProgramMimoIndexuAleboNeplatnaVerzia()
    {
        var index = UpdateIndex.Parse(IndexJson);

        Assert.IsNull(UpdateChecker.Newer(index, Source("ElenFontEditor", "1.0")), "program bez vydania");
        Assert.IsNull(UpdateChecker.Newer(index, Source("RawBankEditor", "1.0")), "neplatna verzia v indexe");
        Assert.IsNull(UpdateChecker.Newer(index, Source("gvdeditor", "1.0")), "nazov programu je presny");
    }

    [TestMethod]
    public void Source_ZostavaBezIndexuNekontroluje()
    {
        Assert.IsNull(UpdateSource.FromAssembly(typeof(UpdateChecker).Assembly));
    }

    [TestMethod]
    public async Task FindNewer_NacitaIndexZAdresy()
    {
        using var http = new HttpClient(new StubHandler(request =>
        {
            Assert.AreEqual(Index, request.RequestUri);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(IndexJson) };
        }));

        var release = await UpdateChecker.FindNewerAsync(http, Source("GVDEditor", "1.9"), CancellationToken.None);

        Assert.AreEqual("1.10", release?.Version);
    }

    [TestMethod]
    public async Task FindNewer_ChybaServera()
    {
        using var http = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => UpdateChecker.FindNewerAsync(http, Source("GVDEditor", "1.9"), CancellationToken.None));
    }

    [TestMethod]
    public async Task Download_OveriSuctaVelkost()
    {
        var data = Enumerable.Range(0, 200_000).Select(i => (byte) i).ToArray();
        var path = Path.GetTempFileName();
        try
        {
            using var http = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data) }));
            var reported = new List<double>();

            await UpdateChecker.DownloadAsync(http, Installer(data), path, new SyncProgress(reported.Add), CancellationToken.None);

            CollectionAssert.AreEqual(data, await File.ReadAllBytesAsync(path));
            Assert.AreEqual(1.0, reported[^1]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public async Task Download_InySuborZmaze()
    {
        var data = new byte[] { 1, 2, 3 };
        var path = Path.GetTempFileName();
        using var http = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data) }));

        await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
            UpdateChecker.DownloadAsync(http, Installer(data) with { Sha256 = new string('0', 64) }, path, null, CancellationToken.None));
        Assert.IsFalse(File.Exists(path), "zly subor sa zmaze");

        await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
            UpdateChecker.DownloadAsync(http, Installer(data) with { Size = 2 }, path, null, CancellationToken.None));
        Assert.IsFalse(File.Exists(path), "vacsi subor sa zmaze");
    }

    [TestMethod]
    public async Task Download_LenHttps()
    {
        using var http = new HttpClient(new StubHandler(_ => throw new AssertFailedException("nesmie sa stahovat")));

        await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
            UpdateChecker.DownloadAsync(http, new UpdateInstaller { Url = "http://example.test/setup.exe", Size = 1 }, "nic.exe", null, CancellationToken.None));
    }

    [TestMethod]
    public void Source_AdresaIndexuZMetadatZostavy()
    {
        var source = UpdateSource.FromAssembly(new MetadataAssembly("https://example.test/updates.json"));

        Assert.AreEqual(new UpdateSource("Program", new Version(1, 10, 0), Index), source);
        Assert.IsNull(UpdateSource.FromAssembly(new MetadataAssembly("http://example.test/updates.json")), "len https");
    }

    private static UpdateInstaller Installer(byte[] data) => new()
    {
        Url = "https://example.test/setup.exe",
        Sha256 = Convert.ToHexStringLower(SHA256.HashData(data)),
        Size = data.Length
    };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private sealed class SyncProgress(Action<double> report) : IProgress<double>
    {
        public void Report(double value) => report(value);
    }

    /// <summary>Zostava s metadatami ako z vydania (AssemblyMetadata UpdateIndex) a verziou 1.10.0.0.</summary>
    private sealed class MetadataAssembly(string index) : Assembly
    {
        public override AssemblyName GetName() => new("Program") { Version = new Version(1, 10, 0, 0) };

        // pole typu atributu - CustomAttributeExtensions ho pretypuje na IEnumerable<T>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) =>
            attributeType == typeof(AssemblyMetadataAttribute)
                ? new[] { new AssemblyMetadataAttribute(UpdateSource.IndexMetadataKey, index) }
                : (object[]) Array.CreateInstance(attributeType, 0);
    }
}
