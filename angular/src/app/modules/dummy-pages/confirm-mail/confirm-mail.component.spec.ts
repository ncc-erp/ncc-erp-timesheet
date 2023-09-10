import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmMailComponent } from './confirm-mail.component';

describe('ConfirmMailComponent', () => {
  let component: ConfirmMailComponent;
  let fixture: ComponentFixture<ConfirmMailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConfirmMailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConfirmMailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
