import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

const CAS_LOGIN_URL = import.meta.env.VITE_CAS_LOGIN_URL || "https://centerlizedlogin.sewedy.com.eg";
const BUSINESS_ENTITY_ID = 2;

const LoginPage = () => {
  const navigate = useNavigate();
  const [redirecting, setRedirecting] = useState(false);

  const redirectToCas = () => {
    setRedirecting(true);
    const callbackUrl = `${window.location.origin}/sso-callback`;
    const casUrl = new URL(CAS_LOGIN_URL);
    casUrl.searchParams.set("redirect", callbackUrl);
    casUrl.searchParams.set("businessEntity", "Attendance");
    casUrl.searchParams.set("businessEntityId", String(BUSINESS_ENTITY_ID));
    window.location.href = casUrl.toString();
  };

  useEffect(() => {
    // If token is passed in query string (e.g. ?token=...), route to /sso-callback
    const params = new URLSearchParams(window.location.search);
    if (params.get("token")) {
      navigate(`/sso-callback${window.location.search}`, { replace: true });
      return;
    }

    // Already authenticated — go straight to the app
    const token = localStorage.getItem("token");
    if (token) {
      const role = (localStorage.getItem("userRole") || "").toLowerCase().replace(/\s+/g, "");
      navigate(role === "student" ? "/profile" : "/dashboard", { replace: true });
      return;
    }

    // Prevent immediate infinite loops if redirect occurred within the last 5 seconds
    const lastAttempt = sessionStorage.getItem("sso_last_redirect");
    const now = Date.now();
    if (lastAttempt && now - parseInt(lastAttempt, 10) < 5000) {
      return;
    }

    sessionStorage.setItem("sso_last_redirect", String(now));
    redirectToCas();
  }, [navigate]);

  return (
    <div style={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100vh", flexDirection: "column", gap: "16px" }}>
      {redirecting ? (
        <p style={{ color: "#64748b" }}>Redirecting to sign-in portal...</p>
      ) : (
        <>
          <p style={{ color: "#334155", fontSize: "16px", fontWeight: "500" }}>Sign in to ElSewedy Attendance System</p>
          <button
            onClick={redirectToCas}
            style={{
              padding: "10px 24px",
              backgroundColor: "#2563eb",
              color: "#ffffff",
              border: "none",
              borderRadius: "8px",
              fontSize: "15px",
              fontWeight: "600",
              cursor: "pointer",
              boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
            }}
          >
            Sign in with CAS SSO
          </button>
        </>
      )}
    </div>
  );
};

export default LoginPage;
