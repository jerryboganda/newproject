'use client'

import { useState, useEffect } from 'react'
import { useParams } from 'next/navigation'
import { VideoPlayer } from '@/components/videos/VideoPlayer'
import { Button } from '@/components/ui/button'
import { 
  ArrowTopRightOnSquareIcon,
  PlayIcon,
  SpeakerWaveIcon,
  SpeakerXMarkIcon,
  ArrowsPointingOutIcon,
  ClosedCaptionIcon
} from '@heroicons/react/24/outline'

interface VideoData {
  id: string
  title: string
  videoUrl: string
  thumbnail: string
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
  allowEmbed: boolean
  allowDownload: boolean
  playerSettings: {
    autoplay: boolean
    controls: boolean
    loop: boolean
    muted: boolean
    showTitle: boolean
    showShareButton: boolean
    defaultQuality: string
  }
}

export default function EmbedPage() {
  const params = useParams()
  const [video, setVideo] = useState<VideoData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [isHovered, setIsHovered] = useState(false)

  useEffect(() => {
    if (params.videoId) {
      fetchVideoData(params.videoId as string)
    }
  }, [params.videoId])

  const fetchVideoData = async (videoId: string) => {
    try {
      // Mock API call - replace with actual implementation
      const mockVideo: VideoData = {
        id: videoId,
        title: 'Product Demo 2024 - Complete Overview',
        videoUrl: '/api/placeholder/video.mp4',
        thumbnail: '/api/placeholder/1280/720',
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
          }
        ],
        allowEmbed: true,
        allowDownload: true,
        playerSettings: {
          autoplay: false,
          controls: true,
          loop: false,
          muted: false,
          showTitle: true,
          showShareButton: true,
          defaultQuality: 'auto'
        }
      }
      
      // Parse URL parameters for player settings
      const urlParams = new URLSearchParams(window.location.search)
      const settings = { ...mockVideo.playerSettings }
      
      if (urlParams.get('autoplay') === '1') settings.autoplay = true
      if (urlParams.get('controls') === '0') settings.controls = false
      if (urlParams.get('loop') === '1') settings.loop = true
      if (urlParams.get('muted') === '1') settings.muted = true
      if (urlParams.get('title') === '0') settings.showTitle = false
      if (urlParams.get('share') === '0') settings.showShareButton = false
      
      mockVideo.playerSettings = settings
      setVideo(mockVideo)
    } catch (err: any) {
      setError(err.message || 'Failed to load video')
    } finally {
      setLoading(false)
    }
  }

  const handleShare = () => {
    if (navigator.share) {
      navigator.share({
        title: video?.title,
        url: window.location.href
      })
    } else {
      navigator.clipboard.writeText(window.location.href)
      // Show toast notification
    }
  }

  const handleViewOriginal = () => {
    window.open(`${window.location.origin}/dashboard/videos/${video?.id}`, '_blank')
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen bg-black">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto mb-4"></div>
          <p className="text-white">Loading video...</p>
        </div>
      </div>
    )
  }

  if (error || !video) {
    return (
      <div className="flex items-center justify-center h-screen bg-black">
        <div className="text-center text-white">
          <h2 className="text-2xl font-bold mb-2">Video Not Available</h2>
          <p className="text-gray-400">{error || 'This video cannot be embedded'}</p>
        </div>
      </div>
    )
  }

  if (!video.allowEmbed) {
    return (
      <div className="flex items-center justify-center h-screen bg-black">
        <div className="text-center text-white">
          <h2 className="text-2xl font-bold mb-2">Embedding Disabled</h2>
          <p className="text-gray-400 mb-4">This video cannot be embedded on external sites</p>
          <Button onClick={handleViewOriginal}>
            <ArrowTopRightOnSquareIcon className="h-4 w-4 mr-2" />
            View on Original Site
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="h-screen bg-black flex flex-col">
      {/* Video Title Bar */}
      {video.playerSettings.showTitle && (
        <div className="bg-gray-900 px-4 py-2 flex items-center justify-between">
          <h3 className="text-white font-medium truncate flex-1">
            {video.title}
          </h3>
          {video.playerSettings.showShareButton && (
            <Button
              variant="ghost"
              size="sm"
              onClick={handleShare}
              className="text-white hover:bg-gray-800 ml-4"
            >
              <ArrowTopRightOnSquareIcon className="h-4 w-4" />
            </Button>
          )}
        </div>
      )}

      {/* Video Player Container */}
      <div 
        className="flex-1 relative"
        onMouseEnter={() => setIsHovered(true)}
        onMouseLeave={() => setIsHovered(false)}
      >
        <VideoPlayer
          src={video.videoUrl}
          poster={video.thumbnail}
          captions={video.captions}
          chapters={video.chapters}
          autoplay={video.playerSettings.autoplay}
          controls={video.playerSettings.controls}
          loop={video.playerSettings.loop}
          muted={video.playerSettings.muted}
          className="w-full h-full"
        />

        {/* Watermark/Logo Overlay */}
        <div className="absolute bottom-4 right-4 opacity-75">
          <div className="flex items-center space-x-2 bg-black bg-opacity-50 px-3 py-1 rounded">
            <div className="w-6 h-6 bg-blue-600 rounded"></div>
            <span className="text-white text-sm font-medium">StreamVault</span>
          </div>
        </div>

        {/* Hover Overlay for Additional Controls */}
        {isHovered && video.playerSettings.controls && (
          <div className="absolute top-4 right-4 flex space-x-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={handleShare}
              className="bg-black bg-opacity-50 text-white hover:bg-opacity-75"
            >
              <ArrowTopRightOnSquareIcon className="h-4 w-4" />
            </Button>
          </div>
        )}
      </div>

      {/* Minimal Controls (when controls are disabled) */}
      {!video.playerSettings.controls && (
        <div className="bg-gray-900 px-4 py-3 flex items-center justify-center space-x-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              const videoElement = document.querySelector('video')
              if (videoElement) {
                if (videoElement.paused) {
                  videoElement.play()
                } else {
                  videoElement.pause()
                }
              }
            }}
            className="text-white hover:bg-gray-800"
          >
            <PlayIcon className="h-5 w-5" />
          </Button>
          
          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              const videoElement = document.querySelector('video')
              if (videoElement) {
                videoElement.muted = !videoElement.muted
              }
            }}
            className="text-white hover:bg-gray-800"
          >
            <SpeakerWaveIcon className="h-5 w-5" />
          </Button>

          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              const container = document.querySelector('.h-screen')
              if (container && !document.fullscreenElement) {
                container.requestFullscreen()
              } else if (document.fullscreenElement) {
                document.exitFullscreen()
              }
            }}
            className="text-white hover:bg-gray-800"
          >
            <ArrowsPointingOutIcon className="h-5 w-5" />
          </Button>

          <Button
            variant="ghost"
            size="sm"
            onClick={handleViewOriginal}
            className="text-white hover:bg-gray-800"
          >
            <ArrowTopRightOnSquareIcon className="h-4 w-4" />
          </Button>
        </div>
      )}
    </div>
  )
}
