namespace AttendanceBehaviour_Backend.DTOs.Attendance
{
    public class StudentRiskDetailsDto
    {
        public long StudentId { get; set; }
        public string StudentNameEn { get; set; } = string.Empty;
        public string StudentNameAr { get; set; } = string.Empty;
        public string? ClassName { get; set; }

        public int TotalAbsences { get; set; }
        public int TotalLate { get; set; }
        public int AbsenceDaysLast30Days { get; set; }

        public string RiskLevel { get; set; } = string.Empty;
        public string RiskReason { get; set; } = string.Empty;

        public List<StudentAbsenceHistoryDto> History { get; set; } = new();
    }

    public class StudentAbsenceHistoryDto
    {
        public long Id { get; set; }
        public string Date { get; set; } = string.Empty;
        public int? SessionNo { get; set; }
        public int? AbsenceTypeId { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}