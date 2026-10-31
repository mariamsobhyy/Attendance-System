// --- File: Repos/AuthService.cs ---

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AttendanceBehaviour_Backend.Data;
using AttendanceBehaviour_Backend.DTOs;
using AttendanceBehaviour_Backend.Interfaces;
using AttendanceBehaviour_Backend.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace AttendanceBehaviour_Backend.Repos
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly EmailService _emailService;
        private readonly ElsewedySchoolContext _context;

        public AuthService(IUserRepository userRepository, IJwtService jwtService, EmailService emailService, ElsewedySchoolContext dbContext)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _context = dbContext;
        }

        public async Task<object> CheckUserAsync(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
            {
                return new { Status = "InvalidInput" };
            }

            // Check if login already exists (use consistent name field)
            var loginss = await _context.Logins
                .Include(l => l.Account)
                .FirstOrDefaultAsync(l => l.Account.NationalId == nationalId);

            if (loginss != null)
            {
                return new
                {
                    Status = "LoginAccountExists",
                    FullName = loginss.Account.FullNameEn,
                    Email = loginss.Email,
                    Password = loginss.PasswordHash
                };
            }

            // Check if account exists without login
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.NationalId == nationalId);

            if (account != null)
            {
                var loginExists = await _context.Logins.AnyAsync(l => l.AccountId == account.Id);

                if (!loginExists)
                {
                    var generatedEmail = GenerateEmail(account.FullNameEn, account.NationalId);

                    if (string.IsNullOrEmpty(account.Email))
                    {
                        account.Email = generatedEmail;
                        await _context.SaveChangesAsync();
                    }

                    return new
                    {
                        Status = "AccountExistsNoLogin",
                        Email = account.Email
                    };
                }

                return new
                {
                    Status = "AccountExists",
                    Email = account.Email
                };
            }

            return new
            {
                Status = "NotFound"
            };
        }

        public async Task<string?> SignupAsync(RegisterDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return "no data.";
                }
                else if (string.IsNullOrWhiteSpace(dto.Email))
                {

                    return "bad email.";

                }
                else if (string.IsNullOrWhiteSpace(dto.Password))
                {
                    return "bad pass.";

                }
                else if (string.IsNullOrWhiteSpace(dto.NationalId))
                {
                    return "bad ud.";

                }

                // Check if email already exists in Login table
                var existingLogin = await _context.Logins
                    .FirstOrDefaultAsync(l => l.Email.ToLower() == dto.Email.ToLower());

                if (existingLogin != null)
                {
                    return "Email already registered.";
                }

                // Check if email already exists in Account table
                var existingAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Email.ToLower() == dto.Email.ToLower());

                if (existingAccount != null)
                {
                    return "Email already exists in system.";
                }

                // Check if National ID already exists
                var existingNationalId = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.NationalId == dto.NationalId);

                if (existingNationalId != null)
                {
                    return "National ID already registered.";
                }

                // Validate role exists
                var role = await _context.Roles.FindAsync(dto.RoleId);
                if (role == null)
                {
                    return "Invalid role specified.";
                }

                // Create new Account
                var newAccount = new Account
                {
                    FullNameEn = dto.FullName,
                    FullNameAr = dto.FullName, // Using same name for both languages
                    Email = dto.Email.ToLower(),
                    RoleId = dto.RoleId,
                    NationalId = dto.NationalId, // Use provided National ID
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    IsActive = true,
                    StatusId = 1, // Assuming 1 is active status
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    Phone = dto.Phone // Save phone number to Account entity
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                // Create corresponding Login record
                var newLogin = new Login
                {
                    AccountId = newAccount.Id,
                    Email = dto.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    StatusId = 1 // Assuming 1 is active status
                };

                _context.Logins.Add(newLogin);
                await _context.SaveChangesAsync();

                return "Registration successful.";
            }
            catch (Exception ex)
            {
                return $"Registration failed: {ex.Message}";
            }
        }

        // NEW: Signup method that also returns the account ID
        public async Task<(string?, long?)> SignupAsyncWithId(RegisterDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return ("no data.", null);
                }
                else if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    return ("bad email.", null);
                }
                else if (string.IsNullOrWhiteSpace(dto.Password))
                {
                    return ("bad pass.", null);
                }
                else if (string.IsNullOrWhiteSpace(dto.NationalId))
                {
                    return ("bad ud.", null);
                }

                // Check if email already exists in Login table
                var existingLogin = await _context.Logins
                    .FirstOrDefaultAsync(l => l.Email.ToLower() == dto.Email.ToLower());

                if (existingLogin != null)
                {
                    return ("Email already registered.", null);
                }

                // Check if email already exists in Account table
                var existingAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Email.ToLower() == dto.Email.ToLower());

                if (existingAccount != null)
                {
                    return ("Email already exists in system.", null);
                }

                // Check if National ID already exists
                var existingNationalId = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.NationalId == dto.NationalId);

                if (existingNationalId != null)
                {
                    return ("National ID already registered.", null);
                }

                // Validate role exists
                var role = await _context.Roles.FindAsync(dto.RoleId);
                if (role == null)
                {
                    return ("Invalid role specified.", null);
                }

                // Create new Account
                var newAccount = new Account
                {
                    FullNameEn = dto.FullName,
                    FullNameAr = dto.FullName, // Using same name for both languages
                    Email = dto.Email.ToLower(),
                    RoleId = dto.RoleId,
                    NationalId = dto.NationalId, // Use provided National ID
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    IsActive = true,
                    StatusId = 1, // Assuming 1 is active status
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                    Phone = dto.Phone // Save phone number to Account entity
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                // Create corresponding Login record
                var newLogin = new Login
                {
                    AccountId = newAccount.Id,
                    Email = dto.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    StatusId = 1 // Assuming 1 is active status
                };

                _context.Logins.Add(newLogin);
                await _context.SaveChangesAsync();

                return ("Registration successful.", newAccount.Id);
            }
            catch (Exception ex)
            {
                return ($"Registration failed: {ex.Message}", null);
            }
        }

        private string GenerateNationalId()
        {
            // Generate a unique national ID (you can modify this logic as needed)
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            var random = new Random().Next(1000, 9999);
            return $"{timestamp}{random}".Substring(0, 14); // Ensure it's not too long
        }

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return "Invalid login data.";
                }

                string searchEmail = dto.Email.Trim().ToLower();

                // 1. Look up login record by email, safely including Account, Role, and AccountRoles
                var login = await _context.Logins
                    .Include(l => l.Account)
                        .ThenInclude(a => a.Role)
                    .Include(l => l.Account)
                        .ThenInclude(a => a.AccountRoles)
                            .ThenInclude(ar => ar.Roles)
                    .FirstOrDefaultAsync(l => l.Email.ToLower() == searchEmail);

                Account? account = login?.Account;

                // 2. Fallback: If no record in Logins table, search Accounts table directly
                if (account == null)
                {
                    account = await _context.Accounts
                        .Include(a => a.Role)
                        .Include(a => a.AccountRoles)
                            .ThenInclude(ar => ar.Roles)
                        .FirstOrDefaultAsync(a => a.Email.ToLower() == searchEmail);

                    if (account == null)
                    {
                        return "Email not registered.";
                    }
                }

                // 3. Obtain the password hash from Login or Account
                string dbHash = login?.PasswordHash ?? account.PasswordHash ?? "";
                if (string.IsNullOrEmpty(dbHash))
                {
                    return "Incorrect password.";
                }

                bool isPasswordValid = false;
                string passInput = dto.Password;
                string passTrimmed = passInput.Trim();

                // Check if stored password is in BCrypt format
                if (dbHash.StartsWith("$2a$") || dbHash.StartsWith("$2b$") || dbHash.StartsWith("$2y$"))
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(passInput, dbHash) ||
                                       BCrypt.Net.BCrypt.Verify(passTrimmed, dbHash);

                    // If BCrypt verification failed, check casing variations (e.g., Alaaabdelrahman@123 vs AlaaAbdelrahman@123)
                    if (!isPasswordValid && !string.IsNullOrEmpty(account.Email))
                    {
                        var emailUsername = account.Email.Split('@')[0];
                        if (!string.IsNullOrEmpty(emailUsername))
                        {
                            // Try substituting username portion with exact email casing or vice versa
                            string altPassword1 = passInput.Replace(emailUsername.ToLower(), emailUsername);
                            if (altPassword1 != passInput && BCrypt.Net.BCrypt.Verify(altPassword1, dbHash))
                            {
                                isPasswordValid = true;
                            }
                            else
                            {
                                string altPassword2 = passInput.Replace(emailUsername, emailUsername.ToLower());
                                if (altPassword2 != passInput && BCrypt.Net.BCrypt.Verify(altPassword2, dbHash))
                                {
                                    isPasswordValid = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Legacy plain text check
                    isPasswordValid = (passInput == dbHash) ||
                                      (passTrimmed == dbHash) ||
                                      (passInput.Equals(dbHash, StringComparison.OrdinalIgnoreCase));
                }

                if (!isPasswordValid)
                    return "Incorrect password.";

                // 4. Generate JWT Token safely
                var token = _jwtService.GenerateToken(account);
                return token;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GenerateEmail(string fullName, string nationalId)
        {
            var names = fullName.Trim().ToLower().Split(' ');

            var first = names.Length > 0 ? names[0] : "user";
            var second = names.Length > 1 ? names[1] : "name";

            var last6 = nationalId.Length >= 6 ? nationalId.Substring(nationalId.Length - 6) : "000000";

            return $"{first}.{second}.{last6}@gmail.com";
        }


        // --- END: FULLY CORRECTED LOGIN METHOD ---

        public async Task<string?> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userRepository.GetUserByEmailAsync(dto.Email.ToLower());
            if (user == null) return "Email not registered.";
            var token = Guid.NewGuid().ToString();
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _userRepository.SaveChangesAsync();
            await _emailService.SendEmailAsync(dto.Email, "Reset Password", $"Your token is: {token}");
            // Return the token in the response for development/testing convenience
            return token;
        }

        public async Task<string?> ResetPasswordAsync(string token, ResetPasswordDto dto)
        {
            var user = await _userRepository.GetUserByResetTokenAsync(token);
            if (user == null) return "Invalid or expired token.";
            if (dto.NewPassword != dto.ConfirmPassword) return "Passwords do not match.";
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _userRepository.SaveChangesAsync();
            return "Password changed successfully.";
        }
    }
}

