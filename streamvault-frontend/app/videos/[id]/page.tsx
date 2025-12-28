'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api-client';

interface Video {
  id: string;
  title: string;
  description?: string;
  thumbnailUrl?: string;
  videoUrl: string;
  durationSeconds: number;
  viewCount: number;
  createdAt: string;
  user: {
    firstName: string;
    lastName: string;
    email: string;
  };
  tags: string[];
  isPublic: boolean;
}

export default function VideoDetailPage() {
  const params = useParams();
  const router = useRouter();
  const [video, setVideo] = useState<Video | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [editTitle, setEditTitle] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [editIsPublic, setEditIsPublic] = useState(false);

  useEffect(() => {
    if (params.id) {
      fetchVideo(params.id as string);
    }
  }, [params.id]);

  const fetchVideo = async (id: string) => {
    try {
      setLoading(true);
      const response = await apiClient.get(`/api/v1/video/${id}`);
      const videoData = response.data as any;
      setVideo(videoData);
      setEditTitle(videoData.title);
      setEditDescription(videoData.description || '');
      setEditIsPublic(videoData.isPublic);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to fetch video');
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    if (!video) return;

    try {
      await apiClient.put(`/api/v1/video/${video.id}`, {
        title: editTitle,
        description: editDescription,
        isPublic: editIsPublic
      });
      
      setVideo({
        ...video,
        title: editTitle,
        description: editDescription,
        isPublic: editIsPublic
      });
      setIsEditing(false);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to update video');
    }
  };

  const handleDelete = async () => {
    if (!video || !confirm('Are you sure you want to delete this video?')) return;

    try {
      await apiClient.delete(`/api/v1/video/${video.id}`);
      router.push('/videos');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to delete video');
    }
  };

  const formatDuration = (seconds: number) => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    }
    return `${minutes}:${secs.toString().padStart(2, '0')}`;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex justify-center items-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error || !video) {
    return (
      <div className="min-h-screen bg-gray-50 flex justify-center items-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 mb-2">Error</h1>
          <p className="text-gray-600">{error || 'Video not found'}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Video Player */}
        <div className="bg-black rounded-lg overflow-hidden mb-6">
          <video
            controls
            className="w-full aspect-video"
            poster={video.thumbnailUrl}
          >
            <source src={video.videoUrl} type="video/mp4" />
            Your browser does not support the video tag.
          </video>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main Content */}
          <div className="lg:col-span-2">
            {/* Title and Actions */}
            <div className="bg-white rounded-lg shadow p-6 mb-6">
              {isEditing ? (
                <div className="space-y-4">
                  <input
                    type="text"
                    value={editTitle}
                    onChange={(e) => setEditTitle(e.target.value)}
                    className="w-full text-2xl font-bold px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  />
                  <textarea
                    value={editDescription}
                    onChange={(e) => setEditDescription(e.target.value)}
                    rows={4}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Add a description..."
                  />
                  <div className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      id="public"
                      checked={editIsPublic}
                      onChange={(e) => setEditIsPublic(e.target.checked)}
                      className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <label htmlFor="public" className="text-sm text-gray-700">
                      Make this video public
                    </label>
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={handleSave}
                      className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                    >
                      Save
                    </button>
                    <button
                      onClick={() => {
                        setIsEditing(false);
                        setEditTitle(video.title);
                        setEditDescription(video.description || '');
                        setEditIsPublic(video.isPublic);
                      }}
                      className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                <div>
                  <h1 className="text-2xl font-bold text-gray-900 mb-2">{video.title}</h1>
                  {video.description && (
                    <p className="text-gray-600 mb-4">{video.description}</p>
                  )}
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4 text-sm text-gray-500">
                      <span>{video.viewCount} views</span>
                      <span>{formatDate(video.createdAt)}</span>
                      {!video.isPublic && (
                        <span className="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs">
                          Private
                        </span>
                      )}
                    </div>
                    <div className="flex gap-2">
                      <button
                        onClick={() => setIsEditing(true)}
                        className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300"
                      >
                        Edit
                      </button>
                      <button
                        onClick={handleDelete}
                        className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              )}
            </div>

            {/* Tags */}
            {video.tags.length > 0 && (
              <div className="bg-white rounded-lg shadow p-6 mb-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-3">Tags</h2>
                <div className="flex flex-wrap gap-2">
                  {video.tags.map((tag, index) => (
                    <span
                      key={index}
                      className="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm"
                    >
                      {tag}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </div>

          {/* Sidebar */}
          <div>
            {/* Video Info */}
            <div className="bg-white rounded-lg shadow p-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Video Details</h2>
              <dl className="space-y-3">
                <div>
                  <dt className="text-sm font-medium text-gray-500">Duration</dt>
                  <dd className="text-sm text-gray-900">{formatDuration(video.durationSeconds)}</dd>
                </div>
                <div>
                  <dt className="text-sm font-medium text-gray-500">Uploaded by</dt>
                  <dd className="text-sm text-gray-900">
                    {video.user.firstName} {video.user.lastName}
                  </dd>
                  <dd className="text-xs text-gray-500">{video.user.email}</dd>
                </div>
                <div>
                  <dt className="text-sm font-medium text-gray-500">Visibility</dt>
                  <dd className="text-sm text-gray-900">
                    {video.isPublic ? 'Public' : 'Private'}
                  </dd>
                </div>
              </dl>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
