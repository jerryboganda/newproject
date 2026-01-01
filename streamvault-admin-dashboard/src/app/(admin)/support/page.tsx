"use client";

import { useState, useEffect } from "react";
import { HubConnectionBuilder, LogLevel, type HubConnection } from "@microsoft/signalr";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs";
import {
  Search,
  Filter,
  Plus,
  Reply,
  MoreHorizontal,
  Mail,
  MessageSquare,
  Clock,
  User,
  AlertCircle,
  CheckCircle,
  XCircle,
  ArrowUpCircle,
  ArrowDownCircle,
  ExternalLink,
  Tag,
  Calendar,
  Paperclip,
  Send,
  Eye,
  Edit,
  Trash2,
  Archive,
  Flag,
  UserCheck,
  Zap,
  MessageSquare as MessageIcon,
} from "lucide-react";
import { formatDistanceToNow } from "date-fns";

import { MarkdownEditor } from "@/components/kb/markdown-editor";
import { MarkdownViewer } from "@/components/kb/markdown-viewer";
import {
  streamvaultApi,
  type StreamVaultCannedResponse,
  type StreamVaultKbArticleDetails,
  type StreamVaultKbArticleListItem,
  type StreamVaultKbCategory,
  type StreamVaultSupportAgent,
  type StreamVaultSupportDepartment,
  type StreamVaultSupportEscalationRule,
  type StreamVaultSupportSlaPolicy,
  type StreamVaultSupportTicketDetails,
  type StreamVaultSupportTicketListItem,
} from "@/lib/streamvault-api";

type SupportTicket = StreamVaultSupportTicketListItem;
type KnowledgeBaseArticle = StreamVaultKbArticleListItem;
type CannedResponse = StreamVaultCannedResponse;

