'use client'

import { useState, FormEvent } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { apiClient } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Loader2, MailIcon, CheckCircleIcon } from 'lucide-react'

export default function ForgotPasswordPage() {
  const router = useRouter()
  const [email, setEmail] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [isSuccess, setIsSuccess] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      await apiClient.auth.forgotPassword(email)
      setIsSuccess(true)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send reset email')
    } finally {
      setIsLoading(false)
    }
  }

  if (isSuccess) {
    return (
      <div className="text-center">
        <CheckCircleIcon className="mx-auto h-12 w-12 text-green-500 mb-4" />
        
        <h2 className="text-2xl font-bold text-gray-900 mb-2">
          Check your email
        </h2>
        
        <p className="text-gray-600 mb-6">
          We&apos;ve sent a password reset link to{' '}
          <span className="font-medium">{email}</span>
        </p>
        
        <div className="space-y-3">
          <Button
            onClick={() => router.push('/login')}
            className="w-full"
          >
            Back to login
          </Button>
          
          <button
            onClick={() => {
              setIsSuccess(false)
              setEmail('')
            }}
            className="w-full text-sm text-blue-600 hover:text-blue-500"
          >
            Try a different email
          </button>
        </div>
        
        <div className="mt-6 p-4 bg-gray-50 rounded-lg text-left">
          <h3 className="text-sm font-medium text-gray-900 mb-2">
            Didn&apos;t receive the email?
          </h3>
          <ul className="text-xs text-gray-600 space-y-1">
            <li>• Check your spam folder</li>
            <li>• Make sure the email address is correct</li>
            <li>• Wait a few minutes for it to arrive</li>
            <li>• Contact support if you still don&apos;t see it</li>
          </ul>
        </div>
      </div>
    )
  }

  return (
    <>
      <div className="text-center mb-6">
        <MailIcon className="mx-auto h-12 w-12 text-gray-400 mb-4" />
        
        <h2 className="text-2xl font-bold text-gray-900">
          Forgot your password?
        </h2>
        
        <p className="mt-2 text-sm text-gray-600">
          Enter your email address and we&apos;ll send you a link to reset your password
        </p>
      </div>

      {error && (
        <Alert variant="destructive" className="mb-6">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <Label htmlFor="email">Email address</Label>
          <Input
            id="email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="mt-1"
            placeholder="Enter your email"
          />
        </div>

        <Button
          type="submit"
          className="w-full"
          disabled={isLoading}
        >
          {isLoading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Sending...
            </>
          ) : (
            'Send reset link'
          )}
        </Button>

        <div className="text-center">
          <Link
            href="/login"
            className="font-medium text-blue-600 hover:text-blue-500"
          >
            Back to login
          </Link>
        </div>
      </form>

      <div className="mt-6 p-4 bg-blue-50 rounded-lg">
        <h3 className="text-sm font-medium text-blue-900 mb-2">
          Password reset tips
        </h3>
        <ul className="text-xs text-blue-700 space-y-1">
          <li>• The reset link will expire in 1 hour</li>
          <li>• Make sure to check your spam folder</li>
          <li>• If you don&apos;t receive an email, try again with a different address</li>
          <li>• For security reasons, we won&apos;t confirm if an email exists</li>
        </ul>
      </div>
    </>
  )
}
