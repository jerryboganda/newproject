'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Checkbox } from '@/components/ui/checkbox'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { 
  CheckIcon,
  XMarkIcon,
  FunnelIcon,
  ArrowDownTrayIcon
} from '@heroicons/react/24/outline'

interface Permission {
  id: string
  name: string
  category: string
  description: string
}

interface Role {
  id: string
  name: string
  permissions: string[]
  isSystem: boolean
}

interface PermissionMatrixProps {
  roles: Role[]
  permissions: Permission[]
  onPermissionChange?: (roleId: string, permissionId: string, granted: boolean) => void
  editable?: boolean
}

export function PermissionMatrix({ 
  roles, 
  permissions, 
  onPermissionChange,
  editable = false 
}: PermissionMatrixProps) {
  const [selectedCategory, setSelectedCategory] = useState<string>('all')
  const [showOnlyDifferences, setShowOnlyDifferences] = useState(false)

  const categories = ['all', ...Array.from(new Set(permissions.map(p => p.category)))]
  
  const filteredPermissions = selectedCategory === 'all' 
    ? permissions 
    : permissions.filter(p => p.category === selectedCategory)

  const getPermissionValue = (role: Role, permissionId: string) => {
    if (role.permissions.includes('*')) return true
    return role.permissions.includes(permissionId)
  }

  const handleToggle = (roleId: string, permissionId: string, currentValue: boolean) => {
    if (editable && onPermissionChange) {
      onPermissionChange(roleId, permissionId, !currentValue)
    }
  }

  const exportMatrix = () => {
    const csvContent = [
      ['Permission', 'Description', ...roles.map(r => r.name)],
      ...filteredPermissions.map(permission => [
        permission.name,
        permission.description,
        ...roles.map(role => getPermissionValue(role, permission.id) ? '✓' : '✗')
      ])
    ].map(row => row.join(',')).join('\n')

    const blob = new Blob([csvContent], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'permission-matrix.csv'
    a.click()
    URL.revokeObjectURL(url)
  }

  const getPermissionStats = (permissionId: string) => {
    const granted = roles.filter(role => getPermissionValue(role, permissionId)).length
    return { granted, total: roles.length }
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Permission Matrix</CardTitle>
          <div className="flex items-center space-x-2">
            {editable && (
              <Badge variant="outline" className="text-green-600">
                Edit Mode
              </Badge>
            )}
            <Button variant="outline" size="sm" onClick={exportMatrix}>
              <ArrowDownTrayIcon className="h-4 w-4 mr-1" />
              Export
            </Button>
          </div>
        </div>
        
        <div className="flex items-center space-x-4 mt-4">
          <div className="flex items-center space-x-2">
            <FunnelIcon className="h-4 w-4 text-gray-500" />
            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="text-sm border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            >
              {categories.map(cat => (
                <option key={cat} value={cat}>
                  {cat === 'all' ? 'All Categories' : cat}
                </option>
              ))}
            </select>
          </div>
          
          {editable && (
            <label className="flex items-center space-x-2 text-sm">
              <Checkbox
                checked={showOnlyDifferences}
                onCheckedChange={(checked) => setShowOnlyDifferences(checked as boolean)}
              />
              <span>Show only differences</span>
            </label>
          )}
        </div>
      </CardHeader>
      
      <CardContent>
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-[300px]">Permission</TableHead>
                <TableHead className="w-[400px]">Description</TableHead>
                {roles.map((role) => (
                  <TableHead key={role.id} className="text-center min-w-[120px]">
                    <div className="flex flex-col items-center">
                      <span className="font-medium">{role.name}</span>
                      {role.isSystem && (
                        <Badge variant="secondary" className="text-xs mt-1">
                          System
                        </Badge>
                      )}
                    </div>
                  </TableHead>
                ))}
                <TableHead className="text-center">Coverage</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredPermissions.map((permission) => {
                const stats = getPermissionStats(permission.id)
                const hasDifferences = stats.granted > 0 && stats.granted < stats.total
                
                if (showOnlyDifferences && !hasDifferences) {
                  return null
                }
                
                return (
                  <TableRow key={permission.id} className="hover:bg-gray-50">
                    <TableCell className="font-medium">
                      <div>
                        {permission.name}
                        <Badge variant="outline" className="ml-2 text-xs">
                          {permission.category}
                        </Badge>
                      </div>
                    </TableCell>
                    <TableCell className="text-sm text-gray-600">
                      {permission.description}
                    </TableCell>
                    {roles.map((role) => {
                      const hasPermission = getPermissionValue(role, permission.id)
                      return (
                        <TableCell key={`${role.id}-${permission.id}`} className="text-center">
                          {editable && !role.isSystem ? (
                            <Checkbox
                              checked={hasPermission}
                              onCheckedChange={() => handleToggle(role.id, permission.id, hasPermission)}
                              className="mx-auto"
                            />
                          ) : (
                            <div className="flex justify-center">
                              {hasPermission ? (
                                <CheckIcon className="h-5 w-5 text-green-500" />
                              ) : (
                                <XMarkIcon className="h-5 w-5 text-gray-400" />
                              )}
                            </div>
                          )}
                        </TableCell>
                      )
                    })}
                    <TableCell className="text-center">
                      <div className="flex items-center justify-center">
                        <div className="w-16 bg-gray-200 rounded-full h-2 mr-2">
                          <div 
                            className="bg-blue-600 h-2 rounded-full"
                            style={{ width: `${(stats.granted / stats.total) * 100}%` }}
                          ></div>
                        </div>
                        <span className="text-sm text-gray-600">
                          {stats.granted}/{stats.total}
                        </span>
                      </div>
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        </div>
        
        {filteredPermissions.length === 0 && (
          <div className="text-center py-8 text-gray-500">
            No permissions found for the selected category
          </div>
        )}
        
        <div className="mt-6 p-4 bg-gray-50 rounded-lg">
          <h4 className="font-medium text-gray-900 mb-2">Legend</h4>
          <div className="flex items-center space-x-6 text-sm">
            <div className="flex items-center">
              <CheckIcon className="h-4 w-4 text-green-500 mr-1" />
              <span>Permission granted</span>
            </div>
            <div className="flex items-center">
              <XMarkIcon className="h-4 w-4 text-gray-400 mr-1" />
              <span>Permission denied</span>
            </div>
            <div className="flex items-center">
              <div className="w-4 h-4 border border-gray-300 rounded mr-1"></div>
              <span>Editable (in edit mode)</span>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
