"use client";

import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import VideoPlayer from "@/components/VideoPlayer";

type PlaybackTokenResponse = {
  token: string;
  expiresAt: string;
  embedUrl: string;
};

type PlaybackInfoResponse = {
  mp4Url: string;
  thumbnailUrl?: string | null;
};

export default function EmbedPage({ params }: { params: { id: string } }) {
  const searchParams = useSearchParams();
  const providedToken = searchParams.get("token");

  const apiBase = useMemo(
    () => (process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api/v1").replace(/\/$/, ""),
    [],
  );

  const [token, setToken] = useState<string | null>(providedToken);
  const [playback, setPlayback] = useState<PlaybackInfoResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [tracked, setTracked] = useState(false);

  function getSessionId(): string {
    if (typeof window === "undefined") return "";
    const key = "sv-session-id";
    const existing = localStorage.getItem(key);
    if (existing) return existing;
    const created = typeof crypto !== "undefined" && "randomUUID" in crypto ? crypto.randomUUID() : `${Date.now()}-${Math.random()}`;
    localStorage.setItem(key, created);
    return created;
  }

  useEffect(() => {
    let cancelled = false;

    async function run() {
      setError(null);

      try {
        let t = token;
        if (!t) {
          const res = await fetch(`${apiBase}/videos/${params.id}/playback-token`, {
            method: "POST",
            headers: {
              Accept: "application/json",
              "Content-Type": "application/json",
              "X-Tenant-Slug": "demo",
            },
          });

          if (!res.ok) {
            const text = await res.text().catch(() => "");
            throw new Error(text || "Failed to create playback token");
          }

          const data = (await res.json()) as PlaybackTokenResponse;
          t = data.token;
          if (!cancelled) setToken(t);
        }

        const playbackRes = await fetch(`${apiBase}/videos/${params.id}/playback?token=${encodeURIComponent(t)}`, {
          headers: {
            Accept: "application/json",
            "X-Tenant-Slug": "demo",
          },
        });

        if (!playbackRes.ok) {
          const text = await playbackRes.text().catch(() => "");
          throw new Error(text || "Failed to fetch playback info");
        }

        const info = (await playbackRes.json()) as PlaybackInfoResponse;
        if (!cancelled) setPlayback(info);
      } catch (e: any) {
        if (!cancelled) setError(e?.message || "Failed to load video");
      }
    }

    run();
    return () => {
      cancelled = true;
    };
  }, [apiBase, params.id, token]);

  useEffect(() => {
    if (!playback?.mp4Url) return;
    if (tracked) return;

    let cancelled = false;

    (async () => {
      try {
        const sessionId = getSessionId();
        await fetch(`${apiBase}/analytics/track`, {
          method: "POST",
          headers: {
            Accept: "application/json",
            "Content-Type": "application/json",
            "X-Tenant-Slug": "demo",
          },
          body: JSON.stringify({
            videoId: params.id,
            eventType: 0, // AnalyticsEventType.View
            sessionId: sessionId || undefined,
            referrer: typeof document !== "undefined" ? document.referrer || undefined : undefined,
            deviceType: "web",
          }),
        });

        if (!cancelled) setTracked(true);
      } catch {
        // Non-fatal: playback should still work even if analytics tracking fails.
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [apiBase, params.id, playback?.mp4Url, tracked]);

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center p-6">
        <div className="max-w-lg w-full text-sm text-red-600">{error}</div>
      </div>
    );
  }

  if (!playback?.mp4Url) {
    return (
      <div className="min-h-screen flex items-center justify-center p-6">
        <div className="text-sm text-muted-foreground">Loading…</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <VideoPlayer src={playback.mp4Url} poster={playback.thumbnailUrl ?? undefined} autoPlay />
    </div>
  );
}
