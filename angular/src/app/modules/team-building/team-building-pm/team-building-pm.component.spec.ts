import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamBuildingPmComponent } from './team-building-pm.component';

describe('TeamBuildingPmComponent', () => {
  let component: TeamBuildingPmComponent;
  let fixture: ComponentFixture<TeamBuildingPmComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TeamBuildingPmComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TeamBuildingPmComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
