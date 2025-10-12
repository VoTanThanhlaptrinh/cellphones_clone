import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryAccessoryComponent } from './category-accessory.component';

describe('CategoryAccessoryComponent', () => {
  let component: CategoryAccessoryComponent;
  let fixture: ComponentFixture<CategoryAccessoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryAccessoryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CategoryAccessoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
