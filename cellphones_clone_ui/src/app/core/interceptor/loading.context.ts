import { HttpContextToken } from '@angular/common/http';

export const SKIP_GLOBAL_SPINNER = new HttpContextToken<boolean>(() => false);