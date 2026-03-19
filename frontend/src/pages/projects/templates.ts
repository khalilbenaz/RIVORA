export interface TemplateEntity {
  name: string;
  fields: { name: string; type: string; required: boolean }[];
}

export interface ProjectTemplate {
  id: string;
  name: string;
  description: string;
  icon: string;
  category: 'saas' | 'ecommerce' | 'crm' | 'content' | 'internal' | 'api';
  color: string;
  features: string[];
  modules: string[];
  database: string;
  auth: string[];
  entities: TemplateEntity[];
  flows: string[];
  estimatedSetup: string;
}

export const projectTemplates: ProjectTemplate[] = [
  {
    id: 'saas-starter',
    name: 'SaaS Starter',
    description: 'Multi-tenant application with billing, user management, and subscription handling. Perfect for launching a SaaS product quickly.',
    icon: '\u{1F680}',
    category: 'saas',
    color: 'bg-violet-500',
    features: [
      'Multi-tenant architecture',
      'Stripe billing integration',
      'Subscription lifecycle management',
      'User invitation & onboarding',
      'Usage metering & limits',
      'Admin super-dashboard',
    ],
    modules: [
      'jwt', 'rbac', 'multi-tenancy', 'billing-stripe', 'onboarding',
      'rate-limiting', 'caching-redis', 'health-checks', 'opentelemetry',
      'email', 'webhooks', 'export-csv',
    ],
    database: 'postgresql',
    auth: ['JWT', 'RBAC', 'API Keys', '2FA/TOTP'],
    entities: [
      {
        name: 'Organization',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Name', type: 'string', required: true },
          { name: 'Slug', type: 'string', required: true },
          { name: 'Plan', type: 'string', required: true },
          { name: 'StripeCustomerId', type: 'string', required: false },
          { name: 'IsActive', type: 'bool', required: true },
          { name: 'CreatedAt', type: 'DateTime', required: true },
        ],
      },
      {
        name: 'Subscription',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'OrganizationId', type: 'Guid', required: true },
          { name: 'PlanName', type: 'string', required: true },
          { name: 'Status', type: 'enum', required: true },
          { name: 'PricePerMonth', type: 'decimal', required: true },
          { name: 'CurrentPeriodEnd', type: 'DateTime', required: true },
          { name: 'CancelAtPeriodEnd', type: 'bool', required: true },
        ],
      },
      {
        name: 'Invoice',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'OrganizationId', type: 'Guid', required: true },
          { name: 'Amount', type: 'decimal', required: true },
          { name: 'Currency', type: 'string', required: true },
          { name: 'Status', type: 'enum', required: true },
          { name: 'PaidAt', type: 'DateTime', required: false },
          { name: 'PdfUrl', type: 'string', required: false },
        ],
      },
    ],
    flows: [
      'User signup -> Create org -> Provision tenant -> Send welcome email',
      'Stripe webhook -> Update subscription status -> Notify admin',
      'Usage threshold reached -> Send warning email -> Enforce limits',
    ],
    estimatedSetup: '~15 min',
  },
  {
    id: 'ecommerce',
    name: 'E-commerce',
    description: 'Full-featured product catalog with shopping cart, order management, and payment processing via Stripe.',
    icon: '\u{1F6D2}',
    category: 'ecommerce',
    color: 'bg-emerald-500',
    features: [
      'Product catalog with variants',
      'Shopping cart & checkout',
      'Order lifecycle management',
      'Stripe payment processing',
      'Inventory tracking',
      'PDF invoice generation',
    ],
    modules: [
      'jwt', 'rbac', 'billing-stripe', 'caching-redis', 'search-elasticsearch',
      'email', 'export-pdf', 'export-csv', 'webhooks', 'health-checks',
      'rate-limiting', 'jobs-quartz',
    ],
    database: 'postgresql',
    auth: ['JWT', 'RBAC', 'API Keys'],
    entities: [
      {
        name: 'Product',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Name', type: 'string', required: true },
          { name: 'Description', type: 'string', required: false },
          { name: 'Price', type: 'decimal', required: true },
          { name: 'Sku', type: 'string', required: true },
          { name: 'Stock', type: 'int', required: true },
          { name: 'Category', type: 'string', required: true },
          { name: 'IsPublished', type: 'bool', required: true },
        ],
      },
      {
        name: 'Order',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'CustomerId', type: 'Guid', required: true },
          { name: 'Status', type: 'enum', required: true },
          { name: 'TotalAmount', type: 'decimal', required: true },
          { name: 'ShippingAddress', type: 'string', required: true },
          { name: 'PlacedAt', type: 'DateTime', required: true },
          { name: 'ShippedAt', type: 'DateTime', required: false },
        ],
      },
      {
        name: 'CartItem',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'CartId', type: 'Guid', required: true },
          { name: 'ProductId', type: 'Guid', required: true },
          { name: 'Quantity', type: 'int', required: true },
          { name: 'UnitPrice', type: 'decimal', required: true },
        ],
      },
      {
        name: 'Payment',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'OrderId', type: 'Guid', required: true },
          { name: 'Amount', type: 'decimal', required: true },
          { name: 'Currency', type: 'string', required: true },
          { name: 'StripePaymentIntentId', type: 'string', required: false },
          { name: 'Status', type: 'enum', required: true },
          { name: 'PaidAt', type: 'DateTime', required: false },
        ],
      },
    ],
    flows: [
      'Order placed -> Process payment -> Send confirmation email -> Update inventory',
      'Payment failed -> Retry 3x -> Notify customer -> Cancel order',
      'Low stock threshold -> Notify admin -> Generate reorder report',
    ],
    estimatedSetup: '~20 min',
  },
  {
    id: 'crm',
    name: 'CRM',
    description: 'Customer relationship management with contact tracking, deal pipeline, and activity logging for sales teams.',
    icon: '\u{1F4C8}',
    category: 'crm',
    color: 'bg-blue-500',
    features: [
      'Contact & company management',
      'Deal pipeline with stages',
      'Activity timeline',
      'Email integration',
      'Reporting dashboards',
      'CSV import/export',
    ],
    modules: [
      'jwt', 'rbac', 'caching-redis', 'search-elasticsearch', 'email',
      'export-csv', 'export-excel', 'webhooks', 'health-checks',
      'opentelemetry', 'rate-limiting',
    ],
    database: 'postgresql',
    auth: ['JWT', 'RBAC', '2FA/TOTP'],
    entities: [
      {
        name: 'Contact',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'FirstName', type: 'string', required: true },
          { name: 'LastName', type: 'string', required: true },
          { name: 'Email', type: 'string', required: true },
          { name: 'Phone', type: 'string', required: false },
          { name: 'Company', type: 'string', required: false },
          { name: 'Status', type: 'enum', required: true },
          { name: 'CreatedAt', type: 'DateTime', required: true },
        ],
      },
      {
        name: 'Deal',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Title', type: 'string', required: true },
          { name: 'ContactId', type: 'Guid', required: true },
          { name: 'PipelineId', type: 'Guid', required: true },
          { name: 'Stage', type: 'string', required: true },
          { name: 'Value', type: 'decimal', required: true },
          { name: 'ExpectedCloseDate', type: 'DateTime', required: false },
          { name: 'Probability', type: 'int', required: true },
        ],
      },
      {
        name: 'Pipeline',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Name', type: 'string', required: true },
          { name: 'Stages', type: 'string', required: true },
          { name: 'IsDefault', type: 'bool', required: true },
        ],
      },
      {
        name: 'Activity',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'ContactId', type: 'Guid', required: true },
          { name: 'DealId', type: 'Guid', required: false },
          { name: 'Type', type: 'enum', required: true },
          { name: 'Description', type: 'string', required: true },
          { name: 'DueDate', type: 'DateTime', required: false },
          { name: 'IsCompleted', type: 'bool', required: true },
        ],
      },
    ],
    flows: [
      'New contact created -> Enrich data -> Assign to sales rep -> Send intro email',
      'Deal stage changed -> Update forecast -> Notify manager',
      'Activity overdue -> Send reminder -> Escalate if ignored',
    ],
    estimatedSetup: '~15 min',
  },
  {
    id: 'blog-cms',
    name: 'Blog / CMS',
    description: 'Content management system with articles, categories, media uploads, and a comment system. Ideal for blogs and editorial sites.',
    icon: '\u{1F4DD}',
    category: 'content',
    color: 'bg-orange-500',
    features: [
      'Rich text editor for articles',
      'Category & tag management',
      'Media library with uploads',
      'Comment moderation',
      'SEO metadata',
      'RSS feed generation',
    ],
    modules: [
      'jwt', 'rbac', 'caching-redis', 'search-elasticsearch', 'email',
      'export-pdf', 'webhooks', 'health-checks', 'rate-limiting',
    ],
    database: 'postgresql',
    auth: ['JWT', 'RBAC'],
    entities: [
      {
        name: 'Article',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Title', type: 'string', required: true },
          { name: 'Slug', type: 'string', required: true },
          { name: 'Content', type: 'string', required: true },
          { name: 'Excerpt', type: 'string', required: false },
          { name: 'AuthorId', type: 'Guid', required: true },
          { name: 'CategoryId', type: 'Guid', required: true },
          { name: 'Status', type: 'enum', required: true },
          { name: 'PublishedAt', type: 'DateTime', required: false },
        ],
      },
      {
        name: 'Category',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Name', type: 'string', required: true },
          { name: 'Slug', type: 'string', required: true },
          { name: 'Description', type: 'string', required: false },
          { name: 'ParentId', type: 'Guid', required: false },
        ],
      },
      {
        name: 'Media',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'FileName', type: 'string', required: true },
          { name: 'MimeType', type: 'string', required: true },
          { name: 'SizeBytes', type: 'int', required: true },
          { name: 'Url', type: 'string', required: true },
          { name: 'AltText', type: 'string', required: false },
          { name: 'UploadedAt', type: 'DateTime', required: true },
        ],
      },
      {
        name: 'Comment',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'ArticleId', type: 'Guid', required: true },
          { name: 'AuthorName', type: 'string', required: true },
          { name: 'AuthorEmail', type: 'string', required: true },
          { name: 'Body', type: 'string', required: true },
          { name: 'IsApproved', type: 'bool', required: true },
          { name: 'CreatedAt', type: 'DateTime', required: true },
        ],
      },
    ],
    flows: [
      'Article published -> Clear cache -> Send newsletter -> Post to social media',
      'New comment -> Spam check -> If clean, notify author -> Moderate',
    ],
    estimatedSetup: '~12 min',
  },
  {
    id: 'internal-tools',
    name: 'Internal Tools',
    description: 'Back-office dashboard with admin panels, configurable reports, and widgets for internal team use.',
    icon: '\u{1F6E0}\u{FE0F}',
    category: 'internal',
    color: 'bg-slate-500',
    features: [
      'Customizable dashboard',
      'Drag-and-drop widget layout',
      'Report builder with filters',
      'Data export (PDF/Excel/CSV)',
      'Role-based access control',
      'Audit trail',
    ],
    modules: [
      'jwt', 'rbac', '2fa-totp', 'caching-redis', 'export-pdf',
      'export-excel', 'export-csv', 'health-checks', 'opentelemetry',
      'jobs-quartz', 'rate-limiting',
    ],
    database: 'postgresql',
    auth: ['JWT', 'RBAC', '2FA/TOTP'],
    entities: [
      {
        name: 'Report',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Name', type: 'string', required: true },
          { name: 'Description', type: 'string', required: false },
          { name: 'Query', type: 'string', required: true },
          { name: 'Schedule', type: 'string', required: false },
          { name: 'Format', type: 'enum', required: true },
          { name: 'CreatedBy', type: 'Guid', required: true },
          { name: 'LastRunAt', type: 'DateTime', required: false },
        ],
      },
      {
        name: 'Widget',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'DashboardId', type: 'Guid', required: true },
          { name: 'Type', type: 'enum', required: true },
          { name: 'Title', type: 'string', required: true },
          { name: 'Config', type: 'string', required: true },
          { name: 'PositionX', type: 'int', required: true },
          { name: 'PositionY', type: 'int', required: true },
          { name: 'Width', type: 'int', required: true },
          { name: 'Height', type: 'int', required: true },
        ],
      },
      {
        name: 'Dashboard',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Name', type: 'string', required: true },
          { name: 'Description', type: 'string', required: false },
          { name: 'OwnerId', type: 'Guid', required: true },
          { name: 'IsPublic', type: 'bool', required: true },
          { name: 'CreatedAt', type: 'DateTime', required: true },
        ],
      },
    ],
    flows: [
      'Scheduled report -> Generate data -> Export to PDF -> Email recipients',
      'Dashboard shared -> Validate permissions -> Grant read access',
    ],
    estimatedSetup: '~10 min',
  },
  {
    id: 'api-backend',
    name: 'API Backend',
    description: 'Pure REST API backend with no frontend UI. Includes authentication, rate limiting, API key management, and full OpenAPI documentation.',
    icon: '\u{1F50C}',
    category: 'api',
    color: 'bg-cyan-500',
    features: [
      'RESTful API with OpenAPI/Swagger',
      'API key management',
      'Rate limiting per client',
      'Request/response logging',
      'Health check endpoints',
      'Webhook event dispatch',
    ],
    modules: [
      'jwt', 'api-keys', 'rate-limiting', 'caching-redis', 'webhooks',
      'health-checks', 'opentelemetry', 'event-sourcing', 'jobs-quartz',
    ],
    database: 'postgresql',
    auth: ['JWT', 'API Keys', 'Rate Limiting'],
    entities: [
      {
        name: 'Resource',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Type', type: 'string', required: true },
          { name: 'Data', type: 'string', required: true },
          { name: 'Version', type: 'int', required: true },
          { name: 'CreatedAt', type: 'DateTime', required: true },
          { name: 'UpdatedAt', type: 'DateTime', required: true },
        ],
      },
      {
        name: 'ApiClient',
        fields: [
          { name: 'Id', type: 'Guid', required: true },
          { name: 'Name', type: 'string', required: true },
          { name: 'ApiKey', type: 'string', required: true },
          { name: 'RateLimit', type: 'int', required: true },
          { name: 'IsActive', type: 'bool', required: true },
          { name: 'LastUsedAt', type: 'DateTime', required: false },
          { name: 'CreatedAt', type: 'DateTime', required: true },
        ],
      },
    ],
    flows: [
      'API request -> Validate key -> Rate limit check -> Process -> Log event',
      'Rate limit exceeded -> Throttle response -> Alert admin if persistent',
    ],
    estimatedSetup: '~8 min',
  },
];

