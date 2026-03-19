import type { EntityDefinition, EntityField, GeneratedCode } from './types';

function csType(f: EntityField): string {
  switch (f.type) {
    case 'string': return 'string';
    case 'int': return 'int';
    case 'decimal': return 'decimal';
    case 'bool': return 'bool';
    case 'DateTime': return 'DateTime';
    case 'Guid': return 'Guid';
    case 'enum': return f.name + 'Type';
    case 'relation': return f.relationTarget ? (f.relationType === 'many-to-many' || f.relationType === 'one-to-many' ? `ICollection<${f.relationTarget}>` : f.relationTarget) : 'object';
    default: return 'string';
  }
}

function tsType(f: EntityField): string {
  switch (f.type) {
    case 'string': return 'string';
    case 'int': case 'decimal': return 'number';
    case 'bool': return 'boolean';
    case 'DateTime': return 'string';
    case 'Guid': return 'string';
    case 'enum': return f.enumValues?.map(v => `'${v}'`).join(' | ') || 'string';
    case 'relation': return f.relationTarget ? (f.relationType === 'many-to-many' || f.relationType === 'one-to-many' ? `${f.relationTarget}[]` : f.relationTarget) : 'any';
    default: return 'string';
  }
}

function nullable(f: EntityField, type: string): string {
  return f.required ? type : `${type}?`;
}

export function generateDomainEntity(entity: EntityDefinition): GeneratedCode {
  const lines: string[] = [];
  lines.push('using System;');
  lines.push('using System.Collections.Generic;');
  lines.push('');
  lines.push(`namespace Domain.Entities;`);
  lines.push('');

  // Generate enum types
  entity.fields.filter(f => f.type === 'enum').forEach(f => {
    lines.push(`public enum ${f.name}Type`);
    lines.push('{');
    (f.enumValues || []).forEach((v, i) => {
      lines.push(`    ${v}${i < (f.enumValues?.length || 0) - 1 ? ',' : ''}`);
    });
    lines.push('}');
    lines.push('');
  });

  lines.push(`public class ${entity.name}`);
  lines.push('{');
  lines.push('    public Guid Id { get; set; }');
  lines.push('');

  entity.fields.forEach(f => {
    if (f.type !== 'relation') {
      lines.push(`    public ${nullable(f, csType(f))} ${f.name} { get; set; }${f.type === 'string' && !f.required ? ' = string.Empty;' : ''}`);
    }
  });

  // Relation navigation properties
  entity.fields.filter(f => f.type === 'relation').forEach(f => {
    lines.push('');
    if (f.relationType === 'one-to-many' || f.relationType === 'many-to-many') {
      lines.push(`    public virtual ICollection<${f.relationTarget}> ${f.name} { get; set; } = new List<${f.relationTarget}>();`);
    } else {
      lines.push(`    public Guid? ${f.name}Id { get; set; }`);
      lines.push(`    public virtual ${f.relationTarget}? ${f.name} { get; set; }`);
    }
  });

  if (entity.hasAudit) {
    lines.push('');
    lines.push('    // Audit fields');
    lines.push('    public DateTime CreatedAt { get; set; }');
    lines.push('    public string? CreatedBy { get; set; }');
    lines.push('    public DateTime? UpdatedAt { get; set; }');
    lines.push('    public string? UpdatedBy { get; set; }');
  }

  if (entity.hasSoftDelete) {
    lines.push('');
    lines.push('    // Soft delete');
    lines.push('    public bool IsDeleted { get; set; }');
    lines.push('    public DateTime? DeletedAt { get; set; }');
  }

  if (entity.hasTenantId) {
    lines.push('');
    lines.push('    // Multi-tenant');
    lines.push('    public Guid TenantId { get; set; }');
  }

  lines.push('}');

  return {
    fileName: `${entity.name}.cs`,
    layer: 'Domain',
    code: lines.join('\n'),
    language: 'csharp',
  };
}

