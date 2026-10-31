import api from './api';

const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
};

export const staffService = {
  getTodayAttendance: async () => {
    const today = new Date().toISOString().split("T")[0];
    const response = await api.get(`/staff/attendance/${today}`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },

  getDailyReport: async (date = new Date()) => {
    const dateStr = date.toISOString().split("T")[0];
    const response = await api.get(`/staff/daily-report`, {
      params: { date: dateStr },
      headers: getAuthHeaders(),
    });
    return response.data;
  },

  getStaffByDepartment: async (department, date = null) => {
    const params = {};
    if (date) {
      params.date = date.toISOString().split("T")[0];
    }
    const response = await api.get(`/staff/department/${department}`, {
      params,
      headers: getAuthHeaders(),
    });
    return response.data;
  },

  getAllStaff: async () => {
    const response = await api.get(`/staff`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },

  checkInStaff: async (staffData) => {
    const response = await api.post(`/staff/checkin`, staffData, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },

  getStaffById: async (id) => {
    const numericId = parseInt(id, 10);
    const response = await api.get(`/staff/${numericId}`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },

  deleteStaffAttendance: async (id) => {
    const numericId = parseInt(id, 10);
    const response = await api.delete(`/staff/${numericId}`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },

  updateStaffAttendance: async (id, updateData) => {
    const numericId = parseInt(id, 10);
    const response = await api.put(`/staff/${numericId}`, updateData, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },

  getStaffHistory: async (accountId, fromDate = null, toDate = null) => {
    const numericAccountId = parseInt(accountId, 10);
    const params = {};
    if (fromDate) params.fromDate = fromDate.toISOString().split("T")[0];
    if (toDate) params.toDate = toDate.toISOString().split("T")[0];
    const response = await api.get(`/staff/history/${numericAccountId}`, {
      params,
      headers: getAuthHeaders(),
    });
    return response.data;
  },
};
