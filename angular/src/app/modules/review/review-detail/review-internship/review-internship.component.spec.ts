import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ReviewInternshipComponent } from './review-internship.component';

describe('ReviewInternshipComponent', () => {
  let component: ReviewInternshipComponent;
  let fixture: ComponentFixture<ReviewInternshipComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ReviewInternshipComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReviewInternshipComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
