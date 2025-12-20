export interface CartRequest {
    productId: number | undefined | null;
    colorId: number | undefined | null;
}

export interface CartView {
    cartDetailId: number;
    quantity: number;
    productId: number;
    imgUrl: string;
    productName: string;
    basePrice: number;
    salePrice: number;
}