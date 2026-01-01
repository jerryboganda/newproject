'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export default function AcceptInvitationPage({
  params,
}: {
  params: { token: string }
}) {
  const router = useRouter()

  // NOTE: The backend currently defines a TeamInvitation entity, but there are no
  // public invitation endpoints wired up yet. This page intentionally avoids any
  // mock data so the app stays production-honest.
  void params

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Invitation</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Alert>
            <AlertDescription>
              Invitation acceptance isn’t available in this build yet.
            </AlertDescription>
          </Alert>

          <div className="flex gap-2">
            <Button className="flex-1" onClick={() => router.push('/login')}>
              Go to login
            </Button>
            <Link href="/" className="shrink-0">
              <Button variant="outline">Home</Button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
