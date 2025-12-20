import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners, provideZoneChangeDetection, } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { routes } from './app.routes';
import { BrowserModule, provideClientHydration, withEventReplay, } from '@angular/platform-browser';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { LOCALE_ID, DEFAULT_CURRENCY_CODE } from '@angular/core';
import { CurrencyPipe, registerLocaleData } from '@angular/common';
import { provideAnimations } from '@angular/platform-browser/animations';
import localeVi from '@angular/common/locales/vi';
registerLocaleData(localeVi);

export const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideZoneChangeDetection({ eventCoalescing: true }),
		provideRouter(routes, withInMemoryScrolling({
			scrollPositionRestoration: 'top',
		})),
		provideClientHydration(withEventReplay()),
		importProvidersFrom(BrowserModule),
		provideAnimations(),
		provideHttpClient(withFetch()),
		{ provide: LOCALE_ID, useValue: 'vi-VN' },
		{ provide: DEFAULT_CURRENCY_CODE, useValue: 'VND' },
		CurrencyPipe,
	],
};
