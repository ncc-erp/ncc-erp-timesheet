import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { NewReviewInternshipComponent } from './new-review-internship.component';
describe('NewReviewInternshipComponent', () => {
  let component: NewReviewInternshipComponent;
  let fixture: ComponentFixture<NewReviewInternshipComponent>;
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NewReviewInternshipComponent ]
    })
    .compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(NewReviewInternshipComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});