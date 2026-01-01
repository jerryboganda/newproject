"use client";

import { useState } from "react";

import { Upload } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import * as tus from "tus-js-client";

export default function Page() {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [status, setStatus] = useState<string | null>(null);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setStatus(null);

    if (!file) {
      setStatus("Please select a file.");
      return;
    }

    try {
      const apiBase = (process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api/v1").replace(/\/$/, "");
      const apiOrigin = apiBase.endsWith("/api/v1") ? apiBase.slice(0, -"/api/v1".length) : apiBase;
      const token = localStorage.getItem("auth-token");
      if (!token) throw new Error("Not authenticated");

      setStatus("Initiating upload...");
      const initRes = await fetch(`${apiBase}/videos/upload/initiate`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
          "X-Tenant-Slug": "demo",
        },
        body: JSON.stringify({
          title,
          description,
          fileName: file.name,
          contentType: file.type || "application/octet-stream",
        }),
      });

      if (!initRes.ok) {
        throw new Error(`Initiate failed: ${initRes.status}`);
      }

      const init = await initRes.json();

      setStatus(`Uploading... videoId=${init.videoId}`);
      const tusEndpoint = `${apiOrigin}${init.tusEndpoint}`;

      await new Promise<void>((resolve, reject) => {
        const upload = new tus.Upload(file, {
          endpoint: tusEndpoint,
          metadata: init.uploadMetadata || {
            videoId: init.videoId,
            fileName: file.name,
            contentType: file.type,
          },
          headers: {
            Authorization: `Bearer ${token}`,
            "X-Tenant-Slug": "demo",
          },
          onError: (err) => reject(err),
          onProgress: (bytesUploaded, bytesTotal) => {
            const pct = bytesTotal > 0 ? Math.round((bytesUploaded / bytesTotal) * 100) : 0;
            setStatus(`Uploading... ${pct}%`);
          },
          onSuccess: () => resolve(),
        });
        upload.start();
      });

      setStatus("Upload finished. Processing...");
      setTitle("");
      setDescription("");
      setFile(null);
    } catch (err: any) {
      setStatus(err?.message ?? "Upload failed");
    }
  }

  return (
    <div className="@container/main flex flex-col gap-4 md:gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Upload</h1>
        <p className="text-muted-foreground">Connected to StreamVault mock upload endpoints</p>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Upload className="size-5" /> Upload a video
          </CardTitle>
          <CardDescription>
            This uses your backend endpoints:
            <span className="font-mono"> /api/v1/videos/upload/initiate</span> and
            <span className="font-mono"> /api/v1/videos/upload/complete</span>
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form className="grid gap-4" onSubmit={onSubmit}>
            <div className="grid gap-2">
              <Label htmlFor="title">Title</Label>
              <Input id="title" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="My video" />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Description</Label>
              <Input
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Short description"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="file">File</Label>
              <Input
                id="file"
                type="file"
                accept="video/*"
                onChange={(e) => setFile(e.target.files?.[0] ?? null)}
              />
            </div>

            <div className="flex items-center gap-3">
              <Button type="submit">Start Upload</Button>
              {status ? <p className="text-sm text-muted-foreground">{status}</p> : null}
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
