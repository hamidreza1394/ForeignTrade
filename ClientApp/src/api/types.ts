// Contracts of the ForeignTrade API v1 (Server/Api).
// Every response is checked at runtime before it is used, so a contract change on the
// server surfaces as a clear error instead of `undefined` deep inside a component.

export type Session = {
  username: string;
  fullName: string;
  accountKind: string;
  isAdministrator: boolean;
};

export type CompanySummary = {
  id: string;
  name: string;
  nationalId: string | null;
  roleName: string;
};

export type OperationalPeriodStatus = 'Open' | 'Closed' | 'Archived';

export type OperationalPeriod = {
  id: string;
  title: string;
  /** Calendar date as stored by the server: ISO `yyyy-MM-dd` (Gregorian). */
  startDate: string;
  /** Calendar date as stored by the server: ISO `yyyy-MM-dd` (Gregorian). */
  endDate: string;
  status: OperationalPeriodStatus;
};

export type CompanyOverview = {
  company: CompanySummary;
  periods: OperationalPeriod[];
};

/** What a working context allows: an open period is read-write, a closed one is read-only. */
export type AccessMode = 'readWrite' | 'readOnly';

/**
 * An operational period as offered for selection; `isSelectable` and `accessMode` are decided by the
 * server (`accessMode` is `null` for a period that cannot be entered).
 */
export type OperationalPeriodChoice = OperationalPeriod & { isSelectable: boolean; accessMode: AccessMode | null };

export type CompanyRelationshipType = 'parent' | 'subsidiary' | 'associate' | 'jointVenture' | 'jointVenturer' | 'investor' | 'investment' | 'groupMember' | 'independent' | 'unknown';

export type CompanyChoice = CompanySummary & { periods: OperationalPeriodChoice[]; relationshipTypes: CompanyRelationshipType[] };

export type SelectedContext = {
  company: CompanySummary;
  period: OperationalPeriodChoice;
};

export type EntryMode = 'directEntry' | 'selectionRequired' | 'noCompany' | 'noEligiblePeriod';

export type DirectSelection = { companyId: string; periodId: string };

/**
 * The selected company and operational period for the sign-in: the validated current choice (if any), the choices, and
 * what to do next. Every decision is made by the server; the client only presents it.
 */
export type WorkContext = {
  current: SelectedContext | null;
  entry: EntryMode;
  directSelection: DirectSelection | null;
  companies: CompanyChoice[];
};

export type AntiforgeryToken = {
  headerName: string;
  requestToken: string;
};

export class ContractError extends Error {
  constructor(what: string) {
    super(`پاسخ سرور با قرارداد API هم‌خوان نیست (${what}).`);
    this.name = 'ContractError';
  }
}

type JsonObject = Record<string, unknown>;

