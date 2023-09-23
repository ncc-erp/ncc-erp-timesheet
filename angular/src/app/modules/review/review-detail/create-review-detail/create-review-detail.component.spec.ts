import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateReviewDetailComponent } from './create-review-detail.component';

describe('CreateReviewDetailComponent', () => {
  let component: CreateReviewDetailComponent;
  let fixture: ComponentFixture<CreateReviewDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateReviewDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateReviewDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
