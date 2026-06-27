import { APP_INITIALIZER, ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';

import { routes } from './app.routes';
import { jwtInterceptor } from './core/auth/jwt.interceptor';
import { createTranslateLoader } from './core/i18n/translate-loader';
import { LanguageService } from './core/i18n/language.service';

export function initLanguage(lang: LanguageService) {
  return () => lang.init();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor])),
    importProvidersFrom(
      TranslateModule.forRoot({
        defaultLanguage: 'uk',
        loader: {
          provide: TranslateLoader,
          useFactory: createTranslateLoader,
          deps: [HttpClient]
        }
      })
    ),
    {
      provide: APP_INITIALIZER,
      useFactory: initLanguage,
      deps: [LanguageService],
      multi: true
    }
  ]
};