function isObject(value: unknown): value is JsonObject {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function readString(source: JsonObject, key: string, what: string): string {
  const value = source[key];
  if (typeof value !== 'string') {
    throw new ContractError(`${what}.${key}`);
  }
  return value;
}

function readNullableString(source: JsonObject, key: string, what: string): string | null {
  const value = source[key];
  if (value === null || value === undefined) {
    return null;
  }
  if (typeof value !== 'string') {
    throw new ContractError(`${what}.${key}`);
  }
  return value;
}

const isoDate = /^\d{4}-\d{2}-\d{2}$/;

function readIsoDate(source: JsonObject, key: string, what: string): string {
  const value = readString(source, key, what);
  if (!isoDate.test(value)) {
    throw new ContractError(`${what}.${key}`);
  }
  return value;
}

const statuses: readonly OperationalPeriodStatus[] = ['Open', 'Closed', 'Archived'];

function isOperationalPeriodStatus(value: string): value is OperationalPeriodStatus {
  return (statuses as readonly string[]).includes(value);
}

export function parseSession(value: unknown): Session {
  if (!isObject(value) || typeof value['isAdministrator'] !== 'boolean') {
    throw new ContractError('session');
  }
  return {
    username: readString(value, 'username', 'session'),
    fullName: readString(value, 'fullName', 'session'),
    accountKind: readString(value, 'accountKind', 'session'),
    isAdministrator: value['isAdministrator'],
  };
}

export function parseCompanySummary(value: unknown): CompanySummary {
  if (!isObject(value)) {
    throw new ContractError('company');
  }
  return {
    id: readString(value, 'id', 'company'),
    name: readString(value, 'name', 'company'),
    nationalId: readNullableString(value, 'nationalId', 'company'),
    roleName: readString(value, 'roleName', 'company'),
  };
}

export function parseCompanies(value: unknown): CompanySummary[] {
  if (!Array.isArray(value)) {
    throw new ContractError('companies');
  }
  return value.map(parseCompanySummary);
}

export function parseOperationalPeriod(value: unknown): OperationalPeriod {
  if (!isObject(value)) {
    throw new ContractError('period');
  }
  const status = readString(value, 'status', 'period');
  if (!isOperationalPeriodStatus(status)) {
    throw new ContractError('period.status');
  }
  return {
    id: readString(value, 'id', 'period'),
    title: readString(value, 'title', 'period'),
    startDate: readIsoDate(value, 'startDate', 'period'),
    endDate: readIsoDate(value, 'endDate', 'period'),
    status,
  };
}

function readBoolean(source: JsonObject, key: string, what: string): boolean {
  const value = source[key];
  if (typeof value !== 'boolean') {
    throw new ContractError(`${what}.${key}`);
  }
  return value;
}

export function parseOperationalPeriodChoice(value: unknown): OperationalPeriodChoice {
  if (!isObject(value)) {
    throw new ContractError('period');
  }
  const mode = value['accessMode'];
  if (mode !== null && mode !== 'readWrite' && mode !== 'readOnly') {
    throw new ContractError('period');
  }
  return { ...parseOperationalPeriod(value), isSelectable: readBoolean(value, 'isSelectable', 'period'), accessMode: mode };
}

const companyRelationshipTypes: readonly CompanyRelationshipType[] = ['parent', 'subsidiary', 'associate', 'jointVenture', 'jointVenturer', 'investor', 'investment', 'groupMember', 'independent', 'unknown'];

function parseCompanyRelationshipTypes(value: unknown): CompanyRelationshipType[] {
  if (!Array.isArray(value) || value.some((item) => typeof item !== 'string' || !(companyRelationshipTypes as readonly string[]).includes(item))) {
    throw new ContractError('companyChoice.relationshipTypes');
  }
  return value as CompanyRelationshipType[];
}

function parseCompanyChoice(value: unknown): CompanyChoice {
  if (!isObject(value) || !Array.isArray(value['periods'])) {
    throw new ContractError('companyChoice');
  }
  return {
    ...parseCompanySummary(value),
    relationshipTypes: parseCompanyRelationshipTypes(value['relationshipTypes']),
    periods: value['periods'].map(parseOperationalPeriodChoice),
  };
}

function parseSelectedContext(value: unknown): SelectedContext {
  if (!isObject(value)) {
    throw new ContractError('context');
  }
  return {
    company: parseCompanySummary(value['company']),
    period: parseOperationalPeriodChoice(value['period']),
  };
}

const entryModes: readonly EntryMode[] = ['directEntry', 'selectionRequired', 'noCompany', 'noEligiblePeriod'];

export function parseSelectedContextResponse(value: unknown): SelectedContext {
  return parseSelectedContext(value);
}

export function parseWorkContext(value: unknown): WorkContext {
  if (!isObject(value) || !Array.isArray(value['companies'])) {
    throw new ContractError('workContext');
  }
  const entry = readString(value, 'entry', 'workContext');
  if (!(entryModes as readonly string[]).includes(entry)) {
    throw new ContractError('workContext.entry');
  }
  const direct = value['directSelection'];
  let directSelection: DirectSelection | null = null;
  if (direct !== null && direct !== undefined) {
    if (!isObject(direct)) {
      throw new ContractError('workContext.directSelection');
    }
    directSelection = {
      companyId: readString(direct, 'companyId', 'directSelection'),
      periodId: readString(direct, 'periodId', 'directSelection'),
    };
  }
  const current = value['current'];
  return {
    current: current === null || current === undefined ? null : parseSelectedContext(current),
    entry: entry as EntryMode,
    directSelection,
    companies: value['companies'].map(parseCompanyChoice),
  };
}

export function parseCompanyOverview(value: unknown): CompanyOverview {
  if (!isObject(value) || !Array.isArray(value['periods'])) {
    throw new ContractError('overview');
  }
  return {
    company: parseCompanySummary(value['company']),
    periods: value['periods'].map(parseOperationalPeriod),
  };
}

export function parseAntiforgeryToken(value: unknown): AntiforgeryToken {
  if (!isObject(value)) {
    throw new ContractError('antiforgery');
  }
  return {
    headerName: readString(value, 'headerName', 'antiforgery'),
    requestToken: readString(value, 'requestToken', 'antiforgery'),
  };
}
