import { create } from "zustand";

interface Video {
  id: string;
  title: string;
  description?: string;
  thumbnailUrl?: string;
  videoUrl?: string;
  duration: number;
  fileSize: number;
  viewCount: number;
  isPublic: boolean;
  status: string;
  createdAt: string;
  publishedAt?: string;
  user: {
    id: string;
    firstName: string;
    lastName: string;
    avatarUrl?: string;
  };
  category?: {
    id: string;
    name: string;
    slug: string;
  };
}

interface VideoFilters {
  search?: string;
  categoryId?: string;
  isPublic?: boolean;
  status?: string;
}

interface VideoState {
  videos: Video[];
  currentVideo: Video | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  page: number;
  pageSize: number;
  filters: VideoFilters;
}

interface VideoActions {
  fetchVideos: (page?: number, filters?: VideoFilters) => Promise<void>;
  fetchVideo: (id: string) => Promise<void>;
  uploadVideo: (file: File, title: string, description?: string) => Promise<any>;
  updateVideo: (id: string, data: Partial<Video>) => Promise<void>;
  deleteVideo: (id: string) => Promise<void>;
  getVideoStatus: (id: string) => Promise<any>;
  setFilters: (filters: VideoFilters) => void;
  clearError: () => void;
}

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api";

export const useVideoStore = create<VideoState & VideoActions>((set, get) => ({
  // State
  videos: [],
  currentVideo: null,
  loading: false,
  error: null,
  totalCount: 0,
  page: 1,
  pageSize: 20,
  filters: {},

  // Actions
  fetchVideos: async (page = 1, filters = {}) => {
    set({ loading: true, error: null });
    try {
      const params = new URLSearchParams();
      params.append("page", page.toString());
      params.append("pageSize", get().pageSize.toString());
      
      // Add filters that have values
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== "") {
          params.append(key, value.toString());
        }
      });

      const response = await fetch(`${API_BASE_URL}/videos?${params}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) throw new Error("Failed to fetch videos");

      const data = await response.json();
      
      set({
        videos: data.items,
        totalCount: data.totalCount,
        page: data.page,
        loading: false,
        filters: { ...get().filters, ...filters },
      });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "An error occurred", loading: false });
    }
  },

  fetchVideo: async (id: string) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/videos/${id}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) throw new Error("Failed to fetch video");

      const video = await response.json();
      set({ currentVideo: video, loading: false });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "An error occurred", loading: false });
    }
  },

  uploadVideo: async (file: File, title: string, description?: string) => {
    try {
      const formData = new FormData();
      formData.append("file", file);
      formData.append("title", title);
      if (description) formData.append("description", description);

      const response = await fetch(`${API_BASE_URL}/videos/upload`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
        body: formData,
      });

      if (!response.ok) throw new Error("Failed to upload video");

      return await response.json();
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Upload failed" });
      throw error;
    }
  },

  updateVideo: async (id: string, data: Partial<Video>) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/videos/${id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error("Failed to update video");

      const updatedVideo = await response.json();
      
      set((state) => ({
        videos: state.videos.map((v) => (v.id === id ? updatedVideo : v)),
        currentVideo: state.currentVideo?.id === id ? updatedVideo : state.currentVideo,
        loading: false,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Update failed", loading: false });
    }
  },

  deleteVideo: async (id: string) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/videos/${id}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) throw new Error("Failed to delete video");

      set((state) => ({
        videos: state.videos.filter((v) => v.id !== id),
        currentVideo: state.currentVideo?.id === id ? null : state.currentVideo,
        loading: false,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Delete failed", loading: false });
    }
  },

  getVideoStatus: async (id: string) => {
    try {
      const response = await fetch(`${API_BASE_URL}/videos/${id}/status`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) throw new Error("Failed to get video status");

      return await response.json();
    } catch (error) {
      console.error("Failed to get video status:", error);
      throw error;
    }
  },

  setFilters: (filters: VideoFilters) => {
    set({ filters: { ...get().filters, ...filters } });
  },

  clearError: () => {
    set({ error: null });
  },
}));
