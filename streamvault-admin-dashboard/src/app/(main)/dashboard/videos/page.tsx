import Link from "next/link";

import { Eye, Play, Video } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { streamvaultApi } from "@/lib/streamvault-api";

export default async function Page() {
  const videos = await streamvaultApi.videos.list();

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
        {videos.map((v) => (
          <Card key={v.id} className="overflow-hidden">
            <div className="relative">
              <img src={v.thumbnailUrl} alt={v.title} className="h-44 w-full object-cover" />
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
