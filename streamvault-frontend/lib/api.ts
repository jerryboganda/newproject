import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'

// API configuration
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api/v1'

// Create axios instance
const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor
api.interceptors.request.use(
  (config) => {
    // Get token from localStorage
    const token = localStorage.getItem('access_token')
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    // Add tenant ID if available
    const tenantId = localStorage.getItem('tenant_id')
    if (tenantId) {
      config.headers['X-Tenant-ID'] = tenantId
    }

    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor
api.interceptors.response.use(
  (response: AxiosResponse) => {
    return response
  },
  async (error) => {
    const originalRequest = error.config

    // Handle 401 Unauthorized
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        // Try to refresh the token
        const refreshToken = localStorage.getItem('refresh_token')
        
        if (refreshToken) {
          const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
            refreshToken,
          })

          const { accessToken } = response.data
          localStorage.setItem('access_token', accessToken)

          // Retry the original request
          originalRequest.headers.Authorization = `Bearer ${accessToken}`
          return api(originalRequest)
        }
      } catch (refreshError) {
        // Refresh failed, logout user
        localStorage.removeItem('access_token')
        localStorage.removeItem('refresh_token')
        localStorage.removeItem('tenant_id')
        window.location.href = '/login'
        return Promise.reject(refreshError)
      }
    }

    return Promise.reject(error)
  }
)

// API methods
export const apiClient = {
  // Auth endpoints
  auth: {
    login: (data: { email: string; password: string }) =>
      api.post('/api/v1/auth/login', data),
    
    register: (data: { email: string; password: string; firstName: string; lastName: string }) =>
      api.post('/api/v1/auth/register', data),
    
    logout: () => api.post('/auth/logout'),
    
    refresh: (refreshToken: string) =>
      api.post('/auth/refresh', { refreshToken }),
    
    forgotPassword: (email: string) =>
      api.post('/auth/forgot-password', { email }),
    
    resetPassword: (data: { token: string; newPassword: string }) =>
      api.post('/auth/reset-password', data),
    
    verifyEmail: (token: string) =>
      api.post('/auth/verify-email', { token }),
    
    verify2FA: (data: { code: string; token: string }) =>
      api.post('/auth/verify-2fa', data),
    
    enable2FA: () => api.post('/auth/enable-2fa'),
    
    disable2FA: (data: { code: string }) =>
      api.post('/auth/disable-2fa', data),
  },

  // User endpoints
  users: {
    getProfile: () => api.get('/api/v1/user/profile'),
    
    updateProfile: (data: Partial<UserProfile>) =>
      api.put('/users/profile', data),
    
    changePassword: (data: { currentPassword: string; newPassword: string }) =>
      api.post('/users/change-password', data),
    
    uploadAvatar: (formData: FormData) =>
      api.post('/users/avatar', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      }),
    
    deleteAccount: () => api.delete('/users/account'),
  },

  // Tenant endpoints
  tenants: {
    getTenants: (params?: { page?: number; pageSize?: number; search?: string }) =>
      api.get('/tenants', { params }),
    
    getTenant: (id: string) => api.get(`/tenants/${id}`),
    
    createTenant: (data: CreateTenantRequest) =>
      api.post('/tenants', data),
    
    updateTenant: (id: string, data: Partial<UpdateTenantRequest>) =>
      api.put(`/tenants/${id}`, data),
    
    deleteTenant: (id: string) => api.delete(`/tenants/${id}`),
    
    getTenantStats: (id: string) =>
      api.get(`/tenants/${id}/stats`),
    
    updateTenantSettings: (id: string, data: TenantSettings) =>
      api.put(`/tenants/${id}/settings`, data),
  },

  // Video endpoints
  videos: {
    getVideos: (params?: VideoListParams) =>
      api.get('/api/v1/videos', { params }),
    
    getVideo: (id: string) => api.get(`/api/v1/videos/${id}`),
    
    uploadVideo: (formData: FormData) =>
      api.post('/videos/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
        onUploadProgress: (progressEvent) => {
          const progress = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total!
          )
          // You can emit this progress to a callback if needed
        },
      }),
    
    updateVideo: (id: string, data: Partial<UpdateVideoRequest>) =>
      api.put(`/videos/${id}`, data),
    
    deleteVideo: (id: string) => api.delete(`/videos/${id}`),
    
    getVideoAnalytics: (id: string) =>
      api.get(`/videos/${id}/analytics`),
    
    generateThumbnail: (id: string, time?: number) =>
      api.post(`/videos/${id}/thumbnail`, { time }),
  },

  // Collection endpoints
  collections: {
    getCollections: (params?: CollectionListParams) =>
      api.get('/collections', { params }),
    
    getCollection: (id: string) => api.get(`/collections/${id}`),
    
    createCollection: (data: CreateCollectionRequest) =>
      api.post('/collections', data),
    
    updateCollection: (id: string, data: Partial<UpdateCollectionRequest>) =>
      api.put(`/collections/${id}`, data),
    
    deleteCollection: (id: string) => api.delete(`/collections/${id}`),
    
    addVideoToCollection: (collectionId: string, videoId: string) =>
      api.post(`/collections/${collectionId}/videos/${videoId}`),
    
    removeVideoFromCollection: (collectionId: string, videoId: string) =>
      api.delete(`/collections/${collectionId}/videos/${videoId}`),
  },

  // Subscription endpoints
  subscriptions: {
    getPlans: () => api.get('/subscriptions/plans'),
    
    getCurrentPlan: () => api.get('/subscriptions/current'),
    
    subscribe: (data: { planId: string; paymentMethodId: string }) =>
      api.post('/subscriptions/subscribe', data),
    
    cancelSubscription: () => api.post('/subscriptions/cancel'),
    
    updateSubscription: (data: { planId: string }) =>
      api.put('/subscriptions/update', data),
    
    getUsage: () => api.get('/subscriptions/usage'),
  },

  // Support endpoints
  support: {
    getTickets: (params?: { page?: number; status?: string }) =>
      api.get('/support/tickets', { params }),
    
    createTicket: (data: CreateTicketRequest) =>
      api.post('/support/tickets', data),
    
    getTicket: (id: string) => api.get(`/support/tickets/${id}`),
    
    updateTicket: (id: string, data: Partial<UpdateTicketRequest>) =>
      api.put(`/support/tickets/${id}`, data),
    
    addTicketReply: (id: string, data: { message: string; attachments?: File[] }) =>
      api.post(`/support/tickets/${id}/replies`, data, {
        headers: { 'Content-Type': 'multipart/form-data' },
      }),
  },
}

