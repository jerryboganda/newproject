export type StreamVaultVideo = {
  id: string
  title: string
  description?: string | null
  thumbnailUrl?: string | null
  videoUrl: string
  createdAt?: string
  viewCount: number
  isPublic?: boolean
  status?: string
}

export type StreamVaultUserProfile = {
  id: string
  email: string
  username?: string
  firstName?: string
  lastName?: string
  avatar?: string
  role?: string
  tenantId?: string | null
  isSuperAdmin?: boolean
  twoFactorEnabled?: boolean
}

export type StreamVaultSubscriptionPlan = {
  id: string
  name: string
  slug: string
  description?: string | null
  priceMonthly: number
  priceYearly: number
  stripePriceIdMonthly?: string | null
  stripePriceIdYearly?: string | null
  isCustom: boolean
}

export type StreamVaultTenantSubscription = {
  id: string
  planId: string
  planName: string
  status: string
  billingCycle: string
  currentPeriodStart?: string | null
  currentPeriodEnd?: string | null
  cancelAt?: string | null
  trialEnd?: string | null
}

export type StreamVaultTenantUsageSnapshot = {
  periodStartUtc: string
  storageBytes: number
  bandwidthBytes: number
  videoCount: number
}

export type StreamVaultStripeInvoice = {
  id: string
  status: string
  amountDue: number
  currency: string
  hostedInvoiceUrl?: string | null
  invoicePdf?: string | null
  createdAtUtc: string
  dueDateUtc?: string | null
}

export type StreamVaultManualPayment = {
  id: string
  amount: number
  currency: string
  paidAtUtc: string
  reference?: string | null
  notes?: string | null
}

export type StreamVaultUsageMultiplier = {
  id: string
  name: string
  metricType: string
  multiplier: number
  isActive: boolean
}

export type StreamVaultTenantUsageMultiplierOverride = {
  id: string
  metricType: string
  multiplier: number
  isActive: boolean
}

export type StreamVaultBillingSummary = {
  tenantId: string
  stripeCustomerId?: string | null
  stripeSubscriptionId?: string | null
  currentSubscription?: StreamVaultTenantSubscription | null
  latestUsage?: StreamVaultTenantUsageSnapshot | null
  stripeInvoices: StreamVaultStripeInvoice[]
  manualPayments: StreamVaultManualPayment[]
  globalUsageMultipliers: StreamVaultUsageMultiplier[]
  tenantUsageMultiplierOverrides: StreamVaultTenantUsageMultiplierOverride[]
}

export type StreamVaultAnalyticsTimePoint = {
  dateUtc: string
  views: number
  uniqueViewers: number
  watchTimeSeconds: number
  completes: number
}

export type StreamVaultAnalyticsTopVideoPoint = {
  videoId: string
  title: string
  views: number
}

export type StreamVaultAnalyticsCountryPoint = {
  countryCode: string
  views: number
}

export type StreamVaultAnalyticsOverviewResponse = {
  startUtc: string
  endUtc: string
  totalViews: number
  uniqueViewers: number
  totalWatchTimeSeconds: number
  averageWatchTimeSeconds: number
  completes: number
  viewsByDay: StreamVaultAnalyticsTimePoint[]
  topVideos: StreamVaultAnalyticsTopVideoPoint[]
  countries: StreamVaultAnalyticsCountryPoint[]
}

export type StreamVaultVideoSeriesPoint = {
  bucketStartUtc: string
  views: number
  uniqueViewers: number
  watchTimeSeconds: number
  completes: number
}

export type StreamVaultSupportDepartment = {
  id: string
  name: string
  slug: string
  isActive: boolean
  defaultSlaPolicyId?: string | null
}

export type StreamVaultSupportSlaPolicy = {
  id: string
  name: string
  firstResponseMinutes: number
  resolutionMinutes: number
  isActive: boolean
}

export type StreamVaultSupportEscalationRule = {
  id: string
  name: string
  trigger: string
  thresholdMinutes: number
  escalateToPriority: string
  setStatusToEscalated: boolean
  isActive: boolean
}

export type StreamVaultSupportAgent = {
  id: string
  name: string
  email: string
}

