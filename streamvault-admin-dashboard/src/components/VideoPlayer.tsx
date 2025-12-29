"use client";

import { useState, useRef, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Slider } from "@/components/ui/slider";
import {
  Play,
  Pause,
  Volume2,
  VolumeX,
  Maximize,
  SkipBack,
  SkipForward,
  Settings,
  Type,
  Layers,
  Monitor,
  Loader2,
  Download,
  Share,
  ThumbsUp,
  ThumbsDown,
  Bookmark,
} from "lucide-react";

interface VideoPlayerProps {
  src: string;
  poster?: string;
  title?: string;
  captions?: CaptionTrack[];
  chapters?: Chapter[];
  autoPlay?: boolean;
  onProgress?: (progress: number) => void;
  onEnded?: () => void;
  onTimeUpdate?: (currentTime: number, duration: number) => void;
}

interface CaptionTrack {
  id: string;
  label: string;
  language: string;
  src: string;
}

interface Chapter {
  id: string;
  title: string;
  startTime: number;
  endTime?: number;
}

export default function VideoPlayer({
  src,
  poster,
  title,
  captions = [],
  chapters = [],
  autoPlay = false,
  onProgress,
  onEnded,
  onTimeUpdate,
}: VideoPlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [volume, setVolume] = useState(1);
  const [isMuted, setIsMuted] = useState(false);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [showControls, setShowControls] = useState(true);
  const [selectedCaption, setSelectedCaption] = useState<string | null>(null);
  const [playbackRate, setPlaybackRate] = useState(1);
  const [quality, setQuality] = useState<string>("auto");
  const [showSettings, setShowSettings] = useState(false);
  const [buffered, setBuffered] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const controlsTimeoutRef = useRef<NodeJS.Timeout>();

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    const handleTimeUpdate = () => {
      setCurrentTime(video.currentTime);
      const progress = (video.currentTime / video.duration) * 100;
      onProgress?.(progress);
      onTimeUpdate?.(video.currentTime, video.duration);
    };

    const handleLoadedMetadata = () => {
      setDuration(video.duration);
      setIsLoading(false);
    };

    const handleProgress = () => {
      if (video.buffered.length > 0) {
        const bufferedEnd = video.buffered.end(video.buffered.length - 1);
        const duration = video.duration;
        if (duration > 0) {
          setBuffered((bufferedEnd / duration) * 100);
        }
      }
    };

    const handleEnded = () => {
      setIsPlaying(false);
      onEnded?.();
    };

    const handleWaiting = () => setIsLoading(true);
    const handleCanPlay = () => setIsLoading(false);

    video.addEventListener("timeupdate", handleTimeUpdate);
    video.addEventListener("loadedmetadata", handleLoadedMetadata);
    video.addEventListener("progress", handleProgress);
    video.addEventListener("ended", handleEnded);
    video.addEventListener("waiting", handleWaiting);
    video.addEventListener("canplay", handleCanPlay);

    return () => {
      video.removeEventListener("timeupdate", handleTimeUpdate);
      video.removeEventListener("loadedmetadata", handleLoadedMetadata);
      video.removeEventListener("progress", handleProgress);
      video.removeEventListener("ended", handleEnded);
      video.removeEventListener("waiting", handleWaiting);
      video.removeEventListener("canplay", handleCanPlay);
    };
  }, [onProgress, onEnded, onTimeUpdate]);

  useEffect(() => {
    if (autoPlay && videoRef.current) {
      videoRef.current.play();
      setIsPlaying(true);
    }
  }, [autoPlay]);

  useEffect(() => {
    const handleFullscreenChange = () => {
      setIsFullscreen(!!document.fullscreenElement);
    };

    document.addEventListener("fullscreenchange", handleFullscreenChange);
    return () => document.removeEventListener("fullscreenchange", handleFullscreenChange);
  }, []);

  const togglePlay = () => {
    const video = videoRef.current;
    if (!video) return;

    if (isPlaying) {
      video.pause();
    } else {
      video.play();
    }
    setIsPlaying(!isPlaying);
  };

  const handleSeek = (value: number[]) => {
    const video = videoRef.current;
    if (!video) return;

    const time = (value[0] / 100) * video.duration;
    video.currentTime = time;
    setCurrentTime(time);
  };

  const handleVolumeChange = (value: number[]) => {
    const video = videoRef.current;
    if (!video) return;

    const newVolume = value[0] / 100;
    video.volume = newVolume;
    setVolume(newVolume);
    setIsMuted(newVolume === 0);
  };

  const toggleMute = () => {
    const video = videoRef.current;
    if (!video) return;

    video.muted = !isMuted;
    setIsMuted(!isMuted);
  };

  const toggleFullscreen = () => {
    const container = containerRef.current;
    if (!container) return;

    if (!isFullscreen) {
      container.requestFullscreen();
    } else {
      document.exitFullscreen();
    }
  };

  const skip = (seconds: number) => {
    const video = videoRef.current;
    if (!video) return;

    video.currentTime = Math.max(0, Math.min(duration, video.currentTime + seconds));
  };

  const changePlaybackRate = (rate: number) => {
    const video = videoRef.current;
    if (!video) return;

    video.playbackRate = rate;
    setPlaybackRate(rate);
    setShowSettings(false);
  };

  const selectCaptionTrack = (trackId: string | null) => {
    const video = videoRef.current;
    if (!video) return;

    // Remove all existing tracks
    for (let i = 0; i < video.textTracks.length; i++) {
      video.textTracks[i].mode = "hidden";
    }

    // Enable selected track
    if (trackId) {
      const track = Array.from(video.textTracks).find(
        (t) => (t as any).id === trackId
      );
      if (track) {
        track.mode = "showing";
      }
    }

    setSelectedCaption(trackId);
    setShowSettings(false);
  };

  const formatTime = (time: number) => {
    const hours = Math.floor(time / 3600);
    const minutes = Math.floor((time % 3600) / 60);
    const seconds = Math.floor(time % 60);

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, "0")}:${seconds
        .toString()
        .padStart(2, "0")}`;
    }
    return `${minutes}:${seconds.toString().padStart(2, "0")}`;
  };

  const getCurrentChapter = () => {
    return chapters.find(
      (chapter) =>
        currentTime >= chapter.startTime &&
        (!chapter.endTime || currentTime <= chapter.endTime)
    );
  };

  const showControlsTemporarily = () => {
    setShowControls(true);
    if (controlsTimeoutRef.current) {
      clearTimeout(controlsTimeoutRef.current);
    }
    controlsTimeoutRef.current = setTimeout(() => {
      if (isPlaying) {
        setShowControls(false);
      }
    }, 3000);
  };

  const currentChapter = getCurrentChapter();

  return (
    <div
      ref={containerRef}
      className="relative bg-black rounded-lg overflow-hidden group"
      onMouseMove={showControlsTemporarily}
      onMouseLeave={() => isPlaying && setShowControls(false)}
    >
      <video
        ref={videoRef}
        src={src}
        poster={poster}
        className="w-full h-full"
        onClick={togglePlay}
      >
        {captions.map((caption) => (
          <track
            key={caption.id}
            id={caption.id}
            kind="subtitles"
            label={caption.label}
            srcLang={caption.language}
            src={caption.src}
          />
        ))}
      </video>

      {/* Loading indicator */}
      {isLoading && (
        <div className="absolute inset-0 flex items-center justify-center bg-black/50">
          <Loader2 className="h-8 w-8 animate-spin text-white" />
        </div>
      )}

      {/* Overlay controls */}
      <div
        className={`absolute inset-0 bg-gradient-to-b from-black/50 via-transparent to-black/50 transition-opacity duration-300 ${
          showControls ? "opacity-100" : "opacity-0"
        }`}
      />

      {/* Play button overlay */}
      {!isPlaying && !isLoading && (
        <div className="absolute inset-0 flex items-center justify-center">
          <Button
            size="lg"
            variant="secondary"
            className="h-16 w-16 rounded-full"
            onClick={togglePlay}
          >
            <Play className="h-8 w-8" />
          </Button>
        </div>
      )}

      {/* Top bar */}
      <div
        className={`absolute top-0 left-0 right-0 p-4 bg-gradient-to-b from-black/70 to-transparent transition-opacity duration-300 ${
          showControls ? "opacity-100" : "opacity-0"
        }`}
      >
        <div className="flex items-center justify-between">
          {title && <h3 className="text-white font-semibold">{title}</h3>}
          <div className="flex items-center gap-2">
            {currentChapter && (
              <span className="text-white text-sm">{currentChapter.title}</span>
            )}
            <Button size="sm" variant="ghost" className="text-white hover:bg-white/20">
              <Share className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      {/* Bottom controls */}
      <div
        className={`absolute bottom-0 left-0 right-0 p-4 bg-gradient-to-t from-black/70 to-transparent transition-opacity duration-300 ${
          showControls ? "opacity-100" : "opacity-0"
        }`}
      >
        {/* Progress bar */}
        <div className="mb-4">
          <Slider
            value={[duration > 0 ? (currentTime / duration) * 100 : 0]}
            onValueChange={handleSeek}
            max={100}
            step={0.1}
            className="w-full"
          />
          <div className="relative mt-1 h-1 bg-gray-600 rounded">
            <div
              className="absolute h-full bg-red-600 rounded"
              style={{ width: `${buffered}%` }}
            />
          </div>
        </div>

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            {/* Play/Pause */}
            <Button size="sm" variant="ghost" onClick={togglePlay}>
              {isPlaying ? (
                <Pause className="h-4 w-4" />
              ) : (
                <Play className="h-4 w-4" />
              )}
            </Button>

            {/* Skip backward */}
            <Button size="sm" variant="ghost" onClick={() => skip(-10)}>
              <SkipBack className="h-4 w-4" />
            </Button>

            {/* Skip forward */}
            <Button size="sm" variant="ghost" onClick={() => skip(10)}>
              <SkipForward className="h-4 w-4" />
            </Button>

            {/* Time display */}
            <span className="text-white text-sm font-mono">
              {formatTime(currentTime)} / {formatTime(duration)}
            </span>

            {/* Volume */}
            <div className="flex items-center gap-2">
              <Button size="sm" variant="ghost" onClick={toggleMute}>
                {isMuted || volume === 0 ? (
                  <VolumeX className="h-4 w-4" />
                ) : (
                  <Volume2 className="h-4 w-4" />
                )}
              </Button>
              <Slider
                value={[volume * 100]}
                onValueChange={handleVolumeChange}
                max={100}
                step={1}
                className="w-20"
              />
            </div>
          </div>

          <div className="flex items-center gap-2">
            {/* Captions */}
            {captions.length > 0 && (
              <Button
                size="sm"
                variant={selectedCaption ? "secondary" : "ghost"}
                onClick={() => setShowSettings(!showSettings)}
              >
                <Type className="h-4 w-4" />
              </Button>
            )}

            {/* Settings */}
            <div className="relative">
              <Button
                size="sm"
                variant="ghost"
                onClick={() => setShowSettings(!showSettings)}
              >
                <Settings className="h-4 w-4" />
              </Button>

              {showSettings && (
                <div className="absolute bottom-full right-0 mb-2 w-48 bg-black/90 rounded-lg p-2 space-y-2">
                  {/* Playback speed */}
                  <div className="space-y-1">
                    <p className="text-white text-xs">Playback Speed</p>
                    {[0.5, 0.75, 1, 1.25, 1.5, 2].map((rate) => (
                      <Button
                        key={rate}
                        size="sm"
                        variant={playbackRate === rate ? "secondary" : "ghost"}
                        className="w-full justify-start"
                        onClick={() => changePlaybackRate(rate)}
                      >
                        {rate}x
                      </Button>
                    ))}
                  </div>

                  {/* Captions */}
                  {captions.length > 0 && (
                    <div className="space-y-1">
                      <p className="text-white text-xs">Captions</p>
                      <Button
                        size="sm"
                        variant={!selectedCaption ? "secondary" : "ghost"}
                        className="w-full justify-start"
                        onClick={() => selectCaptionTrack(null)}
                      >
                        Off
                      </Button>
                      {captions.map((caption) => (
                        <Button
                          key={caption.id}
                          size="sm"
                          variant={selectedCaption === caption.id ? "secondary" : "ghost"}
                          className="w-full justify-start"
                          onClick={() => selectCaptionTrack(caption.id)}
                        >
                          {caption.label}
                        </Button>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>

            {/* Picture in Picture */}
            <Button
              size="sm"
              variant="ghost"
              onClick={() => videoRef.current?.requestPictureInPicture()}
            >
              <Monitor className="h-4 w-4" />
            </Button>

            {/* Fullscreen */}
            <Button size="sm" variant="ghost" onClick={toggleFullscreen}>
              <Maximize className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      {/* Engagement buttons */}
      <div className="absolute bottom-20 right-4 flex flex-col gap-2">
        <Button size="sm" variant="ghost" className="text-white hover:bg-white/20">
          <ThumbsUp className="h-4 w-4" />
        </Button>
        <Button size="sm" variant="ghost" className="text-white hover:bg-white/20">
          <ThumbsDown className="h-4 w-4" />
        </Button>
        <Button size="sm" variant="ghost" className="text-white hover:bg-white/20">
          <Bookmark className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
