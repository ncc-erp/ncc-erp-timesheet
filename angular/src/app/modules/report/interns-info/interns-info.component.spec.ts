import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InternsInfoComponent } from './interns-info.component';

describe('InternsInfoComponent', () => {
  let component: InternsInfoComponent;
  let fixture: ComponentFixture<InternsInfoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InternsInfoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InternsInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
