export type StreamVaultVideo = {
  id: number
  title: string
  description: string
  thumbnailUrl: string
  videoUrl: string
  duration: number
  createdAt?: string
  viewCount: number
  isPublic?: boolean
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

export const STREAMVAULT_API_BASE_URL =
  process.env.NEXT_PUBLIC_STREAMVAULT_API_BASE_URL ?? "http://localhost:5001"

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
      requestJson<{ token: string; user: { id: string; email: string } }>("/api/v1/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      }),
  },
  user: {
    profile: () => requestJson<StreamVaultUserProfile>("/api/v1/user/profile"),
  },
  videos: {
    list: () => requestJson<StreamVaultVideo[]>("/api/v1/videos"),
    get: (id: number) => requestJson<StreamVaultVideo>(`/api/v1/videos/${id}`),
  },
}
