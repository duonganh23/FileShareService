using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileShareService.Data;
using FileShareService.Models;
using Microsoft.AspNetCore.Identity;

namespace FileShareService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FilesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        private readonly PasswordHasher<FileRecord> _passwordHasher = new();
        // POST api/files — upload a file
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int maxDownloads = 0, string? expiresAt = null, string? password = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");
            DateTime? parsedExpiry = null;
            if (!string.IsNullOrEmpty(expiresAt))
            {
                if (!DateTime.TryParse(expiresAt, out var dt))
                    return BadRequest("Invalid expiresAt date format.");
                parsedExpiry = dt.ToUniversalTime();
            }
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var storedFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var storagePath = Path.Combine(uploadsFolder, storedFileName);
            using (var stream = new FileStream(storagePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var record = new FileRecord
            {
                Code = Guid.NewGuid().ToString("N").Substring(0, 8),
                OriginalFileName = file.FileName,
                MimeType = file.ContentType,
                SizeBytes = file.Length,
                StoragePath = storagePath,
                DownloadCount = 0,
                MaxDownloads = maxDownloads,
                ExpiresAt = parsedExpiry,
                CreatedAt = DateTime.UtcNow
            };
            if (!string.IsNullOrEmpty(password))
            {
                record.PasswordHash = _passwordHasher.HashPassword(record, password);
            }
            _context.Files.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                record.Code,
                record.OriginalFileName,
                record.MimeType,
                record.SizeBytes,
                record.MaxDownloads,
                record.ExpiresAt,
                record.CreatedAt,
                HasPassword = record.PasswordHash != null
            });
        }

        // GET api/files — list all uploaded files
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var files = await _context.Files
                .Select(f => new
                {
                    f.Code,
                    f.OriginalFileName,
                    f.MimeType,
                    f.SizeBytes,
                    f.DownloadCount,
                    f.MaxDownloads,
                    f.ExpiresAt,
                    f.CreatedAt
                })
                .ToListAsync();

            return Ok(files);
        }

        // GET api/files/{code}/info — get metadata for a file
        [HttpGet("{code}/info")]
        public async Task<IActionResult> GetInfo(string code, [FromQuery] string? password = null)
        {
            var record = await _context.Files.FirstOrDefaultAsync(f => f.Code == code);

            if (record == null)
                return NotFound("File not found.");

            if (record.ExpiresAt.HasValue && record.ExpiresAt < DateTime.UtcNow)
                return Gone("File has expired.");

            if (record.MaxDownloads > 0 && record.DownloadCount >= record.MaxDownloads)
                return Gone("Download limit reached.");

            if (record.PasswordHash != null)
            {
                if (string.IsNullOrEmpty(password))
                    return Unauthorized("Password required.");

                var result = _passwordHasher.VerifyHashedPassword(record, record.PasswordHash, password);
                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized("Incorrect password.");
            }
            return Ok(new
            {
                record.Code,
                record.OriginalFileName,
                record.MimeType,
                record.SizeBytes,
                record.DownloadCount,
                record.MaxDownloads,
                record.ExpiresAt,
                record.CreatedAt,
                HasPassword = record.PasswordHash != null
            });
        }

        // GET api/files/{code} — download a file
        [HttpGet("{code}")]
        public async Task<IActionResult> Download(string code, [FromQuery] string? password = null)
        {
            var record = await _context.Files.FirstOrDefaultAsync(f => f.Code == code);

            if (record == null)
                return NotFound("File not found.");

            if (record.ExpiresAt.HasValue && record.ExpiresAt < DateTime.UtcNow)
                return Gone("File has expired.");

            if (record.MaxDownloads > 0 && record.DownloadCount >= record.MaxDownloads)
                return Gone("Download limit reached.");

            if (record.PasswordHash != null)
            {
                if (string.IsNullOrEmpty(password))
                    return Unauthorized("Password required.");

                var result = _passwordHasher.VerifyHashedPassword(record, record.PasswordHash, password);
                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized("Incorrect password.");
            }

            if (!System.IO.File.Exists(record.StoragePath))
                return NotFound("File missing from storage.");

            record.DownloadCount++;
            await _context.SaveChangesAsync();

            var stream = System.IO.File.OpenRead(record.StoragePath);
            return File(stream, record.MimeType, record.OriginalFileName);
        }

        // DELETE api/files/{code}
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var record = await _context.Files.FirstOrDefaultAsync(f => f.Code == code);

            if (record == null)
                return NotFound("File not found.");

            if (System.IO.File.Exists(record.StoragePath))
                System.IO.File.Delete(record.StoragePath);

            _context.Files.Remove(record);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private ObjectResult Gone(string message)
        {
            return StatusCode(410, message);
        }
    }
}