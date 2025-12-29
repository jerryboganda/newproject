import { create } from "zustand";

interface Collection {
  id: string;
  name: string;
  description?: string;
  isPublic: boolean;
  videoCount: number;
  createdAt: string;
  updatedAt: string;
  user: {
    id: string;
    firstName: string;
    lastName: string;
    avatarUrl?: string;
  };
}

interface CollectionState {
  collections: Collection[];
  currentCollection: Collection | null;
  loading: boolean;
  error: string | null;
}

interface CollectionActions {
  fetchCollections: () => Promise<void>;
  fetchCollection: (id: string) => Promise<void>;
  createCollection: (name: string, description?: string) => Promise<void>;
  updateCollection: (id: string, data: Partial<Collection>) => Promise<void>;
  deleteCollection: (id: string) => Promise<void>;
  addVideoToCollection: (collectionId: string, videoId: string) => Promise<void>;
  removeVideoFromCollection: (collectionId: string, videoId: string) => Promise<void>;
  reorderVideos: (collectionId: string, videoOrders: { videoId: string; order: number }[]) => Promise<void>;
  clearError: () => void;
}

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api";

export const useCollectionStore = create<CollectionState & CollectionActions>((set, get) => ({
  // State
  collections: [],
  currentCollection: null,
  loading: false,
  error: null,

  // Actions
  fetchCollections: async () => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/collections`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) throw new Error("Failed to fetch collections");

      const collections = await response.json();
      set({ collections, loading: false });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "An error occurred", loading: false });
    }
  },

  fetchCollection: async (id: string) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/collections/${id}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) throw new Error("Failed to fetch collection");

      const collection = await response.json();
      set({ currentCollection: collection, loading: false });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "An error occurred", loading: false });
    }
  },

  createCollection: async (name: string, description?: string) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/collections`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
        body: JSON.stringify({ name, description }),
      });

      if (!response.ok) throw new Error("Failed to create collection");

      const newCollection = await response.json();
      
      set((state) => ({
        collections: [...state.collections, newCollection],
        loading: false,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Create failed", loading: false });
    }
  },

  updateCollection: async (id: string, data: Partial<Collection>) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/collections/${id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error("Failed to update collection");

      const updatedCollection = await response.json();
      
      set((state) => ({
        collections: state.collections.map((c) => (c.id === id ? updatedCollection : c)),
        currentCollection: state.currentCollection?.id === id ? updatedCollection : state.currentCollection,
        loading: false,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Update failed", loading: false });
    }
  },

  deleteCollection: async (id: string) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/collections/${id}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) throw new Error("Failed to delete collection");

      set((state) => ({
        collections: state.collections.filter((c) => c.id !== id),
        currentCollection: state.currentCollection?.id === id ? null : state.currentCollection,
        loading: false,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Delete failed", loading: false });
    }
  },

  addVideoToCollection: async (collectionId: string, videoId: string) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/collections/${collectionId}/videos`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
        body: JSON.stringify({ videoId }),
      });

      if (!response.ok) throw new Error("Failed to add video to collection");

      // Refresh collections to update video counts
      await get().fetchCollections();
      set({ loading: false });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Failed to add video", loading: false });
    }
  },

  removeVideoFromCollection: async (collectionId: string, videoId: string) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/collections/${collectionId}/videos/${videoId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });

      if (!response.ok) throw new Error("Failed to remove video from collection");

      // Refresh collections to update video counts
      await get().fetchCollections();
      set({ loading: false });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Failed to remove video", loading: false });
    }
  },

  reorderVideos: async (collectionId: string, videoOrders: { videoId: string; order: number }[]) => {
    set({ loading: true, error: null });
    try {
      const response = await fetch(`${API_BASE_URL}/collections/${collectionId}/videos/reorder`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
        body: JSON.stringify({ videoOrders }),
      });

      if (!response.ok) throw new Error("Failed to reorder videos");

      set({ loading: false });
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Failed to reorder videos", loading: false });
    }
  },

  clearError: () => {
    set({ error: null });
  },
}));
