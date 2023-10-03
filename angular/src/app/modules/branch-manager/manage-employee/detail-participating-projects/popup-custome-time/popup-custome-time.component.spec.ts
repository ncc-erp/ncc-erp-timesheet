import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PopupCustomeTimeComponent } from './popup-custome-time.component';

describe('PopupCustomeTimeComponent', () => {
  let component: PopupCustomeTimeComponent;
  let fixture: ComponentFixture<PopupCustomeTimeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PopupCustomeTimeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PopupCustomeTimeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
