export interface CommitmentItem {
  id: number;
  productId: number;
  context: string;
  status: string;
}

export interface SpecificationDetailItem {
  id?: number;
  name: string;
  value: string;
}

export interface SpecificationItem {
  id?: number;
  name: string;
  specDetails?: SpecificationDetailItem[];
}

export interface ImageItem {
  id?: number;
  originUrl?: string;
  blobUrl?: string;
  mimeType?: string;
  name?: string;
  alt?: string;
}
