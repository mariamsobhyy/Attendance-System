
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getStudentRiskDetails } from "../Services/attendanceRiskService";
import "./Styles/StudentRiskDetailsPage.css";

export default function StudentRiskDetailsPage() {
  const { studentId } = useParams();
  const navigate = useNavigate();

  const [student, setStudent] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadStudentDetails() {
      try {
        setLoading(true);
        setError("");

        const data = await getStudentRiskDetails(studentId);
        setStudent(data);
      } catch (err) {
        console.error("Student risk details error:", err);
        setError("Failed to load student details.");
      } finally {
        setLoading(false);
      }
    }

    loadStudentDetails();
  }, [studentId]);

  if (loading) {
    return (
      <div className="student-risk-page">
        <p>Loading student details...</p>
      </div>
    );
  }

  if (error || !student) {
    return (
      <div className="student-risk-page">
        <button
          className="student-risk-back"
          onClick={() => navigate("/attendance-risk")}
        >
          Back to Attendance Risk
        </button>

        <p className="student-risk-error">
          {error || "Student not found."}
        </p>
      </div>
    );
  }

  const riskClass =
    student.riskLevel === "High Risk"
      ? "high"
      : student.riskLevel === "Medium Risk"
        ? "medium"
        : "low";

  return (
    <div className="student-risk-page">

      {/* Header */}
      <div className="student-risk-header">
        <div>
          <button
            className="student-risk-back"
            onClick={() => navigate("/attendance-risk")}
          >
            ← Back to Attendance Risk
          </button>

          <h1>Student Risk Details</h1>
          <p>Student attendance information and risk analysis</p>
        </div>
      </div>

      {/* Student Information */}
      <div className="student-risk-info">
        <div>
          <span>Student Name (English)</span>
          <h2>{student.studentNameEn}</h2>
        </div>

        <div>
          <span>Student Name (Arabic)</span>
          <h2>{student.studentNameAr}</h2>
        </div>

        <div>
          <span>Student ID</span>
          <h2>{student.studentId}</h2>
        </div>

        <div>
          <span>Class</span>
          <h2>{student.className || "Not assigned"}</h2>
        </div>
      </div>

      {/* Risk Information */}
      <div className="student-risk-analysis">
        <div className="student-risk-level">
          <span>Risk Level</span>

          <span className={`student-risk-badge ${riskClass}`}>
            {student.riskLevel}
          </span>
        </div>

        <div className="student-risk-reason">
          <h3>Why is this student classified as {student.riskLevel}?</h3>
          <p>{student.riskReason}</p>
        </div>
      </div>

      {/* Statistics */}
      <div className="student-risk-cards">
        <div className="student-risk-card">
          <span>Total Absences</span>
          <h2>{student.totalAbsences}</h2>
        </div>

        <div className="student-risk-card">
          <span>Total Late</span>
          <h2>{student.totalLate}</h2>
        </div>

        <div className="student-risk-card">
          <span>Absence Days - Last 30 Days</span>
          <h2>{student.absenceDaysLast30Days}</h2>
        </div>
      </div>

      {/* Attendance History */}
      <div className="student-risk-history">
        <div className="student-risk-section-header">
          <h2>Attendance History</h2>
          <span>
            {student.history?.length || 0} Records
          </span>
        </div>

        <div className="student-risk-table-container">
          <table className="student-risk-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Session</th>
                <th>Type</th>
              </tr>
            </thead>

            <tbody>
              {student.history?.map((record) => (
                <tr key={record.id}>
                  <td>{record.date}</td>
                  <td>{record.sessionNo ?? "-"}</td>
                  <td>
                    <span
                      className={
                        record.type === "Absent"
                          ? "student-history-badge absent"
                          : record.type === "Late"
                            ? "student-history-badge late"
                            : "student-history-badge other"
                      }
                    >
                      {record.type}
                    </span>
                  </td>
                </tr>
              ))}

              {(!student.history || student.history.length === 0) && (
                <tr>
                  <td colSpan="3" className="student-risk-empty">
                    No attendance history found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

    </div>
  );
}
