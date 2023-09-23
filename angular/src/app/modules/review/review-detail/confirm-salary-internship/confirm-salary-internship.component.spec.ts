import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmSalaryInternshipComponent } from './confirm-salary-internship.component';

describe('ConfirmSalaryInternshipComponent', () => {
  let component: ConfirmSalaryInternshipComponent;
  let fixture: ComponentFixture<ConfirmSalaryInternshipComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConfirmSalaryInternshipComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConfirmSalaryInternshipComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
