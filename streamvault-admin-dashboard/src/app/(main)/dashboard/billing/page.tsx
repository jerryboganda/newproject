"use client";

import { useEffect, useMemo, useState } from "react";

import { CreditCard, ExternalLink } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  streamvaultApi,
  type StreamVaultBillingSummary,
  type StreamVaultSubscriptionPlan,
  type StreamVaultTenantUsageMultiplierOverride,
} from "@/lib/streamvault-api";

function bytesToGiB(bytes: number): number {
  if (!Number.isFinite(bytes) || bytes <= 0) return 0;
  return bytes / 1024 / 1024 / 1024;
}

function formatMoney(amount: number, currency: string): string {
  const safe = Number.isFinite(amount) ? amount : 0;
  const c = (currency || "USD").toUpperCase();
  return new Intl.NumberFormat(undefined, { style: "currency", currency: c }).format(safe);
}

export default function Page() {
  const [plans, setPlans] = useState<StreamVaultSubscriptionPlan[]>([]);
  const [summary, setSummary] = useState<StreamVaultBillingSummary | null>(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [checkoutBusy, setCheckoutBusy] = useState<string | null>(null);
  const [portalBusy, setPortalBusy] = useState(false);

  const [manualAmount, setManualAmount] = useState<string>("");
  const [manualCurrency, setManualCurrency] = useState<string>("USD");
  const [manualReference, setManualReference] = useState<string>("");
  const [manualNotes, setManualNotes] = useState<string>("");
  const [manualBusy, setManualBusy] = useState(false);

  const [overrideDraft, setOverrideDraft] = useState<Record<string, { multiplier: string; isActive: boolean }>>({});
  const [overrideBusy, setOverrideBusy] = useState<string | null>(null);

  const billingUrl = useMemo(() => {
    if (typeof window === "undefined") return "";
    return `${window.location.origin}/dashboard/billing`;
  }, []);

  async function refresh() {
    setLoading(true);
    setError(null);
    try {
      const [p, s] = await Promise.all([streamvaultApi.billing.plans(), streamvaultApi.billing.summary()]);
      setPlans(p);
      setSummary(s);

      const draft: Record<string, { multiplier: string; isActive: boolean }> = {};
      for (const o of s.tenantUsageMultiplierOverrides ?? []) {
        draft[o.metricType] = { multiplier: String(o.multiplier), isActive: o.isActive };
      }
      setOverrideDraft(draft);
    } catch (e: any) {
      setError(e?.message || "Failed to load billing");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        setError(null);
        const [p, s] = await Promise.all([streamvaultApi.billing.plans(), streamvaultApi.billing.summary()]);
        if (cancelled) return;
        setPlans(p);
        setSummary(s);

        const draft: Record<string, { multiplier: string; isActive: boolean }> = {};
        for (const o of s.tenantUsageMultiplierOverrides ?? []) {
          draft[o.metricType] = { multiplier: String(o.multiplier), isActive: o.isActive };
        }
        setOverrideDraft(draft);
      } catch (e: any) {
        if (!cancelled) setError(e?.message || "Failed to load billing");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  async function startCheckout(planId: string, interval: "monthly" | "yearly") {
    setCheckoutBusy(`${planId}:${interval}`);
    setError(null);
    try {
      const res = await streamvaultApi.billing.createCheckoutSession({
        planId,
        interval,
        successUrl: billingUrl,
        cancelUrl: billingUrl,
      });

      if (!res.url) throw new Error("Stripe Checkout did not return a URL");
      window.location.href = res.url;
    } catch (e: any) {
      setError(e?.message || "Failed to start checkout");
    } finally {
      setCheckoutBusy(null);
    }
  }

  async function openPortal() {
    setPortalBusy(true);
    setError(null);
    try {
      const res = await streamvaultApi.billing.createPortalSession({ returnUrl: billingUrl });
      if (!res.url) throw new Error("Stripe Portal did not return a URL");
      window.location.href = res.url;
    } catch (e: any) {
      setError(e?.message || "Failed to open billing portal");
    } finally {
      setPortalBusy(false);
    }
  }

  async function createManualPayment() {
    const amount = Number(manualAmount);
    if (!Number.isFinite(amount) || amount <= 0) {
      setError("Manual payment amount must be > 0");
      return;
    }

    setManualBusy(true);
    setError(null);
    try {
      await streamvaultApi.billing.createManualPayment({
        amount,
        currency: manualCurrency,
        reference: manualReference || undefined,
        notes: manualNotes || undefined,
      });

      setManualAmount("");
      setManualReference("");
      setManualNotes("");

      await refresh();
    } catch (e: any) {
      setError(e?.message || "Failed to create manual payment");
    } finally {
      setManualBusy(false);
    }
  }

  function getDraft(metricType: string, fallback: StreamVaultTenantUsageMultiplierOverride | null): {
    multiplier: string;
    isActive: boolean;
  } {
    const draft = overrideDraft[metricType];
    if (draft) return draft;
    if (fallback) return { multiplier: String(fallback.multiplier), isActive: fallback.isActive };
    return { multiplier: "1", isActive: true };
  }

  async function saveOverride(metricType: string) {
    const draft = overrideDraft[metricType];
    const multiplier = Number(draft?.multiplier);
    if (!Number.isFinite(multiplier) || multiplier <= 0) {
      setError("Multiplier must be > 0");
      return;
    }

    setOverrideBusy(metricType);
    setError(null);
    try {
      await streamvaultApi.billing.upsertTenantMultiplierOverride({
        metricType,
        multiplier,
        isActive: !!draft?.isActive,
      });
      await refresh();
    } catch (e: any) {
      setError(e?.message || "Failed to save override");
    } finally {
      setOverrideBusy(null);
    }
  }

  const currentPlan = useMemo(() => {
    if (!summary?.currentSubscription) return null;
    return plans.find((p) => p.id === summary.currentSubscription?.planId) || null;
  }, [plans, summary]);

  return (
    <div className="@container/main flex flex-col gap-4 md:gap-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Billing</h1>
          <p className="text-muted-foreground">Manage your StreamVault subscription, invoices, and usage</p>
        </div>
        <Button onClick={openPortal} disabled={portalBusy || loading || !summary?.stripeCustomerId} variant="outline">
          <CreditCard className="mr-2 size-4" />
          {portalBusy ? "Opening…" : "Manage in Stripe"}
        </Button>
      </div>

      {loading ? <div className="text-sm text-muted-foreground">Loading…</div> : null}
      {error ? <div className="text-sm text-red-600">{error}</div> : null}

      {summary ? (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
          <Card className="lg:col-span-1">
            <CardHeader>
              <CardTitle>Current subscription</CardTitle>
              <CardDescription>Plan and renewal status</CardDescription>
            </CardHeader>
            <CardContent className="space-y-2">
              {summary.currentSubscription ? (
                <>
                  <div className="flex items-center justify-between gap-2">
                    <div className="text-sm font-medium">{summary.currentSubscription.planName}</div>
                    <Badge variant="secondary">{summary.currentSubscription.status}</Badge>
                  </div>
                  <div className="text-sm text-muted-foreground">
                    Billing cycle: {summary.currentSubscription.billingCycle}
                  </div>
                  {summary.currentSubscription.currentPeriodEnd ? (
                    <div className="text-sm text-muted-foreground">
                      Renews: {new Date(summary.currentSubscription.currentPeriodEnd).toLocaleString()}
                    </div>
                  ) : null}
                </>
              ) : (
                <div className="text-sm text-muted-foreground">No active subscription yet.</div>
              )}

              {currentPlan?.description ? (
                <div className="text-sm text-muted-foreground">{currentPlan.description}</div>
              ) : null}
            </CardContent>
          </Card>

          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Plans</CardTitle>
              <CardDescription>Start or change your subscription</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
                {plans.map((p) => (
                  <Card key={p.id} className="shadow-none">
                    <CardHeader className="pb-3">
                      <CardTitle className="text-base">{p.name}</CardTitle>
                      {p.description ? <CardDescription>{p.description}</CardDescription> : null}
                    </CardHeader>
                    <CardContent className="flex items-center justify-between gap-2 pt-0">
                      <div className="text-sm text-muted-foreground">
                        {p.isCustom ? "Custom pricing" : `${formatMoney(p.priceMonthly, "USD")}/mo or ${formatMoney(p.priceYearly, "USD")}/yr`}
                      </div>
                      <div className="flex items-center gap-2">
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={!!checkoutBusy || loading || p.isCustom}
                          onClick={() => startCheckout(p.id, "monthly")}
                        >
                          {checkoutBusy === `${p.id}:monthly` ? "Starting…" : "Monthly"}
                        </Button>
                        <Button
                          size="sm"
                          disabled={!!checkoutBusy || loading || p.isCustom}
                          onClick={() => startCheckout(p.id, "yearly")}
                        >
                          {checkoutBusy === `${p.id}:yearly` ? "Starting…" : "Yearly"}
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </CardContent>
          </Card>

          <Card className="lg:col-span-1">
            <CardHeader>
              <CardTitle>Latest usage</CardTitle>
              <CardDescription>Most recent synced snapshot</CardDescription>
            </CardHeader>
            <CardContent className="space-y-2">
              {summary.latestUsage ? (
                <>
                  <div className="text-sm text-muted-foreground">
                    As of: {new Date(summary.latestUsage.periodStartUtc).toLocaleString()}
                  </div>
                  <div className="text-sm">
                    Storage: {bytesToGiB(summary.latestUsage.storageBytes).toFixed(2)} GiB
                  </div>
                  <div className="text-sm">
                    Bandwidth: {bytesToGiB(summary.latestUsage.bandwidthBytes).toFixed(2)} GiB
                  </div>
                  <div className="text-sm">Videos: {summary.latestUsage.videoCount.toLocaleString()}</div>
                </>
              ) : (
                <div className="text-sm text-muted-foreground">No usage snapshots yet.</div>
              )}
            </CardContent>
          </Card>

          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Invoices</CardTitle>
              <CardDescription>Most recent Stripe invoices</CardDescription>
            </CardHeader>
            <CardContent>
              {summary.stripeInvoices?.length ? (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Status</TableHead>
                      <TableHead>Amount</TableHead>
                      <TableHead>Created</TableHead>
                      <TableHead className="text-right">Link</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {summary.stripeInvoices.map((inv) => (
                      <TableRow key={inv.id}>
                        <TableCell>
                          <Badge variant="secondary">{inv.status}</Badge>
                        </TableCell>
                        <TableCell>{formatMoney(inv.amountDue, inv.currency)}</TableCell>
                        <TableCell>{new Date(inv.createdAtUtc).toLocaleString()}</TableCell>
                        <TableCell className="text-right">
                          {inv.hostedInvoiceUrl ? (
                            <a
                              className="inline-flex items-center gap-1 text-sm underline underline-offset-4"
                              href={inv.hostedInvoiceUrl}
                              target="_blank"
                              rel="noreferrer"
                            >
                              View <ExternalLink className="size-3" />
                            </a>
                          ) : inv.invoicePdf ? (
                            <a
                              className="inline-flex items-center gap-1 text-sm underline underline-offset-4"
                              href={inv.invoicePdf}
                              target="_blank"
                              rel="noreferrer"
                            >
                              PDF <ExternalLink className="size-3" />
                            </a>
                          ) : (
                            <span className="text-sm text-muted-foreground">—</span>
                          )}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                <div className="text-sm text-muted-foreground">
                  {summary.stripeCustomerId ? "No invoices yet." : "Create a subscription to see invoices."}
                </div>
              )}
            </CardContent>
          </Card>

          <Card className="lg:col-span-1">
            <CardHeader>
              <CardTitle>Manual payments</CardTitle>
              <CardDescription>Record offline/enterprise payments</CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="space-y-2">
                <div className="grid grid-cols-2 gap-2">
                  <div className="space-y-1">
                    <Label htmlFor="manual-amount">Amount</Label>
                    <Input
                      id="manual-amount"
                      inputMode="decimal"
                      placeholder="100"
                      value={manualAmount}
                      onChange={(e) => setManualAmount(e.target.value)}
                    />
                  </div>
                  <div className="space-y-1">
                    <Label htmlFor="manual-currency">Currency</Label>
                    <Input
                      id="manual-currency"
                      placeholder="USD"
                      value={manualCurrency}
                      onChange={(e) => setManualCurrency(e.target.value)}
                    />
                  </div>
                </div>
                <div className="space-y-1">
                  <Label htmlFor="manual-reference">Reference</Label>
                  <Input
                    id="manual-reference"
                    placeholder="Invoice # / wire ref"
                    value={manualReference}
                    onChange={(e) => setManualReference(e.target.value)}
                  />
                </div>
                <div className="space-y-1">
                  <Label htmlFor="manual-notes">Notes</Label>
                  <Input
                    id="manual-notes"
                    placeholder="Optional"
                    value={manualNotes}
                    onChange={(e) => setManualNotes(e.target.value)}
                  />
                </div>
                <Button className="w-full" onClick={createManualPayment} disabled={manualBusy || loading}>
                  {manualBusy ? "Saving…" : "Add manual payment"}
                </Button>
              </div>

              {summary.manualPayments?.length ? (
                <div className="space-y-2">
                  {summary.manualPayments.slice(0, 10).map((p) => (
                    <div key={p.id} className="rounded-md border p-2">
                      <div className="flex items-center justify-between gap-2">
                        <div className="text-sm font-medium">{formatMoney(p.amount, p.currency)}</div>
                        <div className="text-xs text-muted-foreground">{new Date(p.paidAtUtc).toLocaleString()}</div>
                      </div>
                      {p.reference ? <div className="text-xs text-muted-foreground">Ref: {p.reference}</div> : null}
                      {p.notes ? <div className="text-xs text-muted-foreground">{p.notes}</div> : null}
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-sm text-muted-foreground">No manual payments recorded.</div>
              )}
            </CardContent>
          </Card>

          <Card className="lg:col-span-3">
            <CardHeader>
              <CardTitle>Usage multipliers</CardTitle>
              <CardDescription>Adjust how usage contributes to overages</CardDescription>
            </CardHeader>
            <CardContent>
              {summary.globalUsageMultipliers?.length ? (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Metric</TableHead>
                      <TableHead>Global</TableHead>
                      <TableHead>Override</TableHead>
                      <TableHead>Active</TableHead>
                      <TableHead className="text-right">Action</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {summary.globalUsageMultipliers.map((g) => {
                      const existing =
                        summary.tenantUsageMultiplierOverrides?.find((o) => o.metricType === g.metricType) ?? null;
                      const draft = getDraft(g.metricType, existing);

                      return (
                        <TableRow key={g.id}>
                          <TableCell>
                            <div className="font-medium">{g.name}</div>
                            <div className="text-xs text-muted-foreground">{g.metricType}</div>
                          </TableCell>
                          <TableCell>{g.multiplier.toFixed(4)}</TableCell>
                          <TableCell>
                            <Input
                              inputMode="decimal"
                              value={draft.multiplier}
                              onChange={(e) =>
                                setOverrideDraft((prev) => ({
                                  ...prev,
                                  [g.metricType]: {
                                    multiplier: e.target.value,
                                    isActive: prev[g.metricType]?.isActive ?? existing?.isActive ?? true,
                                  },
                                }))
                              }
                            />
                          </TableCell>
                          <TableCell>
                            <Switch
                              checked={draft.isActive}
                              onCheckedChange={(checked) =>
                                setOverrideDraft((prev) => ({
                                  ...prev,
                                  [g.metricType]: {
                                    multiplier: prev[g.metricType]?.multiplier ?? String(existing?.multiplier ?? 1),
                                    isActive: checked,
                                  },
                                }))
                              }
                            />
                          </TableCell>
                          <TableCell className="text-right">
                            <Button
                              size="sm"
                              variant="outline"
                              disabled={overrideBusy !== null || loading}
                              onClick={() => saveOverride(g.metricType)}
                            >
                              {overrideBusy === g.metricType ? "Saving…" : "Save"}
                            </Button>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              ) : (
                <div className="text-sm text-muted-foreground">No multipliers configured.</div>
              )}
            </CardContent>
          </Card>
        </div>
      ) : null}
    </div>
  );
}
