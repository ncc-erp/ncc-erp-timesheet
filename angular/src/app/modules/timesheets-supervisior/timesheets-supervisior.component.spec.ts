import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TimesheetsSupervisiorComponent } from './timesheets-supervisior.component';

describe('TimesheetsSupervisiorComponent', () => {
  let component: TimesheetsSupervisiorComponent;
  let fixture: ComponentFixture<TimesheetsSupervisiorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TimesheetsSupervisiorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TimesheetsSupervisiorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
