using Microsoft.AspNetCore.Mvc.Rendering;

namespace Folder.Models.ViewModels
{
    public class FoldersViewModel
    {
        public string? FolderName { get; set; }

        public int? ParrentFolderId { get; set; }

        public List<SelectListItem>? ParentFolderOptions { get; set; }

    }
}
