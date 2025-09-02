
namespace BlazorComponents.Services;

public sealed class DirectorySelectionService : IDirectorySelectionService
{
    public string? SelectDirectory(string? initialDirectory = null)
    {
        using var dialog = new FolderBrowserDialog();
        if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }
        dialog.Description = "Select a directory for anime downloads";
        dialog.UseDescriptionForTitle = true;
        dialog.ShowNewFolderButton = true;
        var result = dialog.ShowDialog();
        if (result == DialogResult.OK)
        {
            return dialog.SelectedPath;
        }
        return null;
    }
}