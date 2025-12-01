import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DailyEmployeeReportComponent } from './daily-employee-report.component';

describe('DailyEmployeeReportComponent', () => {
  let component: DailyEmployeeReportComponent;
  let fixture: ComponentFixture<DailyEmployeeReportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DailyEmployeeReportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DailyEmployeeReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});