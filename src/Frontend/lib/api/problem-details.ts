/** Problem Details RFC 9457 kèm trường mở rộng của backend (S-10c). */
export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
};

export const UNKNOWN_ERROR_CODE = "UNKNOWN_ERROR";

export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly problem: ProblemDetails;

  constructor(status: number, problem: ProblemDetails) {
    super(problem.title ?? `Request failed with status ${status}`);
    this.name = "ApiError";
    this.status = status;
    this.code = problem.code ?? UNKNOWN_ERROR_CODE;
    this.problem = problem;
  }
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

export function toProblemDetails(value: unknown): ProblemDetails {
  if (!isRecord(value)) {
    return {};
  }

  const pick = (key: string) =>
    typeof value[key] === "string" ? (value[key] as string) : undefined;

  return {
    type: pick("type"),
    title: pick("title"),
    status: typeof value.status === "number" ? value.status : undefined,
    detail: pick("detail"),
    code: pick("code"),
    traceId: pick("traceId"),
    errors: isRecord(value.errors)
      ? (value.errors as Record<string, string[]>)
      : undefined,
  };
}
