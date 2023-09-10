import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TardinessLeaveEarlyDialogComponent } from './tardiness-leave-early-dialog.component';

describe('TardinessLeaveEarlyDialogComponent', () => {
  let component: TardinessLeaveEarlyDialogComponent;
  let fixture: ComponentFixture<TardinessLeaveEarlyDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TardinessLeaveEarlyDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TardinessLeaveEarlyDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
