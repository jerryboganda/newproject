import { Suspense } from "react";
import ResetPasswordClient from "./reset-password-client";

function ResetPasswordFallback() {
  return <div className="min-h-screen flex items-center justify-center" />;
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<ResetPasswordFallback />}>
      <ResetPasswordClient />
    </Suspense>
  );
}
