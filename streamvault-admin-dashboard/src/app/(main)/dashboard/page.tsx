"use client";

import { useEffect, useMemo, useState } from "react";
import { Eye, Video } from "lucide-react";

import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { streamvaultApi, type StreamVaultVideo } from "@/lib/streamvault-api";

export default function Page() {
  const [videos, setVideos] = useState<StreamVaultVideo[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    async function run() {
      try {
        setError(null);
        const list = await streamvaultApi.videos.list();
        if (!cancelled) setVideos(list);
      } catch (e: any) {
        if (!cancelled) setError(e?.message || "Failed to load dashboard data");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    run();
    return () => {
      cancelled = true;
    };
  }, []);

  const totalVideos = useMemo(() => videos.length, [videos]);
  const totalViews = useMemo(() => videos.reduce((sum, v) => sum + (v.viewCount ?? 0), 0), [videos]);

  return (
    <div className="@container/main flex flex-col gap-4 md:gap-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Dashboard</h1>
          <p className="text-muted-foreground">StreamVault overview</p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Card>
          <CardHeader>
            <CardDescription className="flex items-center gap-2">
              <Video className="size-4" /> Total Videos
            </CardDescription>
            <CardTitle className="text-3xl tabular-nums">{totalVideos.toLocaleString()}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader>
            <CardDescription className="flex items-center gap-2">
              <Eye className="size-4" /> Total Views
            </CardDescription>
            <CardTitle className="text-3xl tabular-nums">{totalViews.toLocaleString()}</CardTitle>
          </CardHeader>
        </Card>
      </div>

      <div className="rounded-lg border bg-card p-4">
        <div className="mb-3">
          <h2 className="font-medium">Latest Videos</h2>
          <p className="text-sm text-muted-foreground">Fetched from your backend at /api/v1/videos</p>
        </div>

        {loading ? (
          <div className="text-sm text-muted-foreground">Loading…</div>
        ) : error ? (
          <div className="text-sm text-red-600">{error}</div>
        ) : null}

        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          {videos.map((v) => (
            <div key={v.id} className="overflow-hidden rounded-lg border bg-background">
              <img src={v.thumbnailUrl ?? ""} alt={v.title} className="h-36 w-full object-cover" />
              <div className="p-3">
                <div className="line-clamp-1 font-medium">{v.title}</div>
                <div className="line-clamp-2 text-sm text-muted-foreground">{v.description}</div>
                <div className="mt-2 text-xs text-muted-foreground">{(v.viewCount ?? 0).toLocaleString()} views</div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
