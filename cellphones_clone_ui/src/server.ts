
import { renderApplication } from '@angular/platform-server';
import bootstrap from './main.server';
import { BootstrapContext } from '@angular/platform-browser';

export default async function render(url: string, document: string, context: BootstrapContext) {
  const appRef = await bootstrap(context);
  return renderApplication(async () => appRef, {
    document,
    url,
  });
}
