import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { CreateEditCapabilityDialogComponent } from './create-edit-capability-dialog.component';
describe('CreateEditCapabilityDialogComponent', () => {
  let component: CreateEditCapabilityDialogComponent;
  let fixture: ComponentFixture<CreateEditCapabilityDialogComponent>;
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateEditCapabilityDialogComponent ]
    })
    .compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(CreateEditCapabilityDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});