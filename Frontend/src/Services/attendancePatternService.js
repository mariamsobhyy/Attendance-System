import api from "./api";

export const getAttendancePatterns = async () => {
    const response = await api.get("/AttendancePattern");
    return response.data;
};