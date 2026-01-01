"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";

import { ArrowLeft, Plus, Save, Trash } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import {
  streamvaultApi,
  type StreamVaultAdminEmailTemplate,
  type StreamVaultAdminUpsertEmailTemplateRequest,
} from "@/lib/streamvault-api";

export default function AdminEmailTemplatesPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [templates, setTemplates] = useState<StreamVaultAdminEmailTemplate[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const selected = useMemo(() => templates.find((t) => t.id === selectedId) ?? null, [templates, selectedId]);

  const [draft, setDraft] = useState<StreamVaultAdminEmailTemplate | null>(null);
  const [saving, setSaving] = useState(false);
  const [creating, setCreating] = useState(false);

  async function refresh() {
    setLoading(true);
    setError(null);

    try {
      const items = await streamvaultApi.admin.emailTemplates.list({ page: 1, pageSize: 200 });
      setTemplates(items);
      if (!selectedId && items.length > 0) {
        setSelectedId(items[0].id);
        setDraft(items[0]);
      }
    } catch (e: any) {
      setError(e?.message || "Failed to load templates");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (selected) setDraft(selected);
  }, [selected]);

  async function save() {
    if (!draft) return;

    try {
      setSaving(true);
      setError(null);

      const payload: StreamVaultAdminUpsertEmailTemplateRequest = {
        name: draft.name,
        subject: draft.subject,
        category: draft.category,
        htmlContent: draft.htmlContent,
        textContent: draft.textContent,
        variables: draft.variables,
        isActive: draft.isActive,
      };

      const updated = await streamvaultApi.admin.emailTemplates.update(draft.id, payload);
      setTemplates((prev) => prev.map((t) => (t.id === updated.id ? updated : t)));
      setDraft(updated);
    } catch (e: any) {
      setError(e?.message || "Failed to save template");
    } finally {
      setSaving(false);
    }
  }

  async function createNew() {
    try {
      setCreating(true);
      setError(null);

      const created = await streamvaultApi.admin.emailTemplates.create({
        name: `new_template_${Date.now()}`,
        subject: "Subject",
        category: "general",
        htmlContent: "<p>Hello {{name}}</p>",
        textContent: "Hello {{name}}",
        variables: ["name"],
        isActive: true,
      });

      setTemplates((prev) => [created, ...prev]);
      setSelectedId(created.id);
      setDraft(created);
    } catch (e: any) {
      setError(e?.message || "Failed to create template");
    } finally {
      setCreating(false);
    }
  }

  async function removeSelected() {
    if (!selected) return;

    try {
      setSaving(true);
      setError(null);
      await streamvaultApi.admin.emailTemplates.delete(selected.id);

      const remaining = templates.filter((t) => t.id !== selected.id);
      setTemplates(remaining);
      setSelectedId(remaining[0]?.id ?? null);
      setDraft(remaining[0] ?? null);
    } catch (e: any) {
      setError(e?.message || "Failed to delete template");
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
            <h1 className="text-2xl font-bold">Email Templates</h1>
          </div>
          <p className="text-sm text-muted-foreground">Stored in the database and rendered at send-time.</p>
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={createNew} disabled={creating}>
            <Plus className="h-4 w-4 mr-2" />
            {creating ? "Creating…" : "New"}
          </Button>
          <Button onClick={save} disabled={!draft || saving}>
            <Save className="h-4 w-4 mr-2" />
            {saving ? "Saving…" : "Save"}
          </Button>
        </div>
      </div>

      {error && <div className="text-sm text-red-600">{error}</div>}

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle>Templates</CardTitle>
            <CardDescription>Select a template to edit.</CardDescription>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>Category</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {templates.map((t) => (
                  <TableRow
                    key={t.id}
                    className={t.id === selectedId ? "bg-muted" : "cursor-pointer"}
                    onClick={() => setSelectedId(t.id)}
                  >
                    <TableCell className="font-medium">{t.name}</TableCell>
                    <TableCell>{t.category}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Editor</CardTitle>
            <CardDescription>
              Use <code>{"{{variable}}"}</code> placeholders. Variables list is optional.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {!draft ? (
              <div className="text-sm text-muted-foreground">Select a template.</div>
            ) : (
              <>
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="name">Name</Label>
                    <Input id="name" value={draft.name} onChange={(e) => setDraft({ ...draft, name: e.target.value })} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="category">Category</Label>
                    <Input
                      id="category"
                      value={draft.category}
                      onChange={(e) => setDraft({ ...draft, category: e.target.value })}
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="subject">Subject</Label>
                  <Input
                    id="subject"
                    value={draft.subject}
                    onChange={(e) => setDraft({ ...draft, subject: e.target.value })}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="variables">Variables (comma-separated)</Label>
                  <Input
                    id="variables"
                    value={(draft.variables ?? []).join(", ")}
                    onChange={(e) =>
                      setDraft({
                        ...draft,
                        variables: e.target.value
                          .split(",")
                          .map((x) => x.trim())
                          .filter(Boolean),
                      })
                    }
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <Label>Active</Label>
                    <div className="text-xs text-muted-foreground">Inactive templates won’t be used.</div>
                  </div>
                  <Switch checked={draft.isActive} onCheckedChange={(v) => setDraft({ ...draft, isActive: v })} />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="html">HTML Content</Label>
                  <Textarea
                    id="html"
                    rows={10}
                    value={draft.htmlContent}
                    onChange={(e) => setDraft({ ...draft, htmlContent: e.target.value })}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="text">Text Content</Label>
                  <Textarea
                    id="text"
                    rows={8}
                    value={draft.textContent}
                    onChange={(e) => setDraft({ ...draft, textContent: e.target.value })}
                  />
                </div>

                <div className="flex justify-end">
                  <Button variant="destructive" onClick={removeSelected} disabled={saving}>
                    <Trash className="h-4 w-4 mr-2" />
                    Delete
                  </Button>
                </div>
              </>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
