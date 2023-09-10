import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SubmitAbsenseDayComponent } from './submit-absense-day.component';

describe('SubmitAbsenseDayComponent', () => {
  let component: SubmitAbsenseDayComponent;
  let fixture: ComponentFixture<SubmitAbsenseDayComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SubmitAbsenseDayComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SubmitAbsenseDayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
