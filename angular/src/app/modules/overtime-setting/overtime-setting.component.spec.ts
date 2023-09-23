import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OvertimeSettingComponent } from './overtime-setting.component';

describe('OvertimeSettingComponent', () => {
  let component: OvertimeSettingComponent;
  let fixture: ComponentFixture<OvertimeSettingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OvertimeSettingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OvertimeSettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