export default function SupportCenter() {
  const [tickets, setTickets] = useState<SupportTicket[]>([]);
  const [selectedTicket, setSelectedTicket] = useState<StreamVaultSupportTicketDetails | null>(null);
  const [kbArticles, setKbArticles] = useState<KnowledgeBaseArticle[]>([]);
  const [cannedResponses, setCannedResponses] = useState<CannedResponse[]>([]);
  const [departments, setDepartments] = useState<StreamVaultSupportDepartment[]>([]);
  const [slaPolicies, setSlaPolicies] = useState<StreamVaultSupportSlaPolicy[]>([]);
  const [escalationRules, setEscalationRules] = useState<StreamVaultSupportEscalationRule[]>([]);
  const [agents, setAgents] = useState<StreamVaultSupportAgent[]>([]);
  const [kbCategories, setKbCategories] = useState<StreamVaultKbCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("tickets");
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [priorityFilter, setPriorityFilter] = useState<string>("all");
  const [showNewTicketDialog, setShowNewTicketDialog] = useState(false);
  const [showNewArticleDialog, setShowNewArticleDialog] = useState(false);
  const [showNewCategoryDialog, setShowNewCategoryDialog] = useState(false);
  const [showEditArticleDialog, setShowEditArticleDialog] = useState(false);
  const [showPreviewArticleDialog, setShowPreviewArticleDialog] = useState(false);
  const [previewArticle, setPreviewArticle] = useState<StreamVaultKbArticleDetails | null>(null);
  const [showNewCannedDialog, setShowNewCannedDialog] = useState(false);
  const [showEditCannedDialog, setShowEditCannedDialog] = useState(false);
  const [editingCanned, setEditingCanned] = useState<StreamVaultCannedResponse | null>(null);
  const [showDeleteCannedDialog, setShowDeleteCannedDialog] = useState(false);
  const [deletingCanned, setDeletingCanned] = useState<StreamVaultCannedResponse | null>(null);
  const [replyContent, setReplyContent] = useState("");
  const [isInternalReply, setIsInternalReply] = useState(false);

  const [showNewDepartmentDialog, setShowNewDepartmentDialog] = useState(false);
  const [showEditDepartmentDialog, setShowEditDepartmentDialog] = useState(false);
  const [editingDepartment, setEditingDepartment] = useState<StreamVaultSupportDepartment | null>(null);

  const [showNewSlaDialog, setShowNewSlaDialog] = useState(false);
  const [showEditSlaDialog, setShowEditSlaDialog] = useState(false);
  const [editingSla, setEditingSla] = useState<StreamVaultSupportSlaPolicy | null>(null);

  const [showNewEscalationDialog, setShowNewEscalationDialog] = useState(false);
  const [showEditEscalationDialog, setShowEditEscalationDialog] = useState(false);
  const [editingEscalation, setEditingEscalation] = useState<StreamVaultSupportEscalationRule | null>(null);

  // New ticket form state
  const [newTicket, setNewTicket] = useState({
    subject: "",
    description: "",
    departmentId: "",
    priority: "Normal",
  });

  const [newArticle, setNewArticle] = useState({
    title: "",
    summary: "",
    categoryId: "",
    tags: "",
    isPublished: false,
    content: "",
  });

  const [newCategory, setNewCategory] = useState({
    name: "",
    slug: "",
    description: "",
    sortOrder: "0",
  });

  const [editingArticle, setEditingArticle] = useState<null | {
    id: string;
    slug: string;
    title: string;
    summary: string;
    categoryId: string;
    tags: string;
    isPublished: boolean;
    content: string;
  }>(null);

  const [newCanned, setNewCanned] = useState({
    name: "",
    category: "general",
    shortcuts: "",
    content: "",
  });

  const [newDepartment, setNewDepartment] = useState({
    name: "",
    slug: "",
    defaultSlaPolicyId: "",
  });

  const [newSla, setNewSla] = useState({
    name: "",
    firstResponseMinutes: "60",
    resolutionMinutes: "1440",
  });

  const [newEscalation, setNewEscalation] = useState({
    name: "",
    trigger: "FirstResponseOverdue",
    thresholdMinutes: "60",
    escalateToPriority: "High",
    setStatusToEscalated: true,
  });

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (typeof window === "undefined") return;
    const token = localStorage.getItem("auth-token");
    if (!token) return;

    const apiBase = (process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api/v1").replace(/\/$/, "");
    const hubBase = apiBase.replace(/\/api\/v1\/?$/, "");

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
            await loadData();
            if (payload?.ticketId && selectedTicket?.id === payload.ticketId) {
              await loadTicketDetails(payload.ticketId);
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
  }, [selectedTicket?.id]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [deptList, slaList, escalationList, agentList, categoryList, ticketList, articleList, cannedList] = await Promise.all([
        streamvaultApi.support.departments.list(),
        streamvaultApi.support.slaPolicies.list(),
        streamvaultApi.support.escalationRules.list(),
        streamvaultApi.support.agents.list(),
        streamvaultApi.kb.categories.list(),
        streamvaultApi.support.tickets.list(),
        streamvaultApi.kb.articles.list(),
        streamvaultApi.support.cannedResponses.list(),
      ]);

      setDepartments(deptList);
      setSlaPolicies(slaList);
      setEscalationRules(escalationList);
      setAgents(agentList);
      setKbCategories(categoryList);
      setTickets(ticketList);
      setKbArticles(articleList);
      setCannedResponses(cannedList);

      if (!newTicket.departmentId && deptList.length > 0) {
        setNewTicket((prev) => ({ ...prev, departmentId: deptList[0].id }));
      }

      if (!newArticle.categoryId && categoryList.length > 0) {
        setNewArticle((prev) => ({ ...prev, categoryId: categoryList[0].id }));
      }
    } catch (error) {
      console.error("Failed to load data:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleAssignTicket = async (ticketId: string, assignedToUserId: string | null) => {
    try {
      await streamvaultApi.support.tickets.assign(ticketId, assignedToUserId);
      await loadData();
      await loadTicketDetails(ticketId);
    } catch (error) {
      console.error("Failed to assign ticket:", error);
    }
  };

  const handleCreateDepartment = async () => {
    try {
      await streamvaultApi.support.departments.create({
        name: newDepartment.name,
        slug: newDepartment.slug || undefined,
        defaultSlaPolicyId: newDepartment.defaultSlaPolicyId || null,
      });
      setShowNewDepartmentDialog(false);
      setNewDepartment({ name: "", slug: "", defaultSlaPolicyId: "" });
      await loadData();
    } catch (error) {
      console.error("Failed to create department:", error);
    }
  };

  const handleUpdateDepartment = async () => {
    if (!editingDepartment) return;
    try {
      await streamvaultApi.support.departments.update(editingDepartment.id, {
        name: editingDepartment.name,
        slug: editingDepartment.slug,
        defaultSlaPolicyId: editingDepartment.defaultSlaPolicyId ?? null,
      });
      setShowEditDepartmentDialog(false);
      setEditingDepartment(null);
      await loadData();
    } catch (error) {
      console.error("Failed to update department:", error);
    }
  };

  const handleSetDepartmentStatus = async (id: string, isActive: boolean) => {
    try {
      await streamvaultApi.support.departments.setStatus(id, isActive);
      await loadData();
    } catch (error) {
      console.error("Failed to update department status:", error);
    }
  };

  const handleCreateSla = async () => {
    try {
      await streamvaultApi.support.slaPolicies.create({
        name: newSla.name,
        firstResponseMinutes: Number(newSla.firstResponseMinutes),
        resolutionMinutes: Number(newSla.resolutionMinutes),
      });
      setShowNewSlaDialog(false);
      setNewSla({ name: "", firstResponseMinutes: "60", resolutionMinutes: "1440" });
      await loadData();
    } catch (error) {
      console.error("Failed to create SLA policy:", error);
    }
  };

  const handleUpdateSla = async () => {
    if (!editingSla) return;
    try {
      await streamvaultApi.support.slaPolicies.update(editingSla.id, {
        name: editingSla.name,
        firstResponseMinutes: editingSla.firstResponseMinutes,
        resolutionMinutes: editingSla.resolutionMinutes,
      });
      setShowEditSlaDialog(false);
      setEditingSla(null);
      await loadData();
    } catch (error) {
      console.error("Failed to update SLA policy:", error);
    }
  };

  const handleSetSlaStatus = async (id: string, isActive: boolean) => {
    try {
      await streamvaultApi.support.slaPolicies.setStatus(id, isActive);
      await loadData();
    } catch (error) {
      console.error("Failed to update SLA policy status:", error);
    }
  };

  const handleDeleteSla = async (id: string) => {
    try {
      await streamvaultApi.support.slaPolicies.delete(id);
      await loadData();
    } catch (error) {
      console.error("Failed to delete SLA policy:", error);
    }
  };

  const handleCreateEscalation = async () => {
    try {
      await streamvaultApi.support.escalationRules.create({
        name: newEscalation.name,
        trigger: newEscalation.trigger,
        thresholdMinutes: Number(newEscalation.thresholdMinutes),
        escalateToPriority: newEscalation.escalateToPriority,
        setStatusToEscalated: newEscalation.setStatusToEscalated,
      });
      setShowNewEscalationDialog(false);
      setNewEscalation({
        name: "",
        trigger: "FirstResponseOverdue",
        thresholdMinutes: "60",
        escalateToPriority: "High",
        setStatusToEscalated: true,
      });
      await loadData();
    } catch (error) {
      console.error("Failed to create escalation rule:", error);
    }
  };

  const handleUpdateEscalation = async () => {
    if (!editingEscalation) return;
    try {
      await streamvaultApi.support.escalationRules.update(editingEscalation.id, {
        name: editingEscalation.name,
        trigger: editingEscalation.trigger,
        thresholdMinutes: editingEscalation.thresholdMinutes,
        escalateToPriority: editingEscalation.escalateToPriority,
        setStatusToEscalated: editingEscalation.setStatusToEscalated,
      });
      setShowEditEscalationDialog(false);
      setEditingEscalation(null);
      await loadData();
    } catch (error) {
      console.error("Failed to update escalation rule:", error);
    }
  };

  const handleSetEscalationStatus = async (id: string, isActive: boolean) => {
    try {
      await streamvaultApi.support.escalationRules.setStatus(id, isActive);
      await loadData();
    } catch (error) {
      console.error("Failed to update escalation rule status:", error);
    }
  };

  const handleDeleteEscalation = async (id: string) => {
    try {
      await streamvaultApi.support.escalationRules.delete(id);
      await loadData();
    } catch (error) {
      console.error("Failed to delete escalation rule:", error);
    }
  };

  const handleCreateTicket = async () => {
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
      await loadData();
    } catch (error) {
      console.error("Failed to create ticket:", error);
    }
  };

  const handleReplyToTicket = async () => {
    if (!selectedTicket || !replyContent) return;

    try {
      await streamvaultApi.support.tickets.addMessage(selectedTicket.id, {
        content: replyContent,
        isInternal: isInternalReply,
      });
      setReplyContent("");
      setIsInternalReply(false);
      await loadTicketDetails(selectedTicket.id);
    } catch (error) {
      console.error("Failed to reply to ticket:", error);
    }
  };

  const loadTicketDetails = async (ticketId: string) => {
    try {
      const ticket = await streamvaultApi.support.tickets.get(ticketId);
      setSelectedTicket(ticket);
    } catch (error) {
      console.error("Failed to load ticket details:", error);
    }
  };

  const handleUpdateTicketStatus = async (ticketId: string, status: string) => {
    try {
      await streamvaultApi.support.tickets.updateStatus(ticketId, status);
      await loadData();
    } catch (error) {
      console.error("Failed to update ticket:", error);
    }
  };

  const handleCreateArticle = async () => {
    try {
      await streamvaultApi.kb.articles.create({
        title: newArticle.title,
        categoryId: newArticle.categoryId,
        summary: newArticle.summary || undefined,
        tags: newArticle.tags
          ? newArticle.tags
              .split(",")
              .map((t) => t.trim())
              .filter(Boolean)
          : undefined,
        isPublished: newArticle.isPublished,
        content: newArticle.content,
      });
      setShowNewArticleDialog(false);
      setNewArticle({
        title: "",
        summary: "",
        categoryId: kbCategories[0]?.id ?? "",
        tags: "",
        isPublished: false,
        content: "",
      });
      await loadData();
    } catch (error) {
      console.error("Failed to create KB article:", error);
    }
  };

  const handleCreateCategory = async () => {
    try {
      const sortOrder = Number.parseInt(newCategory.sortOrder, 10);
      await streamvaultApi.kb.categories.create({
        name: newCategory.name.trim(),
        slug: newCategory.slug.trim() || undefined,
        description: newCategory.description.trim() || undefined,
        sortOrder: Number.isFinite(sortOrder) ? sortOrder : undefined,
      });
      setShowNewCategoryDialog(false);
      setNewCategory({ name: "", slug: "", description: "", sortOrder: "0" });
      await loadData();
    } catch (error) {
      console.error("Failed to create KB category:", error);
    }
  };

  const openPreviewArticle = async (slug: string) => {
    try {
      const details = await streamvaultApi.kb.articles.getBySlug(slug);
      setPreviewArticle(details);
      setShowPreviewArticleDialog(true);
    } catch (error) {
      console.error("Failed to load KB article:", error);
    }
  };

  const openEditArticle = async (article: KnowledgeBaseArticle) => {
    try {
      const details = await streamvaultApi.kb.articles.getBySlug(article.slug);
      setEditingArticle({
        id: article.id,
        slug: article.slug,
        title: article.title,
        summary: article.summary ?? "",
        categoryId: article.categoryId,
        tags: (article.tags ?? []).join(", "),
        isPublished: article.isPublished,
        content: details.content,
      });
      setShowEditArticleDialog(true);
    } catch (error) {
      console.error("Failed to load KB article for edit:", error);
    }
  };

  const handleUpdateArticle = async () => {
    if (!editingArticle) return;

    try {
      await streamvaultApi.kb.articles.update(editingArticle.id, {
        title: editingArticle.title.trim(),
        summary: editingArticle.summary.trim() || undefined,
        categoryId: editingArticle.categoryId,
        content: editingArticle.content,
        tags: editingArticle.tags
          ? editingArticle.tags
              .split(",")
              .map((t) => t.trim())
              .filter(Boolean)
          : undefined,
        isPublished: editingArticle.isPublished,
      });

      setShowEditArticleDialog(false);
      setEditingArticle(null);
      await loadData();
    } catch (error) {
      console.error("Failed to update KB article:", error);
    }
  };

  const handleCreateCanned = async () => {
    try {
      await streamvaultApi.support.cannedResponses.create({
        name: newCanned.name,
        category: newCanned.category,
        content: newCanned.content,
        shortcuts: newCanned.shortcuts
          ? newCanned.shortcuts
              .split(",")
              .map((s) => s.trim())
              .filter(Boolean)
          : undefined,
      });
      setShowNewCannedDialog(false);
      setNewCanned({ name: "", category: "general", shortcuts: "", content: "" });
      await loadData();
    } catch (error) {
      console.error("Failed to create canned response:", error);
    }
  };

  const handleUpdateCanned = async () => {
    if (!editingCanned) return;
    try {
      await streamvaultApi.support.cannedResponses.update(editingCanned.id, {
        name: editingCanned.name,
        category: editingCanned.category,
        content: editingCanned.content,
        shortcuts: editingCanned.shortcuts,
        isActive: editingCanned.isActive,
      });
      setShowEditCannedDialog(false);
      setEditingCanned(null);
      await loadData();
    } catch (error) {
      console.error("Failed to update canned response:", error);
    }
  };

  const handleDeleteCanned = async () => {
    if (!deletingCanned) return;
    try {
      await streamvaultApi.support.cannedResponses.delete(deletingCanned.id);
      setShowDeleteCannedDialog(false);
      setDeletingCanned(null);
      await loadData();
    } catch (error) {
      console.error("Failed to delete canned response:", error);
    }
  };

  const toKey = (value: string) =>
    value
      .trim()
      .replace(/\s+/g, "_")
      .replace(/([a-z0-9])([A-Z])/g, "$1_$2")
      .replace(/__+/g, "_")
      .toLowerCase();

  const formatLabel = (value: string) =>
    toKey(value)
      .split("_")
      .filter(Boolean)
      .map((p) => p.charAt(0).toUpperCase() + p.slice(1))
      .join(" ");

  const getPriorityColor = (priority: string) => {
    switch (toKey(priority)) {
      case "urgent":
        return "bg-red-100 text-red-800";
      case "high":
        return "bg-orange-100 text-orange-800";
      case "normal":
        return "bg-blue-100 text-blue-800";
      case "low":
        return "bg-gray-100 text-gray-800";
      case "critical":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const getStatusIcon = (status: string) => {
    switch (toKey(status)) {
      case "open":
        return <Mail className="h-4 w-4" />;
      case "in_progress":
        return <Clock className="h-4 w-4" />;
      case "waiting_for_customer":
        return <MessageSquare className="h-4 w-4" />;
      case "waiting_for_support":
        return <UserCheck className="h-4 w-4" />;
      case "resolved":
        return <CheckCircle className="h-4 w-4" />;
      case "closed":
        return <XCircle className="h-4 w-4" />;
      default:
        return <AlertCircle className="h-4 w-4" />;
    }
  };

  const filteredTickets = tickets.filter(ticket => {
    const matchesSearch = ticket.subject.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         ticket.description.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         ticket.userName.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         ticket.ticketNumber.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesStatus = statusFilter === "all" || toKey(ticket.status) === statusFilter;
    const matchesPriority = priorityFilter === "all" || toKey(ticket.priority) === priorityFilter;
    
    return matchesSearch && matchesStatus && matchesPriority;
  });

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold">Support Center</h1>
          <p className="text-gray-600 dark:text-gray-400">
            Manage customer support tickets and knowledge base
          </p>
        </div>
        <div className="flex gap-2">
          <Button onClick={() => setShowNewTicketDialog(true)}>
            <Plus className="mr-2 h-4 w-4" />
            New Ticket
          </Button>
        </div>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="tickets">Tickets</TabsTrigger>
          <TabsTrigger value="knowledge-base">Knowledge Base</TabsTrigger>
          <TabsTrigger value="canned-responses">Canned Responses</TabsTrigger>
          <TabsTrigger value="departments">Departments</TabsTrigger>
          <TabsTrigger value="sla-policies">SLA Policies</TabsTrigger>
          <TabsTrigger value="escalation-rules">Escalation Rules</TabsTrigger>
        </TabsList>

        <TabsContent value="tickets" className="space-y-6">
          {/* Ticket Filters */}
          <Card>
            <CardContent className="p-4">
              <div className="flex flex-col lg:flex-row gap-4">
                <div className="flex-1 relative">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                  <Input
                    placeholder="Search tickets..."
                    className="pl-10"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                  />
                </div>
                <div className="flex gap-2">
                  <Select value={statusFilter} onValueChange={setStatusFilter}>
                    <SelectTrigger className="w-40">
                      <SelectValue placeholder="Status" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Status</SelectItem>
                      <SelectItem value="open">Open</SelectItem>
                      <SelectItem value="in_progress">In Progress</SelectItem>
                      <SelectItem value="waiting_for_customer">Waiting for Customer</SelectItem>
                      <SelectItem value="waiting_for_support">Waiting for Support</SelectItem>
                      <SelectItem value="resolved">Resolved</SelectItem>
                      <SelectItem value="closed">Closed</SelectItem>
                    </SelectContent>
                  </Select>
                  <Select value={priorityFilter} onValueChange={setPriorityFilter}>
                    <SelectTrigger className="w-40">
                      <SelectValue placeholder="Priority" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Priority</SelectItem>
                      <SelectItem value="urgent">Urgent</SelectItem>
                      <SelectItem value="high">High</SelectItem>
                      <SelectItem value="normal">Normal</SelectItem>
                      <SelectItem value="low">Low</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Tickets List */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2">
              <Card>
                <CardHeader>
                  <CardTitle>Tickets ({filteredTickets.length})</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    {filteredTickets.map((ticket) => (
                      <div
                        key={ticket.id}
                        className={`p-4 border rounded-lg cursor-pointer transition-colors hover:bg-gray-50 ${
                          selectedTicket?.id === ticket.id ? "bg-blue-50 border-blue-200" : ""
                        }`}
                        onClick={() => loadTicketDetails(ticket.id)}
                      >
                        <div className="flex items-start justify-between">
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2 mb-1">
                              <span className="font-medium text-sm">#{ticket.ticketNumber}</span>
                              <Badge variant="outline" className={getPriorityColor(ticket.priority)}>
                                {ticket.priority}
                              </Badge>
                              <Badge variant="outline" className="flex items-center gap-1">
                                {getStatusIcon(ticket.status)}
                                {formatLabel(ticket.status)}
                              </Badge>
                            </div>
                            <h3 className="font-medium truncate">{ticket.subject}</h3>
                            <p className="text-sm text-gray-600 truncate">{ticket.description}</p>
                            <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
                              <span>{ticket.userName}</span>
                              <span>{ticket.departmentName}</span>
                              <span>{formatDistanceToNow(new Date(ticket.createdAt), { addSuffix: true })}</span>
                            </div>
                          </div>
                          {ticket.assignedToName && (
                            <div className="text-right">
                              <p className="text-xs text-gray-500">Assigned to</p>
                              <p className="text-sm font-medium">{ticket.assignedToName}</p>
                            </div>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </div>

            {/* Ticket Details */}
            {selectedTicket && (
              <div className="space-y-4">
                <Card>
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <div>
                        <CardTitle className="text-lg">#{selectedTicket.ticketNumber}</CardTitle>
                        <CardDescription>{selectedTicket.subject}</CardDescription>
                      </div>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="sm">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem onClick={() => handleUpdateTicketStatus(selectedTicket.id, "open")}>
                            <Mail className="mr-2 h-4 w-4" />
                            Mark as Open
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleUpdateTicketStatus(selectedTicket.id, "InProgress")}>
                            <Clock className="mr-2 h-4 w-4" />
                            Mark as In Progress
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleUpdateTicketStatus(selectedTicket.id, "Resolved")}>
                            <CheckCircle className="mr-2 h-4 w-4" />
                            Mark as Resolved
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleUpdateTicketStatus(selectedTicket.id, "Closed")}>
                            <XCircle className="mr-2 h-4 w-4" />
                            Close Ticket
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem>
                            <Flag className="mr-2 h-4 w-4" />
                            Escalate
                          </DropdownMenuItem>
                          <DropdownMenuItem>
                            <Archive className="mr-2 h-4 w-4" />
                            Archive
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      <div>
                        <p className="text-sm font-medium mb-2">Description</p>
                        <p className="text-sm text-gray-600">{selectedTicket.description}</p>
                      </div>
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="font-medium">Department:</span> {selectedTicket.departmentName}
                        </div>
                        <div>
                          <span className="font-medium">Assigned to:</span>
                          <div className="mt-1">
                            <Select
                              value={selectedTicket.assignedToId ?? "unassigned"}
                              onValueChange={(value) =>
                                handleAssignTicket(selectedTicket.id, value === "unassigned" ? null : value)
                              }
                            >
                              <SelectTrigger className="h-9">
                                <SelectValue placeholder="Unassigned" />
                              </SelectTrigger>
                              <SelectContent>
                                <SelectItem value="unassigned">Unassigned</SelectItem>
                                {agents.map((a) => (
                                  <SelectItem key={a.id} value={a.id}>
                                    {a.name}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          </div>
                        </div>
                        <div>
                          <span className="font-medium">Priority:</span>{" "}
                          <Badge variant="outline" className={getPriorityColor(selectedTicket.priority)}>
                            {selectedTicket.priority}
                          </Badge>
                        </div>
                        <div>
                          <span className="font-medium">Created:</span>{" "}
                          {formatDistanceToNow(new Date(selectedTicket.createdAt), { addSuffix: true })}
                        </div>
                        <div>
                          <span className="font-medium">Updated:</span>{" "}
                          {formatDistanceToNow(new Date(selectedTicket.updatedAt), { addSuffix: true })}
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                {/* Reply Section */}
                <Card>
                  <CardHeader>
                    <CardTitle className="text-lg">Reply</CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    {cannedResponses.length > 0 ? (
                      <div className="grid grid-cols-1 gap-2">
                        <label className="text-sm font-medium">Insert canned response</label>
                        <Select
                          value={""}
                          onValueChange={(value) => {
                            const selected = cannedResponses.find((c) => c.id === value);
                            if (!selected) return;
                            setReplyContent((prev) => (prev ? `${prev}\n\n${selected.content}` : selected.content));
                          }}
                        >
                          <SelectTrigger>
                            <SelectValue placeholder="Choose a response…" />
                          </SelectTrigger>
                          <SelectContent>
                            {cannedResponses.map((c) => (
                              <SelectItem key={c.id} value={c.id}>
                                {c.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    ) : null}
                    <Textarea
                      placeholder="Type your reply..."
                      value={replyContent}
                      onChange={(e) => setReplyContent(e.target.value)}
                      rows={4}
                    />
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <input
                          type="checkbox"
                          id="internal"
                          checked={isInternalReply}
                          onChange={(e) => setIsInternalReply(e.target.checked)}
                          className="rounded"
                        />
                        <label htmlFor="internal" className="text-sm">
                          Internal note (not visible to customer)
                        </label>
                      </div>
                      <Button onClick={handleReplyToTicket} disabled={!replyContent}>
                        <Send className="mr-2 h-4 w-4" />
                        Send Reply
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              </div>
            )}
          </div>
        </TabsContent>

        <TabsContent value="knowledge-base" className="space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-start justify-between gap-4">
                <div>
                  <CardTitle>Knowledge Base Articles</CardTitle>
                  <CardDescription>Manage help documentation and FAQs</CardDescription>
                </div>
                <div className="flex items-center gap-2">
                  <Dialog open={showNewCategoryDialog} onOpenChange={setShowNewCategoryDialog}>
                    <DialogTrigger asChild>
                      <Button variant="outline">
                        <Plus className="mr-2 h-4 w-4" />
                        New Category
                      </Button>
                    </DialogTrigger>
                    <DialogContent className="max-w-2xl">
                      <DialogHeader>
                        <DialogTitle>Create KB Category</DialogTitle>
                        <DialogDescription>Categories organize knowledge base articles</DialogDescription>
                      </DialogHeader>
                      <div className="space-y-4">
                        <div>
                          <label className="text-sm font-medium">Name</label>
                          <Input value={newCategory.name} onChange={(e) => setNewCategory({ ...newCategory, name: e.target.value })} />
                        </div>
                        <div>
                          <label className="text-sm font-medium">Slug (optional)</label>
                          <Input value={newCategory.slug} onChange={(e) => setNewCategory({ ...newCategory, slug: e.target.value })} placeholder="billing" />
                        </div>
                        <div>
                          <label className="text-sm font-medium">Description (optional)</label>
                          <Textarea value={newCategory.description} onChange={(e) => setNewCategory({ ...newCategory, description: e.target.value })} rows={2} />
                        </div>
                        <div>
                          <label className="text-sm font-medium">Sort Order</label>
                          <Input value={newCategory.sortOrder} onChange={(e) => setNewCategory({ ...newCategory, sortOrder: e.target.value })} />
                        </div>
                      </div>
                      <DialogFooter>
                        <Button onClick={handleCreateCategory} disabled={!newCategory.name.trim()}>
                          Create Category
                        </Button>
                      </DialogFooter>
                    </DialogContent>
                  </Dialog>

                  <Dialog open={showNewArticleDialog} onOpenChange={setShowNewArticleDialog}>
                    <DialogTrigger asChild>
                      <Button>
                        <Plus className="mr-2 h-4 w-4" />
                        New Article
                      </Button>
                    </DialogTrigger>
                    <DialogContent className="max-w-3xl">
                      <DialogHeader>
                        <DialogTitle>Create KB Article</DialogTitle>
                        <DialogDescription>Write in Markdown (TipTap)</DialogDescription>
                      </DialogHeader>
                      <div className="space-y-4">
                        <div>
                          <label className="text-sm font-medium">Title</label>
                          <Input value={newArticle.title} onChange={(e) => setNewArticle({ ...newArticle, title: e.target.value })} />
                        </div>
                        <div>
                          <label className="text-sm font-medium">Summary</label>
                          <Textarea value={newArticle.summary} onChange={(e) => setNewArticle({ ...newArticle, summary: e.target.value })} rows={2} />
                        </div>
                        <div className="grid grid-cols-2 gap-4">
                          <div>
                            <label className="text-sm font-medium">Category</label>
                            <Select value={newArticle.categoryId} onValueChange={(value) => setNewArticle({ ...newArticle, categoryId: value })}>
                              <SelectTrigger>
                                <SelectValue placeholder="Select category" />
                              </SelectTrigger>
                              <SelectContent>
                                {kbCategories.map((c) => (
                                  <SelectItem key={c.id} value={c.id}>
                                    {c.name}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          </div>
                          <div>
                            <label className="text-sm font-medium">Tags (comma-separated)</label>
                            <Input value={newArticle.tags} onChange={(e) => setNewArticle({ ...newArticle, tags: e.target.value })} placeholder="billing, upload, playback" />
                          </div>
                        </div>
                        <div>
                          <label className="text-sm font-medium">Content</label>
                          <MarkdownEditor value={newArticle.content} onChange={(md) => setNewArticle({ ...newArticle, content: md })} placeholder="Write your article…" />
                        </div>
                        <div className="flex items-center gap-2">
                          <input
                            type="checkbox"
                            id="publish"
                            checked={newArticle.isPublished}
                            onChange={(e) => setNewArticle({ ...newArticle, isPublished: e.target.checked })}
                            className="rounded"
                          />
                          <label htmlFor="publish" className="text-sm">
                            Publish immediately
                          </label>
                        </div>
                      </div>
                      <DialogFooter>
                        <Button onClick={handleCreateArticle} disabled={!newArticle.title || !newArticle.categoryId || !newArticle.content}>
                          Create Article
                        </Button>
                      </DialogFooter>
                    </DialogContent>
                  </Dialog>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {kbArticles.map((article) => (
                  <div key={article.id} className="p-4 border rounded-lg">
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <h3 className="font-medium mb-1">{article.title}</h3>
                        <p className="text-sm text-gray-600 mb-2">{article.summary}</p>
                        <div className="flex items-center gap-4 text-xs text-gray-500">
                          <span className="flex items-center gap-1">
                            <Eye className="h-3 w-3" />
                            {article.views} views
                          </span>
                          <span className="flex items-center gap-1">
                            <ArrowUpCircle className="h-3 w-3" />
                            {article.helpfulVotes} helpful
                          </span>
                          <span>{article.categoryName}</span>
                          <div className="flex gap-1">
                            {article.tags.map((tag) => (
                              <Badge key={tag} variant="outline" className="text-xs">
                                {tag}
                              </Badge>
                            ))}
                          </div>
                        </div>
                      </div>
                      <div className="flex items-center gap-1">
                        <Button variant="ghost" size="sm" onClick={() => openPreviewArticle(article.slug)}>
                          <ExternalLink className="h-4 w-4" />
                        </Button>
                        <Button variant="outline" size="sm" onClick={() => openEditArticle(article)}>
                          <Edit className="mr-2 h-4 w-4" />
                          Edit
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <Dialog open={showEditArticleDialog} onOpenChange={setShowEditArticleDialog}>
          <DialogContent className="max-w-3xl">
            <DialogHeader>
              <DialogTitle>Edit KB Article</DialogTitle>
              <DialogDescription>Update content and publishing state</DialogDescription>
            </DialogHeader>

            {editingArticle ? (
              <div className="space-y-4">
                <div>
                  <label className="text-sm font-medium">Title</label>
                  <Input value={editingArticle.title} onChange={(e) => setEditingArticle({ ...editingArticle, title: e.target.value })} />
                </div>
                <div>
                  <label className="text-sm font-medium">Summary</label>
                  <Textarea value={editingArticle.summary} onChange={(e) => setEditingArticle({ ...editingArticle, summary: e.target.value })} rows={2} />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm font-medium">Category</label>
                    <Select value={editingArticle.categoryId} onValueChange={(value) => setEditingArticle({ ...editingArticle, categoryId: value })}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select category" />
                      </SelectTrigger>
                      <SelectContent>
                        {kbCategories.map((c) => (
                          <SelectItem key={c.id} value={c.id}>
                            {c.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <label className="text-sm font-medium">Tags (comma-separated)</label>
                    <Input value={editingArticle.tags} onChange={(e) => setEditingArticle({ ...editingArticle, tags: e.target.value })} />
                  </div>
                </div>
                <div>
                  <label className="text-sm font-medium">Content</label>
                  <MarkdownEditor value={editingArticle.content} onChange={(md) => setEditingArticle({ ...editingArticle, content: md })} placeholder="Write your article…" />
                </div>
                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="publishEdit"
                    checked={editingArticle.isPublished}
                    onChange={(e) => setEditingArticle({ ...editingArticle, isPublished: e.target.checked })}
                    className="rounded"
                  />
                  <label htmlFor="publishEdit" className="text-sm">
                    Published
                  </label>
                </div>
              </div>
            ) : null}

            <DialogFooter>
              <Button variant="outline" onClick={() => setShowEditArticleDialog(false)}>
                Cancel
              </Button>
              <Button
                onClick={handleUpdateArticle}
                disabled={!editingArticle?.title.trim() || !editingArticle?.categoryId || !editingArticle?.content}
              >
                Save Changes
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        <Dialog open={showPreviewArticleDialog} onOpenChange={setShowPreviewArticleDialog}>
          <DialogContent className="max-w-3xl">
            <DialogHeader>
              <DialogTitle>{previewArticle?.title ?? "Article"}</DialogTitle>
              <DialogDescription>{previewArticle?.categoryName ?? ""}</DialogDescription>
            </DialogHeader>

            {previewArticle ? (
              <div className="max-h-[70vh] overflow-auto rounded-lg border p-4">
                <MarkdownViewer markdown={previewArticle.content} />
              </div>
            ) : (
              <div className="text-sm text-muted-foreground">Loading…</div>
            )}
          </DialogContent>
        </Dialog>

        <TabsContent value="canned-responses" className="space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-start justify-between gap-4">
                <div>
                  <CardTitle>Canned Responses</CardTitle>
                  <CardDescription>Quick response templates for common issues</CardDescription>
                </div>
                <Dialog open={showNewCannedDialog} onOpenChange={setShowNewCannedDialog}>
                  <DialogTrigger asChild>
                    <Button>
                      <Plus className="mr-2 h-4 w-4" />
                      New Response
                    </Button>
                  </DialogTrigger>
                  <DialogContent className="max-w-2xl">
                    <DialogHeader>
                      <DialogTitle>Create Canned Response</DialogTitle>
                      <DialogDescription>Use commas for shortcuts (e.g. /refund, /upload)</DialogDescription>
                    </DialogHeader>
                    <div className="space-y-4">
                      <div>
                        <label className="text-sm font-medium">Name</label>
                        <Input value={newCanned.name} onChange={(e) => setNewCanned({ ...newCanned, name: e.target.value })} />
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="text-sm font-medium">Category</label>
                          <Input value={newCanned.category} onChange={(e) => setNewCanned({ ...newCanned, category: e.target.value })} />
                        </div>
                        <div>
                          <label className="text-sm font-medium">Shortcuts</label>
                          <Input value={newCanned.shortcuts} onChange={(e) => setNewCanned({ ...newCanned, shortcuts: e.target.value })} placeholder="/refund, /upload" />
                        </div>
                      </div>
                      <div>
                        <label className="text-sm font-medium">Content</label>
                        <Textarea value={newCanned.content} onChange={(e) => setNewCanned({ ...newCanned, content: e.target.value })} rows={6} />
                      </div>
                    </div>
                    <DialogFooter>
                      <Button onClick={handleCreateCanned} disabled={!newCanned.name || !newCanned.content}>
                        Create Response
                      </Button>
                    </DialogFooter>
                  </DialogContent>
                </Dialog>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {cannedResponses.map((response) => (
                  <div key={response.id} className="p-4 border rounded-lg">
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          <h3 className="font-medium">{response.name}</h3>
                          <Badge variant="outline">{response.category}</Badge>
                          <span className="text-xs text-gray-500">
                            Used {response.usageCount} times
                          </span>
                        </div>
                        <p className="text-sm text-gray-600 line-clamp-2">{response.content}</p>
                        {response.shortcuts.length > 0 && (
                          <div className="flex gap-1 mt-2">
                            {response.shortcuts.map((shortcut) => (
                              <kbd key={shortcut} className="px-2 py-1 text-xs bg-gray-100 rounded">
                                {shortcut}
                              </kbd>
                            ))}
                          </div>
                        )}
                      </div>
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="sm">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem
                            onClick={() => {
                              setEditingCanned(response);
                              setShowEditCannedDialog(true);
                            }}
                          >
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => {
                              setDeletingCanned(response);
                              setShowDeleteCannedDialog(true);
                            }}
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  </div>
                  ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="departments" className="space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Departments</CardTitle>
                <Button onClick={() => setShowNewDepartmentDialog(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  New Department
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Slug</TableHead>
                    <TableHead>Default SLA</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {departments.map((d) => (
                    <TableRow key={d.id}>
                      <TableCell className="font-medium">{d.name}</TableCell>
                      <TableCell>{d.slug}</TableCell>
                      <TableCell>
                        {d.defaultSlaPolicyId
                          ? slaPolicies.find((s) => s.id === d.defaultSlaPolicyId)?.name ?? "—"
                          : "—"}
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{d.isActive ? "Active" : "Disabled"}</Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="sm">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => {
                                setEditingDepartment(d);
                                setShowEditDepartmentDialog(true);
                              }}
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={() => handleSetDepartmentStatus(d.id, !d.isActive)}>
                              <Archive className="mr-2 h-4 w-4" />
                              {d.isActive ? "Disable" : "Enable"}
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="sla-policies" className="space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>SLA Policies</CardTitle>
                <Button onClick={() => setShowNewSlaDialog(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  New SLA Policy
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>First Response (min)</TableHead>
                    <TableHead>Resolution (min)</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {slaPolicies.map((s) => (
                    <TableRow key={s.id}>
                      <TableCell className="font-medium">{s.name}</TableCell>
                      <TableCell>{s.firstResponseMinutes}</TableCell>
                      <TableCell>{s.resolutionMinutes}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{s.isActive ? "Active" : "Disabled"}</Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="sm">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => {
                                setEditingSla(s);
                                setShowEditSlaDialog(true);
                              }}
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={() => handleSetSlaStatus(s.id, !s.isActive)}>
                              <Archive className="mr-2 h-4 w-4" />
                              {s.isActive ? "Disable" : "Enable"}
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem onClick={() => handleDeleteSla(s.id)}>
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="escalation-rules" className="space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Escalation Rules</CardTitle>
                <Button onClick={() => setShowNewEscalationDialog(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  New Rule
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Trigger</TableHead>
                    <TableHead>Threshold (min)</TableHead>
                    <TableHead>Priority</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {escalationRules.map((r) => (
                    <TableRow key={r.id}>
                      <TableCell className="font-medium">{r.name}</TableCell>
                      <TableCell>{r.trigger}</TableCell>
                      <TableCell>{r.thresholdMinutes}</TableCell>
                      <TableCell>{r.escalateToPriority}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{r.isActive ? "Active" : "Disabled"}</Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="sm">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => {
                                setEditingEscalation(r);
                                setShowEditEscalationDialog(true);
                              }}
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={() => handleSetEscalationStatus(r.id, !r.isActive)}>
                              <Archive className="mr-2 h-4 w-4" />
                              {r.isActive ? "Disable" : "Enable"}
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem onClick={() => handleDeleteEscalation(r.id)}>
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <Dialog open={showNewDepartmentDialog} onOpenChange={setShowNewDepartmentDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>New Department</DialogTitle>
            <DialogDescription>Create a new support department.</DialogDescription>
          </DialogHeader>
          <div className="space-y-3">
            <div>
              <p className="text-sm font-medium mb-1">Name</p>
              <Input value={newDepartment.name} onChange={(e) => setNewDepartment((p) => ({ ...p, name: e.target.value }))} />
            </div>
            <div>
              <p className="text-sm font-medium mb-1">Slug (optional)</p>
              <Input value={newDepartment.slug} onChange={(e) => setNewDepartment((p) => ({ ...p, slug: e.target.value }))} />
            </div>
            <div>
              <p className="text-sm font-medium mb-1">Default SLA</p>
              <Select
                value={newDepartment.defaultSlaPolicyId || "none"}
                onValueChange={(v) => setNewDepartment((p) => ({ ...p, defaultSlaPolicyId: v === "none" ? "" : v }))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="None" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">None</SelectItem>
                  {slaPolicies.map((s) => (
                    <SelectItem key={s.id} value={s.id}>
                      {s.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowNewDepartmentDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleCreateDepartment}>Create</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={showEditDepartmentDialog} onOpenChange={setShowEditDepartmentDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Department</DialogTitle>
            <DialogDescription>Update department details.</DialogDescription>
          </DialogHeader>
          {editingDepartment && (
            <div className="space-y-3">
              <div>
                <p className="text-sm font-medium mb-1">Name</p>
                <Input value={editingDepartment.name} onChange={(e) => setEditingDepartment((p) => (p ? { ...p, name: e.target.value } : p))} />
              </div>
              <div>
                <p className="text-sm font-medium mb-1">Slug</p>
                <Input value={editingDepartment.slug} onChange={(e) => setEditingDepartment((p) => (p ? { ...p, slug: e.target.value } : p))} />
              </div>
              <div>
                <p className="text-sm font-medium mb-1">Default SLA</p>
                <Select
                  value={editingDepartment.defaultSlaPolicyId ?? "none"}
                  onValueChange={(v) => setEditingDepartment((p) => (p ? { ...p, defaultSlaPolicyId: v === "none" ? null : v } : p))}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="None" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None</SelectItem>
                    {slaPolicies.map((s) => (
                      <SelectItem key={s.id} value={s.id}>
                        {s.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowEditDepartmentDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleUpdateDepartment}>Save</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={showNewSlaDialog} onOpenChange={setShowNewSlaDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>New SLA Policy</DialogTitle>
            <DialogDescription>Create a new SLA policy.</DialogDescription>
          </DialogHeader>
          <div className="space-y-3">
            <div>
              <p className="text-sm font-medium mb-1">Name</p>
              <Input value={newSla.name} onChange={(e) => setNewSla((p) => ({ ...p, name: e.target.value }))} />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <p className="text-sm font-medium mb-1">First Response (min)</p>
                <Input value={newSla.firstResponseMinutes} onChange={(e) => setNewSla((p) => ({ ...p, firstResponseMinutes: e.target.value }))} />
              </div>
              <div>
                <p className="text-sm font-medium mb-1">Resolution (min)</p>
                <Input value={newSla.resolutionMinutes} onChange={(e) => setNewSla((p) => ({ ...p, resolutionMinutes: e.target.value }))} />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowNewSlaDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleCreateSla}>Create</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={showEditSlaDialog} onOpenChange={setShowEditSlaDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit SLA Policy</DialogTitle>
            <DialogDescription>Update SLA policy details.</DialogDescription>
          </DialogHeader>
          {editingSla && (
            <div className="space-y-3">
              <div>
                <p className="text-sm font-medium mb-1">Name</p>
                <Input value={editingSla.name} onChange={(e) => setEditingSla((p) => (p ? { ...p, name: e.target.value } : p))} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <p className="text-sm font-medium mb-1">First Response (min)</p>
                  <Input
                    value={String(editingSla.firstResponseMinutes)}
                    onChange={(e) => setEditingSla((p) => (p ? { ...p, firstResponseMinutes: Number(e.target.value) } : p))}
                  />
                </div>
                <div>
                  <p className="text-sm font-medium mb-1">Resolution (min)</p>
                  <Input
                    value={String(editingSla.resolutionMinutes)}
                    onChange={(e) => setEditingSla((p) => (p ? { ...p, resolutionMinutes: Number(e.target.value) } : p))}
                  />
                </div>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowEditSlaDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleUpdateSla}>Save</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={showNewEscalationDialog} onOpenChange={setShowNewEscalationDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>New Escalation Rule</DialogTitle>
            <DialogDescription>Create a new escalation rule.</DialogDescription>
          </DialogHeader>
          <div className="space-y-3">
            <div>
              <p className="text-sm font-medium mb-1">Name</p>
              <Input value={newEscalation.name} onChange={(e) => setNewEscalation((p) => ({ ...p, name: e.target.value }))} />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <p className="text-sm font-medium mb-1">Trigger</p>
                <Select value={newEscalation.trigger} onValueChange={(v) => setNewEscalation((p) => ({ ...p, trigger: v }))}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="FirstResponseOverdue">FirstResponseOverdue</SelectItem>
                    <SelectItem value="ResolutionOverdue">ResolutionOverdue</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <p className="text-sm font-medium mb-1">Threshold (min)</p>
                <Input
                  value={newEscalation.thresholdMinutes}
                  onChange={(e) => setNewEscalation((p) => ({ ...p, thresholdMinutes: e.target.value }))}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <p className="text-sm font-medium mb-1">Escalate To Priority</p>
                <Select
                  value={newEscalation.escalateToPriority}
                  onValueChange={(v) => setNewEscalation((p) => ({ ...p, escalateToPriority: v }))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Low">Low</SelectItem>
                    <SelectItem value="Normal">Normal</SelectItem>
                    <SelectItem value="High">High</SelectItem>
                    <SelectItem value="Critical">Critical</SelectItem>
                    <SelectItem value="Urgent">Urgent</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <p className="text-sm font-medium mb-1">Set Status To Escalated</p>
                <Select
                  value={newEscalation.setStatusToEscalated ? "yes" : "no"}
                  onValueChange={(v) => setNewEscalation((p) => ({ ...p, setStatusToEscalated: v === "yes" }))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="yes">Yes</SelectItem>
                    <SelectItem value="no">No</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowNewEscalationDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleCreateEscalation}>Create</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={showEditEscalationDialog} onOpenChange={setShowEditEscalationDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Escalation Rule</DialogTitle>
            <DialogDescription>Update escalation rule details.</DialogDescription>
          </DialogHeader>
          {editingEscalation && (
            <div className="space-y-3">
              <div>
                <p className="text-sm font-medium mb-1">Name</p>
                <Input value={editingEscalation.name} onChange={(e) => setEditingEscalation((p) => (p ? { ...p, name: e.target.value } : p))} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <p className="text-sm font-medium mb-1">Trigger</p>
                  <Select
                    value={editingEscalation.trigger}
                    onValueChange={(v) => setEditingEscalation((p) => (p ? { ...p, trigger: v } : p))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="FirstResponseOverdue">FirstResponseOverdue</SelectItem>
                      <SelectItem value="ResolutionOverdue">ResolutionOverdue</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <p className="text-sm font-medium mb-1">Threshold (min)</p>
                  <Input
                    value={String(editingEscalation.thresholdMinutes)}
                    onChange={(e) => setEditingEscalation((p) => (p ? { ...p, thresholdMinutes: Number(e.target.value) } : p))}
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <p className="text-sm font-medium mb-1">Escalate To Priority</p>
                  <Select
                    value={editingEscalation.escalateToPriority}
                    onValueChange={(v) => setEditingEscalation((p) => (p ? { ...p, escalateToPriority: v } : p))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Low">Low</SelectItem>
                      <SelectItem value="Normal">Normal</SelectItem>
                      <SelectItem value="High">High</SelectItem>
                      <SelectItem value="Critical">Critical</SelectItem>
                      <SelectItem value="Urgent">Urgent</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <p className="text-sm font-medium mb-1">Set Status To Escalated</p>
                  <Select
                    value={editingEscalation.setStatusToEscalated ? "yes" : "no"}
                    onValueChange={(v) => setEditingEscalation((p) => (p ? { ...p, setStatusToEscalated: v === "yes" } : p))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="yes">Yes</SelectItem>
                      <SelectItem value="no">No</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowEditEscalationDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleUpdateEscalation}>Save</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={showEditCannedDialog} onOpenChange={setShowEditCannedDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Edit Canned Response</DialogTitle>
            <DialogDescription>Update the template and shortcuts</DialogDescription>
          </DialogHeader>

          {editingCanned ? (
            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium">Name</label>
                <Input value={editingCanned.name} onChange={(e) => setEditingCanned({ ...editingCanned, name: e.target.value })} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium">Category</label>
                  <Input value={editingCanned.category} onChange={(e) => setEditingCanned({ ...editingCanned, category: e.target.value })} />
                </div>
                <div>
                  <label className="text-sm font-medium">Shortcuts (comma-separated)</label>
                  <Input
                    value={editingCanned.shortcuts.join(", ")}
                    onChange={(e) =>
                      setEditingCanned({
                        ...editingCanned,
                        shortcuts: e.target.value
                          .split(",")
                          .map((s) => s.trim())
                          .filter(Boolean),
                      })
                    }
                  />
                </div>
              </div>
              <div>
                <label className="text-sm font-medium">Content</label>
                <Textarea value={editingCanned.content} onChange={(e) => setEditingCanned({ ...editingCanned, content: e.target.value })} rows={6} />
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="cannedActive"
                  checked={editingCanned.isActive}
                  onChange={(e) => setEditingCanned({ ...editingCanned, isActive: e.target.checked })}
                  className="rounded"
                />
                <label htmlFor="cannedActive" className="text-sm">
                  Active
                </label>
              </div>
            </div>
          ) : null}

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowEditCannedDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleUpdateCanned} disabled={!editingCanned?.name || !editingCanned?.content}>
              Save
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <AlertDialog open={showDeleteCannedDialog} onOpenChange={setShowDeleteCannedDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete canned response?</AlertDialogTitle>
            <AlertDialogDescription>
              {deletingCanned ? `This will permanently delete "${deletingCanned.name}".` : "This action cannot be undone."}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteCanned}>Delete</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Dialog open={showNewTicketDialog} onOpenChange={setShowNewTicketDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Create Support Ticket</DialogTitle>
            <DialogDescription>Create a new support ticket</DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">Department</label>
              <Select
                value={newTicket.departmentId}
                onValueChange={(value) => setNewTicket({ ...newTicket, departmentId: value })}
              >
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
              <label className="text-sm font-medium">Subject</label>
              <Input
                value={newTicket.subject}
                onChange={(e) => setNewTicket({ ...newTicket, subject: e.target.value })}
                placeholder="Brief summary of the issue"
              />
            </div>

            <div>
              <label className="text-sm font-medium">Description</label>
              <Textarea
                value={newTicket.description}
                onChange={(e) => setNewTicket({ ...newTicket, description: e.target.value })}
                rows={6}
                placeholder="Describe the issue in detail…"
              />
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
                  <SelectItem value="Critical">Critical</SelectItem>
                  <SelectItem value="Urgent">Urgent</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowNewTicketDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleCreateTicket} disabled={!newTicket.subject || !newTicket.description || !newTicket.departmentId}>
              Create Ticket
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
