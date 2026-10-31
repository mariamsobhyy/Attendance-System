
import api from "./api";

export const getAtRiskStudents = async () => {
  const response = await api.get(
    "/AttendanceRisk/at-risk-students"
  );

  return response.data;
};

export const getStudentRisk = async (studentId) => {
  const response = await api.get(
    `/AttendanceRisk/student/${studentId}`
  );

  return response.data;
};

// Get full student risk details and attendance history
export const getStudentRiskDetails = async (studentId) => {
  const response = await api.get(
    `/AttendanceRisk/student/${studentId}/details`
  );

  return response.data;
};
