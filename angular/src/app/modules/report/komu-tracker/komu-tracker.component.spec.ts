import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KomuTrackerComponent } from './komu-tracker.component';

describe('KomuTrackerComponent', () => {
  let component: KomuTrackerComponent;
  let fixture: ComponentFixture<KomuTrackerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KomuTrackerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KomuTrackerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
