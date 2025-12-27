
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