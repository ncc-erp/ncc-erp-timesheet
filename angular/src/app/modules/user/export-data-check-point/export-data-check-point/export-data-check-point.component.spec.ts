import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ExportDataCheckPointComponent } from './export-data-check-point.component';

describe('ExportDataCheckPointComponent', () => {
  let component: ExportDataCheckPointComponent;
  let fixture: ComponentFixture<ExportDataCheckPointComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ExportDataCheckPointComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExportDataCheckPointComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
