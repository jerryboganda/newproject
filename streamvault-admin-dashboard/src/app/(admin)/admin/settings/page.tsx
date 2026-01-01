"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";

import { ArrowLeft, Save } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { streamvaultApi, type StreamVaultAdminSystemSettings } from "@/lib/streamvault-api";

export default function AdminSettingsPage() {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [settings, setSettings] = useState<StreamVaultAdminSystemSettings | null>(null);

  const formatsText = useMemo(() => (settings?.supportedVideoFormats ?? []).join(", "), [settings?.supportedVideoFormats]);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        setLoading(true);
        setError(null);
        const s = await streamvaultApi.admin.systemSettings.get();
        if (!cancelled) setSettings(s);
      } catch (e: any) {
        if (!cancelled) setError(e?.message || "Failed to load settings");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  async function save() {
    if (!settings) return;

    try {
      setSaving(true);
      setError(null);

      const formats = formatsText
        .split(",")
        .map((x) => x.trim())
        .filter(Boolean);

      const updated = await streamvaultApi.admin.systemSettings.update({
        allowNewRegistrations: settings.allowNewRegistrations,
        requireEmailVerification: settings.requireEmailVerification,
        defaultSubscriptionPlanId: settings.defaultSubscriptionPlanId ?? null,
        maxFileSizeMB: settings.maxFileSizeMB,
        supportedVideoFormats: formats,
        maintenanceMode: settings.maintenanceMode,
        maintenanceMessage: settings.maintenanceMessage ?? null,
      });

      setSettings(updated);
    } catch (e: any) {
      setError(e?.message || "Failed to save settings");
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <div className="container mx-auto py-8">
        <div className="text-sm text-muted-foreground">Loading…</div>
      </div>
    );
  }

  if (!settings) {
    return (
      <div className="container mx-auto py-8 space-y-4">
        <div className="text-sm text-red-600">{error || "Settings not available"}</div>
        <Button asChild variant="outline">
          <Link href="/admin/dashboard">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="container mx-auto space-y-6 py-8">
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <Button asChild variant="outline" size="sm">
              <Link href="/admin/dashboard">
                <ArrowLeft className="h-4 w-4 mr-2" />
                Back
              </Link>
            </Button>
            <h1 className="text-2xl font-bold">System Settings</h1>
          </div>
          <p className="text-sm text-muted-foreground">Platform-wide behavior and limits.</p>
        </div>

        <Button onClick={save} disabled={saving}>
          <Save className="h-4 w-4 mr-2" />
          {saving ? "Saving…" : "Save"}
        </Button>
      </div>

      {error && <div className="text-sm text-red-600">{error}</div>}

      <Card>
        <CardHeader>
          <CardTitle>Registration</CardTitle>
          <CardDescription>Control who can create accounts.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between gap-6">
            <div className="space-y-1">
              <Label>Allow new registrations</Label>
              <div className="text-xs text-muted-foreground">When off, self-service signup is disabled.</div>
            </div>
            <Switch
              checked={settings.allowNewRegistrations}
              onCheckedChange={(v) => setSettings((s) => (s ? { ...s, allowNewRegistrations: v } : s))}
            />
          </div>

          <div className="flex items-center justify-between gap-6">
            <div className="space-y-1">
              <Label>Require email verification</Label>
              <div className="text-xs text-muted-foreground">New users must verify email before use.</div>
            </div>
            <Switch
              checked={settings.requireEmailVerification}
              onCheckedChange={(v) => setSettings((s) => (s ? { ...s, requireEmailVerification: v } : s))}
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Uploads</CardTitle>
          <CardDescription>Global limits for uploads.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="maxFileSizeMB">Max file size (MB)</Label>
            <Input
              id="maxFileSizeMB"
              type="number"
              min={1}
              value={settings.maxFileSizeMB}
              onChange={(e) =>
                setSettings((s) => (s ? { ...s, maxFileSizeMB: Number(e.target.value || 0) } : s))
              }
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="formats">Supported video formats</Label>
            <div className="text-xs text-muted-foreground">Comma-separated (e.g. mp4, mov, webm).</div>
            <Input
              id="formats"
              value={formatsText}
              onChange={(e) =>
                setSettings((s) => (s ? { ...s, supportedVideoFormats: e.target.value.split(",").map((x) => x.trim()) } : s))
              }
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Maintenance</CardTitle>
          <CardDescription>Temporarily disable normal operation.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between gap-6">
            <div className="space-y-1">
              <Label>Maintenance mode</Label>
              <div className="text-xs text-muted-foreground">Show maintenance banner and restrict access.</div>
            </div>
            <Switch
              checked={settings.maintenanceMode}
              onCheckedChange={(v) => setSettings((s) => (s ? { ...s, maintenanceMode: v } : s))}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="maintenanceMessage">Maintenance message</Label>
            <Textarea
              id="maintenanceMessage"
              value={settings.maintenanceMessage ?? ""}
              onChange={(e) => setSettings((s) => (s ? { ...s, maintenanceMessage: e.target.value } : s))}
            />
          </div>
        </CardContent>
      </Card>

      <div className="flex gap-2">
        <Button asChild variant="outline">
          <Link href="/admin/email-templates">Email Templates</Link>
        </Button>
        <Button asChild variant="outline">
          <Link href="/admin/audit-logs">Audit Logs</Link>
        </Button>
      </div>
    </div>
  );
}