export function generateDto(entity: EntityDefinition): GeneratedCode {
  const formFields = entity.fields.filter(f => f.showInForm && f.type !== 'relation');
  const lines: string[] = [];
  lines.push(`namespace Application.DTOs.${entity.name};`);
  lines.push('');

  // CreateDto
  lines.push(`public record Create${entity.name}Dto(`);
  formFields.forEach((f, i) => {
    const comma = i < formFields.length - 1 ? ',' : '';
    lines.push(`    ${nullable(f, csType(f))} ${f.name}${comma}`);
  });
  lines.push(');');
  lines.push('');

  // UpdateDto
  lines.push(`public record Update${entity.name}Dto(`);
  formFields.forEach((f, i) => {
    const comma = i < formFields.length - 1 ? ',' : '';
    lines.push(`    ${nullable(f, csType(f))} ${f.name}${comma}`);
  });
  lines.push(');');
  lines.push('');

  // ResponseDto
  const listFields = entity.fields.filter(f => f.showInList && f.type !== 'relation');
  lines.push(`public record ${entity.name}ResponseDto(`);
  lines.push('    Guid Id,');
  listFields.forEach((f, i) => {
    const comma = i < listFields.length - 1 ? ',' : '';
    lines.push(`    ${nullable(f, csType(f))} ${f.name}${comma}`);
  });
  if (entity.hasAudit) {
    lines.push('    ,DateTime CreatedAt');
    lines.push('    ,string? CreatedBy');
  }
  lines.push(');');

  return {
    fileName: `${entity.name}Dtos.cs`,
    layer: 'Application',
    code: lines.join('\n'),
    language: 'csharp',
  };
}

export function generateValidator(entity: EntityDefinition): GeneratedCode {
  const lines: string[] = [];
  lines.push('using FluentValidation;');
  lines.push(`using Application.DTOs.${entity.name};`);
  lines.push('');
  lines.push(`namespace Application.Validators;`);
  lines.push('');
  lines.push(`public class Create${entity.name}Validator : AbstractValidator<Create${entity.name}Dto>`);
  lines.push('{');
  lines.push(`    public Create${entity.name}Validator()`);
  lines.push('    {');

  entity.fields.filter(f => f.showInForm && f.type !== 'relation').forEach(f => {
    if (f.required) {
      lines.push(`        RuleFor(x => x.${f.name}).NotEmpty();`);
    }
    if (f.type === 'string' && f.maxLength) {
      lines.push(`        RuleFor(x => x.${f.name}).MaximumLength(${f.maxLength});`);
    }
    if (f.type === 'decimal') {
      lines.push(`        RuleFor(x => x.${f.name}).GreaterThanOrEqualTo(0);`);
    }
  });

  lines.push('    }');
  lines.push('}');

  return {
    fileName: `Create${entity.name}Validator.cs`,
    layer: 'Application',
    code: lines.join('\n'),
    language: 'csharp',
  };
}

export function generateRepository(entity: EntityDefinition): GeneratedCode {
  const lines: string[] = [];
  lines.push(`using Domain.Entities;`);
  lines.push('');
  lines.push(`namespace Infrastructure.Repositories;`);
  lines.push('');
  lines.push(`public interface I${entity.name}Repository`);
  lines.push('{');
  lines.push(`    Task<${entity.name}?> GetByIdAsync(Guid id);`);
  lines.push(`    Task<IReadOnlyList<${entity.name}>> GetAllAsync(int page = 1, int pageSize = 20);`);
  lines.push(`    Task<${entity.name}> CreateAsync(${entity.name} entity);`);
  lines.push(`    Task UpdateAsync(${entity.name} entity);`);
  lines.push(`    Task DeleteAsync(Guid id);`);
  lines.push(`    Task<int> CountAsync();`);

  const searchable = entity.fields.filter(f => f.isSearchable);
  if (searchable.length > 0) {
    lines.push(`    Task<IReadOnlyList<${entity.name}>> SearchAsync(string query, int page = 1, int pageSize = 20);`);
  }

  lines.push('}');

  return {
    fileName: `I${entity.name}Repository.cs`,
    layer: 'Infrastructure',
    code: lines.join('\n'),
    language: 'csharp',
  };
}

