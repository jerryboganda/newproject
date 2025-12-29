"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
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
  Archive,
  Flag,
  UserCheck,
  Zap,
  BookOpen,
  FileText,
  MessageSquare as MessageIcon,
} from "lucide-react";
import { formatDistanceToNow } from "date-fns";

interface SupportTicket {
  id: string;
  ticketNumber: string;
  subject: string;
  description: string;
  category: string;
  priority: string;
  status: string;
  tenantId: string;
  tenantName: string;
  userId: string;
  userName: string;
  userEmail: string;
  assignedToId?: string;
  assignedToName?: string;
  createdAt: string;
  updatedAt: string;
  lastReplyAt?: string;
  repliesCount: number;
  isInternal?: boolean;
}

interface KnowledgeBaseArticle {
  id: string;
  title: string;
  summary: string;
  category: string;
  views: number;
  helpfulVotes: number;
  tags: string[];
  createdAt: string;
  updatedAt: string;
}

interface CannedResponse {
  id: string;
  name: string;
  content: string;
  category: string;
  shortcuts: string[];
  usageCount: number;
}

export default function SupportCenter() {
  const [tickets, setTickets] = useState<SupportTicket[]>([]);
  const [selectedTicket, setSelectedTicket] = useState<SupportTicket | null>(null);
  const [kbArticles, setKbArticles] = useState<KnowledgeBaseArticle[]>([]);
  const [cannedResponses, setCannedResponses] = useState<CannedResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("tickets");
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [priorityFilter, setPriorityFilter] = useState<string>("all");
  const [showNewTicketDialog, setShowNewTicketDialog] = useState(false);
  const [replyContent, setReplyContent] = useState("");
  const [isInternalReply, setIsInternalReply] = useState(false);

  // New ticket form state
  const [newTicket, setNewTicket] = useState({
    tenantId: "",
    userId: "",
    subject: "",
    description: "",
    category: "general",
    priority: "normal",
  });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      // Fetch tickets
      const ticketsResponse = await fetch("/api/support/tickets");
      const ticketsData = await ticketsResponse.json();
      setTickets(ticketsData);

      // Fetch knowledge base articles
      const kbResponse = await fetch("/api/support/knowledge-base");
      const kbData = await kbResponse.json();
      setKbArticles(kbData);

      // Fetch canned responses
      const responsesResponse = await fetch("/api/support/canned-responses");
      const responsesData = await responsesResponse.json();
      setCannedResponses(responsesData);
    } catch (error) {
      console.error("Failed to load data:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTicket = async () => {
    try {
      await fetch("/api/support/tickets", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newTicket),
      });
      setShowNewTicketDialog(false);
      setNewTicket({
        tenantId: "",
        userId: "",
        subject: "",
        description: "",
        category: "general",
        priority: "normal",
      });
      await loadData();
    } catch (error) {
      console.error("Failed to create ticket:", error);
    }
  };

  const handleReplyToTicket = async () => {
    if (!selectedTicket || !replyContent) return;

    try {
      await fetch(`/api/support/tickets/${selectedTicket.id}/reply`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          content: replyContent,
          isInternal: isInternalReply,
        }),
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
      const response = await fetch(`/api/support/tickets/${ticketId}`);
      const ticket = await response.json();
      setSelectedTicket(ticket);
    } catch (error) {
      console.error("Failed to load ticket details:", error);
    }
  };

  const handleUpdateTicketStatus = async (ticketId: string, status: string) => {
    try {
      await fetch(`/api/support/tickets/${ticketId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status }),
      });
      await loadData();
    } catch (error) {
      console.error("Failed to update ticket:", error);
    }
  };

  const handleAssignTicket = async (ticketId: string, userId: string) => {
    try {
      await fetch(`/api/support/tickets/${ticketId}/assign`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userId }),
      });
      await loadTicketDetails(ticketId);
    } catch (error) {
      console.error("Failed to assign ticket:", error);
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case "urgent":
        return "bg-red-100 text-red-800";
      case "high":
        return "bg-orange-100 text-orange-800";
      case "normal":
        return "bg-blue-100 text-blue-800";
      case "low":
        return "bg-gray-100 text-gray-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
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
    
    const matchesStatus = statusFilter === "all" || ticket.status === statusFilter;
    const matchesPriority = priorityFilter === "all" || ticket.priority === priorityFilter;
    
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
          <Button variant="outline" asChild>
            <Link href="/admin/support/knowledge-base">
              <BookOpen className="mr-2 h-4 w-4" />
              Knowledge Base
            </Link>
          </Button>
          <Button variant="outline" asChild>
            <Link href="/admin/support/canned-responses">
              <FileText className="mr-2 h-4 w-4" />
              Canned Responses
            </Link>
          </Button>
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
                                {ticket.status.replace("_", " ")}
                              </Badge>
                            </div>
                            <h3 className="font-medium truncate">{ticket.subject}</h3>
                            <p className="text-sm text-gray-600 truncate">{ticket.description}</p>
                            <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
                              <span>{ticket.userName}</span>
                              <span>{ticket.tenantName}</span>
                              <span>{formatDistanceToNow(new Date(ticket.createdAt), { addSuffix: true })}</span>
                              {ticket.repliesCount > 0 && (
                                <span className="flex items-center gap-1">
                                  <MessageIcon className="h-3 w-3" />
                                  {ticket.repliesCount}
                                </span>
                              )}
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
                          <DropdownMenuItem onClick={() => handleUpdateTicketStatus(selectedTicket.id, "in_progress")}>
                            <Clock className="mr-2 h-4 w-4" />
                            Mark as In Progress
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleUpdateTicketStatus(selectedTicket.id, "resolved")}>
                            <CheckCircle className="mr-2 h-4 w-4" />
                            Mark as Resolved
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => handleUpdateTicketStatus(selectedTicket.id, "closed")}>
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
                          <span className="font-medium">Category:</span> {selectedTicket.category}
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
              <CardTitle>Knowledge Base Articles</CardTitle>
              <CardDescription>Manage help documentation and FAQs</CardDescription>
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
                          <span>{article.category}</span>
                          <div className="flex gap-1">
                            {article.tags.map((tag) => (
                              <Badge key={tag} variant="outline" className="text-xs">
                                {tag}
                              </Badge>
                            ))}
                          </div>
                        </div>
                      </div>
                      <Button variant="ghost" size="sm">
                        <ExternalLink className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="canned-responses" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Canned Responses</CardTitle>
              <CardDescription>Quick response templates for common issues</CardDescription>
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
                          <DropdownMenuItem>
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuItem>
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
      </Tabs>

      {/* New Ticket Dialog */}
      <Dialog open={showNewTicketDialog} onOpenChange={setShowNewTicketDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Create New Ticket</DialogTitle>
            <DialogDescription>
              Create a new support ticket for a customer
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium">Tenant</label>
                <Select value={newTicket.tenantId} onValueChange={(value) => setNewTicket({ ...newTicket, tenantId: value })}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select tenant" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="tenant1">Tenant 1</SelectItem>
                    <SelectItem value="tenant2">Tenant 2</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <label className="text-sm font-medium">User</label>
                <Select value={newTicket.userId} onValueChange={(value) => setNewTicket({ ...newTicket, userId: value })}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select user" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="user1">User 1</SelectItem>
                    <SelectItem value="user2">User 2</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div>
              <label className="text-sm font-medium">Subject</label>
              <Input
                value={newTicket.subject}
                onChange={(e) => setNewTicket({ ...newTicket, subject: e.target.value })}
                placeholder="Brief description of the issue"
              />
            </div>
            <div>
              <label className="text-sm font-medium">Description</label>
              <Textarea
                value={newTicket.description}
                onChange={(e) => setNewTicket({ ...newTicket, description: e.target.value })}
                placeholder="Detailed description of the issue"
                rows={4}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium">Category</label>
                <Select value={newTicket.category} onValueChange={(value) => setNewTicket({ ...newTicket, category: value })}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="general">General</SelectItem>
                    <SelectItem value="technical">Technical</SelectItem>
                    <SelectItem value="billing">Billing</SelectItem>
                    <SelectItem value="account">Account</SelectItem>
                    <SelectItem value="feature_request">Feature Request</SelectItem>
                    <SelectItem value="bug_report">Bug Report</SelectItem>
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
                    <SelectItem value="low">Low</SelectItem>
                    <SelectItem value="normal">Normal</SelectItem>
                    <SelectItem value="high">High</SelectItem>
                    <SelectItem value="urgent">Urgent</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowNewTicketDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleCreateTicket}>Create Ticket</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
