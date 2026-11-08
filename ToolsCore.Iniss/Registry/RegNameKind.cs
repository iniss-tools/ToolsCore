namespace ToolsCore.Iniss.Registry;

/// <summary>Ako sa tvori nazov hodnoty.</summary>
public enum RegNameKind
{
    /// <summary>Pevny nazov.</summary>
    Fixed,

    /// <summary>Sablona s indexom tabule (<c>Enabled&lt;N&gt;</c>).</summary>
    Indexed,

    /// <summary>Nazov urcuje hardver alebo program za behu (prvky mixera).</summary>
    Dynamic,

    /// <summary>Farba - nazov je z jazykovej kniznice RCIniss.dll (retazce 10000-10041).</summary>
    Color
}
