
import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";

import LoginPage from "./Pages/LoginPage";
import SsoCallbackPage from "./Pages/SsoCallbackPage";
import DashboardPage from "./Pages/DashboardPage";
import AbsenceViewPage from "./Pages/AbsenceViewPage";
import AbsenceRecordPage from "./Pages/AbsenceRecordPage";
import AttendanceRiskPage from "./Pages/AttendanceRiskPage";
import StudentRiskDetailsPage from "./Pages/StudentRiskDetailsPage";
import ProfilePage from "./Pages/ProfilePage";
import NotificationsPage from "./Pages/NotificationsPage";
import AttendancePatternsPage from "./Pages/AttendancePatternsPage";

import SidebarLayout from "./Components/Layout/SidebarLayout";
import ProtectedRoute from "./Components/Layout/ProtectedRoute";

import "./GlobalStyle.css";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/sso-callback" element={<SsoCallbackPage />} />

        <Route
          element={
            <ProtectedRoute
              allowedRoles={[
                "admin",
                "teacher",
                "engineer",
                "student affair",
                "board",
                "student",
                "superadmin"
              ]}
            />
          }
        >
          <Route element={<SidebarLayout />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/absence-entry" element={<AbsenceRecordPage />} />
            <Route path="/attendance-view" element={<AbsenceViewPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/attendance-risk" element={<AttendanceRiskPage />} />

            <Route
              path="/attendance-risk/student/:studentId"
              element={<StudentRiskDetailsPage />}
            />

            <Route
              path="/attendance-patterns"
              element={<AttendancePatternsPage />}
            />
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </Router>
  );
}

export default App;
