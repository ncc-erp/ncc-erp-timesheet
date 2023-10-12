import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PopupUpdateProjectComponent } from './popup-update-project.component';

describe('PopupUpdateProjectComponent', () => {
  let component: PopupUpdateProjectComponent;
  let fixture: ComponentFixture<PopupUpdateProjectComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PopupUpdateProjectComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PopupUpdateProjectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
