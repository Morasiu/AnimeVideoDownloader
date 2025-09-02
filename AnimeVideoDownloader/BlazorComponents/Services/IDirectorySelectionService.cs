namespace BlazorComponents.Services;

public interface IDirectorySelectionService
{
    /// <summary>
    /// Opens a native Windows directory selection dialog
    /// </summary>
    /// <param name="initialDirectory">The initial directory to show in the dialog</param>
    /// <returns>The selected directory path, or null if the user cancelled</returns>
    string? SelectDirectory(string? initialDirectory = null);
}