import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomAbsenceTimeDialogComponent } from './custom-absence-time-dialog.component';

describe('CustomAbsenceTimeDialogComponent', () => {
  let component: CustomAbsenceTimeDialogComponent;
  let fixture: ComponentFixture<CustomAbsenceTimeDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CustomAbsenceTimeDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CustomAbsenceTimeDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
