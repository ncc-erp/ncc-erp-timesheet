import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterWorkingTimeComponent } from './register-working-time.component';

describe('RegisterWorkingTimeComponent', () => {
  let component: RegisterWorkingTimeComponent;
  let fixture: ComponentFixture<RegisterWorkingTimeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RegisterWorkingTimeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RegisterWorkingTimeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
