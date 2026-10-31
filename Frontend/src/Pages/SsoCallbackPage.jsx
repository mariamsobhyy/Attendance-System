import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

/**
 * Landing page for CAS SSO redirect.
 * CAS redirects here with: ?token=<jwt>&role=<role>&name=<name>&email=<email>
 * We store the token and user info in localStorage, then navigate to the app.
 */
const SsoCallbackPage = () => {
  const navigate = useNavigate();
  const [error, setError] = useState("");

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get("token");
    const role = params.get("role");
    const name = params.get("name");
    const email = params.get("email");

    if (!token) {
      setError("Authentication failed: no token received. Redirecting to login...");
      setTimeout(() => navigate("/login", { replace: true }), 2500);
      return;
    }

    try {
      // Base64URL-safe JWT payload parser
      const safeParseJwt = (t) => {
        try {
          const parts = t.split(".");
          if (parts.length !== 3) return null;
          let base64Url = parts[1];
          let base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
          const pad = base64.length % 4;
          if (pad) {
            base64 += "=".repeat(4 - pad);
          }
          const jsonPayload = decodeURIComponent(
            atob(base64)
              .split("")
              .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
              .join("")
          );
          return JSON.parse(jsonPayload);
        } catch (e) {
          console.error("JWT parse error:", e);
          return null;
        }
      };

      const payload = safeParseJwt(token) || {};

      // Flexible extraction of role, user ID, name, and email from claims or query params
      const userRole = (
        role ||
        payload.Role ||
        payload.role ||
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
        payload.roleName ||
        "staff"
      ).toLowerCase().replace(/\s+/g, "");

      const userId =
        payload.AccountId ||
        payload.accountId ||
        payload.NameId ||
        payload.nameid ||
        payload.sub ||
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
        "1";

      const userName =
        name ||
        payload.FullNameEn ||
        payload.fullNameEn ||
        payload.fullName ||
        payload.name ||
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
        email ||
        "User";

      const userEmail =
        email ||
        payload.Email ||
        payload.email ||
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
        "";

      // Store in the same keys the rest of the app already uses
      localStorage.setItem("token", token);
      localStorage.setItem("userRole", userRole);
      localStorage.setItem(
        "user",
        JSON.stringify({
          id: userId,
          name: userName,
          email: userEmail,
          role: userRole,
        })
      );

      // Clean the token out of the address bar
      window.history.replaceState(null, "", window.location.pathname);

      const target = userRole === "student" ? "/profile" : "/dashboard";
      navigate(target, { replace: true });
    } catch (e) {
      console.error("SSO callback error:", e);
      setError("Authentication failed or session expired. Please try signing in again.");
      window.history.replaceState(null, "", window.location.pathname);
    }
  }, [navigate]);

  return (
    <div style={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100vh", flexDirection: "column", gap: "16px" }}>
      {error ? (
        <>
          <p style={{ color: "#dc2626", fontWeight: "500" }}>{error}</p>
          <button
            onClick={() => navigate("/login", { replace: true })}
            style={{
              padding: "8px 20px",
              backgroundColor: "#2563eb",
              color: "#ffffff",
              border: "none",
              borderRadius: "6px",
              cursor: "pointer",
              fontWeight: "600",
              boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
            }}
          >
            Return to Login
          </button>
        </>
      ) : (
        <p style={{ color: "#64748b" }}>Completing sign-in, please wait...</p>
      )}
    </div>
  );
};

export default SsoCallbackPage;
