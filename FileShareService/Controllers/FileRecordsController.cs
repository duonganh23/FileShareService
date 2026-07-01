
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileShareService.Models;
using FileShareService.Data;

public class FileRecordsController : Controller
{
    private readonly AppDbContext _context;

    public FileRecordsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: FILERECORDS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Files.ToListAsync());
    }

    // GET: FILERECORDS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var filerecord = await _context.Files
            .FirstOrDefaultAsync(m => m.Id == id);
        if (filerecord == null)
        {
            return NotFound();
        }

        return View(filerecord);
    }

    // GET: FILERECORDS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: FILERECORDS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Code,OriginalFileName,MimeType,SizeBytes,StoragePath,DownloadCount,MaxDownloads,ExpiresAt,CreatedAt")] FileRecord filerecord)
    {
        if (ModelState.IsValid)
        {
            _context.Add(filerecord);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(filerecord);
    }

    // GET: FILERECORDS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var filerecord = await _context.Files.FindAsync(id);
        if (filerecord == null)
        {
            return NotFound();
        }
        return View(filerecord);
    }

    // POST: FILERECORDS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Code,OriginalFileName,MimeType,SizeBytes,StoragePath,DownloadCount,MaxDownloads,ExpiresAt,CreatedAt")] FileRecord filerecord)
    {
        if (id != filerecord.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(filerecord);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FileRecordExists(filerecord.Id))
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
        return View(filerecord);
    }

    // GET: FILERECORDS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var filerecord = await _context.Files
            .FirstOrDefaultAsync(m => m.Id == id);
        if (filerecord == null)
        {
            return NotFound();
        }

        return View(filerecord);
    }

    // POST: FILERECORDS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var filerecord = await _context.Files.FindAsync(id);
        if (filerecord != null)
        {
            _context.Files.Remove(filerecord);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FileRecordExists(int? id)
    {
        return _context.Files.Any(e => e.Id == id);
    }
}
