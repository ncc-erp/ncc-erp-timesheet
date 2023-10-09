import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import * as Sentry from "@sentry/browser";

export class HttpErrorInterceptor implements HttpInterceptor {

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
          Sentry.setExtra("Request", request)
          Sentry.setExtra("Response", response)
          const error = response.error;
          let errMsg = '';
          if (error instanceof ErrorEvent) {
            errMsg = `Error: ${error.message}`;
                      } else {
            errMsg = error ? `${error.error.message || response.message}` : response.message;
                        abp.notify.error(errMsg);
          }
          Sentry.captureMessage(errMsg)
          return throwError(errMsg);
        })
      );
  }
}
