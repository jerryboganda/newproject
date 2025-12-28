'use client';

import { useAuth } from '@/contexts/AuthContext';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { AppLayout } from '@/components/layout/AppLayout';

export default function DashboardPage() {
  const { user, logout } = useAuth();

  return (
    <ProtectedRoute>
      <AppLayout>
        <div className="px-4 py-6 sm:px-0">
          <div className="border-4 border-dashed border-gray-200 rounded-lg h-96">
            <div className="p-8">
              <h1 className="text-2xl font-bold text-gray-900">Welcome to Dashboard</h1>
              <p className="mt-2 text-gray-600">
                Hello, {user?.firstName} {user?.lastName}!
              </p>
              <p className="mt-1 text-sm text-gray-500">
                Email: {user?.email}
              </p>
              <button
                onClick={logout}
                className="mt-4 px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </AppLayout>
    </ProtectedRoute>
  );
}
