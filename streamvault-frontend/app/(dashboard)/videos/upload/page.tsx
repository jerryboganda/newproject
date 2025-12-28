'use client'

import { useState, useCallback, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Textarea } from '@/components/ui/textarea'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { Progress } from "@/components/ui/progress"
import {
  CloudArrowUpIcon,
  XMarkIcon,
  DocumentIcon,
  FilmIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  ArrowPathIcon
} from '@heroicons/react/24/outline'
import { formatBytes } from '@/lib/utils'

interface UploadFile {
  file: File
  id: string
  progress: number
  status: 'pending' | 'uploading' | 'processing' | 'completed' | 'error'
  error?: string
  videoId?: string
}

export default function VideoUploadPage() {
  const router = useRouter()
  const [files, setFiles] = useState<UploadFile[]>([])
  const [isDragOver, setIsDragOver] = useState(false)
  const [uploading, setUploading] = useState(false)
  const [globalError, setGlobalError] = useState('')
  const fileInputRef = useRef<HTMLInputElement>(null)
  
  // Form state
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [category, setCategory] = useState('')
  const [tags, setTags] = useState<string[]>([])
  const [isPublic, setIsPublic] = useState(false)
  const [allowDownload, setAllowDownload] = useState(true)
  const [allowEmbed, setAllowEmbed] = useState(true)

  const categories = [
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

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setIsDragOver(true)
  }, [])

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setIsDragOver(false)
  }, [])

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    setIsDragOver(false)
    
    const droppedFiles = Array.from(e.dataTransfer.files)
    handleFiles(droppedFiles)
  }, [])

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      handleFiles(Array.from(e.target.files))
    }
  }

  const handleFiles = (newFiles: File[]) => {
    const validFiles = newFiles.filter(file => {
      if (!file.type.startsWith('video/')) {
        setGlobalError(`${file.name} is not a valid video file`)
        return false
      }
      if (file.size > 5 * 1024 * 1024 * 1024) { // 5GB limit
        setGlobalError(`${file.name} exceeds the 5GB size limit`)
        return false
      }
      return true
    })

    const uploadFiles: UploadFile[] = validFiles.map(file => ({
      file,
      id: Math.random().toString(36).substr(2, 9),
      progress: 0,
      status: 'pending'
    }))

    setFiles(prev => [...prev, ...uploadFiles])
    setGlobalError('')
  }

  const removeFile = (id: string) => {
    setFiles(prev => prev.filter(f => f.id !== id))
  }

  const simulateUpload = async (file: UploadFile) => {
    // Update status to uploading
    setFiles(prev => prev.map(f => 
      f.id === file.id ? { ...f, status: 'uploading' } : f
    ))

    // Simulate upload progress
    for (let progress = 0; progress <= 100; progress += 10) {
      await new Promise(resolve => setTimeout(resolve, 200))
      setFiles(prev => prev.map(f => 
        f.id === file.id ? { ...f, progress } : f
      ))
    }

    // Simulate processing
    setFiles(prev => prev.map(f => 
      f.id === file.id ? { ...f, status: 'processing', progress: 0 } : f
    ))

    await new Promise(resolve => setTimeout(resolve, 1000))

    // Complete
    setFiles(prev => prev.map(f => 
      f.id === file.id ? { 
        ...f, 
        status: 'completed', 
        videoId: `video-${f.id}`
      } : f
    ))
  }

  const handleUpload = async () => {
    if (files.length === 0) return

    setUploading(true)
    setGlobalError('')

    try {
      // Upload all files
      await Promise.all(files.map(file => simulateUpload(file)))
    } catch (error) {
      setGlobalError('Upload failed. Please try again.')
    } finally {
      setUploading(false)
    }
  }

  const handleContinue = () => {
    const completedFiles = files.filter(f => f.status === 'completed')
    if (completedFiles.length > 0) {
      router.push('/dashboard/videos')
    }
  }

  const getStatusIcon = (status: UploadFile['status']) => {
    switch (status) {
      case 'pending':
        return <DocumentIcon className="h-5 w-5 text-gray-400" />
      case 'uploading':
        return <ArrowPathIcon className="h-5 w-5 text-blue-500 animate-spin" />
      case 'processing':
        return <ArrowPathIcon className="h-5 w-5 text-yellow-500 animate-spin" />
      case 'completed':
        return <CheckCircleIcon className="h-5 w-5 text-green-500" />
      case 'error':
        return <ExclamationTriangleIcon className="h-5 w-5 text-red-500" />
    }
  }

  const getStatusText = (status: UploadFile['status']) => {
    switch (status) {
      case 'pending':
        return 'Pending'
      case 'uploading':
        return 'Uploading...'
      case 'processing':
        return 'Processing...'
      case 'completed':
        return 'Completed'
      case 'error':
        return 'Error'
    }
  }

  const allCompleted = files.length > 0 && files.every(f => f.status === 'completed')
  const hasActiveUploads = files.some(f => f.status === 'uploading' || f.status === 'processing')

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-gray-900">Upload Videos</h2>
        <p className="text-gray-600">Add new content to your video library</p>
      </div>

      {globalError && (
        <Alert variant="destructive">
          <AlertDescription>{globalError}</AlertDescription>
        </Alert>
      )}

      {/* Upload Area */}
      <Card>
        <CardHeader>
          <CardTitle>Select Videos</CardTitle>
        </CardHeader>
        <CardContent>
          <div
            className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
              isDragOver 
                ? 'border-blue-500 bg-blue-50' 
                : 'border-gray-300 hover:border-gray-400'
            }`}
            onDragOver={handleDragOver}
            onDragLeave={handleDragLeave}
            onDrop={handleDrop}
          >
            <CloudArrowUpIcon className="mx-auto h-12 w-12 text-gray-400" />
            <div className="mt-4">
              <p className="text-lg font-medium text-gray-900">
                Drop videos here or click to browse
              </p>
              <p className="text-sm text-gray-600 mt-1">
                Supported formats: MP4, MOV, AVI, WebM (Max 5GB per file)
              </p>
            </div>
            <input
              ref={fileInputRef}
              type="file"
              multiple
              accept="video/*"
              onChange={handleFileSelect}
              className="hidden"
            />
            <Button
              variant="outline"
              className="mt-4"
              onClick={() => fileInputRef.current?.click()}
              disabled={uploading}
            >
              Select Videos
            </Button>
          </div>

          {/* File List */}
          {files.length > 0 && (
            <div className="mt-6 space-y-3">
              <h4 className="font-medium text-gray-900">Selected Files</h4>
              {files.map((file) => (
                <div key={file.id} className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
                  {getStatusIcon(file.status)}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 truncate">
                      {file.file.name}
                    </p>
                    <div className="flex items-center space-x-2 mt-1">
                      <span className="text-xs text-gray-500">
                        {formatBytes(file.file.size)}
                      </span>
                      <span className="text-xs text-gray-400">•</span>
                      <span className="text-xs text-gray-500">
                        {getStatusText(file.status)}
                      </span>
                    </div>
                    {(file.status === 'uploading' || file.status === 'processing') && (
                      <Progress value={file.progress} className="mt-2" />
                    )}
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => removeFile(file.id)}
                    disabled={file.status === 'uploading' || file.status === 'processing'}
                  >
                    <XMarkIcon className="h-4 w-4" />
                  </Button>
                </div>
              ))}
            </div>
          )}

          {/* Upload Button */}
          {files.length > 0 && (
            <div className="mt-6 flex justify-end">
              <Button
                onClick={handleUpload}
                disabled={uploading || hasActiveUploads}
              >
                {uploading ? (
                  <>
                    <ArrowPathIcon className="h-4 w-4 mr-2 animate-spin" />
                    Uploading...
                  </>
                ) : (
                  <>
                    <CloudArrowUpIcon className="h-4 w-4 mr-2" />
                    Upload {files.length} Video{files.length > 1 ? 's' : ''}
                  </>
                )}
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Video Details */}
      {files.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Video Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label htmlFor="title">Title</Label>
              <Input
                id="title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Enter video title"
              />
            </div>

            <div>
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Describe your video"
                rows={3}
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="category">Category</Label>
                <Select value={category} onValueChange={setCategory}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select category" />
                  </SelectTrigger>
                  <SelectContent>
                    {categories.map(cat => (
                      <SelectItem key={cat} value={cat}>{cat}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div>
                <Label>Tags</Label>
                <div className="mt-2 flex flex-wrap gap-2">
                  {availableTags.map(tag => (
                    <label key={tag} className="flex items-center space-x-1">
                      <Checkbox
                        checked={tags.includes(tag)}
                        onCheckedChange={(checked) => {
                          if (checked) {
                            setTags(prev => [...prev, tag])
                          } else {
                            setTags(prev => prev.filter(t => t !== tag))
                          }
                        }}
                      />
                      <span className="text-sm">{tag}</span>
                    </label>
                  ))}
                </div>
              </div>
            </div>

            <div className="space-y-3">
              <label className="flex items-center space-x-2">
                <Checkbox
                  checked={isPublic}
                  onCheckedChange={(checked) => setIsPublic(checked as boolean)}
                />
                <span className="text-sm">Make video public</span>
              </label>

              <label className="flex items-center space-x-2">
                <Checkbox
                  checked={allowDownload}
                  onCheckedChange={(checked) => setAllowDownload(checked as boolean)}
                />
                <span className="text-sm">Allow downloads</span>
              </label>

              <label className="flex items-center space-x-2">
                <Checkbox
                  checked={allowEmbed}
                  onCheckedChange={(checked) => setAllowEmbed(checked as boolean)}
                />
                <span className="text-sm">Allow embedding</span>
              </label>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Success State */}
      {allCompleted && (
        <Alert className="bg-green-50 border-green-200">
          <CheckCircleIcon className="h-4 w-4 text-green-600" />
          <AlertDescription className="text-green-800">
            All videos uploaded successfully!
          </AlertDescription>
          <div className="mt-3">
            <Button onClick={handleContinue}>
              Continue to Videos
            </Button>
          </div>
        </Alert>
      )}
    </div>
  )
}
