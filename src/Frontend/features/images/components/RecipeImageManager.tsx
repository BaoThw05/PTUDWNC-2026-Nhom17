"use client";

/* Ảnh có thể là URL MinIO hoặc object URL xem trước nên dùng img gốc. */
/* eslint-disable @next/next/no-img-element */

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { ApiError } from "@/lib/api/client";
import {
  deleteRecipeImage,
  getRecipeImages,
  updateRecipeImage,
  uploadRecipeImage,
} from "../api/client";
import type { RecipeImage } from "../types";

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const ALLOWED_TYPES = new Set(["image/jpeg", "image/png", "image/webp"]);

type UploadItem = {
  id: string;
  file: File;
  previewUrl: string;
  progress: number;
  status: "queued" | "uploading" | "failed";
  error?: string;
};

function sortImages(images: RecipeImage[]): RecipeImage[] {
  return [...images].sort((a, b) => a.orderIndex - b.orderIndex || a.imageId.localeCompare(b.imageId));
}

function imageErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    switch (error.code) {
      case "FILE_SIZE_EXCEEDED": return "Ảnh vượt quá 5 MB.";
      case "FILE_TYPE_NOT_ALLOWED": return "Chỉ chấp nhận ảnh JPEG, PNG hoặc WebP hợp lệ.";
      case "IMAGE_ALT_TEXT_INVALID": return "Mô tả ảnh không được quá 500 ký tự.";
      case "IMAGE_ORDER_INVALID": return "Thứ tự ảnh không hợp lệ.";
      case "IMAGE_NOT_FOUND": return "Ảnh không còn tồn tại. Hãy tải lại danh sách.";
      case "RECIPE_FORBIDDEN": return "Bạn không có quyền sửa ảnh của công thức này.";
      case "STORAGE_UNAVAILABLE": return "Kho ảnh đang không khả dụng. Hãy thử lại.";
      default: return error.problem.detail ?? "Không thể lưu thay đổi. Hãy thử lại.";
    }
  }
  return "Không thể kết nối. Hãy thử lại.";
}

export type RecipeImageManagerProps = {
  recipeId: string;
  initialImages?: RecipeImage[];
  onChange?: (images: RecipeImage[]) => void;
  onPendingChange?: (pending: boolean) => void;
};

