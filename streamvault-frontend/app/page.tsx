'use client';

import { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { apiClient } from '@/lib/api-client';
import { useAuthStore } from '@/stores/auth-store';

type LoginResponse =
  | {
      success: true;
      message: string;
      requires2FA: true;
      userId: string;
      email: string;
    }
  | {
      accessToken: string;
      refreshToken: string;
      user: any;
    };

const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(1, 'Password is required'),
  tenantSlug: z.string().optional(),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { setUser } = useAuthStore();
  const [isLoading, setIsLoading] = useState(false);
  const [globalError, setGlobalError] = useState('');
  const [step, setStep] = useState<'login' | '2fa'>('login');
  const [pendingUserId, setPendingUserId] = useState<string>('');

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  useEffect(() => {
    const tenant = searchParams.get('tenant');
    if (tenant) {
      setValue('tenantSlug', tenant);
    }
  }, [searchParams, setValue]);

  const onSubmit = async (data: LoginFormData) => {
    setGlobalError('');
    setIsLoading(true);

    try {
      const response = await apiClient.post<LoginResponse>('/api/v1/auth/login', {
        email: data.email,
        password: data.password,
        tenantSlug: data.tenantSlug || undefined,
      });

      if ('accessToken' in response) {
        apiClient.setAuthToken(response.accessToken);
        localStorage.setItem('refresh_token', response.refreshToken);
        setUser(response.user);
        router.push('/dashboard');
      } else {
        setPendingUserId(response.userId);
        setStep('2fa');
      }
    } catch (error: any) {
      setGlobalError(error.response?.data?.error || 'Login failed');
    } finally {
      setIsLoading(false);
    }
  };

  if (step === '2fa') {
    return (
      <TwoFactorModal
        userId={pendingUserId}
        email={watch('email')}
        onSuccess={() => router.push('/dashboard')}
        onBack={() => setStep('login')}
      />
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-50 to-slate-100 py-12 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-md">
        <div className="bg-white rounded-lg shadow-lg p-8">
          <div className="text-center mb-8">
            <h1 className="text-3xl font-bold text-gray-900">StreamVault</h1>
            <h2 className="mt-2 text-xl font-semibold text-gray-700">Sign In</h2>
            <p className="mt-2 text-sm text-gray-600">Access your video hosting platform</p>
          </div>

          {globalError && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm text-red-700">{globalError}</p>
            </div>
          )}

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <label htmlFor="tenantSlug" className="block text-sm font-medium text-gray-700 mb-1">
                Workspace
              </label>
              <input
                {...register('tenantSlug')}
                type="text"
                placeholder="your-workspace"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition"
              />
            </div>

            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
                Email Address
              </label>
              <input
                {...register('email')}
                type="email"
                placeholder="you@example.com"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition"
              />
              {errors.email && (
                <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
              )}
            </div>

            <div>
              <div className="flex items-center justify-between mb-1">
                <label htmlFor="password" className="block text-sm font-medium text-gray-700">
                  Password
                </label>
                <Link href="/forgot-password" className="text-xs text-blue-600 hover:text-blue-500">
                  Forgot password?
                </Link>
              </div>
              <input
                {...register('password')}
                type="password"
                placeholder="••••••••"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition"
              />
              {errors.password && (
                <p className="mt-1 text-sm text-red-600">{errors.password.message}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-2 px-4 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition mt-6"
            >
              {isLoading ? 'Signing in...' : 'Sign In'}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-gray-600">
            Don&apos;t have an account?{' '}
            <Link href="/register" className="font-medium text-blue-600 hover:text-blue-500">
              Create one
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}

function TwoFactorModal({
  userId,
  email,
  onSuccess,
  onBack,
}: {
  userId: string;
  email: string;
  onSuccess: () => void;
  onBack: () => void;
}) {
  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleVerify = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const response = await apiClient.post<{ accessToken: string; refreshToken: string; user?: any }>(
        '/api/v1/auth/login/verify-2fa',
        {
        userId,
        code,
        }
      );

      apiClient.setAuthToken(response.accessToken);
      localStorage.setItem('refresh_token', response.refreshToken);
      onSuccess();
    } catch (error: any) {
      setError(error.response?.data?.error || 'Invalid code');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-50 to-slate-100 py-12 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-md">
        <div className="bg-white rounded-lg shadow-lg p-8">
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Two-Factor Authentication</h2>
          <p className="text-gray-600 text-sm mb-6">
            We&apos;ve sent a 6-digit code to {email}. Enter it below to continue.
          </p>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          )}

          <form onSubmit={handleVerify} className="space-y-4">
            <input
              type="text"
              value={code}
              onChange={(e) => setCode(e.target.value.slice(0, 6))}
              maxLength={6}
              placeholder="000000"
              className="w-full px-4 py-3 text-center text-2xl tracking-widest border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              autoFocus
            />

            <button
              type="submit"
              disabled={isLoading || code.length !== 6}
              className="w-full py-2 px-4 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition"
            >
              {isLoading ? 'Verifying...' : 'Verify'}
            </button>
          </form>

          <button
            onClick={onBack}
            className="w-full mt-4 py-2 px-4 text-gray-600 font-medium rounded-lg border border-gray-300 hover:bg-gray-50 transition"
          >
            Back
          </button>
        </div>
      </div>
    </div>
  );
}
