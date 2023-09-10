import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RetroComponent } from './retro.component';

describe('RetroComponent', () => {
  let component: RetroComponent;
  let fixture: ComponentFixture<RetroComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RetroComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RetroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