// Types
export interface UserProfile {
  id: string
  email: string
  firstName: string
  lastName: string
  avatar?: string
  role: string
  tenantId?: string
  createdAt: string
  updatedAt: string
}

export interface CreateTenantRequest {
  name: string
  subdomain: string
  planId: string
  settings?: {
    branding?: {
      logo?: string
      primaryColor?: string
      secondaryColor?: string
    }
    features?: {
      [key: string]: boolean
    }
  }
}

export interface UpdateTenantRequest {
  name?: string;
  status?: 'active' | 'inactive' | 'suspended';
  settings?: TenantSettings;
}

export interface TenantSettings {
  branding?: {
    logo?: string
    primaryColor?: string
    secondaryColor?: string
  }
  features?: {
    [key: string]: boolean
  }
  security?: {
    require2FA?: boolean
    passwordPolicy?: {
      minLength?: number
      requireUppercase?: boolean
      requireLowercase?: boolean
      requireNumbers?: boolean
      requireSymbols?: boolean
    }
  }
}

export interface VideoListParams {
  page?: number
  pageSize?: number
  search?: string
  category?: string
  tags?: string[]
  sortBy?: 'createdAt' | 'updatedAt' | 'title' | 'views'
  sortOrder?: 'asc' | 'desc'
}

export interface UpdateVideoRequest {
  title?: string
  description?: string
  tags?: string[]
  category?: string
  isPublic?: boolean
  allowDownload?: boolean
  allowEmbed?: boolean
}

export interface CollectionListParams {
  page?: number
  pageSize?: number
  search?: string
  parentId?: string
}

export interface CreateCollectionRequest {
  name: string
  description?: string
  parentId?: string
}

export interface UpdateCollectionRequest {
  name?: string
  description?: string
  parentId?: string
}

export interface CreateTicketRequest {
  subject: string
  description: string
  category: string
  priority: 'low' | 'medium' | 'high' | 'urgent'
  attachments?: File[]
}

export interface UpdateTicketRequest {
  subject?: string
  description?: string
  status?: string
  priority?: string
}

export default api
