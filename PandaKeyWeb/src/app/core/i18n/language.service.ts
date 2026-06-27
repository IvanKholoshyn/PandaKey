import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

const LANG_KEY = 'pk_lang';
export type Lang = 'uk' | 'en';

/** Runtime language switching with persistence in localStorage. */
@Injectable({ providedIn: 'root' })
export class LanguageService {
  private translate = inject(TranslateService);
  readonly current = signal<Lang>('uk');

  init(): void {
    const saved = (localStorage.getItem(LANG_KEY) as Lang) || 'uk';
    this.translate.addLangs(['uk', 'en']);
    this.translate.setDefaultLang('uk');
    this.use(saved);
  }

  use(lang: Lang): void {
    this.translate.use(lang);
    this.current.set(lang);
    localStorage.setItem(LANG_KEY, lang);
  }

  /** Locale-aware string comparator for table sorting. */
  collator(): Intl.Collator {
    return new Intl.Collator(this.current(), { sensitivity: 'base', numeric: true });
  }
}
