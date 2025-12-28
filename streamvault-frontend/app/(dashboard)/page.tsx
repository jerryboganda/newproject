'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { 
  VideoCameraIcon,
  PlayIcon,
  EyeIcon,
  CloudArrowUpIcon,
  FolderIcon,
  ChartBarIcon,
  PlusIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
  ClockIcon
} from '@heroicons/react/24/outline'
import { formatNumber, formatDateTime } from '@/lib/utils'

interface DashboardStats {
  totalVideos: number
  totalViews: number
  totalStorage: number
  storageLimit: number
  recentUploads: Array<{
    id: string
    title: string
    thumbnail: string
    duration: number
    createdAt: string
    views: number
    status: 'processing' | 'ready' | 'failed'
  }>
  topVideos: Array<{
    id: string
    title: string
    views: number
    change: number
  }>
  quickActions: Array<{
    title: string
    description: string
    icon: React.ElementType
    href: string
    color: string
  }>
}

export default function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchDashboardStats()
  }, [])

  const fetchDashboardStats = async () => {
    try {
      // Mock data - replace with actual API call
      const mockStats: DashboardStats = {
        totalVideos: 156,
        totalViews: 45680,
        totalStorage: 8.5,
        storageLimit: 20,
        recentUploads: [
          {
            id: '1',
            title: 'Product Demo 2024',
            thumbnail: '/api/placeholder/320/180',
            duration: 245,
            createdAt: new Date(Date.now() - 1000 * 60 * 30).toISOString(),
            views: 45,
            status: 'ready'
          },
          {
            id: '2',
            title: 'Tutorial: Getting Started',
            thumbnail: '/api/placeholder/320/180',
            duration: 580,
            createdAt: new Date(Date.now() - 1000 * 60 * 60 * 2).toISOString(),
            views: 128,
            status: 'processing'
          },
          {
            id: '3',
            title: 'Customer Testimonial',
            thumbnail: '/api/placeholder/320/180',
            duration: 120,
            createdAt: new Date(Date.now() - 1000 * 60 * 60 * 5).toISOString(),
            views: 89,
            status: 'ready'
          },
          {
            id: '4',
            title: 'Webinar Recording',
            thumbnail: '/api/placeholder/320/180',
            duration: 3600,
            createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24).toISOString(),
            views: 234,
            status: 'ready'
          }
        ],
        topVideos: [
          {
            id: '1',
            title: 'Ultimate Guide to Video Marketing',
            views: 5420,
            change: 12.5
          },
          {
            id: '2',
            title: 'How to Use Our Platform',
            views: 3210,
            change: -5.2
          },
          {
            id: '3',
            title: 'Customer Success Stories',
            views: 2890,
            change: 8.7
          }
        ],
        quickActions: [
          {
            title: 'Upload Video',
            description: 'Add new content to your library',
            icon: CloudArrowUpIcon,
            href: '/dashboard/videos/upload',
            color: 'bg-blue-500'
          },
          {
            title: 'Create Collection',
            description: 'Organize videos into collections',
            icon: FolderIcon,
            href: '/dashboard/collections',
            color: 'bg-purple-500'
          },
          {
            title: 'View Analytics',
            description: 'Track performance metrics',
            icon: ChartBarIcon,
            href: '/dashboard/analytics',
            color: 'bg-green-500'
          },
          {
            title: 'Invite Team Member',
            description: 'Collaborate with your team',
            icon: PlusIcon,
            href: '/dashboard/team/invite',
            color: 'bg-orange-500'
          }
        ]
      }
      
      setStats(mockStats)
    } catch (error) {
      console.error('Failed to fetch dashboard stats:', error)
    } finally {
      setLoading(false)
    }
  }

  const getStoragePercentage = () => {
    if (!stats) return 0
    return Math.round((stats.totalStorage / stats.storageLimit) * 100)
  }

  const getStorageColor = () => {
    const percentage = getStoragePercentage()
    if (percentage >= 90) return 'bg-red-500'
    if (percentage >= 70) return 'bg-yellow-500'
    return 'bg-green-500'
  }

  const formatDuration = (seconds: number) => {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    const secs = seconds % 60
    
    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
    }
    return `${minutes}:${secs.toString().padStart(2, '0')}`
  }

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'ready':
        return <Badge className="bg-green-100 text-green-800">Ready</Badge>
      case 'processing':
        return <Badge className="bg-yellow-100 text-yellow-800">Processing</Badge>
      case 'failed':
        return <Badge className="bg-red-100 text-red-800">Failed</Badge>
      default:
        return <Badge variant="secondary">Unknown</Badge>
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
        <p className="text-gray-600">Welcome back! Here's what's happening with your videos.</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Videos</CardTitle>
            <VideoCameraIcon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.totalVideos)}</div>
            <p className="text-xs text-muted-foreground">
              +8% from last month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Views</CardTitle>
            <PlayIcon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatNumber(stats.totalViews)}</div>
            <p className="text-xs text-muted-foreground">
              +12% from last month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Storage Used</CardTitle>
            <EyeIcon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalStorage} GB</div>
            <div className="flex items-center space-x-2 mt-2">
              <div className="flex-1 bg-gray-200 rounded-full h-2">
                <div className={`h-2 rounded-full ${getStorageColor()}`} style={{ width: `${getStoragePercentage()}%` }}></div>
              </div>
              <span className="text-xs text-gray-500">{getStoragePercentage()}%</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">This Month</CardTitle>
            <ChartBarIcon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">2,456</div>
            <p className="text-xs text-muted-foreground">
              +18% from last month
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.quickActions.map((action) => (
          <Link key={action.title} href={action.href}>
            <Card className="hover:shadow-md transition-shadow cursor-pointer">
              <CardContent className="p-6">
                <div className={`w-12 h-12 ${action.color} rounded-lg flex items-center justify-center mb-4`}>
                  <action.icon className="h-6 w-6 text-white" />
                </div>
                <h3 className="font-semibold text-gray-900">{action.title}</h3>
                <p className="text-sm text-gray-600 mt-1">{action.description}</p>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Uploads */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Uploads</CardTitle>
            <CardDescription>
              Your latest video uploads
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {stats.recentUploads.map((video) => (
                <div key={video.id} className="flex items-center space-x-4">
                  <div className="relative">
                    <img 
                      src={video.thumbnail} 
                      alt={video.title}
                      className="w-20 h-14 object-cover rounded"
                    />
                    <div className="absolute bottom-1 right-1 bg-black bg-opacity-75 text-white text-xs px-1 rounded">
                      {formatDuration(video.duration)}
                    </div>
                  </div>
                  <div className="flex-1 min-w-0">
                    <h4 className="text-sm font-medium text-gray-900 truncate">
                      {video.title}
                    </h4>
                    <div className="flex items-center space-x-2 mt-1">
                      {getStatusBadge(video.status)}
                      <span className="text-xs text-gray-500">
                        {formatDateTime(video.createdAt)}
                      </span>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-medium">{formatNumber(video.views)}</p>
                    <p className="text-xs text-gray-500">views</p>
                  </div>
                </div>
              ))}
            </div>
            <div className="mt-4">
              <Link href="/dashboard/videos">
                <Button variant="outline" className="w-full">
                  View all videos
                </Button>
              </Link>
            </div>
          </CardContent>
        </Card>

        {/* Top Videos */}
        <Card>
          <CardHeader>
            <CardTitle>Top Performing Videos</CardTitle>
            <CardDescription>
              Most viewed videos this month
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {stats.topVideos.map((video, index) => (
                <div key={video.id} className="flex items-center space-x-4">
                  <div className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center">
                    <span className="text-sm font-medium text-gray-600">{index + 1}</span>
                  </div>
                  <div className="flex-1 min-w-0">
                    <h4 className="text-sm font-medium text-gray-900 truncate">
                      {video.title}
                    </h4>
                    <div className="flex items-center space-x-2 mt-1">
                      <span className="text-sm text-gray-500">{formatNumber(video.views)} views</span>
                      {video.change > 0 ? (
                        <span className="flex items-center text-green-600 text-xs">
                          <ArrowTrendingUpIcon className="h-3 w-3 mr-1" />
                          {video.change}%
                        </span>
                      ) : (
                        <span className="flex items-center text-red-600 text-xs">
                          <ArrowTrendingDownIcon className="h-3 w-3 mr-1" />
                          {Math.abs(video.change)}%
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
            <div className="mt-4">
              <Link href="/dashboard/analytics">
                <Button variant="outline" className="w-full">
                  View analytics
                </Button>
              </Link>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
