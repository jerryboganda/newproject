"use client";

import { useEffect, useMemo, useState } from "react";

import { HubConnectionBuilder, LogLevel, type HubConnection } from "@microsoft/signalr";
import { Line, LineChart, CartesianGrid, XAxis, YAxis, Tooltip, ResponsiveContainer } from "recharts";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { streamvaultApi, type StreamVaultAnalyticsOverviewResponse } from "@/lib/streamvault-api";

function formatNumber(value: number): string {
  const safe = Number.isFinite(value) ? value : 0;
  return new Intl.NumberFormat(undefined).format(safe);
}

function formatDurationSeconds(seconds: number): string {
  const s = Number.isFinite(seconds) ? Math.max(0, seconds) : 0;
  if (s < 60) return `${Math.round(s)}s`;
  const minutes = Math.round(s / 60);
  if (minutes < 60) return `${minutes}m`;
  const hours = Math.round(minutes / 60);
  return `${hours}h`;
}

export default function Page() {
  const [rangeDays, setRangeDays] = useState<string>("30");
  const [overview, setOverview] = useState<StreamVaultAnalyticsOverviewResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [liveViews, setLiveViews] = useState<number>(0);

  const apiBase = useMemo(
    () => (process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api/v1").replace(/\/$/, ""),
    [],
  );

  const hubBase = useMemo(() => apiBase.replace(/\/api\/v1\/?$/, ""), [apiBase]);

  const startEnd = useMemo(() => {
    const days = Number(rangeDays);
    const end = new Date();
    const start = new Date(end.getTime() - (Number.isFinite(days) ? days : 30) * 24 * 60 * 60 * 1000);
    return { startUtc: start.toISOString(), endUtc: end.toISOString() };
  }, [rangeDays]);

  async function refresh() {
    setLoading(true);
    setError(null);
    try {
      const data = await streamvaultApi.analytics.overview({
        startUtc: startEnd.startUtc,
        endUtc: startEnd.endUtc,
        top: 10,
      });
      setOverview(data);
    } catch (e: any) {
      setError(e?.message || "Failed to load analytics");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await streamvaultApi.analytics.overview({
          startUtc: startEnd.startUtc,
          endUtc: startEnd.endUtc,
          top: 10,
        });
        if (!cancelled) setOverview(data);
      } catch (e: any) {
        if (!cancelled) setError(e?.message || "Failed to load analytics");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [startEnd.endUtc, startEnd.startUtc]);

  useEffect(() => {
    if (typeof window === "undefined") return;

    const token = localStorage.getItem("auth-token");
    if (!token) return;

    let connection: HubConnection | null = null;

    (async () => {
      try {
        connection = new HubConnectionBuilder()
          .withUrl(`${hubBase}/hubs/live-analytics`, {
            accessTokenFactory: () => token,
          })
          .withAutomaticReconnect()
          .configureLogging(LogLevel.Warning)
          .build();

        connection.on("view", () => {
          setLiveViews((v) => v + 1);
        });

        await connection.start();
        await connection.invoke("JoinTenant");
      } catch {
        // Live updates are best-effort.
      }
    })();

    return () => {
      connection?.stop();
    };
  }, [hubBase]);

  const chartData = useMemo(() => {
    return (overview?.viewsByDay ?? []).map((p) => ({
      date: p.dateUtc,
      views: p.views,
    }));
  }, [overview]);

  return (
    <div className="@container/main flex flex-col gap-4 md:gap-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Analytics</h1>
          <p className="text-muted-foreground">Real usage and view analytics from your production data</p>
        </div>

        <div className="w-[180px]">
          <Select value={rangeDays} onValueChange={setRangeDays}>
            <SelectTrigger>
              <SelectValue placeholder="Range" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="7">Last 7 days</SelectItem>
              <SelectItem value="30">Last 30 days</SelectItem>
              <SelectItem value="90">Last 90 days</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {loading ? <div className="text-sm text-muted-foreground">Loading…</div> : null}
      {error ? <div className="text-sm text-red-600">{error}</div> : null}

      {overview ? (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-4">
          <Card>
            <CardHeader>
              <CardTitle>Total views</CardTitle>
              <CardDescription>Selected range</CardDescription>
            </CardHeader>
            <CardContent className="text-2xl font-semibold">{formatNumber(overview.totalViews)}</CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Unique viewers</CardTitle>
              <CardDescription>Approx. (user or session)</CardDescription>
            </CardHeader>
            <CardContent className="text-2xl font-semibold">{formatNumber(overview.uniqueViewers)}</CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Watch time</CardTitle>
              <CardDescription>Total seconds (tracked events)</CardDescription>
            </CardHeader>
            <CardContent className="text-2xl font-semibold">{formatDurationSeconds(overview.totalWatchTimeSeconds)}</CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Live views</CardTitle>
              <CardDescription>Incremental (SignalR)</CardDescription>
            </CardHeader>
            <CardContent className="text-2xl font-semibold">{formatNumber(liveViews)}</CardContent>
          </Card>

          <Card className="lg:col-span-4">
            <CardHeader>
              <CardTitle>Views over time</CardTitle>
              <CardDescription>Daily buckets</CardDescription>
            </CardHeader>
            <CardContent className="h-[300px]">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={chartData} margin={{ top: 10, right: 20, bottom: 0, left: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" hide />
                  <YAxis />
                  <Tooltip />
                  <Line type="monotone" dataKey="views" stroke="currentColor" dot={false} />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>

          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Top videos</CardTitle>
              <CardDescription>By views</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Title</TableHead>
                    <TableHead className="text-right">Views</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {(overview.topVideos ?? []).map((v) => (
                    <TableRow key={v.videoId}>
                      <TableCell className="font-medium">{v.title}</TableCell>
                      <TableCell className="text-right">{formatNumber(v.views)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>

          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Countries</CardTitle>
              <CardDescription>By views</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Country</TableHead>
                    <TableHead className="text-right">Views</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {(overview.countries ?? []).slice(0, 10).map((c) => (
                    <TableRow key={c.countryCode}>
                      <TableCell className="font-medium">{c.countryCode}</TableCell>
                      <TableCell className="text-right">{formatNumber(c.views)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>
      ) : null}
    </div>
  );
}