export function generateService(entity: EntityDefinition): GeneratedCode {
  const lines: string[] = [];
  lines.push(`using Application.DTOs.${entity.name};`);
  lines.push(`using Infrastructure.Repositories;`);
  lines.push(`using Domain.Entities;`);
  lines.push('');
  lines.push(`namespace Application.Services;`);
  lines.push('');

  // Interface
  lines.push(`public interface I${entity.name}Service`);
  lines.push('{');
  lines.push(`    Task<${entity.name}ResponseDto?> GetByIdAsync(Guid id);`);
  lines.push(`    Task<IReadOnlyList<${entity.name}ResponseDto>> GetAllAsync(int page, int pageSize);`);
  lines.push(`    Task<${entity.name}ResponseDto> CreateAsync(Create${entity.name}Dto dto);`);
  lines.push(`    Task UpdateAsync(Guid id, Update${entity.name}Dto dto);`);
  lines.push(`    Task DeleteAsync(Guid id);`);
  lines.push('}');
  lines.push('');

  // Implementation
  lines.push(`public class ${entity.name}Service : I${entity.name}Service`);
  lines.push('{');
  lines.push(`    private readonly I${entity.name}Repository _repository;`);
  lines.push('');
  lines.push(`    public ${entity.name}Service(I${entity.name}Repository repository)`);
  lines.push('    {');
  lines.push('        _repository = repository;');
  lines.push('    }');
  lines.push('');

  // GetById
  lines.push(`    public async Task<${entity.name}ResponseDto?> GetByIdAsync(Guid id)`);
  lines.push('    {');
  lines.push('        var entity = await _repository.GetByIdAsync(id);');
  lines.push('        if (entity == null) return null;');
  lines.push(`        return MapToDto(entity);`);
  lines.push('    }');
  lines.push('');

  // GetAll
  lines.push(`    public async Task<IReadOnlyList<${entity.name}ResponseDto>> GetAllAsync(int page, int pageSize)`);
  lines.push('    {');
  lines.push('        var entities = await _repository.GetAllAsync(page, pageSize);');
  lines.push('        return entities.Select(MapToDto).ToList();');
  lines.push('    }');
  lines.push('');

  // Create
  lines.push(`    public async Task<${entity.name}ResponseDto> CreateAsync(Create${entity.name}Dto dto)`);
  lines.push('    {');
  lines.push(`        var entity = new ${entity.name}`);
  lines.push('        {');
  lines.push('            Id = Guid.NewGuid(),');
  entity.fields.filter(f => f.showInForm && f.type !== 'relation').forEach(f => {
    lines.push(`            ${f.name} = dto.${f.name},`);
  });
  if (entity.hasAudit) {
    lines.push('            CreatedAt = DateTime.UtcNow,');
  }
  lines.push('        };');
  lines.push('        var created = await _repository.CreateAsync(entity);');
  lines.push('        return MapToDto(created);');
  lines.push('    }');
  lines.push('');

  // Update
  lines.push(`    public async Task UpdateAsync(Guid id, Update${entity.name}Dto dto)`);
  lines.push('    {');
  lines.push('        var entity = await _repository.GetByIdAsync(id);');
  lines.push('        if (entity == null) throw new KeyNotFoundException();');
  entity.fields.filter(f => f.showInForm && f.type !== 'relation').forEach(f => {
    lines.push(`        entity.${f.name} = dto.${f.name};`);
  });
  if (entity.hasAudit) {
    lines.push('        entity.UpdatedAt = DateTime.UtcNow;');
  }
  lines.push('        await _repository.UpdateAsync(entity);');
  lines.push('    }');
  lines.push('');

  // Delete
  lines.push('    public async Task DeleteAsync(Guid id)');
  lines.push('    {');
  if (entity.hasSoftDelete) {
    lines.push('        var entity = await _repository.GetByIdAsync(id);');
    lines.push('        if (entity == null) throw new KeyNotFoundException();');
    lines.push('        entity.IsDeleted = true;');
    lines.push('        entity.DeletedAt = DateTime.UtcNow;');
    lines.push('        await _repository.UpdateAsync(entity);');
  } else {
    lines.push('        await _repository.DeleteAsync(id);');
  }
  lines.push('    }');
  lines.push('');

  // Map helper
  const listFields = entity.fields.filter(f => f.showInList && f.type !== 'relation');
  lines.push(`    private static ${entity.name}ResponseDto MapToDto(${entity.name} e)`);
  lines.push('    {');
  lines.push(`        return new ${entity.name}ResponseDto(`);
  lines.push('            e.Id,');
  listFields.forEach((f, i) => {
    const comma = i < listFields.length - 1 ? ',' : (entity.hasAudit ? ',' : '');
    lines.push(`            e.${f.name}${comma}`);
  });
  if (entity.hasAudit) {
    lines.push('            e.CreatedAt,');
    lines.push('            e.CreatedBy');
  }
  lines.push('        );');
  lines.push('    }');

  lines.push('}');

  return {
    fileName: `${entity.name}Service.cs`,
    layer: 'Application',
    code: lines.join('\n'),
    language: 'csharp',
  };
}

