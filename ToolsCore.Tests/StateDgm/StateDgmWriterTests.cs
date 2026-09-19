using System.Diagnostics.CodeAnalysis;
using ToolsCore.StateDgm;

namespace ToolsCore.Tests.StateDgm;

/// <summary>
///     Zapisovac - kanonicky tvar a zachovanie neznamych poloziek.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class StateDgmWriterTests
{
    private const string Sample = """
        0001
        C:"Popis"
        P:"StateDgmCtrls\\CtrlDesign"
        {
        	G:"Design1" {S:"Key"="KoľajN"	S:"Bitmaps"="3-0,1,2"	I:"DefPushBtn"=0	S:"Class"="SDCCtrlDesignBtn"}
        	I:"NumDesigns"=1
        }
        P:"StateDgmCtrls\\StateDgm"
        {
        	I:"NumTimePoints"=0
        	I:"NumCategories"=1
        	S:"IndCat"="INDCAT8"
        	G:"ModeTabsSections" {S:"X"="y"}
        }
        P:"StateDgmCtrls\\StateDgm\\Categorie1"
        {
        	S:"Key"="#Výchozí vlak"
        	S:"Name"="Výchozí vlak"
        	I:"Icon"=0
        	G:"State"
        	{
        		S:"Key"="#Start"
        		I:"Attr"=0x98
        		G:"DoState" {S:"Class"="SVFTableSet" B:"OnDepTable"=Ano B:"TrackNumber"=Ne }
        		I:"AutoMode"=2
        		S:"AutoTimePointAdd"="ZPOZDENIPRIJ*60"
        		I:"WaitPath"=1
        		I:"Neznamy"=7
        		G:"Event" {S:"Key"="#GoToVypiš" S:"NextState"="Vypiš" S:"Class"="SDEventUniPos" I:"Extra"=1}
        		G:"Control" {I:"CtrlID"=0 S:"DesignKey"="KoľajN" S:"EventKey"="#GoToVypiš"}
        		I:"NumEvents"=5
        	}
        }
        P:"Cudzie" { S:"A"="b" }
        """;

    [TestMethod]
    public void Writer_CanonicalFormAndExtras()
    {
        var d = StateDgmDiagram.Parse(Sample);
        Assert.AreEqual(1, d.Warnings.Count, string.Join("; ", d.Warnings));
        StringAssert.Contains(d.Warnings[0].Message, "NumEvents=5");

        var s = d.Categories[0].States[0];
        Assert.AreEqual(StateDgmAttr.Stoji | StateDgmAttr.PotvrzenaKolej | StateDgmAttr.NyniStoji, s.Attr);
        Assert.IsTrue(s.DoState!.OnDepartureTable);
        Assert.IsFalse(s.DoState.ShowTrack);
        Assert.IsTrue(s.DoState.ShowPosition == false);
        Assert.AreEqual("VVC", s.Wait!.Expression, "číselný WaitPath sa prevedie na Wait=VVC");
        Assert.IsNull(s.WaitPath);
        Assert.IsTrue(s.AutoTimePointAdd!.IsExpression);
        Assert.AreEqual(1, s.Extras.Count);
        Assert.AreEqual(1, s.Events[0].Extras.Count);
        Assert.AreEqual("INDCAT8", d.IndCat);
        Assert.AreEqual(1, d.HeaderExtras.Count);
        Assert.AreEqual(1, d.RootExtras.Count);

        var text = d.ToText();
        StringAssert.Contains(text, "0001\r\nC:\"Popis\"\r\n");
        StringAssert.Contains(text, "I:\"Attr\"=0x98\t; SVSA_Stoji, SVSA_PotvrzenaKolej, SVSA_NyniStoji");
        StringAssert.Contains(text, "B:\"JeNaOdjezdové\"=Ano");
        StringAssert.Contains(text, "B:\"JeZobrazenaKolej\"=Ne");
        StringAssert.Contains(text, "I:\"AutoMode\"=2\t; automat");
        StringAssert.Contains(text, "S:\"AutoTimePointAdd\"=\"ZPOZDENIPRIJ*60\"");
        StringAssert.Contains(text, "S:\"Wait\"=\"VVC\"");
        StringAssert.Contains(text, "I:\"Neznamy\"=7");
        StringAssert.Contains(text, "S:\"Class\"=\"SDEventUniPos\"\tI:\"Extra\"=1}");
        StringAssert.Contains(text, "I:\"NumEvents\"=1");
        StringAssert.Contains(text, "G:\"ModeTabsSections\" {S:\"X\"=\"y\"}");
        StringAssert.Contains(text, "P:\"Cudzie\"\r\n{\r\n\tS:\"A\"=\"b\"\r\n}");
        Assert.IsFalse(text.Contains("WaitPath"));

        var again = StateDgmDiagram.Parse(text);
        Assert.AreEqual(0, again.Warnings.Count);
        Assert.AreEqual(text, again.ToText());
    }

    [TestMethod]
    public void Writer_EmptyDiagram()
    {
        var d = new StateDgmDiagram();
        var text = d.ToText();
        var again = StateDgmDiagram.Parse(text);
        Assert.AreEqual(0, again.Categories.Count);
        Assert.AreEqual(0, again.Designs.Count);
        Assert.AreEqual(0, again.Warnings.Count);
    }
}
