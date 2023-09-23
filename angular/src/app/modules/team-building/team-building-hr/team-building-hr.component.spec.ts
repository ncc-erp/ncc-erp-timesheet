import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamBuildingHrComponent } from './team-building-hr.component';

describe('TeamBuildingHrComponent', () => {
  let component: TeamBuildingHrComponent;
  let fixture: ComponentFixture<TeamBuildingHrComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TeamBuildingHrComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TeamBuildingHrComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