export function generateController(entity: EntityDefinition): GeneratedCode {
  const prefix = entity.apiPrefix || entity.pluralName.toLowerCase();
  const lines: string[] = [];
  lines.push('using Microsoft.AspNetCore.Mvc;');
  lines.push(`using Application.DTOs.${entity.name};`);
  lines.push(`using Application.Services;`);
  lines.push('');
  lines.push(`namespace API.Controllers;`);
  lines.push('');
  lines.push('[ApiController]');
  lines.push(`[Route("api/${prefix}")]`);
  lines.push(`public class ${entity.pluralName}Controller : ControllerBase`);
  lines.push('{');
  lines.push(`    private readonly I${entity.name}Service _service;`);
  lines.push('');
  lines.push(`    public ${entity.pluralName}Controller(I${entity.name}Service service)`);
  lines.push('    {');
  lines.push('        _service = service;');
  lines.push('    }');
  lines.push('');

  // GET all
  lines.push('    [HttpGet]');
  lines.push(`    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)`);
  lines.push('    {');
  lines.push('        var items = await _service.GetAllAsync(page, pageSize);');
  lines.push('        return Ok(items);');
  lines.push('    }');
  lines.push('');

  // GET by id
  lines.push('    [HttpGet("{id}")]');
  lines.push('    public async Task<IActionResult> GetById(Guid id)');
  lines.push('    {');
  lines.push('        var item = await _service.GetByIdAsync(id);');
  lines.push('        if (item == null) return NotFound();');
  lines.push('        return Ok(item);');
  lines.push('    }');
  lines.push('');

  // POST
  lines.push('    [HttpPost]');
  lines.push(`    public async Task<IActionResult> Create([FromBody] Create${entity.name}Dto dto)`);
  lines.push('    {');
  lines.push('        var created = await _service.CreateAsync(dto);');
  lines.push(`        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);`);
  lines.push('    }');
  lines.push('');

  // PUT
  lines.push('    [HttpPut("{id}")]');
  lines.push(`    public async Task<IActionResult> Update(Guid id, [FromBody] Update${entity.name}Dto dto)`);
  lines.push('    {');
  lines.push('        await _service.UpdateAsync(id, dto);');
  lines.push('        return NoContent();');
  lines.push('    }');
  lines.push('');

  // DELETE
  lines.push('    [HttpDelete("{id}")]');
  lines.push('    public async Task<IActionResult> Delete(Guid id)');
  lines.push('    {');
  lines.push('        await _service.DeleteAsync(id);');
  lines.push('        return NoContent();');
  lines.push('    }');

  lines.push('}');

  return {
    fileName: `${entity.pluralName}Controller.cs`,
    layer: 'API',
    code: lines.join('\n'),
    language: 'csharp',
  };
}

