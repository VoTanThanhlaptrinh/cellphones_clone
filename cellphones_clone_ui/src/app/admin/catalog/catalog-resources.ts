import { CatalogResource, CatalogRow, OptionSource } from './catalog.model';

const text = (key: string) => (row: CatalogRow) => String(row[key] ?? '');

const categories: OptionSource = { listUrl: 'category', label: text('name') };
const brands: OptionSource = { listUrl: 'brands', label: text('name') };

/**
 * Các tài nguyên danh mục có CRUD ở backend (`/api/admin/*`). Thêm tài nguyên mới = thêm một phần tử ở đây:
 * menu, route, bảng, form và hộp thoại xoá đều sinh từ cấu hình này.
 *
 * Lưu ý theo API hiện tại:
 * - Danh sách chỉ trả các bản ghi `active`, nên xoá (xoá mềm) là biến mất khỏi bảng.
 * - Danh sách thương hiệu/nhu cầu không trả `categoryId`, danh mục không trả `parentCategoryId`;
 *   các trường đó chỉ nhập khi tạo (`createOnly`).
 * - Cam kết, thông số và hình ảnh gắn với từng sản phẩm nên sẽ làm cùng trang quản lý sản phẩm.
 */
export const CATALOG_RESOURCES: readonly CatalogResource[] = [
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
