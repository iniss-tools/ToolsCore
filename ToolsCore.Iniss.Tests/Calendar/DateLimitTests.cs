using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ToolsCore.Iniss.Calendar;

namespace ToolsCore.Tests.Calendar;

/// <summary>
/// Zapis datumoveho obmedzenia (priklady z dokumentacie GVDEditora) nad grafikonom 13.12.2026 - 11.12.2027:
/// pocet dni, v ktore vlak ide, a text, na ktory GVDEditor zapis upravi.
/// </summary>
[TestClass]
[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class DateLimitTests
{
    private static readonly DateTime From = new(2026, 12, 13);
    private static readonly DateTime To = new(2027, 12, 11);

    private static DateLimit Limit() => new(From, To, insertMarks: false);

    private static int Count(BitArray bits) => bits.Cast<bool>().Count(b => b);

    [TestMethod]
    [DataRow("", 364, "")]
    [DataRow("ide denne", 364, "")]
    [DataRow("t.č. nejde", 0, "t.č. nejde")]
    [DataRow("ide v 1-5", 260, "ide v 1-5")]
    [DataRow("ide v 6,7", 104, "ide v 6,7")]
    [DataRow("ide v 6 a 7", 104, "ide v 6,7")]
    [DataRow("ide v X", 249, "ide v X")]
    [DataRow("ide v 5,7 a +", 115, "ide v 5,7,od 24. do 26.XII.,6.I.,29.III.,1.,8.V.,5.VII.,1.,15.IX.,1.,17.XI.")]
    [DataRow("nejde v 7", 312, "ide v 1-6")]
    [DataRow("ide 24.XII.", 1, "ide 24.XII.")]
    [DataRow("nejde 24.XII., 25.XII.", 362, "nejde 24.,25.XII.")]
    [DataRow("ide od 1.VI. do 30.IX.", 122, "ide od 1.VI. do 30.IX.")]
    [DataRow("ide od 1.VI. do 30.IX. v 6,7", 34, "ide od 5.VI. do 26.IX. v 6,7")]
    [DataRow("ide v 1-5 okrem 24.XII.", 259, "ide v 1-5,nejde 24.XII.")]
    [DataRow("ide v 1-5, nejde 24.XII.", 259, "ide v 1-5,nejde 24.XII.")]
    [DataRow("ide 1.6.", 1, "ide 1.VI.")]
    [DataRow("ide od 1.VI. do 30.VI. a od 1.IX. do 30.IX. v 6,7", 16, "ide od 5. do 27.VI. a od 4. do 26.IX. v 6,7")]
    [DataRow("jede v 1-5", 260, "ide v 1-5")]
    [DataRow("ide v 1-5 vrátane 26.XII.", 261, "ide v 1-5,26.XII.")]
    // Export3: dni za spojkou "a" samostatny datum pred nou neobmedzuju
    [DataRow("ide 26.XII. a od 26.III. v 7", 38, "ide 26.XII.,od 28.III. v 7")]
    [DataRow("ide 23.XII.,5.I.,24.III. a od 17.VI. v 5", 29, "ide 23.XII.,5.I.,24.III.,od 18.VI. v 5")]
    // Export3: jednotlivy datum ma prednost pred obdobim vo vynimkach
    [DataRow("ide v 7,1.IX.,nejde od 23.VIII. do 4.IX.", 52, "ide v 7,1.IX.,nejde 29.VIII.")]
    public void DatumoveObmedzenie_Zapis_DaDniAText(string text, int days, string normalized)
    {
        var bits = Limit().TextToBitArray(text);

        Assert.AreEqual(days, Count(bits), "počet dní");
        var result = Limit().BitArrayToText(bits);
        Assert.AreEqual(Count(bits), Count(Limit().TextToBitArray(result)), $"text „{result}“ nedá tie isté dni");
        CollectionAssert.AreEqual(bits.Cast<bool>().ToArray(), Limit().TextToBitArray(result).Cast<bool>().ToArray(), result);
        Assert.AreEqual(normalized, result);
    }

    [TestMethod]
    public void DatumoveObmedzenie_DatumMimoObdobia_JeChyba()
    {
        var e = Assert.ThrowsExactly<DateLimit.ParseException>(() => Limit().TextToBitArray("ide 24.XII.2030"));
        Assert.AreEqual("Dátum 24.12.2030 je mimo rozsahu platnosti grafikonu.", e.Message);
    }

    [TestMethod]
    public void DatumoveObmedzenie_Interval_SPomlckou_JeChyba()
    {
        Assert.ThrowsExactly<DateLimit.ParseException>(() => Limit().TextToBitArray("ide 20.XII.-2.I."));
    }

    [TestMethod]
    [DataRow(2026, 1, 6, DateLimit.Locale.Sk, true)]
    [DataRow(2026, 1, 6, DateLimit.Locale.Cz, false)]
    [DataRow(2026, 10, 28, DateLimit.Locale.Sk, false)]
    [DataRow(2026, 10, 28, DateLimit.Locale.Cz, true)]
    [DataRow(2026, 4, 3, DateLimit.Locale.Cz, true)]
    [DataRow(2026, 4, 6, DateLimit.Locale.Sk, true)]
    public void IsHoliday_PodlaZadanejKrajiny(int year, int month, int day, DateLimit.Locale locale, bool expected)
    {
        Assert.AreEqual(expected, DateLimit.IsHoliday(new DateTime(year, month, day), locale));
    }

    [TestMethod]
    public void HolidayLocale_MaPrednostPredJazykomPoznamok()
    {
        var from = new DateTime(2026, 1, 5);
        var to = new DateTime(2026, 1, 7);

        var common = new DateLimit(from, to, insertMarks: false).TextToBitArray("ide v +");
        var czech = new DateLimit(from, to, insertMarks: false) { HolidayLocale = DateLimit.Locale.Cz }.TextToBitArray("ide v +");

        Assert.AreEqual(DateLimit.Locale.Sk, DateLimit.Loc);
        Assert.IsTrue(common[1], "6.1. je sviatok na Slovensku");
        Assert.IsFalse(czech[1], "6.1. nie je sviatok v Cesku");
    }
}