export function generateReactPage(entity: EntityDefinition): GeneratedCode {
  const listFields = entity.fields.filter(f => f.showInList && f.type !== 'relation');
  const formFields = entity.fields.filter(f => f.showInForm && f.type !== 'relation');
  const lower = entity.name.charAt(0).toLowerCase() + entity.name.slice(1);

  const lines: string[] = [];
  lines.push(`import { useState, useEffect } from 'react';`);
  lines.push(`import { ${lower}Api, type ${entity.name}Response } from '../api/${lower}';`);
  lines.push('');
  lines.push(`export default function ${entity.pluralName}Page() {`);
  lines.push(`  const [items, setItems] = useState<${entity.name}Response[]>([]);`);
  lines.push('  const [loading, setLoading] = useState(true);');
  lines.push('  const [showForm, setShowForm] = useState(false);');
  lines.push(`  const [editItem, setEditItem] = useState<${entity.name}Response | null>(null);`);
  lines.push('');

  // Form state
  formFields.forEach(f => {
    const def = f.type === 'bool' ? 'false' : f.type === 'int' || f.type === 'decimal' ? '0' : "''";
    lines.push(`  const [${f.name.charAt(0).toLowerCase() + f.name.slice(1)}, set${f.name}] = useState(${def});`);
  });
  lines.push('');

  // Load
  lines.push('  useEffect(() => {');
  lines.push('    loadItems();');
  lines.push('  }, []);');
  lines.push('');
  lines.push('  async function loadItems() {');
  lines.push('    setLoading(true);');
  lines.push('    try {');
  lines.push(`      const res = await ${lower}Api.getAll();`);
  lines.push('      setItems(res.data);');
  lines.push('    } finally {');
  lines.push('      setLoading(false);');
  lines.push('    }');
  lines.push('  }');
  lines.push('');

  // Submit
  lines.push('  async function handleSubmit(e: React.FormEvent) {');
  lines.push('    e.preventDefault();');
  lines.push('    const data = {');
  formFields.forEach(f => {
    lines.push(`      ${f.name.charAt(0).toLowerCase() + f.name.slice(1)},`);
  });
  lines.push('    };');
  lines.push('    if (editItem) {');
  lines.push(`      await ${lower}Api.update(editItem.id, data);`);
  lines.push('    } else {');
  lines.push(`      await ${lower}Api.create(data);`);
  lines.push('    }');
  lines.push('    setShowForm(false);');
  lines.push('    setEditItem(null);');
  lines.push('    loadItems();');
  lines.push('  }');
  lines.push('');

  // Delete
  lines.push('  async function handleDelete(id: string) {');
  lines.push("    if (!confirm('Are you sure?')) return;");
  lines.push(`    await ${lower}Api.delete(id);`);
  lines.push('    loadItems();');
  lines.push('  }');
  lines.push('');

  // Render
  lines.push('  if (loading) return <div className="p-6">Loading...</div>;');
  lines.push('');
  lines.push('  return (');
  lines.push('    <div className="p-6">');
  lines.push(`      <div className="mb-4 flex items-center justify-between">`);
  lines.push(`        <h1 className="text-2xl font-bold">${entity.pluralName}</h1>`);
  lines.push('        <button');
  lines.push('          onClick={() => setShowForm(true)}');
  lines.push('          className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"');
  lines.push('        >');
  lines.push(`          Add ${entity.name}`);
  lines.push('        </button>');
  lines.push('      </div>');
  lines.push('');
  lines.push('      <table className="w-full border-collapse">');
  lines.push('        <thead>');
  lines.push('          <tr className="border-b text-left">');
  listFields.forEach(f => {
    lines.push(`            <th className="p-2">${f.name}</th>`);
  });
  lines.push('            <th className="p-2">Actions</th>');
  lines.push('          </tr>');
  lines.push('        </thead>');
  lines.push('        <tbody>');
  lines.push('          {items.map(item => (');
  lines.push('            <tr key={item.id} className="border-b">');
  listFields.forEach(f => {
    lines.push(`              <td className="p-2">{String(item.${f.name.charAt(0).toLowerCase() + f.name.slice(1)})}</td>`);
  });
  lines.push('              <td className="p-2">');
  lines.push('                <button onClick={() => handleDelete(item.id)} className="text-red-500">Delete</button>');
  lines.push('              </td>');
  lines.push('            </tr>');
  lines.push('          ))}');
  lines.push('        </tbody>');
  lines.push('      </table>');
  lines.push('');
  lines.push('      {showForm && (');
  lines.push('        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">');
  lines.push('          <form onSubmit={handleSubmit} className="rounded-lg bg-white p-6 shadow-lg dark:bg-slate-800">');
  lines.push(`            <h2 className="mb-4 text-lg font-bold">{editItem ? 'Edit' : 'Add'} ${entity.name}</h2>`);
  formFields.forEach(f => {
    const stateVar = f.name.charAt(0).toLowerCase() + f.name.slice(1);
    if (f.type === 'bool') {
      lines.push(`            <label className="mb-2 flex items-center gap-2">`);
      lines.push(`              <input type="checkbox" checked={${stateVar}} onChange={e => set${f.name}(e.target.checked)} />`);
      lines.push(`              ${f.name}`);
      lines.push('            </label>');
    } else {
      const inputType = f.type === 'int' || f.type === 'decimal' ? 'number' : f.type === 'DateTime' ? 'datetime-local' : 'text';
      lines.push(`            <input`);
      lines.push(`              type="${inputType}"`);
      lines.push(`              placeholder="${f.name}"`);
      lines.push(`              value={${stateVar}}`);
      lines.push(`              onChange={e => set${f.name}(${f.type === 'int' || f.type === 'decimal' ? 'Number(e.target.value)' : 'e.target.value'})}`);
      lines.push(`              className="mb-2 w-full rounded border p-2 dark:bg-slate-700"`);
      lines.push(`              ${f.required ? 'required' : ''}`);
      lines.push('            />');
    }
  });
  lines.push('            <div className="mt-4 flex justify-end gap-2">');
  lines.push('              <button type="button" onClick={() => { setShowForm(false); setEditItem(null); }} className="px-4 py-2">Cancel</button>');
  lines.push('              <button type="submit" className="rounded bg-blue-600 px-4 py-2 text-white">Save</button>');
  lines.push('            </div>');
  lines.push('          </form>');
  lines.push('        </div>');
  lines.push('      )}');
  lines.push('    </div>');
  lines.push('  );');
  lines.push('}');

  return {
    fileName: `${entity.pluralName}Page.tsx`,
    layer: 'Frontend',
    code: lines.join('\n'),
    language: 'typescript',
  };
}

