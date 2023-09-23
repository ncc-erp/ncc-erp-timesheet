import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RetroDetailComponent } from './retro-detail.component';

describe('RetroDetailComponent', () => {
  let component: RetroDetailComponent;
  let fixture: ComponentFixture<RetroDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RetroDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RetroDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
