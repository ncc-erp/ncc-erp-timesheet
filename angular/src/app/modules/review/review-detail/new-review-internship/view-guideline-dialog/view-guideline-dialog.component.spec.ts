		
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ViewGuidelineDialogComponent } from './view-guideline-dialog.component';
describe('ViewGuidelineDialogComponent', () => {
  let component: ViewGuidelineDialogComponent;
  let fixture: ComponentFixture<ViewGuidelineDialogComponent>;
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewGuidelineDialogComponent ]
    })
    .compileComponents();
  }));
  beforeEach(() => {
    fixture = TestBed.createComponent(ViewGuidelineDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
