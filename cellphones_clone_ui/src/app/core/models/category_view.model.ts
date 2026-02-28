import { ProductView } from "./product.model";

export interface CategoryView {
    id: number;
    name: string;
    slugName?: string;
    demands: DemandView[];
    brands: BrandView[];
    products: ProductView[];
}

export interface DemandView {
    id: number;
    name: string;
    slugName?: string;
}

export interface BrandView {
    id: number;
    name: string;
    slugName?: string;
}

export interface CategoryDetailView {
    id: number;
    name: string;
    slugName?: string;
    demands: DemandView[];
    brands: BrandView[];
    products: ProductView[];
    total: number;
}
