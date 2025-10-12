import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryAudioComponent } from './category-audio.component';

describe('CategoryAudioComponent', () => {
  let component: CategoryAudioComponent;
  let fixture: ComponentFixture<CategoryAudioComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryAudioComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CategoryAudioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
