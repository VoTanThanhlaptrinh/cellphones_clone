import { HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { LoadingService } from '../services/loading.service';
import { SKIP_GLOBAL_SPINNER } from './loading.context';
import { finalize } from 'rxjs';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  const isSkip = req.context.get(SKIP_GLOBAL_SPINNER);
  if (isSkip) {
    return next(req);
  }
  loadingService.show();
  return next(req).pipe(
    finalize(() => {
      loadingService.hide();
    })
  );
};
