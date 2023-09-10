import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamBuildingProjectComponent } from './team-building-project.component';

describe('TeamBuildingProjectComponent', () => {
  let component: TeamBuildingProjectComponent;
  let fixture: ComponentFixture<TeamBuildingProjectComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TeamBuildingProjectComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TeamBuildingProjectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
