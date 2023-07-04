
using Folder.Data;
using Folder.Models;
using Folder.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace Folder.Controllers
{
    public class FoldersController : Controller
    {
        public readonly FolderContext _context;

        public FoldersController(FolderContext context)
        {
            _context = context;
        }


        public IActionResult Index(int? id)
        {
            if (id == null)
            {
                // Якщо id не вказано, знаходимо головну папку (з тим ParrentFolderId, що дорівнює null).
                var mainFolder = _context.Folders.SingleOrDefault(f => f.ParrentFolderId == null);
                if (mainFolder == null)
                {
                    return NotFound();
                }

                // Знаходимо дочірні папки для головної папки.
                var childFolders = _context.Folders.Where(f => f.ParrentFolderId == mainFolder.Id).ToList();

                var viewModel = new FoldersIndexViewModel
                {
                    MainFolder = mainFolder,
                    ChildFolders = childFolders
                };

                return View(viewModel);
            }
            else
            {
                // Якщо вказано id папки, знаходимо папку за цим id.
                var selectedFolder = _context.Folders.SingleOrDefault(f => f.Id == id);
                if (selectedFolder == null)
                {
                    return NotFound();
                }

                // Знаходимо дочірні папки для вказаної папки.
                var childFolders = _context.Folders.Where(f => f.ParrentFolderId == id).ToList();

                var viewModel = new FoldersIndexViewModel
                {
                    MainFolder = selectedFolder,
                    ChildFolders = childFolders
                };

                return View(viewModel);
            }
        }


        public IActionResult CreateFolders()
        {
            var viewModel = new FoldersViewModel();
            var parentFolders = _context.Folders.Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.FolderName
            }).ToList();
            parentFolders.Insert(0, new SelectListItem
            {
                Value = null,
                Text = "Select Parent Folder"
            });
            viewModel.ParentFolderOptions = parentFolders;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFolders(FoldersViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var folder = new Folders
                {

                    FolderName = viewModel.FolderName
                };

                if (viewModel.ParrentFolderId.HasValue)
                {
                    folder.ParrentFolderId = viewModel.ParrentFolderId;
                }

                _context.Folders.Add(folder);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(viewModel);
        }


        public async Task<IActionResult> ExportFolders()
        {
            var folders = await _context.Folders.ToListAsync();
            var serializedFolders = JsonConvert.SerializeObject(folders);
            var bytes = Encoding.UTF8.GetBytes(serializedFolders);
            return File(new MemoryStream(bytes), "application/json", "folders.json");
        }

        [HttpPost]
        public async Task<IActionResult> ImportFolders()
        {
            // Get the uploaded JSON file
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Read the JSON content from the uploaded file
            using var streamReader = new StreamReader(file.OpenReadStream());
            var jsonContent = await streamReader.ReadToEndAsync();

            // Deserialize the JSON content to a list of Folders without "Id" field
            var foldersWithoutId = JsonConvert.DeserializeObject<List<Folders>>(jsonContent);

            foreach (var folderWithoutId in foldersWithoutId)
            {
                // Check if the folder with the given Id already exists in the database
                var existingFolder = await _context.Folders.FindAsync(folderWithoutId.Id);
                if (existingFolder != null)
                {
                    // Update the existing folder with the new data from the JSON file
                    existingFolder.FolderName = folderWithoutId.FolderName;
                    existingFolder.ParrentFolderId = folderWithoutId.ParrentFolderId;
                }
                else
                {
                    // If the folder with the given Id does not exist, create a new folder
                    var newFolder = new Folders
                    {
                        Id = folderWithoutId.Id,
                        FolderName = folderWithoutId.FolderName,
                        ParrentFolderId = folderWithoutId.ParrentFolderId
                    };
                    _context.Folders.Add(newFolder);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }



    }
}