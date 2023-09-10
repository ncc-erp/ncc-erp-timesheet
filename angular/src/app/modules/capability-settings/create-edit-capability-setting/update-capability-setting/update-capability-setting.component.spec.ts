import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { UpdateCapabilitySettingComponent } from './update-capability-setting.component';
describe('UpdateCapabilitySettingComponent', () => {
  let component: UpdateCapabilitySettingComponent;
  let fixture: ComponentFixture<UpdateCapabilitySettingComponent>;
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UpdateCapabilitySettingComponent ]
    })
    .compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(UpdateCapabilitySettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
