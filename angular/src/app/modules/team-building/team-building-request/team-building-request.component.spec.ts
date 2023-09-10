import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TeamBuildingRequestComponent } from './team-building-request.component';

describe('TeamBuildingRequestComponent', () => {
  let component: TeamBuildingRequestComponent;
  let fixture: ComponentFixture<TeamBuildingRequestComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TeamBuildingRequestComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TeamBuildingRequestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
