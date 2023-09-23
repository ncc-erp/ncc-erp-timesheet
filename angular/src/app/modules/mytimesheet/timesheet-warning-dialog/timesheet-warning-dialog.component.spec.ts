import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TimesheetWarningDialogComponent } from './timesheet-warning-dialog.component';

describe('TimesheetWarningDialogComponent', () => {
  let component: TimesheetWarningDialogComponent;
  let fixture: ComponentFixture<TimesheetWarningDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TimesheetWarningDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TimesheetWarningDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
