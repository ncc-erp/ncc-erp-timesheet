import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateReviewComponent } from './create-review.component';

describe('NewReviewComponent', () => {
  let component: CreateReviewComponent;
  let fixture: ComponentFixture<CreateReviewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateReviewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateReviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
