"use client";

import { useCallback, useEffect, useState } from "react";

import Link from "next/link";

import {
  Activity,
  AlertTriangle,
  Building2,
  Calendar,
  CheckCircle,
  DollarSign,
  Edit,
  Eye as EyeIcon,
  Globe,
  HardDrive,
  Lock,
  Mail,
  MoreHorizontal,
  Play,
  Plus,
  Search,
  Settings,
  Trash2,
  TrendingDown,
  Unlock,
  UserCheck,
  Users,
  Video,
  Wifi,
  XCircle,
  Zap,
} from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Line,
  LineChart,
  Pie,
  PieChart as RePieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  type StreamVaultAdminPlatformAnalyticsResponse,
  type StreamVaultAdminTenantListItem,
  streamvaultApi,
} from "@/lib/streamvault-api";

type Tenant = StreamVaultAdminTenantListItem;
type PlatformAnalytics = StreamVaultAdminPlatformAnalyticsResponse;

export default function SuperAdminDashboard() {
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [analytics, setAnalytics] = useState<PlatformAnalytics | null>(null);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [_showCreateTenantDialog, _setShowCreateTenantDialog] = useState(false);
  const [_selectedTenant, _setSelectedTenant] = useState<Tenant | null>(null);
  const [activeTab, setActiveTab] = useState("overview");

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [tenantsData, analyticsData] = await Promise.all([
        streamvaultApi.admin.tenants.list({ page: 1, pageSize: 200 }),
        streamvaultApi.admin.analytics.get(),
      ]);

      setTenants(tenantsData);
      setAnalytics(analyticsData);
    } catch (error) {
      console.error("Failed to load data:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleSuspendTenant = async (tenantId: string, reason: string) => {
    try {
      await streamvaultApi.admin.tenants.suspend(tenantId, reason);
      await loadData();
    } catch (error) {
      console.error("Failed to suspend tenant:", error);
    }
  };

  const handleUnsuspendTenant = async (tenantId: string) => {
    try {
      await streamvaultApi.admin.tenants.unsuspend(tenantId);
      await loadData();
    } catch (error) {
      console.error("Failed to unsuspend tenant:", error);
    }
  };

  const handleImpersonateUser = async (tenantId: string) => {
    try {
      const { token, targetUrl } = await streamvaultApi.admin.impersonate(tenantId);

      const url = `${targetUrl}?accessToken=${encodeURIComponent(token)}`;
      window.open(url, "_blank");
    } catch (error) {
      console.error("Failed to impersonate user:", error);
    }
  };

  const filteredTenants = tenants.filter(
    (tenant) =>
      tenant.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      tenant.slug.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  const chartData = analytics?.series ?? [];

  const tenantStatusData = [
    { name: "Active", value: analytics?.activeTenants || 0, color: "#10b981" },
    { name: "Suspended", value: (analytics?.totalTenants || 0) - (analytics?.activeTenants || 0), color: "#ef4444" },
  ];

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB", "TB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${Math.round((bytes / k ** i) * 100) / 100} ${sizes[i]}`;
  };

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-12 w-12 animate-spin rounded-full border-blue-600 border-b-2" />
      </div>
    );
  }

  return (
    <div className="container mx-auto space-y-6 py-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-bold text-3xl">Super Admin Dashboard</h1>
          <p className="text-gray-600 dark:text-gray-400">Manage platform-wide settings and monitor all tenants</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" asChild>
            <Link href="/admin/settings">
              <Settings className="mr-2 h-4 w-4" />
              System Settings
            </Link>
          </Button>
          <Button asChild>
            <Link href="/admin/support">
              <Mail className="mr-2 h-4 w-4" />
              Support Center
            </Link>
          </Button>
        </div>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="tenants">Tenants</TabsTrigger>
          <TabsTrigger value="analytics">Analytics</TabsTrigger>
          <TabsTrigger value="system">System</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          {/* Key Metrics */}
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="font-medium text-sm">Total Tenants</CardTitle>
                <Building2 className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="font-bold text-2xl">{analytics?.totalTenants}</div>
                <p className="text-muted-foreground text-xs">+{analytics?.newTenantsThisMonth} this month</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="font-medium text-sm">Active Users</CardTitle>
                <Users className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="font-bold text-2xl">{analytics?.activeUsers}</div>
                <p className="text-muted-foreground text-xs">+{analytics?.newUsersThisMonth} this month</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="font-medium text-sm">Total Revenue</CardTitle>
                <DollarSign className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="font-bold text-2xl">${analytics?.revenue.toLocaleString()}</div>
                <p className="text-muted-foreground text-xs">+12% from last month</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="font-medium text-sm">Churn Rate</CardTitle>
                <TrendingDown className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="font-bold text-2xl">{analytics?.churnRate}%</div>
                <p className="text-muted-foreground text-xs">-2.1% from last month</p>
              </CardContent>
            </Card>
          </div>

          {/* Charts */}
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Growth Trends</CardTitle>
                <CardDescription>Monthly platform growth metrics</CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip />
                    <Line type="monotone" dataKey="tenants" stroke="#3b82f6" strokeWidth={2} />
                    <Line type="monotone" dataKey="users" stroke="#10b981" strokeWidth={2} />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Tenant Status</CardTitle>
                <CardDescription>Distribution of tenant statuses</CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <RePieChart>
                    <Pie
                      data={tenantStatusData}
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={100}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {tenantStatusData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </RePieChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="tenants" className="space-y-6">
          {/* Tenant Management */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Tenant Management</CardTitle>
                  <CardDescription>Manage all platform tenants</CardDescription>
                </div>
                <div className="flex gap-2">
                  <div className="relative">
                    <Search className="absolute top-3 left-3 h-4 w-4 text-gray-400" />
                    <Input
                      placeholder="Search tenants..."
                      className="w-64 pl-10"
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                    />
                  </div>
                  <Button>
                    <Plus className="mr-2 h-4 w-4" />
                    New Tenant
                  </Button>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Tenant</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Users</TableHead>
                    <TableHead>Videos</TableHead>
                    <TableHead>Storage</TableHead>
                    <TableHead>Views</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredTenants.map((tenant) => (
                    <TableRow key={tenant.id}>
                      <TableCell>
                        <div className="flex items-center gap-3">
                          {tenant.logoUrl ? (
                            <img src={tenant.logoUrl} alt={tenant.name} className="h-8 w-8 rounded" />
                          ) : (
                            <div className="flex h-8 w-8 items-center justify-center rounded bg-gray-200">
                              <Building2 className="h-4 w-4 text-gray-500" />
                            </div>
                          )}
                          <div>
                            <div className="font-medium">{tenant.name}</div>
                            <div className="text-gray-500 text-sm">{tenant.slug}</div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1 text-gray-500 text-sm">
                          <Calendar className="h-3 w-3" />
                          {new Date(tenant.createdAt).toLocaleDateString()}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          {tenant.isActive ? (
                            <Badge variant="default" className="bg-green-100 text-green-800">
                              <CheckCircle className="mr-1 h-3 w-3" />
                              Active
                            </Badge>
                          ) : (
                            <Badge variant="destructive">
                              <XCircle className="mr-1 h-3 w-3" />
                              Suspended
                            </Badge>
                          )}
                          {tenant.isSuspended && tenant.suspensionReason && (
                            <p className="text-gray-500 text-xs">{tenant.suspensionReason}</p>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>{tenant.stats.users}</TableCell>
                      <TableCell>{tenant.stats.videos}</TableCell>
                      <TableCell>{formatBytes(tenant.stats.storageUsedBytes)}</TableCell>
                      <TableCell>{tenant.stats.views.toLocaleString()}</TableCell>
                      <TableCell>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="sm">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem asChild>
                              <Link href={`/admin/tenants/${tenant.id}`}>
                                <EyeIcon className="mr-2 h-4 w-4" />
                                View Details
                              </Link>
                            </DropdownMenuItem>
                            <DropdownMenuItem asChild>
                              <Link href={`/admin/tenants/${tenant.id}/edit`}>
                                <Edit className="mr-2 h-4 w-4" />
                                Edit
                              </Link>
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem onClick={() => handleImpersonateUser(tenant.id)}>
                              <UserCheck className="mr-2 h-4 w-4" />
                              Impersonate
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            {tenant.isActive ? (
                              <DropdownMenuItem
                                onClick={() => handleSuspendTenant(tenant.id, "Manual suspension")}
                                className="text-red-600"
                              >
                                <Lock className="mr-2 h-4 w-4" />
                                Suspend
                              </DropdownMenuItem>
                            ) : (
                              <DropdownMenuItem
                                onClick={() => handleUnsuspendTenant(tenant.id)}
                                className="text-green-600"
                              >
                                <Unlock className="mr-2 h-4 w-4" />
                                Unsuspend
                              </DropdownMenuItem>
                            )}
                            <DropdownMenuSeparator />
                            <DropdownMenuItem className="text-red-600">
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="analytics" className="space-y-6">
          {/* Platform Analytics */}
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Revenue Analytics</CardTitle>
                <CardDescription>Monthly revenue breakdown</CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="revenue" fill="#3b82f6" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Usage Metrics</CardTitle>
                <CardDescription>Platform-wide usage statistics</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Video className="h-4 w-4 text-blue-600" />
                      <span>Total Videos</span>
                    </div>
                    <span className="font-semibold">{analytics?.totalVideos.toLocaleString()}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Play className="h-4 w-4 text-green-600" />
                      <span>Total Views</span>
                    </div>
                    <span className="font-semibold">{analytics?.totalViews.toLocaleString()}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <HardDrive className="h-4 w-4 text-purple-600" />
                      <span>Storage Used</span>
                    </div>
                    <span className="font-semibold">{formatBytes(analytics?.totalStorageUsed || 0)}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Wifi className="h-4 w-4 text-orange-600" />
                      <span>Bandwidth</span>
                    </div>
                    <span className="font-semibold">12.5 TB this month</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="system" className="space-y-6">
          {/* System Health */}
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="font-medium text-sm">CPU Usage</CardTitle>
                <Activity className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="font-bold text-2xl">45.5%</div>
                <div className="mt-2 h-2 w-full rounded-full bg-gray-200">
                  <div className="h-2 rounded-full bg-blue-600" style={{ width: "45.5%" }} />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="font-medium text-sm">Memory Usage</CardTitle>
                <Zap className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="font-bold text-2xl">62.3%</div>
                <div className="mt-2 h-2 w-full rounded-full bg-gray-200">
                  <div className="h-2 rounded-full bg-green-600" style={{ width: "62.3%" }} />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="font-medium text-sm">Disk Usage</CardTitle>
                <HardDrive className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="font-bold text-2xl">78.9%</div>
                <div className="mt-2 h-2 w-full rounded-full bg-gray-200">
                  <div className="h-2 rounded-full bg-yellow-600" style={{ width: "78.9%" }} />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="font-medium text-sm">Uptime</CardTitle>
                <Globe className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="font-bold text-2xl">99.9%</div>
                <p className="text-muted-foreground text-xs">Last 30 days</p>
              </CardContent>
            </Card>
          </div>

          {/* Recent System Events */}
          <Card>
            <CardHeader>
              <CardTitle>System Events</CardTitle>
              <CardDescription>Recent system notifications and alerts</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex items-center gap-3 rounded-lg bg-blue-50 p-3">
                  <AlertTriangle className="h-5 w-5 text-blue-600" />
                  <div className="flex-1">
                    <p className="font-medium">Scheduled Maintenance</p>
                    <p className="text-gray-600 text-sm">
                      System maintenance scheduled for Dec 31, 2025 at 2:00 AM UTC
                    </p>
                  </div>
                  <Badge variant="outline">Upcoming</Badge>
                </div>
                <div className="flex items-center gap-3 rounded-lg bg-green-50 p-3">
                  <CheckCircle className="h-5 w-5 text-green-600" />
                  <div className="flex-1">
                    <p className="font-medium">Backup Completed</p>
                    <p className="text-gray-600 text-sm">Daily backup completed successfully</p>
                  </div>
                  <Badge variant="outline" className="text-green-600">
                    Completed
                  </Badge>
                </div>
                <div className="flex items-center gap-3 rounded-lg bg-yellow-50 p-3">
                  <AlertTriangle className="h-5 w-5 text-yellow-600" />
                  <div className="flex-1">
                    <p className="font-medium">High Disk Usage</p>
                    <p className="text-gray-600 text-sm">Disk usage exceeded 75% threshold</p>
                  </div>
                  <Badge variant="outline" className="text-yellow-600">
                    Warning
                  </Badge>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
