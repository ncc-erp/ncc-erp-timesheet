import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { CapabilitySettingComponent } from './capability-setting.component';
describe('CapabilitySettingComponent', () => {
  let component: CapabilitySettingComponent;
  let fixture: ComponentFixture<CapabilitySettingComponent>;
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CapabilitySettingComponent ]
    })
    .compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(CapabilitySettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
