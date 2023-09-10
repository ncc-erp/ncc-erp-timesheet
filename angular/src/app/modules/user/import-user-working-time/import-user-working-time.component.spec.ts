import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportUserWorkingTimeComponent } from './import-user-working-time.component';

describe('ImportUserWorkingTimeComponent', () => {
  let component: ImportUserWorkingTimeComponent;
  let fixture: ComponentFixture<ImportUserWorkingTimeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImportUserWorkingTimeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportUserWorkingTimeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
