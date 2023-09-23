import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MytimesheetNormalWorkingComponent } from './mytimesheet-normal-working.component';

describe('MytimesheetNormalWorkingComponent', () => {
  let component: MytimesheetNormalWorkingComponent;
  let fixture: ComponentFixture<MytimesheetNormalWorkingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MytimesheetNormalWorkingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MytimesheetNormalWorkingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
