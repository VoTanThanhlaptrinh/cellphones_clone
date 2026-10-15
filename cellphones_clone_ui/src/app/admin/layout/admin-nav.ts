import { IconName } from '@shared/ui/icon/icon.component';

export interface AdminNavItem {
  label: string;
  icon: IconName;
  /** Đường dẫn con của /admin. */
  path: string;
}

/** Menu của khu quản trị. Thêm trang mới = thêm một dòng ở đây và một route trong admin.routes.ts. */
export const ADMIN_NAV: readonly AdminNavItem[] = [
  { label: 'Tổng quan', icon: 'grid', path: 'dashboard' },
  { label: 'Danh mục', icon: 'folder', path: 'categories' },
  { label: 'Thương hiệu', icon: 'tag', path: 'brands' },
  { label: 'Dòng sản phẩm', icon: 'layers', path: 'series' },
  { label: 'Nhu cầu', icon: 'sliders', path: 'demands' },
  { label: 'Cửa hàng / kho', icon: 'store', path: 'storehouses' },
];
