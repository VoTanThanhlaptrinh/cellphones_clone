import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryHouseholdComponent } from './category-household.component';

describe('CategoryHouseholdComponent', () => {
  let component: CategoryHouseholdComponent;
  let fixture: ComponentFixture<CategoryHouseholdComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryHouseholdComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CategoryHouseholdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
