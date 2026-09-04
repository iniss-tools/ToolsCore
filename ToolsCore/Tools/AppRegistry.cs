using System.Globalization;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace ToolsCore.Tools;

/// <summary>
///     Trieda spracujuca zoznam poslednych pouzivanych projektov.
/// </summary>
public static class AppRegistry
{
    private static string _productName;
    private static ProjectInfo[] _projects;

    private static string ProductName => _productName ??= Assembly.GetEntryAssembly()?.GetName().Name;

    private static string _jumpListCategory;
    private static bool _jumpListFailed;
    private static bool _jumpListItemRemoved;
    
    /// <summary>
    ///     Predvoleny nazov kategorie v zozname odkazov (jump list) na paneli uloh.
    /// </summary>
    private const string JUMPLIST_CATEGORY = "Posledné projekty";

    /// <summary>
    ///     Maximalny pocet projektov zobrazenych v zozname odkazov na paneli uloh.
    /// </summary>
    private const int MAX_JUMPLIST_ITEMS = 10;

    /// <summary>
    ///     Nazov kluca v Registy so zoznamom poslednych pouzivanych priecinkov s datami.
    /// </summary>
    private const string REG_RECENT_DIRS = "RecentDirs";

    /// <summary>
    ///     Nazov kluca v Registy so zoznamom poslednych pouzivanych suborov.
    /// </summary>
    private const string REG_RECENT_FILES = "RecentFiles";

    /// <summary>
    ///     Nazov kluca v Registy s posledne otvorenehym projektom.
    /// </summary>
    private const string REG_LAST_PROJECT = "LastProject";

    /// <summary>
    ///     Nazov kluca v Registy so zoznamom posledne pouzivanych projektu.
    /// </summary>
    private const string REG_OPENED_PROJECTS = "OpenedProjects";

    private static List<string> GetOpenedProjectsOld(bool forFiles = false)
    {
        var key = Registry.CurrentUser.OpenSubKey($"SOFTWARE\\{ProductName}");
        var dirs = new HashSet<string>();

        var value = key?.GetValue(forFiles ? REG_RECENT_FILES : REG_RECENT_DIRS);

        if (value == null) 
            return dirs.ToList();

        var itemsString = value.ToString();
        var items = itemsString.Split(';');

        foreach (var item in items)
            if (!string.IsNullOrWhiteSpace(item))
                dirs.Add(item);

        return dirs.ToList();
    }

    /// <summary>
    ///     Vrati zoznam vsetkych ciest poslednych pouzivanych priecinkov s datami.<br></br>
    ///     Ak kluc v Registri s tymto zoznamom neexisstuje, metoda vrati prazdny list.
    /// </summary>
    /// <returns>zoznam ciest.</returns>
    public static ProjectInfo[] GetOpenedProjects()
    {
        var key = Registry.CurrentUser.OpenSubKey($"SOFTWARE\\{ProductName}");
        if (key is null)
            return Array.Empty<ProjectInfo>();

        var projects = new HashSet<ProjectInfo>();
        var regValue = key.GetValue(REG_OPENED_PROJECTS);
        if (regValue is null)
        {
            //konvertovanie stareho listu priecinkov na novy zoznam projektov
            //nebude fungovat ak sa pouzival kluc REG_RECENT_FILES
            var dirs = GetOpenedProjectsOld();
            foreach (var dir in dirs)
            {
                projects.Add(new ProjectInfo(dir, DateTime.MinValue));
            }
        }
        else
        {
            var itemsString = regValue.ToString();
            var items = itemsString.Split('|');
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                var pathAndDate = item.Split('*');
                switch (pathAndDate.Length)
                {
                    case 0:
                        continue;
                    case 1:
                        projects.Add(new ProjectInfo(pathAndDate[0], DateTime.MinValue));
                        break;
                    case 2:
                        projects.Add(new ProjectInfo(pathAndDate[0], DateTime.Parse(pathAndDate[1], CultureInfo.InvariantCulture)));
                        break;
                    default:
                        continue;
                }
            }
        }

