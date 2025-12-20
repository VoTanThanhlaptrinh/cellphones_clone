export interface ProductView {
    id: number;
    imgUrl: string;
    productName: string;
    basePrice: number;
    salePrice: number;
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
    productSpecification: ProductSpecification[];
    commitments: string[];
    colorDTOs: ColorDTO[];
    StoreHouseDTOs: StoreHouseDTO[]
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
    image: ProductImage
}
