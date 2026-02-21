import { Injectable, signal, computed } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private requestCount = signal(0);
  public isLoading = computed(() => this.requestCount() > 0);
  public show() {
    this.requestCount.update(count => count + 1);
  }
  public hide() {
    this.requestCount.update(count => Math.max(count - 1, 0));
  }
}
