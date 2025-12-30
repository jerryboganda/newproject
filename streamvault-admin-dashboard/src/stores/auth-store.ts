import { create } from "zustand";
import { persist } from "zustand/middleware";

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
  tenantId?: string;
  roles: string[];
  isEmailVerified: boolean;
  twoFactorEnabled: boolean;
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
}

interface AuthActions {
  login: (
    email: string,
    password: string,
    tenantSlug?: string
  ) => Promise<void>;
  register: (
    email: string,
    password: string,
    firstName: string,
    lastName: string,
    tenantSlug?: string
  ) => Promise<void>;
  logout: () => Promise<void>;
  refreshTokens: () => Promise<void>;
  verifyEmail: (userId: string, token: string) => Promise<void>;
  resetPassword: (token: string, newPassword: string) => Promise<void>;
  forgotPassword: (email: string) => Promise<void>;
  enable2FA: () => Promise<void>;
  verify2FA: (code: string) => Promise<void>;
  setTokens: (accessToken: string, refreshToken: string) => void;
  setUser: (user: User) => void;
  clearAuth: () => void;
}

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5001/api/v1";

export const useAuthStore = create<AuthState & AuthActions>()(
  persist(
    (set, get) => ({
      // State
      user: null,
      accessToken: null,
      refreshToken: null,
      isLoading: false,
      isAuthenticated: false,

      // Actions
      setTokens: (accessToken: string, refreshToken: string) => {
        set({ accessToken, refreshToken, isAuthenticated: true });
      },

      setUser: (user: User) => {
        set({ user });
      },

      clearAuth: () => {
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        });
      },

      login: async (email: string, password: string, tenantSlug?: string) => {
        set({ isLoading: true });
        try {
          const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({ email, password, tenantSlug }),
          });

          const data = await response.json();

          if (!response.ok) {
            throw new Error(data.error || "Login failed");
          }

          const { token, user } = data;
          
          // Save token to localStorage
          if (typeof window !== "undefined") {
            localStorage.setItem("auth-token", token);
          }
          
          set({ accessToken: token, isAuthenticated: true, user });
        } catch (error) {
          console.error("Login error:", error);
          throw error;
        } finally {
          set({ isLoading: false });
        }
      },

      register: async (
        email: string,
        password: string,
        firstName: string,
        lastName: string,
        tenantSlug?: string
      ) => {
        set({ isLoading: true });
        try {
          const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              email,
              password,
              firstName,
              lastName,
              tenantSlug,
            }),
          });

          const data = await response.json();

          if (!response.ok) {
            throw new Error(data.error || "Registration failed");
          }

          // Registration might not return tokens if email verification is required
          if (data.accessToken) {
            const { accessToken, refreshToken, user } = data;
            get().setTokens(accessToken, refreshToken);
            get().setUser(user);
          }
        } catch (error) {
          console.error("Registration error:", error);
          throw error;
        } finally {
          set({ isLoading: false });
        }
      },

      logout: async () => {
        try {
          const { accessToken } = get();
          if (accessToken) {
            await fetch(`${API_BASE_URL}/auth/logout`, {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${accessToken}`,
              },
            });
          }
        } catch (error) {
          console.error("Logout error:", error);
        } finally {
          // Remove token from localStorage
          if (typeof window !== "undefined") {
            localStorage.removeItem("auth-token");
          }
          get().clearAuth();
        }
      },

      refreshTokens: async () => {
        const { refreshToken } = get();
        if (!refreshToken) {
          throw new Error("No refresh token available");
        }

        try {
          const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              accessToken: get().accessToken,
              refreshToken,
            }),
          });

          const data = await response.json();

          if (!response.ok) {
            throw new Error(data.error || "Token refresh failed");
          }

          const { accessToken: newAccessToken, refreshToken: newRefreshToken } = data;
          get().setTokens(newAccessToken, newRefreshToken);
        } catch (error) {
          console.error("Token refresh error:", error);
          get().clearAuth();
          throw error;
        }
      },

      verifyEmail: async (userId: string, token: string) => {
        try {
          const response = await fetch(`${API_BASE_URL}/auth/verify-email`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({ userId, token }),
          });

          const data = await response.json();

          if (!response.ok) {
            throw new Error(data.error || "Email verification failed");
          }

          return data;
        } catch (error) {
          console.error("Email verification error:", error);
          throw error;
        }
      },

      forgotPassword: async (email: string) => {
        try {
          const response = await fetch(`${API_BASE_URL}/auth/forgot-password`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({ email }),
          });

          const data = await response.json();

          if (!response.ok) {
            throw new Error(data.error || "Failed to send reset email");
          }

          return data;
        } catch (error) {
          console.error("Forgot password error:", error);
          throw error;
        }
      },

      resetPassword: async (token: string, newPassword: string) => {
        try {
          const response = await fetch(`${API_BASE_URL}/auth/reset-password`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({ token, newPassword }),
          });

          const data = await response.json();

          if (!response.ok) {
            throw new Error(data.error || "Password reset failed");
          }

          return data;
        } catch (error) {
          console.error("Password reset error:", error);
          throw error;
        }
      },

      enable2FA: async () => {
        const { accessToken } = get();
        if (!accessToken) {
          throw new Error("Not authenticated");
        }

        try {
          const response = await fetch(`${API_BASE_URL}/auth/enable-2fa`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${accessToken}`,
            },
          });

          const data = await response.json();

          if (!response.ok) {
            throw new Error(data.error || "Failed to enable 2FA");
          }

          return data;
        } catch (error) {
          console.error("Enable 2FA error:", error);
          throw error;
        }
      },

      verify2FA: async (code: string) => {
        const { accessToken } = get();
        if (!accessToken) {
          throw new Error("Not authenticated");
        }

        try {
          const response = await fetch(`${API_BASE_URL}/auth/verify-2fa`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${accessToken}`,
            },
            body: JSON.stringify({ code }),
          });

          const data = await response.json();

          if (!response.ok) {
            throw new Error(data.error || "2FA verification failed");
          }

          return data;
        } catch (error) {
          console.error("2FA verification error:", error);
          throw error;
        }
      },
    }),
    {
      name: "auth-storage",
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
