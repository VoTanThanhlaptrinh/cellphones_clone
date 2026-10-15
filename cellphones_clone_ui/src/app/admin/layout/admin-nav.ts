import { IconName } from '@shared/ui/icon/icon.component';
import { CATALOG_RESOURCES } from '../catalog/catalog-resources';

export interface AdminNavItem {
  label: string;
  icon: IconName;
  /** Đường dẫn con của /admin. */
  path: string;
}

/** Menu của khu quản trị: tổng quan, rồi mỗi tài nguyên trong CATALOG_RESOURCES. Trang riêng khác thì thêm một dòng ở đây. */
export const ADMIN_NAV: readonly AdminNavItem[] = [
  { label: 'Tổng quan', icon: 'grid', path: 'dashboard' },
  ...CATALOG_RESOURCES.map(({ title, icon, key }) => ({ label: title, icon, path: key })),
];
