namespace AttendanceBehaviour_Backend.DTOs.Attendance
{
    public class AttendancePatternDto
    {
        public long StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string PatternType { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Occurrences { get; set; }

        public string Severity { get; set; } = string.Empty;
    }
}