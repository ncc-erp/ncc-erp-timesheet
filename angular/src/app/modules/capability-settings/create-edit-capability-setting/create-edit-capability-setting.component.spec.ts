import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { CreateEditCapabilitySettingComponent } from './create-edit-capability-setting.component';
describe('CreateEditCapabilitySettingComponent', () => {
  let component: CreateEditCapabilitySettingComponent;
  let fixture: ComponentFixture<CreateEditCapabilitySettingComponent>;
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateEditCapabilitySettingComponent ]
    })
    .compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(CreateEditCapabilitySettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});