using AttendanceBehaviour_Backend.Data;
using AttendanceBehaviour_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceBehaviour_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentProfileController : ControllerBase
{
    private readonly ElsewedySchoolContext _context;

    public StudentProfileController(ElsewedySchoolContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns basic profile data for any account by its ID, Email, or National ID.
    /// Used by the Attendance frontend ProfilePage.
    /// </summary>
    [HttpGet("account/{identifier}")]
    public async Task<IActionResult> GetByAccountIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return BadRequest(new { message = "Invalid account identifier." });

        Account? account = null;
        string search = identifier.Trim().ToLower();
        bool isNumeric = long.TryParse(identifier, out long accountId);

        // 1. Try lookup by Account ID if identifier is numeric
        if (isNumeric && accountId > 0)
        {
            account = await _context.Accounts
                .AsNoTracking()
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            // If not found directly by Account.Id, check by Login.Id or Login.AccountId
            if (account == null)
            {
                var login = await _context.Logins
                    .AsNoTracking()
                    .Include(l => l.Account)
                        .ThenInclude(a => a.Role)
                    .FirstOrDefaultAsync(l => l.Id == accountId || l.AccountId == accountId);

                account = login?.Account;
            }
        }

        // 2. Try lookup by Email or National ID
        if (account == null)
        {
            account = await _context.Accounts
                .AsNoTracking()
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Email.ToLower() == search || a.NationalId == identifier);

            if (account == null)
            {
                var login = await _context.Logins
                    .AsNoTracking()
                    .Include(l => l.Account)
                        .ThenInclude(a => a.Role)
                    .FirstOrDefaultAsync(l => l.Email.ToLower() == search);

                account = login?.Account;
            }
        }

        // 3. Fallback: If login exists without account, return login details as profile
        if (account == null)
        {
            var orphanedLogin = await _context.Logins
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Email.ToLower() == search || (isNumeric && l.Id == accountId));

            if (orphanedLogin != null)
            {
                return Ok(new
                {
                    id = orphanedLogin.AccountId > 0 ? orphanedLogin.AccountId : orphanedLogin.Id,
                    profileId = orphanedLogin.Id,
                    name = orphanedLogin.Email.Split('@')[0],
                    nameAr = orphanedLogin.Email.Split('@')[0],
                    email = orphanedLogin.Email,
                    phoneNumber = (string?)null,
                    joinDate = (DateTime?)null,
                    role = "User",
                    isActive = true,
                    imageP = (string?)null
                });
            }

            return NotFound(new { message = "Profile not found." });
        }

        return Ok(new
        {
            id = account.Id,
            profileId = account.Id,
            name = account.FullNameEn,
            nameAr = account.FullNameAr,
            email = account.Email,
            phoneNumber = account.Phone,
            joinDate = account.CreatedAt,
            role = account.Role?.RoleName ?? "User",
            isActive = account.IsActive,
            imageP = (string?)null
        });
    }
}