export type StreamVaultSupportTicketListItem = {
  id: string
  ticketNumber: string
  subject: string
  description: string
  departmentId: string
  departmentName: string
  priority: string
  status: string
  userId: string
  userName: string
  userEmail: string
  assignedToId?: string | null
  assignedToName?: string | null
  createdAt: string
  updatedAt: string
  firstResponseAt?: string | null
  firstResponseDueAt?: string | null
  resolutionDueAt?: string | null
}

export type StreamVaultSupportTicketReply = {
  id: string
  content: string
  isInternal: boolean
  userId: string
  userName: string
  userEmail: string
  createdAt: string
}

export type StreamVaultSupportTicketActivity = {
  id: string
  type: string
  message: string
  metadataJson?: string | null
  createdByUserId?: string | null
  createdAt: string
}

export type StreamVaultSupportTicketDetails = StreamVaultSupportTicketListItem & {
  replies: StreamVaultSupportTicketReply[]
  activities: StreamVaultSupportTicketActivity[]
}

export type StreamVaultCannedResponse = {
  id: string
  name: string
  content: string
  category: string
  shortcuts: string[]
  isActive: boolean
  usageCount: number
  createdAt: string
  updatedAt: string
}

export type StreamVaultKbCategory = {
  id: string
  name: string
  slug: string
  description?: string | null
  sortOrder: number
  isActive: boolean
}

export type StreamVaultKbArticleListItem = {
  id: string
  title: string
  slug: string
  summary?: string | null
  categoryId: string
  categoryName: string
  tags: string[]
  isPublished: boolean
  views: number
  helpfulVotes: number
  createdAt: string
  updatedAt: string
}

export type StreamVaultKbArticleDetails = StreamVaultKbArticleListItem & {
  content: string
  publishedAt?: string | null
}

export type StreamVaultAdminSystemSettings = {
  id: string
  allowNewRegistrations: boolean
  requireEmailVerification: boolean
  defaultSubscriptionPlanId?: string | null
  maxFileSizeMB: number
  supportedVideoFormats: string[]
  maintenanceMode: boolean
  maintenanceMessage?: string | null
  createdAt: string
  updatedAt: string
}

export type StreamVaultAdminUpdateSystemSettingsRequest = {
  allowNewRegistrations: boolean
  requireEmailVerification: boolean
  defaultSubscriptionPlanId?: string | null
  maxFileSizeMB: number
  supportedVideoFormats: string[]
  maintenanceMode: boolean
  maintenanceMessage?: string | null
}

export type StreamVaultAdminEmailTemplate = {
  id: string
  name: string
  subject: string
  htmlContent: string
  textContent: string
  category: string
  variables: string[]
  isActive: boolean
  createdAt: string
  updatedAt: string
  createdByUserId: string
}

export type StreamVaultAdminUpsertEmailTemplateRequest = {
  name?: string
  subject?: string
  htmlContent?: string
  textContent?: string
  category?: string
  variables?: string[]
  isActive: boolean
}

export type StreamVaultAdminAuditLogListItem = {
  id: string
  tenantId: string
  userId: string
  action: string
  entityType: string
  entityId?: string | null
  ipAddress?: string | null
  createdAt: string
}

export type StreamVaultAdminAuditLogListResponse = {
  items: StreamVaultAdminAuditLogListItem[]
  total: number
  page: number
  pageSize: number
}

export type StreamVaultAdminTenantListItem = {
  id: string
  name: string
  slug: string
  logoUrl?: string | null
  isActive: boolean
  isSuspended: boolean
  suspensionReason?: string | null
  createdAt: string
  subscription?: {
    plan: string
    status: string
    billingCycle: string
    currentPeriodStart?: string | null
    currentPeriodEnd?: string | null
    cancelAt?: string | null
    trialEnd?: string | null
  } | null
  stats: {
    users: number
    videos: number
    views: number
    storageUsedBytes: number
  }
}

export type StreamVaultAdminPlatformAnalyticsSeriesPoint = {
  name: string
  tenants: number
  users: number
  revenue: number
}

