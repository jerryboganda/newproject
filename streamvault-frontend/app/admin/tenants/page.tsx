'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { apiClient } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { 
  PlusIcon,
  MagnifyingGlassIcon,
  BuildingOfficeIcon,
  UsersIcon,
  CreditCardIcon,
  EllipsisVerticalIcon,
  PencilIcon,
  TrashIcon,
  EyeIcon
} from '@heroicons/react/24/outline'
import { formatDateTime, formatNumber } from '@/lib/utils'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

interface Tenant {
  id: string
  name: string
  subdomain: string
  status: 'active' | 'inactive' | 'suspended'
  createdAt: string
  userCount: number
  subscriptionPlan: string
  monthlyRevenue: number
  storageUsed: number
  storageLimit: number
}

export default function TenantsPage() {
  const [tenants, setTenants] = useState<Tenant[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)

  useEffect(() => {
    fetchTenants()
  }, [page, search])

  const fetchTenants = async () => {
    setLoading(true)
    try {
      // Mock data for now - replace with actual API call
      const mockTenants: Tenant[] = [
        {
          id: '1',
          name: 'Acme Corporation',
          subdomain: 'acme',
          status: 'active',
          createdAt: '2024-01-15T10:30:00Z',
          userCount: 125,
          subscriptionPlan: 'Enterprise',
          monthlyRevenue: 299,
          storageUsed: 45.6,
          storageLimit: 100,
        },
        {
          id: '2',
          name: 'Tech Industries',
          subdomain: 'techindustries',
          status: 'active',
          createdAt: '2024-02-20T14:15:00Z',
          userCount: 89,
          subscriptionPlan: 'Professional',
          monthlyRevenue: 149,
          storageUsed: 23.4,
          storageLimit: 50,
        },
        {
          id: '3',
          name: 'Global Media',
          subdomain: 'globalmedia',
          status: 'inactive',
          createdAt: '2024-03-10T09:00:00Z',
          userCount: 45,
          subscriptionPlan: 'Business',
          monthlyRevenue: 99,
          storageUsed: 12.8,
          storageLimit: 25,
        },
        {
          id: '4',
          name: 'Digital Solutions',
          subdomain: 'digitalsolutions',
          status: 'suspended',
          createdAt: '2024-01-05T16:45:00Z',
          userCount: 200,
          subscriptionPlan: 'Enterprise',
          monthlyRevenue: 299,
          storageUsed: 89.2,
          storageLimit: 100,
        },
        {
          id: '5',
          name: 'Cloud Services',
          subdomain: 'cloudservices',
          status: 'active',
          createdAt: '2024-04-01T11:30:00Z',
          userCount: 67,
          subscriptionPlan: 'Business',
          monthlyRevenue: 99,
          storageUsed: 18.9,
          storageLimit: 25,
        },
      ]
      
      setTenants(mockTenants)
      setTotalPages(Math.ceil(mockTenants.length / 10))
    } catch (error) {
      console.error('Failed to fetch tenants:', error)
    } finally {
      setLoading(false)
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active':
        return 'bg-green-100 text-green-800'
      case 'inactive':
        return 'bg-gray-100 text-gray-800'
      case 'suspended':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getStoragePercentage = (used: number, limit: number) => {
    return Math.round((used / limit) * 100)
  }

  const getStorageColor = (percentage: number) => {
    if (percentage >= 90) return 'bg-red-500'
    if (percentage >= 70) return 'bg-yellow-500'
    return 'bg-green-500'
  }

  const handleDelete = async (tenantId: string) => {
    if (confirm('Are you sure you want to delete this tenant? This action cannot be undone.')) {
      try {
        await apiClient.tenants.deleteTenant(tenantId)
        fetchTenants()
      } catch (error) {
        console.error('Failed to delete tenant:', error)
      }
    }
  }

  const handleSuspend = async (tenantId: string) => {
    try {
      await apiClient.tenants.updateTenant(tenantId, { status: 'suspended' })
      fetchTenants()
    } catch (error) {
      console.error('Failed to suspend tenant:', error)
    }
  }

  const handleActivate = async (tenantId: string) => {
    try {
      await apiClient.tenants.updateTenant(tenantId, { status: 'active' })
      fetchTenants()
    } catch (error) {
      console.error('Failed to activate tenant:', error)
    }
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
          <h2 className="text-2xl font-bold text-gray-900">Tenants</h2>
          <p className="text-gray-600">Manage all tenant accounts</p>
        </div>
        <Link href="/admin/tenants/create">
          <Button>
            <PlusIcon className="h-4 w-4 mr-2" />
            Add Tenant
          </Button>
        </Link>
      </div>

      {/* Search and Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center space-x-4">
            <div className="flex-1 relative">
              <MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Search tenants..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pl-10"
              />
            </div>
            <Button variant="outline">
              Filter
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Tenants List */}
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b bg-gray-50">
                  <th className="text-left py-3 px-6 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tenant
                  </th>
                  <th className="text-left py-3 px-6 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="text-left py-3 px-6 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Users
                  </th>
                  <th className="text-left py-3 px-6 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Plan
                  </th>
                  <th className="text-left py-3 px-6 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Storage
                  </th>
                  <th className="text-left py-3 px-6 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Revenue
                  </th>
                  <th className="text-left py-3 px-6 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Created
                  </th>
                  <th className="text-right py-3 px-6 text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {tenants.map((tenant) => {
                  const storagePercentage = getStoragePercentage(tenant.storageUsed, tenant.storageLimit)
                  return (
                    <tr key={tenant.id} className="hover:bg-gray-50">
                      <td className="py-4 px-6">
                        <div className="flex items-center">
                          <div className="w-10 h-10 bg-gray-200 rounded-lg flex items-center justify-center">
                            <BuildingOfficeIcon className="h-6 w-6 text-gray-500" />
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">
                              {tenant.name}
                            </div>
                            <div className="text-sm text-gray-500">
                              {tenant.subdomain}.streamvault.com
                            </div>
                          </div>
                        </div>
                      </td>
                      <td className="py-4 px-6">
                        <Badge className={getStatusColor(tenant.status)}>
                          {tenant.status}
                        </Badge>
                      </td>
                      <td className="py-4 px-6">
                        <div className="flex items-center text-sm text-gray-900">
                          <UsersIcon className="h-4 w-4 mr-1 text-gray-400" />
                          {formatNumber(tenant.userCount)}
                        </div>
                      </td>
                      <td className="py-4 px-6">
                        <div className="text-sm text-gray-900">{tenant.subscriptionPlan}</div>
                      </td>
                      <td className="py-4 px-6">
                        <div className="w-32">
                          <div className="flex items-center justify-between text-sm mb-1">
                            <span className="text-gray-900">
                              {tenant.storageUsed} GB
                            </span>
                            <span className="text-gray-500">
                              / {tenant.storageLimit} GB
                            </span>
                          </div>
                          <div className="w-full bg-gray-200 rounded-full h-2">
                            <div
                              className={`h-2 rounded-full ${getStorageColor(storagePercentage)}`}
                              style={{ width: `${storagePercentage}%` }}
                            ></div>
                          </div>
                        </div>
                      </td>
                      <td className="py-4 px-6">
                        <div className="text-sm font-medium text-gray-900">
                          ${formatNumber(tenant.monthlyRevenue)}
                        </div>
                        <div className="text-xs text-gray-500">/month</div>
                      </td>
                      <td className="py-4 px-6">
                        <div className="text-sm text-gray-500">
                          {formatDateTime(tenant.createdAt)}
                        </div>
                      </td>
                      <td className="py-4 px-6 text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="sm">
                              <EllipsisVerticalIcon className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem asChild>
                              <Link href={`/admin/tenants/${tenant.id}`}>
                                <EyeIcon className="h-4 w-4 mr-2" />
                                View Details
                              </Link>
                            </DropdownMenuItem>
                            <DropdownMenuItem asChild>
                              <Link href={`/admin/tenants/${tenant.id}/edit`}>
                                <PencilIcon className="h-4 w-4 mr-2" />
                                Edit
                              </Link>
                            </DropdownMenuItem>
                            {tenant.status === 'active' && (
                              <DropdownMenuItem onClick={() => handleSuspend(tenant.id)}>
                                <CreditCardIcon className="h-4 w-4 mr-2" />
                                Suspend
                              </DropdownMenuItem>
                            )}
                            {tenant.status !== 'active' && (
                              <DropdownMenuItem onClick={() => handleActivate(tenant.id)}>
                                <CreditCardIcon className="h-4 w-4 mr-2" />
                                Activate
                              </DropdownMenuItem>
                            )}
                            <DropdownMenuItem
                              onClick={() => handleDelete(tenant.id)}
                              className="text-red-600"
                            >
                              <TrashIcon className="h-4 w-4 mr-2" />
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="px-6 py-4 border-t border-gray-200">
            <div className="flex items-center justify-between">
              <div className="text-sm text-gray-700">
                Showing {((page - 1) * 10) + 1} to {Math.min(page * 10, tenants.length)} of{' '}
                {tenants.length} results
              </div>
              <div className="flex items-center space-x-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(page - 1)}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <span className="px-3 py-1 text-sm">
                  Page {page} of {totalPages}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(page + 1)}
                  disabled={page === totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
