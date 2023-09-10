import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LeaveDayOfUserComponent } from './leave-day-of-user.component';

describe('LeaveDayOfUserComponent', () => {
  let component: LeaveDayOfUserComponent;
  let fixture: ComponentFixture<LeaveDayOfUserComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LeaveDayOfUserComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LeaveDayOfUserComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
