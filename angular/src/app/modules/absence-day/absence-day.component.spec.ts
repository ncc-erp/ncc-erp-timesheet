import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AbsenceDayComponent } from './absence-day.component';

describe('AbsenceDayComponent', () => {
  let component: AbsenceDayComponent;
  let fixture: ComponentFixture<AbsenceDayComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AbsenceDayComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AbsenceDayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
