import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditMoneyComponent } from './edit-money.component';

describe('EditMoneyComponent', () => {
  let component: EditMoneyComponent;
  let fixture: ComponentFixture<EditMoneyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditMoneyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditMoneyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
