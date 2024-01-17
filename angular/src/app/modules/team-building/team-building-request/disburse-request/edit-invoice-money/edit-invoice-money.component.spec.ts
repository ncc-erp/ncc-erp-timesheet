import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditInvoiceMoneyComponent } from './edit-invoice-money.component';

describe('EditInvoiceMoneyComponent', () => {
  let component: EditInvoiceMoneyComponent;
  let fixture: ComponentFixture<EditInvoiceMoneyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditInvoiceMoneyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditInvoiceMoneyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
