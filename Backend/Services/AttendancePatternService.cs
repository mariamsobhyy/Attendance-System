
using AttendanceBehaviour_Backend.Data;
using AttendanceBehaviour_Backend.DTOs.Attendance;
using Microsoft.EntityFrameworkCore;

namespace AttendanceBehaviour_Backend.Services
{
    public class AttendancePatternService
    {
        private readonly ElsewedySchoolContext _context;

        public AttendancePatternService(ElsewedySchoolContext context)
        {
            _context = context;
        }

        // 1. Detect repeated absence on the same weekday
        public async Task<List<AttendancePatternDto>> GetRepeatedAbsenceDaysAsync()
        {
            var records = await _context.AbsenceRecords
                .AsNoTracking()
                .Where(a => a.AbsenceTypeId == 10 ||
                            a.AbsenceTypeId == 20)
                .Select(a => new
                {
                    a.StudentId,
                    StudentName = a.Studentextension!.Account.FullNameEn,
                    a.DateOfAbsence
                })
                .ToListAsync();

            var patterns = records
                .GroupBy(a => new
                {
                    a.StudentId,
                    a.StudentName,
                    Day = a.DateOfAbsence.DayOfWeek
                })
                .Where(g => g.Select(a => a.DateOfAbsence)
                             .Distinct()
                             .Count() >= 3)
                .Select(g => new AttendancePatternDto
                {
                    StudentId = g.Key.StudentId,
                    StudentName = g.Key.StudentName ?? string.Empty,
                    PatternType = "Repeated Absence Day",
                    Description = $"Frequently absent on {g.Key.Day}",
                    Occurrences = g.Select(a => a.DateOfAbsence)
                                   .Distinct()
                                   .Count(),
                    Severity = "Medium"
                })
                .ToList();

            return patterns;
        }

        // 2. Detect repeated absence in the same session
        public async Task<List<AttendancePatternDto>> GetRepeatedAbsenceSessionsAsync()
        {
            var records = await _context.AbsenceRecords
                .AsNoTracking()
                .Where(a => (a.AbsenceTypeId == 10 ||
                             a.AbsenceTypeId == 20) &&
                            a.SessionId != null)
                .Select(a => new
                {
                    a.StudentId,
                    StudentName = a.Studentextension!.Account.FullNameEn,
                    a.DateOfAbsence,
                    SessionNo = a.Session!.SessionNo
                })
                .ToListAsync();

            var patterns = records
                .Where(a => a.SessionNo != null)
                .GroupBy(a => new
                {
                    a.StudentId,
                    a.StudentName,
                    SessionNo = a.SessionNo
                })
                .Where(g => g.Select(a => a.DateOfAbsence)
                             .Distinct()
                             .Count() >= 3)
                .Select(g => new AttendancePatternDto
                {
                    StudentId = g.Key.StudentId,
                    StudentName = g.Key.StudentName ?? string.Empty,
                    PatternType = "Repeated Absence Session",
                    Description = $"Frequently absent in Session {g.Key.SessionNo}",
                    Occurrences = g.Select(a => a.DateOfAbsence)
                                   .Distinct()
                                   .Count(),
                    Severity = "Medium"
                })
                .ToList();

            return patterns;
        }

        // 3. Get all detected patterns
        public async Task<List<AttendancePatternDto>> GetAllPatternsAsync()
        {
            var dayPatterns = await GetRepeatedAbsenceDaysAsync();

            var sessionPatterns = await GetRepeatedAbsenceSessionsAsync();

            var allPatterns = dayPatterns
                .Concat(sessionPatterns)
                .OrderByDescending(p => p.Occurrences)
                .ToList();

            return allPatterns;
        }
    }
}
