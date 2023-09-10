import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MyWorkingTimeComponent } from './my-working-time.component';

describe('MyWorkingTimeComponent', () => {
  let component: MyWorkingTimeComponent;
  let fixture: ComponentFixture<MyWorkingTimeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MyWorkingTimeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MyWorkingTimeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
