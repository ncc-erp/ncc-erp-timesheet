import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DailyProjectReportComponent } from './daily-project-report.component';

describe('DailyProjectReportComponent', () => {
  let component: DailyProjectReportComponent;
  let fixture: ComponentFixture<DailyProjectReportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DailyProjectReportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DailyProjectReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
