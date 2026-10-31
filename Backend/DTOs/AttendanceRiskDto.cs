
namespace AttendanceBehaviour_Backend.DTOs.Attendance
{
    public class AttendanceRiskDto
    {
        public long StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public int TotalAbsences { get; set; }

        public int TotalLate { get; set; }

        public int AbsenceDaysLast30Days { get; set; }

        public double? AttendanceRate { get; set; }

        public string RiskLevel { get; set; } = string.Empty;
    }
}