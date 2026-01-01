"use client";

import { useEffect, useState } from "react";
import Link from "next/link";

import { Eye, Play, Video } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
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
        if (!cancelled) setError(e?.message || "Failed to load videos");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    run();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="@container/main flex flex-col gap-4 md:gap-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Videos</h1>
          <p className="text-muted-foreground">Manage videos from your StreamVault backend</p>
        </div>
        <Button asChild>
          <Link href="/dashboard/upload">Upload</Link>
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {loading ? (
          <div className="text-sm text-muted-foreground">Loading…</div>
        ) : error ? (
          <div className="text-sm text-red-600">{error}</div>
        ) : null}

        {videos.map((v) => (
          <Card key={v.id} className="overflow-hidden">
            <div className="relative">
              <img src={v.thumbnailUrl ?? ""} alt={v.title} className="h-44 w-full object-cover" />
              <Badge className="absolute left-2 top-2" variant="secondary">
                <Video className="mr-1 size-3" /> Video
              </Badge>
            </div>
            <CardHeader>
              <CardTitle className="line-clamp-1">{v.title}</CardTitle>
              <CardDescription className="line-clamp-2">{v.description}</CardDescription>
            </CardHeader>
            <CardContent className="flex items-center justify-between gap-2">
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Eye className="size-4" />
                {(v.viewCount ?? 0).toLocaleString()}
              </div>
              <Button asChild size="sm" variant="outline">
                <Link href={v.videoUrl} target="_blank">
                  <Play className="mr-2 size-4" /> Watch
                </Link>
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
