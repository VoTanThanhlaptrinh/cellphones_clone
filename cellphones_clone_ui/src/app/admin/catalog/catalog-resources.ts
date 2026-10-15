import { VndPipe } from '@shared/pipes/vnd.pipe';
import { BadgeTone } from '@shared/ui/badge/badge.component';
import { ORDER_STATUSES, orderStatusLabel, orderStatusTone } from '@shared/ui/badge/order-status';
import { CatalogResource, CatalogRow, OptionSource } from './catalog.model';

const text = (key: string) => (row: CatalogRow) => String(row[key] ?? '');

const vnd = new VndPipe();
/** Số tiền dạng `29.990.000đ`; `0` vẫn hiện `0đ` (xem DESIGN.md §6). */
const money = (key: string) => (row: CatalogRow) => vnd.transform(Number(row[key] ?? 0), false);

const pad = (n: number) => String(n).padStart(2, '0');
/** Ngày giờ dạng `dd/MM/yyyy HH:mm` theo múi giờ của trình duyệt; giá trị trống hoặc không hợp lệ thì để trống. */
const dateTime = (value: unknown): string => {
  const date = value ? new Date(String(value)) : null;
  if (!date || Number.isNaN(date.getTime())) return '';
  return `${pad(date.getDate())}/${pad(date.getMonth() + 1)}/${date.getFullYear()} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

/** Trạng thái của sản phẩm/người dùng (khác trạng thái đơn hàng). */
const recordStatusLabel = (status: string) =>
  ({ active: 'Đang hoạt động', inactive: 'Ngừng hoạt động', deleted: 'Đã xoá' } as Record<string, string>)[status] ?? status;
const recordStatusTone = (status: string): BadgeTone =>
  ({ active: 'success', inactive: 'neutral', deleted: 'danger' } as Record<string, BadgeTone>)[status] ?? 'neutral';

const categories: OptionSource = { listUrl: 'category', label: text('name') };
const brands: OptionSource = { listUrl: 'brands', label: text('name') };

/**
 * Các tài nguyên của khu quản trị (`/api/admin/*`). Thêm tài nguyên mới = thêm một phần tử ở đây:
 * menu, route, bảng, bộ lọc, form và hộp thoại xoá đều sinh từ cấu hình này. Tài nguyên `readOnly` (đơn hàng, giao dịch,
 * sản phẩm, người dùng) chỉ có bảng, bộ lọc và phân trang.
 *
 * Lưu ý theo API hiện tại:
 * - Danh sách chỉ trả các bản ghi `active`, nên xoá (xoá mềm) là biến mất khỏi bảng.
 * - Danh sách thương hiệu/nhu cầu không trả `categoryId`, danh mục không trả `parentCategoryId`;
 *   các trường đó chỉ nhập khi tạo (`createOnly`).
 * - Cam kết, thông số và hình ảnh gắn với từng sản phẩm nên sẽ làm cùng trang quản lý sản phẩm.
 */
export const CATALOG_RESOURCES: readonly CatalogResource[] = [
  {
    key: 'orders',
    icon: 'bag',
    title: 'Đơn hàng',
    singular: 'đơn hàng',
    listUrl: 'admin/orders',
    adminUrl: 'orders',
    readOnly: true,
    detail: 'order',
    columns: [
      { header: 'Mã đơn', cell: row => `#${row['id']}` },
      { header: 'Ngày đặt', cell: row => dateTime(row['createDate']) },
      { header: 'Khách hàng', cell: row => [row['customerName'], row['customerEmail']].filter(Boolean).join(' · ') },
      { header: 'Hình thức', cell: row => (row['type'] === 'pickup' ? 'Nhận tại cửa hàng' : 'Giao hàng tận nơi') },
      { header: 'Số lượng', cell: text('itemCount'), alignRight: true },
      { header: 'Tổng tiền', cell: money('totalAmount'), alignRight: true },
      {
        header: 'Trạng thái',
        cell: row => orderStatusLabel(String(row['status'])),
        tone: row => orderStatusTone(String(row['status'])),
      },
    ],
    fields: [],
    filters: [
      {
        key: 'status',
        label: 'Trạng thái',
        type: 'select',
        placeholder: 'Tất cả trạng thái',
        options: ORDER_STATUSES.map(value => ({ value, label: orderStatusLabel(value) })),
      },
    ],
  },
  {
    key: 'transactions',
    icon: 'card',
    title: 'Giao dịch',
    singular: 'giao dịch',
    listUrl: 'admin/transactions',
    adminUrl: 'transactions',
    readOnly: true,
    columns: [
      { header: 'Mã GD', cell: row => `#${row['id']}` },
      { header: 'Đơn hàng', cell: row => `#${row['orderId']}` },
      { header: 'Khách hàng', cell: text('customerEmail') },
      { header: 'Số tiền', cell: money('amount'), alignRight: true },
      { header: 'Cổng thanh toán', cell: text('provider') },
      { header: 'Mã tham chiếu', cell: text('reference') },
      { header: 'Thời điểm', cell: row => dateTime(row['transactionDate'] ?? row['createDate']) },
      {
        header: 'Trạng thái',
        cell: row => (row['status'] === 'success' ? 'Thành công' : String(row['status'] ?? '')),
        tone: row => (row['status'] === 'success' ? 'success' : 'neutral'),
      },
    ],
    fields: [],
  },
  {
    key: 'products',
    icon: 'box',
    title: 'Sản phẩm',
    singular: 'sản phẩm',
    listUrl: 'admin/products',
    adminUrl: 'products',
    readOnly: true,
    columns: [
      { header: 'Mã', cell: text('id') },
      { header: 'Tên sản phẩm', cell: text('name') },
      { header: 'Giá bán', cell: money('salePrice'), alignRight: true },
      { header: 'Giá gốc', cell: money('basePrice'), alignRight: true },
      { header: 'Tồn kho', cell: text('stock'), alignRight: true },
      {
        header: 'Trạng thái',
        cell: row => recordStatusLabel(String(row['status'] ?? '')),
        tone: row => recordStatusTone(String(row['status'] ?? '')),
      },
      { header: 'Ngày tạo', cell: row => dateTime(row['createDate']) },
    ],
    fields: [],
    filters: [{ key: 'search', label: 'Tìm kiếm', type: 'search', placeholder: 'Tìm theo tên sản phẩm' }],
  },
  {
    key: 'users',
    icon: 'user',
    title: 'Người dùng',
    singular: 'người dùng',
    listUrl: 'admin/users',
    adminUrl: 'users',
    readOnly: true,
    columns: [
      { header: 'Họ tên', cell: text('fullName') },
      { header: 'Email', cell: text('email') },
      { header: 'Số điện thoại', cell: text('phone') },
      { header: 'Vai trò', cell: row => (Array.isArray(row['roles']) ? row['roles'].join(', ') : '') },
      {
        header: 'Trạng thái',
        cell: row => recordStatusLabel(String(row['status'] ?? '')),
        tone: row => recordStatusTone(String(row['status'] ?? '')),
      },
      { header: 'Ngày tạo', cell: row => dateTime(row['createDate']) },
    ],
    fields: [],
    filters: [{ key: 'search', label: 'Tìm kiếm', type: 'search', placeholder: 'Tìm theo tên hoặc email' }],
  },
  {
    key: 'categories',
    icon: 'folder',
    title: 'Danh mục',
    singular: 'danh mục',
    listUrl: 'category',
    adminUrl: 'categories',
    columns: [
      { header: 'Tên', cell: text('name') },
      { header: 'Đường dẫn', cell: text('slugName') },
    ],
    fields: [
      { key: 'name', label: 'Tên danh mục', type: 'text', required: true, maxLength: 255 },
      { key: 'parentCategoryId', label: 'Danh mục cha', type: 'select', optionsFrom: categories, createOnly: true },
    ],
  },
  {
    key: 'brands',
    icon: 'tag',
    title: 'Thương hiệu',
    singular: 'thương hiệu',
    listUrl: 'brands',
    adminUrl: 'brands',
    columns: [
      { header: 'Tên', cell: text('name') },
      { header: 'Đường dẫn', cell: text('slugName') },
    ],
    fields: [
      { key: 'categoryId', label: 'Danh mục', type: 'select', required: true, optionsFrom: categories, createOnly: true },
      { key: 'name', label: 'Tên thương hiệu', type: 'text', required: true, maxLength: 255 },
    ],
  },
  {
    key: 'series',
    icon: 'layers',
    title: 'Dòng sản phẩm',
    singular: 'dòng sản phẩm',
    listUrl: 'series',
    adminUrl: 'series',
    columns: [
      { header: 'Tên', cell: text('name') },
      { header: 'Thương hiệu', cell: text('brandName') },
    ],
    fields: [
      { key: 'brandId', label: 'Thương hiệu', type: 'select', required: true, optionsFrom: brands },
      { key: 'name', label: 'Tên dòng sản phẩm', type: 'text', required: true, maxLength: 255 },
    ],
  },
  {
    key: 'demands',
    icon: 'sliders',
    title: 'Nhu cầu',
    singular: 'nhu cầu',
    listUrl: 'demands',
    adminUrl: 'demands',
    columns: [
      { header: 'Tên', cell: text('name') },
      { header: 'Đường dẫn', cell: text('slugName') },
    ],
    fields: [
      { key: 'categoryId', label: 'Danh mục', type: 'select', required: true, optionsFrom: categories, createOnly: true },
      { key: 'name', label: 'Tên nhu cầu', type: 'text', required: true, maxLength: 255 },
    ],
  },
  {
    key: 'storehouses',
    icon: 'store',
    title: 'Cửa hàng / kho',
    singular: 'cửa hàng',
    listUrl: 'storeHouses',
    adminUrl: 'storehouses',
    columns: [
      { header: 'Địa chỉ', cell: text('street') },
      { header: 'Quận/Huyện', cell: text('district') },
      { header: 'Tỉnh/Thành phố', cell: text('city') },
    ],
    fields: [
      { key: 'street', label: 'Số nhà, tên đường', type: 'text', required: true, maxLength: 255 },
      { key: 'district', label: 'Quận/Huyện', type: 'text', required: true, maxLength: 255 },
      { key: 'city', label: 'Tỉnh/Thành phố', type: 'text', required: true, maxLength: 255 },
    ],
  },
];

export function findCatalogResource(key: string | undefined): CatalogResource | undefined {
  return CATALOG_RESOURCES.find(r => r.key === key);
}
