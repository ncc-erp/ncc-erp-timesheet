import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateEditDayOffComponent } from './create-edit-day-off.component';

describe('CreateEditDayOffComponent', () => {
  let component: CreateEditDayOffComponent;
  let fixture: ComponentFixture<CreateEditDayOffComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateEditDayOffComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateEditDayOffComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
