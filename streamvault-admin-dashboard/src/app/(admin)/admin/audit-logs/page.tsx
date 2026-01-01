"use client";

import { useEffect, useState } from "react";
import Link from "next/link";

import { ArrowLeft, RefreshCcw } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { streamvaultApi, type StreamVaultAdminAuditLogListItem } from "@/lib/streamvault-api";

function formatDate(value: string) {
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return value;
  return d.toLocaleString();
}

export default function AdminAuditLogsPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [items, setItems] = useState<StreamVaultAdminAuditLogListItem[]>([]);

  const [tenantId, setTenantId] = useState("");
  const [userId, setUserId] = useState("");
  const [action, setAction] = useState("");

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const result = await streamvaultApi.admin.auditLogs.list({
        page: 1,
        pageSize: 200,
        tenantId: tenantId.trim() || undefined,
        userId: userId.trim() || undefined,
        action: action.trim() || undefined,
      });
      setItems(result.items);
    } catch (e: any) {
      setError(e?.message || "Failed to load audit logs");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

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
            <h1 className="text-2xl font-bold">Audit Logs</h1>
          </div>
          <p className="text-sm text-muted-foreground">Security-relevant events across the platform.</p>
        </div>
        <Button variant="outline" onClick={load} disabled={loading}>
          <RefreshCcw className="h-4 w-4 mr-2" />
          Refresh
        </Button>
      </div>

      {error && <div className="text-sm text-red-600">{error}</div>}

      <Card>
        <CardHeader>
          <CardTitle>Filters</CardTitle>
          <CardDescription>Optional; leave blank for recent events.</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
            <div className="space-y-2">
              <Label htmlFor="tenantId">Tenant ID</Label>
              <Input id="tenantId" value={tenantId} onChange={(e) => setTenantId(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="userId">User ID</Label>
              <Input id="userId" value={userId} onChange={(e) => setUserId(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="action">Action</Label>
              <Input id="action" value={action} onChange={(e) => setAction(e.target.value)} />
            </div>
            <div className="flex items-end">
              <Button onClick={load} disabled={loading} className="w-full">
                Apply
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Recent Events</CardTitle>
          <CardDescription>Showing up to 200 entries.</CardDescription>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="text-sm text-muted-foreground">Loading…</div>
          ) : items.length === 0 ? (
            <div className="text-sm text-muted-foreground">No entries found.</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Time</TableHead>
                  <TableHead>Action</TableHead>
                  <TableHead>Tenant</TableHead>
                  <TableHead>User</TableHead>
                  <TableHead>IP</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.map((x) => (
                  <TableRow key={x.id}>
                    <TableCell className="whitespace-nowrap">{formatDate(x.createdAt)}</TableCell>
                    <TableCell className="font-medium">{x.action}</TableCell>
                    <TableCell className="font-mono text-xs">{x.tenantId ?? "-"}</TableCell>
                    <TableCell className="font-mono text-xs">{x.userId ?? "-"}</TableCell>
                    <TableCell className="font-mono text-xs">{x.ipAddress ?? "-"}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
