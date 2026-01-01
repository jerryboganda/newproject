'use client'

import { useState, FormEvent, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/contexts/AuthContext'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Loader2, ShieldCheckIcon, ArrowLeftIcon } from 'lucide-react'

export default function TwoFactorPage() {
  const router = useRouter()
  const { verify2FA, isLoading } = useAuth()
  const [code, setCode] = useState(['', '', '', '', '', ''])
  const [error, setError] = useState('')
  const [tempToken, setTempToken] = useState('')

  useEffect(() => {
    const token = localStorage.getItem('temp_token')
    if (!token) {
      router.push('/login')
      return
    }
    setTempToken(token)
  }, [router])

  const handleInputChange = (index: number, value: string) => {
    // Only allow numbers
    if (value && !/^[0-9]$/.test(value)) return

    const newCode = [...code]
    newCode[index] = value
    setCode(newCode)

    // Auto-focus next input
    if (value && index < 5) {
      const nextInput = document.getElementById(`code-${index + 1}`) as HTMLInputElement
      nextInput?.focus()
    }
  }

  const handleKeyDown = (index: number, e: React.KeyboardEvent) => {
    // Handle backspace
    if (e.key === 'Backspace' && !code[index] && index > 0) {
      const prevInput = document.getElementById(`code-${index - 1}`) as HTMLInputElement
      prevInput?.focus()
    }
  }

  const handlePaste = (e: React.ClipboardEvent) => {
    e.preventDefault()
    const pastedData = e.clipboardData.getData('text').slice(0, 6)
    const digits = pastedData.split('').filter((char) => /^[0-9]$/.test(char))
    
    if (digits.length === 6) {
      setCode(digits)
      // Focus last input
      const lastInput = document.getElementById('code-5') as HTMLInputElement
      lastInput?.focus()
    }
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')

    const verificationCode = code.join('')
    
    if (verificationCode.length !== 6) {
      setError('Please enter all 6 digits')
      return
    }

    try {
      await verify2FA(verificationCode, tempToken)
      
      // Redirect will be handled by the verify2FA function
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid verification code')
      setCode(['', '', '', '', '', ''])
      // Focus first input
      const firstInput = document.getElementById('code-0') as HTMLInputElement
      firstInput?.focus()
    }
  }

  const handleResend = async () => {
    // Implement resend code logic
    console.log('Resend code')
  }

  const handleBack = () => {
    localStorage.removeItem('temp_token')
    router.push('/login')
  }

  return (
    <>
      <div className="text-center mb-6">
        <button
          onClick={handleBack}
          className="absolute left-0 top-0 p-2 text-gray-400 hover:text-gray-600"
        >
          <ArrowLeftIcon className="h-5 w-5" />
        </button>
        
        <ShieldCheckIcon className="mx-auto h-12 w-12 text-blue-600 mb-4" />
        
        <h2 className="text-2xl font-bold text-gray-900">
          Two-Factor Authentication
        </h2>
        
        <p className="mt-2 text-sm text-gray-600">
          Enter the 6-digit code from your authenticator app
        </p>
      </div>

      {error && (
        <Alert variant="destructive" className="mb-6">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <form onSubmit={handleSubmit}>
        <div className="flex justify-center space-x-2 mb-6">
          {code.map((digit, index) => (
            <Input
              key={index}
              id={`code-${index}`}
              type="text"
              inputMode="numeric"
              pattern="[0-9]*"
              maxLength={1}
              value={digit}
              onChange={(e) => handleInputChange(index, e.target.value)}
              onKeyDown={(e) => handleKeyDown(index, e)}
              onPaste={index === 0 ? handlePaste : undefined}
              className="w-12 h-12 text-center text-lg font-semibold"
              autoFocus={index === 0}
            />
          ))}
        </div>

        <div className="space-y-3">
          <Button
            type="submit"
            className="w-full"
            disabled={isLoading || code.some(d => !d)}
          >
            {isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Verifying...
              </>
            ) : (
              'Verify'
            )}
          </Button>

          <button
            type="button"
            onClick={handleResend}
            className="w-full text-sm text-blue-600 hover:text-blue-500"
          >
            Didn&apos;t receive a code? Resend
          </button>
        </div>
      </form>

      <div className="mt-6 p-4 bg-gray-50 rounded-lg">
        <h3 className="text-sm font-medium text-gray-900 mb-2">
          Having trouble?
        </h3>
        <ul className="text-xs text-gray-600 space-y-1">
          <li>• Make sure your device&apos;s time is synced</li>
          <li>• Try generating a new code in your authenticator app</li>
          <li>• Contact support if the issue persists</li>
        </ul>
      </div>
    </>
  )
}
