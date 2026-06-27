import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { TokenStorage } from './token-storage';
import { AuthService } from './auth.service';

/**
 * Adds the Bearer token to every request and, on a 401 response, clears the
 * session and redirects to the login screen.
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const tokens = inject(TokenStorage);
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = tokens.get();
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        auth.forceLogout();
        router.navigate(['/auth/login']);
      }
      return throwError(() => err);
    })
  );
};
