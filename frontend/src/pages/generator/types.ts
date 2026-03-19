export type FieldType = 'string' | 'int' | 'decimal' | 'bool' | 'DateTime' | 'Guid' | 'enum' | 'relation';

export interface EntityField {
  id: string;
  name: string;
  type: FieldType;
  required: boolean;
  maxLength?: number;
  enumValues?: string[];
  relationTarget?: string;
  relationType?: 'one-to-one' | 'one-to-many' | 'many-to-many';
  isSearchable: boolean;
  isFilterable: boolean;
  showInList: boolean;
  showInForm: boolean;
}

export interface EntityDefinition {
  id: string;
  name: string;
  pluralName: string;
  description?: string;
  fields: EntityField[];
  hasAudit: boolean;
  hasSoftDelete: boolean;
  hasTenantId: boolean;
  apiPrefix: string;
}

export interface GeneratedCode {
  fileName: string;
  layer: 'Domain' | 'Application' | 'Infrastructure' | 'API' | 'Frontend';
  code: string;
  language: 'csharp' | 'typescript';
}
