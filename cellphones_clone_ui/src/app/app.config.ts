import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners, provideZoneChangeDetection, } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { routes } from './app.routes';
import { BrowserModule, provideClientHydration, withEventReplay, } from '@angular/platform-browser';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { LOCALE_ID, DEFAULT_CURRENCY_CODE } from '@angular/core';
import { CurrencyPipe, registerLocaleData } from '@angular/common';
import { provideAnimations } from '@angular/platform-browser/animations';
import { DefaultGlobalConfig, provideToastr } from 'ngx-toastr';
import localeVi from '@angular/common/locales/vi';
registerLocaleData(localeVi);

import { errorInterceptor } from './interceptor/error.interceptor';
import { authInterceptor } from './interceptor/auth.interceptor';

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
		provideToastr({
			timeOut: 3000,
			preventDuplicates: true,
		}),
		provideHttpClient(withFetch(), withInterceptors([authInterceptor, errorInterceptor])),
		{ provide: LOCALE_ID, useValue: 'vi-VN' },
		{ provide: DEFAULT_CURRENCY_CODE, useValue: 'VND' },
		CurrencyPipe,
	],
};
