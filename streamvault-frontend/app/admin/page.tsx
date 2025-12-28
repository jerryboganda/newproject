'use client'

import { useEffect, useState } from 'react'
import { apiClient } from '@/lib/api'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { 
  BuildingOfficeIcon, 
  UsersIcon, 
  CreditCardIcon,
  ChartBarIcon,
  CurrencyDollarIcon,
  ServerIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon
} from '@heroicons/react/24/outline'
import { formatNumber, formatDateTime } from '@/lib/utils'

interface DashboardStats {
  totalTenants: number
  totalUsers: number
  activeSubscriptions: number
  monthlyRevenue: number
  totalStorage: number
  systemHealth: 'healthy' | 'warning' | 'critical'
  recentActivity: Array<{
    id: string
    type: string
    description: string
    timestamp: string
  }>
  topTenants: Array<{
    id: string
    name: string
    users: number
    storage: number
    revenue: number
  }>
}

export default function AdminDashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchDashboardStats()
  }, [])

  const fetchDashboardStats = async () => {
    try {
      // Mock data for now - replace with actual API call
      const mockStats: DashboardStats = {
        totalTenants: 156,
        totalUsers: 12450,
        activeSubscriptions: 89,
        monthlyRevenue: 48500,
        totalStorage: 8.5, // in TB
        systemHealth: 'healthy',
        recentActivity: [
          {
            id: '1',
            type: 'tenant_created',
            description: 'New tenant "TechCorp Inc." registered',
            timestamp: new Date(Date.now() - 1000 * 60 * 5).toISOString(),
          },
          {
            id: '2',
            type: 'payment_received',
            description: 'Payment of $299 received from Acme Corp',
            timestamp: new Date(Date.now() - 1000 * 60 * 15).toISOString(),
          },
          {
            id: '3',
            type: 'user_registered',
            description: '25 new users registered in the last hour',
            timestamp: new Date(Date.now() - 1000 * 60 * 60).toISOString(),
          },
        ],
        topTenants: [
          { id: '1', name: 'Acme Corporation', users: 1250, storage: 2.5, revenue: 12500 },
          { id: '2', name: 'Tech Industries', users: 980, storage: 1.8, revenue: 9800 },
          { id: '3', name: 'Global Media', users: 750, storage: 1.2, revenue: 7500 },
          { id: '4', name: 'Digital Solutions', users: 620, storage: 0.9, revenue: 6200 },
          { id: '5', name: 'Cloud Services', users: 450, storage: 0.7, revenue: 4500 },
        ],
      }
      
      setStats(mockStats)
    } catch (error) {
      console.error('Failed to fetch dashboard stats:', error)
    } finally {
      setLoading(false)
    }
  }

  const getHealthColor = (health: string) => {
    switch (health) {
      case 'healthy':
        return 'text-green-600 bg-green-100'
      case 'warning':
        return 'text-yellow-600 bg-yellow-100'
      case 'critical':
        return 'text-red-600 bg-red-100'
      default:
        return 'text-gray-600 bg-gray-100'
    }
  }

  const getHealthIcon = (health: string) => {
    switch (health) {
      case 'healthy':
        return CheckCircleIcon
      case 'warning':
      case 'critical':
        return ExclamationTriangleIcon
      default:
        return CheckCircleIcon
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  if (!stats) {
    return (
      <div className="text-center text-gray-500">
        Failed to load dashboard data
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h2 className="text-2xl font-bold text-gray-900">Dashboard</h2>
        <p className="text-gray-600">Welcome to the StreamVault Admin Portal</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Tenants</CardTitle>
            <BuildingOfficeIcon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.totalTenants)}</div>
            <p className="text-xs text-muted-foreground">
              +12% from last month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Users</CardTitle>
            <UsersIcon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.totalUsers)}</div>
            <p className="text-xs text-muted-foreground">
              +8% from last month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Subscriptions</CardTitle>
            <CreditCardIcon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.activeSubscriptions)}</div>
            <p className="text-xs text-muted-foreground">
              +5% from last month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Monthly Revenue</CardTitle>
            <CurrencyDollarIcon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">${formatNumber(stats.monthlyRevenue)}</div>
            <p className="text-xs text-muted-foreground">
              +15% from last month
            </p>
          </CardContent>
        </Card>
      </div>

      {/* System Health and Storage */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <ServerIcon className="h-5 w-5 mr-2" />
              System Health
            </CardTitle>
            <CardDescription>
              Overall system status and performance
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex items-center space-x-2">
              <div className={`p-2 rounded-full ${getHealthColor(stats.systemHealth)}`}>
                {(() => {
                  const Icon = getHealthIcon(stats.systemHealth)
                  return <Icon className="h-5 w-5" />
                })()}
              </div>
              <div>
                <p className="font-medium capitalize">{stats.systemHealth}</p>
                <p className="text-sm text-gray-500">All systems operational</p>
              </div>
            </div>
            
            <div className="mt-4 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-gray-600">Storage Used</span>
                <span className="font-medium">{stats.totalStorage} TB</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-blue-600 h-2 rounded-full" style={{ width: '65%' }}></div>
              </div>
              <p className="text-xs text-gray-500">65% of 13 TB allocated</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Recent Activity</CardTitle>
            <CardDescription>
              Latest system events and notifications
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {stats.recentActivity.map((activity) => (
                <div key={activity.id} className="flex items-start space-x-3">
                  <div className="w-2 h-2 bg-blue-600 rounded-full mt-2"></div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm text-gray-900">{activity.description}</p>
                    <p className="text-xs text-gray-500">
                      {formatDateTime(activity.timestamp)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Top Tenants */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <span>Top Tenants by Revenue</span>
            <ChartBarIcon className="h-5 w-5 text-gray-400" />
          </CardTitle>
          <CardDescription>
            Highest performing tenants this month
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-2 text-sm font-medium text-gray-900">Tenant</th>
                  <th className="text-right py-2 text-sm font-medium text-gray-900">Users</th>
                  <th className="text-right py-2 text-sm font-medium text-gray-900">Storage</th>
                  <th className="text-right py-2 text-sm font-medium text-gray-900">Revenue</th>
                </tr>
              </thead>
              <tbody>
                {stats.topTenants.map((tenant) => (
                  <tr key={tenant.id} className="border-b">
                    <td className="py-3 text-sm">{tenant.name}</td>
                    <td className="py-3 text-sm text-right">{formatNumber(tenant.users)}</td>
                    <td className="py-3 text-sm text-right">{tenant.storage} TB</td>
                    <td className="py-3 text-sm text-right font-medium">
                      ${formatNumber(tenant.revenue)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
