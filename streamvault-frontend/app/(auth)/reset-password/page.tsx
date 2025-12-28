'use client'

import { useState, FormEvent, useEffect } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { apiClient } from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Loader2, EyeIcon, EyeOffIcon, CheckIcon, XIcon } from 'lucide-react'

export default function ResetPasswordPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const [formData, setFormData] = useState({
    newPassword: '',
    confirmPassword: '',
  })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [isSuccess, setIsSuccess] = useState(false)
  const [token, setToken] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [passwordStrength, setPasswordStrength] = useState(0)

  const passwordRequirements = [
    { regex: /.{8,}/, text: 'At least 8 characters' },
    { regex: /[A-Z]/, text: 'One uppercase letter' },
    { regex: /[a-z]/, text: 'One lowercase letter' },
    { regex: /[0-9]/, text: 'One number' },
    { regex: /[^A-Za-z0-9]/, text: 'One special character' },
  ]

  useEffect(() => {
    const tokenParam = searchParams.get('token')
    if (!tokenParam) {
      router.push('/login')
      return
    }
    setToken(tokenParam)
  }, [searchParams, router])

  const checkPasswordStrength = (password: string) => {
    let strength = 0
    passwordRequirements.forEach((req) => {
      if (req.regex.test(password)) strength++
    })
    return strength
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    
    setFormData({
      ...formData,
      [name]: value,
    })

    if (name === 'newPassword') {
      setPasswordStrength(checkPasswordStrength(value))
    }
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')

    if (formData.newPassword !== formData.confirmPassword) {
      setError('Passwords do not match')
      return
    }

    if (passwordStrength < 3) {
      setError('Please choose a stronger password')
      return
    }

    setIsLoading(true)

    try {
      await apiClient.auth.resetPassword({
        token,
        newPassword: formData.newPassword,
      })
      setIsSuccess(true)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset password')
    } finally {
      setIsLoading(false)
    }
  }

  const getPasswordStrengthColor = () => {
    if (passwordStrength <= 2) return 'bg-red-500'
    if (passwordStrength <= 3) return 'bg-yellow-500'
    if (passwordStrength <= 4) return 'bg-blue-500'
    return 'bg-green-500'
  }

  const getPasswordStrengthText = () => {
    if (passwordStrength <= 2) return 'Weak'
    if (passwordStrength <= 3) return 'Fair'
    if (passwordStrength <= 4) return 'Good'
    return 'Strong'
  }

  if (isSuccess) {
    return (
      <div className="text-center">
        <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
          <svg
            className="h-6 w-6 text-green-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M5 13l4 4L19 7"
            />
          </svg>
        </div>
        
        <h2 className="text-2xl font-bold text-gray-900 mb-2">
          Password reset successful
        </h2>
        
        <p className="text-gray-600 mb-6">
          Your password has been updated successfully
        </p>
        
        <Button onClick={() => router.push('/login')} className="w-full">
          Continue to login
        </Button>
      </div>
    )
  }

  return (
    <>
      <h2 className="text-2xl font-bold text-gray-900 text-center mb-6">
        Reset your password
      </h2>

      {error && (
        <Alert variant="destructive" className="mb-6">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <Label htmlFor="newPassword">New password</Label>
          <div className="relative mt-1">
            <Input
              id="newPassword"
              name="newPassword"
              type={showPassword ? 'text' : 'password'}
              required
              value={formData.newPassword}
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

          {formData.newPassword && (
            <div className="mt-2">
              <div className="flex items-center justify-between mb-1">
                <span className="text-xs text-gray-600">
                  Password strength: {getPasswordStrengthText()}
                </span>
                <span className="text-xs text-gray-600">
                  {passwordStrength}/5
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${getPasswordStrengthColor()}`}
                  style={{ width: `${(passwordStrength / 5) * 100}%` }}
                />
              </div>
              <ul className="mt-2 space-y-1">
                {passwordRequirements.map((req, index) => (
                  <li key={index} className="flex items-center text-xs">
                    {req.regex.test(formData.newPassword) ? (
                      <CheckIcon className="h-3 w-3 text-green-500 mr-1" />
                    ) : (
                      <XIcon className="h-3 w-3 text-red-500 mr-1" />
                    )}
                    <span
                      className={
                        req.regex.test(formData.newPassword)
                          ? 'text-green-600'
                          : 'text-gray-600'
                      }
                    >
                      {req.text}
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>

        <div>
          <Label htmlFor="confirmPassword">Confirm new password</Label>
          <div className="relative mt-1">
            <Input
              id="confirmPassword"
              name="confirmPassword"
              type={showConfirmPassword ? 'text' : 'password'}
              required
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Confirm your new password"
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
          {formData.confirmPassword && formData.newPassword && (
            <p className="mt-1 text-xs">
              {formData.newPassword === formData.confirmPassword ? (
                <span className="text-green-600">Passwords match</span>
              ) : (
                <span className="text-red-600">Passwords do not match</span>
              )}
            </p>
          )}
        </div>

        <Button
          type="submit"
          className="w-full"
          disabled={isLoading || passwordStrength < 3}
        >
          {isLoading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Resetting password...
            </>
          ) : (
            'Reset password'
          )}
        </Button>

        <div className="text-center">
          <button
            type="button"
            onClick={() => router.push('/login')}
            className="font-medium text-blue-600 hover:text-blue-500"
          >
            Back to login
          </button>
        </div>
      </form>

      <div className="mt-6 p-4 bg-yellow-50 rounded-lg">
        <h3 className="text-sm font-medium text-yellow-900 mb-2">
          Security reminder
        </h3>
        <ul className="text-xs text-yellow-700 space-y-1">
          <li>• Choose a unique password that you haven't used before</li>
          <li>• Don't share your password with anyone</li>
          <li>• Consider using a password manager</li>
          <li>• Enable two-factor authentication for extra security</li>
        </ul>
      </div>
    </>
  )
}
