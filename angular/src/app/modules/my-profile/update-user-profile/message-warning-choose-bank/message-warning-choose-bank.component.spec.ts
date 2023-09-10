import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MessageWarningChooseBankComponent } from './message-warning-choose-bank.component';

describe('MessageWarningChooseBankComponent', () => {
  let component: MessageWarningChooseBankComponent;
  let fixture: ComponentFixture<MessageWarningChooseBankComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MessageWarningChooseBankComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MessageWarningChooseBankComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
