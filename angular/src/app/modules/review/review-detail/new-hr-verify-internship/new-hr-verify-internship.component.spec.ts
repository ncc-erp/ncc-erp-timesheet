import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NewHrVerifyInternshipComponent } from './new-hr-verify-internship.component';

describe('NewHrVerifyInternshipComponent', () => {
  let component: NewHrVerifyInternshipComponent;
  let fixture: ComponentFixture<NewHrVerifyInternshipComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NewHrVerifyInternshipComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NewHrVerifyInternshipComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
