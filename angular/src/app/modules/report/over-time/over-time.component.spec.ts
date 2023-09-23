import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OverTimeComponent } from './over-time.component';

describe('OverTimeComponent', () => {
  let component: OverTimeComponent;
  let fixture: ComponentFixture<OverTimeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OverTimeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OverTimeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
