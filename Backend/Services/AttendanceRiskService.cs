
using AttendanceBehaviour_Backend.Data;
using AttendanceBehaviour_Backend.DTOs.Attendance;
using Microsoft.EntityFrameworkCore;

namespace AttendanceBehaviour_Backend.Services
{
    public class AttendanceRiskService
    {
        private readonly ElsewedySchoolContext _context;

        public AttendanceRiskService(ElsewedySchoolContext context)
        {
            _context = context;
        }

        // Get risk for one student
        public async Task<AttendanceRiskDto?> GetStudentRiskAsync(long studentId)
        {
            var student = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return null;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var fromDate = today.AddDays(-29);

            var records = await _context.AbsenceRecords
                .AsNoTracking()
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            var absenceRecords = records
                .Where(a => a.AbsenceTypeId == 10 || a.AbsenceTypeId == 20)
                .ToList();

            var lateRecords = records
                .Where(a => a.AbsenceTypeId == 50 || a.AbsenceTypeId == 55)
                .ToList();

            int totalAbsences = absenceRecords
                .Select(a => a.DateOfAbsence)
                .Distinct()
                .Count();

            int totalLate = lateRecords
                .Select(a => a.DateOfAbsence)
                .Distinct()
                .Count();

            int absenceDaysLast30Days = absenceRecords
                .Where(a => a.DateOfAbsence >= fromDate &&
                            a.DateOfAbsence <= today)
                .Select(a => a.DateOfAbsence)
                .Distinct()
                .Count();

            string riskLevel;

            if (records.Count == 0)
                riskLevel = "Insufficient Data";
            else if (absenceDaysLast30Days >= 5)
                riskLevel = "High Risk";
            else if (absenceDaysLast30Days >= 3)
                riskLevel = "Medium Risk";
            else
                riskLevel = "Low Risk";

            return new AttendanceRiskDto
            {
                StudentId = studentId,
                StudentName = student.FullNameEn ?? string.Empty,
                TotalAbsences = totalAbsences,
                TotalLate = totalLate,
                AbsenceDaysLast30Days = absenceDaysLast30Days,
                AttendanceRate = null,
                RiskLevel = riskLevel
            };
        }

        // Get all high and medium risk students
        public async Task<List<AttendanceRiskDto>> GetAtRiskStudentsAsync()
        {
            var studentIds = await _context.StudentExtensions
                .AsNoTracking()
                .Select(s => s.AccountId)
                .Distinct()
                .ToListAsync();

            var results = new List<AttendanceRiskDto>();

            foreach (var studentId in studentIds)
            {
                var risk = await GetStudentRiskAsync(studentId);

                if (risk != null &&
                    (risk.RiskLevel == "High Risk" ||
                     risk.RiskLevel == "Medium Risk"))
                {
                    results.Add(risk);
                }
            }

            return results
                .OrderByDescending(s => s.RiskLevel == "High Risk")
                .ThenByDescending(s => s.AbsenceDaysLast30Days)
                .ToList();
        }

        // Get full risk details for one student
        public async Task<StudentRiskDetailsDto?> GetStudentRiskDetailsAsync(long studentId)
        {
            // Get only the student information we need
            var student = await _context.StudentExtensions
                .AsNoTracking()
                .Where(s => s.AccountId == studentId)
                .Select(s => new
                {
                    StudentId = s.AccountId,
                    StudentNameEn = s.Account.FullNameEn,
                    StudentNameAr = s.Account.FullNameAr,
                    ClassName = s.TblClass != null
                        ? s.TblClass.ClassName
                        : null
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return null;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var fromDate = today.AddDays(-29);

            // Select only the required columns.
            // Do not load the complete Session entity.
            var records = await _context.AbsenceRecords
                .AsNoTracking()
                .Where(a => a.StudentId == studentId)
                .Select(a => new
                {
                    a.Id,
                    a.DateOfAbsence,
                    a.AbsenceTypeId,
                    SessionNo = a.Session != null
                        ? a.Session.SessionNo
                        : null
                })
                .OrderByDescending(a => a.DateOfAbsence)
                .ToListAsync();

            var absenceRecords = records
                .Where(a => a.AbsenceTypeId == 10 ||
                            a.AbsenceTypeId == 20)
                .ToList();

            var lateRecords = records
                .Where(a => a.AbsenceTypeId == 50 ||
                            a.AbsenceTypeId == 55)
                .ToList();

            int totalAbsences = absenceRecords
                .Select(a => a.DateOfAbsence)
                .Distinct()
                .Count();

            int totalLate = lateRecords
                .Select(a => a.DateOfAbsence)
                .Distinct()
                .Count();

            int absenceDaysLast30Days = absenceRecords
                .Where(a => a.DateOfAbsence >= fromDate &&
                            a.DateOfAbsence <= today)
                .Select(a => a.DateOfAbsence)
                .Distinct()
                .Count();

            string riskLevel;
            string riskReason;

            if (records.Count == 0)
            {
                riskLevel = "Insufficient Data";
                riskReason = "There are no attendance records for this student.";
            }
            else if (absenceDaysLast30Days >= 5)
            {
                riskLevel = "High Risk";
                riskReason =
                    $"The student has {absenceDaysLast30Days} absence days in the last 30 days. " +
                    "The high-risk threshold is 5 days.";
            }
            else if (absenceDaysLast30Days >= 3)
            {
                riskLevel = "Medium Risk";
                riskReason =
                    $"The student has {absenceDaysLast30Days} absence days in the last 30 days. " +
                    "The medium-risk threshold is 3 days.";
            }
            else
            {
                riskLevel = "Low Risk";
                riskReason =
                    $"The student has {absenceDaysLast30Days} absence days in the last 30 days.";
            }

            return new StudentRiskDetailsDto
            {
                StudentId = student.StudentId,
                StudentNameEn = student.StudentNameEn ?? string.Empty,
                StudentNameAr = student.StudentNameAr ?? string.Empty,
                ClassName = student.ClassName,

                TotalAbsences = totalAbsences,
                TotalLate = totalLate,
                AbsenceDaysLast30Days = absenceDaysLast30Days,

                RiskLevel = riskLevel,
                RiskReason = riskReason,

                History = records.Select(a => new StudentAbsenceHistoryDto
                {
                    Id = a.Id,
                    Date = a.DateOfAbsence.ToString("yyyy-MM-dd"),
                    SessionNo = a.SessionNo,
                    AbsenceTypeId = a.AbsenceTypeId,

                    Type = a.AbsenceTypeId == 10 || a.AbsenceTypeId == 20
                        ? "Absent"
                        : a.AbsenceTypeId == 50 || a.AbsenceTypeId == 55
                            ? "Late"
                            : "Other"
                }).ToList()
            };
        }
    }
}
