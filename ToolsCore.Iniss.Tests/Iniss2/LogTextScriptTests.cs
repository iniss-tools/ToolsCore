using ToolsCore.Iniss2.Grafikon;
using ToolsCore.Iniss2.Grafikon.Script;

namespace ToolsCore.Tests.Iniss2;

/// <summary>
/// Skript hlaseni LogText.vb: clenenie na funkcie, hlasenia upravitelne ako udaje (rozpoznanie spatnym zapisom),
/// nahradenie, nova funkcia a zmazanie bez zmeny ostatneho textu.
/// </summary>
[TestClass]
public class LogTextScriptTests
{
    private static readonly string Members = """"
        Imports IniSrv

        Public Class LogText
            Inherits LogTextBase

            Private zoznam As List(Of Long) = New List(Of Long)

            ' Dodatky
            <Dodatek("Bufet ""U vlaku""")> _
            Public Sub D1()
                Zvuk("SK/Dodatky/D1") ' End Sub v komentari
                Dim text = "End Sub"
            End Sub ' D1

            <OstatniHlaseni("Výzva", _
                ProStanice:="5400010")> Public Sub Vyzva()
                Dim f = Sub()
                            Zvuk("SK/Vety/a")
                        End Sub
                f()
            End Sub

            Private Class Pomocna
                Public Sub Vnutorna()
                End Sub
            End Class

            Private Function Kontrola() As Boolean
                Return True
            End Function
        End Class
        """".ReplaceLineEndings("\r\n");

    private static readonly string Reports = """
        Public Class LogText
            Inherits LogTextBase

            ' Dodatky
            <Dodatek("Zmena radenia")> _
            Public Sub D1006()
                Zvuk("SK/Dodatky/D1006")
                Zvuk("SK/Slova/cso", 200, 0)
            End Sub ' D1006

            <OstatniHlaseni("Bezpečnostná výzva", ProStanice:="5400010,5400020")> _
            Public Sub Bezpecnostna_vyzva()
                Zvuk("SK/Vety/bezpec")
                'Zvuk("D/Vety/bezpec")
                Zvuk("GB/Vety/bezpec") ' doplnene
                Pauza(300)
            End Sub

            <VzorOdklonu("Odklon cez Lipovú")> _
            Public Sub Odklon_Lipova(ByVal aiStanice As Integer())
                If (Jazyk <> "SK") Then Return
                Zvuk("SK/Slova/odklon", 0, 200)

                If Jazyk = "SK" Then
                    Pauza(200)
                End If
            End Sub

            <InterpreterAttribute("Úvodná znelka")> _
            Public Sub Znělka()
            End Sub

            <VzorVyluky("Výluka", TYPVYLUKY.AUTO)> _
            Public Sub Vyluka_v_stanici(ByVal aiStanice As Integer())
                Zvuk("SK/Slova/vyluka")
            End Sub

            <OstatniHlaseni("Bomba")> _
            Public Sub Bomba()
                Select Case AudioLine.iSR70
                    Case 5400010
                        Zvuk("SK/Vety/bomba2")
                End Select
            End Sub

            Private Sub Pomocna()
                Zvuk("SK/Slova/a")
            End Sub

            <Dodatek("Dva")> <OstatniHlaseni("Dva")> _
            Public Sub Dve_oznacenia()
            End Sub

            <Dodatek("Funkcia")> _
            Public Function D2() As Boolean
                Return True
            End Function

            <Dodatek("Obmedzenie", ProStanice:="5400010")> _
            Public Sub D3()
            End Sub

            <Dodatek("Nulove pauzy")> _
            Public Sub D4()
                Zvuk("SK/Dodatky/D4", 0, 0)
            End Sub
        End Class

        """.ReplaceLineEndings("\r\n");

    private static string[] Lines(string text) => text.Split("\r\n");

    private static int LineOf(string text, string start) =>
        Array.FindIndex(Lines(text), line => line.TrimStart().StartsWith(start, StringComparison.Ordinal));

    private static ScriptReadResult ReadReport(string text, string name)
    {
        var script = LogTextScript.Parse(text);
        return ScriptReportReader.Read(script, script.Find(name)!);
    }

    [TestMethod]
    public void Clenenie_FunkcieOznaceniaAKoniec()
    {
        var script = LogTextScript.Parse(Members);

        Assert.AreEqual("\r\n", script.NewLine);
        Assert.AreEqual(LineOf(Members, "Public Class LogText"), script.ClassStart);
        Assert.AreEqual(Lines(Members).Length - 1, script.ClassEnd);
        CollectionAssert.AreEqual(new[] { "D1", "Vyzva", "Kontrola" }, script.Members.Select(member => member.Name).ToArray());

        var d1 = script.Members[0];
        Assert.AreEqual(LineOf(Members, "<Dodatek("), d1.Start);
        Assert.AreEqual(LineOf(Members, "Public Sub D1()"), d1.Signature);
        Assert.AreEqual(LineOf(Members, "End Sub ' D1"), d1.End);
        Assert.AreEqual("' D1", d1.EndComment);
        Assert.AreEqual("    ", d1.Indent);
        Assert.AreEqual("Bufet \"U vlaku\"", d1.Attributes.Single().Description);
        CollectionAssert.AreEqual(new[] { "Public" }, d1.Modifiers.ToArray());
        Assert.AreEqual("", d1.Parameters);

        // oznacenie na dvoch riadkoch a podpis na tom istom riadku, viacriadkova lambda ma vlastny koniec
        var vyzva = script.Members[1];
        Assert.AreEqual(LineOf(Members, "<OstatniHlaseni("), vyzva.Start);
        Assert.AreEqual(LineOf(Members, "ProStanice:="), vyzva.Signature);
        Assert.AreEqual(LineOf(Members, "f()") + 1, vyzva.End);
        CollectionAssert.AreEqual(new[] { "\"Výzva\"", "ProStanice:=\"5400010\"" }, vyzva.Attributes.Single().Arguments.ToArray());
        Assert.IsNull(vyzva.EndComment);

        // funkcia vnorenej triedy nepatri triede LogText
        var kontrola = script.Members[2];
        Assert.AreEqual("Function", kontrola.Keyword);
        CollectionAssert.AreEqual(new[] { "Private" }, kontrola.Modifiers.ToArray());
        Assert.AreEqual("As Boolean", kontrola.Rest);
        Assert.AreEqual(0, kontrola.Attributes.Count);
    }

    [TestMethod]
    public void Clenenie_RovnakeFunkcieAOznaceniaAkoLogTextFile()
    {
        var script = LogTextScript.Parse(Reports);
        var functions = LogTextFile.Functions(Lines(Reports));
        var descriptions = LogTextFile.Descriptions(Lines(Reports));

        Assert.AreEqual(functions.Count, script.Members.Count);
        foreach (var member in script.Members)
        {
            Assert.IsTrue(functions[member.Name].SetEquals(member.Attributes.Select(attribute => attribute.Name)), member.Name);
            Assert.AreEqual(descriptions.GetValueOrDefault(member.Name), member.Attributes.Select(attribute => attribute.Description).FirstOrDefault(text => text is not null), member.Name);
        }
    }

    [TestMethod]
    public void Clenenie_FunkciaBezKoncaSaVynecha()
    {
        var script = LogTextScript.Parse(string.Join("\n", "Public Class LogText", "    Public Sub A()", "        Zvuk(\"SK/Slova/a\")",
            "    Public Sub B()", "    End Sub", "End Class"));

        Assert.AreEqual("\n", script.NewLine);
        CollectionAssert.AreEqual(new[] { "B" }, script.Members.Select(member => member.Name).ToArray());
    }

    [TestMethod]
    public void Citanie_HlaseniaUrovne1()
    {
        var reports = ScriptReports.Read(LogTextScript.Parse(Reports)).ToDictionary(result => result.Member.Name);

        Assert.AreEqual(new ScriptReport("D1006", ScriptReportKind.Addition)
        {
            Description = "Zmena radenia",
            Blocks = [new ScriptBlock(null, [new SoundStep("SK/Dodatky/D1006"), new SoundStep("SK/Slova/cso", 200)])]
        }, reports["D1006"].Report);

        Assert.AreEqual(new ScriptReport("Bezpecnostna_vyzva", ScriptReportKind.Other)
        {
            Description = "Bezpečnostná výzva",
            ForStations = "5400010,5400020",
            Blocks =
            [
                new ScriptBlock(null,
                [
                    new SoundStep("SK/Vety/bezpec"), new CommentStep("Zvuk(\"D/Vety/bezpec\")"),
                    new SoundStep("GB/Vety/bezpec") { Comment = " doplnene" }, new PauseStep(300)
                ])
            ]
        }, reports["Bezpecnostna_vyzva"].Report);

        // straz jazyka, jazykovy blok bez zatvoriek a prazdny riadok su len zapis
        Assert.AreEqual(new ScriptReport("Odklon_Lipova", ScriptReportKind.DiversionPattern)
        {
            Description = "Odklon cez Lipovú",
            StationsParameter = "aiStanice",
            OnlyLanguage = "SK",
            Blocks = [new ScriptBlock(null, [new SoundStep("SK/Slova/odklon", 0, 200)]), new ScriptBlock("SK", [new PauseStep(200)])]
        }, reports["Odklon_Lipova"].Report);

        Assert.AreEqual(new ScriptReport("Znělka", ScriptReportKind.Generic) { Description = "Úvodná znelka" }, reports["Znělka"].Report);
        CollectionAssert.AreEqual(new[] { "TYPVYLUKY.AUTO" }, reports["Vyluka_v_stanici"].Report!.Arguments.ToArray());
        Assert.AreEqual("SK", ((SoundStep)reports["Bezpecnostna_vyzva"].Report!.Blocks[0].Steps[0]).Language);
        Assert.IsFalse(reports.ContainsKey("Pomocna"));
    }

    [TestMethod]
    public void Citanie_DodatokZExportuGVDEditora()
    {
        var text = string.Join("\r\n",
            "Public Class LogText",
            "",
            "    ' Dodatky doplnene exportom z GVDEditora (INISS 3.39)",
            "    <Dodatek(\"Upozornenie na zmenu radenia\")> _",
            "    Public Sub D1006()",
            "        Zvuk(\"Dodatky/D1006\")",
            "    End Sub ' D1006",
            "",
            "End Class");

        var report = ReadReport(text, "D1006").Report;

        Assert.AreEqual(new ScriptReport("D1006", ScriptReportKind.Addition)
        {
            Description = "Upozornenie na zmenu radenia",
            Blocks = [new ScriptBlock(null, [new SoundStep("Dodatky/D1006")])]
        }, report);
        Assert.IsNull(((SoundStep)report!.Blocks[0].Steps[0]).Language);
        Assert.AreEqual(text, ScriptReports.Write(text, report, "D1006"));
    }

    [TestMethod]
    [DataRow("Bomba", ScriptManualReason.Statement, "Select Case AudioLine")]
    [DataRow("Dve_oznacenia", ScriptManualReason.NotReport, "<Dodatek(\"Dva\")>")]
    [DataRow("D2", ScriptManualReason.Signature, "Public Function D2()")]
    [DataRow("D3", ScriptManualReason.Attribute, "<Dodatek(\"Obmedzenie\"")]
    [DataRow("D4", ScriptManualReason.Layout, "<Dodatek(\"Nulove pauzy\")>")]
    public void Citanie_RucnaFunkciaSDovodom(string name, ScriptManualReason reason, string line)
    {
        var result = ReadReport(Reports, name);

        Assert.IsNull(result.Report);
        Assert.AreEqual(reason, result.Reason);
        Assert.AreEqual(LineOf(Reports, line), result.Line);
    }

    [TestMethod]
    public void Zapis_NahradiLenFunkciu()
    {
        var report = ReadReport(Reports, "D1006").Report! with
        {
            Description = "Zmena \"radenia\"",
            Blocks = [new ScriptBlock(null, [new SoundStep("SK/Dodatky/D1006")]), new ScriptBlock("GB", [new SoundStep("GB/Dodatky/D1006", 0, 300)])]
        };

        var text = ScriptReports.Write(Reports, report, "D1006");

        var expected = Lines(Reports).ToList();
        var start = LineOf(Reports, "<Dodatek(\"Zmena radenia\")>");
        expected.RemoveRange(start, 5);
        expected.InsertRange(start,
        [
            "    <Dodatek(\"Zmena \"\"radenia\"\"\")> _",
            "    Public Sub D1006()",
            "        Zvuk(\"SK/Dodatky/D1006\")",
            "        If (Jazyk = \"GB\") Then",
            "            Zvuk(\"GB/Dodatky/D1006\", 0, 300)",
            "        End If",
            "    End Sub ' D1006"
        ]);
        Assert.AreEqual(string.Join("\r\n", expected), text);
        Assert.AreEqual(report, ReadReport(text, "D1006").Report);
    }

    [TestMethod]
    public void Zapis_BezZmenyNechaTextPoZnakoch()
    {
        var script = LogTextScript.Parse(Reports);

        Assert.AreEqual(Reports, script.Text);
        foreach (var result in ScriptReports.Read(script).Where(result => result.Report is not null && result.Member.Name != "Odklon_Lipova"))
            Assert.AreEqual(Reports, ScriptReports.Write(Reports, result.Report!, result.Member.Name), result.Member.Name);

        // funkcia v inom zapise (prazdny riadok, If bez zatvoriek) dostane zapis generatora, vyznam ostane
        var odklon = ScriptReports.Read(script).Single(result => result.Member.Name == "Odklon_Lipova").Report!;
        var text = ScriptReports.Write(Reports, odklon, "Odklon_Lipova");
        StringAssert.Contains(text, "        Zvuk(\"SK/Slova/odklon\", 0, 200)\r\n        If (Jazyk = \"SK\") Then\r\n");
        Assert.AreEqual(odklon, ReadReport(text, "Odklon_Lipova").Report);
        Assert.AreEqual(Lines(Reports).Length - 1, Lines(text).Length);
    }

    [TestMethod]
    public void Zapis_PremenovanieZmeniKomentarZaKoncom()
    {
        var report = ReadReport(Reports, "D1006").Report! with { Name = "D1007" };

        var text = ScriptReports.Write(Reports, report, "D1006");

        StringAssert.Contains(text, "    Public Sub D1007()\r\n");
        StringAssert.Contains(text, "    End Sub ' D1007\r\n");
        Assert.IsFalse(text.Contains("D1006()", StringComparison.Ordinal));
        Assert.AreEqual(Lines(Reports).Length, Lines(text).Length);
    }

    [TestMethod]
    public void Zapis_NovaFunkciaZaPoslednouRovnakehoDruhu()
    {
        var report = new ScriptReport("D5", ScriptReportKind.Addition)
        {
            Description = "Nový dodatok",
            Blocks = [new ScriptBlock(null, [new SoundStep("SK/Dodatky/D5")])]
        };

        var text = ScriptReports.Write(Reports, report);

        var lines = Lines(text);
        var d4End = LineOf(Reports, "Zvuk(\"SK/Dodatky/D4\"") + 1;
        CollectionAssert.AreEqual(new[]
        {
            "    End Sub",
            "",
            "    <Dodatek(\"Nový dodatok\")> _",
            "    Public Sub D5()",
            "        Zvuk(\"SK/Dodatky/D5\")",
            "    End Sub ' D5",
            "End Class"
        }, lines[d4End..(d4End + 7)]);
        Assert.AreEqual(report, ReadReport(text, "D5").Report);
    }

    [TestMethod]
    public void Zapis_NovyDruhPredKoniecTriedy()
    {
        var text = ScriptReports.Write("Public Class LogText\n    Inherits LogTextBase\nEnd Class\n",
            new ScriptReport("Posledna_vyzva", ScriptReportKind.Supplementary) { Description = "Posledná výzva" });

        Assert.AreEqual(string.Join("\n",
            "Public Class LogText",
            "    Inherits LogTextBase",
            "",
            "    <DoplnkoveHlaseni(\"Posledná výzva\")> _",
            "    Public Sub Posledna_vyzva()",
            "    End Sub ' Posledna_vyzva",
            "End Class",
            ""), text);
    }

    [TestMethod]
    public void Zapis_KoliziaNazvuAChybajucaTrieda()
    {
        // VB.NET nerozlisuje velkost pismen
        Assert.ThrowsExactly<ArgumentException>(() => ScriptReports.Write(Reports, new ScriptReport("bomba", ScriptReportKind.Other)));
        Assert.ThrowsExactly<ArgumentException>(() => ScriptReports.Write(Reports, new ScriptReport("Bomba", ScriptReportKind.Other), "D1006"));
        Assert.ThrowsExactly<ArgumentException>(() => ScriptReports.Write(Reports, new ScriptReport("D9", ScriptReportKind.Addition), "Neexistuje"));
        Assert.ThrowsExactly<FormatException>(() => ScriptReports.Write("' prazdny skript", new ScriptReport("D1", ScriptReportKind.Addition)));
    }

    [TestMethod]
    public void Zmazanie_OdstraniFunkciuAJedenPrazdnyRiadok()
    {
        var text = ScriptReports.Remove(Reports, "d1006");

        var expected = Lines(Reports).ToList();
        var start = LineOf(Reports, "<Dodatek(\"Zmena radenia\")>");
        expected.RemoveRange(start, 5);
        Assert.AreEqual(string.Join("\r\n", expected), text);
        StringAssert.Contains(text, "    ' Dodatky\r\n\r\n    <OstatniHlaseni(");
        Assert.AreEqual(Reports, ScriptReports.Remove(Reports, "Neexistuje"));

        // posledna funkcia triedy: odstrani sa prazdny riadok pred nou
        var last = ScriptReports.Remove(Reports, "D4");
        StringAssert.Contains(last, "    Public Sub D3()\r\n    End Sub\r\nEnd Class\r\n");
    }

    [TestMethod]
    public void Zapis_PlnyModelSaPrecitaRovnako()
    {
        var report = new ScriptReport("Vzor_vyluky", ScriptReportKind.ExclusionPattern)
        {
            Description = "Výluka \"medzi\" stanicami",
            Arguments = ["TYPVYLUKY.TRATOVA"],
            ForStations = "5400010",
            NotStations = "5400020",
            StationsParameter = "aiStanice",
            OnlyLanguage = "SK",
            Blocks =
            [
                new ScriptBlock(null, [new CommentStep(" poznamka"), new PauseStep(100) { Comment = "ticho" }]),
                new ScriptBlock("SK", [new SoundStep("SK/Slova/vyluka", 100, 0), new CommentStep("Zvuk(\"SK/Slova/x\")")]),
                new ScriptBlock("GB", [])
            ]
        };

        var text = ScriptReports.Write("Public Class LogText\r\nEnd Class", report);

        Assert.AreEqual(report, ReadReport(text, "Vzor_vyluky").Report);
    }

    private static readonly string Level2 = """
        Public Class LogText
            <DoplnkoveHlaseni("Posledná výzva")> _
            Public Sub Posledna_vyzva()
                Zvuk("SK/Slova/vc", 0, 200)
                DruhVlaku("V4", 0, True, bMezinarodni:=True)
                CisloVlaku(VlakS.CVlaku)
                Pauza(200) : Dodatky() ' dodatky
                SeznamStanic(VlakS.StaniceDoSmeru(ModifikatorHlaseni), Nothing, Nothing, "R3", iIndexOd:=-1)
                PoziceKolej("K1", VlakS.PozicePrijSkut, sNahled:="KOLAJ")
                SK_kam_ide()
            End Sub

            <VzorDodatku("Koniec v stanici")> _
            Public Sub Koniec(ByVal aiStanice As Integer())
                SeznamStanic(aiStanice, "R1", "R4", "R4", PrefixPosl:=AddressOf SK_kam_ide)
            End Sub

            <Odklon("Odklon")> _
            Public Sub O1()
                Koniec(Nothing)
            End Sub

            <OstatniHlaseni("Vyraz")> _
            Public Sub Vyraz()
                DruhVlaku("V4", If(JeIDS, 4, 9), True)
            End Sub

            <OstatniHlaseni("Neznama")> _
            Public Sub Neznama()
                Neexistuje()
            End Sub

            <OstatniHlaseni("Malo argumentov")> _
            Public Sub Malo()
                Hodiny("C1")
            End Sub

            Private Sub SK_kam_ide()
                Zvuk("SK/Slova/do")
            End Sub
        End Class
        """.ReplaceLineEndings("\r\n");

    [TestMethod]
    public void Uroven2_VstavaneMetodyAVolania()
    {
        var reports = ScriptReports.Read(LogTextScript.Parse(Level2)).ToDictionary(result => result.Member.Name);

        var steps = reports["Posledna_vyzva"].Report!.Blocks.Single().Steps;
        CollectionAssert.AreEqual(new ScriptStep[]
        {
            new SoundStep("SK/Slova/vc", 0, 200),
            new BuiltinStep("DruhVlaku", ["\"V4\"", "0", "True", "bMezinarodni:=True"]),
            new BuiltinStep("CisloVlaku", ["VlakS.CVlaku"]),
            new PauseStep(200),
            new BuiltinStep("Dodatky", []) { Comment = " dodatky" },
            new BuiltinStep("SeznamStanic", ["VlakS.StaniceDoSmeru(ModifikatorHlaseni)", "Nothing", "Nothing", "\"R3\"", "iIndexOd:=-1"]),
            new BuiltinStep("PoziceKolej", ["\"K1\"", "VlakS.PozicePrijSkut", "sNahled:=\"KOLAJ\""]),
            new CallStep("SK_kam_ide", [])
        }, steps.ToArray());
        Assert.AreEqual(new CallStep("Koniec", ["Nothing"]), reports["O1"].Report!.Blocks.Single().Steps.Single());
        Assert.IsNotNull(reports["Koniec"].Report);

        // vyraz If(…), funkcia mimo skriptu a chybajuci povinny argument ostanu rucnym kodom
        foreach (var name in new[] { "Vyraz", "Neznama", "Malo" })
            Assert.AreEqual(ScriptManualReason.Statement, reports[name].Reason, name);

        // dva kroky na riadku sa po zapise rozdelia, vyznam ostane
        var text = ScriptReports.Write(Level2, reports["Posledna_vyzva"].Report!, "Posledna_vyzva");
        StringAssert.Contains(text, "        Pauza(200)\r\n        Dodatky() ' dodatky\r\n");
        Assert.AreEqual(reports["Posledna_vyzva"].Report, ReadReport(text, "Posledna_vyzva").Report);
    }

    private static readonly string Level3 = """
        Public Class LogText
            <DoplnkoveHlaseni("Predĺženie pobytu")> _
            Public Sub Predlzenie()
                If (Jazyk <> "SK") Then Return
                If Not Kontrola_koncici() Then Return
                If VlakS.JeProjizdejici Then
                    SK_pobyt10()
                End If
            End Sub

            <VzorDodatku("Zastavenie v stanici")> _
            Public Sub Zastavenie(ByVal aiStanice As Integer())
                ZacatekSekce()
                If (Jazyk = "SK") Then
                    If (aiStanice IsNot Nothing) AndAlso (aiStanice.Length = 1) Then
                        Zvuk("SK/Slova/vmzvs")
                    Else
                        Zvuk("SK/Slova/vmzvs2")
                    End If
                    SeznamStanic(aiStanice, "R1", "R4", "R4")
                End If
                If (Jazyk = "GB") Then
                    If AudioLine.bGong Then
                    Else
                        Zvuk("GB/Slova/x")
                    End If
                End If
                KonecSekce(bRollBack:=True)
            End Sub

            <OstatniHlaseni("Bomba")> _
            Public Sub Bomba()
                'Dolne Mesto ma vlastne znenie
                If AudioLine.iSR70 = 5400010 Then
                    Zvuk("SK/Vety/bomba2")
                Else
                    Zvuk("SK/Vety/bomba")
                End If
            End Sub

            <OstatniHlaseni("ElseIf")> _
            Public Sub Vetvy()
                If VlakS.JeVychozi Then
                    Zvuk("SK/Vety/a")
                ElseIf VlakS.JeKoncici Then
                    Zvuk("SK/Vety/b")
                End If
            End Sub

            <OstatniHlaseni("Straz v strede")> _
            Public Sub Straz()
                Zvuk("SK/Vety/a")
                If VlakS.JeKoncici Then Return
            End Sub

            Private Function Kontrola_koncici() As Boolean
                Return Not VlakS.JeKoncici
            End Function

            Private Sub SK_pobyt10()
                Zvuk("SK/Slova/pobyt10")
            End Sub
        End Class
        """.ReplaceLineEndings("\r\n");

    [TestMethod]
    public void Uroven3_StrazePodmienkyElseASekcia()
    {
        var reports = ScriptReports.Read(LogTextScript.Parse(Level3)).ToDictionary(result => result.Member.Name);

        Assert.AreEqual(new ScriptReport("Predlzenie", ScriptReportKind.Supplementary)
        {
            Description = "Predĺženie pobytu",
            OnlyLanguage = "SK",
            Guards = ["Not Kontrola_koncici()"],
            Blocks = [new ScriptBlock(null, [new CallStep("SK_pobyt10", [])]) { Conditions = [new ScriptCondition("VlakS.JeProjizdejici")] }]
        }, reports["Predlzenie"].Report);

        var single = new ScriptCondition("(aiStanice IsNot Nothing) AndAlso (aiStanice.Length = 1)");
        var gong = new ScriptCondition("AudioLine.bGong");
        var zastavenie = reports["Zastavenie"].Report!;
        Assert.IsTrue(zastavenie.Section);
        CollectionAssert.AreEqual(new[]
        {
            new ScriptBlock("SK", [new SoundStep("SK/Slova/vmzvs")]) { Conditions = [single] },
            new ScriptBlock("SK", [new SoundStep("SK/Slova/vmzvs2")]) { Conditions = [single with { Else = true }] },
            new ScriptBlock("SK", [new BuiltinStep("SeznamStanic", ["aiStanice", "\"R1\"", "\"R4\"", "\"R4\""])]),
            new ScriptBlock("GB", []) { Conditions = [gong] },
            new ScriptBlock("GB", [new SoundStep("GB/Slova/x")]) { Conditions = [gong with { Else = true }] }
        }, zastavenie.Blocks.ToArray());

        var bomba = reports["Bomba"].Report!;
        Assert.AreEqual(new CommentStep("Dolne Mesto ma vlastne znenie"), bomba.Blocks[0].Steps.Single());
        Assert.IsTrue(bomba.Blocks[2].Conditions.Single().Else);

        // ElseIf a straz za krokom ostanu rucnym kodom
        Assert.AreEqual(ScriptManualReason.Statement, reports["Vetvy"].Reason);
        Assert.AreEqual(ScriptManualReason.Statement, reports["Straz"].Reason);

        // zapis bez zmeny da ten isty text
        foreach (var name in new[] { "Predlzenie", "Zastavenie", "Bomba" })
            Assert.AreEqual(Level3, ScriptReports.Write(Level3, reports[name].Report!, name), name);
    }

    [TestMethod]
    public void Uroven3_ZapisVnorenychPodmienok()
    {
        var a = new ScriptCondition("VlakS.JeVychozi");
        var b = new ScriptCondition("DlouheHlaseni");
        var report = new ScriptReport("Test", ScriptReportKind.Other)
        {
            Guards = ["VlakS.JeKoncici"],
            Section = true,
            Blocks =
            [
                new ScriptBlock(null, [new SoundStep("SK/a")]) { Conditions = [a] },
                new ScriptBlock(null, [new SoundStep("SK/b")]) { Conditions = [a, b] },
                new ScriptBlock(null, [new SoundStep("SK/c")]) { Conditions = [a, b] },
                new ScriptBlock(null, [new SoundStep("SK/d")]) { Conditions = [a with { Else = true }] },
                new ScriptBlock("SK", [new PauseStep(100)]) { Conditions = [b with { Else = true }] }
            ]
        };

        var text = ScriptReports.Write("Public Class LogText\r\nEnd Class", report);

        StringAssert.Contains(text, string.Join("\r\n",
            "        If VlakS.JeKoncici Then Return",
            "        ZacatekSekce()",
            "        If VlakS.JeVychozi Then",
            "            Zvuk(\"SK/a\")",
            "            If DlouheHlaseni Then",
            "                Zvuk(\"SK/b\")",
            "            End If",
            "            If DlouheHlaseni Then",
            "                Zvuk(\"SK/c\")",
            "            End If",
            "        Else",
            "            Zvuk(\"SK/d\")",
            "        End If",
            "        If (Jazyk = \"SK\") Then",
            "            If DlouheHlaseni Then",
            "            Else",
            "                Pauza(100)",
            "            End If",
            "        End If",
            "        KonecSekce(bRollBack:=True)",
            "    End Sub"));
        Assert.AreEqual(report with { Blocks = [.. report.Blocks.Take(4), new ScriptBlock("SK", []) { Conditions = [b] }, report.Blocks[4]] },
            ReadReport(text, "Test").Report);
    }

    [TestMethod]
    [DataRow("\"V4\"", true)]
    [DataRow("-1", true)]
    [DataRow("True", true)]
    [DataRow("Nothing", true)]
    [DataRow("VlakS.Druh(True)", true)]
    [DataRow("VlakS.Stanice.iSR70", true)]
    [DataRow("AddressOf PrefixPosl", true)]
    [DataRow("aiStanice", true)]
    [DataRow("If(JeIDS, 4, 9)", false)]
    [DataRow("VlakS.CasPrij + 5", false)]
    [DataRow("\"a\" & \"b\"", false)]
    [DataRow("Not VlakS.JeVychozi", false)]
    [DataRow("1.5", false)]
    [DataRow("", false)]
    public void Argumenty_Jednoduche(string expression, bool simple) => Assert.AreEqual(simple, ScriptArguments.IsSimple(expression));

    [TestMethod]
    public void Argumenty_PocetAPomenovaneParametre()
    {
        var druh = ScriptBuiltins.Find("druhvlaku")!;

        Assert.AreEqual(2, druh.Required);
        Assert.IsTrue(druh.Accepts(["\"V4\"", "0"]));
        Assert.IsTrue(druh.Accepts(["\"V4\"", "0", "bPovinneMistenkovy:=True"]));
        Assert.IsTrue(druh.Accepts(["sGroupTypVlaku:=\"V4\"", "iLimitZpozdeni:=0"]));
        Assert.IsFalse(druh.Accepts(["\"V4\""]));
        Assert.IsFalse(druh.Accepts(["\"V4\"", "0", "bNeznamy:=True"]));
        Assert.IsFalse(druh.Accepts(["bPrijezd:=True", "\"V4\"", "0"]));
        Assert.IsFalse(druh.Accepts(["\"V4\"", "0", "True", "True", "True", "\"R\"", "1"]));
        Assert.AreEqual("True", druh.Argument(["\"V4\"", "0", "bMezinarodni:=True"], 3));
        Assert.AreEqual("0", druh.Argument(["\"V4\"", "0"], 1));
        Assert.IsNull(druh.Argument(["\"V4\"", "0"], 2));
    }

    [TestMethod]
    public void Krok_JedenRiadokTamASpat()
    {
        ScriptStep[] steps =
        [
            new SoundStep("SK/Vety/a"), new SoundStep("SK/Vety/a", 0, 200) { Comment = " poznamka" }, new PauseStep(300),
            new CommentStep("Zvuk(\"GB/Vety/a\")")
        ];

        foreach (var step in steps)
            Assert.AreEqual(step, ScriptReportReader.ReadLine(ScriptReportWriter.StepText(step)));
        Assert.AreEqual(new SoundStep("GB/Vety/a"), ScriptReportReader.ReadLine("Zvuk(\"GB/Vety/a\")"));
        Assert.AreEqual(new BuiltinStep("Dodatky", []), ScriptReportReader.ReadLine("Dodatky()"));
        Assert.AreEqual(new CallStep("SK_kam_ide", ["aiStanice"]), ScriptReportReader.ReadLine("SK_kam_ide(aiStanice)"));
        Assert.IsNull(ScriptReportReader.ReadLine("SK_kam_ide()", _ => false));
        Assert.IsNull(ScriptReportReader.ReadLine("If VlakS.JeVychozi Then Return"));
        Assert.IsNull(ScriptReportReader.ReadLine(""));
    }

    [TestMethod]
    public void Druhy_NazvyOznaceni()
    {
        foreach (var kind in ScriptReportKinds.All)
        {
            Assert.IsTrue(ScriptReportKinds.TryParse(ScriptReportKinds.AttributeName(kind), out var parsed));
            Assert.AreEqual(kind, parsed);
        }

        Assert.IsTrue(ScriptReportKinds.TryParse("IniSrv.DodatekAttribute", out var addition));
        Assert.AreEqual(ScriptReportKind.Addition, addition);
        Assert.IsTrue(ScriptReportKinds.TryParse("interpreter", out var generic));
        Assert.AreEqual(ScriptReportKind.Generic, generic);
        Assert.IsFalse(ScriptReportKinds.TryParse("Popis", out _));
        Assert.IsTrue(ScriptReportKinds.IsPattern(ScriptReportKind.AdditionPattern));
        Assert.IsFalse(ScriptReportKinds.HasStationLimit(ScriptReportKind.Addition));
    }

    [TestMethod]
    [DataRow("If (Jazyk = \"SK\") Then", "If Jazyk = \"SK\" Then")]
    [DataRow("Zvuk( \"SK/a\" ,0,  200 )", "Zvuk(\"SK/a\", 0, 200)")]
    [DataRow("End Sub   '  D1", "End Sub ' D1")]
    public void Porovnanie_LenZapisSaNelisi(string first, string second) =>
        CollectionAssert.AreEqual(VbText.Normalize([first]), VbText.Normalize([second]));

    [TestMethod]
    [DataRow("Zvuk(\"SK/a  b\")", "Zvuk(\"SK/a b\")")]
    [DataRow("If (a) Or (b) Then", "If a Or b Then")]
    public void Porovnanie_RetazceAVyznamSaLisia(string first, string second) =>
        CollectionAssert.AreNotEqual(VbText.Normalize([first]), VbText.Normalize([second]));
}
