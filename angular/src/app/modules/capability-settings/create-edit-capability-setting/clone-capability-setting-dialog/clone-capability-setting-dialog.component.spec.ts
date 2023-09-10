import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { CloneCapabilitySettingDialogComponent } from './clone-capability-setting-dialog.component';
describe('CloneCapabilitySettingDialogComponent', () => {
  let component: CloneCapabilitySettingDialogComponent;
  let fixture: ComponentFixture<CloneCapabilitySettingDialogComponent>;
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CloneCapabilitySettingDialogComponent ]
    })
    .compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(CloneCapabilitySettingDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});