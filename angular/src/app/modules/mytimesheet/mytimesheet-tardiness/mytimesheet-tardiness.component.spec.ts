import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MytimesheetTardinessComponent } from './mytimesheet-tardiness.component';

describe('MytimesheetTardinessComponent', () => {
  let component: MytimesheetTardinessComponent;
  let fixture: ComponentFixture<MytimesheetTardinessComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MytimesheetTardinessComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MytimesheetTardinessComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
