
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

                var mainFolder = _context.Folders.SingleOrDefault(f => f.ParrentFolderId == null);
                if (mainFolder == null)
                {
                    return NotFound();
                }


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

                var selectedFolder = _context.Folders.SingleOrDefault(f => f.Id == id);
                if (selectedFolder == null)
                {
                    return NotFound();
                }


                var childFolders = _context.Folders.Where(f => f.ParrentFolderId == id).ToList();

                var viewModel = new FoldersIndexViewModel
                {
                    MainFolder = selectedFolder,
                    ChildFolders = childFolders
                };

                return View(viewModel);
            }
        }
        public IActionResult ImportExport()
        {
            return View();
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

            var foldersWithoutId = folders.Select(f => new
            {
                f.FolderName,
                f.ParrentFolderId
            });

            var serializedFolders = JsonConvert.SerializeObject(foldersWithoutId);
            var bytes = Encoding.UTF8.GetBytes(serializedFolders);
            return File(new MemoryStream(bytes), "application/json", "folders.json");
        }

        [HttpPost]
        public async Task<IActionResult> ImportFolders()
        {

            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }


            using var streamReader = new StreamReader(file.OpenReadStream());
            var jsonContent = await streamReader.ReadToEndAsync();

            var foldersWithoutId = JsonConvert.DeserializeAnonymousType(jsonContent, new[] { new { FolderName = "", ParrentFolderId = (int?)null } });

            foreach (var folderWithoutId in foldersWithoutId)
            {
  
                var existingFolder = await _context.Folders
                    .Where(f => f.FolderName == folderWithoutId.FolderName && f.ParrentFolderId == folderWithoutId.ParrentFolderId)
                    .FirstOrDefaultAsync();

                if (existingFolder == null)
                {
                   
                    var newFolder = new Folders
                    {
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