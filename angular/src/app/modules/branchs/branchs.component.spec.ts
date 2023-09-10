import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BranchsComponent } from './branchs.component';

describe('OvertimeSettingComponent', () => {
  let component: BranchsComponent;
  let fixture: ComponentFixture<BranchsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BranchsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BranchsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