        return _projects = projects.ToArray();
    }

    /// <summary>
    ///     Vytvori zoznam odkazov (jump list) na paneli uloh so zoznamom poslednych pouzivanych projektov.<br/>
    ///     Metodu treba zavolat pri starte aplikacie po vytvoreni hlavneho okna, dalej sa zoznam aktualizuje sam
    ///     pri kazdom otvoreni projektu.
    /// </summary>
    /// <param name="categoryName">Nazov kategorie, pod ktorou sa projekty na paneli uloh zobrazia.</param>
    public static void RegisterJumpList(string categoryName = JUMPLIST_CATEGORY)
    {
        _jumpListCategory = categoryName;
        RefreshJumpList();
    }

    /// <summary>
    ///     Nanovo vytvori a zapise zoznam odkazov na paneli uloh.
    /// </summary>
    /// <param name="retryOnError">
    ///     Ak <see langword="true"/>, po chybe sposobenej polozkou odstranenou pouzivatelom sa zapis zopakuje.
    /// </param>
    private static void RefreshJumpList(bool retryOnError = true)
    {
        //zoznam odkazov sa vytvara len v aplikaciach, ktore o to poziadali metodou RegisterJumpList
        if (_jumpListCategory is null || _jumpListFailed || !TaskbarManager.IsPlatformSupported)
            return;

        var exePath = Assembly.GetEntryAssembly()?.Location;
        if (string.IsNullOrEmpty(exePath))
            return;

        var projects = (_projects ?? GetOpenedProjects())
            .Where(project => !string.IsNullOrWhiteSpace(project.Path))
            .OrderByDescending(project => project.LastAccess)
            .Take(MAX_JUMPLIST_ITEMS)
            .ToArray();

        try
        {
            //uz zapisane polozky sa nedaju odstranit, preto sa cely zoznam zakazdym vytvara nanovo
            var jumpList = JumpList.CreateJumpList();
            jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Neither;
            jumpList.JumpListItemsRemoved += JumpList_ItemsRemoved;

            if (projects.Length != 0)
            {
                var category = new JumpListCustomCategory(_jumpListCategory);
                foreach (var project in projects)
                    category.AddJumpListItems(CreateJumpListLink(project.Path, exePath));

                jumpList.AddCustomCategories(category);
            }

            jumpList.Refresh();
            _jumpListItemRemoved = false;
        }
        catch (Exception e)
        {
            //ak pouzivatel polozku zo zoznamu odstranil, Windows odmietne jej opatovne pridanie
            //- pri zapise sa na nu prislo, takze sa zapis zopakuje uz bez nej
            if (retryOnError && _jumpListItemRemoved)
            {
                _jumpListItemRemoved = false;
                RefreshJumpList(false);
                return;
            }

            //zoznam odkazov nie je kriticka funkcia - aplikacia musi bezat aj ked sa ho nepodari vytvorit
            Log.Exception(e);
            _jumpListFailed = true;
        }
    }

    /// <summary>
    ///     Vytvori polozku zoznamu odkazov, ktora spusti aplikaciu s cestou k projektu ako argumentom.
    /// </summary>
    /// <param name="path">Cesta k projektu.</param>
    /// <param name="exePath">Cesta k spustitelnemu suboru aplikacie.</param>
    /// <returns>polozka zoznamu odkazov.</returns>
    private static JumpListLink CreateJumpListLink(string path, string exePath) =>
        new(exePath, path)
        {
            //cesta musi byt v uvodzovkach, inak sa argument s medzerami rozpadne na viacero argumentov
            Arguments = path.Quote(),
            WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
            IconReference = new IconReference(exePath, 0)
        };

    /// <summary>
    ///     Odstrani zo zoznamu poslednych pouzivanych projektov polozky, ktore pouzivatel odstranil
    ///     zo zoznamu odkazov na paneli uloh.
    /// </summary>
    private static void JumpList_ItemsRemoved(object sender, UserRemovedJumpListItemsEventArgs e)
    {
        foreach (var item in e.RemovedItems)
        {
            var path = item switch
            {
                JumpListLink link when !string.IsNullOrWhiteSpace(link.Arguments) => link.Arguments.Trim('"'),
                IJumpListItem jumpListItem => jumpListItem.Path,
                _ => null
            };

            if (RemoveProject(path))
                _jumpListItemRemoved = true;
        }
    }

    /// <summary>
    ///     Prida novu cestu na koniec zoznamu poslednych pouzivanych projektov.<br/>
    ///     Ak kluc v Registry neexistuje, vytvori sa a prida zadanu cestu path.
    /// </summary>
    /// <param name="path">Cesta k projektu.</param>
    public static void SetUsageOfProject(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\{ProductName}");
        if (key == null)
            return;

        var projects = (_projects ?? GetOpenedProjects()).ToList();
        var project = projects.FirstOrDefault(p => p.Path == path);
        if (project is null)
            projects.Add(new ProjectInfo(path, DateTime.Now));
        else
            project.LastAccess = DateTime.Now;

        _projects = projects.ToArray();
        key.SetValue(REG_OPENED_PROJECTS, SerializeProjects(_projects));

        RefreshJumpList();
    }

    /// <summary>
    ///     Odstrani projekt zo zoznamu poslednych pouzivanych projektov.
    /// </summary>
    /// <param name="path">Cesta k projektu.</param>
    /// <returns><see langword="true"/>, ak sa projekt v zozname nachadzal.</returns>
    public static bool RemoveProject(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\{ProductName}");
        if (key is null)
            return false;

        var projects = _projects ?? GetOpenedProjects();
        var rest = projects.Where(p => !string.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (rest.Length == projects.Length)
            return false;

        _projects = rest;
        key.SetValue(REG_OPENED_PROJECTS, SerializeProjects(rest));
        return true;
    }

    /// <summary>
    ///     Skonvertuje zoznam projektov na retazec zapisovany do Registry.
    /// </summary>
    /// <param name="projects">Zoznam projektov.</param>
    /// <returns>retazec v tvare cesta*datum|cesta*datum|...</returns>
    private static string SerializeProjects(IEnumerable<ProjectInfo> projects)
    {
        var sb = new StringBuilder();
        foreach (var project in projects)
        {
            sb.Append(project.Path);
            sb.Append('*');
            sb.Append(project.LastAccess.ToString(CultureInfo.InvariantCulture));
            sb.Append('|');
        }

        return sb.ToString();
    }

    public static string GetLastProject()
    {
        var key = Registry.CurrentUser.OpenSubKey($"SOFTWARE\\{ProductName}");
        var value = key?.GetValue(REG_LAST_PROJECT);
        return value is null ? "" : value.ToString();
    }

    public static void SetLastProject(string path)
    {
        var key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\{ProductName}");
        key?.SetValue(REG_LAST_PROJECT, path);
    }
}

public class ProjectInfo
{
    public string Path { get; }
    public DateTime LastAccess { get; set; }

    public ProjectInfo(string path, DateTime lastAccess)
    {
        Path = path;
        LastAccess = lastAccess;
    }

    public override bool Equals(object obj)
    {
        return obj is ProjectInfo pi && Equals(pi);
    }

    public bool Equals(ProjectInfo other)
    {
        return Path == other.Path;
    }

    public override int GetHashCode()
    {
        return (Path != null ? Path.GetHashCode() : 0);
    }
}