/** Dùng sau khi recipe đã được tạo; TV2 chỉ cần truyền recipeId vào wizard/trang sửa. */
export function RecipeImageManager({ recipeId, initialImages, onChange, onPendingChange }: RecipeImageManagerProps) {
  const queryClient = useQueryClient();
  const queryKey = ["recipe-images", recipeId] as const;
  const { data: images, isPending, error: loadError } = useQuery({
    queryKey,
    queryFn: () => getRecipeImages(recipeId),
    initialData: initialImages,
  });
  const [uploads, setUploads] = useState<UploadItem[]>([]);
  const [dragging, setDragging] = useState(false);
  const [busyImageId, setBusyImageId] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const queueRef = useRef<UploadItem[]>([]);
  const processingRef = useRef(false);
  const activeControllerRef = useRef<AbortController | null>(null);
  const previewUrlsRef = useRef(new Set<string>());
  const onChangeRef = useRef(onChange);
  const onPendingChangeRef = useRef(onPendingChange);

  useEffect(() => {
    onChangeRef.current = onChange;
  }, [onChange]);

  useEffect(() => {
    onPendingChangeRef.current = onPendingChange;
  }, [onPendingChange]);

  useEffect(() => {
    if (images) onChangeRef.current?.(sortImages(images));
  }, [images]);

  useEffect(() => {
    onPendingChangeRef.current?.(uploads.some((item) => item.status !== "failed"));
  }, [uploads]);

  useEffect(() => {
    const previewUrls = previewUrlsRef.current;
    return () => {
      activeControllerRef.current?.abort();
      queueRef.current = [];
      previewUrls.forEach((url) => URL.revokeObjectURL(url));
      previewUrls.clear();
    };
  }, []);

  function updateUpload(id: string, patch: Partial<UploadItem>) {
    setUploads((current) => current.map((item) => item.id === id ? { ...item, ...patch } : item));
  }

  function releasePreview(url: string) {
    URL.revokeObjectURL(url);
    previewUrlsRef.current.delete(url);
  }

  async function processQueue() {
    if (processingRef.current) return;
    processingRef.current = true;
    try {
      while (queueRef.current.length > 0) {
        const item = queueRef.current.shift()!;
        const controller = new AbortController();
        activeControllerRef.current = controller;
        updateUpload(item.id, { status: "uploading", progress: 0, error: undefined });
        try {
          const uploaded = await uploadRecipeImage(recipeId, item.file,
            (progress) => updateUpload(item.id, { progress }), controller.signal);
          queryClient.setQueryData<RecipeImage[]>(queryKey, (current) =>
            sortImages([...(current ?? []), uploaded]));
          setUploads((current) => current.filter((upload) => upload.id !== item.id));
          releasePreview(item.previewUrl);
          setMessage(`Đã tải lên ${item.file.name}.`);
          void queryClient.invalidateQueries({ queryKey });
        } catch (uploadError) {
          if (!controller.signal.aborted) {
            updateUpload(item.id, { status: "failed", error: imageErrorMessage(uploadError) });
          }
        } finally {
          activeControllerRef.current = null;
        }
      }
    } finally {
      processingRef.current = false;
    }
  }

  function addFiles(files: FileList | File[]) {
    setError(null);
    setMessage(null);
    const accepted: UploadItem[] = [];
    const rejected: string[] = [];
    for (const file of Array.from(files)) {
      if (!ALLOWED_TYPES.has(file.type)) {
        rejected.push(`${file.name}: chỉ nhận JPEG, PNG hoặc WebP`);
      } else if (file.size > MAX_FILE_SIZE) {
        rejected.push(`${file.name}: vượt quá 5 MB`);
      } else {
        const previewUrl = URL.createObjectURL(file);
        previewUrlsRef.current.add(previewUrl);
        accepted.push({ id: crypto.randomUUID(), file, previewUrl, progress: 0, status: "queued" });
      }
    }
    if (rejected.length) setError(rejected.join("; "));
    if (accepted.length) {
      setUploads((current) => [...current, ...accepted]);
      queueRef.current.push(...accepted);
      void processQueue();
    }
  }

  function retryUpload(item: UploadItem) {
    updateUpload(item.id, { status: "queued", progress: 0, error: undefined });
    queueRef.current.push(item);
    void processQueue();
  }

  function removeFailedUpload(item: UploadItem) {
    setUploads((current) => current.filter((upload) => upload.id !== item.id));
    releasePreview(item.previewUrl);
  }

  async function patchImage(imageId: string, patch: Parameters<typeof updateRecipeImage>[2], success: string) {
    setBusyImageId(imageId);
    setError(null);
    setMessage(null);
    try {
      const updated = await updateRecipeImage(recipeId, imageId, patch);
      queryClient.setQueryData<RecipeImage[]>(queryKey, (current) =>
        sortImages((current ?? []).map((image) => image.imageId === imageId ? updated :
          patch.isPrimary ? { ...image, isPrimary: false } : image)));
      setMessage(success);
      void queryClient.invalidateQueries({ queryKey });
    } catch (patchError) {
      setError(imageErrorMessage(patchError));
    } finally {
      setBusyImageId(null);
    }
  }

  async function moveImage(image: RecipeImage, neighbor: RecipeImage) {
    setBusyImageId(image.imageId);
    setError(null);
    setMessage(null);
    try {
      await updateRecipeImage(recipeId, image.imageId, { orderIndex: neighbor.orderIndex });
      await updateRecipeImage(recipeId, neighbor.imageId, { orderIndex: image.orderIndex });
      setMessage("Đã đổi thứ tự ảnh.");
    } catch (moveError) {
      setError(imageErrorMessage(moveError));
    } finally {
      await queryClient.invalidateQueries({ queryKey });
      setBusyImageId(null);
    }
  }

  async function removeImage(image: RecipeImage) {
    if (!window.confirm("Xóa ảnh này khỏi công thức?")) return;
    setBusyImageId(image.imageId);
    setError(null);
    setMessage(null);
    try {
      await deleteRecipeImage(recipeId, image.imageId);
      queryClient.setQueryData<RecipeImage[]>(queryKey, (current) => {
        const remaining = sortImages((current ?? []).filter((item) => item.imageId !== image.imageId));
        return remaining.map((item, index) => image.isPrimary && index === 0
          ? { ...item, isPrimary: true }
          : item);
      });
      setMessage("Đã xóa ảnh.");
      void queryClient.invalidateQueries({ queryKey });
    } catch (deleteError) {
      setError(imageErrorMessage(deleteError));
    } finally {
      setBusyImageId(null);
    }
  }

  const orderedImages = sortImages(images ?? []);
  const busy = busyImageId !== null;

  return (
    <section aria-label="Ảnh công thức" className="space-y-5">
      <div>
        <h2 className="text-xl font-semibold">Ảnh công thức</h2>
        <p className="mt-1 text-sm text-gray-500">Tải nhiều ảnh JPEG, PNG hoặc WebP; mỗi ảnh tối đa 5 MB.</p>
      </div>

      <div
        onDragOver={(event) => { event.preventDefault(); setDragging(true); }}
        onDragLeave={(event) => {
          if (!(event.relatedTarget instanceof Node) || !event.currentTarget.contains(event.relatedTarget))
            setDragging(false);
        }}
        onDrop={(event) => {
          event.preventDefault();
          setDragging(false);
          addFiles(event.dataTransfer.files);
        }}
        className={`rounded-xl border-2 border-dashed p-6 text-center ${dragging ? "border-blue-500 bg-blue-50" : "border-gray-300"}`}
      >
        <p className="mb-3 text-sm">Kéo thả ảnh vào đây hoặc chọn từ máy</p>
        <button type="button" onClick={() => inputRef.current?.click()}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700">
          Chọn ảnh
        </button>
        <input ref={inputRef} type="file" multiple accept="image/jpeg,image/png,image/webp"
          className="sr-only" aria-label="Chọn ảnh công thức"
          onChange={(event) => {
            if (event.target.files) addFiles(event.target.files);
            event.target.value = "";
          }} />
      </div>

      {error && <p role="alert" className="rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</p>}
      {message && <p role="status" className="rounded-lg bg-green-50 p-3 text-sm text-green-700">{message}</p>}
      {loadError && <p role="alert" className="text-sm text-red-700">{imageErrorMessage(loadError)}</p>}

      {uploads.length > 0 && (
        <ul className="space-y-3" aria-label="Tiến trình tải ảnh">
          {uploads.map((item) => (
            <li key={item.id} className="flex items-center gap-3 rounded-lg border p-3">
              <img src={item.previewUrl} alt="Ảnh đang tải lên" className="h-16 w-16 rounded object-cover" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{item.file.name}</p>
                {item.status === "failed" ? (
                  <p role="alert" className="text-sm text-red-700">{item.error}</p>
                ) : (
                  <>
                    <progress value={item.progress} max={100} className="mt-2 h-2 w-full" />
                    <p className="text-xs text-gray-500">{item.status === "queued" ? "Đang chờ" : `Đang tải ${item.progress}%`}</p>
                  </>
                )}
              </div>
              {item.status === "failed" && (
                <div className="flex gap-2 text-sm">
                  <button type="button" onClick={() => retryUpload(item)} className="text-blue-700">Thử lại</button>
                  <button type="button" onClick={() => removeFailedUpload(item)} className="text-red-700">Bỏ</button>
                </div>
              )}
            </li>
          ))}
        </ul>
      )}

      {isPending ? (
        <div className="animate-pulse rounded-lg border p-6 text-sm text-gray-500">Đang tải danh sách ảnh…</div>
      ) : orderedImages.length === 0 ? (
        <p className="rounded-lg border p-6 text-sm text-gray-500">Công thức chưa có ảnh.</p>
      ) : (
        <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3" aria-label="Danh sách ảnh công thức">
          {orderedImages.map((image, index) => (
            <li key={image.imageId} className="overflow-hidden rounded-xl border">
              <img src={image.originalUrl} alt={image.altText || "Ảnh công thức"}
                className="aspect-video w-full object-cover" />
              <div className="space-y-3 p-3">
                <div className="flex items-center justify-between text-sm">
                  <span>{image.isPrimary ? "Ảnh chính" : `Ảnh ${index + 1}`}</span>
                  {!image.isPrimary && (
                    <button type="button" disabled={busy}
                      onClick={() => void patchImage(image.imageId, { isPrimary: true }, "Đã chọn ảnh chính.")}
                      className="text-blue-700 disabled:opacity-50">Chọn ảnh chính</button>
                  )}
                </div>
                <label className="block text-sm">
                  Mô tả ảnh
                  <input key={`${image.imageId}:${image.altText}`} type="text" maxLength={500}
                    defaultValue={image.altText ?? ""} disabled={busy}
                    onBlur={(event) => {
                      const value = event.target.value.trim();
                      if (value !== (image.altText ?? ""))
                        void patchImage(image.imageId, { altText: value || null }, "Đã lưu mô tả ảnh.");
                    }}
                    className="mt-1 w-full rounded-md border px-3 py-2 disabled:opacity-50" />
                </label>
                <div className="flex items-center justify-between text-sm">
                  <div className="flex gap-3">
                    <button type="button" disabled={busy || index === 0}
                      onClick={() => void moveImage(image, orderedImages[index - 1])}
                      className="text-blue-700 disabled:opacity-40">Lên</button>
                    <button type="button" disabled={busy || index === orderedImages.length - 1}
                      onClick={() => void moveImage(image, orderedImages[index + 1])}
                      className="text-blue-700 disabled:opacity-40">Xuống</button>
                  </div>
                  <button type="button" disabled={busy}
                    onClick={() => void removeImage(image)}
                    className="text-red-700 disabled:opacity-50">Xóa ảnh</button>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
