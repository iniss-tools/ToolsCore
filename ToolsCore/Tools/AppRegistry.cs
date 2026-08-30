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

    private static JumpList _jumpList;

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

    public static void RegisterJumpList()
    {
        _jumpList?.ClearAllUserTasks();

        var projects = GetOpenedProjects();

        foreach (var project in projects)
        {
            var jumpListLink = new JumpListLink(project.Path, project.Path);
            jumpListLink.Arguments = project.Path;
            jumpListLink.IconReference = new IconReference(Assembly.GetEntryAssembly()?.Location, 0);
            _jumpList?.AddUserTasks(jumpListLink);
        }

        _jumpList?.Refresh();
    }

    /// <summary>
    ///     Prida novu cestu na koniec zoznamu poslednych pouzivanych projektov.<br/>
    ///     Ak kluc v Registry neexistuje, vytvori sa a prida zadanu cestu path.
    /// </summary>
    /// <param name="path">Cesta k projektu.</param>
    public static void SetUsageOfProject(string path)
    {
        var key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\{ProductName}");
        if (key == null)
            return;

        var sb = new StringBuilder();
        var projects = _projects ?? GetOpenedProjects();
        var exists = false;
        foreach (var project in projects)
        {
            if (project.Path == path)
            {
                project.LastAccess = DateTime.Now;
                exists = true;
            }

            sb.Append(project.Path);
            sb.Append('*');
            sb.Append(project.LastAccess.ToString(CultureInfo.InvariantCulture));
            sb.Append('|');
        }

        if (!exists)
        {
            sb.Append(path);
            sb.Append('*');
            sb.Append(DateTime.Now.ToString(CultureInfo.InvariantCulture));
            sb.Append('|');
            var jumpListLink = new JumpListLink(path, path);
            jumpListLink.Arguments = path;
            jumpListLink.IconReference = new IconReference(Assembly.GetEntryAssembly()?.Location, 0);
            _jumpList?.AddUserTasks(jumpListLink);
            _jumpList?.Refresh();
        }

        key.SetValue(REG_OPENED_PROJECTS, sb.ToString());
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