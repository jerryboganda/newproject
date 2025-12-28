'use client'

import { useState, useEffect, FormEvent } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Checkbox } from '@/components/ui/checkbox'
import { 
  CheckCircleIcon,
  ExclamationTriangleIcon,
  EyeIcon,
  EyeOffIcon,
  BuildingOfficeIcon,
  UserGroupIcon,
  CalendarIcon
} from '@heroicons/react/24/outline'

export default function AcceptInvitationPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const token = searchParams.get('token')
  
  const [invitation, setInvitation] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [accepting, setAccepting] = useState(false)
  const [error, setError] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    password: '',
    confirmPassword: '',
    agreeToTerms: false,
  })

  useEffect(() => {
    if (!token) {
      router.push('/login')
      return
    }
    
    fetchInvitationDetails()
  }, [token, router])

  const fetchInvitationDetails = async () => {
    try {
      // Mock API call - replace with actual implementation
      const mockInvitation = {
        id: '1',
        token: token,
        email: 'john.doe@example.com',
        inviterName: 'Sarah Johnson',
        inviterEmail: 'sarah@company.com',
        tenantName: 'Acme Corporation',
        tenantLogo: null,
        role: 'Editor',
        permissions: ['content.create', 'content.edit', 'media.upload'],
        message: 'We would like to invite you to join our team at Acme Corporation. You will have access to create and edit content.',
        expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
        status: 'pending',
      }
      
      setInvitation(mockInvitation)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid or expired invitation')
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')

    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match')
      return
    }

    if (!formData.agreeToTerms) {
      setError('You must agree to the terms and conditions')
      return
    }

    setAccepting(true)

    try {
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 2000))
      
      // Redirect to login with success message
      router.push('/login?message=invitation-accepted')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to accept invitation')
    } finally {
      setAccepting(false)
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value,
    })
  }

  const isExpired = invitation && new Date(invitation.expiresAt) < new Date()

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  if (error || !invitation || isExpired) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <Card className="w-full max-w-md">
          <CardContent className="pt-6">
            <div className="text-center">
              <ExclamationTriangleIcon className="mx-auto h-12 w-12 text-red-500 mb-4" />
              <h2 className="text-2xl font-bold text-gray-900 mb-2">
                {isExpired ? 'Invitation Expired' : 'Invalid Invitation'}
              </h2>
              <p className="text-gray-600 mb-6">
                {isExpired 
                  ? 'This invitation has expired. Please contact your administrator for a new invitation.'
                  : 'This invitation link is invalid or has already been used.'
                }
              </p>
              <Link href="/login">
                <Button>Back to Login</Button>
              </Link>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-2xl mx-auto">
        {/* Invitation Details */}
        <Card className="mb-6">
          <CardContent className="pt-6">
            <div className="flex items-start space-x-4">
              {invitation.tenantLogo ? (
                <img 
                  src={invitation.tenantLogo} 
                  alt={invitation.tenantName}
                  className="w-16 h-16 rounded-lg"
                />
              ) : (
                <div className="w-16 h-16 bg-gray-200 rounded-lg flex items-center justify-center">
                  <BuildingOfficeIcon className="h-8 w-8 text-gray-500" />
                </div>
              )}
              
              <div className="flex-1">
                <h1 className="text-2xl font-bold text-gray-900">
                  You're invited to join {invitation.tenantName}
                </h1>
                <p className="text-gray-600 mt-1">
                  Invited by {invitation.inviterName} ({invitation.inviterEmail})
                </p>
                
                <div className="flex items-center space-x-4 mt-3">
                  <Badge variant="secondary" className="text-sm">
                    {invitation.role}
                  </Badge>
                  <div className="flex items-center text-sm text-gray-500">
                    <CalendarIcon className="h-4 w-4 mr-1" />
                    Expires {new Date(invitation.expiresAt).toLocaleDateString()}
                  </div>
                </div>
              </div>
            </div>
            
            {invitation.message && (
              <div className="mt-4 p-4 bg-blue-50 rounded-lg">
                <p className="text-sm text-blue-800">{invitation.message}</p>
              </div>
            )}
            
            <div className="mt-4">
              <h3 className="text-sm font-medium text-gray-900 mb-2">You'll have access to:</h3>
              <div className="flex flex-wrap gap-2">
                {invitation.permissions.map((permission: string) => (
                  <Badge key={permission} variant="outline" className="text-xs">
                    {permission.replace('.', ' ').replace(/\b\w/g, l => l.toUpperCase())}
                  </Badge>
                ))}
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Accept Form */}
        <Card>
          <CardHeader>
            <CardTitle>Accept Invitation</CardTitle>
          </CardHeader>
          <CardContent>
            {error && (
              <Alert variant="destructive" className="mb-6">
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            <form onSubmit={handleSubmit} className="space-y-6">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="firstName">First name</Label>
                  <Input
                    id="firstName"
                    name="firstName"
                    type="text"
                    required
                    value={formData.firstName}
                    onChange={handleChange}
                    placeholder="John"
                  />
                </div>
                
                <div>
                  <Label htmlFor="lastName">Last name</Label>
                  <Input
                    id="lastName"
                    name="lastName"
                    type="text"
                    required
                    value={formData.lastName}
                    onChange={handleChange}
                    placeholder="Doe"
                  />
                </div>
              </div>

              <div>
                <Label htmlFor="email">Email address</Label>
                <Input
                  id="email"
                  type="email"
                  value={invitation.email}
                  disabled
                  className="bg-gray-100"
                />
                <p className="text-xs text-gray-500 mt-1">
                  This email is associated with the invitation
                </p>
              </div>

              <div>
                <Label htmlFor="password">Create password</Label>
                <div className="relative mt-1">
                  <Input
                    id="password"
                    name="password"
                    type={showPassword ? 'text' : 'password'}
                    required
                    value={formData.password}
                    onChange={handleChange}
                    placeholder="Create a strong password"
                  />
                  <button
                    type="button"
                    className="absolute inset-y-0 right-0 pr-3 flex items-center"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? (
                      <EyeOffIcon className="h-5 w-5 text-gray-400" />
                    ) : (
                      <EyeIcon className="h-5 w-5 text-gray-400" />
                    )}
                  </button>
                </div>
              </div>

              <div>
                <Label htmlFor="confirmPassword">Confirm password</Label>
                <div className="relative mt-1">
                  <Input
                    id="confirmPassword"
                    name="confirmPassword"
                    type={showConfirmPassword ? 'text' : 'password'}
                    required
                    value={formData.confirmPassword}
                    onChange={handleChange}
                    placeholder="Confirm your password"
                  />
                  <button
                    type="button"
                    className="absolute inset-y-0 right-0 pr-3 flex items-center"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  >
                    {showConfirmPassword ? (
                      <EyeOffIcon className="h-5 w-5 text-gray-400" />
                    ) : (
                      <EyeIcon className="h-5 w-5 text-gray-400" />
                    )}
                  </button>
                </div>
              </div>

              <div className="flex items-center">
                <Checkbox
                  id="agreeToTerms"
                  name="agreeToTerms"
                  checked={formData.agreeToTerms}
                  onCheckedChange={(checked) => 
                    setFormData(prev => ({ ...prev, agreeToTerms: checked as boolean }))
                  }
                />
                <Label htmlFor="agreeToTerms" className="ml-2 text-sm">
                  I agree to the{' '}
                  <Link href="/terms" className="text-blue-600 hover:text-blue-500">
                    Terms and Conditions
                  </Link>{' '}
                  and{' '}
                  <Link href="/privacy" className="text-blue-600 hover:text-blue-500">
                    Privacy Policy
                  </Link>
                </Label>
              </div>

              <div className="flex space-x-3">
                <Button
                  type="submit"
                  className="flex-1"
                  disabled={accepting}
                >
                  {accepting ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                      Accepting...
                    </>
                  ) : (
                    <>
                      <CheckCircleIcon className="h-4 w-4 mr-2" />
                      Accept Invitation
                    </>
                  )}
                </Button>
                
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => router.push('/login')}
                >
                  Decline
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
