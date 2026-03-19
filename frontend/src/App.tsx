import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { useEffect } from 'react';
import { useAuthStore } from './store/authStore';
import ErrorBoundary from './components/ErrorBoundary';
import ToastContainer from './components/ToastContainer';

// Admin BO
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import UsersPage from './pages/UsersPage';
import ProductsPage from './pages/ProductsPage';
import AuditLogsPage from './pages/AuditLogsPage';
import HealthPage from './pages/HealthPage';
import TenantsPage from './pages/TenantsPage';
import RolesPage from './pages/RolesPage';
import WebhooksPage from './pages/WebhooksPage';
import ChatPage from './pages/ChatPage';
import FilesPage from './pages/FilesPage';
import AnalyticsPage from './pages/AnalyticsPage';
import CalendarPage from './pages/CalendarPage';
import NotesPage from './pages/NotesPage';
import ActivityPage from './pages/ActivityPage';
import FlowListPage from './pages/flows/FlowListPage';
import FlowEditorPage from './pages/FlowEditorPage';
import ProjectListPage from './pages/projects/ProjectListPage';
import ProjectWizard from './pages/projects/ProjectWizard';
import EntityDesigner from './pages/generator/EntityDesigner';
import KanbanPage from './pages/KanbanPage';
import Login from './pages/Login';

// Landing
import LandingPage from './pages/landing/LandingPage';

// Showcase
import ComponentShowcase from './pages/ComponentShowcase';

// Client SaaS
import ClientLayout from './components/client/ClientLayout';
import ClientLogin from './pages/client/ClientLogin';
import ClientRegister from './pages/client/ClientRegister';
import ClientDashboard from './pages/client/ClientDashboard';
import ClientSettings from './pages/client/ClientSettings';

export default function App() {
  const loadFromStorage = useAuthStore((s) => s.loadFromStorage);

  useEffect(() => {
    loadFromStorage();
  }, [loadFromStorage]);

  return (
    <ErrorBoundary>
    <BrowserRouter>
      <Routes>
        {/* Public landing page */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/components" element={<ComponentShowcase />} />

        {/* Client SaaS app */}
        <Route path="/app/login" element={<ClientLogin />} />
        <Route path="/app/register" element={<ClientRegister />} />
        <Route path="/app" element={<ClientLayout />}>
          <Route index element={<ClientDashboard />} />
          <Route path="settings" element={<ClientSettings />} />
        </Route>

        {/* Admin Back Office */}
        <Route path="/admin/login" element={<Login />} />
        <Route path="/admin" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="products" element={<ProductsPage />} />
          <Route path="tenants" element={<TenantsPage />} />
          <Route path="audit" element={<AuditLogsPage />} />
          <Route path="health" element={<HealthPage />} />
          <Route path="roles" element={<RolesPage />} />
          <Route path="webhooks" element={<WebhooksPage />} />
          <Route path="chat" element={<ChatPage />} />
          <Route path="files" element={<FilesPage />} />
          <Route path="analytics" element={<AnalyticsPage />} />
          <Route path="calendar" element={<CalendarPage />} />
          <Route path="notes" element={<NotesPage />} />
          <Route path="activity" element={<ActivityPage />} />
          <Route path="flows" element={<FlowListPage />} />
          <Route path="flows/new" element={<FlowEditorPage />} />
          <Route path="flows/:id" element={<FlowEditorPage />} />
          <Route path="projects" element={<ProjectListPage />} />
          <Route path="projects/new" element={<ProjectWizard />} />
          <Route path="generator" element={<EntityDesigner />} />
          <Route path="kanban" element={<KanbanPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
    <ToastContainer />
    </ErrorBoundary>
  );
}
