import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DetailParticipatingProjectsComponent } from './detail-participating-projects.component';

describe('DetailParticipatingProjectsComponent', () => {
  let component: DetailParticipatingProjectsComponent;
  let fixture: ComponentFixture<DetailParticipatingProjectsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DetailParticipatingProjectsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DetailParticipatingProjectsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
