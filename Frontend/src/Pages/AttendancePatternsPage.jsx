
import { useEffect, useState, useCallback } from "react";
import { getAttendancePatterns } from "../Services/attendancePatternService";
import "./Styles/AttendancePatternsPage.css";

export default function AttendancePatternsPage() {
  const [patterns, setPatterns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadPatterns = useCallback(async () => {
    try {
      setLoading(true);
      setError("");

      const data = await getAttendancePatterns();
      setPatterns(data);
    } catch (err) {
      console.error("Attendance patterns error:", err);
      setError("Failed to load attendance patterns.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadPatterns();
  }, [loadPatterns]);

  const repeatedDays = patterns.filter(
    (pattern) => pattern.patternType === "Repeated Absence Day"
  ).length;

  const repeatedSessions = patterns.filter(
    (pattern) => pattern.patternType === "Repeated Absence Session"
  ).length;

  return (
    <div className="attendance-patterns-page">

      {/* Page Header */}
      <div className="attendance-patterns-header">
        <div className="content-header">
          <h2>Attendance Patterns</h2>
          <p>Students with repeated absence behaviors</p>
        </div>

         <button
    className="card-button"
    onClick={loadPatterns}
    disabled={loading}
>
    {loading ? "Refreshing..." : "Refresh"}
          </button>
      </div>

      {loading ? (
        <div className="attendance-patterns-loading">
          <div className="attendance-patterns-spinner"></div>
          <p>Loading attendance patterns...</p>
        </div>
      ) : error ? (
        <div className="attendance-patterns-error">
          <p>{error}</p>
          <button type="button" onClick={loadPatterns}>
            Try Again
          </button>
        </div>
      ) : (
        <>
          {/* Summary Cards */}
          <div className="attendance-patterns-cards">

            <div className="attendance-patterns-card total-patterns-card">
              <div className="attendance-patterns-card-content">
                <span>Total Patterns</span>
                <h2>{patterns.length}</h2>
                <p>Detected attendance patterns</p>
              </div>
              <div className="attendance-patterns-card-icon">◎</div>
            </div>

            <div className="attendance-patterns-card repeated-days-card">
              <div className="attendance-patterns-card-content">
                <span>Repeated Absence Days</span>
                <h2>{repeatedDays}</h2>
                <p>Students with repeated absence days</p>
              </div>
              <div className="attendance-patterns-card-icon">◷</div>
            </div>

            <div className="attendance-patterns-card repeated-sessions-card">
              <div className="attendance-patterns-card-content">
                <span>Repeated Absence Sessions</span>
                <h2>{repeatedSessions}</h2>
                <p>Students with repeated absence sessions</p>
              </div>
              <div className="attendance-patterns-card-icon">▦</div>
            </div>

          </div>

          {/* Patterns Table */}
          <div className="attendance-patterns-table-section">

            <div className="attendance-patterns-table-header">
              <div>
                <h2>Attendance Patterns</h2>
                <p>Students with repeated absence behaviors</p>
              </div>

              <span className="attendance-patterns-record-count">
                {patterns.length} Patterns
              </span>
            </div>

            <div className="attendance-patterns-table-container">
              <table className="attendance-patterns-table">

                <thead>
                  <tr>
                    <th>Student ID</th>
                    <th>Student Name</th>
                    <th>Pattern Type</th>
                    <th>Description</th>
                    <th>Occurrences</th>
                    <th>Severity</th>
                  </tr>
                </thead>

                <tbody>
                  {patterns.map((pattern, index) => (
                    <tr
                      key={`${pattern.studentId}-${pattern.patternType}-${index}`}
                    >
                      <td>
                        <span className="attendance-patterns-student-id">
                          {pattern.studentId}
                        </span>
                      </td>

                      <td>{pattern.studentName}</td>
                      <td>{pattern.patternType}</td>
                      <td>{pattern.description}</td>
                      <td>{pattern.occurrences}</td>

                      <td>
                        <span className="attendance-patterns-badge">
                          {pattern.severity}
                        </span>
                      </td>
                    </tr>
                  ))}

                  {patterns.length === 0 && (
                    <tr>
                      <td colSpan="6" className="attendance-patterns-empty">
                        No attendance patterns detected.
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
