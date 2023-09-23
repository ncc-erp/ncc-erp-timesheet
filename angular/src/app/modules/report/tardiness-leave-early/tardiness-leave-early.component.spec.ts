import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TardinessLeaveEarlyComponent } from './tardiness-leave-early.component';

describe('TardinessLeaveEarlyComponent', () => {
  let component: TardinessLeaveEarlyComponent;
  let fixture: ComponentFixture<TardinessLeaveEarlyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TardinessLeaveEarlyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TardinessLeaveEarlyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
