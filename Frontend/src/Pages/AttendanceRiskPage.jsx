
import { useEffect, useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { getAtRiskStudents } from "../Services/attendanceRiskService";
import "./Styles/AttendanceRiskPage.css";

export default function AttendanceRiskPage() {
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const navigate = useNavigate();

  const loadStudents = useCallback(async () => {
    try {
      setLoading(true);
      setError("");

      const data = await getAtRiskStudents();
      setStudents(data);
    } catch (err) {
      console.error("Attendance risk error:", err);
      setError("Failed to load attendance risk students.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadStudents();
  }, [loadStudents]);

  const highRisk = students.filter(
    (student) => student.riskLevel === "High Risk"
  ).length;

  const mediumRisk = students.filter(
    (student) => student.riskLevel === "Medium Risk"
  ).length;

  return (
    <div className="attendance-risk-page">

      {/* Page Header */}
      <div className="attendance-risk-header">
        <div className="content-header">
          <h2>Attendance Risk Prediction</h2>
          <p>Students who need attendance follow-up</p>
        </div>

   <button
  type="button"
  className="attendance-risk-refresh"
  onClick={loadStudents}
  disabled={loading}
>
  {loading ? "Refreshing..." : "Refresh"}
</button>
      </div>

      {loading ? (
        <div className="attendance-risk-loading">
          <div className="attendance-risk-spinner"></div>
          <p>Loading attendance risk data...</p>
        </div>
      ) : error ? (
        <div className="attendance-risk-error">
          <p>{error}</p>
          <button type="button" onClick={loadStudents}>
            Try Again
          </button>
        </div>
      ) : (
        <>
          {/* Summary Cards */}
          <div className="attendance-risk-cards">

            <div className="attendance-risk-card high-card">
              <div className="attendance-risk-card-content">
                <span>High Risk Students</span>
                <h2>{highRisk}</h2>
                <p>Require immediate follow-up</p>
              </div>

              <div className="attendance-risk-card-icon">!</div>
            </div>

            <div className="attendance-risk-card medium-card">
              <div className="attendance-risk-card-content">
                <span>Medium Risk Students</span>
                <h2>{mediumRisk}</h2>
                <p>Need attendance monitoring</p>
              </div>

              <div className="attendance-risk-card-icon">◷</div>
            </div>

            <div className="attendance-risk-card total-card">
              <div className="attendance-risk-card-content">
                <span>Total At-Risk Students</span>
                <h2>{students.length}</h2>
                <p>Students requiring attention</p>
              </div>

              <div className="attendance-risk-card-icon">◎</div>
            </div>

          </div>

          {/* Students Table */}
          <div className="attendance-risk-table-section">

            <div className="attendance-risk-table-header">
              <div>
                <h2>At-Risk Students</h2>
                <p>Attendance records and risk classification</p>
              </div>

              <span className="attendance-risk-record-count">
                {students.length} Students
              </span>
            </div>

            <div className="attendance-risk-table-container">
              <table className="attendance-risk-table">

                <thead>
                  <tr>
                    <th>Student ID</th>
                    <th>Student Name</th>
                    <th>Total Absences</th>
                    <th>Total Late</th>
                    <th>Absences - Last 30 Days</th>
                    <th>Risk Level</th>
                  </tr>
                </thead>

                <tbody>
                  {students.map((student) => (
                    <tr key={student.studentId}>

                      <td>
                        <span className="attendance-risk-student-id">
                          {student.studentId}
                        </span>
                      </td>

                      <td>
                        <button
                          type="button"
                          className="attendance-risk-student-link"
                          onClick={() =>
                            navigate(
                              `/attendance-risk/student/${student.studentId}`
                            )
                          }
                        >
                          {student.studentName}
                        </button>
                      </td>

                      <td>{student.totalAbsences}</td>
                      <td>{student.totalLate}</td>
                      <td>{student.absenceDaysLast30Days}</td>

                      <td>
                        <span
                          className={
                            student.riskLevel === "High Risk"
                              ? "attendance-risk-badge high"
                              : "attendance-risk-badge medium"
                          }
                        >
                          {student.riskLevel}
                        </span>
                      </td>

                    </tr>
                  ))}

                  {students.length === 0 && (
                    <tr>
                      <td colSpan="6" className="attendance-risk-empty">
                        No at-risk students found.
                      </td>
                    </tr>
                  )}

                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
