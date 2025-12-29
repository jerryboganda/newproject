"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Loader2, Shield, ShieldCheck, ArrowLeft, Copy, Check, QrCode } from "lucide-react";
import { useAuthStore } from "@/stores/auth-store";

export default function TwoFactorPage() {
  const [isSetup, setIsSetup] = useState(false);
  const [isVerifying, setIsVerifying] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);
  const [code, setCode] = useState("");
  const [secretKey, setSecretKey] = useState("");
  const [copied, setCopied] = useState(false);
  const router = useRouter();
  const { enable2FA, verify2FA } = useAuthStore();

  const handleSetup2FA = async () => {
    setLoading(true);
    setError("");

    try {
      const result = await enable2FA();
      setSecretKey(result.secretKey || "JBSWY3DPEHPK3PXP"); // Mock secret key
      setIsSetup(true);
    } catch (err: any) {
      setError(err.message || "Failed to setup 2FA");
    } finally {
      setLoading(false);
    }
  };

  const handleVerifyCode = async () => {
    if (code.length !== 6) {
      setError("Please enter a 6-digit code");
      return;
    }

    setLoading(true);
    setError("");

    try {
      await verify2FA(code);
      setSuccess(true);
    } catch (err: any) {
      setError(err.message || "Invalid verification code");
    } finally {
      setLoading(false);
    }
  };

  const copyToClipboard = async () => {
    try {
      await navigator.clipboard.writeText(secretKey);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error("Failed to copy:", err);
    }
  };

  if (success) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 px-4">
        <Card className="w-full max-w-md">
          <CardHeader className="text-center">
            <ShieldCheck className="h-16 w-16 text-green-600 mx-auto mb-4" />
            <CardTitle className="text-2xl font-bold">2FA Enabled!</CardTitle>
            <CardDescription>
              Your account is now protected with two-factor authentication
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="text-sm text-gray-600 dark:text-gray-400 space-y-2">
              <p>✓ Two-factor authentication enabled</p>
              <p>✓ Backup codes generated</p>
              <p>✓ Account security enhanced</p>
            </div>
            <Alert>
              <AlertDescription className="text-xs">
                Make sure to save your backup codes in a secure location. You'll need them if you
                lose access to your authentication app.
              </AlertDescription>
            </Alert>
          </CardContent>
          <CardFooter>
            <Button className="w-full" asChild>
              <Link href="/dashboard">Continue to Dashboard</Link>
            </Button>
          </CardFooter>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800 px-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle className="text-2xl font-bold text-center flex items-center justify-center gap-2">
            <Shield className="h-6 w-6" />
            Two-Factor Authentication
          </CardTitle>
          <CardDescription className="text-center">
            Add an extra layer of security to your account
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs value={isSetup ? "verify" : "setup"} className="w-full">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="setup" disabled={isSetup}>
                Setup
              </TabsTrigger>
              <TabsTrigger value="verify" disabled={!isSetup}>
                Verify
              </TabsTrigger>
            </TabsList>

            <TabsContent value="setup" className="space-y-4">
              <div className="text-center space-y-2">
                <Shield className="h-12 w-12 text-blue-600 mx-auto" />
                <p className="text-sm text-gray-600">
                  Enable two-factor authentication to protect your account with an additional
                  security layer.
                </p>
              </div>

              {error && (
                <Alert variant="destructive">
                  <AlertDescription>{error}</AlertDescription>
                </Alert>
              )}

              <div className="space-y-2">
                <h4 className="font-medium">How it works:</h4>
                <ul className="text-sm text-gray-600 space-y-1">
                  <li>1. Install an authenticator app (Google Authenticator, Authy, etc.)</li>
                  <li>2. Scan the QR code or enter the secret key manually</li>
                  <li>3. Enter the 6-digit code to verify setup</li>
                </ul>
              </div>

              <Button onClick={handleSetup2FA} className="w-full" disabled={loading}>
                {loading ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Setting up 2FA...
                  </>
                ) : (
                  "Enable 2FA"
                )}
              </Button>
            </TabsContent>

            <TabsContent value="verify" className="space-y-4">
              <div className="space-y-4">
                <div className="text-center space-y-2">
                  <QrCode className="h-12 w-12 text-blue-600 mx-auto" />
                  <p className="text-sm text-gray-600">
                    Scan this QR code with your authenticator app
                  </p>
                </div>

                {/* Mock QR Code - In production, generate actual QR code */}
                <div className="flex justify-center p-4 bg-white rounded-lg border">
                  <div className="w-48 h-48 bg-gray-200 flex items-center justify-center">
                    <QrCode className="h-24 w-24 text-gray-400" />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label>Or enter this code manually:</Label>
                  <div className="flex gap-2">
                    <Input
                      value={secretKey}
                      readOnly
                      className="font-mono text-xs"
                    />
                    <Button
                      type="button"
                      variant="outline"
                      size="icon"
                      onClick={copyToClipboard}
                    >
                      {copied ? (
                        <Check className="h-4 w-4" />
                      ) : (
                        <Copy className="h-4 w-4" />
                      )}
                    </Button>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="code">Enter 6-digit code</Label>
                  <Input
                    id="code"
                    type="text"
                    placeholder="000000"
                    value={code}
                    onChange={(e) => setCode(e.target.value.replace(/\D/g, "").slice(0, 6))}
                    className="text-center text-2xl tracking-widest"
                    maxLength={6}
                  />
                </div>

                {error && (
                  <Alert variant="destructive">
                    <AlertDescription>{error}</AlertDescription>
                  </Alert>
                )}

                <Button onClick={handleVerifyCode} className="w-full" disabled={loading}>
                  {loading ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Verifying...
                    </>
                  ) : (
                    "Verify & Enable"
                  )}
                </Button>
              </div>
            </TabsContent>
          </Tabs>
        </CardContent>
        <CardFooter>
          <Button variant="ghost" className="w-full" asChild>
            <Link href="/dashboard">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Skip for Now
            </Link>
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
