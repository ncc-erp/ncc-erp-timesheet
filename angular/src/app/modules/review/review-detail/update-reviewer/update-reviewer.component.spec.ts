import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateReviewerComponent } from './update-reviewer.component';

describe('UpdateReviewerComponent', () => {
  let component: UpdateReviewerComponent;
  let fixture: ComponentFixture<UpdateReviewerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UpdateReviewerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UpdateReviewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
