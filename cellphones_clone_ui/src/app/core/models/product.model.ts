export interface ProductView {
    id: number;
    imgUrl: string;
    productName: string;
    basePrice: number;
    salePrice: number;
    /** Tổng tồn kho; null/undefined nghĩa là API chưa trả thông tin này. */
    stock?: number | null;
    /** Điểm đánh giá trung bình (1–5), null khi chưa có đánh giá. */
    rating?: number | null;
    reviewCount?: number;
    /** Thông số nổi bật lấy từ dữ liệu thật (màn hình, RAM, bộ nhớ…). */
    highlights?: string[] | null;
    /** Màu còn nhiều hàng nhất, dùng để thêm nhanh vào giỏ; null khi hết hàng hoặc chưa biết. */
    defaultColorId?: number | null;
}

export interface ProductImage {
    originUrl: string;
    mimeType: string;
    name: string;
    alt: string | null;
}

export interface ProductSpecificationDetail {
    name: string;
    value: string;
}

export interface ProductSpecification {
    name: string;
    specDetails?: ProductSpecificationDetail[];
}

export interface ProductViewDetail {
    id: number;
    name: string | null;
    basePrice: number;
    salePrice: number;
    versions: string | null;
    imageUrl: string | null;
    productImages: ProductImage[];
    productSpecification?: ProductSpecification[];
    commitments?: string[];
    colorDTOs?: ColorDTO[];
    StoreHouseDTOs?: StoreHouseDTO[]
}

export interface StoreHouseDTO {
    quantity: number;
    street: string | null;
    district: string | null;
    city: string | null;
}

export interface ColorDTO {
    colorId: number;
    name: string;
    image?: ProductImage
}
