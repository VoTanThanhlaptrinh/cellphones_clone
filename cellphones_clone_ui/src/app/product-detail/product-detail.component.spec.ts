import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { ProductDetailComponent } from './product-detail.component';
import { ProductService } from '../services/product.service';
import { ProductViewDetail } from '../core/models/product.model';

describe('ProductDetailComponent', () => {
  let component: ProductDetailComponent;
  let fixture: ComponentFixture<ProductDetailComponent>;
  let productServiceSpy: jasmine.SpyObj<ProductService>;
  let mockDetail: ProductViewDetail;

  beforeEach(async () => {
    productServiceSpy = jasmine.createSpyObj<ProductService>('ProductService', ['getProductDetail']);

    const mockActivatedRoute = {
      paramMap: of(convertToParamMap({ id: '1' }))
    } as Partial<ActivatedRoute>;

    mockDetail = {
      id: 1,
      name: 'Test Product',
      basePrice: 10000000,
      salePrice: 9000000,
      version: '128GB',
      imageUrl: 'https://cdn.example.com/cover.jpg',
      productImages: [
        {
          blobUrl: 'https://cdn.example.com/gallery-1.jpg',
          mimeType: 'image/jpeg',
          name: 'gallery-1',
          alt: 'Product image'
        }
      ],
      productSpecification: [
        {
          name: 'Display',
          specificationDetails: [
            { name: 'Size', value: '6.7 inch' }
          ]
        }
      ],
      commitments: ['Official warranty guarantee'],
      quantity: 5,
      street: '123 ABC Street',
      district: 'District 1',
      city: 'Ho Chi Minh City'
    };

    productServiceSpy.getProductDetail.and.returnValue(of(mockDetail));

    await TestBed.configureTestingModule({
      imports: [ProductDetailComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: ProductService, useValue: productServiceSpy }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load product detail data on init', () => {
    expect(productServiceSpy.getProductDetail).toHaveBeenCalledWith(1);
    expect(component.product).toEqual(mockDetail);
    expect(component.images).toEqual(['https://cdn.example.com/gallery-1.jpg']);
    expect(component.warehouseAddress).toBe('123 ABC Street, District 1, Ho Chi Minh City');
  });
});
