import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TimesheetWarningComponent } from './timesheet-warning.component';

describe('TimesheetWarningComponent', () => {
  let component: TimesheetWarningComponent;
  let fixture: ComponentFixture<TimesheetWarningComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TimesheetWarningComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TimesheetWarningComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
