import { Component, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../core/i18n/language.service';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [TranslateModule],
  template: `
    <div class="btn-group btn-group-sm" role="group" aria-label="Language">
      <button type="button" class="btn"
              [class.btn-primary]="lang.current() === 'uk'"
              [class.btn-outline-secondary]="lang.current() !== 'uk'"
              (click)="lang.use('uk')">{{ 'lang.uk' | translate }}</button>
      <button type="button" class="btn"
              [class.btn-primary]="lang.current() === 'en'"
              [class.btn-outline-secondary]="lang.current() !== 'en'"
              (click)="lang.use('en')">{{ 'lang.en' | translate }}</button>
    </div>
  `
})
export class LanguageSwitcherComponent {
  lang = inject(LanguageService);
}
