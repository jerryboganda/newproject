'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { 
  PlusIcon,
  MagnifyingGlassIcon,
  PencilIcon,
  TrashIcon,
  EllipsisVerticalIcon,
  ShieldCheckIcon,
  UserGroupIcon,
  KeyIcon,
  EyeIcon
} from '@heroicons/react/24/outline'

interface Role {
  id: string
  name: string
  description: string
  permissions: string[]
  userCount: number
  isSystem: boolean
  createdAt: string
}

interface Permission {
  id: string
  name: string
  category: string
  description: string
}

export default function RolesPage() {
  const [roles, setRoles] = useState<Role[]>([])
  const [permissions, setPermissions] = useState<Permission[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)
  const [selectedRole, setSelectedRole] = useState<Role | null>(null)
  const [newRole, setNewRole] = useState({
    name: '',
    description: '',
    permissions: [] as string[]
  })

  useEffect(() => {
    fetchRoles()
    fetchPermissions()
  }, [])

  const fetchRoles = async () => {
    try {
      // Mock data - replace with actual API call
      const mockRoles: Role[] = [
        {
          id: '1',
          name: 'Super Admin',
          description: 'Full system access with all permissions',
          permissions: ['*'],
          userCount: 2,
          isSystem: true,
          createdAt: '2024-01-01T00:00:00Z'
        },
        {
          id: '2',
          name: 'Admin',
          description: 'Full tenant access with management permissions',
          permissions: ['users.manage', 'content.manage', 'analytics.view', 'billing.manage'],
          userCount: 5,
          isSystem: false,
          createdAt: '2024-01-15T10:30:00Z'
        },
        {
          id: '3',
          name: 'Moderator',
          description: 'Content moderation and user management',
          permissions: ['content.moderate', 'users.view', 'analytics.view'],
          userCount: 12,
          isSystem: false,
          createdAt: '2024-02-01T14:20:00Z'
        },
        {
          id: '4',
          name: 'Editor',
          description: 'Create and edit content',
          permissions: ['content.create', 'content.edit', 'media.upload'],
          userCount: 25,
          isSystem: false,
          createdAt: '2024-02-15T09:00:00Z'
        },
        {
          id: '5',
          name: 'Viewer',
          description: 'View-only access to content',
          permissions: ['content.view'],
          userCount: 150,
          isSystem: false,
          createdAt: '2024-03-01T11:45:00Z'
        }
      ]
      setRoles(mockRoles)
    } catch (error) {
      console.error('Failed to fetch roles:', error)
    } finally {
      setLoading(false)
    }
  }

  const fetchPermissions = async () => {
    try {
      // Mock permissions
      const mockPermissions: Permission[] = [
        { id: 'users.manage', name: 'Manage Users', category: 'Users', description: 'Create, edit, and delete users' },
        { id: 'users.view', name: 'View Users', category: 'Users', description: 'View user list and profiles' },
        { id: 'content.manage', name: 'Manage Content', category: 'Content', description: 'Full control over all content' },
        { id: 'content.create', name: 'Create Content', category: 'Content', description: 'Create new content' },
        { id: 'content.edit', name: 'Edit Content', category: 'Content', description: 'Edit existing content' },
        { id: 'content.moderate', name: 'Moderate Content', category: 'Content', description: 'Approve or reject content' },
        { id: 'content.view', name: 'View Content', category: 'Content', description: 'View published content' },
        { id: 'media.upload', name: 'Upload Media', category: 'Media', description: 'Upload images and videos' },
        { id: 'analytics.view', name: 'View Analytics', category: 'Analytics', description: 'View analytics and reports' },
        { id: 'billing.manage', name: 'Manage Billing', category: 'Billing', description: 'Manage subscriptions and payments' },
        { id: 'settings.manage', name: 'Manage Settings', category: 'Settings', description: 'Configure system settings' },
      ]
      setPermissions(mockPermissions)
    } catch (error) {
      console.error('Failed to fetch permissions:', error)
    }
  }

  const handleCreateRole = async () => {
    try {
      // Mock API call
      const createdRole: Role = {
        id: Date.now().toString(),
        name: newRole.name,
        description: newRole.description,
        permissions: newRole.permissions,
        userCount: 0,
        isSystem: false,
        createdAt: new Date().toISOString()
      }
      setRoles([...roles, createdRole])
      setIsCreateDialogOpen(false)
      setNewRole({ name: '', description: '', permissions: [] })
    } catch (error) {
      console.error('Failed to create role:', error)
    }
  }

  const handleDeleteRole = async () => {
    if (!selectedRole) return
    
    try {
      // Mock API call
      setRoles(roles.filter(r => r.id !== selectedRole.id))
      setIsDeleteDialogOpen(false)
      setSelectedRole(null)
    } catch (error) {
      console.error('Failed to delete role:', error)
    }
  }

  const togglePermission = (permissionId: string) => {
    setNewRole(prev => ({
      ...prev,
      permissions: prev.permissions.includes(permissionId)
        ? prev.permissions.filter(p => p !== permissionId)
        : [...prev.permissions, permissionId]
    }))
  }

  const getPermissionsByCategory = () => {
    const grouped: Record<string, Permission[]> = {}
    permissions.forEach(permission => {
      if (!grouped[permission.category]) {
        grouped[permission.category] = []
      }
      grouped[permission.category].push(permission)
    })
    return grouped
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
          <h2 className="text-2xl font-bold text-gray-900">Roles & Permissions</h2>
          <p className="text-gray-600">Manage user roles and their permissions</p>
        </div>
        <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <PlusIcon className="h-4 w-4 mr-2" />
              Create Role
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Create New Role</DialogTitle>
              <DialogDescription>
                Define a new role with specific permissions
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="role-name">Role Name</Label>
                  <Input
                    id="role-name"
                    value={newRole.name}
                    onChange={(e) => setNewRole(prev => ({ ...prev, name: e.target.value }))}
                    placeholder="e.g. Content Manager"
                  />
                </div>
                <div>
                  <Label htmlFor="role-description">Description</Label>
                  <Input
                    id="role-description"
                    value={newRole.description}
                    onChange={(e) => setNewRole(prev => ({ ...prev, description: e.target.value }))}
                    placeholder="Brief description of the role"
                  />
                </div>
              </div>
              
              <div>
                <Label>Permissions</Label>
                <div className="mt-2 space-y-4 max-h-60 overflow-y-auto">
                  {Object.entries(getPermissionsByCategory()).map(([category, perms]) => (
                    <div key={category}>
                      <h4 className="font-medium text-sm text-gray-900 mb-2">{category}</h4>
                      <div className="space-y-2">
                        {perms.map((permission) => (
                          <label key={permission.id} className="flex items-center space-x-2">
                            <input
                              type="checkbox"
                              checked={newRole.permissions.includes(permission.id)}
                              onChange={() => togglePermission(permission.id)}
                              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                            />
                            <div>
                              <span className="text-sm font-medium">{permission.name}</span>
                              <p className="text-xs text-gray-500">{permission.description}</p>
                            </div>
                          </label>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsCreateDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleCreateRole} disabled={!newRole.name}>
                Create Role
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
              placeholder="Search roles..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardContent>
      </Card>

      {/* Roles Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {roles
          .filter(role => 
            role.name.toLowerCase().includes(search.toLowerCase()) ||
            role.description.toLowerCase().includes(search.toLowerCase())
          )
          .map((role) => (
          <Card key={role.id} className="relative">
            <CardHeader className="pb-3">
              <div className="flex items-start justify-between">
                <div className="flex items-center space-x-2">
                  <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                    <ShieldCheckIcon className="h-6 w-6 text-blue-600" />
                  </div>
                  <div>
                    <CardTitle className="text-lg">{role.name}</CardTitle>
                    {role.isSystem && (
                      <Badge variant="secondary" className="text-xs">
                        System Role
                      </Badge>
                    )}
                  </div>
                </div>
                {!role.isSystem && (
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="sm">
                        <EllipsisVerticalIcon className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem asChild>
                        <Link href={`/admin/roles/${role.id}`}>
                          <EyeIcon className="h-4 w-4 mr-2" />
                          View Details
                        </Link>
                      </DropdownMenuItem>
                      <DropdownMenuItem asChild>
                        <Link href={`/admin/roles/${role.id}/edit`}>
                          <PencilIcon className="h-4 w-4 mr-2" />
                          Edit
                        </Link>
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        onClick={() => {
                          setSelectedRole(role)
                          setIsDeleteDialogOpen(true)
                        }}
                        className="text-red-600"
                      >
                        <TrashIcon className="h-4 w-4 mr-2" />
                        Delete
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                )}
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-gray-600 mb-4">{role.description}</p>
              
              <div className="space-y-3">
                <div className="flex items-center justify-between text-sm">
                  <span className="flex items-center text-gray-500">
                    <UserGroupIcon className="h-4 w-4 mr-1" />
                    Users
                  </span>
                  <span className="font-medium">{role.userCount}</span>
                </div>
                
                <div className="flex items-center justify-between text-sm">
                  <span className="flex items-center text-gray-500">
                    <KeyIcon className="h-4 w-4 mr-1" />
                    Permissions
                  </span>
                  <span className="font-medium">
                    {role.permissions[0] === '*' ? 'All' : role.permissions.length}
                  </span>
                </div>
              </div>

              {role.permissions.length > 0 && role.permissions[0] !== '*' && (
                <div className="mt-4">
                  <div className="flex flex-wrap gap-1">
                    {role.permissions.slice(0, 3).map((permission) => (
                      <Badge key={permission} variant="outline" className="text-xs">
                        {permission.split('.').pop()}
                      </Badge>
                    ))}
                    {role.permissions.length > 3 && (
                      <Badge variant="outline" className="text-xs">
                        +{role.permissions.length - 3} more
                      </Badge>
                    )}
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Delete Confirmation Dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Role</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete the role "{selectedRole?.name}"? 
              This action cannot be undone and will affect {selectedRole?.userCount} users.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDeleteDialogOpen(false)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDeleteRole}>
              Delete Role
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
