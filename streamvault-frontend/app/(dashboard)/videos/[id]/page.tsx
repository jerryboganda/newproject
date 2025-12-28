'use client'

import { useState, useEffect } from 'react'
import { useParams, useRouter } from 'next/navigation'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { VideoPlayer } from '@/components/videos/VideoPlayer'
import { 
  ArrowLeftIcon,
  PlayIcon,
  EyeIcon,
  CalendarIcon,
  ClockIcon,
  TagIcon,
  FolderIcon,
  ShareIcon,
  ArrowDownTrayIcon,
  PencilIcon,
  TrashIcon,
  EllipsisVerticalIcon,
  ChartBarIcon,
  ClosedCaptionIcon,
  QueueListIcon,
  CodeBracketIcon
} from '@heroicons/react/24/outline'
import { formatDateTime, formatDuration, formatNumber } from '@/lib/utils'

interface VideoDetails {
  id: string
  title: string
  description: string
  thumbnail: string
  videoUrl: string
  duration: number
  views: number
  createdAt: string
  updatedAt: string
  status: 'processing' | 'ready' | 'failed'
  isPublic: boolean
  allowDownload: boolean
  allowEmbed: boolean
  tags: string[]
  category: string
  size: number
  resolution: string
  fps: number
  codec: string
  captions: Array<{
    id: string
    language: string
    label: string
    url: string
  }>
  chapters: Array<{
    id: string
    title: string
    startTime: number
    endTime: number
  }>
  analytics: {
    views: number
    uniqueViews: number
    averageWatchTime: number
    completionRate: number
    engagement: number
  }
}

