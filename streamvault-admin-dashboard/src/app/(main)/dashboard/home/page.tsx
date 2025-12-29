"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { Badge } from "@/components/ui/badge";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs";
import {
  Play,
  Upload,
  Users,
  Eye,
  HardDrive,
  TrendingUp,
  TrendingDown,
  Calendar,
  Clock,
  BarChart3,
  PieChart,
  Activity,
  FileVideo,
  FolderPlus,
  Settings,
  Mail,
  Shield,
  AlertCircle,
  CheckCircle,
} from "lucide-react";
import { useAuthStore } from "@/stores/auth-store";
import { useVideoStore } from "@/stores/video-store";
import { useCollectionStore } from "@/stores/collection-store";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart as RePieChart, Pie, Cell, BarChart, Bar } from "recharts";

interface DashboardStats {
  totalVideos: number;
  totalViews: number;
  totalUsers: number;
  storageUsed: number;
  storageLimit: number;
  thisMonthViews: number;
  thisMonthUploads: number;
  recentActivity: ActivityItem[];
}

interface ActivityItem {
  id: string;
  type: "upload" | "view" | "comment" | "share";
  message: string;
  timestamp: Date;
  userId?: string;
  videoId?: string;
}

interface ChartData {
  views: { date: string; views: number }[];
  uploads: { date: string; uploads: number }[];
  storage: { name: string; value: number; color: string }[];
}

