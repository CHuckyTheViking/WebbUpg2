#nullable disable


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebbUpg2.Data;
using WebbUpg2.Models;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using System.Web;
using WebbUpg2.Utilities;
using System.Net.Mime;

namespace WebbUpg2.Controllers
{
    public class DataFilesController : Controller
    {
        private readonly WebbUpg2Context _context;
        private readonly long fileSizeLimit = 10 * 100000;
        private readonly string[] whiteListedExtensions = { ".jpg" };

        public DataFilesController(WebbUpg2Context context)
        {
            _context = context;
        }

        // GET: DataFiles
        public async Task<IActionResult> Index()
        {
            return View(await _context.DataFile.ToListAsync());
        }

        [HttpPost]
        [Route(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile()
        {
            var theWebRequest = HttpContext.Request;


             if (!theWebRequest.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(theWebRequest.ContentType, out var theMediaTypeHeader) || 
                string.IsNullOrEmpty(theMediaTypeHeader.Boundary.Value))
            {
                
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(theMediaTypeHeader.Boundary.Value, theWebRequest.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var DoesItHaveContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var theContentDisposition);

                if (DoesItHaveContentDispositionHeader && theContentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(theContentDisposition.FileName.Value))
                {

                    DataFile dataFile = new DataFile();
                    dataFile.UnTrustedName = HttpUtility.HtmlEncode(theContentDisposition.FileName.Value);
                    dataFile.TimeStamp = DateTime.UtcNow;

                    dataFile.Content =
                            await Filehelper.ProcessStreamedFile(section, theContentDisposition,
                                ModelState, whiteListedExtensions, fileSizeLimit);
                    if (dataFile.Content.Length == 0)
                    {
                        return RedirectToAction("Index", "DataFiles");
                    }
                    dataFile.Size = dataFile.Content.Length;

                    await _context.DataFile.AddAsync(dataFile);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "DataFiles");

                }

                section = await reader.ReadNextSectionAsync();
            }

            return BadRequest("No files data in the request.");
        }



        // GET: ApplicationFiles/Download/5
        public async Task<IActionResult> Download(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataFile = await _context.DataFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dataFile == null)
            {
                return NotFound();
            }

            return File(dataFile.Content, MediaTypeNames.Application.Octet, dataFile.UnTrustedName);
        }



        // GET: DataFiles/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataFile = await _context.DataFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dataFile == null)
            {
                return NotFound();
            }

            return View(dataFile);
        }

        // GET: DataFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DataFiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UnTrustedName,TimeStamp,Size,Content")] DataFile dataFile)
        {
            if (ModelState.IsValid)
            {
                dataFile.Id = Guid.NewGuid();
                _context.Add(dataFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dataFile);
        }

        // GET: DataFiles/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataFile = await _context.DataFile.FindAsync(id);
            if (dataFile == null)
            {
                return NotFound();
            }
            return View(dataFile);
        }

        // POST: DataFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,UnTrustedName,TimeStamp,Size,Content")] DataFile dataFile)
        {
            if (id != dataFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dataFile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DataFileExists(dataFile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(dataFile);
        }

        // GET: DataFiles/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataFile = await _context.DataFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dataFile == null)
            {
                return NotFound();
            }

            return View(dataFile);
        }

        // POST: DataFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var dataFile = await _context.DataFile.FindAsync(id);
            _context.DataFile.Remove(dataFile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DataFileExists(Guid id)
        {
            return _context.DataFile.Any(e => e.Id == id);
        }
    }
}
