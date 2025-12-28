'use client'

import { useState, FormEvent } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { Textarea } from "@/components/ui/textarea"
import { 
  UsersIcon,
  EnvelopeIcon,
  CheckCircleIcon,
  XMarkIcon,
  PlusIcon,
  TrashIcon
} from '@heroicons/react/24/outline'

interface Invitation {
  id: string
  email: string
  role: string
  message?: string
}

export default function InviteUsersPage() {
  const router = useRouter()
  const [invitations, setInvitations] = useState<Invitation[]>([
    { id: '1', email: '', role: '', message: '' }
  ])
  const [selectedRole, setSelectedRole] = useState('')
  const [customMessage, setCustomMessage] = useState('')
  const [sendImmediately, setSendImmediately] = useState(true)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  const roles = [
    { id: 'admin', name: 'Admin', description: 'Full access to all features' },
    { id: 'moderator', name: 'Moderator', description: 'Manage content and users' },
    { id: 'editor', name: 'Editor', description: 'Create and edit content' },
    { id: 'viewer', name: 'Viewer', description: 'View-only access' },
  ]

  const permissions = [
    { id: 'users', name: 'Manage Users', roles: ['admin'] },
    { id: 'content', name: 'Manage Content', roles: ['admin', 'moderator', 'editor'] },
    { id: 'analytics', name: 'View Analytics', roles: ['admin', 'moderator'] },
    { id: 'settings', name: 'Manage Settings', roles: ['admin'] },
    { id: 'billing', name: 'Manage Billing', roles: ['admin'] },
  ]

  const addInvitation = () => {
    const newInvitation: Invitation = {
      id: Date.now().toString(),
      email: '',
      role: '',
      message: customMessage,
    }
    setInvitations([...invitations, newInvitation])
  }

  const removeInvitation = (id: string) => {
    if (invitations.length > 1) {
      setInvitations(invitations.filter(inv => inv.id !== id))
    }
  }

  const updateInvitation = (id: string, field: keyof Invitation, value: string) => {
    setInvitations(invitations.map(inv => 
      inv.id === id ? { ...inv, [field]: value } : inv
    ))
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)

    // Validate all invitations
    const invalidInvitations = invitations.filter(inv => !inv.email || !inv.role)
    if (invalidInvitations.length > 0) {
      setError('Please fill in email and role for all invitations')
      setIsLoading(false)
      return
    }

    try {
      // Mock API call - replace with actual implementation
      await new Promise(resolve => setTimeout(resolve, 2000))
      
      setSuccess(true)
      setInvitations([{ id: '1', email: '', role: '', message: '' }])
      setCustomMessage('')
      setSelectedRole('')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send invitations')
    } finally {
      setIsLoading(false)
    }
  }

  const getRolePermissions = (roleId: string) => {
    return permissions.filter(p => p.roles.includes(roleId))
  }

  if (success) {
    return (
      <div className="max-w-2xl mx-auto">
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <CheckCircleIcon className="mx-auto h-16 w-16 text-green-500 mb-4" />
              <h2 className="text-2xl font-bold text-gray-900 mb-2">
                Invitations Sent!
              </h2>
              <p className="text-gray-600 mb-6">
                {invitations.length} invitation(s) have been sent successfully.
              </p>
              <div className="space-y-2">
                <Button onClick={() => router.push('/admin/users')}>
                  View All Users
                </Button>
                <Button
                  variant="outline"
                  onClick={() => {
                    setSuccess(false)
                    setInvitations([{ id: '1', email: '', role: '', message: '' }])
                  }}
                  className="ml-2"
                >
                  Send More Invitations
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-gray-900">Invite Users</h2>
        <p className="text-gray-600">Send invitations to new team members</p>
      </div>

      {error && (
        <Alert variant="destructive">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Quick Invite Section */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Quick Invite</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label htmlFor="quick-email">Email address</Label>
                <Input
                  id="quick-email"
                  type="email"
                  placeholder="colleague@example.com"
                  value={invitations[0].email}
                  onChange={(e) => updateInvitation(invitations[0].id, 'email', e.target.value)}
                />
              </div>
              <div>
                <Label htmlFor="quick-role">Role</Label>
                <Select
                  value={invitations[0].role}
                  onValueChange={(value) => updateInvitation(invitations[0].id, 'role', value)}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select a role" />
                  </SelectTrigger>
                  <SelectContent>
                    {roles.map((role) => (
                      <SelectItem key={role.id} value={role.id}>
                        <div>
                          <div className="font-medium">{role.name}</div>
                          <div className="text-sm text-gray-500">{role.description}</div>
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            {invitations[0].role && (
              <div className="p-4 bg-gray-50 rounded-lg">
                <h4 className="font-medium text-gray-900 mb-2">Permissions for this role:</h4>
                <div className="flex flex-wrap gap-2">
                  {getRolePermissions(invitations[0].role).map((permission) => (
                    <Badge key={permission.id} variant="secondary">
                      {permission.name}
                    </Badge>
                  ))}
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Multiple Invitations */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-lg">Multiple Invitations</CardTitle>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={addInvitation}
            >
              <PlusIcon className="h-4 w-4 mr-1" />
              Add Another
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {invitations.slice(1).map((invitation, index) => (
                <div key={invitation.id} className="flex gap-3 items-start">
                  <div className="flex-1 grid grid-cols-1 md:grid-cols-2 gap-3">
                    <Input
                      type="email"
                      placeholder="Email address"
                      value={invitation.email}
                      onChange={(e) => updateInvitation(invitation.id, 'email', e.target.value)}
                    />
                    <Select
                      value={invitation.role}
                      onValueChange={(value) => updateInvitation(invitation.id, 'role', value)}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select role" />
                      </SelectTrigger>
                      <SelectContent>
                        {roles.map((role) => (
                          <SelectItem key={role.id} value={role.id}>
                            {role.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => removeInvitation(invitation.id)}
                  >
                    <TrashIcon className="h-4 w-4 text-red-500" />
                  </Button>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Message Options */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Invitation Message</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label htmlFor="custom-message">Custom message (optional)</Label>
              <Textarea
                id="custom-message"
                placeholder="Add a personal message to your invitation..."
                value={customMessage}
                onChange={(e) => setCustomMessage(e.target.value)}
                rows={3}
              />
            </div>
            
            <div className="flex items-center space-x-2">
              <Checkbox
                id="send-immediately"
                checked={sendImmediately}
                onCheckedChange={(checked) => setSendImmediately(checked as boolean)}
              />
              <Label htmlFor="send-immediately" className="text-sm">
                Send invitations immediately
              </Label>
            </div>
            
            {!sendImmediately && (
              <p className="text-sm text-gray-500">
                Invitations will be saved as drafts and can be sent later from the Users page.
              </p>
            )}
          </CardContent>
        </Card>

        {/* Submit */}
        <div className="flex justify-end space-x-3">
          <Button
            type="button"
            variant="outline"
            onClick={() => router.back()}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={isLoading}
          >
            {isLoading ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Sending...
              </>
            ) : (
              <>
                <EnvelopeIcon className="h-4 w-4 mr-2" />
                Send {invitations.length} Invitation{invitations.length > 1 ? 's' : ''}
              </>
            )}
          </Button>
        </div>
      </form>
    </div>
  )
}
