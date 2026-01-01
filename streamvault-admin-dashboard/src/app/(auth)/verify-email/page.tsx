import { Suspense } from "react";
import VerifyEmailClient from "./verify-email-client";

function VerifyEmailFallback() {
  return <div className="min-h-screen flex items-center justify-center" />;
}

export default function VerifyEmailPage() {
  return (
    <Suspense fallback={<VerifyEmailFallback />}>
      <VerifyEmailClient />
    </Suspense>
  );
}