export default function VideoDetailsPage() {
  const params = useParams()
  const router = useRouter()
  const [video, setVideo] = useState<VideoDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [embedCode, setEmbedCode] = useState('')

  useEffect(() => {
    if (params.id) {
      fetchVideoDetails(params.id as string)
    }
  }, [params.id])

  useEffect(() => {
    if (video) {
      const embedUrl = `${window.location.origin}/embed/${video.id}`
      setEmbedCode(`<iframe src="${embedUrl}" width="640" height="360" frameborder="0" allowfullscreen></iframe>`)
    }
  }, [video])

  const fetchVideoDetails = async (id: string) => {
    try {
      // Mock API call - replace with actual implementation
      const mockVideo: VideoDetails = {
        id: id,
        title: 'Product Demo 2024 - Complete Overview',
        description: 'A comprehensive demonstration of our latest product features and capabilities. This video covers everything from basic setup to advanced features, helping users get the most out of our platform.',
        thumbnail: '/api/placeholder/1280/720',
        videoUrl: '/api/placeholder/video.mp4',
        duration: 580,
        views: 1250,
        createdAt: '2024-01-15T10:30:00Z',
        updatedAt: '2024-01-15T10:30:00Z',
        status: 'ready',
        isPublic: true,
        allowDownload: true,
        allowEmbed: true,
        tags: ['featured', 'demo', 'published'],
        category: 'Demo',
        size: 125829120, // 120MB
        resolution: '1920x1080',
        fps: 30,
        codec: 'H.264',
        captions: [
          {
            id: '1',
            language: 'en',
            label: 'English',
            url: '/api/captions/en.vtt'
          },
          {
            id: '2',
            language: 'es',
            label: 'Spanish',
            url: '/api/captions/es.vtt'
          }
        ],
        chapters: [
          {
            id: '1',
            title: 'Introduction',
            startTime: 0,
            endTime: 60
          },
          {
            id: '2',
            title: 'Getting Started',
            startTime: 60,
            endTime: 180
          },
          {
            id: '3',
            title: 'Core Features',
            startTime: 180,
            endTime: 420
          },
          {
            id: '4',
            title: 'Advanced Features',
            startTime: 420,
            endTime: 540
          },
          {
            id: '5',
            title: 'Conclusion',
            startTime: 540,
            endTime: 580
          }
        ],
        analytics: {
          views: 1250,
          uniqueViews: 980,
          averageWatchTime: 420,
          completionRate: 72,
          engagement: 85
        }
      }
      
      setVideo(mockVideo)
    } catch (err: any) {
      setError(err.message || 'Failed to load video')
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async () => {
    if (!video) return
    
    if (confirm('Are you sure you want to delete this video? This action cannot be undone.')) {
      try {
        // Mock API call
        await new Promise(resolve => setTimeout(resolve, 1000))
        router.push('/dashboard/videos')
      } catch (error) {
        setError('Failed to delete video')
      }
    }
  }

  const handleCopyEmbedCode = () => {
    navigator.clipboard.writeText(embedCode)
    // Show toast notification
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  if (error || !video) {
    return (
      <Alert variant="destructive">
        <AlertDescription>{error || 'Video not found'}</AlertDescription>
      </Alert>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Link href="/dashboard/videos">
            <Button variant="ghost" size="sm">
              <ArrowLeftIcon className="h-4 w-4 mr-2" />
              Back to Videos
            </Button>
          </Link>
          <div>
            <h2 className="text-2xl font-bold text-gray-900">{video.title}</h2>
            <p className="text-gray-600">Video details and settings</p>
          </div>
        </div>
        
        <div className="flex items-center space-x-2">
          <Link href={`/dashboard/videos/${video.id}/edit`}>
            <Button variant="outline">
              <PencilIcon className="h-4 w-4 mr-2" />
              Edit
            </Button>
          </Link>
          
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline">
                <EllipsisVerticalIcon className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={handleCopyEmbedCode}>
                <CodeBracketIcon className="h-4 w-4 mr-2" />
                Copy Embed Code
              </DropdownMenuItem>
              <DropdownMenuItem>
                <ShareIcon className="h-4 w-4 mr-2" />
                Share
              </DropdownMenuItem>
              <DropdownMenuItem>
                <ArrowDownTrayIcon className="h-4 w-4 mr-2" />
                Download
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link href={`/dashboard/videos/${video.id}/analytics`}>
                  <ChartBarIcon className="h-4 w-4 mr-2" />
                  View Analytics
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={handleDelete}
                className="text-red-600"
              >
                <TrashIcon className="h-4 w-4 mr-2" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>

      {/* Video Player and Info */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2">
          <Card>
            <CardContent className="p-0">
              <VideoPlayer
                src={video.videoUrl}
                poster={video.thumbnail}
                captions={video.captions}
                chapters={video.chapters}
              />
            </CardContent>
          </Card>

          {/* Tabs */}
          <Tabs defaultValue="details" className="mt-6">
            <TabsList>
              <TabsTrigger value="details">Details</TabsTrigger>
              <TabsTrigger value="captions">
                <ClosedCaptionIcon className="h-4 w-4 mr-2" />
                Captions
              </TabsTrigger>
              <TabsTrigger value="chapters">
                <QueueListIcon className="h-4 w-4 mr-2" />
                Chapters
              </TabsTrigger>
              <TabsTrigger value="embed">
                <CodeBracketIcon className="h-4 w-4 mr-2" />
                Embed
              </TabsTrigger>
            </TabsList>

            <TabsContent value="details" className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle>Video Information</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <h4 className="font-medium text-gray-900 mb-2">Description</h4>
                    <p className="text-gray-600">{video.description}</p>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <h4 className="font-medium text-gray-900 mb-2">Metadata</h4>
                      <dl className="space-y-1 text-sm">
                        <div className="flex justify-between">
                          <dt className="text-gray-500">Duration:</dt>
                          <dd>{formatDuration(video.duration)}</dd>
                        </div>
                        <div className="flex justify-between">
                          <dt className="text-gray-500">Size:</dt>
                          <dd>{(video.size / (1024 * 1024)).toFixed(2)} MB</dd>
                        </div>
                        <div className="flex justify-between">
                          <dt className="text-gray-500">Resolution:</dt>
                          <dd>{video.resolution}</dd>
                        </div>
                        <div className="flex justify-between">
                          <dt className="text-gray-500">Frame Rate:</dt>
                          <dd>{video.fps} fps</dd>
                        </div>
                        <div className="flex justify-between">
                          <dt className="text-gray-500">Codec:</dt>
                          <dd>{video.codec}</dd>
                        </div>
                      </dl>
                    </div>

                    <div>
                      <h4 className="font-medium text-gray-900 mb-2">Settings</h4>
                      <dl className="space-y-1 text-sm">
                        <div className="flex justify-between">
                          <dt className="text-gray-500">Visibility:</dt>
                          <dd>{video.isPublic ? 'Public' : 'Private'}</dd>
                        </div>
                        <div className="flex justify-between">
                          <dt className="text-gray-500">Downloads:</dt>
                          <dd>{video.allowDownload ? 'Allowed' : 'Disabled'}</dd>
                        </div>
                        <div className="flex justify-between">
                          <dt className="text-gray-500">Embedding:</dt>
                          <dd>{video.allowEmbed ? 'Allowed' : 'Disabled'}</dd>
                        </div>
                      </dl>
                    </div>
                  </div>

                  <div>
                    <h4 className="font-medium text-gray-900 mb-2">Category</h4>
                    <Badge variant="outline">{video.category}</Badge>
                  </div>

                  <div>
                    <h4 className="font-medium text-gray-900 mb-2">Tags</h4>
                    <div className="flex flex-wrap gap-2">
                      {video.tags.map(tag => (
                        <Badge key={tag} variant="secondary">{tag}</Badge>
                      ))}
                    </div>
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="captions">
              <Card>
                <CardHeader>
                  <CardTitle>Captions & Subtitles</CardTitle>
                </CardHeader>
                <CardContent>
                  {video.captions.length > 0 ? (
                    <div className="space-y-3">
                      {video.captions.map(caption => (
                        <div key={caption.id} className="flex items-center justify-between p-3 border rounded-lg">
                          <div>
                            <p className="font-medium">{caption.label}</p>
                            <p className="text-sm text-gray-500">{caption.language}</p>
                          </div>
                          <div className="flex space-x-2">
                            <Button variant="outline" size="sm">
                              Edit
                            </Button>
                            <Button variant="outline" size="sm">
                              Download
                            </Button>
                          </div>
                        </div>
                      ))}
                      <Button className="w-full">
                        Add New Caption
                      </Button>
                    </div>
                  ) : (
                    <div className="text-center py-8">
                      <ClosedCaptionIcon className="mx-auto h-12 w-12 text-gray-400" />
                      <h3 className="mt-2 text-sm font-medium text-gray-900">No captions</h3>
                      <p className="mt-1 text-sm text-gray-500">Add captions to make your video accessible</p>
                      <Button className="mt-4">
                        Add Caption
                      </Button>
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="chapters">
              <Card>
                <CardHeader>
                  <CardTitle>Video Chapters</CardTitle>
                </CardHeader>
                <CardContent>
                  {video.chapters.length > 0 ? (
                    <div className="space-y-3">
                      {video.chapters.map((chapter, index) => (
                        <div key={chapter.id} className="flex items-center justify-between p-3 border rounded-lg">
                          <div className="flex items-center space-x-3">
                            <span className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center text-sm font-medium">
                              {index + 1}
                            </span>
                            <div>
                              <p className="font-medium">{chapter.title}</p>
                              <p className="text-sm text-gray-500">
                                {formatDuration(chapter.startTime)} - {formatDuration(chapter.endTime)}
                              </p>
                            </div>
                          </div>
                          <div className="flex space-x-2">
                            <Button variant="outline" size="sm">
                              Edit
                            </Button>
                          </div>
                        </div>
                      ))}
                      <Button className="w-full">
                        Add New Chapter
                      </Button>
                    </div>
                  ) : (
                    <div className="text-center py-8">
                      <QueueListIcon className="mx-auto h-12 w-12 text-gray-400" />
                      <h3 className="mt-2 text-sm font-medium text-gray-900">No chapters</h3>
                      <p className="mt-1 text-sm text-gray-500">Add chapters to help viewers navigate</p>
                      <Button className="mt-4">
                        Add Chapter
                      </Button>
                    </div>
                  )}
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="embed">
              <Card>
                <CardHeader>
                  <CardTitle>Embed Video</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Embed Code
                    </label>
                    <div className="relative">
                      <textarea
                        value={embedCode}
                        readOnly
                        className="w-full p-3 border rounded-lg bg-gray-50 font-mono text-sm"
                        rows={4}
                      />
                      <Button
                        variant="outline"
                        size="sm"
                        className="absolute top-2 right-2"
                        onClick={handleCopyEmbedCode}
                      >
                        Copy
                      </Button>
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Preview
                    </label>
                    <div className="border rounded-lg p-4 bg-gray-50">
                      <div dangerouslySetInnerHTML={{ __html: embedCode }} />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Width
                      </label>
                      <Input defaultValue="640" />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Height
                      </label>
                      <Input defaultValue="360" />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <label className="flex items-center space-x-2">
                      <input type="checkbox" defaultChecked />
                      <span className="text-sm">Show player controls</span>
                    </label>
                    <label className="flex items-center space-x-2">
                      <input type="checkbox" defaultChecked />
                      <span className="text-sm">Autoplay</span>
                    </label>
                    <label className="flex items-center space-x-2">
                      <input type="checkbox" defaultChecked />
                      <span className="text-sm">Loop video</span>
                    </label>
                  </div>
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Stats */}
          <Card>
            <CardHeader>
              <CardTitle>Analytics</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="flex items-center text-sm text-gray-600">
                    <EyeIcon className="h-4 w-4 mr-1" />
                    Total Views
                  </span>
                  <span className="font-medium">{formatNumber(video.analytics.views)}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Unique Views</span>
                  <span className="font-medium">{formatNumber(video.analytics.uniqueViews)}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Avg. Watch Time</span>
                  <span className="font-medium">{formatDuration(video.analytics.averageWatchTime)}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Completion Rate</span>
                  <span className="font-medium">{video.analytics.completionRate}%</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Engagement</span>
                  <span className="font-medium">{video.analytics.engagement}%</span>
                </div>
              </div>
              <Link href={`/dashboard/videos/${video.id}/analytics`}>
                <Button variant="outline" className="w-full mt-4">
                  View Detailed Analytics
                </Button>
              </Link>
            </CardContent>
          </Card>

          {/* Quick Actions */}
          <Card>
            <CardHeader>
              <CardTitle>Quick Actions</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <Button variant="outline" className="w-full justify-start">
                <ShareIcon className="h-4 w-4 mr-2" />
                Share Video
              </Button>
              <Button variant="outline" className="w-full justify-start">
                <ArrowDownTrayIcon className="h-4 w-4 mr-2" />
                Download Video
              </Button>
              <Button variant="outline" className="w-full justify-start">
                <ClosedCaptionIcon className="h-4 w-4 mr-2" />
                Add Captions
              </Button>
              <Button variant="outline" className="w-full justify-start">
                <QueueListIcon className="h-4 w-4 mr-2" />
                Add Chapters
              </Button>
            </CardContent>
          </Card>

          {/* Video Info */}
          <Card>
            <CardHeader>
              <CardTitle>Video Info</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-gray-600">Uploaded</span>
                <span>{formatDateTime(video.createdAt)}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-gray-600">Last Modified</span>
                <span>{formatDateTime(video.updatedAt)}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-gray-600">Status</span>
                <Badge className="bg-green-100 text-green-800">Ready</Badge>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
