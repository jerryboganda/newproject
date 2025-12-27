"use client";

import { useEffect, useState } from "react";

type HealthStatus = { status: string } | null;

export default function HomePage() {
  const [health, setHealth] = useState<HealthStatus>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const apiBase = "http://localhost:5000";
    fetch(`${apiBase}/health`)
      .then(async (res) => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return res.json();
      })
      .then((data) => setHealth(data))
      .catch((err) => setError(err.message));
  }, []);

  return (
    <main style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh', backgroundColor: '#f8fafc', color: '#0f172a' }}>
      <div style={{ padding: '1.5rem', backgroundColor: 'white', border: '1px solid #e2e8f0', borderRadius: '0.75rem', boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)', maxWidth: '32rem', textAlign: 'center' }}>
        <p style={{ fontSize: '0.875rem', fontWeight: '600', color: '#64748b', marginBottom: '0.75rem' }}>StreamVault</p>
        <h1 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '0.75rem' }}>Frontend scaffold is ready</h1>
        <p style={{ fontSize: '0.875rem', color: '#64748b', marginBottom: '0.75rem' }}>
          Replace this placeholder with the shadcn admin template. Configure NEXT_PUBLIC_API_BASE_URL in .env.local and
          run npm install before building.
        </p>
        <div style={{ fontSize: '0.875rem', textAlign: 'left', padding: '0.75rem', backgroundColor: 'white', border: '1px solid #e2e8f0', borderRadius: '0.5rem' }}>
          <p style={{ fontWeight: '600', color: '#64748b', marginBottom: '0.25rem' }}>API Health Check</p>
          {health && <p style={{ color: '#10b981' }}>Status: {health.status}</p>}
          {error && <p style={{ color: '#ef4444' }}>Error: {error}</p>}
          {!health && !error && <p style={{ color: '#64748b' }}>Checking...</p>}
        </div>
      </div>
    </main>
  );
}
