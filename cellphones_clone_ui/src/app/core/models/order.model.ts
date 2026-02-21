export interface Order {
    id: number;
    orderDetails: OrderDetail[];
    status: string;
    createDate: string;
    updateDate: string;
    createBy?: string;
    updateBy?: string;
    // createUser and updateUser omitted for simplicity unless needed
}

export interface OrderDetail {
    id: number;
    productOrderDetailId: number;
    product: any; // Use appropriate Product interface if available
    colorId: number;
    color: any; // Use appropriate Color interface if available
    price: number;
    quantity: number;
    createDate: string;
    updateDate: string;
}
