import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ApproveAbsenceRequestComponent } from './approve-absence-request.component';

describe('ApproveAbsenceRequestComponent', () => {
  let component: ApproveAbsenceRequestComponent;
  let fixture: ComponentFixture<ApproveAbsenceRequestComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ApproveAbsenceRequestComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ApproveAbsenceRequestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
