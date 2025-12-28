import { create } from 'zustand';

interface Video {
  id: string;
  title: string;
  description?: string;
  thumbnailUrl?: string;
  videoId: string;
  duration: number;
  sizeBytes: number;
  mimeType: string;
  encodingStatus: string;
  isPublic: boolean;
  isEmbeddable: boolean;
  allowDownload: boolean;
  views: number;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string;
  tags?: string[];
}

interface VideoState {
  videos: Video[];
  currentVideo: Video | null;
  isLoading: boolean;
  error: string | null;
  fetchVideos: () => Promise<void>;
  uploadVideo: (file: File, metadata: any) => Promise<void>;
  getVideoById: (id: string) => Promise<void>;
  updateVideo: (id: string, data: Partial<Video>) => Promise<void>;
  deleteVideo: (id: string) => Promise<void>;
  setCurrentVideo: (video: Video | null) => void;
}

export const useVideoStore = create<VideoState>((set, get) => ({
  videos: [],
  currentVideo: null,
  isLoading: false,
  error: null,

  fetchVideos: async () => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch('/api/v1/videos', {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to fetch videos');
      }

      const videos = await response.json();
      set({ videos, isLoading: false });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : 'An error occurred', isLoading: false });
    }
  },

  uploadVideo: async (file: File, metadata: any) => {
    set({ isLoading: true, error: null });
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('metadata', JSON.stringify(metadata));

      const response = await fetch('/api/v1/videos/upload', {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
        },
        body: formData,
      });

      if (!response.ok) {
        throw new Error('Failed to upload video');
      }

      const newVideo = await response.json();
      set(state => ({
        videos: [newVideo, ...state.videos],
        isLoading: false,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : 'An error occurred', isLoading: false });
    }
  },

  getVideoById: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`/api/v1/videos/${id}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to fetch video');
      }

      const video = await response.json();
      set({ currentVideo: video, isLoading: false });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : 'An error occurred', isLoading: false });
    }
  },

  updateVideo: async (id: string, data: Partial<Video>) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`/api/v1/videos/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error('Failed to update video');
      }

      const updatedVideo = await response.json();
      set(state => ({
        videos: state.videos.map(v => v.id === id ? updatedVideo : v),
        currentVideo: state.currentVideo?.id === id ? updatedVideo : state.currentVideo,
        isLoading: false,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : 'An error occurred', isLoading: false });
    }
  },

  deleteVideo: async (id: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await fetch(`/api/v1/videos/${id}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to delete video');
      }

      set(state => ({
        videos: state.videos.filter(v => v.id !== id),
        currentVideo: state.currentVideo?.id === id ? null : state.currentVideo,
        isLoading: false,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : 'An error occurred', isLoading: false });
    }
  },

  setCurrentVideo: (video: Video | null) => {
    set({ currentVideo: video });
  },
}));
