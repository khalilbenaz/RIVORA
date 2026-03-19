import { useState, useRef, useCallback, useEffect } from 'react';
import {
  Upload,
  Trash2,
  Search,
  Grid,
  List,
  Image as ImageIcon,
  FileText,
  File,
  FolderOpen,
  ChevronRight,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { filesApi, type FileItem } from '../api/files';
import { formatFileSize } from '../utils/formatFileSize';
import Spinner from '../components/Spinner';

function getFileIcon(mimeType: string) {
  if (mimeType.startsWith('image/')) return ImageIcon;
  if (mimeType === 'application/pdf') return FileText;
  return File;
}

export default function FilesPage() {
  const { t } = useTranslation();
  const [files, setFiles] = useState<FileItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [searchQuery, setSearchQuery] = useState('');
  const [currentFolder, setCurrentFolder] = useState<string | undefined>(undefined);
  const [dragOver, setDragOver] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);
  const [selectedFile, setSelectedFile] = useState<FileItem | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const breadcrumbs = currentFolder ? currentFolder.split('/') : [];

  const fetchFiles = useCallback(async () => {
    setLoading(true);
    try {
      const res = await filesApi.getAll(currentFolder);
      setFiles(res.data);
    } catch {
      setFiles([]);
    } finally {
      setLoading(false);
    }
  }, [currentFolder]);

  useEffect(() => {
    fetchFiles();
  }, [fetchFiles]);

  const handleUpload = async (fileList: FileList | null) => {
    if (!fileList || fileList.length === 0) return;
    setUploading(true);
    setUploadProgress(0);

    // Simulate progress for UX
    const interval = setInterval(() => {
      setUploadProgress((prev) => Math.min(prev + 10, 90));
    }, 200);

    try {
      for (let i = 0; i < fileList.length; i++) {
        const f = fileList[i];
        if (f) await filesApi.upload(f, currentFolder);
      }
      setUploadProgress(100);
      await fetchFiles();
    } catch {
      // Error handled silently
    } finally {
      clearInterval(interval);
      setTimeout(() => {
        setUploading(false);
        setUploadProgress(0);
      }, 500);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await filesApi.delete(id);
      setFiles((prev) => prev.filter((f) => f.id !== id));
      setDeleteConfirm(null);
      if (selectedFile?.id === id) setSelectedFile(null);
    } catch {
      // Error handled silently
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    handleUpload(e.dataTransfer.files);
  };

  const navigateToFolder = (folderPath: string | undefined) => {
    setCurrentFolder(folderPath);
    setSelectedFile(null);
  };

  const filteredFiles = files.filter((f) =>
    f.name.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  return (
    <div>
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-slate-900">{t('files.title')}</h1>
        <div className="flex items-center gap-2">
          <div className="relative">
            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder={t('common.search')}
              className="rounded-lg border border-slate-200 bg-white py-2 pl-9 pr-3 text-sm outline-none transition-colors focus:border-blue-400 focus:ring-1 focus:ring-blue-400"
            />
          </div>
          <button
            onClick={() => setViewMode(viewMode === 'grid' ? 'list' : 'grid')}
            className="rounded-lg border border-slate-200 bg-white p-2 text-slate-500 hover:text-slate-700"
            title={viewMode === 'grid' ? 'List view' : 'Grid view'}
          >
            {viewMode === 'grid' ? <List size={18} /> : <Grid size={18} />}
          </button>
          <button
            onClick={() => inputRef.current?.click()}
            className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700"
          >
            <Upload size={16} />
            {t('files.upload')}
          </button>
          <input
            ref={inputRef}
            type="file"
            multiple
            className="hidden"
            onChange={(e) => handleUpload(e.target.files)}
          />
        </div>
      </div>

      {/* Breadcrumb */}
      {breadcrumbs.length > 0 && (
        <nav className="mb-4 flex items-center gap-1 text-sm text-slate-500">
          <button
            onClick={() => navigateToFolder(undefined)}
            className="hover:text-blue-600"
          >
            {t('files.root')}
          </button>
          {breadcrumbs.map((crumb, i) => {
            const path = breadcrumbs.slice(0, i + 1).join('/');
            return (
              <span key={path} className="flex items-center gap-1">
                <ChevronRight size={14} />
                <button
                  onClick={() => navigateToFolder(path)}
                  className="hover:text-blue-600"
                >
                  {crumb}
                </button>
              </span>
            );
          })}
        </nav>
      )}

      {/* Upload progress */}
      {uploading && (
        <div className="mb-4 overflow-hidden rounded-full bg-slate-200">
          <div
            className="h-2 rounded-full bg-blue-600 transition-all duration-300"
            style={{ width: `${uploadProgress}%` }}
          />
        </div>
      )}

      {/* Drop zone / file area */}
      <div
        onDragOver={(e) => {
          e.preventDefault();
          setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={handleDrop}
        className={`min-h-[300px] rounded-xl border-2 transition-colors ${
          dragOver
            ? 'border-blue-400 bg-blue-50'
            : 'border-dashed border-slate-300 bg-white'
        }`}
      >
        {loading ? (
          <div className="flex h-64 items-center justify-center">
            <Spinner />
          </div>
        ) : filteredFiles.length === 0 ? (
          <div className="flex h-64 flex-col items-center justify-center gap-3 text-slate-400">
            <FolderOpen size={48} strokeWidth={1.2} />
            <p className="text-sm">{t('files.emptyState')}</p>
          </div>
        ) : viewMode === 'grid' ? (
          <div className="grid grid-cols-2 gap-4 p-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6">
            {filteredFiles.map((file) => {
              const Icon = getFileIcon(file.mimeType);
              const isImage = file.mimeType.startsWith('image/');
              return (
                <div
                  key={file.id}
                  onClick={() => setSelectedFile(file)}
                  className={`group relative cursor-pointer rounded-lg border p-3 transition-all hover:shadow-md ${
                    selectedFile?.id === file.id
                      ? 'border-blue-400 bg-blue-50'
                      : 'border-slate-200 bg-white'
                  }`}
                >
                  <div className="mb-2 flex h-20 items-center justify-center overflow-hidden rounded-md bg-slate-50">
                    {isImage && file.thumbnailUrl ? (
                      <img
                        src={file.thumbnailUrl}
                        alt={file.name}
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <Icon size={32} className="text-slate-400" />
                    )}
                  </div>
                  <p className="truncate text-xs font-medium text-slate-700" title={file.name}>
                    {file.name}
                  </p>
                  <p className="text-[10px] text-slate-400">
                    {formatFileSize(file.size)}
                  </p>
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      setDeleteConfirm(file.id);
                    }}
                    className="absolute right-1.5 top-1.5 hidden rounded p-1 text-slate-400 transition-colors hover:bg-red-50 hover:text-red-500 group-hover:block"
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
              );
            })}
          </div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-slate-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {t('files.name')}
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {t('files.size')}
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {t('files.type')}
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {t('files.uploadedBy')}
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {t('common.date')}
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                  {t('common.actions')}
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filteredFiles.map((file) => {
                const Icon = getFileIcon(file.mimeType);
                return (
                  <tr
                    key={file.id}
                    onClick={() => setSelectedFile(file)}
                    className={`cursor-pointer transition-colors hover:bg-slate-50 ${
                      selectedFile?.id === file.id ? 'bg-blue-50' : ''
                    }`}
                  >
                    <td className="flex items-center gap-2 px-4 py-2.5">
                      <Icon size={16} className="text-slate-400" />
                      <span className="truncate font-medium text-slate-700">{file.name}</span>
                    </td>
                    <td className="px-4 py-2.5 tabular-nums text-slate-600">
                      {formatFileSize(file.size)}
                    </td>
                    <td className="px-4 py-2.5 text-slate-500">{file.mimeType}</td>
                    <td className="px-4 py-2.5 text-slate-500">{file.uploadedBy}</td>
                    <td className="px-4 py-2.5 text-slate-500">
                      {new Date(file.uploadedAt).toLocaleDateString()}
                    </td>
                    <td className="px-4 py-2.5 text-right">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          setDeleteConfirm(file.id);
                        }}
                        className="rounded p-1 text-slate-400 transition-colors hover:bg-red-50 hover:text-red-500"
                      >
                        <Trash2 size={14} />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>

      {/* File detail panel */}
      {selectedFile && (
        <div className="mt-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h3 className="mb-3 text-sm font-semibold text-slate-800">{t('files.details')}</h3>
          <dl className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm sm:grid-cols-3">
            <div>
              <dt className="text-xs text-slate-500">{t('files.name')}</dt>
              <dd className="truncate font-medium text-slate-700">{selectedFile.name}</dd>
            </div>
            <div>
              <dt className="text-xs text-slate-500">{t('files.size')}</dt>
              <dd className="font-medium text-slate-700">{formatFileSize(selectedFile.size)}</dd>
            </div>
            <div>
              <dt className="text-xs text-slate-500">{t('files.type')}</dt>
              <dd className="font-medium text-slate-700">{selectedFile.mimeType}</dd>
            </div>
            <div>
              <dt className="text-xs text-slate-500">{t('files.uploadedBy')}</dt>
              <dd className="font-medium text-slate-700">{selectedFile.uploadedBy}</dd>
            </div>
            <div>
              <dt className="text-xs text-slate-500">{t('common.date')}</dt>
              <dd className="font-medium text-slate-700">
                {new Date(selectedFile.uploadedAt).toLocaleString()}
              </dd>
            </div>
          </dl>
        </div>
      )}

      {/* Delete confirmation modal */}
      {deleteConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="w-full max-w-sm rounded-xl bg-white p-6 shadow-xl">
            <h3 className="mb-2 text-lg font-semibold text-slate-800">{t('common.confirm')}</h3>
            <p className="mb-4 text-sm text-slate-600">{t('files.deleteConfirm')}</p>
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setDeleteConfirm(null)}
                className="rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-50"
              >
                {t('common.cancel')}
              </button>
              <button
                onClick={() => handleDelete(deleteConfirm)}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700"
              >
                {t('common.delete')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
