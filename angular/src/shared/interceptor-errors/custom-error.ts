
export class CustomError extends Error {
  errorType?: string;
  isCustomError: boolean = true;
}
