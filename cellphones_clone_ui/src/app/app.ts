import { ChangeDetectionStrategy, Component, effect } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { LoadingService } from '@core/services/loading.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgxSpinnerModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected title = 'cellphones-clone-ui';

  constructor(
    private spinner: NgxSpinnerService,
    private loadingService: LoadingService,
  ) {
    effect(() => {
      if (this.loadingService.isLoading()) {
        this.spinner.show();
      } else {
        this.spinner.hide();
      }
    });
  }
}
