import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AnomaliesReportComponent } from './anomalies-report.component';

describe('AnomaliesReportComponent', () => {
  let component: AnomaliesReportComponent;
  let fixture: ComponentFixture<AnomaliesReportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AnomaliesReportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AnomaliesReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
