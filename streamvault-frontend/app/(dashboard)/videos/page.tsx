'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { VideoCard } from '@/components/videos/VideoCard'
import { 
  MagnifyingGlassIcon,
  PlusIcon,
  AdjustmentsHorizontalIcon,
  CloudArrowUpIcon,
  FunnelIcon,
  ArrowDownTrayIcon,
  EyeIcon,
  TrashIcon,
  PencilIcon,
  DocumentDuplicateIcon
} from '@heroicons/react/24/outline'

interface Video {
  id: string
  title: string
  description?: string
  thumbnail: string
  duration: number
  views: number
  createdAt: string
  updatedAt: string
  status: 'processing' | 'ready' | 'failed'
  isPublic: boolean
  tags?: string[]
  category?: string
}

interface Filters {
  search: string
  status: string
  category: string
  tags: string[]
  dateRange: string
  sortBy: string
  sortOrder: 'asc' | 'desc'
}

export default function VideosPage() {
  const [videos, setVideos] = useState<Video[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedVideos, setSelectedVideos] = useState<string[]>([])
  const [showFilters, setShowFilters] = useState(false)
  const [filters, setFilters] = useState<Filters>({
    search: '',
    status: 'all',
    category: 'all',
    tags: [],
    dateRange: 'all',
    sortBy: 'createdAt',
    sortOrder: 'desc'
  })

  const categories = [
    'All',
    'Tutorial',
    'Demo',
    'Webinar',
    'Testimonial',
    'Presentation',
    'Marketing',
    'Other'
  ]

  const availableTags = [
    'featured',
    'tutorial',
    'new',
    'popular',
    'archive',
    'draft',
    'published',
    'internal',
    'external'
  ]

  useEffect(() => {
    fetchVideos()
  }, [filters])

  const fetchVideos = async () => {
    setLoading(true)
    try {
      // Mock API call - replace with actual implementation
      const mockVideos: Video[] = [
        {
          id: '1',
          title: 'Product Demo 2024 - Complete Overview',
          description: 'A comprehensive demonstration of our latest product features and capabilities.',
          thumbnail: '/api/placeholder/320/180',
          duration: 580,
          views: 1250,
          createdAt: '2024-01-15T10:30:00Z',
          updatedAt: '2024-01-15T10:30:00Z',
          status: 'ready',
          isPublic: true,
          tags: ['featured', 'demo', 'published'],
          category: 'Demo'
        },
        {
          id: '2',
          title: 'Getting Started Tutorial',
          description: 'Learn the basics of using our platform in this step-by-step tutorial.',
          thumbnail: '/api/placeholder/320/180',
          duration: 1240,
          views: 3420,
          createdAt: '2024-01-10T14:20:00Z',
          updatedAt: '2024-01-10T14:20:00Z',
          status: 'ready',
          isPublic: true,
          tags: ['tutorial', 'new', 'published'],
          category: 'Tutorial'
        },
        {
          id: '3',
          title: 'Customer Success Story - TechCorp',
          description: 'How TechCorp transformed their business using our solutions.',
          thumbnail: '/api/placeholder/320/180',
          duration: 360,
          views: 890,
          createdAt: '2024-01-08T09:15:00Z',
          updatedAt: '2024-01-08T09:15:00Z',
          status: 'processing',
          isPublic: false,
          tags: ['testimonial', 'featured'],
          category: 'Testimonial'
        },
        {
          id: '4',
          title: 'Monthly Webinar - January 2024',
          description: 'Join our monthly webinar to learn about the latest updates and best practices.',
          thumbnail: '/api/placeholder/320/180',
          duration: 3600,
          views: 2100,
          createdAt: '2024-01-05T16:00:00Z',
          updatedAt: '2024-01-05T16:00:00Z',
          status: 'ready',
          isPublic: true,
          tags: ['webinar', 'archive'],
          category: 'Webinar'
        },
        {
          id: '5',
          title: 'Internal Training - Sales Team',
          description: 'Internal training session for the sales team on new product features.',
          thumbnail: '/api/placeholder/320/180',
          duration: 1800,
          views: 45,
          createdAt: '2024-01-03T11:30:00Z',
          updatedAt: '2024-01-03T11:30:00Z',
          status: 'ready',
          isPublic: false,
          tags: ['internal', 'training'],
          category: 'Other'
        },
        {
          id: '6',
          title: 'Marketing Campaign Launch',
          description: 'Announcing our latest marketing campaign with exciting new features.',
          thumbnail: '/api/placeholder/320/180',
          duration: 90,
          views: 5670,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
          status: 'ready',
          isPublic: true,
          tags: ['marketing', 'featured', 'popular'],
          category: 'Marketing'
        }
      ]
      
      // Apply filters
      let filteredVideos = mockVideos

      if (filters.search) {
        filteredVideos = filteredVideos.filter(video =>
          video.title.toLowerCase().includes(filters.search.toLowerCase()) ||
          video.description?.toLowerCase().includes(filters.search.toLowerCase())
        )
      }

      if (filters.status !== 'all') {
        filteredVideos = filteredVideos.filter(video => video.status === filters.status)
      }

      if (filters.category !== 'all') {
        filteredVideos = filteredVideos.filter(video => video.category === filters.category)
      }

      if (filters.tags.length > 0) {
        filteredVideos = filteredVideos.filter(video =>
          filters.tags.every(tag => video.tags?.includes(tag))
        )
      }

      // Sort videos
      filteredVideos.sort((a, b) => {
        const aValue = a[filters.sortBy as keyof Video]
        const bValue = b[filters.sortBy as keyof Video]
        
        if (filters.sortOrder === 'asc') {
          return aValue > bValue ? 1 : -1
        } else {
          return aValue < bValue ? 1 : -1
        }
      })

      setVideos(filteredVideos)
    } catch (error) {
      console.error('Failed to fetch videos:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleSelectVideo = (videoId: string) => {
    setSelectedVideos(prev =>
      prev.includes(videoId)
        ? prev.filter(id => id !== videoId)
        : [...prev, videoId]
    )
  }

  const handleSelectAll = () => {
    if (selectedVideos.length === videos.length) {
      setSelectedVideos([])
    } else {
      setSelectedVideos(videos.map(v => v.id))
    }
  }

  const handleBulkAction = (action: string) => {
    console.log(`Bulk action: ${action}`, selectedVideos)
    // Implement bulk actions
  }

  const clearFilters = () => {
    setFilters({
      search: '',
      status: 'all',
      category: 'all',
      tags: [],
      dateRange: 'all',
      sortBy: 'createdAt',
      sortOrder: 'desc'
    })
  }

  const hasActiveFilters = filters.search || 
    filters.status !== 'all' || 
    filters.category !== 'all' || 
    filters.tags.length > 0

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Videos</h2>
          <p className="text-gray-600">Manage your video library</p>
        </div>
        <Link href="/dashboard/videos/upload">
          <Button>
            <CloudArrowUpIcon className="h-4 w-4 mr-2" />
            Upload Video
          </Button>
        </Link>
      </div>

      {/* Search and Filters */}
      <Card>
        <CardContent className="p-6">
          <div className="flex flex-col lg:flex-row gap-4">
            {/* Search */}
            <div className="flex-1 relative">
              <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Search videos..."
                value={filters.search}
                onChange={(e) => setFilters(prev => ({ ...prev, search: e.target.value }))}
                className="pl-10"
              />
            </div>

            {/* Quick Filters */}
            <div className="flex items-center gap-2">
              <Select
                value={filters.status}
                onValueChange={(value) => setFilters(prev => ({ ...prev, status: value }))}
              >
                <SelectTrigger className="w-[140px]">
                  <SelectValue placeholder="Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="ready">Ready</SelectItem>
                  <SelectItem value="processing">Processing</SelectItem>
                  <SelectItem value="failed">Failed</SelectItem>
                </SelectContent>
              </Select>

              <Select
                value={filters.category}
                onValueChange={(value) => setFilters(prev => ({ ...prev, category: value }))}
              >
                <SelectTrigger className="w-[140px]">
                  <SelectValue placeholder="Category" />
                </SelectTrigger>
                <SelectContent>
                  {categories.map(category => (
                    <SelectItem key={category} value={category.toLowerCase()}>
                      {category}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <Button
                variant="outline"
                onClick={() => setShowFilters(!showFilters)}
                className={hasActiveFilters ? 'border-blue-500 text-blue-600' : ''}
              >
                <AdjustmentsHorizontalIcon className="h-4 w-4 mr-2" />
                Filters
                {hasActiveFilters && (
                  <Badge variant="secondary" className="ml-2 text-xs">
                    {[
                      filters.search && 'search',
                      filters.status !== 'all' && 'status',
                      filters.category !== 'all' && 'category',
                      filters.tags.length > 0 && 'tags'
                    ].filter(Boolean).length}
                  </Badge>
                )}
              </Button>
            </div>

            {/* Sort */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  Sort
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => setFilters(prev => ({ ...prev, sortBy: 'createdAt', sortOrder: 'desc' }))}>
                  Newest First
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setFilters(prev => ({ ...prev, sortBy: 'createdAt', sortOrder: 'asc' }))}>
                  Oldest First
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setFilters(prev => ({ ...prev, sortBy: 'title', sortOrder: 'asc' }))}>
                  Title A-Z
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setFilters(prev => ({ ...prev, sortBy: 'title', sortOrder: 'desc' }))}>
                  Title Z-A
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setFilters(prev => ({ ...prev, sortBy: 'views', sortOrder: 'desc' }))}>
                  Most Viewed
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>

          {/* Advanced Filters */}
          {showFilters && (
            <div className="mt-4 pt-4 border-t border-gray-200">
              <div className="space-y-4">
                {/* Tags */}
                <div>
                  <h4 className="text-sm font-medium text-gray-900 mb-2">Tags</h4>
                  <div className="flex flex-wrap gap-2">
                    {availableTags.map(tag => (
                      <label key={tag} className="flex items-center space-x-2">
                        <Checkbox
                          checked={filters.tags.includes(tag)}
                          onCheckedChange={(checked) => {
                            if (checked) {
                              setFilters(prev => ({ ...prev, tags: [...prev.tags, tag] }))
                            } else {
                              setFilters(prev => ({ ...prev, tags: prev.tags.filter(t => t !== tag) }))
                            }
                          }}
                        />
                        <span className="text-sm">{tag}</span>
                      </label>
                    ))}
                  </div>
                </div>

                {/* Clear Filters */}
                <div className="flex justify-end">
                  <Button variant="outline" onClick={clearFilters}>
                    Clear All Filters
                  </Button>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Bulk Actions */}
      {selectedVideos.length > 0 && (
        <Card className="bg-blue-50 border-blue-200">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <span className="text-sm text-blue-900">
                  {selectedVideos.length} video{selectedVideos.length > 1 ? 's' : ''} selected
                </span>
              </div>
              <div className="flex items-center space-x-2">
                <Button variant="outline" size="sm" onClick={() => handleBulkAction('delete')}>
                  <TrashIcon className="h-4 w-4 mr-1" />
                  Delete
                </Button>
                <Button variant="outline" size="sm" onClick={() => handleBulkAction('duplicate')}>
                  <DocumentDuplicateIcon className="h-4 w-4 mr-1" />
                  Duplicate
                </Button>
                <Button variant="outline" size="sm" onClick={() => handleBulkAction('download')}>
                  <ArrowDownTrayIcon className="h-4 w-4 mr-1" />
                  Download
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Video Grid */}
      {loading ? (
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      ) : videos.length > 0 ? (
        <>
          <div className="flex items-center justify-between">
            <Checkbox
              checked={selectedVideos.length === videos.length && videos.length > 0}
              onCheckedChange={handleSelectAll}
              className="mr-2"
            />
            <span className="text-sm text-gray-600">
              {videos.length} video{videos.length !== 1 ? 's' : ''}
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {videos.map((video) => (
              <div key={video.id} className="relative">
                <Checkbox
                  checked={selectedVideos.includes(video.id)}
                  onCheckedChange={() => handleSelectVideo(video.id)}
                  className="absolute top-2 left-2 z-10 bg-white"
                />
                <VideoCard
                  video={video}
                  onDelete={(id) => {
                    setVideos(prev => prev.filter(v => v.id !== id))
                    setSelectedVideos(prev => prev.filter(v => v !== id))
                  }}
                  onDuplicate={(id) => {
                    console.log('Duplicate video:', id)
                  }}
                />
              </div>
            ))}
          </div>
        </>
      ) : (
        <Card>
          <CardContent className="p-12 text-center">
            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <VideoCameraIcon className="h-8 w-8 text-gray-400" />
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">No videos found</h3>
            <p className="text-gray-600 mb-6">
              {hasActiveFilters 
                ? 'Try adjusting your filters or search terms'
                : 'Get started by uploading your first video'
              }
            </p>
            <div className="space-x-3">
              {hasActiveFilters ? (
                <Button onClick={clearFilters}>
                  Clear Filters
                </Button>
              ) : (
                <Link href="/dashboard/videos/upload">
                  <Button>
                    <CloudArrowUpIcon className="h-4 w-4 mr-2" />
                    Upload Video
                  </Button>
                </Link>
              )}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
