import { IconName } from '@shared/ui/icon/icon.component';

/** Một dòng dữ liệu backend trả về; các trường khác nhau theo từng tài nguyên nên đọc qua `row['tên']`. */
export interface CatalogRow {
  id: number;
  [key: string]: unknown;
}

/** Nguồn dữ liệu cho ô chọn: danh sách phân trang của một tài nguyên khác (ví dụ danh mục cho thương hiệu). */
export interface OptionSource {
  /** Đường dẫn GET danh sách, tương đối với apiUrl. */
  listUrl: string;
  label: (row: CatalogRow) => string;
}

export interface CatalogField {
  /** Tên trường trong body gửi lên backend (camelCase). */
  key: string;
  label: string;
  type: 'text' | 'select';
  required?: boolean;
  maxLength?: number;
  /** Bắt buộc với `type = 'select'`. */
  optionsFrom?: OptionSource;
  /** Chỉ nhập khi tạo mới: backend không cho đổi, hoặc API danh sách không trả về giá trị để điền sẵn khi sửa. */
  createOnly?: boolean;
}

export interface CatalogColumn {
  header: string;
  cell: (row: CatalogRow) => string;
}

/** Mô tả một tài nguyên CRUD của khu quản trị; một cấu hình là đủ cho cả bảng, form thêm/sửa và xoá. */
export interface CatalogResource {
  /** Segment URL dưới /admin, cũng là khoá tra cứu cấu hình. */
  key: string;
  icon: IconName;
  /** Tiêu đề trang và nhãn menu. */
  title: string;
  /** Tên một bản ghi, dùng trong nút và hộp thoại ("Thêm thương hiệu"). */
  singular: string;
  /** GET danh sách phân trang: `${apiUrl}/${listUrl}` (route công khai của backend). */
  listUrl: string;
  /** POST / PUT / DELETE: `${apiUrl}/admin/${adminUrl}`. */
  adminUrl: string;
  columns: CatalogColumn[];
  fields: CatalogField[];
}