export function generateApiClient(entity: EntityDefinition): GeneratedCode {
  const lower = entity.name.charAt(0).toLowerCase() + entity.name.slice(1);
  const prefix = entity.apiPrefix || entity.pluralName.toLowerCase();
  const listFields = entity.fields.filter(f => f.showInList && f.type !== 'relation');
  const formFields = entity.fields.filter(f => f.showInForm && f.type !== 'relation');

  const lines: string[] = [];
  lines.push("import api from './client';");
  lines.push('');

  // Response type
  lines.push(`export interface ${entity.name}Response {`);
  lines.push('  id: string;');
  listFields.forEach(f => {
    lines.push(`  ${f.name.charAt(0).toLowerCase() + f.name.slice(1)}: ${tsType(f)};`);
  });
  if (entity.hasAudit) {
    lines.push('  createdAt: string;');
    lines.push('  createdBy?: string;');
  }
  lines.push('}');
  lines.push('');

  // Create type
  lines.push(`export interface Create${entity.name}Dto {`);
  formFields.forEach(f => {
    lines.push(`  ${f.name.charAt(0).toLowerCase() + f.name.slice(1)}${f.required ? '' : '?'}: ${tsType(f)};`);
  });
  lines.push('}');
  lines.push('');

  // Api object
  lines.push(`export const ${lower}Api = {`);
  lines.push(`  getAll: (page = 1, pageSize = 20) =>`);
  lines.push(`    api.get<${entity.name}Response[]>('/${prefix}', { params: { page, pageSize } }),`);
  lines.push(`  getById: (id: string) =>`);
  lines.push(`    api.get<${entity.name}Response>(\`/${prefix}/\${id}\`),`);
  lines.push(`  create: (data: Create${entity.name}Dto) =>`);
  lines.push(`    api.post<${entity.name}Response>('/${prefix}', data),`);
  lines.push(`  update: (id: string, data: Partial<Create${entity.name}Dto>) =>`);
  lines.push(`    api.put<${entity.name}Response>(\`/${prefix}/\${id}\`, data),`);
  lines.push(`  delete: (id: string) =>`);
  lines.push(`    api.delete(\`/${prefix}/\${id}\`),`);
  lines.push('};');

  return {
    fileName: `${lower}.ts`,
    layer: 'Frontend',
    code: lines.join('\n'),
    language: 'typescript',
  };
}

export function generateAll(entity: EntityDefinition): GeneratedCode[] {
  return [
    generateDomainEntity(entity),
    generateDto(entity),
    generateValidator(entity),
    generateRepository(entity),
    generateService(entity),
    generateController(entity),
    generateReactPage(entity),
    generateApiClient(entity),
  ];
}
