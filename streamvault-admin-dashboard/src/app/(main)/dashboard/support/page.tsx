"use client";

import { useEffect, useMemo, useState } from "react";

import { HubConnectionBuilder, LogLevel, type HubConnection } from "@microsoft/signalr";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";

import {
  streamvaultApi,
  type StreamVaultKbArticleDetails,
  type StreamVaultKbArticleListItem,
  type StreamVaultKbCategory,
  type StreamVaultSupportDepartment,
  type StreamVaultSupportTicketDetails,
  type StreamVaultSupportTicketListItem,
} from "@/lib/streamvault-api";

import { MarkdownViewer } from "@/components/kb/markdown-viewer";

export default function SupportPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState("tickets");

  const [departments, setDepartments] = useState<StreamVaultSupportDepartment[]>([]);
  const [tickets, setTickets] = useState<StreamVaultSupportTicketListItem[]>([]);
  const [selectedTicket, setSelectedTicket] = useState<StreamVaultSupportTicketDetails | null>(null);
  const [showTicketDialog, setShowTicketDialog] = useState(false);
  const [replyContent, setReplyContent] = useState("");

  const [kbCategories, setKbCategories] = useState<StreamVaultKbCategory[]>([]);
  const [kbArticles, setKbArticles] = useState<StreamVaultKbArticleListItem[]>([]);
  const [selectedArticle, setSelectedArticle] = useState<StreamVaultKbArticleDetails | null>(null);
  const [showArticleDialog, setShowArticleDialog] = useState(false);

  const [showNewTicketDialog, setShowNewTicketDialog] = useState(false);
  const [newTicket, setNewTicket] = useState({
    subject: "",
    description: "",
    departmentId: "",
    priority: "Normal",
  });

  useEffect(() => {
    let cancelled = false;
    async function run() {
      try {
        setLoading(true);
        setError(null);

        const [deptList, ticketList, categoryList, articleList] = await Promise.all([
          streamvaultApi.support.departments.list(),
          streamvaultApi.support.tickets.list(),
          streamvaultApi.kb.categories.list(),
          streamvaultApi.kb.articles.list(),
        ]);

        if (cancelled) return;

        setDepartments(deptList);
        setTickets(ticketList);
        setKbCategories(categoryList);
        setKbArticles(articleList);

        if (!newTicket.departmentId && deptList.length > 0) {
          setNewTicket((prev) => ({ ...prev, departmentId: deptList[0].id }));
        }
      } catch (e: any) {
        if (!cancelled) setError(e?.message || "Failed to load support data");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    run();
    return () => {
      cancelled = true;
    };
  }, []);

  const apiBase = useMemo(
    () => (process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api/v1").replace(/\/$/, ""),
    [],
  );
  const hubBase = useMemo(() => apiBase.replace(/\/api\/v1\/?$/, ""), [apiBase]);

  async function refreshAll() {
    const [ticketList, articleList, categoryList] = await Promise.all([
      streamvaultApi.support.tickets.list(),
      streamvaultApi.kb.articles.list(),
      streamvaultApi.kb.categories.list(),
    ]);
    setTickets(ticketList);
    setKbArticles(articleList);
    setKbCategories(categoryList);
  }

  async function openTicket(ticketId: string) {
    try {
      setError(null);
      const details = await streamvaultApi.support.tickets.get(ticketId);
      setSelectedTicket(details);
      setShowTicketDialog(true);
      setReplyContent("");
    } catch (e: any) {
      setError(e?.message || "Failed to load ticket");
    }
  }

  async function sendReply() {
    if (!selectedTicket) return;
    const content = replyContent.trim();
    if (!content) return;

    try {
      setError(null);
      await streamvaultApi.support.tickets.addMessage(selectedTicket.id, { content, isInternal: false });
      setReplyContent("");
      const updated = await streamvaultApi.support.tickets.get(selectedTicket.id);
      setSelectedTicket(updated);
      const list = await streamvaultApi.support.tickets.list();
      setTickets(list);
    } catch (e: any) {
      setError(e?.message || "Failed to send reply");
    }
  }

  async function openArticle(slug: string) {
    try {
      setError(null);
      const details = await streamvaultApi.kb.articles.getBySlug(slug);
      setSelectedArticle(details);
      setShowArticleDialog(true);
    } catch (e: any) {
      setError(e?.message || "Failed to load article");
    }
  }

  useEffect(() => {
    if (typeof window === "undefined") return;
    const token = localStorage.getItem("auth-token");
    if (!token) return;

    let connection: HubConnection | null = null;

    (async () => {
      try {
        connection = new HubConnectionBuilder()
          .withUrl(`${hubBase}/hubs/support`, {
            accessTokenFactory: () => token,
          })
          .withAutomaticReconnect()
          .configureLogging(LogLevel.Warning)
          .build();

        connection.on("ticketChanged", async (payload: any) => {
          try {
            await refreshAll();
            if (payload?.ticketId && selectedTicket?.id === payload.ticketId) {
              const updated = await streamvaultApi.support.tickets.get(payload.ticketId);
              setSelectedTicket(updated);
            }
          } catch {
            // Best-effort.
          }
        });

        await connection.start();
        await connection.invoke("JoinTenant");
      } catch {
        // Best-effort.
      }
    })();

    return () => {
      connection?.stop();
    };
  }, [hubBase, selectedTicket?.id]);

  const ticketCount = useMemo(() => tickets.length, [tickets]);

  async function createTicket() {
    try {
      await streamvaultApi.support.tickets.create({
        subject: newTicket.subject,
        description: newTicket.description,
        departmentId: newTicket.departmentId,
        priority: newTicket.priority,
      });

      setShowNewTicketDialog(false);
      setNewTicket({
        subject: "",
        description: "",
        departmentId: departments[0]?.id ?? "",
        priority: "Normal",
      });

      const updated = await streamvaultApi.support.tickets.list();
      setTickets(updated);
    } catch (e: any) {
      setError(e?.message || "Failed to create ticket");
    }
  }

  return (
    <div className="@container/main flex flex-col gap-4 md:gap-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Support</h1>
          <p className="text-muted-foreground">Tickets and knowledge base</p>
        </div>

        <Dialog open={showNewTicketDialog} onOpenChange={setShowNewTicketDialog}>
          <DialogTrigger asChild>
            <Button>Create Ticket</Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Create Ticket</DialogTitle>
              <DialogDescription>Describe your issue and we’ll respond.</DialogDescription>
            </DialogHeader>

            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium">Subject</label>
                <Input value={newTicket.subject} onChange={(e) => setNewTicket({ ...newTicket, subject: e.target.value })} />
              </div>
              <div>
                <label className="text-sm font-medium">Description</label>
                <Textarea value={newTicket.description} onChange={(e) => setNewTicket({ ...newTicket, description: e.target.value })} rows={5} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium">Department</label>
                  <Select value={newTicket.departmentId} onValueChange={(value) => setNewTicket({ ...newTicket, departmentId: value })}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select department" />
                    </SelectTrigger>
                    <SelectContent>
                      {departments.map((d) => (
                        <SelectItem key={d.id} value={d.id}>
                          {d.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <label className="text-sm font-medium">Priority</label>
                  <Select value={newTicket.priority} onValueChange={(value) => setNewTicket({ ...newTicket, priority: value })}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Low">Low</SelectItem>
                      <SelectItem value="Normal">Normal</SelectItem>
                      <SelectItem value="High">High</SelectItem>
                      <SelectItem value="Urgent">Urgent</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>

            <DialogFooter>
              <Button
                onClick={createTicket}
                disabled={!newTicket.subject || !newTicket.description || !newTicket.departmentId}
              >
                Submit
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      {loading ? <div className="text-sm text-muted-foreground">Loading…</div> : null}
      {error ? <div className="text-sm text-red-600">{error}</div> : null}

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="tickets">Tickets</TabsTrigger>
          <TabsTrigger value="knowledge-base">Knowledge Base</TabsTrigger>
        </TabsList>

        <TabsContent value="tickets" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Your Tickets</CardTitle>
              <CardDescription>{ticketCount.toLocaleString()} total</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {tickets.map((t) => (
                  <button
                    key={t.id}
                    type="button"
                    onClick={() => openTicket(t.id)}
                    className="w-full text-left rounded-lg border p-3 hover:bg-muted/50"
                  >
                    <div className="flex items-center justify-between gap-3">
                      <div className="min-w-0">
                        <div className="truncate font-medium">{t.subject}</div>
                        <div className="text-xs text-muted-foreground">{t.ticketNumber} • {t.departmentName} • {t.status}</div>
                      </div>
                      <div className="text-xs text-muted-foreground">{new Date(t.updatedAt).toLocaleString()}</div>
                    </div>
                  </button>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="knowledge-base" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Articles</CardTitle>
              <CardDescription>Browse help articles by category.</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="mb-4 flex flex-wrap gap-2 text-sm text-muted-foreground">
                {kbCategories.map((c) => (
                  <span key={c.id} className="rounded-md border px-2 py-1">
                    {c.name}
                  </span>
                ))}
              </div>
              <div className="space-y-3">
                {kbArticles.map((a) => (
                  <button
                    key={a.id}
                    type="button"
                    onClick={() => openArticle(a.slug)}
                    className="w-full text-left rounded-lg border p-3 hover:bg-muted/50"
                  >
                    <div className="font-medium">{a.title}</div>
                    {a.summary ? <div className="text-sm text-muted-foreground">{a.summary}</div> : null}
                    <div className="mt-1 text-xs text-muted-foreground">{a.categoryName} • {a.views} views</div>
                  </button>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <Dialog open={showTicketDialog} onOpenChange={setShowTicketDialog}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Ticket</DialogTitle>
            <DialogDescription>{selectedTicket ? `${selectedTicket.ticketNumber} • ${selectedTicket.status}` : ""}</DialogDescription>
          </DialogHeader>

          {selectedTicket ? (
            <div className="space-y-4">
              <div className="rounded-lg border p-3">
                <div className="font-medium">{selectedTicket.subject}</div>
                <div className="mt-1 text-sm text-muted-foreground whitespace-pre-wrap">{selectedTicket.description}</div>
              </div>

              <div className="space-y-2">
                <div className="text-sm font-medium">Messages</div>
                <div className="max-h-[280px] overflow-auto space-y-2 rounded-lg border p-3">
                  {selectedTicket.replies.length === 0 ? (
                    <div className="text-sm text-muted-foreground">No replies yet.</div>
                  ) : (
                    selectedTicket.replies.map((r) => (
                      <div key={r.id} className="rounded-md border p-2">
                        <div className="flex items-center justify-between gap-2">
                          <div className="text-sm font-medium">{r.userName}</div>
                          <div className="text-xs text-muted-foreground">{new Date(r.createdAt).toLocaleString()}</div>
                        </div>
                        <div className="mt-1 text-sm whitespace-pre-wrap">{r.content}</div>
                      </div>
                    ))
                  )}
                </div>
              </div>

              <div className="space-y-2">
                <div className="text-sm font-medium">Reply</div>
                <Textarea value={replyContent} onChange={(e) => setReplyContent(e.target.value)} rows={4} />
                <div className="flex justify-end">
                  <Button onClick={sendReply} disabled={!replyContent.trim()}>
                    Send
                  </Button>
                </div>
              </div>
            </div>
          ) : null}
        </DialogContent>
      </Dialog>

      <Dialog open={showArticleDialog} onOpenChange={setShowArticleDialog}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>{selectedArticle?.title ?? "Article"}</DialogTitle>
            <DialogDescription>{selectedArticle?.categoryName ?? ""}</DialogDescription>
          </DialogHeader>

          {selectedArticle ? (
            <div className="max-h-[70vh] overflow-auto rounded-lg border p-4">
              <MarkdownViewer markdown={selectedArticle.content} />
            </div>
          ) : null}
        </DialogContent>
      </Dialog>
    </div>
  );
}
