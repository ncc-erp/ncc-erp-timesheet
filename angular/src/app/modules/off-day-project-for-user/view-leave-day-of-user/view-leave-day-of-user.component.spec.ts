import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewLeaveDayOfUserComponent } from './view-leave-day-of-user.component';

describe('ViewLeaveDayOfUserComponent', () => {
  let component: ViewLeaveDayOfUserComponent;
  let fixture: ComponentFixture<ViewLeaveDayOfUserComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewLeaveDayOfUserComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewLeaveDayOfUserComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
