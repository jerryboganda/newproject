'use client'

import Link from 'next/link'
import { useState } from 'react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { 
  PlayIcon,
  EyeIcon,
  ClockIcon,
  CalendarIcon,
  EllipsisVerticalIcon,
  PencilIcon,
  TrashIcon,
  ShareIcon,
  ArrowDownTrayIcon,
  ChartBarIcon
} from '@heroicons/react/24/outline'
import { formatDateTime, formatDuration, formatNumber } from '@/lib/utils'

interface VideoCardProps {
  video: {
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
  onDelete?: (id: string) => void
  onDuplicate?: (id: string) => void
  showActions?: boolean
  size?: 'small' | 'medium' | 'large'
}

export function VideoCard({ 
  video, 
  onDelete, 
  onDuplicate, 
  showActions = true,
  size = 'medium' 
}: VideoCardProps) {
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)
  const [imageError, setImageError] = useState(false)

  const handleDelete = () => {
    if (onDelete) {
      onDelete(video.id)
    }
    setIsDeleteDialogOpen(false)
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

  const sizeClasses = {
    small: {
      container: 'max-w-xs',
      thumbnail: 'h-32',
      title: 'text-sm',
      description: 'text-xs'
    },
    medium: {
      container: 'max-w-sm',
      thumbnail: 'h-40',
      title: 'text-base',
      description: 'text-sm'
    },
    large: {
      container: 'max-w-md',
      thumbnail: 'h-48',
      title: 'text-lg',
      description: 'text-base'
    }
  }

  const classes = sizeClasses[size]

  return (
    <>
      <div className={`group bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow ${classes.container}`}>
        {/* Thumbnail */}
        <Link href={`/dashboard/videos/${video.id}`}>
          <div className={`relative ${classes.thumbnail} bg-gray-100`}>
            {!imageError ? (
              <img
                src={video.thumbnail}
                alt={video.title}
                className="w-full h-full object-cover"
                onError={() => setImageError(true)}
              />
            ) : (
              <div className="w-full h-full flex items-center justify-center bg-gray-200">
                <PlayIcon className="h-12 w-12 text-gray-400" />
              </div>
            )}
            
            {/* Duration badge */}
            <div className="absolute bottom-2 right-2 bg-black bg-opacity-75 text-white text-xs px-2 py-1 rounded">
              {formatDuration(video.duration)}
            </div>
            
            {/* Play button overlay */}
            <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity bg-black bg-opacity-30">
              <div className="w-12 h-12 bg-white rounded-full flex items-center justify-center">
                <PlayIcon className="h-6 w-6 text-gray-900 ml-1" />
              </div>
            </div>

            {/* Status badge */}
            <div className="absolute top-2 left-2">
              {getStatusBadge(video.status)}
            </div>

            {/* Public indicator */}
            {video.isPublic && (
              <div className="absolute top-2 right-2">
                <Badge variant="secondary" className="text-xs">
                  Public
                </Badge>
              </div>
            )}
          </div>
        </Link>

        {/* Content */}
        <div className="p-4">
          <Link href={`/dashboard/videos/${video.id}`}>
            <h3 className={`font-semibold text-gray-900 line-clamp-2 hover:text-blue-600 transition-colors ${classes.title}`}>
              {video.title}
            </h3>
          </Link>

          {video.description && size !== 'small' && (
            <p className={`text-gray-600 mt-1 line-clamp-2 ${classes.description}`}>
              {video.description}
            </p>
          )}

          {/* Tags */}
          {video.tags && video.tags.length > 0 && size !== 'small' && (
            <div className="flex flex-wrap gap-1 mt-2">
              {video.tags.slice(0, 3).map((tag) => (
                <Badge key={tag} variant="outline" className="text-xs">
                  {tag}
                </Badge>
              ))}
              {video.tags.length > 3 && (
                <Badge variant="outline" className="text-xs">
                  +{video.tags.length - 3}
                </Badge>
              )}
            </div>
          )}

          {/* Metadata */}
          <div className="flex items-center justify-between mt-3 text-sm text-gray-500">
            <div className="flex items-center space-x-3">
              <span className="flex items-center">
                <EyeIcon className="h-4 w-4 mr-1" />
                {formatNumber(video.views)}
              </span>
              <span className="flex items-center">
                <CalendarIcon className="h-4 w-4 mr-1" />
                {new Date(video.createdAt).toLocaleDateString()}
              </span>
            </div>
          </div>

          {/* Actions */}
          {showActions && (
            <div className="flex items-center justify-between mt-3">
              <div className="flex space-x-1">
                <Link href={`/dashboard/videos/${video.id}/analytics`}>
                  <Button variant="ghost" size="sm">
                    <ChartBarIcon className="h-4 w-4" />
                  </Button>
                </Link>
                <Button variant="ghost" size="sm">
                  <ShareIcon className="h-4 w-4" />
                </Button>
              </div>

              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="sm">
                    <EllipsisVerticalIcon className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem asChild>
                    <Link href={`/dashboard/videos/${video.id}/edit`}>
                      <PencilIcon className="h-4 w-4 mr-2" />
                      Edit
                    </Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem asChild>
                    <Link href={`/dashboard/videos/${video.id}`}>
                      <EyeIcon className="h-4 w-4 mr-2" />
                      View Details
                    </Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem>
                    <ArrowDownTrayIcon className="h-4 w-4 mr-2" />
                    Download
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => onDuplicate?.(video.id)}>
                    <ShareIcon className="h-4 w-4 mr-2" />
                    Duplicate
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    onClick={() => setIsDeleteDialogOpen(true)}
                    className="text-red-600"
                  >
                    <TrashIcon className="h-4 w-4 mr-2" />
                    Delete
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          )}
        </div>
      </div>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Video</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete "{video.title}"? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-red-600 hover:bg-red-700">
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  )
}