export default function EnhancedDashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [chartData, setChartData] = useState<ChartData | null>(null);
  const [loading, setLoading] = useState(true);
  
  const { user } = useAuthStore();
  const { videos, fetchVideos } = useVideoStore();
  const { collections, fetchCollections } = useCollectionStore();

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    setLoading(true);
    try {
      // Simulate API calls
      await Promise.all([
        fetchVideos(),
        fetchCollections(),
      ]);

      // Mock stats data
      const mockStats: DashboardStats = {
        totalVideos: 156,
        totalViews: 45678,
        totalUsers: 12,
        storageUsed: 85.6, // GB
        storageLimit: 200, // GB
        thisMonthViews: 12345,
        thisMonthUploads: 23,
        recentActivity: [
          {
            id: "1",
            type: "upload",
            message: "New video 'Product Demo' uploaded",
            timestamp: new Date(Date.now() - 1000 * 60 * 5),
          },
          {
            id: "2",
            type: "view",
            message: "Video 'Tutorial Part 3' reached 1000 views",
            timestamp: new Date(Date.now() - 1000 * 60 * 30),
          },
          {
            id: "3",
            type: "comment",
            message: "New comment on 'Company Overview'",
            timestamp: new Date(Date.now() - 1000 * 60 * 60),
          },
          {
            id: "4",
            type: "upload",
            message: "Video 'Webinar Recording' processed successfully",
            timestamp: new Date(Date.now() - 1000 * 60 * 60 * 2),
          },
        ],
      };

      // Mock chart data
      const mockChartData: ChartData = {
        views: [
          { date: "Mon", views: 400 },
          { date: "Tue", views: 300 },
          { date: "Wed", views: 600 },
          { date: "Thu", views: 800 },
          { date: "Fri", views: 500 },
          { date: "Sat", views: 700 },
          { date: "Sun", views: 900 },
        ],
        uploads: [
          { date: "Mon", uploads: 2 },
          { date: "Tue", uploads: 3 },
          { date: "Wed", uploads: 1 },
          { date: "Thu", uploads: 4 },
          { date: "Fri", uploads: 2 },
          { date: "Sat", uploads: 5 },
          { date: "Sun", uploads: 3 },
        ],
        storage: [
          { name: "Videos", value: 75, color: "#3b82f6" },
          { name: "Thumbnails", value: 8, color: "#10b981" },
          { name: "Other", value: 2.6, color: "#f59e0b" },
        ],
      };

      setStats(mockStats);
      setChartData(mockChartData);
    } catch (error) {
      console.error("Failed to load dashboard data:", error);
    } finally {
      setLoading(false);
    }
  };

  const getActivityIcon = (type: ActivityItem["type"]) => {
    switch (type) {
      case "upload":
        return <Upload className="h-4 w-4 text-blue-600" />;
      case "view":
        return <Eye className="h-4 w-4 text-green-600" />;
      case "comment":
        return <Mail className="h-4 w-4 text-purple-600" />;
      case "share":
        return <Activity className="h-4 w-4 text-orange-600" />;
    }
  };

  const formatTimeAgo = (date: Date) => {
    const seconds = Math.floor((new Date().getTime() - date.getTime()) / 1000);
    
    if (seconds < 60) return "just now";
    if (seconds < 3600) return `${Math.floor(seconds / 60)} minutes ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)} hours ago`;
    return `${Math.floor(seconds / 86400)} days ago`;
  };

  if (loading) {
    return (
      <div className="container mx-auto py-8">
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </div>
    );
  }

  if (!stats || !chartData) {
    return (
      <div className="container mx-auto py-8">
        <Card>
          <CardContent className="py-12 text-center">
            <AlertCircle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-semibold mb-2">Unable to load dashboard</h3>
            <p className="text-gray-600 mb-4">Please try refreshing the page</p>
            <Button onClick={loadDashboardData}>Retry</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 space-y-6">
      {/* Welcome Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">
            Welcome back, {user?.firstName || "User"}! 👋
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            Here's what's happening with your videos today
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" asChild>
            <Link href="/dashboard/settings">
              <Settings className="mr-2 h-4 w-4" />
              Settings
            </Link>
          </Button>
          {!user?.isEmailVerified && (
            <Button variant="outline" asChild>
              <Link href="/auth/verify-email">
                <Mail className="mr-2 h-4 w-4" />
                Verify Email
              </Link>
            </Button>
          )}
          {!user?.twoFactorEnabled && (
            <Button variant="outline" asChild>
              <Link href="/auth/2fa">
                <Shield className="mr-2 h-4 w-4" />
                Enable 2FA
              </Link>
            </Button>
          )}
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Videos</CardTitle>
            <FileVideo className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalVideos}</div>
            <div className="flex items-center text-xs text-muted-foreground">
              <TrendingUp className="mr-1 h-3 w-3" />
              +{stats.thisMonthUploads} this month
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Views</CardTitle>
            <Eye className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalViews.toLocaleString()}</div>
            <div className="flex items-center text-xs text-muted-foreground">
              <TrendingUp className="mr-1 h-3 w-3" />
              +{stats.thisMonthViews.toLocaleString()} this month
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Team Members</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalUsers}</div>
            <div className="flex items-center text-xs text-muted-foreground">
              <Users className="mr-1 h-3 w-3" />
              {stats.totalUsers} active users
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Storage Used</CardTitle>
            <HardDrive className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {stats.storageUsed.toFixed(1)} GB
            </div>
            <Progress
              value={(stats.storageUsed / stats.storageLimit) * 100}
              className="mt-2"
            />
            <p className="text-xs text-muted-foreground mt-1">
              of {stats.storageLimit} GB
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Quick Actions</CardTitle>
          <CardDescription>
            Common tasks you might want to perform
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Button asChild className="h-20 flex-col">
              <Link href="/dashboard/upload/enhanced">
                <Upload className="h-6 w-6 mb-2" />
                Upload Video
              </Link>
            </Button>
            <Button variant="outline" asChild className="h-20 flex-col">
              <Link href="/dashboard/collections">
                <FolderPlus className="h-6 w-6 mb-2" />
                Create Collection
              </Link>
            </Button>
            <Button variant="outline" asChild className="h-20 flex-col">
              <Link href="/dashboard/videos/list">
                <Play className="h-6 w-6 mb-2" />
                Browse Videos
              </Link>
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Charts and Activity */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Tabs defaultValue="views" className="space-y-4">
            <TabsList>
              <TabsTrigger value="views">Views Analytics</TabsTrigger>
              <TabsTrigger value="uploads">Upload Trends</TabsTrigger>
              <TabsTrigger value="storage">Storage Breakdown</TabsTrigger>
            </TabsList>

            <TabsContent value="views" className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle>Views This Week</CardTitle>
                  <CardDescription>
                    Daily view count for the last 7 days
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={chartData.views}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis />
                      <Tooltip />
                      <Line
                        type="monotone"
                        dataKey="views"
                        stroke="#3b82f6"
                        strokeWidth={2}
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="uploads" className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle>Upload Activity</CardTitle>
                  <CardDescription>
                    Number of videos uploaded per day
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={chartData.uploads}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis />
                      <Tooltip />
                      <Bar dataKey="uploads" fill="#10b981" />
                    </BarChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="storage" className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle>Storage Usage</CardTitle>
                  <CardDescription>
                    Breakdown of your storage consumption
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <ResponsiveContainer width="100%" height={300}>
                    <RePieChart>
                      <Pie
                        data={chartData.storage}
                        cx="50%"
                        cy="50%"
                        innerRadius={60}
                        outerRadius={100}
                        paddingAngle={5}
                        dataKey="value"
                      >
                        {chartData.storage.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={entry.color} />
                        ))}
                      </Pie>
                      <Tooltip />
                    </RePieChart>
                  </ResponsiveContainer>
                  <div className="flex justify-center gap-4 mt-4">
                    {chartData.storage.map((item) => (
                      <div key={item.name} className="flex items-center gap-2">
                        <div
                          className="w-3 h-3 rounded-full"
                          style={{ backgroundColor: item.color }}
                        />
                        <span className="text-sm">{item.name}</span>
                        <span className="text-sm font-medium">{item.value} GB</span>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>

        {/* Recent Activity */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Activity</CardTitle>
            <CardDescription>
              Latest updates from your video platform
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {stats.recentActivity.map((activity) => (
                <div key={activity.id} className="flex items-start gap-3">
                  <div className="mt-0.5">
                    {getActivityIcon(activity.type)}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm text-gray-900 dark:text-gray-100">
                      {activity.message}
                    </p>
                    <p className="text-xs text-gray-500">
                      {formatTimeAgo(activity.timestamp)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
            <Button variant="outline" className="w-full mt-4" asChild>
              <Link href="/dashboard/activity">View All Activity</Link>
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Notifications */}
      {!user?.isEmailVerified && (
        <Card className="border-yellow-200 bg-yellow-50 dark:border-yellow-800 dark:bg-yellow-950">
          <CardContent className="py-4">
            <div className="flex items-center gap-3">
              <AlertCircle className="h-5 w-5 text-yellow-600" />
              <div className="flex-1">
                <p className="text-sm font-medium text-yellow-800 dark:text-yellow-200">
                  Email verification required
                </p>
                <p className="text-sm text-yellow-700 dark:text-yellow-300">
                  Please verify your email address to access all features
                </p>
              </div>
              <Button size="sm" asChild>
                <Link href="/auth/verify-email">Verify Now</Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
