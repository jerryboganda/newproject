'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/auth-store';

export default function DashboardPage() {
  const router = useRouter();
  const { user, isAuthenticated } = useAuthStore();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    if (mounted && !isAuthenticated) {
      router.push('/');
    }
  }, [mounted, isAuthenticated, router]);

  if (!mounted) return null;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center">
            <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
            <button
              onClick={() => {
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                router.push('/');
              }}
              className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition"
            >
              Logout
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {/* Welcome Card */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-2">Welcome</h2>
            <p className="text-gray-600">
              {user?.email || 'Guest'}
            </p>
            <p className="text-sm text-gray-500 mt-2">
              Role: {user?.role || 'User'}
            </p>
          </div>

          {/* Videos Card */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-2">Videos</h2>
            <p className="text-gray-600">0 videos</p>
            <button className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition">
              Upload Video
            </button>
          </div>

          {/* Collections Card */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-2">Collections</h2>
            <p className="text-gray-600">0 collections</p>
            <button className="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition">
              Create Collection
            </button>
          </div>
        </div>

        {/* Quick Stats */}
        <div className="mt-8 bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Quick Stats</h2>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="border-l-4 border-blue-600 pl-4">
              <p className="text-sm text-gray-600">Total Views</p>
              <p className="text-2xl font-bold text-gray-900">0</p>
            </div>
            <div className="border-l-4 border-green-600 pl-4">
              <p className="text-sm text-gray-600">Total Hours</p>
              <p className="text-2xl font-bold text-gray-900">0h</p>
            </div>
            <div className="border-l-4 border-purple-600 pl-4">
              <p className="text-sm text-gray-600">Storage Used</p>
              <p className="text-2xl font-bold text-gray-900">0 GB</p>
            </div>
            <div className="border-l-4 border-orange-600 pl-4">
              <p className="text-sm text-gray-600">Bandwidth</p>
              <p className="text-2xl font-bold text-gray-900">0 GB</p>
            </div>
          </div>
        </div>

        {/* Feature Navigation */}
        <div className="mt-8 grid grid-cols-1 md:grid-cols-2 gap-6">
          <a href="/videos" className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">📹 Videos</h3>
            <p className="text-gray-600">Manage your video library</p>
          </a>
          <a href="/collections" className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">📂 Collections</h3>
            <p className="text-gray-600">Organize your content</p>
          </a>
          <a href="/analytics" className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">📊 Analytics</h3>
            <p className="text-gray-600">View detailed statistics</p>
          </a>
          <a href="/settings" className="bg-white rounded-lg shadow p-6 hover:shadow-lg transition">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">⚙️ Settings</h3>
            <p className="text-gray-600">Manage your account</p>
          </a>
        </div>
      </main>
    </div>
  );
}
