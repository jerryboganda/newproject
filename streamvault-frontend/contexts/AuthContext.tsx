'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { apiClient } from '@/lib/api';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  avatar?: string;
  role: string;
  tenantId?: string;
  isSuperAdmin: boolean;
  twoFactorEnabled: boolean;
}

interface AuthState {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  tenant: any | null;
}

interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => void;
  forgotPassword: (email: string) => Promise<void>;
  resetPassword: (token: string, newPassword: string) => Promise<void>;
  verifyEmail: (token: string) => Promise<void>;
  enable2FA: () => Promise<string>;
  verify2FA: (code: string, token: string) => Promise<void>;
  disable2FA: (code: string) => Promise<void>;
  updateProfile: (data: Partial<User>) => Promise<void>;
  switchTenant: (tenantId: string) => Promise<void>;
  refreshToken: () => Promise<void>;
}

interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isLoading: true,
    isAuthenticated: false,
    tenant: null,
  });

  // Initialize auth state
  useEffect(() => {
    const initAuth = async () => {
      const token = localStorage.getItem('access_token');
      const tenantId = localStorage.getItem('tenant_id');

      if (token) {
        try {
          const response = await apiClient.users.getProfile();
          const user = response.data;

          setState({
            user,
            isLoading: false,
            isAuthenticated: true,
            tenant: user.tenantId ? { id: user.tenantId, name: '', subdomain: '', planId: '' } : null,
          });
        } catch (error) {
          // Token is invalid, clear it
          localStorage.removeItem('access_token');
          localStorage.removeItem('refresh_token');
          setState({
            user: null,
            isLoading: false,
            isAuthenticated: false,
            tenant: null,
          });
        }
      } else {
        setState({
          user: null,
          isLoading: false,
          isAuthenticated: false,
          tenant: null,
        });
      }
    };

    initAuth();
  }, []);

  const login = async (email: string, password: string) => {
    setState(prev => ({ ...prev, isLoading: true }));

    try {
      const response = await apiClient.auth.login({ email, password });
      const { user, accessToken, refreshToken, requires2FA, tempToken } = response.data;

      if (requires2FA) {
        // Store temporary token for 2FA verification
        localStorage.setItem('temp_token', tempToken);
        setState(prev => ({ ...prev, isLoading: false }));
        return;
      }

      localStorage.setItem('access_token', accessToken);
      localStorage.setItem('refresh_token', refreshToken);
      
      if (user.tenantId) {
        localStorage.setItem('tenant_id', user.tenantId);
      }

      setState({
        user,
        isLoading: false,
        isAuthenticated: true,
        tenant: user.tenantId ? { id: user.tenantId, name: '', subdomain: '', planId: '' } : null,
      });
    } catch (error) {
      setState(prev => ({ ...prev, isLoading: false }));
      throw error;
    }
  };

  const register = async (data: RegisterData) => {
    setState(prev => ({ ...prev, isLoading: true }));

    try {
      const response = await apiClient.auth.register(data);
      const { message } = response.data;
      
      setState(prev => ({ ...prev, isLoading: false }));
      // Show success message, redirect to email verification
    } catch (error) {
      setState(prev => ({ ...prev, isLoading: false }));
      throw error;
    }
  };

  const logout = () => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('tenant_id');
    localStorage.removeItem('temp_token');
    
    setState({
      user: null,
      isLoading: false,
      isAuthenticated: false,
      tenant: null,
    });

    // Call logout endpoint to invalidate token on server
    apiClient.auth.logout().catch(console.error);
  };

  const forgotPassword = async (email: string) => {
    await apiClient.auth.forgotPassword(email);
  };

  const resetPassword = async (token: string, newPassword: string) => {
    await apiClient.auth.resetPassword({ token, newPassword });
  };

  const verifyEmail = async (token: string) => {
    await apiClient.auth.verifyEmail(token);
  };

  const enable2FA = async (): Promise<string> => {
    const response = await apiClient.auth.enable2FA();
    return response.data.qrCode;
  };

  const verify2FA = async (code: string, token: string) => {
    const response = await apiClient.auth.verify2FA({ code, token });
    const { user, accessToken, refreshToken } = response.data;

    localStorage.setItem('access_token', accessToken);
    localStorage.setItem('refresh_token', refreshToken);
    localStorage.removeItem('temp_token');
    
    if (user.tenantId) {
      localStorage.setItem('tenant_id', user.tenantId);
    }

    setState({
      user,
      isLoading: false,
      isAuthenticated: true,
      tenant: user.tenantId ? { id: user.tenantId, name: '', subdomain: '', planId: '' } : null,
    });
  };

  const disable2FA = async (code: string) => {
    await apiClient.auth.disable2FA({ code });
    
    if (state.user) {
      setState(prev => ({
        ...prev,
        user: { ...prev.user!, twoFactorEnabled: false },
      }));
    }
  };

  const updateProfile = async (data: Partial<User>) => {
    const response = await apiClient.users.updateProfile(data);
    const updatedUser = response.data;

    setState(prev => ({
      ...prev,
      user: updatedUser,
    }));
  };

  const switchTenant = async (tenantId: string) => {
    localStorage.setItem('tenant_id', tenantId);
    
    // Fetch updated user profile with new tenant context
    const response = await apiClient.users.getProfile();
    const user = response.data;

    setState(prev => ({
      ...prev,
      user,
      tenant: { id: tenantId, name: '', subdomain: '', planId: '' },
    }));
  };

  const refreshToken = async () => {
    try {
      const refreshToken = localStorage.getItem('refresh_token');
      if (refreshToken) {
        const response = await apiClient.auth.refresh(refreshToken);
        const { accessToken } = response.data;
        localStorage.setItem('access_token', accessToken);
      }
    } catch (error) {
      logout();
    }
  };

  const value: AuthContextType = {
    ...state,
    login,
    register,
    logout,
    forgotPassword,
    resetPassword,
    verifyEmail,
    enable2FA,
    verify2FA,
    disable2FA,
    updateProfile,
    switchTenant,
    refreshToken,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