export const categoryLabels: Record<ProjectTemplate['category'], string> = {
  saas: 'SaaS',
  ecommerce: 'E-commerce',
  crm: 'CRM',
  content: 'Content',
  internal: 'Internal',
  api: 'API',
};

export const allModules = {
  security: {
    label: 'Security',
    modules: [
      { id: 'jwt', name: 'JWT Authentication', description: 'JSON Web Token based auth' },
      { id: '2fa-totp', name: '2FA / TOTP', description: 'Two-factor authentication with TOTP' },
      { id: 'api-keys', name: 'API Keys', description: 'API key generation & validation' },
      { id: 'rate-limiting', name: 'Rate Limiting', description: 'Request rate limiting per client' },
      { id: 'rbac', name: 'RBAC', description: 'Role-based access control' },
    ],
  },
  data: {
    label: 'Data',
    modules: [
      { id: 'caching-redis', name: 'Caching (Redis)', description: 'Distributed caching with Redis' },
      { id: 'search-elasticsearch', name: 'Search (Elasticsearch)', description: 'Full-text search engine' },
      { id: 'event-sourcing', name: 'Event Sourcing', description: 'Event-driven state management' },
    ],
  },
  integration: {
    label: 'Integration',
    modules: [
      { id: 'webhooks', name: 'Webhooks', description: 'Outgoing webhook dispatch' },
      { id: 'email', name: 'Email', description: 'Transactional email sending' },
      { id: 'sms', name: 'SMS', description: 'SMS notifications' },
      { id: 'export-pdf', name: 'Export PDF', description: 'PDF document generation' },
      { id: 'export-excel', name: 'Export Excel', description: 'Excel spreadsheet export' },
      { id: 'export-csv', name: 'Export CSV', description: 'CSV data export' },
    ],
  },
  saas: {
    label: 'SaaS',
    modules: [
      { id: 'multi-tenancy', name: 'Multi-tenancy', description: 'Tenant isolation & management' },
      { id: 'billing-stripe', name: 'Billing (Stripe)', description: 'Stripe payment & subscription' },
      { id: 'onboarding', name: 'Onboarding', description: 'User onboarding flows' },
    ],
  },
  ai: {
    label: 'AI',
    modules: [
      { id: 'rag', name: 'RAG', description: 'Retrieval-augmented generation' },
      { id: 'natural-query', name: 'Natural Query', description: 'Natural language to SQL/API' },
      { id: 'ai-agents', name: 'AI Agents', description: 'Autonomous AI agent framework' },
    ],
  },
  devops: {
    label: 'DevOps',
    modules: [
      { id: 'health-checks', name: 'Health Checks', description: 'Liveness & readiness probes' },
      { id: 'opentelemetry', name: 'OpenTelemetry', description: 'Distributed tracing & metrics' },
      { id: 'jobs-quartz', name: 'Jobs (Quartz)', description: 'Background job scheduling' },
    ],
  },
};

export const databaseOptions = [
  { id: 'postgresql', name: 'PostgreSQL', icon: '\u{1F418}', description: 'Recommended for most projects' },
  { id: 'sqlserver', name: 'SQL Server', icon: '\u{1F4CA}', description: 'Enterprise Microsoft stack' },
  { id: 'mysql', name: 'MySQL', icon: '\u{1F42C}', description: 'Popular open-source database' },
  { id: 'sqlite', name: 'SQLite', icon: '\u{1F4C4}', description: 'Lightweight, file-based' },
];

export const fieldTypes = [
  'string', 'int', 'decimal', 'bool', 'DateTime', 'Guid', 'enum',
];