export type StreamVaultAdminPlatformAnalyticsResponse = {
  totalTenants: number
  activeTenants: number
  totalUsers: number
  activeUsers: number
  totalVideos: number
  totalViews: number
  totalStorageUsed: number
  revenue: number
  newTenantsThisMonth: number
  newUsersThisMonth: number
  churnRate: number
  series: StreamVaultAdminPlatformAnalyticsSeriesPoint[]
}

export type StreamVaultAdminImpersonationResponse = {
  token: string
  tenantSlug: string
  targetUrl: string
}

export const STREAMVAULT_API_BASE_URL = (process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api/v1").replace(
  /\/$/,
  "",
)

async function requestJson<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${STREAMVAULT_API_BASE_URL}${path}`
  
  // Get token from localStorage if available
  let headers: Record<string, string> = {
    Accept: "application/json",
    "Content-Type": "application/json",
    ...(init?.headers as Record<string, string> ?? {}),
  }
  
  // Add authorization header if token exists
  if (typeof window !== "undefined") {
    const token = localStorage.getItem("auth-token")
    if (token) {
      headers = {
        ...headers,
        Authorization: `Bearer ${token}`,
      }
    }
  }
  
  const res = await fetch(url, {
    ...init,
    headers,
    cache: "no-store",
  })

  if (!res.ok) {
    const text = await res.text().catch(() => "")
    throw new Error(`StreamVault API ${res.status} ${res.statusText} for ${path}${text ? `: ${text}` : ""}`)
  }

  return (await res.json()) as T
}

export const streamvaultApi = {
  auth: {
    login: (email: string, password: string) =>
      requestJson<{ accessToken: string; refreshToken?: string; user: { id: string; email: string } }>("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      }),
  },
  user: {
    profile: () => requestJson<StreamVaultUserProfile>("/user/profile"),
  },
  billing: {
    plans: () => requestJson<StreamVaultSubscriptionPlan[]>("/billing/plans"),
    summary: () => requestJson<StreamVaultBillingSummary>("/billing/summary"),
    createCheckoutSession: (args: {
      planId: string
      interval?: "monthly" | "yearly"
      successUrl: string
      cancelUrl: string
    }) =>
      requestJson<{ sessionId: string; url?: string | null }>("/billing/checkout-session", {
        method: "POST",
        body: JSON.stringify({
          planId: args.planId,
          interval: args.interval,
          successUrl: args.successUrl,
          cancelUrl: args.cancelUrl,
        }),
      }),
    createPortalSession: (args: { returnUrl: string }) =>
      requestJson<{ sessionId: string; url?: string | null }>("/billing/portal-session", {
        method: "POST",
        body: JSON.stringify({ returnUrl: args.returnUrl }),
      }),
    listManualPayments: () => requestJson<StreamVaultManualPayment[]>("/billing/manual-payments"),
    createManualPayment: (args: {
      amount: number
      currency?: string
      paidAtUtc?: string
      reference?: string
      notes?: string
    }) =>
      requestJson<StreamVaultManualPayment>("/billing/manual-payments", {
        method: "POST",
        body: JSON.stringify({
          amount: args.amount,
          currency: args.currency,
          paidAtUtc: args.paidAtUtc,
          reference: args.reference,
          notes: args.notes,
        }),
      }),
    upsertTenantMultiplierOverride: (args: { metricType: string; multiplier: number; isActive: boolean }) =>
      requestJson<StreamVaultTenantUsageMultiplierOverride>("/billing/usage-multipliers/override", {
        method: "PUT",
        body: JSON.stringify({
          metricType: args.metricType,
          multiplier: args.multiplier,
          isActive: args.isActive,
        }),
      }),
  },
  analytics: {
    overview: (args?: { startUtc?: string; endUtc?: string; top?: number }) => {
      const qs = new URLSearchParams()
      if (args?.startUtc) qs.set("startUtc", args.startUtc)
      if (args?.endUtc) qs.set("endUtc", args.endUtc)
      if (args?.top != null) qs.set("top", String(args.top))

      const suffix = qs.toString() ? `?${qs.toString()}` : ""
      return requestJson<StreamVaultAnalyticsOverviewResponse>(`/analytics/overview${suffix}`)
    },
    videoTimeseries: (videoId: string, args?: { bucket?: "day" | "hour"; startUtc?: string; endUtc?: string }) => {
      const qs = new URLSearchParams()
      if (args?.bucket) qs.set("bucket", args.bucket)
      if (args?.startUtc) qs.set("startUtc", args.startUtc)
      if (args?.endUtc) qs.set("endUtc", args.endUtc)
      const suffix = qs.toString() ? `?${qs.toString()}` : ""
      return requestJson<StreamVaultVideoSeriesPoint[]>(`/analytics/videos/${videoId}/timeseries${suffix}`)
    },
  },
  admin: {
    tenants: {
      list: (args?: { page?: number; pageSize?: number; search?: string }) => {
        const qs = new URLSearchParams()
        if (args?.page != null) qs.set("page", String(args.page))
        if (args?.pageSize != null) qs.set("pageSize", String(args.pageSize))
        if (args?.search) qs.set("search", args.search)
        const suffix = qs.toString() ? `?${qs.toString()}` : ""
        return requestJson<StreamVaultAdminTenantListItem[]>(`/admin/tenants${suffix}`)
      },
      suspend: (tenantId: string, reason: string) =>
        requestJson<void>(`/admin/tenants/${tenantId}/suspend`, {
          method: "POST",
          body: JSON.stringify({ reason }),
        }),
      unsuspend: (tenantId: string) =>
        requestJson<void>(`/admin/tenants/${tenantId}/unsuspend`, {
          method: "POST",
        }),
    },
    analytics: {
      get: () => requestJson<StreamVaultAdminPlatformAnalyticsResponse>("/admin/analytics"),
    },
    impersonate: (tenantId: string) =>
      requestJson<StreamVaultAdminImpersonationResponse>("/admin/impersonate", {
        method: "POST",
        body: JSON.stringify({ tenantId }),
      }),

    systemSettings: {
      get: () => requestJson<StreamVaultAdminSystemSettings>("/admin/system-settings"),
      update: (args: StreamVaultAdminUpdateSystemSettingsRequest) =>
        requestJson<StreamVaultAdminSystemSettings>("/admin/system-settings", {
          method: "PUT",
          body: JSON.stringify(args),
        }),
    },

    emailTemplates: {
      list: (args?: { page?: number; pageSize?: number; category?: string; search?: string }) => {
        const qs = new URLSearchParams()
        if (args?.page != null) qs.set("page", String(args.page))
        if (args?.pageSize != null) qs.set("pageSize", String(args.pageSize))
        if (args?.category) qs.set("category", args.category)
        if (args?.search) qs.set("search", args.search)
        const suffix = qs.toString() ? `?${qs.toString()}` : ""
        return requestJson<StreamVaultAdminEmailTemplate[]>(`/admin/email-templates${suffix}`)
      },
      get: (id: string) => requestJson<StreamVaultAdminEmailTemplate>(`/admin/email-templates/${id}`),
      create: (args: StreamVaultAdminUpsertEmailTemplateRequest) =>
        requestJson<StreamVaultAdminEmailTemplate>("/admin/email-templates", {
          method: "POST",
          body: JSON.stringify(args),
        }),
      update: (id: string, args: StreamVaultAdminUpsertEmailTemplateRequest) =>
        requestJson<StreamVaultAdminEmailTemplate>(`/admin/email-templates/${id}`, {
          method: "PUT",
          body: JSON.stringify(args),
        }),
      delete: (id: string) =>
        requestJson<void>(`/admin/email-templates/${id}`, {
          method: "DELETE",
        }),
    },

    auditLogs: {
      list: (args?: { page?: number; pageSize?: number; tenantId?: string; userId?: string; action?: string }) => {
        const qs = new URLSearchParams()
        if (args?.page != null) qs.set("page", String(args.page))
        if (args?.pageSize != null) qs.set("pageSize", String(args.pageSize))
        if (args?.tenantId) qs.set("tenantId", args.tenantId)
        if (args?.userId) qs.set("userId", args.userId)
        if (args?.action) qs.set("action", args.action)
        const suffix = qs.toString() ? `?${qs.toString()}` : ""
        return requestJson<StreamVaultAdminAuditLogListResponse>(`/admin/audit-logs${suffix}`)
      },
    },
  },
  videos: {
    list: async () => {
      const res = await requestJson<{
        items: Array<{
          id: string
          title: string
          description?: string | null
          viewCount: number
          isPublic: boolean
          status: string
          createdAt: string
          thumbnailUrl?: string | null
          watchUrl?: string | null
        }>
      }>("/videos?page=1&pageSize=50")

      return res.items.map((v) => ({
        id: v.id,
        title: v.title,
        description: v.description,
        thumbnailUrl: v.thumbnailUrl,
        videoUrl: v.watchUrl ?? "#",
        createdAt: v.createdAt,
        viewCount: v.viewCount,
        isPublic: v.isPublic,
        status: v.status,
      })) satisfies StreamVaultVideo[]
    },
    get: async (id: string) => {
      const v = await requestJson<{
        id: string
        title: string
        description?: string | null
        viewCount: number
        isPublic: boolean
        status: string
        createdAt: string
        publishedAt?: string | null
        thumbnailUrl?: string | null
        watchUrl: string
      }>(`/videos/${id}`)

      return {
        id: v.id,
        title: v.title,
        description: v.description,
        thumbnailUrl: v.thumbnailUrl,
        videoUrl: v.watchUrl,
        createdAt: v.createdAt,
        viewCount: v.viewCount,
        isPublic: v.isPublic,
        status: v.status,
      } satisfies StreamVaultVideo
    },
  },
  support: {
    departments: {
      list: () => requestJson<StreamVaultSupportDepartment[]>("/support/departments"),
      create: (args: { name: string; slug?: string; defaultSlaPolicyId?: string | null }) =>
        requestJson<StreamVaultSupportDepartment>("/support/departments", {
          method: "POST",
          body: JSON.stringify(args),
        }),
      update: (id: string, args: { name: string; slug?: string; defaultSlaPolicyId?: string | null }) =>
        requestJson<StreamVaultSupportDepartment>(`/support/departments/${id}`, {
          method: "PUT",
          body: JSON.stringify(args),
        }),
      setStatus: (id: string, isActive: boolean) =>
        requestJson<{ success: true }>(`/support/departments/${id}/status`, {
          method: "PUT",
          body: JSON.stringify({ isActive }),
        }),
    },
    tickets: {
      list: (args?: { status?: string; priority?: string; q?: string }) => {
        const params = new URLSearchParams()
        if (args?.status) params.set("status", args.status)
        if (args?.priority) params.set("priority", args.priority)
        if (args?.q) params.set("q", args.q)
        const suffix = params.toString() ? `?${params.toString()}` : ""
        return requestJson<StreamVaultSupportTicketListItem[]>(`/support/tickets${suffix}`)
      },
      get: (ticketId: string) => requestJson<StreamVaultSupportTicketDetails>(`/support/tickets/${ticketId}`),
      create: (args: { subject: string; description: string; departmentId: string; priority?: string }) =>
        requestJson<StreamVaultSupportTicketDetails>("/support/tickets", {
          method: "POST",
          body: JSON.stringify({
            subject: args.subject,
            description: args.description,
            departmentId: args.departmentId,
            priority: args.priority,
          }),
        }),
      addMessage: (ticketId: string, args: { content: string; isInternal: boolean }) =>
        requestJson<{ success: true }>(`/support/tickets/${ticketId}/messages`, {
          method: "POST",
          body: JSON.stringify(args),
        }),
      updateStatus: (ticketId: string, status: string) =>
        requestJson<{ success: true }>(`/support/tickets/${ticketId}/status`, {
          method: "PUT",
          body: JSON.stringify({ status }),
        }),
      assign: (ticketId: string, assignedToUserId: string | null) =>
        requestJson<{ success: true }>(`/support/tickets/${ticketId}/assign`, {
          method: "PUT",
          body: JSON.stringify({ assignedToUserId }),
        }),
    },
    agents: {
      list: () => requestJson<StreamVaultSupportAgent[]>("/support/agents"),
    },
    slaPolicies: {
      list: () => requestJson<StreamVaultSupportSlaPolicy[]>("/support/sla-policies"),
      create: (args: { name: string; firstResponseMinutes: number; resolutionMinutes: number }) =>
        requestJson<StreamVaultSupportSlaPolicy>("/support/sla-policies", {
          method: "POST",
          body: JSON.stringify(args),
        }),
      update: (id: string, args: { name: string; firstResponseMinutes: number; resolutionMinutes: number }) =>
        requestJson<StreamVaultSupportSlaPolicy>(`/support/sla-policies/${id}`, {
          method: "PUT",
          body: JSON.stringify(args),
        }),
      setStatus: (id: string, isActive: boolean) =>
        requestJson<{ success: true }>(`/support/sla-policies/${id}/status`, {
          method: "PUT",
          body: JSON.stringify({ isActive }),
        }),
      delete: (id: string) => requestJson<{ success: true }>(`/support/sla-policies/${id}`, { method: "DELETE" }),
    },
    escalationRules: {
      list: () => requestJson<StreamVaultSupportEscalationRule[]>("/support/escalation-rules"),
      create: (args: {
        name: string
        trigger: string
        thresholdMinutes: number
        escalateToPriority: string
        setStatusToEscalated: boolean
      }) =>
        requestJson<StreamVaultSupportEscalationRule>("/support/escalation-rules", {
          method: "POST",
          body: JSON.stringify(args),
        }),
      update: (id: string, args: {
        name: string
        trigger: string
        thresholdMinutes: number
        escalateToPriority: string
        setStatusToEscalated: boolean
      }) =>
        requestJson<StreamVaultSupportEscalationRule>(`/support/escalation-rules/${id}`, {
          method: "PUT",
          body: JSON.stringify(args),
        }),
      setStatus: (id: string, isActive: boolean) =>
        requestJson<{ success: true }>(`/support/escalation-rules/${id}/status`, {
          method: "PUT",
          body: JSON.stringify({ isActive }),
        }),
      delete: (id: string) => requestJson<{ success: true }>(`/support/escalation-rules/${id}`, { method: "DELETE" }),
    },
    cannedResponses: {
      list: () => requestJson<StreamVaultCannedResponse[]>("/support/canned-responses"),
      create: (args: { name: string; content: string; category?: string; shortcuts?: string[] }) =>
        requestJson<StreamVaultCannedResponse>("/support/canned-responses", {
          method: "POST",
          body: JSON.stringify(args),
        }),
      update: (id: string, args: { name?: string; content?: string; category?: string; shortcuts?: string[]; isActive?: boolean }) =>
        requestJson<{ success: true }>(`/support/canned-responses/${id}`, {
          method: "PUT",
          body: JSON.stringify(args),
        }),
      delete: (id: string) => requestJson<{ success: true }>(`/support/canned-responses/${id}`, { method: "DELETE" }),
    },
  },
  kb: {
    categories: {
      list: () => requestJson<StreamVaultKbCategory[]>("/kb/categories"),
      create: (args: { name: string; slug?: string; description?: string; sortOrder?: number }) =>
        requestJson<StreamVaultKbCategory>("/kb/categories", {
          method: "POST",
          body: JSON.stringify(args),
        }),
    },
    articles: {
      list: (args?: { categoryId?: string; q?: string }) => {
        const params = new URLSearchParams()
        if (args?.categoryId) params.set("categoryId", args.categoryId)
        if (args?.q) params.set("q", args.q)
        const suffix = params.toString() ? `?${params.toString()}` : ""
        return requestJson<StreamVaultKbArticleListItem[]>(`/kb/articles${suffix}`)
      },
      getBySlug: (slug: string) => requestJson<StreamVaultKbArticleDetails>(`/kb/articles/${slug}`),
      create: (args: {
        title: string
        slug?: string
        categoryId: string
        content: string
        summary?: string
        tags?: string[]
        isPublished: boolean
      }) =>
        requestJson<StreamVaultKbArticleDetails>("/kb/articles", {
          method: "POST",
          body: JSON.stringify(args),
        }),
      update: (id: string, args: { title?: string; categoryId?: string; content?: string; summary?: string; tags?: string[]; isPublished?: boolean }) =>
        requestJson<{ success: true }>(`/kb/articles/${id}`, {
          method: "PUT",
          body: JSON.stringify(args),
        }),
    },
  },
}
