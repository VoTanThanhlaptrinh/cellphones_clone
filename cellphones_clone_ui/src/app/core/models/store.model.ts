
export interface StreetView {
    id: number;
    street: string;
}
export interface DistrictView {
    district: string;
    streets: StreetView[];
}

export interface StoreView {
    city: string;
    districts: DistrictView[];
}
export interface VietnamWardDto {
    name?: string;
    code: number;
    codename?: string;
    division_type?: string;
    short_codename?: string;
}

export interface VietnamDistrictDto {
    name?: string;
    code: number;
    codename?: string;
    division_type?: string;
    short_codename?: string;
    wards?: VietnamWardDto[];
}

export interface VietnamProvinceDto {
    name?: string;
    code: number;
    codename?: string;
    division_type?: string;
    phone_code: number;
    districts?: VietnamDistrictDto[];
}
