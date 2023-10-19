import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CustomError } from './custom-error';
import { Injectable, Injector } from '@angular/core';
import { SentryService } from '@shared/sentry-service';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
  private sentryService: SentryService;

  constructor(private injector: Injector) {
    this.sentryService = this.injector.get(SentryService);
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request)
      .pipe(
        catchError((response: HttpErrorResponse) => {
          if (response.status === 401) {
            abp.message.error('Your session is expired,')
            setTimeout(() => { window.location.href = '/account/login' }, 3000);
            return;
          }
          // if(response.status === 403 || response.status === 404){
          //   window.location.href = '/app/no-permission' ;
          //   return;
          // }
          const error = response.error;
          let errMsg = '';
          if (error instanceof ErrorEvent) {
            errMsg = `Error: ${error.message}`;
          } else {
            errMsg = error ? `${error.error.message || response.message}` : response.message;
            abp.notify.error(errMsg);
          }

          //post log to sentry
          this.sentryService.postErrorLog(errMsg, {
            requestInfo: request,
            responseInfo: response
          })
         
          const customErr = new CustomError(errMsg);
          customErr.errorType = "Call API error";
          return throwError(customErr);
        })
      );
  }
}
