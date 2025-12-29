"use client";

import { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Loader2, MailCheck, AlertCircle, CheckCircle } from "lucide-react";
import { useAuthStore } from "@/stores/auth-store";

export default function VerifyEmailPage() {
  const [status, setStatus] = useState<"loading" | "success" | "error">("loading");
  const [message, setMessage] = useState("");
  const router = useRouter();
  const searchParams = useSearchParams();
  const { verifyEmail } = useAuthStore();

  useEffect(() => {
    const userId = searchParams.get("userId");
    const token = searchParams.get("token");

    if (!userId || !token) {
      setStatus("error");
      setMessage("Invalid verification link. Please check your email and try again.");
      return;
    }

    handleVerification(userId, token);
  }, [searchParams]);

  const handleVerification = async (userId: string, token: string) => {
    try {
      await verifyEmail(userId, token);
      setStatus("success");
      setMessage("Your email has been successfully verified! You can now log in to your account.");
    } catch (error: any) {
      setStatus("error");
      setMessage(
        error.message || "Failed to verify email. The link may have expired or is invalid."
      );
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 px-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-4">
            {status === "loading" && (
              <Loader2 className="h-16 w-16 text-blue-600 animate-spin" />
            )}
            {status === "success" && (
              <CheckCircle className="h-16 w-16 text-green-600" />
            )}
            {status === "error" && (
              <AlertCircle className="h-16 w-16 text-red-600" />
            )}
          </div>
          <CardTitle className="text-2xl font-bold">
            {status === "loading" && "Verifying Email..."}
            {status === "success" && "Email Verified!"}
            {status === "error" && "Verification Failed"}
          </CardTitle>
          <CardDescription>
            {status === "loading" && "Please wait while we verify your email address."}
            {status === "success" && message}
            {status === "error" && message}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {status === "error" && (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>
                If you continue to experience issues, please contact support or try requesting a new
                verification email.
              </AlertDescription>
            </Alert>
          )}

          {status === "success" && (
            <div className="text-center space-y-4">
              <MailCheck className="h-12 w-12 text-green-600 mx-auto" />
              <p className="text-sm text-gray-600 dark:text-gray-400">
                You can now access all features of StreamVault with your verified account.
              </p>
            </div>
          )}

          <div className="flex flex-col space-y-2">
            {status === "success" && (
              <Button asChild className="w-full">
                <Link href="/auth/login">Continue to Login</Link>
              </Button>
            )}
            
            {status === "error" && (
              <>
                <Button asChild variant="outline" className="w-full">
                  <Link href="/auth/login">Back to Login</Link>
                </Button>
                <Button
                  variant="ghost"
                  className="w-full"
                  onClick={() => window.location.href = "mailto:support@streamvault.com"}
                >
                  Contact Support
                </Button>
              </>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
