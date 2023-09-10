import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdatePunishMoneyComponent } from './update-punish-money.component';

describe('UpdatePunishMoneyComponent', () => {
  let component: UpdatePunishMoneyComponent;
  let fixture: ComponentFixture<UpdatePunishMoneyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UpdatePunishMoneyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UpdatePunishMoneyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
