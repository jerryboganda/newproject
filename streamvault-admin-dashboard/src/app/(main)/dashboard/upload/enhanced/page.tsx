"use client";

import { useState, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Loader2,
  Upload,
  X,
  FileVideo,
  AlertCircle,
  CheckCircle,
  Clock,
  Eye,
  Edit,
  Trash2,
  Plus,
  FolderPlus
} from "lucide-react";
import { useDropzone } from "react-dropzone";
import { useVideoStore } from "@/stores/video-store";
import { useCollectionStore } from "@/stores/collection-store";

interface UploadFile {
  file: File;
  id: string;
  progress: number;
  status: "pending" | "uploading" | "processing" | "completed" | "error";
  error?: string;
  videoId?: string;
  title?: string;
  description?: string;
}

export default function EnhancedUploadPage() {
  const [files, setFiles] = useState<UploadFile[]>([]);
  const [dragActive, setDragActive] = useState(false);
  const [selectedCollection, setSelectedCollection] = useState<string>("");
  const [isCreatingCollection, setIsCreatingCollection] = useState(false);
  const [newCollectionName, setNewCollectionName] = useState("");
  const router = useRouter();
  const { uploadVideo, getVideoStatus } = useVideoStore();
  const { collections, createCollection } = useCollectionStore();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const newFiles: UploadFile[] = acceptedFiles.map((file) => ({
      file,
      id: Math.random().toString(36).substr(2, 9),
      progress: 0,
      status: "pending",
      title: file.name.replace(/\.[^/.]+$/, ""),
    }));
    setFiles((prev) => [...prev, ...newFiles]);
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      "video/*": [".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv"],
    },
    multiple: true,
    maxSize: 2 * 1024 * 1024 * 1024, // 2GB
  });

  const removeFile = (id: string) => {
    setFiles((prev) => prev.filter((f) => f.id !== id));
  };

  const updateFileData = (id: string, data: Partial<UploadFile>) => {
    setFiles((prev) =>
      prev.map((f) => (f.id === id ? { ...f, ...data } : f))
    );
  };

  const uploadSingleFile = async (fileData: UploadFile) => {
    try {
      updateFileData(fileData.id, { status: "uploading", progress: 0 });

      // Initiate upload
      const initiation = await fetch("/api/videos/upload/initiate", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
        body: JSON.stringify({
          title: fileData.title || fileData.file.name,
          description: fileData.description,
          fileName: fileData.file.name,
          contentType: fileData.file.type,
        }),
      });

      const initiationData = await initiation.json();
      if (!initiation.ok) throw new Error(initiationData.error);

      // Upload file with progress simulation
      const formData = new FormData();
      formData.append("file", fileData.file);

      const xhr = new XMLHttpRequest();
      
      xhr.upload.addEventListener("progress", (e) => {
        if (e.lengthComputable) {
          const progress = Math.round((e.loaded / e.total) * 100);
          updateFileData(fileData.id, { progress });
        }
      });

      await new Promise((resolve, reject) => {
        xhr.onload = resolve;
        xhr.onerror = reject;
        xhr.open("POST", `/api/videos/${initiationData.videoId}/upload`);
        xhr.setRequestHeader(
          "Authorization",
          `Bearer ${localStorage.getItem("accessToken")}`
        );
        xhr.send(formData);
      });

      // Complete upload
      await fetch(`/api/videos/${initiationData.videoId}/upload/complete`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
        body: JSON.stringify({
          fileSize: fileData.file.size,
          storagePath: initiationData.storageUrl,
        }),
      });

      updateFileData(fileData.id, {
        status: "processing",
        progress: 100,
        videoId: initiationData.videoId,
      });

      // Poll for processing status
      const pollStatus = async () => {
        const status = await getVideoStatus(initiationData.videoId);
        if (status.status === "ready") {
          updateFileData(fileData.id, { status: "completed" });
        } else if (status.status === "failed") {
          updateFileData(fileData.id, { status: "error", error: "Processing failed" });
        } else {
          setTimeout(pollStatus, 2000);
        }
      };

      pollStatus();
    } catch (error: any) {
      updateFileData(fileData.id, {
        status: "error",
        error: error.message || "Upload failed",
      });
    }
  };

  const startUpload = async () => {
    for (const fileData of files) {
      if (fileData.status === "pending") {
        await uploadSingleFile(fileData);
      }
    }
  };

  const handleCreateCollection = async () => {
    if (!newCollectionName.trim()) return;

    try {
      await createCollection(newCollectionName);
      setNewCollectionName("");
      setIsCreatingCollection(false);
    } catch (error) {
      console.error("Failed to create collection:", error);
    }
  };

  const getStatusIcon = (status: UploadFile["status"]) => {
    switch (status) {
      case "pending":
        return <Clock className="h-4 w-4 text-gray-400" />;
      case "uploading":
        return <Loader2 className="h-4 w-4 text-blue-600 animate-spin" />;
      case "processing":
        return <Loader2 className="h-4 w-4 text-orange-600 animate-spin" />;
      case "completed":
        return <CheckCircle className="h-4 w-4 text-green-600" />;
      case "error":
        return <AlertCircle className="h-4 w-4 text-red-600" />;
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + " " + sizes[i];
  };

  return (
    <div className="container mx-auto py-8 max-w-4xl">
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Upload Videos</h1>
        <p className="text-gray-600 dark:text-gray-400">
          Upload your videos to StreamVault. We support various formats up to 2GB.
        </p>
      </div>

      <Tabs defaultValue="upload" className="space-y-6">
        <TabsList>
          <TabsTrigger value="upload">Upload</TabsTrigger>
          <TabsTrigger value="queue">Queue ({files.length})</TabsTrigger>
        </TabsList>

        <TabsContent value="upload" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Choose Files</CardTitle>
              <CardDescription>
                Drag and drop your videos here, or click to browse
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div
                {...getRootProps()}
                className={`border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors ${
                  isDragActive
                    ? "border-blue-500 bg-blue-50 dark:bg-blue-950"
                    : "border-gray-300 hover:border-gray-400"
                }`}
              >
                <input {...getInputProps()} />
                <Upload className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                {isDragActive ? (
                  <p className="text-blue-600">Drop the files here...</p>
                ) : (
                  <div>
                    <p className="text-gray-600 mb-2">
                      Drag & drop videos here, or click to select
                    </p>
                    <p className="text-sm text-gray-500">
                      MP4, AVI, MOV, WMV, FLV, WebM (Max 2GB)
                    </p>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          {files.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Upload Settings</CardTitle>
                <CardDescription>
                  Configure settings for your uploads
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <Label>Add to Collection</Label>
                    <div className="flex gap-2 mt-1">
                      <Select value={selectedCollection} onValueChange={setSelectedCollection}>
                        <SelectTrigger className="flex-1">
                          <SelectValue placeholder="Select collection" />
                        </SelectTrigger>
                        <SelectContent>
                          {collections.map((collection) => (
                            <SelectItem key={collection.id} value={collection.id}>
                              {collection.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <Button
                        variant="outline"
                        size="icon"
                        onClick={() => setIsCreatingCollection(true)}
                      >
                        <Plus className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                </div>

                {isCreatingCollection && (
                  <div className="flex gap-2">
                    <Input
                      placeholder="Collection name"
                      value={newCollectionName}
                      onChange={(e) => setNewCollectionName(e.target.value)}
                      className="flex-1"
                    />
                    <Button onClick={handleCreateCollection}>
                      <FolderPlus className="h-4 w-4 mr-2" />
                      Create
                    </Button>
                    <Button
                      variant="outline"
                      onClick={() => {
                        setIsCreatingCollection(false);
                        setNewCollectionName("");
                      }}
                    >
                      Cancel
                    </Button>
                  </div>
                )}

                <Button onClick={startUpload} className="w-full" size="lg">
                  <Upload className="mr-2 h-4 w-4" />
                  Start Upload ({files.length} file{files.length > 1 ? "s" : ""})
                </Button>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="queue" className="space-y-4">
          {files.length === 0 ? (
            <Card>
              <CardContent className="py-12 text-center">
                <FileVideo className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                <p className="text-gray-600">No files in queue</p>
                <Button
                  variant="outline"
                  className="mt-4"
                  onClick={() => fileInputRef.current?.click()}
                >
                  Add Files
                </Button>
              </CardContent>
            </Card>
          ) : (
            files.map((fileData) => (
              <Card key={fileData.id}>
                <CardContent className="py-4">
                  <div className="flex items-start gap-4">
                    <div className="mt-1">
                      {getStatusIcon(fileData.status)}
                    </div>
                    <div className="flex-1 space-y-2">
                      <div className="flex items-center justify-between">
                        <Input
                          value={fileData.title}
                          onChange={(e) =>
                            updateFileData(fileData.id, { title: e.target.value })
                          }
                          className="font-medium"
                          disabled={fileData.status !== "pending"}
                        />
                        <div className="flex gap-1">
                          {fileData.status === "completed" && (
                            <Button
                              variant="outline"
                              size="icon"
                              onClick={() => router.push(`/dashboard/videos/${fileData.videoId}`)}
                            >
                              <Eye className="h-4 w-4" />
                            </Button>
                          )}
                          <Button
                            variant="outline"
                            size="icon"
                            onClick={() => removeFile(fileData.id)}
                            disabled={fileData.status === "uploading"}
                          >
                            <X className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                      <Textarea
                        placeholder="Add a description..."
                        value={fileData.description}
                        onChange={(e) =>
                          updateFileData(fileData.id, { description: e.target.value })
                        }
                        className="min-h-[60px]"
                        disabled={fileData.status !== "pending"}
                      />
                      <div className="flex items-center justify-between text-sm text-gray-500">
                        <span>{formatFileSize(fileData.file.size)}</span>
                        <span className="capitalize">{fileData.status}</span>
                      </div>
                      {(fileData.status === "uploading" || fileData.status === "processing") && (
                        <Progress value={fileData.progress} className="w-full" />
                      )}
                      {fileData.error && (
                        <Alert variant="destructive">
                          <AlertDescription>{fileData.error}</AlertDescription>
                        </Alert>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
