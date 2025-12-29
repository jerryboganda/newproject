'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { 
  FolderIcon,
  PlusIcon,
  MagnifyingGlassIcon,
  EllipsisVerticalIcon,
  PencilIcon,
  TrashIcon,
  FolderOpenIcon,
  VideoCameraIcon,
  ClockIcon,
  ChevronRightIcon
} from '@heroicons/react/24/outline'
import { formatDateTime } from '@/lib/utils'

interface Collection {
  id: string
  name: string
  description?: string
  parentId?: string
  videoCount: number
  totalDuration: number
  createdAt: string
  updatedAt: string
  isPublic: boolean
  thumbnail?: string
  children?: Collection[]
}

export default function CollectionsPage() {
  const [collections, setCollections] = useState<Collection[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [newCollection, setNewCollection] = useState({
    name: '',
    description: '',
    parentId: ''
  })

  useEffect(() => {
    fetchCollections()
  }, [])

  const fetchCollections = async () => {
    try {
      // Mock API call - replace with actual implementation
      const mockCollections: Collection[] = [
        {
          id: '1',
          name: 'Product Demos',
          description: 'All product demonstration videos',
          videoCount: 12,
          totalDuration: 7200,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-15T10:30:00Z',
          isPublic: true,
          thumbnail: '/api/placeholder/320/180',
          children: [
            {
              id: '1-1',
              name: '2024 Demos',
              description: 'Latest product demos for 2024',
              videoCount: 5,
              totalDuration: 3000,
              createdAt: '2024-01-01T00:00:00Z',
              updatedAt: '2024-01-10T14:20:00Z',
              isPublic: true,
              parentId: '1'
            },
            {
              id: '1-2',
              name: 'Legacy Demos',
              description: 'Older product demonstrations',
              videoCount: 7,
              totalDuration: 4200,
              createdAt: '2023-12-01T00:00:00Z',
              updatedAt: '2023-12-15T09:00:00Z',
              isPublic: false,
              parentId: '1'
            }
          ]
        },
        {
          id: '2',
          name: 'Tutorials',
          description: 'Step-by-step tutorial videos',
          videoCount: 28,
          totalDuration: 14400,
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-14T16:45:00Z',
          isPublic: true,
          children: [
            {
              id: '2-1',
              name: 'Getting Started',
              description: 'Basic tutorials for beginners',
              videoCount: 10,
              totalDuration: 6000,
              createdAt: '2024-01-02T00:00:00Z',
              updatedAt: '2024-01-12T11:30:00Z',
              isPublic: true,
              parentId: '2'
            },
            {
              id: '2-2',
              name: 'Advanced Features',
              description: 'Advanced tutorials for power users',
              videoCount: 8,
              totalDuration: 4800,
              createdAt: '2024-01-05T00:00:00Z',
              updatedAt: '2024-01-13T14:00:00Z',
              isPublic: true,
              parentId: '2'
            },
            {
              id: '2-3',
              name: 'Tips & Tricks',
              description: 'Quick tips and tricks',
              videoCount: 10,
              totalDuration: 3600,
              createdAt: '2024-01-08T00:00:00Z',
              updatedAt: '2024-01-14T16:45:00Z',
              isPublic: false,
              parentId: '2'
            }
          ]
        },
        {
          id: '3',
          name: 'Webinars',
          description: 'Recorded webinar sessions',
          videoCount: 15,
          totalDuration: 27000,
          createdAt: '2024-01-03T00:00:00Z',
          updatedAt: '2024-01-16T13:20:00Z',
          isPublic: false
        },
        {
          id: '4',
          name: 'Testimonials',
          description: 'Customer success stories',
          videoCount: 8,
          totalDuration: 2400,
          createdAt: '2024-01-04T00:00:00Z',
          updatedAt: '2024-01-11T10:15:00Z',
          isPublic: true
        }
      ]
      
      setCollections(mockCollections)
    } catch (error) {
      console.error('Failed to fetch collections:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleCreateCollection = async () => {
    try {
      // Mock API call
      const createdCollection: Collection = {
        id: Date.now().toString(),
        name: newCollection.name,
        description: newCollection.description,
        videoCount: 0,
        totalDuration: 0,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        isPublic: false,
        parentId: newCollection.parentId || undefined
      }
      
      setCollections(prev => [...prev, createdCollection])
      setIsCreateDialogOpen(false)
      setNewCollection({ name: '', description: '', parentId: '' })
    } catch (error) {
      console.error('Failed to create collection:', error)
    }
  }

  const handleDelete = async (id: string) => {
    if (confirm('Are you sure you want to delete this collection?')) {
      try {
        // Mock API call
        setCollections(prev => prev.filter(c => c.id !== id))
      } catch (error) {
        console.error('Failed to delete collection:', error)
      }
    }
  }

  const formatDuration = (seconds: number) => {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    
    if (hours > 0) {
      return `${hours}h ${minutes}m`
    }
    return `${minutes}m`
  }

  const renderCollectionTree = (collections: Collection[], level = 0) => {
    return collections
      .filter(c => !c.parentId)
      .map(collection => (
        <div key={collection.id}>
          <Card className={`hover:shadow-md transition-shadow ${level > 0 ? 'ml-6' : ''}`}>
            <CardContent className="p-6">
              <div className="flex items-start justify-between">
                <Link 
                  href={`/dashboard/collections/${collection.id}`}
                  className="flex-1 group"
                >
                  <div className="flex items-center space-x-4">
                    <div className="w-16 h-16 bg-gray-100 rounded-lg flex items-center justify-center">
                      {collection.thumbnail ? (
                        <img 
                          src={collection.thumbnail} 
                          alt={collection.name}
                          className="w-full h-full object-cover rounded-lg"
                        />
                      ) : (
                        <FolderIcon className="h-8 w-8 text-gray-400" />
                      )}
                    </div>
                    
                    <div className="flex-1">
                      <div className="flex items-center space-x-2">
                        <h3 className="text-lg font-semibold text-gray-900 group-hover:text-blue-600">
                          {collection.name}
                        </h3>
                        {!collection.isPublic && (
                          <Badge variant="outline" className="text-xs">Private</Badge>
                        )}
                      </div>
                      
                      {collection.description && (
                        <p className="text-gray-600 mt-1">{collection.description}</p>
                      )}
                      
                      <div className="flex items-center space-x-4 mt-2 text-sm text-gray-500">
                        <span className="flex items-center">
                          <VideoCameraIcon className="h-4 w-4 mr-1" />
                          {collection.videoCount} videos
                        </span>
                        <span className="flex items-center">
                          <ClockIcon className="h-4 w-4 mr-1" />
                          {formatDuration(collection.totalDuration)}
                        </span>
                        <span>
                          Updated {formatDateTime(collection.updatedAt)}
                        </span>
                      </div>
                    </div>
                    
                    {collection.children && collection.children.length > 0 && (
                      <ChevronRightIcon className="h-5 w-5 text-gray-400" />
                    )}
                  </div>
                </Link>
                
                <div className="flex items-center space-x-2 ml-4">
                  <Link href={`/dashboard/collections/${collection.id}`}>
                    <Button variant="ghost" size="sm">
                      <FolderOpenIcon className="h-4 w-4" />
                    </Button>
                  </Link>
                  
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="sm">
                        <EllipsisVerticalIcon className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem asChild>
                        <Link href={`/dashboard/collections/${collection.id}/edit`}>
                          <PencilIcon className="h-4 w-4 mr-2" />
                          Edit
                        </Link>
                      </DropdownMenuItem>
                      <DropdownMenuItem>
                        <VideoCameraIcon className="h-4 w-4 mr-2" />
                        Add Videos
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        onClick={() => handleDelete(collection.id)}
                        className="text-red-600"
                      >
                        <TrashIcon className="h-4 w-4 mr-2" />
                        Delete
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              </div>
            </CardContent>
          </Card>
          
          {collection.children && (
            <div className="mt-4">
              {renderCollectionTree(collection.children, level + 1)}
            </div>
          )}
        </div>
      ))
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Collections</h2>
          <p className="text-gray-600">Organize your videos into collections</p>
        </div>
        
        <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <PlusIcon className="h-4 w-4 mr-2" />
              New Collection
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create New Collection</DialogTitle>
              <DialogDescription>
                Create a new collection to organize your videos
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div>
                <Label htmlFor="name">Name</Label>
                <Input
                  id="name"
                  value={newCollection.name}
                  onChange={(e) => setNewCollection(prev => ({ ...prev, name: e.target.value }))}
                  placeholder="Enter collection name"
                />
              </div>
              
              <div>
                <Label htmlFor="description">Description (optional)</Label>
                <Textarea
                  id="description"
                  value={newCollection.description}
                  onChange={(e) => setNewCollection(prev => ({ ...prev, description: e.target.value }))}
                  placeholder="Describe this collection"
                  rows={3}
                />
              </div>
              
              <div>
                <Label htmlFor="parent">Parent Collection (optional)</Label>
                <select
                  id="parent"
                  value={newCollection.parentId}
                  onChange={(e) => setNewCollection(prev => ({ ...prev, parentId: e.target.value }))}
                  className="w-full mt-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">None (Root Level)</option>
                  {collections
                    .filter(c => !c.parentId)
                    .map(collection => (
                      <option key={collection.id} value={collection.id}>
                        {collection.name}
                      </option>
                    ))}
                </select>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsCreateDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateCollection} disabled={!newCollection.name}>
                Create Collection
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      {/* Search */}
      <Card>
        <CardContent className="pt-6">
          <div className="relative">
            <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <Input
              placeholder="Search collections..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardContent>
      </Card>

      {/* Collections Tree */}
      <div className="space-y-4">
        {collections.length > 0 ? (
          renderCollectionTree(collections)
        ) : (
          <Card>
            <CardContent className="p-12 text-center">
              <FolderIcon className="mx-auto h-16 w-16 text-gray-400" />
              <h3 className="mt-4 text-lg font-medium text-gray-900">No collections yet</h3>
              <p className="mt-2 text-gray-600">
                Create your first collection to start organizing your videos
              </p>
              <Button className="mt-6" onClick={() => setIsCreateDialogOpen(true)}>
                <PlusIcon className="h-4 w-4 mr-2" />
                Create Collection
              </Button>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  )
}
